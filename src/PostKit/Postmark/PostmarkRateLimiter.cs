using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace PostKit.Postmark;

internal sealed class PostmarkRateLimiter(Func<TimeSpan, CancellationToken, Task>? delayAsync = null)
{
    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromSeconds(1);
    private readonly ConcurrentDictionary<string, RateLimitBucket> _buckets = new(StringComparer.Ordinal);
    private readonly Func<TimeSpan, CancellationToken, Task> _delayAsync = delayAsync ?? Task.Delay;

    public async Task<PostmarkRateLimitLease> WaitForAvailabilityAsync(string endpoint, CancellationToken cancellationToken)
    {
        var bucketKey = GetBucketKey(endpoint);
        if (!_buckets.TryGetValue(bucketKey, out var bucket))
            return new PostmarkRateLimitLease(bucketKey, false);

        var delay = bucket.RecordRequestAndGetDelay();
        if (delay > TimeSpan.Zero)
            await _delayAsync(delay, cancellationToken);

        return new PostmarkRateLimitLease(bucketKey, true);
    }

    public void ObserveResponse(string endpoint, HttpResponseHeaders headers, PostmarkRateLimitLease lease)
    {
        if (!TryGetRateLimit(headers, out var limit))
            return;

        var bucketKey = string.IsNullOrEmpty(lease.BucketKey) ? GetBucketKey(endpoint) : lease.BucketKey;
        var bucket = _buckets.GetOrAdd(bucketKey, static _ => new RateLimitBucket());
        bucket.SetLimit(limit);

        if (!lease.Recorded)
            bucket.RecordObservedRequest();
    }

    public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken)
    {
        return _delayAsync(delay, cancellationToken);
    }

    private static bool TryGetRateLimit(HttpResponseHeaders headers, out int limit)
    {
        limit = 0;
        return TryGetPositiveIntHeader(headers, "RateLimit-Limit", out limit) || TryGetPositiveIntHeader(headers, "X-RateLimit-Limit-Second", out limit);
    }

    private static bool TryGetPositiveIntHeader(HttpResponseHeaders headers, string name, out int value)
    {
        value = 0;
        if (!headers.TryGetValues(name, out var values))
            return false;

        foreach (var headerValue in values)
        {
            if (int.TryParse(headerValue, out value) && value > 0)
                return true;
        }

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
        private int? _limit;

        public void SetLimit(int limit)
        {
            lock (_gate)
            {
                _limit = limit;
                PruneOldRequests(Stopwatch.GetTimestamp());
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

        public TimeSpan RecordRequestAndGetDelay()
        {
            lock (_gate)
            {
                var now = Stopwatch.GetTimestamp();
                PruneOldRequests(now);

                if (_limit is null || _requestTimestamps.Count == 0)
                {
                    _requestTimestamps.Enqueue(now);
                    return TimeSpan.Zero;
                }

                var targetRequestsPerWindow = Math.Max(1, _limit.Value - 1);
                var oldest = _requestTimestamps.Peek();
                var actualElapsedMilliseconds = GetElapsedMilliseconds(oldest, now);
                var idealElapsedMilliseconds = (_requestTimestamps.Count + 1) * RateLimitWindow.TotalMilliseconds / targetRequestsPerWindow;
                var delayMilliseconds = idealElapsedMilliseconds - actualElapsedMilliseconds;

                if (delayMilliseconds <= 0)
                {
                    _requestTimestamps.Enqueue(now);
                    return TimeSpan.Zero;
                }

                var roundedDelayMilliseconds = Math.Max(1, (int)Math.Ceiling(delayMilliseconds));
                var delay = TimeSpan.FromMilliseconds(roundedDelayMilliseconds);
                _requestTimestamps.Enqueue(AddDelay(now, delay));
                return delay;
            }
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
    }
}

internal readonly record struct PostmarkRateLimitLease(string BucketKey, bool Recorded);
