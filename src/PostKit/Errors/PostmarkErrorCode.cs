namespace PostKit.Errors;

/// <summary>Represents the error codes and their corresponding descriptions for the Postmark API.</summary>
public enum PostmarkErrorCode
{
    /// <summary>Request was successful.</summary>
    Success = 0,

    /// <summary>Bad or missing API token. Your request did not contain the correct API token in the header. Refer to the request’s API reference page to see which API token is required or learn more about authenticating with Postmark.</summary>
    BadOrMissingApiToken = 10,

    /// <summary>Multiple errors occurred. Inspect the Errors property on the response for more detailed information.</summary>
    MultipleErrorsOccurred = 11,

    /// <summary>Resource not found. The item you are trying to access was not found.</summary>
    ResourceNotFound = 12,

    /// <summary>Invalid pagination key. The pagination key provided in the request was invalid.</summary>
    InvalidPaginationKey = 13,

    /// <summary>Maintenance. The Postmark API is offline for maintenance.</summary>
    Maintenance = 100,

    /// <summary>Invalid email request. Validation failed for the email request JSON data that you provided.</summary>
    InvalidEmailRequest = 300,

    /// <summary>Sender Signature not found. You’re trying to send email with a From address that doesn’t have a sender signature. Refer to your existing list of Sender Signatures or add a new one.</summary>
    SenderSignatureNotFound = 400,

    /// <summary>Sender signature not confirmed. You’re trying to send email with a From address that doesn’t have a confirmed sender signature. You can resend the confirmation email on the Sender Signatures page.</summary>
    SenderSignatureNotConfirmed = 401,

    /// <summary>Invalid JSON. The JSON data you provided is syntactically incorrect. We recommend running your JSON through a validator before issuing another request.</summary>
    InvalidJson = 402,

    /// <summary>Invalid request field(s). The JSON data you provided is invalid in some way. Refer to the request's API reference page to see a list of required JSON body parameters.</summary>
    InvalidRequestFields = 403,

    /// <summary>Not allowed to send. Your account has run out of credits. You can purchase more on the Credits page.</summary>
    NotAllowedToSend = 405,

    /// <summary>Inactive recipient. You tried to send email to a recipient that has been marked as inactive.</summary>
    InactiveRecipient = 406,

    /// <summary>JSON required. Your HTTP request doesn’t have the Accept and Content-Type headers set to application/json.</summary>
    JsonRequired = 409,

    /// <summary>Too many batch messages. Your batched request contains more than 500 messages.</summary>
    TooManyBatchMessages = 410,

    /// <summary>Forbidden attachment type. The file type of the attachment isn’t allowed.</summary>
    ForbiddenAttachmentType = 411,

    /// <summary>Account is Pending. The account that is associated with the send request is still pending approval.</summary>
    AccountIsPending = 412,

    /// <summary>Account May Not Send. The account that is associated with the send request is not approved for sending.</summary>
    AccountMayNotSend = 413,

    /// <summary>Sender signature query exception. You provided invalid querystring parameters in your request.</summary>
    SenderSignatureQueryException = 500,

    /// <summary>Sender Signature not found by ID. We couldn’t locate the Sender Signature you’re trying to manage from the ID passed in.</summary>
    SenderSignatureNotFoundById = 501,

    /// <summary>No updated Sender Signature data received. You didn’t pass in any valid updated Sender Signature data.</summary>
    NoUpdatedSenderSignatureData = 502,

    /// <summary>You cannot use a public domain. You tried to create a Sender Signature with a public domain which isn’t allowed.</summary>
    CannotUsePublicDomain = 503,

    /// <summary>Sender Signature already exists. You tried to create a Sender Signature that already exists on Postmark.</summary>
    SenderSignatureAlreadyExists = 504,

    /// <summary>DKIM already scheduled for renewal. The DKIM you tried to renew is already scheduled to be renewed.</summary>
    DkimAlreadyScheduledForRenewal = 505,

    /// <summary>This Sender Signature already confirmed. The signature you tried to resend a confirmation to has already been confirmed by a user.</summary>
    SenderSignatureAlreadyConfirmed = 506,

    /// <summary>You do not own this Sender Signature. This Sender Signature cannot be found using your credentials.</summary>
    DoNotOwnSenderSignature = 507,

    /// <summary>This domain was not found. We couldn’t locate the Domain you’re trying to manage from the ID passed in.</summary>
    DomainNotFound = 510,

