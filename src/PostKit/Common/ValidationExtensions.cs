using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using LightResults;
using PostKit.Bounces;
using PostKit.Messages;
using PostKit.MessageStreams;
using PostKit.Servers;
using PostKit.Stats;
using PostKit.Suppressions;
using PostKit.Templates;
using PostKit.Webhooks;
using DomainResponseModel = PostKit.Postmark.Domains.DomainResponse;
using SenderSignatureSummaryModel = PostKit.Postmark.SenderSignatures.SenderSignatureSummaryResponse;

namespace PostKit.Common;

internal static partial class ValidationExtensions
{
    public const int MaxBounceCount = 500;
    public const int MaxInboundMessageCount = 500;
    public const int MaxMessageTrackingCount = 500;
    public const int MaxOutboundMessageCount = 500;
    public const int MaxTemplateCount = 100;

    private const int MaxBounceSearchWindow = 10_000;
    private const int MaxInboundMessageSearchWindow = 10_000;
    private const int MaxListCount = 500;
    private const int MaxMessageTrackingSearchWindow = 10_000;
    private const int MaxOutboundMessageSearchWindow = 10_000;
    private const int MessageStreamIdMaxLength = 30;
    private const int MetadataNameMaxLength = 20;
    private const int MetadataValueMaxLength = 80;
    private const int TemplateAliasMaxLength = 64;
    private const string HeaderNameRuleMessage = "The header name is required and must contain only visible ASCII characters except ':'.";
    private const string HeaderValueRuleMessage = "The header value must contain only visible ASCII characters or tab, and folded lines must use CRLF followed by a space or tab.";
    private const string MetadataNameRuleMessage = "The metadata name is required, must not exceed 20 characters, and cannot start or end with whitespace.";
    private const string TemplateAliasRegexPattern = "^[A-Za-z][A-Za-z0-9._-]*$";

    public const string MessageStreamIdRequirements = "1-30 characters, start with a lowercase letter, contain only lowercase letters, numbers, '-', or '_', cannot contain consecutive hyphens, and cannot be 'all' or start with 'pm-'";

#if NET9_0_OR_GREATER
    [GeneratedRegex(TemplateAliasRegexPattern)]
    private static partial Regex TemplateAliasRegex { get; }
#else
    [GeneratedRegex(TemplateAliasRegexPattern)]
    private static partial Regex TemplateAliasRegex();
#endif

    public static void EnsureNotSet<T>(this T? field, string propertyName, string? attemptedPropertyName = null, string? guidance = null)
    {
        if (field is not null)
        {
            var attempted = string.IsNullOrWhiteSpace(attemptedPropertyName) ? propertyName : attemptedPropertyName;
            var message = string.Equals(attempted, propertyName, StringComparison.Ordinal) ? $"Cannot set {propertyName} because it has already been set." : $"Cannot set {attempted} because {propertyName} has already been set.";

            if (!string.IsNullOrWhiteSpace(guidance))
                message = $"{message} {guidance}";

            throw new InvalidOperationException(message);
        }
    }

    /// <summary>Gets the length using UTF-16 code units, which matches how Postmark applies the documented limits for fields such as <c>From</c> and <c>Subject</c>.</summary>
    /// <param name="input">The input span to measure.</param>
    /// <returns>The number of UTF-16 code units.</returns>
    public static int GetPostmarkCharacterCount(this ReadOnlySpan<char> input)
    {
        return input.Length;
    }

    public static bool IsValidMessageStreamId(this ReadOnlySpan<char> streamId)
    {
        if (streamId.Length is 0 or > MessageStreamIdMaxLength)
            return false;

        if (streamId[0] is < 'a' or > 'z')
            return false;

        if (streamId[0] == '-' || streamId[^1] == '-')
            return false;

        if (streamId.StartsWith("pm-", StringComparison.OrdinalIgnoreCase))
            return false;

        if (streamId.Equals("all".AsSpan(), StringComparison.OrdinalIgnoreCase))
            return false;

        var previousWasDash = false;
        foreach (var ch in streamId)
        {
            var isLowercaseLetter = ch is >= 'a' and <= 'z';
            var isDigit = ch is >= '0' and <= '9';
            var isHyphen = ch == '-';
            var isUnderscore = ch == '_';

            if (!isLowercaseLetter && !isDigit && !isHyphen && !isUnderscore)
                return false;

            if (isHyphen)
            {
                if (previousWasDash)
                    return false;

                previousWasDash = true;
            }
            else
            {
                previousWasDash = false;
            }
        }

        return true;
    }

    public static void ValidateMessageStreamId(string? messageStreamId, string paramName)
    {
        if (messageStreamId is null)
            throw new ArgumentNullException(paramName, "The message stream ID cannot be null.");

        if (!messageStreamId.AsSpan()
                .IsValidMessageStreamId())
            throw new ArgumentException(FormatMessageStreamIdValidationMessage(messageStreamId, "The message stream ID"), paramName);
    }

    public static string FormatMessageStreamIdValidationMessage(string messageStreamId, string subject)
    {
        return $"{subject} must be {MessageStreamIdRequirements}. {GetMessageStreamIdValidationDetail(messageStreamId.AsSpan())}";
    }

    public static string? ValidateListRequest(string subject, int count, int offset)
    {
        if (count is < 1 or > MaxListCount)
            return $"{subject} count must be between 1 and {MaxListCount}. Received {count}.";

        if (offset < 0)
            return $"{subject} offset must be zero or greater. Received {offset}.";

        return null;
    }

    public static string? ValidateId(long id, string subject)
    {
        return id <= 0 ? $"{subject} must be greater than zero. Received {id}." : null;
    }

    public static string? ValidateRequiredText(string? value, string subject)
    {
        if (value is null)
            return $"{subject} cannot be null.";

        if (string.IsNullOrWhiteSpace(value))
            return $"{subject} cannot be empty or whitespace. Actual length: {value.Length}.";

        return null;
    }

    public static string? ValidateOptionalText(string? value, string subject)
    {
        if (value is null)
            return null;

        if (string.IsNullOrWhiteSpace(value))
            return $"{subject} cannot be empty or whitespace. Set the value to null to omit it. Actual length: {value.Length}.";

        return null;
    }

    public static string? ValidateOptionalUrl(string? url, string subject)
    {
        if (url is null || url.Length == 0)
            return null;

        if (string.IsNullOrWhiteSpace(url))
            return $"{subject} cannot be whitespace. Use an empty string to clear it or null to omit it. Actual length: {url.Length}.";

        if (!Uri.TryCreate(url, UriKind.Absolute, out var parsed) || parsed.Scheme is not ("http" or "https"))
            return $"{subject} must be an absolute HTTP or HTTPS URL.";

        return null;
    }

    public static string? ValidateDomainName(string? value, string subject, bool allowNull, bool allowEmpty)
    {
        if (value is null)
            return allowNull ? null : $"{subject} cannot be null.";

        if (value.Length == 0 && allowEmpty)
            return null;

        if (string.IsNullOrWhiteSpace(value))
            return allowEmpty ? $"{subject} cannot be whitespace. Use an empty string to clear it or null to omit it. Actual length: {value.Length}." : $"{subject} cannot be empty or whitespace. Actual length: {value.Length}.";

        if (!LooksLikeDomainName(value))
            return $"{subject} must be a domain name like 'example.com'.";

        return null;
    }

