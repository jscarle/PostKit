using System.Globalization;
using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.Common;
using PostKit.Domains;
using PostKit.Errors;
using PostKit.Postmark;
using DomainCreateRequestModel = PostKit.Postmark.Domains.DomainCreateRequest;
using DomainDeletionModel = PostKit.Postmark.Domains.DomainDeletionResponse;
using DomainEditRequestModel = PostKit.Postmark.Domains.DomainEditRequest;
using DomainListModel = PostKit.Postmark.Domains.DomainListResponse;
using DomainResponseModel = PostKit.Postmark.Domains.DomainResponse;
using DomainSpfVerificationModel = PostKit.Postmark.Domains.DomainSpfVerificationResponse;

// ReSharper disable once CheckNamespace
namespace PostKit;

internal sealed partial class PostKitClient
{
    public async Task<Result<DomainPage>> ListDomainsAsync(int count = 100, int offset = 0, CancellationToken cancellationToken = default)
    {
        var validationError = ValidationExtensions.ValidateListRequest("The domain list", count, offset);
        if (validationError is not null)
            return Result.Failure<DomainPage>(validationError);

        Result<DomainListModel> response;
        try
        {
            response = await postmark.GetAsync<DomainListModel>(PostmarkTokenScope.Account, $"/domains?count={count.ToString(CultureInfo.InvariantCulture)}&offset={offset.ToString(CultureInfo.InvariantCulture)}", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogListDomainsException(ex);
            return Result.Failure<DomainPage>(ex);
        }

        if (response.IsFailure(out var error, out var listModel))
        {
            LogListDomainsError(error.Message, error);
            return Result.Failure<DomainPage>(error);
        }

        var mapped = CreateDomainPage(listModel);
        if (mapped.IsFailure(out var mappingError, out var page))
        {
            LogListDomainsError(mappingError.Message, mappingError);
            return Result.Failure<DomainPage>(mappingError);
        }

        return Result.Success(page);
    }

    public async Task<Result<PostmarkDomain>> GetDomainAsync(long domainId, CancellationToken cancellationToken = default)
    {
        var validationError = ValidationExtensions.ValidateId(domainId, "The domain ID");
        if (validationError is not null)
            return Result.Failure<PostmarkDomain>(validationError);

        Result<DomainResponseModel> response;
        try
        {
            response = await postmark.GetAsync<DomainResponseModel>(PostmarkTokenScope.Account, $"/domains/{domainId.ToString(CultureInfo.InvariantCulture)}", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogGetDomainException(ex);
            return Result.Failure<PostmarkDomain>(ex);
        }

        if (response.IsFailure(out var error, out var domainModel))
        {
            LogGetDomainError(error.Message, error);
            return Result.Failure<PostmarkDomain>(error);
        }

        var mapped = CreatePostmarkDomain(domainModel, "domain response");
        if (mapped.IsFailure(out var mappingError, out var domain))
        {
            LogGetDomainError(mappingError.Message, mappingError);
            return Result.Failure<PostmarkDomain>(mappingError);
        }

        return Result.Success(domain);
    }

    public async Task<Result<PostmarkDomain>> CreateDomainAsync(DomainCreateParameters parameters, CancellationToken cancellationToken = default)
    {
        if (parameters is null)
            throw new ArgumentNullException(nameof(parameters), "The domain create parameters cannot be null.");

        var mappedRequest = CreateDomainRequest(parameters);
        if (mappedRequest.IsFailure(out var requestError, out var requestModel))
            return Result.Failure<PostmarkDomain>(requestError);

        Result<DomainResponseModel> response;
        try
        {
            response = await postmark.PostAsync<DomainCreateRequestModel, DomainResponseModel>(PostmarkTokenScope.Account, "/domains", requestModel, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogCreateDomainException(ex);
            return Result.Failure<PostmarkDomain>(ex);
        }

        if (response.IsFailure(out var error, out var domainModel))
        {
            LogCreateDomainError(error.Message, error);
            return Result.Failure<PostmarkDomain>(error);
        }

        var mapped = CreatePostmarkDomain(domainModel, "domain create response");
        if (mapped.IsFailure(out var mappingError, out var domain))
        {
            LogCreateDomainError(mappingError.Message, mappingError);
            return Result.Failure<PostmarkDomain>(mappingError);
        }

        return Result.Success(domain);
    }

    public async Task<Result<PostmarkDomain>> EditDomainAsync(long domainId, DomainEditParameters parameters, CancellationToken cancellationToken = default)
    {
        if (parameters is null)
            throw new ArgumentNullException(nameof(parameters), "The domain edit parameters cannot be null.");

        var validationError = ValidationExtensions.ValidateId(domainId, "The domain ID");
        if (validationError is not null)
            return Result.Failure<PostmarkDomain>(validationError);

        var mappedRequest = CreateDomainRequest(parameters);
        if (mappedRequest.IsFailure(out var requestError, out var requestModel))
            return Result.Failure<PostmarkDomain>(requestError);

        Result<DomainResponseModel> response;
        try
        {
            response = await postmark.PutAsync<DomainEditRequestModel, DomainResponseModel>(PostmarkTokenScope.Account, $"/domains/{domainId.ToString(CultureInfo.InvariantCulture)}", requestModel, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogEditDomainException(ex);
            return Result.Failure<PostmarkDomain>(ex);
        }

        if (response.IsFailure(out var error, out var domainModel))
        {
            LogEditDomainError(error.Message, error);
            return Result.Failure<PostmarkDomain>(error);
        }

        var mapped = CreatePostmarkDomain(domainModel, "domain edit response");
        if (mapped.IsFailure(out var mappingError, out var domain))
        {
            LogEditDomainError(mappingError.Message, mappingError);
            return Result.Failure<PostmarkDomain>(mappingError);
        }

        return Result.Success(domain);
    }

    public async Task<Result<DomainDeletion>> DeleteDomainAsync(long domainId, CancellationToken cancellationToken = default)
    {
        var validationError = ValidationExtensions.ValidateId(domainId, "The domain ID");
        if (validationError is not null)
            return Result.Failure<DomainDeletion>(validationError);

        Result<DomainDeletionModel> response;
        try
        {
            response = await postmark.DeleteAsync<DomainDeletionModel>(PostmarkTokenScope.Account, $"/domains/{domainId.ToString(CultureInfo.InvariantCulture)}", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogDeleteDomainException(ex);
            return Result.Failure<DomainDeletion>(ex);
        }

        if (response.IsFailure(out var error, out var deletionModel))
        {
            LogDeleteDomainError(error.Message, error);
            return Result.Failure<DomainDeletion>(error);
        }

        var mapped = CreateDomainDeletion(deletionModel);
        if (mapped.IsFailure(out var mappingError, out var deletion))
        {
            LogDeleteDomainError(mappingError.Message, mappingError);
            return Result.Failure<DomainDeletion>(mappingError);
        }

        return Result.Success(deletion);
    }

    public Task<Result<PostmarkDomain>> VerifyDomainDkimAsync(long domainId, CancellationToken cancellationToken = default)
    {
        return RunDomainActionAsync(domainId, "verify DKIM", $"/domains/{domainId.ToString(CultureInfo.InvariantCulture)}/verifyDkim", (endpoint, token) => postmark.PutAsync<DomainResponseModel>(PostmarkTokenScope.Account, endpoint, token),
            LogVerifyDomainDkimException, LogVerifyDomainDkimError, cancellationToken);
    }

    public Task<Result<PostmarkDomain>> VerifyDomainReturnPathAsync(long domainId, CancellationToken cancellationToken = default)
    {
        return RunDomainActionAsync(domainId, "verify return path", $"/domains/{domainId.ToString(CultureInfo.InvariantCulture)}/verifyReturnPath",
            (endpoint, token) => postmark.PutAsync<DomainResponseModel>(PostmarkTokenScope.Account, endpoint, token), LogVerifyDomainReturnPathException, LogVerifyDomainReturnPathError, cancellationToken);
    }

    public async Task<Result<DomainSpfVerification>> VerifyDomainSpfAsync(long domainId, CancellationToken cancellationToken = default)
    {
        var validationError = ValidationExtensions.ValidateId(domainId, "The domain ID");
        if (validationError is not null)
            return Result.Failure<DomainSpfVerification>(validationError);

        Result<DomainSpfVerificationModel> response;
        try
        {
            response = await postmark.PostAsync<DomainSpfVerificationModel>(PostmarkTokenScope.Account, $"/domains/{domainId.ToString(CultureInfo.InvariantCulture)}/verifyspf", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogVerifyDomainSpfException(ex);
            return Result.Failure<DomainSpfVerification>(ex);
        }

        if (response.IsFailure(out var error, out var spfModel))
        {
            LogVerifyDomainSpfError(error.Message, error);
            return Result.Failure<DomainSpfVerification>(error);
        }

        var mapped = CreateDomainSpfVerification(spfModel);
        if (mapped.IsFailure(out var mappingError, out var verification))
        {
            LogVerifyDomainSpfError(mappingError.Message, mappingError);
            return Result.Failure<DomainSpfVerification>(mappingError);
        }

        return Result.Success(verification);
    }

    public async Task<Result<DomainDkimRotation>> RotateDomainDkimAsync(long domainId, CancellationToken cancellationToken = default)
    {
        var validationError = ValidationExtensions.ValidateId(domainId, "The domain ID");
        if (validationError is not null)
            return Result.Failure<DomainDkimRotation>(validationError);

        Result<DomainResponseModel> response;
        try
        {
            response = await postmark.PostAsync<DomainResponseModel>(PostmarkTokenScope.Account, $"/domains/{domainId.ToString(CultureInfo.InvariantCulture)}/rotatedkim", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogRotateDomainDkimException(ex);
            return Result.Failure<DomainDkimRotation>(ex);
        }

        if (response.IsFailure(out var error, out var domainModel))
        {
            LogRotateDomainDkimError(error.Message, error);
            return Result.Failure<DomainDkimRotation>(error);
        }

        var mapped = CreateDomainDkimRotation(domainModel);
        if (mapped.IsFailure(out var mappingError, out var rotation))
        {
            LogRotateDomainDkimError(mappingError.Message, mappingError);
            return Result.Failure<DomainDkimRotation>(mappingError);
        }

        return Result.Success(rotation);
    }

    private static async Task<Result<PostmarkDomain>> RunDomainActionAsync(long domainId, string actionName, string endpoint, Func<string, CancellationToken, Task<Result<DomainResponseModel>>> send, Action<Exception> logException,
        Action<string, IError> logError, CancellationToken cancellationToken)
    {
        var validationError = ValidationExtensions.ValidateId(domainId, "The domain ID");
        if (validationError is not null)
            return Result.Failure<PostmarkDomain>(validationError);

        Result<DomainResponseModel> response;
        try
        {
            response = await send(endpoint, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logException(ex);
            return Result.Failure<PostmarkDomain>(ex);
        }

        if (response.IsFailure(out var error, out var domainModel))
        {
            logError(error.Message, error);
            return Result.Failure<PostmarkDomain>(error);
        }

        var mapped = CreatePostmarkDomain(domainModel, $"domain {actionName} response");
        if (mapped.IsFailure(out var mappingError, out var domain))
        {
            logError(mappingError.Message, mappingError);
            return Result.Failure<PostmarkDomain>(mappingError);
        }

        return Result.Success(domain);
    }

    private static Result<DomainCreateRequestModel> CreateDomainRequest(DomainCreateParameters parameters)
    {
        var validationError = ValidationExtensions.ValidateDomainName(parameters.Name, "The domain create parameters name", false, false) ??
                              ValidationExtensions.ValidateDomainName(parameters.ReturnPathDomain, "The domain create parameters return-path domain", true, true);

        if (validationError is not null)
            return Result.Failure<DomainCreateRequestModel>(validationError);

        return Result.Success(new DomainCreateRequestModel { Name = parameters.Name, ReturnPathDomain = parameters.ReturnPathDomain });
    }

    private static Result<DomainEditRequestModel> CreateDomainRequest(DomainEditParameters parameters)
    {
        var validationError = ValidationExtensions.ValidateDomainName(parameters.ReturnPathDomain, "The domain edit parameters return-path domain", false, true);
        if (validationError is not null)
            return Result.Failure<DomainEditRequestModel>(validationError);

        return Result.Success(new DomainEditRequestModel { ReturnPathDomain = parameters.ReturnPathDomain });
    }

    private static Result<DomainPage> CreateDomainPage(DomainListModel response)
    {
        if (response.TotalCount is null)
            return Result.Failure<DomainPage>("TotalCount was not returned from the Postmark Domains API.");

        if (response.TotalCount.Value < 0)
            return Result.Failure<DomainPage>($"TotalCount returned from the Postmark Domains API was invalid. Received {response.TotalCount.Value}.");

        if (response.Domains is null)
            return Result.Failure<DomainPage>("Domains were not returned from the Postmark Domains API.");

        var domains = new List<PostmarkDomain>(response.Domains.Count);
        for (var index = 0; index < response.Domains.Count; index++)
        {
            var mapped = CreatePostmarkDomain(response.Domains[index], $"domain list item {index}");
            if (mapped.IsFailure(out var error, out var domain))
                return Result.Failure<DomainPage>(error);

            domains.Add(domain);
        }

        return Result.Success(new DomainPage { TotalCount = response.TotalCount.Value, Domains = domains });
    }

    private static Result<PostmarkDomain> CreatePostmarkDomain(DomainResponseModel? response, string context)
    {
        if (response is null)
            return Result.Failure<PostmarkDomain>($"{context} returned from the Postmark Domains API was null.");

        var core = ValidationExtensions.ValidateDomainCore(response, context);
        if (core.IsFailure(out var coreError))
            return Result.Failure<PostmarkDomain>(coreError);

        if (response.SpfVerified is null)
            return Result.Failure<PostmarkDomain>($"SPFVerified was not returned from the Postmark Domains API for {context}.");

        if (response.DkimVerified is null)
            return Result.Failure<PostmarkDomain>($"DKIMVerified was not returned from the Postmark Domains API for {context}.");

        if (response.WeakDkim is null)
            return Result.Failure<PostmarkDomain>($"WeakDKIM was not returned from the Postmark Domains API for {context}.");

        if (response.ReturnPathDomainVerified is null)
            return Result.Failure<PostmarkDomain>($"ReturnPathDomainVerified was not returned from the Postmark Domains API for {context}.");

        return Result.Success(new PostmarkDomain(response.Id!.Value, response.Name!, response.SpfVerified.Value, ValidationExtensions.NormalizeOptionalString(response.SpfHost),
            ValidationExtensions.NormalizeOptionalString(response.SpfTextValue), response.DkimVerified.Value, response.WeakDkim.Value, ValidationExtensions.NormalizeOptionalString(response.DkimHost),
            ValidationExtensions.NormalizeOptionalString(response.DkimTextValue), ValidationExtensions.NormalizeOptionalString(response.DkimPendingHost), ValidationExtensions.NormalizeOptionalString(response.DkimPendingTextValue),
            ValidationExtensions.NormalizeOptionalString(response.DkimRevokedHost), ValidationExtensions.NormalizeOptionalString(response.DkimRevokedTextValue), response.SafeToRemoveRevokedKeyFromDns,
            ValidationExtensions.NormalizeOptionalString(response.DkimUpdateStatus), ValidationExtensions.NormalizeOptionalString(response.ReturnPathDomain), response.ReturnPathDomainVerified.Value,
            ValidationExtensions.NormalizeOptionalString(response.ReturnPathDomainCNameValue)));
    }

    private static Result<DomainDkimRotation> CreateDomainDkimRotation(DomainResponseModel? response)
    {
        const string context = "domain DKIM rotation response";
        if (response is null)
            return Result.Failure<DomainDkimRotation>($"{context} returned from the Postmark Domains API was null.");

        var core = ValidationExtensions.ValidateDomainCore(response, context);
        if (core.IsFailure(out var coreError))
            return Result.Failure<DomainDkimRotation>(coreError);

        if (response.DkimVerified is null)
            return Result.Failure<DomainDkimRotation>($"DKIMVerified was not returned from the Postmark Domains API for {context}.");

        if (response.WeakDkim is null)
            return Result.Failure<DomainDkimRotation>($"WeakDKIM was not returned from the Postmark Domains API for {context}.");

        return Result.Success(new DomainDkimRotation
        {
            Id = response.Id!.Value,
            Name = response.Name!,
            DkimVerified = response.DkimVerified.Value,
            WeakDkim = response.WeakDkim.Value,
            DkimHost = ValidationExtensions.NormalizeOptionalString(response.DkimHost),
            DkimTextValue = ValidationExtensions.NormalizeOptionalString(response.DkimTextValue),
            DkimPendingHost = ValidationExtensions.NormalizeOptionalString(response.DkimPendingHost),
            DkimPendingTextValue = ValidationExtensions.NormalizeOptionalString(response.DkimPendingTextValue),
            DkimRevokedHost = ValidationExtensions.NormalizeOptionalString(response.DkimRevokedHost),
            DkimRevokedTextValue = ValidationExtensions.NormalizeOptionalString(response.DkimRevokedTextValue),
            SafeToRemoveRevokedKeyFromDns = response.SafeToRemoveRevokedKeyFromDns,
            DkimUpdateStatus = ValidationExtensions.NormalizeOptionalString(response.DkimUpdateStatus)
        });
    }

    private static Result<DomainSpfVerification> CreateDomainSpfVerification(DomainSpfVerificationModel response)
    {
        if (response.SpfVerified is null)
            return Result.Failure<DomainSpfVerification>("SPFVerified was not returned from the Postmark Domains API SPF verification response.");

        return Result.Success(new DomainSpfVerification
        {
            SpfHost = ValidationExtensions.NormalizeOptionalString(response.SpfHost),
            SpfVerified = response.SpfVerified.Value,
            SpfTextValue = ValidationExtensions.NormalizeOptionalString(response.SpfTextValue)
        });
    }

    private static Result<DomainDeletion> CreateDomainDeletion(DomainDeletionModel response)
    {
        if (response.ErrorCode is null)
            return Result.Failure<DomainDeletion>("ErrorCode was not returned from the Postmark Domains API.");

        if (string.IsNullOrWhiteSpace(response.Message))
            return Result.Failure<DomainDeletion>("Message was not returned from the Postmark Domains API.");

        if (response.ErrorCode.Value != 0)
            return Result.Failure<DomainDeletion>(new PostmarkError(response.ErrorCode.Value, response.Message));

        return Result.Success(new DomainDeletion { Message = response.Message });
    }

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to list domains.")]
    private partial void LogListDomainsException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to list domains. {Message}")]
    private partial void LogListDomainsError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to get the domain.")]
    private partial void LogGetDomainException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to get the domain. {Message}")]
    private partial void LogGetDomainError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to create the domain.")]
    private partial void LogCreateDomainException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to create the domain. {Message}")]
    private partial void LogCreateDomainError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to edit the domain.")]
    private partial void LogEditDomainException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to edit the domain. {Message}")]
    private partial void LogEditDomainError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to delete the domain.")]
    private partial void LogDeleteDomainException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to delete the domain. {Message}")]
    private partial void LogDeleteDomainError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to verify domain DKIM.")]
    private partial void LogVerifyDomainDkimException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to verify domain DKIM. {Message}")]
    private partial void LogVerifyDomainDkimError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to verify domain return path.")]
    private partial void LogVerifyDomainReturnPathException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to verify domain return path. {Message}")]
    private partial void LogVerifyDomainReturnPathError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to verify domain SPF.")]
    private partial void LogVerifyDomainSpfException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to verify domain SPF. {Message}")]
    private partial void LogVerifyDomainSpfError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to rotate domain DKIM.")]
    private partial void LogRotateDomainDkimException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to rotate domain DKIM. {Message}")]
    private partial void LogRotateDomainDkimError(string message, [LogProperties] IError error);
}
