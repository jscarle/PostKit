using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;
using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.Common;
using PostKit.Postmark;
using PostKit.Stats;
using OverviewStatsModel = PostKit.Postmark.Stats.OutboundOverviewStatsResponse;
using StatsDayModel = PostKit.Postmark.Stats.OutboundStatsDayResponse;
using StatsSeriesModel = PostKit.Postmark.Stats.OutboundStatsSeriesResponse;

// ReSharper disable once CheckNamespace
namespace PostKit;

internal sealed partial class PostKitClient
{
    public async Task<Result<OutboundOverviewStats>> GetOutboundStatsOverviewAsync(OutboundStatsQuery? query = null, CancellationToken cancellationToken = default)
    {
        var validationError = ValidationExtensions.ValidateOutboundStatsQuery(query);
        if (validationError is not null)
            return Result.Failure<OutboundOverviewStats>(validationError);

        Result<OverviewStatsModel> response;
        try
        {
            response = await postmark.GetAsync<OverviewStatsModel>(PostmarkTokenScope.Server, BuildOutboundStatsEndpoint("/stats/outbound", query), cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogOutboundStatsException(ex);
            return Result.Failure<OutboundOverviewStats>(ex);
        }

        if (response.IsFailure(out var error, out var overviewModel))
        {
            LogOutboundStatsError(error.Message, error);
            return Result.Failure<OutboundOverviewStats>(error);
        }

        var mapped = CreateOutboundOverviewStats(overviewModel);
        if (mapped.IsFailure(out var mappingError, out var overview))
        {
            LogOutboundStatsError(mappingError.Message, mappingError);
            return Result.Failure<OutboundOverviewStats>(mappingError);
        }

        return Result.Success(overview);
    }

    public Task<Result<OutboundSentStats>> GetOutboundSentStatsAsync(OutboundStatsQuery? query = null, CancellationToken cancellationToken = default)
    {
        return GetOutboundStatsSeriesAsync("/stats/outbound/sends", query, CreateOutboundSentStats, cancellationToken);
    }

    public Task<Result<OutboundBounceStats>> GetOutboundBounceStatsAsync(OutboundStatsQuery? query = null, CancellationToken cancellationToken = default)
    {
        return GetOutboundStatsSeriesAsync("/stats/outbound/bounces", query, CreateOutboundBounceStats, cancellationToken);
    }

    public Task<Result<OutboundSpamComplaintStats>> GetOutboundSpamComplaintStatsAsync(OutboundStatsQuery? query = null, CancellationToken cancellationToken = default)
    {
        return GetOutboundStatsSeriesAsync("/stats/outbound/spam", query, CreateOutboundSpamComplaintStats, cancellationToken);
    }

    public Task<Result<OutboundTrackedEmailStats>> GetOutboundTrackedEmailStatsAsync(OutboundStatsQuery? query = null, CancellationToken cancellationToken = default)
    {
        return GetOutboundStatsSeriesAsync("/stats/outbound/tracked", query, CreateOutboundTrackedEmailStats, cancellationToken);
    }

    public Task<Result<OutboundOpenStats>> GetOutboundOpenStatsAsync(OutboundStatsQuery? query = null, CancellationToken cancellationToken = default)
    {
        return GetOutboundStatsSeriesAsync("/stats/outbound/opens", query, CreateOutboundOpenStats, cancellationToken);
    }

    public Task<Result<OutboundEmailPlatformStats>> GetOutboundEmailPlatformStatsAsync(OutboundStatsQuery? query = null, CancellationToken cancellationToken = default)
    {
        return GetOutboundStatsSeriesAsync("/stats/outbound/opens/platforms", query, CreateOutboundEmailPlatformStats, cancellationToken);
    }

    public Task<Result<OutboundEmailClientStats>> GetOutboundEmailClientStatsAsync(OutboundStatsQuery? query = null, CancellationToken cancellationToken = default)
    {
        return GetOutboundStatsSeriesAsync("/stats/outbound/opens/emailclients", query, CreateOutboundEmailClientStats, cancellationToken);
    }

    public Task<Result<OutboundClickStats>> GetOutboundClickStatsAsync(OutboundStatsQuery? query = null, CancellationToken cancellationToken = default)
    {
        return GetOutboundStatsSeriesAsync("/stats/outbound/clicks", query, CreateOutboundClickStats, cancellationToken);
    }

    public Task<Result<OutboundClickBrowserStats>> GetOutboundClickBrowserStatsAsync(OutboundStatsQuery? query = null, CancellationToken cancellationToken = default)
    {
        return GetOutboundStatsSeriesAsync("/stats/outbound/clicks/browserfamilies", query, CreateOutboundClickBrowserStats, cancellationToken);
    }

    public Task<Result<OutboundClickPlatformStats>> GetOutboundClickPlatformStatsAsync(OutboundStatsQuery? query = null, CancellationToken cancellationToken = default)
    {
        return GetOutboundStatsSeriesAsync("/stats/outbound/clicks/platforms", query, CreateOutboundClickPlatformStats, cancellationToken);
    }

    public Task<Result<OutboundClickLocationStats>> GetOutboundClickLocationStatsAsync(OutboundStatsQuery? query = null, CancellationToken cancellationToken = default)
    {
        return GetOutboundStatsSeriesAsync("/stats/outbound/clicks/location", query, CreateOutboundClickLocationStats, cancellationToken);
    }

    private async Task<Result<TStats>> GetOutboundStatsSeriesAsync<TStats>(string endpoint, OutboundStatsQuery? query, Func<StatsSeriesModel, Result<TStats>> createStats, CancellationToken cancellationToken)
    {
        var validationError = ValidationExtensions.ValidateOutboundStatsQuery(query);
        if (validationError is not null)
            return Result.Failure<TStats>(validationError);

        Result<StatsSeriesModel> response;
        try
        {
            response = await postmark.GetAsync<StatsSeriesModel>(PostmarkTokenScope.Server, BuildOutboundStatsEndpoint(endpoint, query), cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogOutboundStatsException(ex);
            return Result.Failure<TStats>(ex);
        }

        if (response.IsFailure(out var error, out var seriesModel))
        {
            LogOutboundStatsError(error.Message, error);
            return Result.Failure<TStats>(error);
        }

        var mapped = createStats(seriesModel);
        if (mapped.IsFailure(out var mappingError, out var stats))
        {
            LogOutboundStatsError(mappingError.Message, mappingError);
            return Result.Failure<TStats>(mappingError);
        }

        return Result.Success(stats);
    }

    private static string BuildOutboundStatsEndpoint(string endpoint, OutboundStatsQuery? query)
    {
        if (query is null)
            return endpoint;

        var parameters = new List<string>(4);
        if (query.Tag is not null)
            parameters.Add($"tag={Uri.EscapeDataString(query.Tag)}");

        if (query.FromDate.HasValue)
            parameters.Add($"fromdate={Uri.EscapeDataString(ValidationExtensions.FormatStatsDate(query.FromDate.Value))}");

        if (query.ToDate.HasValue)
            parameters.Add($"todate={Uri.EscapeDataString(ValidationExtensions.FormatStatsDate(query.ToDate.Value))}");

        if (query.MessageStreamId is not null)
            parameters.Add($"messagestream={Uri.EscapeDataString(query.MessageStreamId)}");

        return parameters.Count == 0 ? endpoint : $"{endpoint}?{string.Join("&", parameters)}";
    }

    private static Result<OutboundOverviewStats> CreateOutboundOverviewStats(OverviewStatsModel response)
    {
        var sent = GetRequiredOverviewCount(response.Sent, "Sent");
        if (sent.IsFailure(out var sentError, out var mappedSent))
            return Result.Failure<OutboundOverviewStats>(sentError);

        var bounced = GetRequiredOverviewCount(response.Bounced, "Bounced");
        if (bounced.IsFailure(out var bouncedError, out var mappedBounced))
            return Result.Failure<OutboundOverviewStats>(bouncedError);

        var smtpApiErrors = GetRequiredOverviewCount(response.SmtpApiErrors, "SMTPApiErrors");
        if (smtpApiErrors.IsFailure(out var smtpError, out var mappedSmtpApiErrors))
            return Result.Failure<OutboundOverviewStats>(smtpError);

        var bounceRate = GetRequiredOverviewRate(response.BounceRate, "BounceRate");
        if (bounceRate.IsFailure(out var bounceRateError, out var mappedBounceRate))
            return Result.Failure<OutboundOverviewStats>(bounceRateError);

        var spamComplaints = GetRequiredOverviewCount(response.SpamComplaints, "SpamComplaints");
        if (spamComplaints.IsFailure(out var spamError, out var mappedSpamComplaints))
            return Result.Failure<OutboundOverviewStats>(spamError);

        var spamComplaintsRate = GetRequiredOverviewRate(response.SpamComplaintsRate, "SpamComplaintsRate");
        if (spamComplaintsRate.IsFailure(out var spamRateError, out var mappedSpamComplaintsRate))
            return Result.Failure<OutboundOverviewStats>(spamRateError);

        var opens = GetRequiredOverviewCount(response.Opens, "Opens");
        if (opens.IsFailure(out var opensError, out var mappedOpens))
            return Result.Failure<OutboundOverviewStats>(opensError);

        var uniqueOpens = GetRequiredOverviewCount(response.UniqueOpens, "UniqueOpens");
        if (uniqueOpens.IsFailure(out var uniqueOpensError, out var mappedUniqueOpens))
            return Result.Failure<OutboundOverviewStats>(uniqueOpensError);

        var totalClicks = GetRequiredOverviewCount(response.TotalClicks, "TotalClicks");
        if (totalClicks.IsFailure(out var totalClicksError, out var mappedTotalClicks))
            return Result.Failure<OutboundOverviewStats>(totalClicksError);

        var uniqueLinksClicked = GetRequiredOverviewCount(response.UniqueLinksClicked, "UniqueLinksClicked");
        if (uniqueLinksClicked.IsFailure(out var uniqueLinksError, out var mappedUniqueLinksClicked))
            return Result.Failure<OutboundOverviewStats>(uniqueLinksError);

        var totalTrackedLinksSent = GetRequiredOverviewCount(response.TotalTrackedLinksSent, "TotalTrackedLinksSent");
        if (totalTrackedLinksSent.IsFailure(out var totalTrackedError, out var mappedTotalTrackedLinksSent))
            return Result.Failure<OutboundOverviewStats>(totalTrackedError);

        var tracked = GetRequiredOverviewCount(response.Tracked, "Tracked");
        if (tracked.IsFailure(out var trackedError, out var mappedTracked))
            return Result.Failure<OutboundOverviewStats>(trackedError);

        var withLinkTracking = GetRequiredOverviewCount(response.WithLinkTracking, "WithLinkTracking");
        if (withLinkTracking.IsFailure(out var linkError, out var mappedWithLinkTracking))
            return Result.Failure<OutboundOverviewStats>(linkError);

        var withOpenTracking = GetRequiredOverviewCount(response.WithOpenTracking, "WithOpenTracking");
        if (withOpenTracking.IsFailure(out var openError, out var mappedWithOpenTracking))
            return Result.Failure<OutboundOverviewStats>(openError);

        var withClientRecorded = GetRequiredOverviewCount(response.WithClientRecorded, "WithClientRecorded");
        if (withClientRecorded.IsFailure(out var clientError, out var mappedWithClientRecorded))
            return Result.Failure<OutboundOverviewStats>(clientError);

        var withPlatformRecorded = GetRequiredOverviewCount(response.WithPlatformRecorded, "WithPlatformRecorded");
        if (withPlatformRecorded.IsFailure(out var platformError, out var mappedWithPlatformRecorded))
            return Result.Failure<OutboundOverviewStats>(platformError);

        return Result.Success(new OutboundOverviewStats
        {
            Sent = mappedSent,
            Bounced = mappedBounced,
            SmtpApiErrors = mappedSmtpApiErrors,
            BounceRate = mappedBounceRate,
            SpamComplaints = mappedSpamComplaints,
            SpamComplaintsRate = mappedSpamComplaintsRate,
            Opens = mappedOpens,
            UniqueOpens = mappedUniqueOpens,
            TotalClicks = mappedTotalClicks,
            UniqueLinksClicked = mappedUniqueLinksClicked,
            TotalTrackedLinksSent = mappedTotalTrackedLinksSent,
            Tracked = mappedTracked,
            WithLinkTracking = mappedWithLinkTracking,
            WithOpenTracking = mappedWithOpenTracking,
            WithClientRecorded = mappedWithClientRecorded,
            WithPlatformRecorded = mappedWithPlatformRecorded
        });
    }

    private static Result<OutboundSentStats> CreateOutboundSentStats(StatsSeriesModel response)
    {
        var sent = GetRequiredStatsTotal(response, "Sent");
        if (sent.IsFailure(out var sentError, out var mappedSent))
            return Result.Failure<OutboundSentStats>(sentError);

        var days = CreateStatsDays(response, "sent stats", day => new OutboundSentStatsDay { Date = day.Date, Sent = GetOptionalStatsDayCount(day.Response, "Sent", day.Index) });
        if (days.IsFailure(out var daysError, out var mappedDays))
            return Result.Failure<OutboundSentStats>(daysError);

        return Result.Success(new OutboundSentStats { Sent = mappedSent, Days = mappedDays });
    }

    private static Result<OutboundBounceStats> CreateOutboundBounceStats(StatsSeriesModel response)
    {
        var hardBounce = GetRequiredStatsTotal(response, "HardBounce");
        if (hardBounce.IsFailure(out var hardError, out var mappedHardBounce))
            return Result.Failure<OutboundBounceStats>(hardError);

        var smtpApiError = GetRequiredStatsTotal(response, "SMTPApiError");
        if (smtpApiError.IsFailure(out var smtpError, out var mappedSmtpApiError))
            return Result.Failure<OutboundBounceStats>(smtpError);

        var softBounce = GetRequiredStatsTotal(response, "SoftBounce");
        if (softBounce.IsFailure(out var softError, out var mappedSoftBounce))
            return Result.Failure<OutboundBounceStats>(softError);

        var transient = GetRequiredStatsTotal(response, "Transient");
        if (transient.IsFailure(out var transientError, out var mappedTransient))
            return Result.Failure<OutboundBounceStats>(transientError);

        var days = CreateStatsDays(response, "bounce stats", day => new OutboundBounceStatsDay
        {
            Date = day.Date,
            HardBounce = GetOptionalStatsDayCount(day.Response, "HardBounce", day.Index),
            SmtpApiError = GetOptionalStatsDayCount(day.Response, "SMTPApiError", day.Index),
            SoftBounce = GetOptionalStatsDayCount(day.Response, "SoftBounce", day.Index),
            Transient = GetOptionalStatsDayCount(day.Response, "Transient", day.Index)
        });
        if (days.IsFailure(out var daysError, out var mappedDays))
            return Result.Failure<OutboundBounceStats>(daysError);

        return Result.Success(new OutboundBounceStats { HardBounce = mappedHardBounce, SmtpApiError = mappedSmtpApiError, SoftBounce = mappedSoftBounce, Transient = mappedTransient, Days = mappedDays });
    }

    private static Result<OutboundSpamComplaintStats> CreateOutboundSpamComplaintStats(StatsSeriesModel response)
    {
        var spamComplaint = GetRequiredStatsTotal(response, "SpamComplaint");
        if (spamComplaint.IsFailure(out var error, out var mappedSpamComplaint))
            return Result.Failure<OutboundSpamComplaintStats>(error);

        var days = CreateStatsDays(response, "spam complaint stats", day => new OutboundSpamComplaintStatsDay { Date = day.Date, SpamComplaint = GetOptionalStatsDayCount(day.Response, "SpamComplaint", day.Index) });
        if (days.IsFailure(out var daysError, out var mappedDays))
            return Result.Failure<OutboundSpamComplaintStats>(daysError);

        return Result.Success(new OutboundSpamComplaintStats { SpamComplaint = mappedSpamComplaint, Days = mappedDays });
    }

    private static Result<OutboundTrackedEmailStats> CreateOutboundTrackedEmailStats(StatsSeriesModel response)
    {
        var tracked = GetRequiredStatsTotal(response, "Tracked");
        if (tracked.IsFailure(out var error, out var mappedTracked))
            return Result.Failure<OutboundTrackedEmailStats>(error);

        var days = CreateStatsDays(response, "tracked email stats", day => new OutboundTrackedEmailStatsDay { Date = day.Date, Tracked = GetOptionalStatsDayCount(day.Response, "Tracked", day.Index) });
        if (days.IsFailure(out var daysError, out var mappedDays))
            return Result.Failure<OutboundTrackedEmailStats>(daysError);

        return Result.Success(new OutboundTrackedEmailStats { Tracked = mappedTracked, Days = mappedDays });
    }

    private static Result<OutboundOpenStats> CreateOutboundOpenStats(StatsSeriesModel response)
    {
        var opens = GetRequiredStatsTotal(response, "Opens");
        if (opens.IsFailure(out var opensError, out var mappedOpens))
            return Result.Failure<OutboundOpenStats>(opensError);

        var unique = GetRequiredStatsTotal(response, "Unique");
        if (unique.IsFailure(out var uniqueError, out var mappedUnique))
            return Result.Failure<OutboundOpenStats>(uniqueError);

        var days = CreateStatsDays(response, "open stats", day => new OutboundOpenStatsDay
        {
            Date = day.Date,
            Opens = GetOptionalStatsDayCount(day.Response, "Opens", day.Index),
            Unique = GetOptionalStatsDayCount(day.Response, "Unique", day.Index)
        });
        if (days.IsFailure(out var daysError, out var mappedDays))
            return Result.Failure<OutboundOpenStats>(daysError);

        return Result.Success(new OutboundOpenStats { Opens = mappedOpens, Unique = mappedUnique, Days = mappedDays });
    }

    private static Result<OutboundEmailPlatformStats> CreateOutboundEmailPlatformStats(StatsSeriesModel response)
    {
        var desktop = GetRequiredStatsTotal(response, "Desktop");
        if (desktop.IsFailure(out var desktopError, out var mappedDesktop))
            return Result.Failure<OutboundEmailPlatformStats>(desktopError);

        var mobile = GetRequiredStatsTotal(response, "Mobile");
        if (mobile.IsFailure(out var mobileError, out var mappedMobile))
            return Result.Failure<OutboundEmailPlatformStats>(mobileError);

        var unknown = GetRequiredStatsTotal(response, "Unknown");
        if (unknown.IsFailure(out var unknownError, out var mappedUnknown))
            return Result.Failure<OutboundEmailPlatformStats>(unknownError);

        var webMail = GetRequiredStatsTotal(response, "WebMail");
        if (webMail.IsFailure(out var webMailError, out var mappedWebMail))
            return Result.Failure<OutboundEmailPlatformStats>(webMailError);

        var days = CreateStatsDays(response, "email platform stats", day => new OutboundEmailPlatformStatsDay
        {
            Date = day.Date,
            Desktop = GetOptionalStatsDayCount(day.Response, "Desktop", day.Index),
            Mobile = GetOptionalStatsDayCount(day.Response, "Mobile", day.Index),
            Unknown = GetOptionalStatsDayCount(day.Response, "Unknown", day.Index),
            WebMail = GetOptionalStatsDayCount(day.Response, "WebMail", day.Index)
        });
        if (days.IsFailure(out var daysError, out var mappedDays))
            return Result.Failure<OutboundEmailPlatformStats>(daysError);

        return Result.Success(new OutboundEmailPlatformStats { Desktop = mappedDesktop, Mobile = mappedMobile, Unknown = mappedUnknown, WebMail = mappedWebMail, Days = mappedDays });
    }

    private static Result<OutboundEmailClientStats> CreateOutboundEmailClientStats(StatsSeriesModel response)
    {
        var usage = CreateOutboundUsageStats(response, "email client stats");
        if (usage.IsFailure(out var error, out var mappedUsage))
            return Result.Failure<OutboundEmailClientStats>(error);

        return Result.Success(new OutboundEmailClientStats(mappedUsage.Totals, mappedUsage.Days));
    }

    private static Result<OutboundClickStats> CreateOutboundClickStats(StatsSeriesModel response)
    {
        var clicks = GetRequiredStatsTotal(response, "Clicks");
        if (clicks.IsFailure(out var clicksError, out var mappedClicks))
            return Result.Failure<OutboundClickStats>(clicksError);

        var unique = GetRequiredStatsTotal(response, "Unique");
        if (unique.IsFailure(out var uniqueError, out var mappedUnique))
            return Result.Failure<OutboundClickStats>(uniqueError);

        var days = CreateStatsDays(response, "click stats", day => new OutboundClickStatsDay
        {
            Date = day.Date,
            Clicks = GetOptionalStatsDayCount(day.Response, "Clicks", day.Index),
            Unique = GetOptionalStatsDayCount(day.Response, "Unique", day.Index)
        });
        if (days.IsFailure(out var daysError, out var mappedDays))
            return Result.Failure<OutboundClickStats>(daysError);

        return Result.Success(new OutboundClickStats { Clicks = mappedClicks, Unique = mappedUnique, Days = mappedDays });
    }

    private static Result<OutboundClickBrowserStats> CreateOutboundClickBrowserStats(StatsSeriesModel response)
    {
        var usage = CreateOutboundUsageStats(response, "click browser stats");
        if (usage.IsFailure(out var error, out var mappedUsage))
            return Result.Failure<OutboundClickBrowserStats>(error);

        return Result.Success(new OutboundClickBrowserStats(mappedUsage.Totals, mappedUsage.Days));
    }

    private static Result<OutboundClickPlatformStats> CreateOutboundClickPlatformStats(StatsSeriesModel response)
    {
        var desktop = GetRequiredStatsTotal(response, "Desktop");
        if (desktop.IsFailure(out var desktopError, out var mappedDesktop))
            return Result.Failure<OutboundClickPlatformStats>(desktopError);

        var mobile = GetRequiredStatsTotal(response, "Mobile");
        if (mobile.IsFailure(out var mobileError, out var mappedMobile))
            return Result.Failure<OutboundClickPlatformStats>(mobileError);

        var unknown = GetRequiredStatsTotal(response, "Unknown");
        if (unknown.IsFailure(out var unknownError, out var mappedUnknown))
            return Result.Failure<OutboundClickPlatformStats>(unknownError);

        var days = CreateStatsDays(response, "click platform stats", day => new OutboundClickPlatformStatsDay
        {
            Date = day.Date,
            Desktop = GetOptionalStatsDayCount(day.Response, "Desktop", day.Index),
            Mobile = GetOptionalStatsDayCount(day.Response, "Mobile", day.Index),
            Unknown = GetOptionalStatsDayCount(day.Response, "Unknown", day.Index)
        });
        if (days.IsFailure(out var daysError, out var mappedDays))
            return Result.Failure<OutboundClickPlatformStats>(daysError);

        return Result.Success(new OutboundClickPlatformStats { Desktop = mappedDesktop, Mobile = mappedMobile, Unknown = mappedUnknown, Days = mappedDays });
    }

    private static Result<OutboundClickLocationStats> CreateOutboundClickLocationStats(StatsSeriesModel response)
    {
        var html = GetRequiredStatsTotal(response, "HTML");
        if (html.IsFailure(out var htmlError, out var mappedHtml))
            return Result.Failure<OutboundClickLocationStats>(htmlError);

        var text = GetRequiredStatsTotal(response, "Text");
        if (text.IsFailure(out var textError, out var mappedText))
            return Result.Failure<OutboundClickLocationStats>(textError);

        var days = CreateStatsDays(response, "click location stats", day => new OutboundClickLocationStatsDay
        {
            Date = day.Date,
            Html = GetOptionalStatsDayCount(day.Response, "HTML", day.Index),
            Text = GetOptionalStatsDayCount(day.Response, "Text", day.Index)
        });
        if (days.IsFailure(out var daysError, out var mappedDays))
            return Result.Failure<OutboundClickLocationStats>(daysError);

        return Result.Success(new OutboundClickLocationStats { Html = mappedHtml, Text = mappedText, Days = mappedDays });
    }

    private static Result<OutboundUsageStatsCore> CreateOutboundUsageStats(StatsSeriesModel response, string context)
    {
        var totals = CreateStatsCounts(response.Totals, $"{context} totals", true);
        if (totals.IsFailure(out var totalsError, out var mappedTotals))
            return Result.Failure<OutboundUsageStatsCore>(totalsError);

        if (response.Days is null)
            return Result.Failure<OutboundUsageStatsCore>("Days were not returned from the Postmark Stats API.");

        var days = new List<OutboundUsageStatsDay>(response.Days.Count);
        for (var index = 0; index < response.Days.Count; index++)
        {
            var day = response.Days[index];
            if (day is null)
                return Result.Failure<OutboundUsageStatsCore>($"Days item {index} returned from the Postmark Stats API was null.");

            if (day.Date is null)
                return Result.Failure<OutboundUsageStatsCore>($"Date was not returned from the Postmark Stats API for Days item {index}.");

            var counts = CreateStatsCounts(day.Counts, $"Days item {index}", false);
            if (counts.IsFailure(out var countsError, out var mappedCounts))
                return Result.Failure<OutboundUsageStatsCore>(countsError);

            days.Add(new OutboundUsageStatsDay { Date = day.Date.Value, Counts = mappedCounts });
        }

        return Result.Success(new OutboundUsageStatsCore(mappedTotals, days));
    }

    private static Result<IReadOnlyList<TDay>> CreateStatsDays<TDay>(StatsSeriesModel response, string context, Func<StatsDayContext, TDay> createDay)
    {
        if (response.Days is null)
            return Result.Failure<IReadOnlyList<TDay>>("Days were not returned from the Postmark Stats API.");

        var days = new List<TDay>(response.Days.Count);
        for (var index = 0; index < response.Days.Count; index++)
        {
            var day = response.Days[index];
            if (day is null)
                return Result.Failure<IReadOnlyList<TDay>>($"Days item {index} returned from the Postmark Stats API was null.");

            if (day.Date is null)
                return Result.Failure<IReadOnlyList<TDay>>($"Date was not returned from the Postmark Stats API for {context} Days item {index}.");

            var countsError = ValidationExtensions.ValidateStatsCountValues(day.Counts, $"Days item {index}", false);
            if (countsError is not null)
                return Result.Failure<IReadOnlyList<TDay>>(countsError);

            days.Add(createDay(new StatsDayContext(day.Date.Value, day, index)));
        }

        return Result.Success<IReadOnlyList<TDay>>(days);
    }

    private static Result<IReadOnlyDictionary<string, int>> CreateStatsCounts(Dictionary<string, JsonElement>? values, string context, bool requireAtLeastOne)
    {
        var validationError = ValidationExtensions.ValidateStatsCountValues(values, context, requireAtLeastOne);
        if (validationError is not null)
            return Result.Failure<IReadOnlyDictionary<string, int>>(validationError);

        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        if (values is not null)
            foreach (var entry in values)
                counts.Add(entry.Key, entry.Value.GetInt32());

        return Result.Success<IReadOnlyDictionary<string, int>>(new ReadOnlyDictionary<string, int>(counts));
    }

    private static Result<int> GetRequiredStatsTotal(StatsSeriesModel response, string propertyName)
    {
        if (response.Totals is null || !response.Totals.TryGetValue(propertyName, out var value))
            return Result.Failure<int>($"{propertyName} was not returned from the Postmark Stats API.");

        return GetRequiredStatsCountValue(value, propertyName);
    }

    private static int GetOptionalStatsDayCount(StatsDayModel response, string propertyName, int index)
    {
        if (response.Counts is null || !response.Counts.TryGetValue(propertyName, out var value))
            return 0;

        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out var count) || count < 0)
            throw new InvalidOperationException($"The Postmark Stats API returned an invalid {propertyName} value for Days item {index}. This should have been caught by validation.");

        return count;
    }