    public static string? ValidateEmailAddress(string? value, string propertyName, string subject)
    {
        return ValidateEmailAddress(value, propertyName, subject, "sender@example.com");
    }

    public static string? ValidateOptionalEmailAddress(string? value, string propertyName, string subject)
    {
        if (value is null || value.Length == 0)
            return null;

        if (string.IsNullOrWhiteSpace(value))
            return $"{subject} cannot be whitespace. Use an empty string to clear it or null to omit it. Actual length: {value.Length}.";

        if (!LooksLikePostmarkEmailAddress(value))
            return $"{subject} must be a valid email address. Set {propertyName} to an address like 'sender@example.com'.";

        return null;
    }

    public static string? ValidateDataRemovalEmailAddress(string? value, string propertyName, string subject)
    {
        return ValidateEmailAddress(value, propertyName, subject, "recipient@example.com");
    }

    public static string? ValidateBounceQuery(string messageStream, int count, int offset, BounceQuery? query)
    {
        if (string.IsNullOrWhiteSpace(messageStream))
            return $"The bounce query message stream cannot be empty or whitespace. Actual length: {messageStream.Length}.";

        if (!messageStream.AsSpan()
                .IsValidMessageStreamId())
            return FormatMessageStreamIdValidationMessage(messageStream, "The bounce query message stream");

        if (count is < 1 or > MaxBounceCount)
            return $"The bounce query count must be between 1 and {MaxBounceCount}. Received {count}.";

        if (offset < 0)
            return $"The bounce query offset must be zero or greater. Received {offset}.";

        var searchWindow = (long)count + offset;
        if (searchWindow > MaxBounceSearchWindow)
            return $"The bounce query count and offset cannot exceed {MaxBounceSearchWindow} when combined. Count: {count}; offset: {offset}; combined: {searchWindow}.";

        if (query is null)
            return null;

        if (query.EmailFilter is not null && string.IsNullOrWhiteSpace(query.EmailFilter.Address))
            return FormatEmptyOptionalFilterMessage("The bounce query email filter", nameof(BounceQuery.EmailFilter), query.EmailFilter.Address);

        if (query.Tag is not null && string.IsNullOrWhiteSpace(query.Tag))
            return FormatEmptyOptionalFilterMessage("The bounce query tag filter", nameof(BounceQuery.Tag), query.Tag);

        if (query is { FromDate: not null, ToDate: not null } && query.FromDate.Value > query.ToDate.Value)
            return $"The bounce query from-date must not be later than the to-date. FromDate: {FormatOffsetForMessage(query.FromDate.Value)}; ToDate: {FormatOffsetForMessage(query.ToDate.Value)}.";

        return null;
    }

    public static Result ValidateDomainCore(DomainResponseModel response, string context)
    {
        if (response.Id is null)
            return Result.Failure($"ID was not returned from the Postmark Domains API for {context}.");

        if (response.Id.Value <= 0)
            return Result.Failure($"ID returned from the Postmark Domains API for {context} was invalid. Received {response.Id.Value}.");

        if (string.IsNullOrWhiteSpace(response.Name))
            return Result.Failure($"Name was not returned from the Postmark Domains API for {context}.");

        return Result.Success();
    }

    public static string? ValidateInboundRuleListRequest(int count, int offset)
    {
        if (count < 1)
            return $"The inbound rule trigger list count must be greater than zero. Received {count}.";

        if (offset < 0)
            return $"The inbound rule trigger list offset must be zero or greater. Received {offset}.";

        return null;
    }

    public static string? ValidateInboundRuleTriggerId(long id)
    {
        return id <= 0 ? $"The inbound rule trigger ID must be greater than zero. Received {id}." : null;
    }

    public static string? ValidateMessageStreamQuery(MessageStreamQuery? query)
    {
        if (query?.MessageStreamType.HasValue == true && !Enum.IsDefined(query.MessageStreamType.Value))
            return
                $"The message stream list type filter must be MessageStreamListType.All, MessageStreamListType.Inbound, MessageStreamListType.Transactional, or MessageStreamListType.Broadcasts. Received {(int)query.MessageStreamType.Value}.";

        return null;
    }

    public static string? ValidateClientMessageStreamId(string? value, string subject)
    {
        if (value is null)
            return $"{subject} cannot be null.";

        if (string.IsNullOrWhiteSpace(value))
            return $"{subject} cannot be empty or whitespace. Actual length: {value.Length}.";

        return value.AsSpan()
            .IsValidMessageStreamId()
            ? null
            : FormatMessageStreamIdValidationMessage(value, subject);
    }

    public static string? ValidateCreateMessageStreamType(MessageStreamType type, string subject)
    {
        if (!Enum.IsDefined(type))
            return $"{subject} must be MessageStreamType.Transactional or MessageStreamType.Broadcasts. Received {(int)type}.";

        return type == MessageStreamType.Inbound ? $"{subject} cannot be MessageStreamType.Inbound because Postmark only allows creating transactional or broadcasts streams." : null;
    }

    public static string? ValidateMessageStreamSubscriptionManagement(MessageStreamSubscriptionManagementConfiguration? configuration, string subject)
    {
        if (configuration is null)
            return null;

        if (!configuration.UnsubscribeHandlingType.HasValue)
            return $"{subject} must set UnsubscribeHandlingType.";

        if (!Enum.IsDefined(configuration.UnsubscribeHandlingType.Value))
            return $"{subject} unsubscribe handling type must be UnsubscribeHandlingType.None, UnsubscribeHandlingType.Postmark, or UnsubscribeHandlingType.Custom. Received {(int)configuration.UnsubscribeHandlingType.Value}.";

        return null;
    }

    public static string? ValidateClearableManagementText(string? value, string subject)
    {
        if (value is null || value.Length == 0)
            return null;

        return string.IsNullOrWhiteSpace(value) ? $"{subject} cannot be whitespace. Use an empty string to clear it or null to omit it. Actual length: {value.Length}." : null;
    }

