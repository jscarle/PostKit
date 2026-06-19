using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace PostKit.Postmark;

internal sealed class PostmarkRateLimiter(Func<TimeSpan, CancellationToken, Task>? delayAsync = null)
{
    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromSeconds(1);

    private static readonly Dictionary<string, int> DefaultRateLimits = new(StringComparer.Ordinal)
    {
        ["email"] = 5000,
        ["messages/outbound"] = 150,
        ["message-streams/suppressions"] = 500
    };

    private readonly ConcurrentDictionary<string, RateLimitBucket> _buckets = new(StringComparer.Ordinal);
    private readonly Func<TimeSpan, CancellationToken, Task> _delayAsync = delayAsync ?? Task.Delay;

    public async Task<PostmarkRateLimitLease> WaitForAvailabilityAsync(string endpoint, CancellationToken cancellationToken)
    {
        var bucketKey = GetBucketKey(endpoint);
        var bucket = _buckets.GetOrAdd(bucketKey, CreateBucket);

        var reservation = bucket.Reserve();
        if (reservation.Delay > TimeSpan.Zero)
            await _delayAsync(reservation.Delay, cancellationToken);

        return new PostmarkRateLimitLease(bucketKey, reservation.Recorded);
    }

    public void ObserveResponse(string endpoint, HttpResponseHeaders headers, PostmarkRateLimitLease lease)
    {
        if (!TryGetRateLimit(headers, out var limit, out var remaining))
            return;

        var bucketKey = string.IsNullOrEmpty(lease.BucketKey) ? GetBucketKey(endpoint) : lease.BucketKey;
        var bucket = _buckets.GetOrAdd(bucketKey, CreateBucket);
        bucket.ObserveLimit(limit, remaining);

        if (!lease.Recorded)
            bucket.RecordObservedRequest();
    }

    public void ObserveTooManyRequests(string endpoint, TimeSpan retryDelay)
    {
        var bucketKey = GetBucketKey(endpoint);
        var bucket = _buckets.GetOrAdd(bucketKey, CreateBucket);
        bucket.BlockUntil(retryDelay);
    }