    /// <summary>Invalid fields supplied. You didn’t pass in any valid Domain data.</summary>
    InvalidFieldsSupplied = 511,

    /// <summary>Domain already exists. You tried to create a Domain that already exists on your account.</summary>
    DomainAlreadyExists = 512,

    /// <summary>You do not own this Domain. This Domain cannot be found using your credentials.</summary>
    DoNotOwnDomain = 513,

    /// <summary>Name is a required field to create a Domain. You must set the Name parameter to create a Domain.</summary>
    DomainNameRequired = 514,

    /// <summary>Name field must be less than or equal to 255 characters. The Name you have specified for this Domain is too long.</summary>
    DomainNameTooLong = 515,

    /// <summary>Name format is invalid. The Name you have specified for this Domain is formatted incorrectly.</summary>
    InvalidDomainNameFormat = 516,

    /// <summary>Missing required field for Sender Signature. When creating a Sender Signature, you must supply a value for Name and FromEmail.</summary>
    MissingRequiredSenderSignatureField = 520,

    /// <summary>Sender Signature field is too long. ConfirmationPersonalNote has a max length of 400 characters.</summary>
    SenderSignatureFieldTooLong = 521,

    /// <summary>Invalid field value for Sender Signature. Value might be an invalid email address or domain.</summary>
    InvalidSenderSignatureFieldValue = 522,

    /// <summary>Server query exception. You provided invalid querystring parameters in your request.</summary>
    ServerQueryException = 600,

    /// <summary>Duplicate Inbound Domain. The Inbound Domain you specified is already in use on Postmark.</summary>
    DuplicateInboundDomain = 602,

    /// <summary>Server name already exists. You tried to create a Server name that already exists in your list.</summary>
    ServerNameAlreadyExists = 603,

    /// <summary>You don’t have delete access. You don’t have permission to delete Servers through the API.</summary>
    NoDeleteAccess = 604,

    /// <summary>Unable to delete Server. Please contact support.</summary>
    UnableToDeleteServer = 605,

    /// <summary>Invalid webhook URL. The webhook URL you’re trying to use is invalid or contains an internal IP range.</summary>
    InvalidWebhookUrl = 606,

    /// <summary>Invalid Server color. Please choose from supported server colors.</summary>
    InvalidServerColor = 607,

    /// <summary>Server name missing or invalid. The Server name you provided is invalid or missing.</summary>
    InvalidServerName = 608,

    /// <summary>No updated Server data received. You didn’t pass in any valid updated Server data.</summary>
    NoUpdatedServerData = 609,

    /// <summary>Invalid MX record for Inbound Domain. The Inbound Domain provided doesn’t have an MX record value of inbound.postmarkapp.com.</summary>
    InvalidMxRecordForInboundDomain = 610,

    /// <summary>Invalid InboundSpamThreshold value. Use a number between 0 and 30 in increments of 5.</summary>
    InvalidInboundSpamThreshold = 611,

    /// <summary>Messages query exception. You provided invalid querystring parameters in your request.</summary>
    MessagesQueryException = 700,

    /// <summary>Message doesn’t exist. This message doesn’t exist.</summary>
    MessageDoesNotExist = 701,

    /// <summary>Cannot bypass blocked inbound message. Please contact support.</summary>
    CannotBypassBlockedInboundMessage = 702,

    /// <summary>Cannot retry failed inbound message. Please contact support.</summary>
    CannotRetryFailedInboundMessage = 703,

    /// <summary>Trigger query exception. You provided invalid querystring parameters in your request.</summary>
    TriggerQueryException = 800,

    /// <summary>No trigger data received. You didn’t provide JSON body parameters in your request.</summary>
    NoTriggerDataReceived = 809,

    /// <summary>Stats query exception. You provided invalid querystring parameters in your request.</summary>
    StatsQueryException = 900,

    /// <summary>Bounces query exception. You provided invalid querystring parameters in your request.</summary>
    BouncesQueryException = 1000,

    /// <summary>Bounce was not found. The BounceID or MessageID you are searching with is invalid.</summary>
    BounceNotFound = 1001,

    /// <summary>BounceID parameter required. You must supply a BounceID to get the bounce dump.</summary>
    BounceIdRequired = 1002,

    /// <summary>Cannot activate bounce. Certain bounces and SPAM complaints cannot be activated by the user.</summary>
    CannotActivateBounce = 1003,