    public static string? ValidateInboundMessageQuery(int count, int offset, InboundMessageQuery? query)
    {
        if (count is < 1 or > MaxInboundMessageCount)
            return $"The inbound message query count must be between 1 and {MaxInboundMessageCount}. Received {count}.";

        if (offset < 0)
            return $"The inbound message query offset must be zero or greater. Received {offset}.";

        var searchWindow = (long)count + offset;
        if (searchWindow > MaxInboundMessageSearchWindow)
            return $"The inbound message query count and offset cannot exceed {MaxInboundMessageSearchWindow} when combined. Count: {count}; offset: {offset}; combined: {searchWindow}.";

        if (query is null)
            return null;

        if (query.Recipient is not null && string.IsNullOrWhiteSpace(query.Recipient.Address))
            return FormatEmptyOptionalFilterMessage("The inbound message query recipient filter", nameof(InboundMessageQuery.Recipient), query.Recipient.Address);

        if (query.FromEmail is not null && string.IsNullOrWhiteSpace(query.FromEmail.Address))
            return FormatEmptyOptionalFilterMessage("The inbound message query from-email filter", nameof(InboundMessageQuery.FromEmail), query.FromEmail.Address);

        if (query.Tag is not null && string.IsNullOrWhiteSpace(query.Tag))
            return FormatEmptyOptionalFilterMessage("The inbound message query tag filter", nameof(InboundMessageQuery.Tag), query.Tag);

        if (query.Subject is not null && string.IsNullOrWhiteSpace(query.Subject))
            return FormatEmptyOptionalFilterMessage("The inbound message query subject filter", nameof(InboundMessageQuery.Subject), query.Subject);

        if (query.MailboxHash is not null && string.IsNullOrWhiteSpace(query.MailboxHash))
            return FormatEmptyOptionalFilterMessage("The inbound message query mailbox hash filter", nameof(InboundMessageQuery.MailboxHash), query.MailboxHash);

        if (query.Status.HasValue && !Enum.IsDefined(query.Status.Value))
            return
                $"The inbound message query status filter must be InboundMessageStatus.Blocked, InboundMessageStatus.Processed, InboundMessageStatus.Queued, InboundMessageStatus.Failed, or InboundMessageStatus.Scheduled. Received {(int)query.Status.Value}.";

        if (query is { FromDate: not null, ToDate: not null } && query.FromDate.Value > query.ToDate.Value)
            return $"The inbound message query from-date must not be later than the to-date. FromDate: {FormatOffsetForMessage(query.FromDate.Value)}; ToDate: {FormatOffsetForMessage(query.ToDate.Value)}.";

        return null;
    }

    public static string? ValidateOutboundMessageQuery(string messageStream, int count, int offset, OutboundMessageQuery? query)
    {
        if (string.IsNullOrWhiteSpace(messageStream))
            return $"The outbound message query message stream cannot be empty or whitespace. Actual length: {messageStream.Length}.";

        if (!messageStream.AsSpan()
                .IsValidMessageStreamId())
            return FormatMessageStreamIdValidationMessage(messageStream, "The outbound message query message stream");

        if (count is < 1 or > MaxOutboundMessageCount)
            return $"The outbound message query count must be between 1 and {MaxOutboundMessageCount}. Received {count}.";

        if (offset < 0)
            return $"The outbound message query offset must be zero or greater. Received {offset}.";

        var searchWindow = (long)count + offset;
        if (searchWindow > MaxOutboundMessageSearchWindow)
            return $"The outbound message query count and offset cannot exceed {MaxOutboundMessageSearchWindow} when combined. Count: {count}; offset: {offset}; combined: {searchWindow}.";

        if (query is null)
            return null;

        if (query.Recipient is not null && string.IsNullOrWhiteSpace(query.Recipient.Address))
            return FormatEmptyOptionalFilterMessage("The outbound message query recipient filter", nameof(OutboundMessageQuery.Recipient), query.Recipient.Address);

        if (query.FromEmail is not null && string.IsNullOrWhiteSpace(query.FromEmail.Address))
            return FormatEmptyOptionalFilterMessage("The outbound message query from-email filter", nameof(OutboundMessageQuery.FromEmail), query.FromEmail.Address);

        if (query.Tag is not null && string.IsNullOrWhiteSpace(query.Tag))
            return FormatEmptyOptionalFilterMessage("The outbound message query tag filter", nameof(OutboundMessageQuery.Tag), query.Tag);

        if (query.Status.HasValue && !Enum.IsDefined(query.Status.Value))
            return $"The outbound message query status filter must be OutboundMessageStatus.Queued, OutboundMessageStatus.Sent, or OutboundMessageStatus.Processed. Received {(int)query.Status.Value}.";

        if (query.Subject is not null && string.IsNullOrWhiteSpace(query.Subject))
            return FormatEmptyOptionalFilterMessage("The outbound message query subject filter", nameof(OutboundMessageQuery.Subject), query.Subject);

        var metadataValidationError = ValidateOutboundMessageMetadataFilter(query.Metadata);
        if (metadataValidationError is not null)
            return metadataValidationError;

        if (query is { FromDate: not null, ToDate: not null } && query.FromDate.Value > query.ToDate.Value)
            return $"The outbound message query from-date must not be later than the to-date. FromDate: {FormatOffsetForMessage(query.FromDate.Value)}; ToDate: {FormatOffsetForMessage(query.ToDate.Value)}.";

        return null;
    }

    public static string? ValidateOutboundMessageMetadataFilter(OutboundMessageMetadataFilter? metadata)
    {
        if (metadata is null)
            return null;

        if (metadata.Name is null)
            return "The outbound message query metadata filter name cannot be null. Set Metadata to null to omit this filter.";

        if (string.IsNullOrWhiteSpace(metadata.Name))
            return $"The outbound message query metadata filter name cannot be empty or whitespace. Set Metadata to null to omit this filter. Actual length: {metadata.Name.Length}.";

        if (metadata.Name.Length > MetadataNameMaxLength)
            return $"The outbound message query metadata filter name must not exceed {MetadataNameMaxLength} characters. Actual length: {metadata.Name.Length}.";

        if (char.IsWhiteSpace(metadata.Name[0]))
            return
                $"The outbound message query metadata filter name is invalid. The metadata name is required, must not exceed {MetadataNameMaxLength} characters, and cannot start or end with whitespace. Invalid leading whitespace {FormatCharacter(metadata.Name[0])} at index 0.";

        if (char.IsWhiteSpace(metadata.Name[^1]))
            return
                $"The outbound message query metadata filter name is invalid. The metadata name is required, must not exceed {MetadataNameMaxLength} characters, and cannot start or end with whitespace. Invalid trailing whitespace {FormatCharacter(metadata.Name[^1])} at index {metadata.Name.Length - 1}.";

        if (metadata.Value is null)
            return "The outbound message query metadata filter value cannot be null. Set Metadata to null to omit this filter.";

        if (string.IsNullOrWhiteSpace(metadata.Value))
            return $"The outbound message query metadata filter value cannot be empty or whitespace. Set Metadata to null to omit this filter. Actual length: {metadata.Value.Length}.";

        if (metadata.Value.Length > MetadataValueMaxLength)
            return $"The outbound message query metadata filter value must not exceed {MetadataValueMaxLength} characters. Actual length: {metadata.Value.Length}.";

        return null;
    }

    public static string? ValidateMessageTrackingSearchRequest(string messageStream, int count, int offset, MessageTrackingQuery? query, string operationName, bool enforceSearchWindow)
    {
        if (string.IsNullOrWhiteSpace(messageStream))
            return $"The {operationName} message stream cannot be empty or whitespace. Actual length: {messageStream.Length}.";

        if (!messageStream.AsSpan()
                .IsValidMessageStreamId())
            return FormatMessageStreamIdValidationMessage(messageStream, $"The {operationName} message stream");

        var validationError = ValidateMessageTrackingWindow(count, offset, operationName, enforceSearchWindow);
        if (validationError is not null)
            return validationError;

        return ValidateMessageTrackingQuery(query, operationName);
    }

