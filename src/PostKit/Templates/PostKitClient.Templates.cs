using System.Globalization;
using System.Text.Json.Nodes;
using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.Common;
using PostKit.Errors;
using PostKit.Postmark;
using PostKit.Templates;
using TemplateDeletionModel = PostKit.Postmark.Templates.TemplateDeletionResponse;
using TemplateListModel = PostKit.Postmark.Templates.TemplateListResponse;
using TemplatePushChangeModel = PostKit.Postmark.Templates.TemplatePushChangeResponse;
using TemplatePushModel = PostKit.Postmark.Templates.TemplatePushResponse;
using TemplatePushRequestModel = PostKit.Postmark.Templates.TemplatePushRequest;
using TemplateRequestModel = PostKit.Postmark.Templates.TemplateRequest;
using TemplateResponseModel = PostKit.Postmark.Templates.TemplateResponse;
using TemplateSummaryModel = PostKit.Postmark.Templates.TemplateSummaryResponse;
using TemplateValidationContentModel = PostKit.Postmark.Templates.TemplateValidationContentResponse;
using TemplateValidationErrorModel = PostKit.Postmark.Templates.TemplateValidationErrorResponse;
using TemplateValidationModel = PostKit.Postmark.Templates.TemplateValidationResponse;
using TemplateValidationRequestModel = PostKit.Postmark.Templates.TemplateValidationRequest;

// ReSharper disable once CheckNamespace
namespace PostKit;

internal sealed partial class PostKitClient
{
    public Task<Result<Template>> GetTemplateAsync(long templateId, CancellationToken cancellationToken = default)
    {
        var endpoint = BuildTemplateIdEndpoint(templateId);
        if (endpoint.IsFailure(out var error, out var value))
            return Task.FromResult(Result.Failure<Template>(error));

        return GetTemplateCoreAsync(value, cancellationToken);
    }

    public Task<Result<Template>> GetTemplateAsync(string templateAlias, CancellationToken cancellationToken = default)
    {
        var endpoint = BuildTemplateAliasEndpoint(templateAlias);
        if (endpoint.IsFailure(out var error, out var value))
            return Task.FromResult(Result.Failure<Template>(error));

        return GetTemplateCoreAsync(value, cancellationToken);
    }