    /// <summary>Template query exception. The value of a GET parameter for the request is not valid.</summary>
    TemplateQueryException = 1100,

    /// <summary>Template not found. The TemplateId, LayoutTemplate, or Alias references a Template that does not exist.</summary>
    TemplateNotFound = 1101,

    /// <summary>Template limit would be exceeded. A Server may have up to 100 templates.</summary>
    TemplateLimitExceeded = 1105,

    /// <summary>No Template data received. You didn’t provide JSON body parameters in your request.</summary>
    NoTemplateDataReceived = 1109,

    /// <summary>Missing required Template field. A required field is missing from the body of the POST request.</summary>
    MissingRequiredTemplateField = 1120,

    /// <summary>Template field is too large. One of the values of the request's body exceeds size restrictions.</summary>
    TemplateFieldTooLarge = 1121,

    /// <summary>Invalid Template field. One of the fields of the request body is invalid.</summary>
    InvalidTemplateField = 1122,

    /// <summary>Invalid field in request body. A field is included in the request that is not allowed or ignored.</summary>
    InvalidFieldInRequestBody = 1123,

    /// <summary>Template type mismatch. The template alias is a layout on one server and a standard template on another.</summary>
    TemplateTypeMismatch = 1125,

    /// <summary>Dependent templates exist. The layout cannot be deleted because associated templates use it.</summary>
    DependentTemplatesExist = 1130,

    /// <summary>Invalid layout content placeholder. It must be present exactly once in the layout HTML and/or Text body.</summary>
    InvalidLayoutContentPlaceholder = 1131,

    /// <summary>Invalid MessageStreamType. Ensure interaction with a supported Message Stream type.</summary>
    InvalidMessageStreamType = 1221,

    /// <summary>Invalid Message Stream ID. Ensure the ID is valid and exists.</summary>
    InvalidMessageStreamId = 1222,

    /// <summary>Invalid Message Stream Name. Ensure the Name is valid and not empty.</summary>
    InvalidMessageStreamName = 1223,

    /// <summary>Message Stream Name too long. Limit the Name to 100 characters.</summary>
    MessageStreamNameTooLong = 1224,

    /// <summary>Max Message Streams reached. Contact support if more are needed.</summary>
    MaxMessageStreamsReached = 1225,

    /// <summary>Message Stream not found. Ensure the provided ID exists.</summary>
    MessageStreamNotFound = 1226,

    /// <summary>Invalid Message Stream ID format. IDs must start with a letter and follow allowed format rules.</summary>
    InvalidMessageStreamIdFormat = 1227,

    /// <summary>Only one inbound stream allowed per server. Create a new server if more are needed.</summary>
    MaxInboundStreamsPerServer = 1228,

    /// <summary>Cannot archive default Message Streams. Transactional and Inbound streams cannot be deleted.</summary>
    CannotArchiveDefaultStreams = 1229,

    /// <summary>Duplicate Message Stream ID. IDs must be unique per server.</summary>
    DuplicateMessageStreamId = 1230,

    /// <summary>Message Stream Description too long. Limit it to 1000 characters.</summary>
    MessageStreamDescriptionTooLong = 1231,

    /// <summary>Cannot unarchive Message Stream. It may be deleted or no longer archived.</summary>
    CannotUnarchiveMessageStream = 1232,

    /// <summary>Invalid Message Stream ID prefix. IDs cannot start with 'pm-'.</summary>
    InvalidMessageStreamIdPrefix = 1233,

    /// <summary>Invalid Message Stream Description. It must not contain HTML tags.</summary>
    InvalidMessageStreamDescription = 1234,

    /// <summary>Message Stream not found on this server. Ensure the provided ID exists.</summary>
    MessageStreamNotFoundOnServer = 1235,

    /// <summary>Sending not supported on this Message Stream. Only supported for certain types.</summary>
    SendingNotSupportedOnMessageStream = 1236,

    /// <summary>Reserved Message Stream ID. Certain IDs like 'all' cannot be used.</summary>
    ReservedMessageStreamId = 1237,

    /// <summary>Invalid Data Removal request. Ensure the parameters match the Data Removal API reference.</summary>
    InvalidDataRemovalRequest = 1300,

    /// <summary>Invalid Data Removal ID. Ensure the ID is correct and exists.</summary>
    InvalidDataRemovalId = 1301,

    /// <summary>No Data Removal access. You lack permission for these requests.</summary>
    NoDataRemovalAccess = 1302,
}
