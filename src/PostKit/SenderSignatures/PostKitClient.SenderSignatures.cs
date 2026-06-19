using System.Globalization;
using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.Common;
using PostKit.Errors;
using PostKit.Postmark;
using PostKit.SenderSignatures;
using SenderSignatureActionModel = PostKit.Postmark.SenderSignatures.SenderSignatureActionResponse;
using SenderSignatureCreateRequestModel = PostKit.Postmark.SenderSignatures.SenderSignatureCreateRequest;
using SenderSignatureEditRequestModel = PostKit.Postmark.SenderSignatures.SenderSignatureEditRequest;
using SenderSignatureListModel = PostKit.Postmark.SenderSignatures.SenderSignatureListResponse;
using SenderSignatureModel = PostKit.Postmark.SenderSignatures.SenderSignatureResponse;
using SenderSignatureSummaryModel = PostKit.Postmark.SenderSignatures.SenderSignatureSummaryResponse;

// ReSharper disable once CheckNamespace
namespace PostKit;

internal sealed partial class PostKitClient
{
    private const int SenderSignatureConfirmationPersonalNoteMaxLength = 400;

    public async Task<Result<SenderSignaturePage>> ListSenderSignaturesAsync(int count = 100, int offset = 0, CancellationToken cancellationToken = default)
    {
        var validationError = ValidationExtensions.ValidateListRequest("The sender signature list", count, offset);
        if (validationError is not null)
            return Result.Failure<SenderSignaturePage>(validationError);

        Result<SenderSignatureListModel> response;
        try
        {
            response = await postmark.GetAsync<SenderSignatureListModel>(PostmarkTokenScope.Account, $"/senders?count={count.ToString(CultureInfo.InvariantCulture)}&offset={offset.ToString(CultureInfo.InvariantCulture)}",
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogListSenderSignaturesException(ex);
            return Result.Failure<SenderSignaturePage>(ex);
        }

        if (response.IsFailure(out var error, out var listModel))
        {
            LogListSenderSignaturesError(error.Message, error);
            return Result.Failure<SenderSignaturePage>(error);
        }

        var mapped = CreateSenderSignaturePage(listModel);
        if (mapped.IsFailure(out var mappingError, out var page))
        {
            LogListSenderSignaturesError(mappingError.Message, mappingError);
            return Result.Failure<SenderSignaturePage>(mappingError);
        }

        return Result.Success(page);
    }

    public async Task<Result<SenderSignature>> GetSenderSignatureAsync(long signatureId, CancellationToken cancellationToken = default)
    {
        var validationError = ValidationExtensions.ValidateId(signatureId, "The sender signature ID");
        if (validationError is not null)
            return Result.Failure<SenderSignature>(validationError);

        Result<SenderSignatureModel> response;
        try
        {
            response = await postmark.GetAsync<SenderSignatureModel>(PostmarkTokenScope.Account, $"/senders/{signatureId.ToString(CultureInfo.InvariantCulture)}", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogGetSenderSignatureException(ex);
            return Result.Failure<SenderSignature>(ex);
        }

        if (response.IsFailure(out var error, out var signatureModel))
        {
            LogGetSenderSignatureError(error.Message, error);
            return Result.Failure<SenderSignature>(error);
        }

        var mapped = CreateSenderSignature(signatureModel, "sender signature response");
        if (mapped.IsFailure(out var mappingError, out var signature))
        {
            LogGetSenderSignatureError(mappingError.Message, mappingError);
            return Result.Failure<SenderSignature>(mappingError);
        }

        return Result.Success(signature);
    }

    public async Task<Result<SenderSignature>> CreateSenderSignatureAsync(SenderSignatureCreateParameters parameters, CancellationToken cancellationToken = default)
    {
        if (parameters is null)
            throw new ArgumentNullException(nameof(parameters), "The sender signature create parameters cannot be null.");

        var mappedRequest = CreateSenderSignatureRequest(parameters);
        if (mappedRequest.IsFailure(out var requestError, out var requestModel))
            return Result.Failure<SenderSignature>(requestError);

        Result<SenderSignatureModel> response;
        try
        {
            response = await postmark.PostAsync<SenderSignatureCreateRequestModel, SenderSignatureModel>(PostmarkTokenScope.Account, "/senders", requestModel, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogCreateSenderSignatureException(ex);
            return Result.Failure<SenderSignature>(ex);
        }

        if (response.IsFailure(out var error, out var signatureModel))
        {
            LogCreateSenderSignatureError(error.Message, error);
            return Result.Failure<SenderSignature>(error);
        }

        var mapped = CreateSenderSignature(signatureModel, "sender signature create response");
        if (mapped.IsFailure(out var mappingError, out var signature))
        {
            LogCreateSenderSignatureError(mappingError.Message, mappingError);
            return Result.Failure<SenderSignature>(mappingError);
        }

        return Result.Success(signature);
    }

    public async Task<Result<SenderSignature>> EditSenderSignatureAsync(long signatureId, SenderSignatureEditParameters parameters, CancellationToken cancellationToken = default)
    {
        if (parameters is null)
            throw new ArgumentNullException(nameof(parameters), "The sender signature edit parameters cannot be null.");

        var validationError = ValidationExtensions.ValidateId(signatureId, "The sender signature ID");
        if (validationError is not null)
            return Result.Failure<SenderSignature>(validationError);

        var mappedRequest = CreateSenderSignatureRequest(parameters);
        if (mappedRequest.IsFailure(out var requestError, out var requestModel))
            return Result.Failure<SenderSignature>(requestError);

        Result<SenderSignatureModel> response;
        try
        {
            response = await postmark.PutAsync<SenderSignatureEditRequestModel, SenderSignatureModel>(PostmarkTokenScope.Account, $"/senders/{signatureId.ToString(CultureInfo.InvariantCulture)}", requestModel, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogEditSenderSignatureException(ex);
            return Result.Failure<SenderSignature>(ex);
        }

        if (response.IsFailure(out var error, out var signatureModel))
        {
            LogEditSenderSignatureError(error.Message, error);
            return Result.Failure<SenderSignature>(error);
        }

        var mapped = CreateSenderSignature(signatureModel, "sender signature edit response");
        if (mapped.IsFailure(out var mappingError, out var signature))
        {
            LogEditSenderSignatureError(mappingError.Message, mappingError);
            return Result.Failure<SenderSignature>(mappingError);
        }

        return Result.Success(signature);
    }

    public async Task<Result<SenderSignatureDeletion>> DeleteSenderSignatureAsync(long signatureId, CancellationToken cancellationToken = default)
    {
        var validationError = ValidationExtensions.ValidateId(signatureId, "The sender signature ID");
        if (validationError is not null)
            return Result.Failure<SenderSignatureDeletion>(validationError);

        Result<SenderSignatureActionModel> response;
        try
        {
            response = await postmark.DeleteAsync<SenderSignatureActionModel>(PostmarkTokenScope.Account, $"/senders/{signatureId.ToString(CultureInfo.InvariantCulture)}", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogDeleteSenderSignatureException(ex);
            return Result.Failure<SenderSignatureDeletion>(ex);
        }

        if (response.IsFailure(out var error, out var deletionModel))
        {
            LogDeleteSenderSignatureError(error.Message, error);
            return Result.Failure<SenderSignatureDeletion>(error);
        }

        var mapped = CreateSenderSignatureAction(deletionModel);
        if (mapped.IsFailure(out var mappingError, out var deletion))
        {
            LogDeleteSenderSignatureError(mappingError.Message, mappingError);
            return Result.Failure<SenderSignatureDeletion>(mappingError);
        }

        return Result.Success(new SenderSignatureDeletion { Message = deletion.Message });
    }

    public Task<Result<SenderSignatureAction>> ResendSenderSignatureConfirmationAsync(long signatureId, CancellationToken cancellationToken = default)
    {
        return RunSenderSignatureActionAsync(signatureId, $"/senders/{signatureId.ToString(CultureInfo.InvariantCulture)}/resend", LogResendSenderSignatureConfirmationException, LogResendSenderSignatureConfirmationError, cancellationToken);
    }

    public async Task<Result<SenderSignature>> VerifySenderSignatureSpfAsync(long signatureId, CancellationToken cancellationToken = default)
    {
        var validationError = ValidationExtensions.ValidateId(signatureId, "The sender signature ID");
        if (validationError is not null)
            return Result.Failure<SenderSignature>(validationError);

        Result<SenderSignatureModel> response;
        try
        {
            response = await postmark.PostAsync<SenderSignatureModel>(PostmarkTokenScope.Account, $"/senders/{signatureId.ToString(CultureInfo.InvariantCulture)}/verifyspf", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogVerifySenderSignatureSpfException(ex);
            return Result.Failure<SenderSignature>(ex);
        }

        if (response.IsFailure(out var error, out var signatureModel))
        {
            LogVerifySenderSignatureSpfError(error.Message, error);
            return Result.Failure<SenderSignature>(error);
        }

        var mapped = CreateSenderSignature(signatureModel, "sender signature SPF verification response");
        if (mapped.IsFailure(out var mappingError, out var signature))
        {
            LogVerifySenderSignatureSpfError(mappingError.Message, mappingError);
            return Result.Failure<SenderSignature>(mappingError);
        }

        return Result.Success(signature);
    }

    public Task<Result<SenderSignatureAction>> RequestNewDkimForSenderSignatureAsync(long signatureId, CancellationToken cancellationToken = default)
    {
        return RunSenderSignatureActionAsync(signatureId, $"/senders/{signatureId.ToString(CultureInfo.InvariantCulture)}/requestnewdkim", LogRequestNewDkimForSenderSignatureException, LogRequestNewDkimForSenderSignatureError,
            cancellationToken);
    }

    private async Task<Result<SenderSignatureAction>> RunSenderSignatureActionAsync(long signatureId, string endpoint, Action<Exception> logException, Action<string, IError> logError, CancellationToken cancellationToken)
    {
        var validationError = ValidationExtensions.ValidateId(signatureId, "The sender signature ID");
        if (validationError is not null)
            return Result.Failure<SenderSignatureAction>(validationError);

        Result<SenderSignatureActionModel> response;
        try
        {
            response = await postmark.PostAsync<SenderSignatureActionModel>(PostmarkTokenScope.Account, endpoint, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logException(ex);
            return Result.Failure<SenderSignatureAction>(ex);
        }

        if (response.IsFailure(out var error, out var actionModel))
        {
            logError(error.Message, error);
            return Result.Failure<SenderSignatureAction>(error);
        }

        var mapped = CreateSenderSignatureAction(actionModel);
        if (mapped.IsFailure(out var mappingError, out var action))
        {
            logError(mappingError.Message, mappingError);
            return Result.Failure<SenderSignatureAction>(mappingError);
        }

        return Result.Success(action);
    }

    private static Result<SenderSignatureCreateRequestModel> CreateSenderSignatureRequest(SenderSignatureCreateParameters parameters)
    {
        var validationError = ValidationExtensions.ValidateEmailAddress(parameters.FromEmail, nameof(SenderSignatureCreateParameters.FromEmail), "The sender signature create parameters from email address");
        if (validationError is not null)
            return Result.Failure<SenderSignatureCreateRequestModel>(validationError);

        validationError = ValidationExtensions.ValidateRequiredText(parameters.Name, "The sender signature create parameters name");
        if (validationError is not null)
            return Result.Failure<SenderSignatureCreateRequestModel>(validationError);

        validationError = ValidationExtensions.ValidateOptionalEmailAddress(parameters.ReplyToEmailAddress, nameof(SenderSignatureCreateParameters.ReplyToEmailAddress), "The sender signature create parameters reply-to email address");
        if (validationError is not null)
            return Result.Failure<SenderSignatureCreateRequestModel>(validationError);

        validationError = ValidationExtensions.ValidateDomainName(parameters.ReturnPathDomain, "The sender signature create parameters return-path domain", true, true);
        if (validationError is not null)
            return Result.Failure<SenderSignatureCreateRequestModel>(validationError);

        validationError = ValidateSenderSignatureConfirmationPersonalNote(parameters.ConfirmationPersonalNote, "The sender signature create parameters confirmation personal note");

        if (validationError is not null)
            return Result.Failure<SenderSignatureCreateRequestModel>(validationError);

        return Result.Success(new SenderSignatureCreateRequestModel
        {
            FromEmail = parameters.FromEmail,
            Name = parameters.Name,
            ReplyToEmailAddress = parameters.ReplyToEmailAddress,
            ReturnPathDomain = parameters.ReturnPathDomain,
            ConfirmationPersonalNote = parameters.ConfirmationPersonalNote
        });
    }

    private static Result<SenderSignatureEditRequestModel> CreateSenderSignatureRequest(SenderSignatureEditParameters parameters)
    {
        var validationError = ValidationExtensions.ValidateRequiredText(parameters.Name, "The sender signature edit parameters name");
        if (validationError is not null)
            return Result.Failure<SenderSignatureEditRequestModel>(validationError);

        validationError = ValidationExtensions.ValidateOptionalEmailAddress(parameters.ReplyToEmailAddress, nameof(SenderSignatureEditParameters.ReplyToEmailAddress), "The sender signature edit parameters reply-to email address");
        if (validationError is not null)
            return Result.Failure<SenderSignatureEditRequestModel>(validationError);

        validationError = ValidationExtensions.ValidateDomainName(parameters.ReturnPathDomain, "The sender signature edit parameters return-path domain", true, true);
        if (validationError is not null)
            return Result.Failure<SenderSignatureEditRequestModel>(validationError);

        validationError = ValidateSenderSignatureConfirmationPersonalNote(parameters.ConfirmationPersonalNote, "The sender signature edit parameters confirmation personal note");

        if (validationError is not null)
            return Result.Failure<SenderSignatureEditRequestModel>(validationError);

        return Result.Success(new SenderSignatureEditRequestModel
        {
            Name = parameters.Name,
            ReplyToEmailAddress = parameters.ReplyToEmailAddress,
            ReturnPathDomain = parameters.ReturnPathDomain,
            ConfirmationPersonalNote = parameters.ConfirmationPersonalNote
        });
    }

    private static string? ValidateSenderSignatureConfirmationPersonalNote(string? value, string subject)
    {
        var validationError = ValidationExtensions.ValidateOptionalText(value, subject);
        if (validationError is not null)
            return validationError;

        var length = value.AsSpan()
            .GetPostmarkCharacterCount();

        return length > SenderSignatureConfirmationPersonalNoteMaxLength ? $"{subject} must not exceed {SenderSignatureConfirmationPersonalNoteMaxLength} characters. Actual length: {length}." : null;
    }

    private static Result<SenderSignaturePage> CreateSenderSignaturePage(SenderSignatureListModel response)
    {
        if (response.TotalCount is null)
            return Result.Failure<SenderSignaturePage>("TotalCount was not returned from the Postmark Sender Signatures API.");

        if (response.TotalCount.Value < 0)
            return Result.Failure<SenderSignaturePage>($"TotalCount returned from the Postmark Sender Signatures API was invalid. Received {response.TotalCount.Value}.");

        if (response.SenderSignatures is null)
            return Result.Failure<SenderSignaturePage>("SenderSignatures were not returned from the Postmark Sender Signatures API.");

        var signatures = new List<SenderSignatureSummary>(response.SenderSignatures.Count);
        for (var index = 0; index < response.SenderSignatures.Count; index++)
        {
            var mapped = CreateSenderSignatureSummary(response.SenderSignatures[index], $"sender signature list item {index}");
            if (mapped.IsFailure(out var error, out var signature))
                return Result.Failure<SenderSignaturePage>(error);

            signatures.Add(signature);
        }

        return Result.Success(new SenderSignaturePage { TotalCount = response.TotalCount.Value, SenderSignatures = signatures });
    }

    private static Result<SenderSignatureSummary> CreateSenderSignatureSummary(SenderSignatureSummaryModel? response, string context)
    {
        if (response is null)
            return Result.Failure<SenderSignatureSummary>($"{context} returned from the Postmark Sender Signatures API was null.");

        var core = ValidationExtensions.ValidateSenderSignatureCore(response, context);
        if (core.IsFailure(out var coreError))
            return Result.Failure<SenderSignatureSummary>(coreError);

        return Result.Success(new SenderSignatureSummary(response.Id!.Value, response.Domain!, response.EmailAddress!, ValidationExtensions.NormalizeOptionalString(response.ReplyToEmailAddress), response.Name!, response.Confirmed!.Value));
    }

    private static Result<SenderSignature> CreateSenderSignature(SenderSignatureModel? response, string context)
    {
        if (response is null)
            return Result.Failure<SenderSignature>($"{context} returned from the Postmark Sender Signatures API was null.");

        var core = ValidationExtensions.ValidateSenderSignatureCore(response, context);
        if (core.IsFailure(out var coreError))
            return Result.Failure<SenderSignature>(coreError);

        if (response.SpfVerified is null)
            return Result.Failure<SenderSignature>($"SPFVerified was not returned from the Postmark Sender Signatures API for {context}.");

        if (response.DkimVerified is null)
            return Result.Failure<SenderSignature>($"DKIMVerified was not returned from the Postmark Sender Signatures API for {context}.");

        if (response.WeakDkim is null)
            return Result.Failure<SenderSignature>($"WeakDKIM was not returned from the Postmark Sender Signatures API for {context}.");

        if (response.ReturnPathDomainVerified is null)
            return Result.Failure<SenderSignature>($"ReturnPathDomainVerified was not returned from the Postmark Sender Signatures API for {context}.");

        return Result.Success(new SenderSignature(response.Id!.Value, response.Domain!, response.EmailAddress!, ValidationExtensions.NormalizeOptionalString(response.ReplyToEmailAddress), response.Name!, response.Confirmed!.Value,
            response.SpfVerified.Value, ValidationExtensions.NormalizeOptionalString(response.SpfHost), ValidationExtensions.NormalizeOptionalString(response.SpfTextValue), response.DkimVerified.Value, response.WeakDkim.Value,
            ValidationExtensions.NormalizeOptionalString(response.DkimHost), ValidationExtensions.NormalizeOptionalString(response.DkimTextValue), ValidationExtensions.NormalizeOptionalString(response.DkimPendingHost),
            ValidationExtensions.NormalizeOptionalString(response.DkimPendingTextValue), ValidationExtensions.NormalizeOptionalString(response.DkimRevokedHost), ValidationExtensions.NormalizeOptionalString(response.DkimRevokedTextValue),
            response.SafeToRemoveRevokedKeyFromDns, ValidationExtensions.NormalizeOptionalString(response.DkimUpdateStatus), ValidationExtensions.NormalizeOptionalString(response.ReturnPathDomain), response.ReturnPathDomainVerified.Value,
            ValidationExtensions.NormalizeOptionalString(response.ReturnPathDomainCNameValue), ValidationExtensions.NormalizeOptionalString(response.ConfirmationPersonalNote)));
    }

    private static Result<SenderSignatureAction> CreateSenderSignatureAction(SenderSignatureActionModel response)
    {
        if (response.ErrorCode is null)
            return Result.Failure<SenderSignatureAction>("ErrorCode was not returned from the Postmark Sender Signatures API.");

        if (string.IsNullOrWhiteSpace(response.Message))
            return Result.Failure<SenderSignatureAction>("Message was not returned from the Postmark Sender Signatures API.");

        if (response.ErrorCode.Value != 0)
            return Result.Failure<SenderSignatureAction>(new PostmarkError(response.ErrorCode.Value, response.Message));

        return Result.Success(new SenderSignatureAction { Message = response.Message });
    }

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to list sender signatures.")]
    private partial void LogListSenderSignaturesException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to list sender signatures. {Message}")]
    private partial void LogListSenderSignaturesError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to get the sender signature.")]
    private partial void LogGetSenderSignatureException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to get the sender signature. {Message}")]
    private partial void LogGetSenderSignatureError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to create the sender signature.")]
    private partial void LogCreateSenderSignatureException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to create the sender signature. {Message}")]
    private partial void LogCreateSenderSignatureError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to edit the sender signature.")]
    private partial void LogEditSenderSignatureException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to edit the sender signature. {Message}")]
    private partial void LogEditSenderSignatureError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to delete the sender signature.")]
    private partial void LogDeleteSenderSignatureException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to delete the sender signature. {Message}")]
    private partial void LogDeleteSenderSignatureError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to resend sender signature confirmation.")]
    private partial void LogResendSenderSignatureConfirmationException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to resend sender signature confirmation. {Message}")]
    private partial void LogResendSenderSignatureConfirmationError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to verify sender signature SPF.")]
    private partial void LogVerifySenderSignatureSpfException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to verify sender signature SPF. {Message}")]
    private partial void LogVerifySenderSignatureSpfError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to request new sender signature DKIM.")]
    private partial void LogRequestNewDkimForSenderSignatureException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to request new sender signature DKIM. {Message}")]
    private partial void LogRequestNewDkimForSenderSignatureError(string message, [LogProperties] IError error);
}