    public async Task<Result<TemplateSummary>> CreateTemplateAsync(TemplateCreateParameters parameters, CancellationToken cancellationToken = default)
    {
        if (parameters is null)
            throw new ArgumentNullException(nameof(parameters), "The template create parameters cannot be null.");

        var mappedRequest = CreateTemplateRequest(parameters);
        if (mappedRequest.IsFailure(out var requestError, out var templateRequest))
            return Result.Failure<TemplateSummary>(requestError);

        Result<TemplateSummaryModel> response;
        try
        {
            response = await postmark.PostAsync<TemplateRequestModel, TemplateSummaryModel>(PostmarkTokenScope.Server, "/templates", templateRequest, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogCreateTemplateException(ex);
            return Result.Failure<TemplateSummary>(ex);
        }

        if (response.IsFailure(out var error, out var templateModel))
        {
            LogCreateTemplateError(error.Message, error);
            return Result.Failure<TemplateSummary>(error);
        }

        var mapped = CreateTemplateSummary(templateModel, "template create response");
        if (mapped.IsFailure(out var mappingError, out var templateSummary))
        {
            LogCreateTemplateError(mappingError.Message, mappingError);
            return Result.Failure<TemplateSummary>(mappingError);
        }

        return Result.Success(templateSummary);
    }

    public Task<Result<TemplateSummary>> EditTemplateAsync(long templateId, TemplateEditParameters parameters, CancellationToken cancellationToken = default)
    {
        var endpoint = BuildTemplateIdEndpoint(templateId);
        if (endpoint.IsFailure(out var error, out var value))
            return Task.FromResult(Result.Failure<TemplateSummary>(error));

        return EditTemplateCoreAsync(value, parameters, cancellationToken);
    }

    public Task<Result<TemplateSummary>> EditTemplateAsync(string templateAlias, TemplateEditParameters parameters, CancellationToken cancellationToken = default)
    {
        var endpoint = BuildTemplateAliasEndpoint(templateAlias);
        if (endpoint.IsFailure(out var error, out var value))
            return Task.FromResult(Result.Failure<TemplateSummary>(error));

        return EditTemplateCoreAsync(value, parameters, cancellationToken);
    }

    public async Task<Result<TemplatePage>> ListTemplatesAsync(int count = ValidationExtensions.MaxTemplateCount, int offset = 0, TemplateQuery? query = null, CancellationToken cancellationToken = default)
    {
        var validationError = ValidationExtensions.ValidateTemplateListRequest(count, offset, query);
        if (validationError is not null)
            return Result.Failure<TemplatePage>(validationError);

        Result<TemplateListModel> response;
        try
        {
            var endpoint = BuildTemplateListEndpoint(count, offset, query);
            response = await postmark.GetAsync<TemplateListModel>(PostmarkTokenScope.Server, endpoint, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogListTemplatesException(ex);
            return Result.Failure<TemplatePage>(ex);
        }

        if (response.IsFailure(out var error, out var templateListModel))
        {
            LogListTemplatesError(error.Message, error);
            return Result.Failure<TemplatePage>(error);
        }

        var mapped = CreateTemplatePage(templateListModel);
        if (mapped.IsFailure(out var mappingError, out var templatePage))
        {
            LogListTemplatesError(mappingError.Message, mappingError);
            return Result.Failure<TemplatePage>(mappingError);
        }

        return Result.Success(templatePage);
    }

    public Task<Result<TemplateDeletion>> DeleteTemplateAsync(long templateId, CancellationToken cancellationToken = default)
    {
        var endpoint = BuildTemplateIdEndpoint(templateId);
        if (endpoint.IsFailure(out var error, out var value))
            return Task.FromResult(Result.Failure<TemplateDeletion>(error));

        return DeleteTemplateCoreAsync(value, cancellationToken);
    }

    public Task<Result<TemplateDeletion>> DeleteTemplateAsync(string templateAlias, CancellationToken cancellationToken = default)
    {
        var endpoint = BuildTemplateAliasEndpoint(templateAlias);
        if (endpoint.IsFailure(out var error, out var value))
            return Task.FromResult(Result.Failure<TemplateDeletion>(error));

        return DeleteTemplateCoreAsync(value, cancellationToken);
    }

    public async Task<Result<TemplateValidation>> ValidateTemplateAsync(TemplateValidationParameters parameters, CancellationToken cancellationToken = default)
    {
        if (parameters is null)
            throw new ArgumentNullException(nameof(parameters), "The template validation parameters cannot be null.");

        var mappedRequest = CreateTemplateValidationRequest(parameters);
        if (mappedRequest.IsFailure(out var requestError, out var templateRequest))
            return Result.Failure<TemplateValidation>(requestError);

        Result<TemplateValidationModel> response;
        try
        {
            response = await postmark.PostAsync<TemplateValidationRequestModel, TemplateValidationModel>(PostmarkTokenScope.Server, "/templates/validate", templateRequest, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogValidateTemplateException(ex);
            return Result.Failure<TemplateValidation>(ex);
        }

        if (response.IsFailure(out var error, out var validationModel))
        {
            LogValidateTemplateError(error.Message, error);
            return Result.Failure<TemplateValidation>(error);
        }

        var mapped = CreateTemplateValidation(validationModel);
        if (mapped.IsFailure(out var mappingError, out var validation))
        {
            LogValidateTemplateError(mappingError.Message, mappingError);
            return Result.Failure<TemplateValidation>(mappingError);
        }

        return Result.Success(validation);
    }

    public async Task<Result<TemplatePush>> PushTemplatesAsync(TemplatePushParameters parameters, CancellationToken cancellationToken = default)
    {
        if (parameters is null)
            throw new ArgumentNullException(nameof(parameters), "The template push parameters cannot be null.");

        var validationError = ValidationExtensions.ValidateTemplatePushParameters(parameters);
        if (validationError is not null)
            return Result.Failure<TemplatePush>(validationError);

        var pushRequest = new TemplatePushRequestModel
        {
            SourceServerId = parameters.SourceServerId,
            DestinationServerId = parameters.DestinationServerId,
            PerformChanges = parameters.PerformChanges
        };

        Result<TemplatePushModel> response;
        try
        {
            response = await postmark.PutAsync<TemplatePushRequestModel, TemplatePushModel>(PostmarkTokenScope.Account, "/templates/push", pushRequest, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogPushTemplatesException(ex);
            return Result.Failure<TemplatePush>(ex);
        }

        if (response.IsFailure(out var error, out var pushModel))
        {
            LogPushTemplatesError(error.Message, error);
            return Result.Failure<TemplatePush>(error);
        }

        var mapped = CreateTemplatePush(pushModel);
        if (mapped.IsFailure(out var mappingError, out var push))
        {
            LogPushTemplatesError(mappingError.Message, mappingError);
            return Result.Failure<TemplatePush>(mappingError);
        }

        return Result.Success(push);
    }

    private async Task<Result<Template>> GetTemplateCoreAsync(string endpoint, CancellationToken cancellationToken)
    {
        Result<TemplateResponseModel> response;
        try
        {
            response = await postmark.GetAsync<TemplateResponseModel>(PostmarkTokenScope.Server, endpoint, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogGetTemplateException(ex);
            return Result.Failure<Template>(ex);
        }

        if (response.IsFailure(out var error, out var templateModel))
        {
            LogGetTemplateError(error.Message, error);
            return Result.Failure<Template>(error);
        }

        var mapped = CreateTemplate(templateModel);
        if (mapped.IsFailure(out var mappingError, out var template))
        {
            LogGetTemplateError(mappingError.Message, mappingError);
            return Result.Failure<Template>(mappingError);
        }

        return Result.Success(template);
    }

    private async Task<Result<TemplateSummary>> EditTemplateCoreAsync(string endpoint, TemplateEditParameters parameters, CancellationToken cancellationToken)
    {
        if (parameters is null)
            throw new ArgumentNullException(nameof(parameters), "The template edit parameters cannot be null.");

        var request = CreateTemplateRequest(parameters);
        if (request.IsFailure(out var requestError, out var templateRequest))
            return Result.Failure<TemplateSummary>(requestError);

        Result<TemplateSummaryModel> response;
        try
        {
            response = await postmark.PutAsync<TemplateRequestModel, TemplateSummaryModel>(PostmarkTokenScope.Server, endpoint, templateRequest, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogEditTemplateException(ex);
            return Result.Failure<TemplateSummary>(ex);
        }

        if (response.IsFailure(out var error, out var templateModel))
        {
            LogEditTemplateError(error.Message, error);
            return Result.Failure<TemplateSummary>(error);
        }

        var mapped = CreateTemplateSummary(templateModel, "template edit response");
        if (mapped.IsFailure(out var mappingError, out var templateSummary))
        {
            LogEditTemplateError(mappingError.Message, mappingError);
            return Result.Failure<TemplateSummary>(mappingError);
        }

        return Result.Success(templateSummary);
    }

    private async Task<Result<TemplateDeletion>> DeleteTemplateCoreAsync(string endpoint, CancellationToken cancellationToken)
    {
        Result<TemplateDeletionModel> response;
        try
        {
            response = await postmark.DeleteAsync<TemplateDeletionModel>(PostmarkTokenScope.Server, endpoint, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogDeleteTemplateException(ex);
            return Result.Failure<TemplateDeletion>(ex);
        }

        if (response.IsFailure(out var error, out var deletionModel))
        {
            LogDeleteTemplateError(error.Message, error);
            return Result.Failure<TemplateDeletion>(error);
        }

        var mapped = CreateTemplateDeletion(deletionModel);
        if (mapped.IsFailure(out var mappingError, out var deletion))
        {
            LogDeleteTemplateError(mappingError.Message, mappingError);
            return Result.Failure<TemplateDeletion>(mappingError);
        }

        return Result.Success(deletion);
    }

    private static Result<string> BuildTemplateIdEndpoint(long templateId)
    {
        if (templateId <= 0)
            return Result.Failure<string>($"The template ID must be greater than zero. Received {templateId}.");

        return Result.Success($"/templates/{templateId.ToString(CultureInfo.InvariantCulture)}");
    }

    private static Result<string> BuildTemplateAliasEndpoint(string templateAlias)
    {
        if (templateAlias is null)
            throw new ArgumentNullException(nameof(templateAlias), "The template alias cannot be null.");

        var validationError = ValidationExtensions.ValidateTemplateAlias(templateAlias, "The template alias", false);
        if (validationError is not null)
            return Result.Failure<string>(validationError);

        return Result.Success($"/templates/{Uri.EscapeDataString(templateAlias)}");
    }

    private static Result<TemplateRequestModel> CreateTemplateRequest(TemplateCreateParameters parameters)
    {
        var validationError = ValidationExtensions.ValidateTemplateParameters(parameters.Name, parameters.Alias, parameters.Subject, parameters.HtmlBody, parameters.TextBody, parameters.TemplateType, parameters.LayoutTemplate, "create");
        if (validationError is not null)
            return Result.Failure<TemplateRequestModel>(validationError);

        return Result.Success(new TemplateRequestModel
        {
            Name = parameters.Name,
            Alias = parameters.Alias,
            Subject = parameters.Subject,
            HtmlBody = parameters.HtmlBody,
            TextBody = parameters.TextBody,
            TemplateType = parameters.TemplateType.HasValue ? GetTemplateTypeValue(parameters.TemplateType.Value) : null,
            LayoutTemplate = parameters.LayoutTemplate
        });
    }

    private static Result<TemplateRequestModel> CreateTemplateRequest(TemplateEditParameters parameters)
    {
        var validationError = ValidationExtensions.ValidateTemplateParameters(parameters.Name, parameters.Alias, parameters.Subject, parameters.HtmlBody, parameters.TextBody, null, parameters.LayoutTemplate, "edit");
        if (validationError is not null)
            return Result.Failure<TemplateRequestModel>(validationError);

        return Result.Success(new TemplateRequestModel
        {
            Name = parameters.Name,
            Alias = parameters.Alias,
            Subject = parameters.Subject,
            HtmlBody = parameters.HtmlBody,
            TextBody = parameters.TextBody,
            LayoutTemplate = parameters.LayoutTemplate
        });
    }

    private static Result<TemplateValidationRequestModel> CreateTemplateValidationRequest(TemplateValidationParameters parameters)
    {
        var validationError = ValidationExtensions.ValidateTemplateValidationParameters(parameters);
        if (validationError is not null)
            return Result.Failure<TemplateValidationRequestModel>(validationError);

        JsonNode? testRenderModel = null;
        if (parameters.TestRenderModel is not null)
            try
            {
                testRenderModel = parameters.TestRenderModel.SnapshotTemplateModel(nameof(parameters.TestRenderModel))
                    .Snapshot;
            }
            catch (Exception ex)
            {
                return Result.Failure<TemplateValidationRequestModel>($"The template validation test render model could not be prepared: {ex.Message}");
            }

        return Result.Success(new TemplateValidationRequestModel
        {
            Subject = parameters.Subject,
            HtmlBody = parameters.HtmlBody,
            TextBody = parameters.TextBody,
            TestRenderModel = testRenderModel,
            InlineCssForHtmlTestRender = parameters.InlineCssForHtmlTestRender,
            TemplateType = parameters.TemplateType.HasValue ? GetTemplateTypeValue(parameters.TemplateType.Value) : null,
            LayoutTemplate = parameters.LayoutTemplate
        });
    }

    private static string BuildTemplateListEndpoint(int count, int offset, TemplateQuery? query)
    {
        var parameters = new List<string>(4)
        {
            $"count={count.ToString(CultureInfo.InvariantCulture)}",
            $"offset={offset.ToString(CultureInfo.InvariantCulture)}"
        };

        if (query?.TemplateType.HasValue == true)
            parameters.Add($"TemplateType={Uri.EscapeDataString(GetTemplateListTypeValue(query.TemplateType.Value))}");

        if (query?.LayoutTemplate is not null)
            parameters.Add($"LayoutTemplate={Uri.EscapeDataString(query.LayoutTemplate)}");

        return $"/templates?{string.Join("&", parameters)}";
    }

    private static Result<TemplatePage> CreateTemplatePage(TemplateListModel response)
    {
        if (response.TotalCount is null)
            return Result.Failure<TemplatePage>("TotalCount was not returned from the Postmark Templates API.");

        if (response.TotalCount.Value < 0)
            return Result.Failure<TemplatePage>($"TotalCount returned from the Postmark Templates API was invalid. Received {response.TotalCount.Value}.");

        if (response.Templates is null)
            return Result.Failure<TemplatePage>("Templates were not returned from the Postmark Templates API.");

        var templates = new List<TemplateSummary>(response.Templates.Count);
        for (var index = 0; index < response.Templates.Count; index++)
        {
            var mapped = CreateTemplateSummary(response.Templates[index], $"template list item {index}");
            if (mapped.IsFailure(out var error, out var template))
                return Result.Failure<TemplatePage>(error);

            templates.Add(template);
        }

        return Result.Success(new TemplatePage { TotalCount = response.TotalCount.Value, Templates = templates });
    }

    private static Result<Template> CreateTemplate(TemplateResponseModel response)
    {
        var summary = CreateTemplateSummary(response, "template response");
        if (summary.IsFailure(out var error, out var mappedSummary))
            return Result.Failure<Template>(error);

        if (response.AssociatedServerId is null)
            return Result.Failure<Template>("AssociatedServerId was not returned from the Postmark Templates API.");

        if (response.AssociatedServerId.Value <= 0)
            return Result.Failure<Template>($"AssociatedServerId returned from the Postmark Templates API was invalid. Received {response.AssociatedServerId.Value}.");

        return Result.Success(new Template(mappedSummary.TemplateId, mappedSummary.Name, response.Subject, response.HtmlBody, response.TextBody, response.AssociatedServerId.Value, mappedSummary.Active, mappedSummary.Alias,
            mappedSummary.TemplateType, mappedSummary.LayoutTemplate));
    }

    private static Result<TemplateSummary> CreateTemplateSummary(TemplateSummaryModel? response, string context)
    {
        if (response is null)
            return Result.Failure<TemplateSummary>($"{context} returned from the Postmark Templates API was null.");

        if (response.TemplateId is null)
            return Result.Failure<TemplateSummary>($"TemplateId was not returned from the Postmark Templates API for {context}.");

        if (response.TemplateId.Value <= 0)
            return Result.Failure<TemplateSummary>($"TemplateId returned from the Postmark Templates API for {context} was invalid. Received {response.TemplateId.Value}.");

        if (string.IsNullOrWhiteSpace(response.Name))
            return Result.Failure<TemplateSummary>($"Name was not returned from the Postmark Templates API for {context}.");

        if (response.Active is null)
            return Result.Failure<TemplateSummary>($"Active was not returned from the Postmark Templates API for {context}.");

        if (string.IsNullOrWhiteSpace(response.TemplateType))
            return Result.Failure<TemplateSummary>($"TemplateType was not returned from the Postmark Templates API for {context}.");

        var templateType = TryMapTemplateType(response.TemplateType);
        if (templateType.IsFailure(out var typeError, out var mappedType))
            return Result.Failure<TemplateSummary>($"{context} could not be mapped: {typeError.Message}");

        return Result.Success(new TemplateSummary(response.TemplateId.Value, response.Name, response.Active.Value, string.IsNullOrWhiteSpace(response.Alias) ? null : response.Alias, mappedType, response.LayoutTemplate));
    }

    private static Result<TemplateDeletion> CreateTemplateDeletion(TemplateDeletionModel response)
    {
        if (response.ErrorCode is null)
            return Result.Failure<TemplateDeletion>("ErrorCode was not returned from the Postmark Templates API.");

        if (string.IsNullOrWhiteSpace(response.Message))
            return Result.Failure<TemplateDeletion>("Message was not returned from the Postmark Templates API.");

        if (response.ErrorCode.Value != 0)
            return Result.Failure<TemplateDeletion>(new PostmarkError(response.ErrorCode.Value, response.Message));

        return Result.Success(new TemplateDeletion { Message = response.Message });
    }

    private static Result<TemplatePush> CreateTemplatePush(TemplatePushModel response)
    {
        if (response.TotalCount is null)
            return Result.Failure<TemplatePush>("TotalCount was not returned from the Postmark Templates API.");

        if (response.TotalCount.Value < 0)
            return Result.Failure<TemplatePush>($"TotalCount returned from the Postmark Templates API was invalid. Received {response.TotalCount.Value}.");

        if (response.Templates is null)
            return Result.Failure<TemplatePush>("Templates were not returned from the Postmark Templates API.");

        var changes = new List<TemplatePushChange>(response.Templates.Count);
        for (var index = 0; index < response.Templates.Count; index++)
        {
            var mapped = CreateTemplatePushChange(response.Templates[index], index);
            if (mapped.IsFailure(out var error, out var change))
                return Result.Failure<TemplatePush>(error);

            changes.Add(change);
        }

        return Result.Success(new TemplatePush { TotalCount = response.TotalCount.Value, Templates = changes });
    }

    private static Result<TemplatePushChange> CreateTemplatePushChange(TemplatePushChangeModel? response, int index)
    {
        if (response is null)
            return Result.Failure<TemplatePushChange>($"Template push change item {index} returned from the Postmark Templates API was null.");

        if (string.IsNullOrWhiteSpace(response.Action))
            return Result.Failure<TemplatePushChange>($"Action was not returned from the Postmark Templates API for template push change item {index}.");

        var action = response.Action switch
        {
            "Create" => TemplatePushAction.Create,
            "Edit" => TemplatePushAction.Edit,
            _ => (TemplatePushAction?)null
        };

        if (action is null)
            return Result.Failure<TemplatePushChange>($"Action value '{response.Action}' returned from the Postmark Templates API for template push change item {index} is not supported.");

        if (response.TemplateId is null)
            return Result.Failure<TemplatePushChange>($"TemplateId was not returned from the Postmark Templates API for template push change item {index}.");

        if (response.TemplateId.Value <= 0)
            return Result.Failure<TemplatePushChange>($"TemplateId returned from the Postmark Templates API for template push change item {index} was invalid. Received {response.TemplateId.Value}.");

        if (string.IsNullOrWhiteSpace(response.Name))
            return Result.Failure<TemplatePushChange>($"Name was not returned from the Postmark Templates API for template push change item {index}.");

        if (string.IsNullOrWhiteSpace(response.TemplateType))
            return Result.Failure<TemplatePushChange>($"TemplateType was not returned from the Postmark Templates API for template push change item {index}.");

        var templateType = TryMapTemplateType(response.TemplateType);
        if (templateType.IsFailure(out var typeError, out var mappedType))
            return Result.Failure<TemplatePushChange>($"Template push change item {index} could not be mapped: {typeError.Message}");

        return Result.Success(new TemplatePushChange
        {
            Action = action.Value,
            TemplateId = response.TemplateId.Value,
            Alias = response.Alias,
            Name = response.Name,
            TemplateType = mappedType
        });
    }

    private static Result<TemplateValidation> CreateTemplateValidation(TemplateValidationModel response)
    {
        if (response.AllContentIsValid is null)
            return Result.Failure<TemplateValidation>("AllContentIsValid was not returned from the Postmark Templates API.");

        var htmlBody = CreateTemplateValidationContent(response.HtmlBody, "HtmlBody");
        if (htmlBody.IsFailure(out var htmlError, out var mappedHtml))
            return Result.Failure<TemplateValidation>(htmlError);

        var textBody = CreateTemplateValidationContent(response.TextBody, "TextBody");
        if (textBody.IsFailure(out var textError, out var mappedText))
            return Result.Failure<TemplateValidation>(textError);

        var subject = CreateTemplateValidationContent(response.Subject, "Subject");
        if (subject.IsFailure(out var subjectError, out var mappedSubject))
            return Result.Failure<TemplateValidation>(subjectError);

        return Result.Success(new TemplateValidation
        {
            AllContentIsValid = response.AllContentIsValid.Value,
            HtmlBody = mappedHtml,
            TextBody = mappedText,
            Subject = mappedSubject,
            SuggestedTemplateModel = response.SuggestedTemplateModel?.DeepClone()
        });
    }

    private static Result<TemplateValidationContent?> CreateTemplateValidationContent(TemplateValidationContentModel? response, string propertyName)
    {
        if (response is null)
            return Result.Success<TemplateValidationContent?>(null);

        if (response.ContentIsValid is null)
            return Result.Failure<TemplateValidationContent?>($"ContentIsValid was not returned from the Postmark Templates API for {propertyName}.");

        if (response.ValidationErrors is null)
            return Result.Failure<TemplateValidationContent?>($"ValidationErrors were not returned from the Postmark Templates API for {propertyName}.");

        var errors = new List<TemplateValidationError>(response.ValidationErrors.Count);
        for (var index = 0; index < response.ValidationErrors.Count; index++)
        {
            var mapped = CreateTemplateValidationError(response.ValidationErrors[index], propertyName, index);
            if (mapped.IsFailure(out var error, out var validationError))
                return Result.Failure<TemplateValidationContent?>(error);

            errors.Add(validationError);
        }

        return Result.Success<TemplateValidationContent?>(new TemplateValidationContent
        {
            ContentIsValid = response.ContentIsValid.Value,
            ValidationErrors = errors,
            RenderedContent = response.RenderedContent
        });
    }

    private static Result<TemplateValidationError> CreateTemplateValidationError(TemplateValidationErrorModel? response, string propertyName, int index)
    {
        if (response is null)
            return Result.Failure<TemplateValidationError>($"ValidationErrors item {index} for {propertyName} returned from the Postmark Templates API was null.");

        if (string.IsNullOrWhiteSpace(response.Message))
            return Result.Failure<TemplateValidationError>($"Message was not returned from the Postmark Templates API for {propertyName} ValidationErrors item {index}.");

        return Result.Success(new TemplateValidationError { Message = response.Message, Line = response.Line, CharacterPosition = response.CharacterPosition });
    }

    private static string GetTemplateTypeValue(TemplateType type)
    {
        return type switch
        {
            TemplateType.Standard => "Standard",
            TemplateType.Layout => "Layout",
            _ => throw new NotImplementedException($"Template type enum value of '{nameof(TemplateType)}.{type}' has not been implemented. Please open an issue in the PostKit repository (https://github.com/jscarle/PostKit/issues).")
        };
    }

    private static string GetTemplateListTypeValue(TemplateListType type)
    {
        return type switch
        {
            TemplateListType.All => "All",
            TemplateListType.Standard => "Standard",
            TemplateListType.Layout => "Layout",
            _ => throw new NotImplementedException(
                $"Template list type enum value of '{nameof(TemplateListType)}.{type}' has not been implemented. Please open an issue in the PostKit repository (https://github.com/jscarle/PostKit/issues).")
        };
    }

    private static Result<TemplateType> TryMapTemplateType(string type)
    {
        var mapped = type switch
        {
            "Standard" => TemplateType.Standard,
            "Layout" => TemplateType.Layout,
            _ => (TemplateType?)null
        };

        if (mapped is null)
            return Result.Failure<TemplateType>($"TemplateType value '{type}' returned from the Postmark Templates API is not supported.");

        return Result.Success(mapped.Value);
    }

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to get the template.")]
    private partial void LogGetTemplateException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to get the template. {Message}")]
    private partial void LogGetTemplateError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to create the template.")]
    private partial void LogCreateTemplateException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to create the template. {Message}")]
    private partial void LogCreateTemplateError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to edit the template.")]
    private partial void LogEditTemplateException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to edit the template. {Message}")]
    private partial void LogEditTemplateError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to list templates.")]
    private partial void LogListTemplatesException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to list templates. {Message}")]
    private partial void LogListTemplatesError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to delete the template.")]
    private partial void LogDeleteTemplateException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to delete the template. {Message}")]
    private partial void LogDeleteTemplateError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to validate the template.")]
    private partial void LogValidateTemplateException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to validate the template. {Message}")]
    private partial void LogValidateTemplateError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to push templates.")]
    private partial void LogPushTemplatesException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to push templates. {Message}")]
    private partial void LogPushTemplatesError(string message, [LogProperties] IError error);
}