    public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken)
    {
        return _delayAsync(delay, cancellationToken);
    }

    private static RateLimitBucket CreateBucket(string bucketKey)
    {
        return DefaultRateLimits.TryGetValue(bucketKey, out var limit) ? new RateLimitBucket(limit) : new RateLimitBucket();
    }

    private static bool TryGetRateLimit(HttpResponseHeaders headers, out int limit, out int? remaining)
    {
        limit = 0;
        remaining = null;

        if (!TryGetPositiveIntHeader(headers, "RateLimit-Limit", out limit) && !TryGetPositiveIntHeader(headers, "X-RateLimit-Limit-Second", out limit))
            return false;

        if (TryGetNonNegativeIntHeader(headers, "RateLimit-Remaining", out var standardRemaining) || TryGetNonNegativeIntHeader(headers, "X-RateLimit-Remaining-Second", out standardRemaining))
            remaining = standardRemaining;

        return true;
    }

    private static bool TryGetPositiveIntHeader(HttpResponseHeaders headers, string name, out int value)
    {
        value = 0;
        if (!headers.TryGetValues(name, out var values))
            return false;

        foreach (var headerValue in values)
            if (int.TryParse(headerValue, out value) && value > 0)
                return true;

        value = 0;
        return false;
    }

    private static bool TryGetNonNegativeIntHeader(HttpResponseHeaders headers, string name, out int value)
    {
        value = 0;
        if (!headers.TryGetValues(name, out var values))
            return false;

        foreach (var headerValue in values)
            if (int.TryParse(headerValue, out value) && value >= 0)
                return true;

        value = 0;
        return false;
    }

    private static string GetBucketKey(string endpoint)
    {
        var queryIndex = endpoint.IndexOf('?', StringComparison.Ordinal);
        var path = queryIndex < 0 ? endpoint : endpoint[..queryIndex];

        if (path.StartsWith("/messages/outbound", StringComparison.OrdinalIgnoreCase))
            return "messages/outbound";

        if (path.StartsWith("/message-streams/", StringComparison.OrdinalIgnoreCase) && path.Contains("/suppressions", StringComparison.OrdinalIgnoreCase))
            return "message-streams/suppressions";

        if (path.StartsWith("/email", StringComparison.OrdinalIgnoreCase))
            return "email";

        return path.ToLowerInvariant();
    }

    private sealed class RateLimitBucket
    {
        private readonly object _gate = new();
        private readonly Queue<long> _requestTimestamps = new();
        private long? _blockedUntilTimestamp;
        private int? _limit;

        public RateLimitBucket(int? limit = null)
        {
            _limit = limit;
        }

        public void ObserveLimit(int limit, int? remaining)
        {
            lock (_gate)
            {
                _limit = limit;
                var now = Stopwatch.GetTimestamp();
                PruneOldRequests(now);

                if (!remaining.HasValue)
                    return;

                var observedRequests = Math.Clamp(limit - remaining.Value, 0, limit);
                while (_requestTimestamps.Count < observedRequests)
                    _requestTimestamps.Enqueue(now);
            }
        }

        public void RecordObservedRequest()
        {
            lock (_gate)
            {
                var now = Stopwatch.GetTimestamp();
                PruneOldRequests(now);
                _requestTimestamps.Enqueue(now);
            }
        }

        public void BlockUntil(TimeSpan retryDelay)
        {
            lock (_gate)
            {
                var now = Stopwatch.GetTimestamp();
                var blockedUntil = AddDelay(now, retryDelay);
                if (!_blockedUntilTimestamp.HasValue || blockedUntil > _blockedUntilTimestamp.Value)
                    _blockedUntilTimestamp = blockedUntil;
            }
        }

        public RateLimitReservation Reserve()
        {
            lock (_gate)
            {
                var now = Stopwatch.GetTimestamp();
                var scheduledTimestamp = GetEarliestAvailableTimestamp(now);
                PruneOldRequests(scheduledTimestamp);

                if (_limit is null)
                    return new RateLimitReservation(false, GetDelay(now, scheduledTimestamp));

                if (_requestTimestamps.Count > 0)
                {
                    var targetRequestsPerWindow = Math.Max(1, _limit.Value - 1);
                    var oldest = _requestTimestamps.Peek();
                    var actualElapsedMilliseconds = GetElapsedMilliseconds(oldest, scheduledTimestamp);
                    var idealElapsedMilliseconds = (_requestTimestamps.Count + 1) * RateLimitWindow.TotalMilliseconds / targetRequestsPerWindow;
                    var delayMilliseconds = idealElapsedMilliseconds - actualElapsedMilliseconds;

                    if (delayMilliseconds > 0)
                        scheduledTimestamp = AddDelay(scheduledTimestamp, TimeSpan.FromMilliseconds(Math.Max(1, (int)Math.Ceiling(delayMilliseconds))));
                }

                _requestTimestamps.Enqueue(scheduledTimestamp);
                return new RateLimitReservation(true, GetDelay(now, scheduledTimestamp));
            }
        }

        private long GetEarliestAvailableTimestamp(long now)
        {
            if (!_blockedUntilTimestamp.HasValue)
                return now;

            if (_blockedUntilTimestamp.Value <= now)
            {
                _blockedUntilTimestamp = null;
                return now;
            }

            return _blockedUntilTimestamp.Value;
        }

        private void PruneOldRequests(long now)
        {
            while (_requestTimestamps.TryPeek(out var timestamp) && GetElapsedMilliseconds(timestamp, now) >= RateLimitWindow.TotalMilliseconds)
                _requestTimestamps.Dequeue();
        }

        private static double GetElapsedMilliseconds(long startTimestamp, long endTimestamp)
        {
            return (endTimestamp - startTimestamp) * 1000.0 / Stopwatch.Frequency;
        }

        private static long AddDelay(long timestamp, TimeSpan delay)
        {
            return timestamp + (long)Math.Ceiling(delay.TotalSeconds * Stopwatch.Frequency);
        }

        private static TimeSpan GetDelay(long now, long scheduledTimestamp)
        {
            return scheduledTimestamp <= now ? TimeSpan.Zero : TimeSpan.FromMilliseconds(Math.Max(1, (int)Math.Ceiling(GetElapsedMilliseconds(now, scheduledTimestamp))));
        }
    }
}

internal readonly record struct RateLimitReservation(bool Recorded, TimeSpan Delay);

internal readonly record struct PostmarkRateLimitLease(string BucketKey, bool Recorded);