    public static string? ValidateSingleMessageTrackingRequest(Guid messageId, int count, int offset, string operationName)
    {
        if (messageId == Guid.Empty)
            return $"The outbound message ID for {operationName} must not be empty.";

        return ValidateMessageTrackingWindow(count, offset, operationName, false);
    }

    public static string? ValidateMessageTrackingWindow(int count, int offset, string operationName, bool enforceSearchWindow)
    {
        if (count is < 1 or > MaxMessageTrackingCount)
            return $"The {operationName} count must be between 1 and {MaxMessageTrackingCount}. Received {count}.";

        if (offset < 0)
            return $"The {operationName} offset must be zero or greater. Received {offset}.";

        var searchWindow = (long)count + offset;
        if (enforceSearchWindow && searchWindow > MaxMessageTrackingSearchWindow)
            return $"The {operationName} count and offset cannot exceed {MaxMessageTrackingSearchWindow} when combined. Count: {count}; offset: {offset}; combined: {searchWindow}.";

        return null;
    }

    public static string? ValidateMessageTrackingQuery(MessageTrackingQuery? query, string operationName)
    {
        if (query is null)
            return null;

        if (query.Recipient is not null && string.IsNullOrWhiteSpace(query.Recipient.Address))
            return FormatEmptyOptionalFilterMessage($"The {operationName} recipient filter", nameof(MessageTrackingQuery.Recipient), query.Recipient.Address);

        var validationError = ValidateMessageTrackingStringFilter(query.Tag, nameof(MessageTrackingQuery.Tag), "tag", operationName);
        if (validationError is not null)
            return validationError;

        validationError = ValidateMessageTrackingStringFilter(query.ClientName, nameof(MessageTrackingQuery.ClientName), "client name", operationName);
        if (validationError is not null)
            return validationError;

        validationError = ValidateMessageTrackingStringFilter(query.ClientCompany, nameof(MessageTrackingQuery.ClientCompany), "client company", operationName);
        if (validationError is not null)
            return validationError;

        validationError = ValidateMessageTrackingStringFilter(query.ClientFamily, nameof(MessageTrackingQuery.ClientFamily), "client family", operationName);
        if (validationError is not null)
            return validationError;

        validationError = ValidateMessageTrackingStringFilter(query.OsName, nameof(MessageTrackingQuery.OsName), "operating system name", operationName);
        if (validationError is not null)
            return validationError;

        validationError = ValidateMessageTrackingStringFilter(query.OsFamily, nameof(MessageTrackingQuery.OsFamily), "operating system family", operationName);
        if (validationError is not null)
            return validationError;

        validationError = ValidateMessageTrackingStringFilter(query.OsCompany, nameof(MessageTrackingQuery.OsCompany), "operating system company", operationName);
        if (validationError is not null)
            return validationError;

        validationError = ValidateMessageTrackingStringFilter(query.Platform, nameof(MessageTrackingQuery.Platform), "platform", operationName);
        if (validationError is not null)
            return validationError;

        validationError = ValidateMessageTrackingStringFilter(query.Country, nameof(MessageTrackingQuery.Country), "country", operationName);
        if (validationError is not null)
            return validationError;

        validationError = ValidateMessageTrackingStringFilter(query.Region, nameof(MessageTrackingQuery.Region), "region", operationName);
        if (validationError is not null)
            return validationError;

        return ValidateMessageTrackingStringFilter(query.City, nameof(MessageTrackingQuery.City), "city", operationName);
    }

    public static string? ValidateMessageTrackingStringFilter(string? value, string propertyName, string filterName, string operationName)
    {
        return value is not null && string.IsNullOrWhiteSpace(value) ? FormatEmptyOptionalFilterMessage($"The {operationName} {filterName} filter", propertyName, value) : null;
    }

    public static string? ValidateOutboundStatsQuery(OutboundStatsQuery? query)
    {
        if (query is null)
            return null;

        if (query.Tag is not null && string.IsNullOrWhiteSpace(query.Tag))
            return $"The outbound stats query tag filter cannot be empty or whitespace. Set Tag to null to omit this filter. Actual length: {query.Tag.Length}.";

        if (query.MessageStreamId is not null)
        {
            if (string.IsNullOrWhiteSpace(query.MessageStreamId))
                return $"The outbound stats query message stream filter cannot be empty or whitespace. Set MessageStreamId to null to omit this filter. Actual length: {query.MessageStreamId.Length}.";

            if (!query.MessageStreamId.AsSpan()
                    .IsValidMessageStreamId())
                return FormatMessageStreamIdValidationMessage(query.MessageStreamId, "The outbound stats query message stream filter");
        }

        if (query is { FromDate: not null, ToDate: not null } && query.FromDate.Value > query.ToDate.Value)
            return $"The outbound stats query from-date must not be later than the to-date. FromDate: {FormatStatsDate(query.FromDate.Value)}; ToDate: {FormatStatsDate(query.ToDate.Value)}.";

        return null;
    }

    public static string? ValidateStatsCountValues(Dictionary<string, JsonElement>? values, string context, bool requireAtLeastOne)
    {
        if (values is null || values.Count == 0)
            return requireAtLeastOne ? $"No count values were returned from the Postmark Stats API for {context}." : null;

        foreach (var entry in values)
        {
            if (string.IsNullOrWhiteSpace(entry.Key))
                return $"A count name returned from the Postmark Stats API for {context} was empty.";

            if (entry.Value.ValueKind != JsonValueKind.Number || !entry.Value.TryGetInt32(out var value))
                return $"Count value '{entry.Key}' returned from the Postmark Stats API for {context} was not an integer.";

            if (value < 0)
                return $"Count value '{entry.Key}' returned from the Postmark Stats API for {context} was invalid. Received {value}.";
        }

        return null;
    }

    public static string? ValidateSuppressionQuery(SuppressionQuery? query)
    {
        if (query is null)
            return null;

        if (query.EmailAddress is not null && string.IsNullOrWhiteSpace(query.EmailAddress))
            return FormatEmptyOptionalFilterMessage("The suppression query email address filter", nameof(SuppressionQuery.EmailAddress), query.EmailAddress);

        if (query is { FromDate: not null, ToDate: not null } && query.FromDate.Value > query.ToDate.Value)
            return $"The suppression query from-date must not be later than the to-date. FromDate: {FormatSuppressionQueryDate(query.FromDate.Value)}; ToDate: {FormatSuppressionQueryDate(query.ToDate.Value)}.";

        return null;
    }

    public static string? ValidateSuppressionMessageStream(string? messageStream, string subject)
    {
        if (string.IsNullOrWhiteSpace(messageStream))
            return $"{subject} cannot be empty or whitespace. Actual length: {messageStream?.Length ?? 0}.";

        if (!messageStream.AsSpan()
                .IsValidMessageStreamId())
            return FormatMessageStreamIdValidationMessage(messageStream, subject);

        return null;
    }