    private static Result<int> GetRequiredStatsCountValue(JsonElement value, string propertyName)
    {
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out var count))
            return Result.Failure<int>($"{propertyName} returned from the Postmark Stats API was not an integer.");

        if (count < 0)
            return Result.Failure<int>($"{propertyName} returned from the Postmark Stats API was invalid. Received {count}.");

        return Result.Success(count);
    }

    private static Result<int> GetRequiredOverviewCount(int? value, string propertyName)
    {
        if (value is null)
            return Result.Failure<int>($"{propertyName} was not returned from the Postmark Stats API.");

        if (value.Value < 0)
            return Result.Failure<int>($"{propertyName} returned from the Postmark Stats API was invalid. Received {value.Value}.");

        return Result.Success(value.Value);
    }

    private static Result<double> GetRequiredOverviewRate(double? value, string propertyName)
    {
        if (value is null)
            return Result.Failure<double>($"{propertyName} was not returned from the Postmark Stats API.");

        if (double.IsNaN(value.Value) || double.IsInfinity(value.Value) || value.Value < 0)
            return Result.Failure<double>($"{propertyName} returned from the Postmark Stats API was invalid. Received {value.Value.ToString(CultureInfo.InvariantCulture)}.");

        return Result.Success(value.Value);
    }

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to retrieve outbound stats.")]
    private partial void LogOutboundStatsException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to retrieve outbound stats. {Message}")]
    private partial void LogOutboundStatsError(string message, [LogProperties] IError error);

    private readonly record struct StatsDayContext(DateOnly Date, StatsDayModel Response, int Index);

    private readonly record struct OutboundUsageStatsCore(IReadOnlyDictionary<string, int> Totals, IReadOnlyList<OutboundUsageStatsDay> Days);
}