    public static string? ValidateTemplateParameters(string? name, string? alias, string? subject, string? htmlBody, string? textBody, TemplateType? templateType, string? layoutTemplate, string operationName)
    {
        if (name is null)
            return $"The template {operationName} parameters name cannot be null.";

        if (string.IsNullOrWhiteSpace(name))
            return $"The template {operationName} parameters name cannot be empty or whitespace. Actual length: {name.Length}.";

        if (alias is not null)
        {
            var aliasValidation = ValidateTemplateAlias(alias, $"The template {operationName} parameters alias", false);
            if (aliasValidation is not null)
                return aliasValidation;
        }

        if (layoutTemplate is not null)
        {
            var layoutValidation = ValidateTemplateAlias(layoutTemplate, $"The template {operationName} parameters layout template", true);
            if (layoutValidation is not null)
                return layoutValidation;
        }

        if (templateType.HasValue && !Enum.IsDefined(templateType.Value))
            return $"The template {operationName} parameters template type must be TemplateType.Standard or TemplateType.Layout. Received {(int)templateType.Value}.";

        var effectiveTemplateType = templateType ?? TemplateType.Standard;
        if (effectiveTemplateType == TemplateType.Layout)
        {
            if (subject is not null)
                return $"The template {operationName} parameters subject cannot be set for a layout template. Set Subject to null for layout templates.";
        }
        else if (subject is null)
        {
            return $"The template {operationName} parameters subject is required for standard templates.";
        }

        if (htmlBody is null && textBody is null)
            return $"The template {operationName} parameters must provide HtmlBody or TextBody.";

        return null;
    }

    public static string? ValidateTemplateValidationParameters(TemplateValidationParameters parameters)
    {
        if (parameters.Subject is null && parameters.HtmlBody is null && parameters.TextBody is null)
            return "The template validation parameters must provide Subject, HtmlBody, or TextBody.";

        if (parameters.TemplateType.HasValue && !Enum.IsDefined(parameters.TemplateType.Value))
            return $"The template validation parameters template type must be TemplateType.Standard or TemplateType.Layout. Received {(int)parameters.TemplateType.Value}.";

        if (parameters.LayoutTemplate is not null)
        {
            var layoutValidation = ValidateTemplateAlias(parameters.LayoutTemplate, "The template validation parameters layout template", false);
            if (layoutValidation is not null)
                return layoutValidation;
        }

        return null;
    }

    public static string? ValidateTemplateListRequest(int count, int offset, TemplateQuery? query)
    {
        if (count is < 1 or > MaxTemplateCount)
            return $"The template list count must be between 1 and {MaxTemplateCount}. Received {count}.";

        if (offset < 0)
            return $"The template list offset must be zero or greater. Received {offset}.";

        if (query is null)
            return null;

        if (query.TemplateType.HasValue && !Enum.IsDefined(query.TemplateType.Value))
            return $"The template list type filter must be TemplateListType.All, TemplateListType.Standard, or TemplateListType.Layout. Received {(int)query.TemplateType.Value}.";

        if (query.LayoutTemplate is not null)
        {
            var layoutValidation = ValidateTemplateAlias(query.LayoutTemplate, "The template list layout template filter", false);
            if (layoutValidation is not null)
                return layoutValidation;
        }

        return null;
    }

    public static string? ValidateTemplatePushParameters(TemplatePushParameters parameters)
    {
        if (parameters.SourceServerId <= 0)
            return $"The template push parameters source server ID must be greater than zero. Received {parameters.SourceServerId}.";

        if (parameters.DestinationServerId <= 0)
            return $"The template push parameters destination server ID must be greater than zero. Received {parameters.DestinationServerId}.";

        if (parameters.SourceServerId == parameters.DestinationServerId)
            return $"The template push parameters source and destination server IDs must be different. Received {parameters.SourceServerId}.";

        return null;
    }

    public static string? ValidateTemplateAlias(string value, string subject, bool allowEmptyString)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            if (allowEmptyString && value.Length == 0)
                return null;

            return $"{subject} cannot be empty or whitespace. Set the value to null to omit it. Actual length: {value.Length}.";
        }

        if (value.Length > TemplateAliasMaxLength)
            return $"{subject} must not exceed {TemplateAliasMaxLength} characters. Actual length: {value.Length}.";

#if NET9_0_OR_GREATER
        if (!TemplateAliasRegex.IsMatch(value))
#else
        if (!TemplateAliasRegex().IsMatch(value))
#endif
            return $"{subject} must start with a letter and may only contain letters, numbers, '-', '_', or '.' characters. {GetTemplateAliasValidationDetail(value)}";

        return null;
    }

    public static string? ValidateWebhookId(long id)
    {
        return id <= 0 ? $"The webhook ID must be greater than zero. Received {id}." : null;
    }

    public static string? ValidateWebhookUrl(string? url, string subject)
    {
        if (url is null)
            return subject.Contains("edit", StringComparison.Ordinal) ? null : $"{subject} cannot be null.";

        if (string.IsNullOrWhiteSpace(url))
            return $"{subject} cannot be empty or whitespace. Actual length: {url.Length}.";

        if (!Uri.TryCreate(url, UriKind.Absolute, out var parsed) || parsed.Scheme is not ("http" or "https"))
            return $"{subject} must be an absolute HTTP or HTTPS URL.";

        return null;
    }

    public static string? ValidateWebhookMessageStream(string? messageStream, string subject)
    {
        if (messageStream is null)
            return null;

        if (string.IsNullOrWhiteSpace(messageStream))
            return $"{subject} cannot be empty or whitespace. Set MessageStreamId to null to omit it. Actual length: {messageStream.Length}.";

        if (!messageStream.AsSpan()
                .IsValidMessageStreamId())
            return FormatMessageStreamIdValidationMessage(messageStream, subject);

        return null;
    }

    public static string? ValidateWebhookHttpAuth(WebhookHttpAuth? auth, string subject)
    {
        if (auth is null)
            return null;

        if (string.IsNullOrWhiteSpace(auth.Username))
            return $"{subject} username cannot be empty or whitespace. Actual length: {auth.Username?.Length ?? 0}.";

        if (string.IsNullOrWhiteSpace(auth.Password))
            return $"{subject} password cannot be empty or whitespace. Actual length: {auth.Password?.Length ?? 0}.";

        return null;
    }

    public static string? ValidateWebhookHeaders(IReadOnlyList<WebhookHeader>? headers, string subject)
    {
        if (headers is null)
            return null;

        for (var index = 0; index < headers.Count; index++)
        {
            var header = headers[index];
            if (header is null)
                return $"{subject} item {index} cannot be null.";

            try
            {
                ValidateHeader(header.Name, header.Value, nameof(headers));
            }
            catch (ArgumentException ex)
            {
                return $"{subject} item {index} is invalid: {ex.Message}";
            }
        }

        return null;
    }

    public static string? ValidateWebhookTriggers(WebhookTriggers? triggers, string subject)
    {
        if (triggers is null)
            return null;

        if (triggers.Open is null && triggers.Click is null && triggers.Delivery is null && triggers.Bounce is null && triggers.SpamComplaint is null && triggers.SubscriptionChange is null)
            return $"{subject} must include at least one trigger setting. Set Triggers to null to omit trigger changes.";

        return null;
    }

    public static string? ValidateServerName(string? value, string subject, bool required)
    {
        return required ? ValidateRequiredText(value, subject) : ValidateOptionalText(value, subject);
    }

    public static string? ValidateServerColor(ServerColor? color, string subject)
    {
        return color.HasValue && !Enum.IsDefined(color.Value)
            ? $"{subject} must be ServerColor.Purple, ServerColor.Blue, ServerColor.Turquoise, ServerColor.Green, ServerColor.Red, ServerColor.Yellow, ServerColor.Grey, or ServerColor.Orange. Received {(int)color.Value}."
            : null;
    }

    public static string? ValidateServerDeliveryType(ServerDeliveryType? deliveryType, string subject)
    {
        return deliveryType.HasValue && !Enum.IsDefined(deliveryType.Value) ? $"{subject} must be ServerDeliveryType.Live or ServerDeliveryType.Sandbox. Received {(int)deliveryType.Value}." : null;
    }

    public static string? ValidateServerHooks(ServerCreateParameters parameters, string subject)
    {
        var validationError = ValidateOptionalUrl(parameters.InboundHookUrl, $"{subject} inbound hook URL");
        if (validationError is not null)
            return validationError;

        validationError = ValidateOptionalUrl(parameters.BounceHookUrl, $"{subject} bounce hook URL");
        if (validationError is not null)
            return validationError;

        validationError = ValidateOptionalUrl(parameters.OpenHookUrl, $"{subject} open hook URL");
        if (validationError is not null)
            return validationError;

        validationError = ValidateOptionalUrl(parameters.DeliveryHookUrl, $"{subject} delivery hook URL");
        if (validationError is not null)
            return validationError;

        return ValidateOptionalUrl(parameters.ClickHookUrl, $"{subject} click hook URL");
    }

    public static string? ValidateServerHooks(ServerEditParameters parameters, string subject)
    {
        var validationError = ValidateOptionalUrl(parameters.InboundHookUrl, $"{subject} inbound hook URL");
        if (validationError is not null)
            return validationError;

        validationError = ValidateOptionalUrl(parameters.BounceHookUrl, $"{subject} bounce hook URL");
        if (validationError is not null)
            return validationError;

        validationError = ValidateOptionalUrl(parameters.OpenHookUrl, $"{subject} open hook URL");
        if (validationError is not null)
            return validationError;

        validationError = ValidateOptionalUrl(parameters.DeliveryHookUrl, $"{subject} delivery hook URL");
        if (validationError is not null)
            return validationError;

        return ValidateOptionalUrl(parameters.ClickHookUrl, $"{subject} click hook URL");
    }

    public static string? ValidateServerInboundDomain(string? domain, string subject)
    {
        return ValidateDomainName(domain, subject, true, true);
    }

    public static string? ValidateServerInboundSpamThreshold(int? value, string subject)
    {
        if (value is null)
            return null;

        return value is < 0 or > 30 ? $"{subject} must be between 0 and 30. Received {value.Value}." : null;
    }

    public static string? ValidateServerLinkTracking(LinkTracking? tracking, string subject)
    {
        return tracking.HasValue && !Enum.IsDefined(tracking.Value) ? $"{subject} must be LinkTracking.None, LinkTracking.HtmlAndText, LinkTracking.HtmlOnly, or LinkTracking.TextOnly. Received {(int)tracking.Value}." : null;
    }

    public static string? ValidateServerListRequest(int count, int offset, ServerQuery? query)
    {
        var validationError = ValidateListRequest("The server list", count, offset);
        if (validationError is not null)
            return validationError;

        return query is null ? null : ValidateOptionalText(query.Name, "The server list name filter");
    }

    public static Result ValidateSenderSignatureCore(SenderSignatureSummaryModel response, string context)
    {
        if (response.Id is null)
            return Result.Failure($"ID was not returned from the Postmark Sender Signatures API for {context}.");

        if (response.Id.Value <= 0)
            return Result.Failure($"ID returned from the Postmark Sender Signatures API for {context} was invalid. Received {response.Id.Value}.");

        if (string.IsNullOrWhiteSpace(response.Domain))
            return Result.Failure($"Domain was not returned from the Postmark Sender Signatures API for {context}.");

        if (string.IsNullOrWhiteSpace(response.EmailAddress))
            return Result.Failure($"EmailAddress was not returned from the Postmark Sender Signatures API for {context}.");

        if (string.IsNullOrWhiteSpace(response.Name))
            return Result.Failure($"Name was not returned from the Postmark Sender Signatures API for {context}.");

        if (response.Confirmed is null)
            return Result.Failure($"Confirmed was not returned from the Postmark Sender Signatures API for {context}.");

        return Result.Success();
    }

    public static string? NormalizeOptionalString(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    public static string FormatOffsetForMessage(DateTimeOffset value)
    {
        return value.ToString("O", CultureInfo.InvariantCulture);
    }

    public static string FormatStatsDate(DateOnly value)
    {
        return value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    public static string FormatSuppressionQueryDate(DateOnly value)
    {
        return value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    public static void EnsureAddressFirst(string? address, string? name, [CallerMemberName] string methodName = "")
    {
        if (LooksLikeEmailAddress(address) || !LooksLikeEmailAddress(name))
            return;

        throw new ArgumentException($"Display name overloads must specify the email address first: use .{methodName}(\"recipient@example.com\", \"Recipient Name\").", nameof(address));
    }

    public static void ValidateHeader(string? name, string? value, string paramName)
    {
        ValidateHeaderName(name, paramName);
        ValidateHeaderValue(value, paramName);
    }

    public static void ValidateHeaderName(string? name, string paramName)
    {
        if (name is null)
            throw new ArgumentNullException(paramName, "The header name cannot be null.");

        if (!IsValidHeaderName(name.AsSpan()))
            throw new ArgumentException(FormatHeaderNameValidationMessage(name), paramName);
    }

    public static void ValidateHeaderValue(string? value, string paramName)
    {
        if (value is null)
            throw new ArgumentNullException(paramName, "The header value cannot be null.");

        if (!IsValidHeaderValue(value.AsSpan()))
            throw new ArgumentException(FormatHeaderValueValidationMessage(value), paramName);
    }

    public static List<KeyValuePair<string, string>> SnapshotValidatedHeaders(IEnumerable<KeyValuePair<string, string>> headers, string paramName, IReadOnlyDictionary<string, string>? existingHeaders = null)
    {
        if (headers is null)
            throw new ArgumentNullException(paramName, "The header collection cannot be null.");

        var headerList = headers.ToList();
        var seen = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < headerList.Count; index++)
        {
            var header = headerList[index];
            if (header.Key is null)
                throw new ArgumentNullException(paramName, $"The header name at index {index} cannot be null.");

            if (!IsValidHeaderName(header.Key.AsSpan()))
                throw new ArgumentException($"The header name at index {index} is invalid. {FormatHeaderNameValidationMessage(header.Key)}", paramName);

            if (header.Value is null)
                throw new ArgumentNullException(paramName, $"The header value at index {index} cannot be null.");

            if (!IsValidHeaderValue(header.Value.AsSpan()))
                throw new ArgumentException($"The header value at index {index} is invalid. {FormatHeaderValueValidationMessage(header.Value)}", paramName);

            if (seen.TryGetValue(header.Key, out var duplicateIndex))
                throw new ArgumentException($"Header names must be unique and are compared case-insensitively. Header name '{header.Key}' at index {index} duplicates index {duplicateIndex}.", paramName);

            if (existingHeaders is not null && TryGetExistingKey(existingHeaders, header.Key, out var existingHeaderName))
                throw new ArgumentException(FormatDuplicateExistingHeaderMessage(header.Key, existingHeaderName, index), paramName);

            seen.Add(header.Key, index);
        }

        return headerList;
    }

    private static bool IsValidHeaderName(ReadOnlySpan<char> name)
    {
        return GetHeaderNameValidationDetail(name) is null;
    }

    private static bool IsValidHeaderValue(ReadOnlySpan<char> value)
    {
        // Empty values are valid on purpose. A live call on 2026-03-13 showed Postmark accepting a custom
        // header with a missing value, so do not reject empty values without re-validating the live API first.
        return GetHeaderValueValidationDetail(value) is null;
    }

    public static string FormatCharacter(char value)
    {
        return value switch
        {
            ' ' => "space",
            '\t' => "tab",
            >= (char)0x21 and <= (char)0x7E => $"'{value}'",
            _ => $"U+{(int)value:X4}"
        };
    }

    private static string FormatHeaderNameValidationMessage(string name)
    {
        var detail = GetHeaderNameValidationDetail(name.AsSpan());
        return detail is null ? HeaderNameRuleMessage : $"{HeaderNameRuleMessage} {detail}";
    }

    private static string FormatHeaderValueValidationMessage(string value)
    {
        var detail = GetHeaderValueValidationDetail(value.AsSpan());
        return detail is null ? HeaderValueRuleMessage : $"{HeaderValueRuleMessage} {detail}";
    }

    private static string GetMessageStreamIdValidationDetail(ReadOnlySpan<char> streamId)
    {
        if (streamId.IsEmpty)
            return "Actual length: 0.";

        if (streamId.Length > MessageStreamIdMaxLength)
            return $"Actual length: {streamId.Length}.";

        if (streamId.Equals("all".AsSpan(), StringComparison.OrdinalIgnoreCase))
            return "'all' is reserved.";

        if (streamId.StartsWith("pm-", StringComparison.OrdinalIgnoreCase))
            return "The prefix 'pm-' is reserved.";

        if (streamId[0] is < 'a' or > 'z')
            return $"First character must be a lowercase letter. Received {FormatCharacter(streamId[0])} at index 0.";

        if (streamId[^1] == '-')
            return "The message stream ID cannot end with '-'.";

        var previousWasDash = false;
        for (var index = 0; index < streamId.Length; index++)
        {
            var current = streamId[index];
            var isLowercaseLetter = current is >= 'a' and <= 'z';
            var isDigit = current is >= '0' and <= '9';
            var isHyphen = current == '-';
            var isUnderscore = current == '_';

            if (!isLowercaseLetter && !isDigit && !isHyphen && !isUnderscore)
                return $"Invalid character {FormatCharacter(current)} at index {index}.";

            if (isHyphen)
            {
                if (previousWasDash)
                    return $"Consecutive hyphen at index {index}.";

                previousWasDash = true;
            }
            else
            {
                previousWasDash = false;
            }
        }

        return "The value is invalid.";
    }

    private static string? GetHeaderNameValidationDetail(ReadOnlySpan<char> name)
    {
        if (name.IsEmpty)
            return "Actual length: 0.";

        for (var index = 0; index < name.Length; index++)
        {
            var current = name[index];
            var isVisibleAscii = current is >= (char)0x21 and <= (char)0x7E;
            if (!isVisibleAscii || current == ':')
                return $"Invalid character {FormatCharacter(current)} at index {index}.";
        }

        return null;
    }

    private static string? GetHeaderValueValidationDetail(ReadOnlySpan<char> value)
    {
        for (var i = 0; i < value.Length; i++)
        {
            var current = value[i];

            if (current == '\r')
            {
                if (i + 2 >= value.Length)
                    return $"Invalid line folding at index {i}; CRLF must be followed by a space or tab.";

                if (value[i + 1] != '\n')
                    return $"Invalid line folding at index {i}; CRLF must be followed by a space or tab.";

                var foldingWhitespace = value[i + 2];
                if (foldingWhitespace != ' ' && foldingWhitespace != '\t')
                    return $"Invalid line folding at index {i}; CRLF must be followed by a space or tab.";

                i += 2;
                continue;
            }

            if (current == '\n')
                return $"Invalid character {FormatCharacter(current)} at index {i}; use CRLF followed by a space or tab for folded lines.";

            if ((current < 0x20 && current != '\t') || current > 0x7E)
                return $"Invalid character {FormatCharacter(current)} at index {i}.";
        }

        return null;
    }

    public static void ValidateMetadata(string? name, string? value, string paramName)
    {
        ValidateMetadataName(name, paramName);
        ValidateMetadataValue(value, paramName);
    }

    public static void ValidateMetadataName(string? name, string paramName)
    {
        if (name is null)
            throw new ArgumentNullException(paramName, "The metadata name cannot be null.");

        if (name.Length > MetadataNameMaxLength)
            throw new ArgumentException($"The metadata name must not exceed {MetadataNameMaxLength} characters. Actual length: {name.Length}.", paramName);

        if (!IsValidMetadataName(name.AsSpan()))
            throw new ArgumentException(FormatMetadataNameValidationMessage(name), paramName);
    }

    public static void ValidateMetadataValue(string? value, string paramName)
    {
        if (value is null)
            throw new ArgumentNullException(paramName, "The metadata value cannot be null.");

        if (value.Length > MetadataValueMaxLength)
            throw new ArgumentException($"The metadata value must not exceed {MetadataValueMaxLength} characters. Actual length: {value.Length}.", paramName);
    }

    public static List<KeyValuePair<string, string>> SnapshotValidatedMetadata(IEnumerable<KeyValuePair<string, string>> metadata, string paramName, IReadOnlyDictionary<string, string>? existingMetadata = null)
    {
        if (metadata is null)
            throw new ArgumentNullException(paramName, "The metadata collection cannot be null.");

        var metadataList = metadata.ToList();
        var seen = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < metadataList.Count; index++)
        {
            var entry = metadataList[index];
            if (entry.Key is null)
                throw new ArgumentNullException(paramName, $"The metadata name at index {index} cannot be null.");

            if (entry.Key.Length > MetadataNameMaxLength)
                throw new ArgumentException($"The metadata name at index {index} must not exceed {MetadataNameMaxLength} characters. Actual length: {entry.Key.Length}.", paramName);

            if (!IsValidMetadataName(entry.Key.AsSpan()))
                throw new ArgumentException($"The metadata name at index {index} is invalid. {FormatMetadataNameValidationMessage(entry.Key)}", paramName);

            if (entry.Value is null)
                throw new ArgumentNullException(paramName, $"The metadata value at index {index} cannot be null.");

            if (entry.Value.Length > MetadataValueMaxLength)
                throw new ArgumentException($"The metadata value at index {index} must not exceed {MetadataValueMaxLength} characters. Actual length: {entry.Value.Length}.", paramName);

            if (seen.TryGetValue(entry.Key, out var duplicateIndex))
                throw new ArgumentException($"Metadata names must be unique and are compared case-insensitively. Metadata name '{entry.Key}' at index {index} duplicates index {duplicateIndex}.", paramName);

            if (existingMetadata is not null && TryGetExistingKey(existingMetadata, entry.Key, out var existingMetadataName))
                throw new ArgumentException(FormatDuplicateExistingMetadataMessage(entry.Key, existingMetadataName, index), paramName);

            seen.Add(entry.Key, index);
        }

        var existingCount = existingMetadata?.Count ?? 0;
        var projectedCount = existingCount + metadataList.Count;
        if (projectedCount > 10)
            throw new ArgumentException($"Cannot set more than 10 metadata fields for a message. Adding {metadataList.Count} metadata fields to the existing {existingCount} would produce {projectedCount}.", paramName);

        return metadataList;
    }

    public static bool TryGetExistingKey(IReadOnlyDictionary<string, string> values, string key, out string existingKey)
    {
        foreach (var entry in values)
            if (string.Equals(entry.Key, key, StringComparison.OrdinalIgnoreCase))
            {
                existingKey = entry.Key;
                return true;
            }

        existingKey = string.Empty;
        return false;
    }

    public static string FormatDuplicateExistingHeaderMessage(string name, string existingName, int? index = null)
    {
        var nameWithIndex = index.HasValue ? $"Header name '{name}' at index {index.Value}" : $"Header name '{name}'";
        return $"Header names must be unique and are compared case-insensitively. {nameWithIndex} duplicates existing header name '{existingName}'.";
    }

    public static string FormatDuplicateExistingMetadataMessage(string name, string existingName, int? index = null)
    {
        var nameWithIndex = index.HasValue ? $"Metadata name '{name}' at index {index.Value}" : $"Metadata name '{name}'";
        return $"Metadata names must be unique and are compared case-insensitively. {nameWithIndex} duplicates existing metadata name '{existingName}'.";
    }

    public static (JsonNode Snapshot, int SerializedSizeInBytes) SnapshotTemplateModel(this object templateModel, string paramName, JsonSerializerOptions? serializerOptions = null)
    {
        if (templateModel is null)
            throw new ArgumentNullException(paramName, "The template model cannot be null.");

        try
        {
            var effectiveSerializerOptions = PostKitTemplateModelSerialization.Resolve(serializerOptions);
            var snapshot = templateModel switch
            {
                JsonNode jsonNode => jsonNode.DeepClone(),
                _ => JsonSerializer.SerializeToNode(templateModel, effectiveSerializerOptions)
            };

            if (snapshot is null)
                throw new ArgumentException("The template model must serialize to a non-null JSON value.", paramName);

            if (snapshot is not JsonObject)
                throw new ArgumentException("The template model must serialize to a JSON object.", paramName);

            var serializedSize = JsonSizeEstimator.GetSerializedSize(snapshot, effectiveSerializerOptions);
            return (snapshot, serializedSize);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or InvalidOperationException)
        {
            throw new ArgumentException("The template model could not be serialized to a JSON object. Ensure it does not contain cycles or members unsupported by System.Text.Json.", paramName, ex);
        }
    }

    private static string? ValidateEmailAddress(string? value, string propertyName, string subject, string exampleAddress)
    {
        if (value is null)
            return $"{subject} cannot be null.";

        if (string.IsNullOrWhiteSpace(value))
            return $"{subject} cannot be empty or whitespace. Actual length: {value.Length}.";

        if (!LooksLikePostmarkEmailAddress(value))
            return $"{subject} must be a valid email address. Set {propertyName} to an address like '{exampleAddress}'.";

        return null;
    }

    private static bool LooksLikeDomainName(string value)
    {
        var span = value.AsSpan()
            .Trim();
        if (span.Length < 3 || span[0] == '.' || span[^1] == '.')
            return false;

        var hasDot = false;
        var previousWasDot = false;
        foreach (var ch in span)
        {
            var isLetter = ch is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
            var isDigit = ch is >= '0' and <= '9';
            var isHyphen = ch == '-';
            if (ch == '.')
            {
                if (previousWasDot)
                    return false;

                hasDot = true;
                previousWasDot = true;
                continue;
            }

            if (!isLetter && !isDigit && !isHyphen)
                return false;

            previousWasDot = false;
        }

        return hasDot;
    }

    private static bool LooksLikePostmarkEmailAddress(string value)
    {
        var span = value.AsSpan()
            .Trim();
        var atIndex = span.IndexOf('@');
        if (atIndex <= 0 || atIndex >= span.Length - 1)
            return false;

        if (span[(atIndex + 1)..]
                .IndexOf('.') <= 0)
            return false;

        foreach (var ch in span)
            if (char.IsWhiteSpace(ch) || ch == '<' || ch == '>')
                return false;

        return true;
    }

    private static string FormatEmptyOptionalFilterMessage(string subject, string propertyName, string? value)
    {
        return $"{subject} cannot be empty or whitespace. Set {propertyName} to null to omit this filter. Actual length: {value?.Length ?? 0}.";
    }

    private static string GetTemplateAliasValidationDetail(string value)
    {
        if (value[0] is not (>= 'A' and <= 'Z' or >= 'a' and <= 'z'))
            return $"First character must be a letter. Received {FormatCharacter(value[0])} at index 0.";

        for (var index = 1; index < value.Length; index++)
        {
            var current = value[index];
            var isLetter = current is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
            var isDigit = current is >= '0' and <= '9';
            var isAllowedPunctuation = current is '-' or '_' or '.';
            if (!isLetter && !isDigit && !isAllowedPunctuation)
                return $"Invalid character {FormatCharacter(current)} at index {index}.";
        }

        return "The value is invalid.";
    }

    private static bool LooksLikeEmailAddress(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var span = value.AsSpan()
            .Trim();
        var atIndex = span.IndexOf('@');
        if (atIndex <= 0 || atIndex >= span.Length - 1)
            return false;

        foreach (var ch in span)
            if (char.IsWhiteSpace(ch))
                return false;

        return true;
    }

    private static bool IsValidMetadataName(ReadOnlySpan<char> name)
    {
        return GetMetadataNameValidationDetail(name) is null;
    }

    private static string FormatMetadataNameValidationMessage(string name)
    {
        var detail = GetMetadataNameValidationDetail(name.AsSpan());
        return detail is null ? MetadataNameRuleMessage : $"{MetadataNameRuleMessage} {detail}";
    }

    private static string? GetMetadataNameValidationDetail(ReadOnlySpan<char> name)
    {
        if (name.IsEmpty)
            return "Actual length: 0.";

        if (name.Length > MetadataNameMaxLength)
            return $"Actual length: {name.Length}.";

        if (char.IsWhiteSpace(name[0]))
            return $"Invalid leading whitespace {FormatCharacter(name[0])} at index 0.";

        if (char.IsWhiteSpace(name[^1]))
            return $"Invalid trailing whitespace {FormatCharacter(name[^1])} at index {name.Length - 1}.";

        return null;
    }
}
