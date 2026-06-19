using System.Globalization;
using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.Common;
using PostKit.Errors;
using PostKit.InboundRules;
using PostKit.Postmark;
using InboundRuleDeletionModel = PostKit.Postmark.InboundRules.InboundRuleTriggerDeletionResponse;
using InboundRuleListModel = PostKit.Postmark.InboundRules.InboundRuleTriggerListResponse;
using InboundRuleRequestModel = PostKit.Postmark.InboundRules.InboundRuleTriggerCreateRequest;
using InboundRuleResponseModel = PostKit.Postmark.InboundRules.InboundRuleTriggerResponse;

// ReSharper disable once CheckNamespace
namespace PostKit;

internal sealed partial class PostKitClient
{
    public async Task<Result<InboundRuleTriggerPage>> ListInboundRuleTriggersAsync(int count = 500, int offset = 0, CancellationToken cancellationToken = default)
    {
        var validationError = ValidationExtensions.ValidateInboundRuleListRequest(count, offset);
        if (validationError is not null)
            return Result.Failure<InboundRuleTriggerPage>(validationError);

        Result<InboundRuleListModel> response;
        try
        {
            var endpoint = $"/triggers/inboundrules?count={count.ToString(CultureInfo.InvariantCulture)}&offset={offset.ToString(CultureInfo.InvariantCulture)}";
            response = await postmark.GetAsync<InboundRuleListModel>(PostmarkTokenScope.Server, endpoint, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogListInboundRuleTriggersException(ex);
            return Result.Failure<InboundRuleTriggerPage>(ex);
        }

        if (response.IsFailure(out var error, out var listModel))
        {
            LogListInboundRuleTriggersError(error.Message, error);
            return Result.Failure<InboundRuleTriggerPage>(error);
        }

        var mapped = CreateInboundRuleTriggerPage(listModel);
        if (mapped.IsFailure(out var mappingError, out var page))
        {
            LogListInboundRuleTriggersError(mappingError.Message, mappingError);
            return Result.Failure<InboundRuleTriggerPage>(mappingError);
        }

        return Result.Success(page);
    }

    public Task<Result<InboundRuleTrigger>> CreateInboundRuleTriggerAsync(string rule, CancellationToken cancellationToken = default)
    {
        if (rule is null)
            throw new ArgumentNullException(nameof(rule), "The inbound rule trigger rule cannot be null.");

        return CreateInboundRuleTriggerAsync(new InboundRuleTriggerCreateParameters { Rule = rule }, cancellationToken);
    }

    public async Task<Result<InboundRuleTrigger>> CreateInboundRuleTriggerAsync(InboundRuleTriggerCreateParameters parameters, CancellationToken cancellationToken = default)
    {
        if (parameters is null)
            throw new ArgumentNullException(nameof(parameters), "The inbound rule trigger create parameters cannot be null.");

        var requestModel = CreateInboundRuleTriggerRequest(parameters);
        if (requestModel.IsFailure(out var requestError, out var mappedRequest))
            return Result.Failure<InboundRuleTrigger>(requestError);

        Result<InboundRuleResponseModel> response;
        try
        {
            response = await postmark.PostAsync<InboundRuleRequestModel, InboundRuleResponseModel>(PostmarkTokenScope.Server, "/triggers/inboundrules", mappedRequest, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogCreateInboundRuleTriggerException(ex);
            return Result.Failure<InboundRuleTrigger>(ex);
        }

        if (response.IsFailure(out var error, out var ruleModel))
        {
            LogCreateInboundRuleTriggerError(error.Message, error);
            return Result.Failure<InboundRuleTrigger>(error);
        }

        var mapped = CreateInboundRuleTrigger(ruleModel, "inbound rule trigger create response");
        if (mapped.IsFailure(out var mappingError, out var rule))
        {
            LogCreateInboundRuleTriggerError(mappingError.Message, mappingError);
            return Result.Failure<InboundRuleTrigger>(mappingError);
        }

        return Result.Success(rule);
    }

    public async Task<Result<InboundRuleTriggerDeletion>> DeleteInboundRuleTriggerAsync(long id, CancellationToken cancellationToken = default)
    {
        var validationError = ValidationExtensions.ValidateInboundRuleTriggerId(id);
        if (validationError is not null)
            return Result.Failure<InboundRuleTriggerDeletion>(validationError);

        Result<InboundRuleDeletionModel> response;
        try
        {
            response = await postmark.DeleteAsync<InboundRuleDeletionModel>(PostmarkTokenScope.Server, $"/triggers/inboundrules/{id}", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogDeleteInboundRuleTriggerException(ex);
            return Result.Failure<InboundRuleTriggerDeletion>(ex);
        }

        if (response.IsFailure(out var error, out var deletionModel))
        {
            LogDeleteInboundRuleTriggerError(error.Message, error);
            return Result.Failure<InboundRuleTriggerDeletion>(error);
        }

        var mapped = CreateInboundRuleTriggerDeletion(deletionModel);
        if (mapped.IsFailure(out var mappingError, out var deletion))
        {
            LogDeleteInboundRuleTriggerError(mappingError.Message, mappingError);
            return Result.Failure<InboundRuleTriggerDeletion>(mappingError);
        }

        return Result.Success(deletion);
    }

    private static Result<InboundRuleRequestModel> CreateInboundRuleTriggerRequest(InboundRuleTriggerCreateParameters parameters)
    {
        if (string.IsNullOrWhiteSpace(parameters.Rule))
            return Result.Failure<InboundRuleRequestModel>($"The inbound rule trigger create parameters rule cannot be empty or whitespace. Actual length: {parameters.Rule?.Length ?? 0}.");

        return Result.Success(new InboundRuleRequestModel { Rule = parameters.Rule });
    }

    private static Result<InboundRuleTriggerPage> CreateInboundRuleTriggerPage(InboundRuleListModel response)
    {
        if (response.TotalCount is null)
            return Result.Failure<InboundRuleTriggerPage>("TotalCount was not returned from the Postmark Inbound Rules API.");

        if (response.TotalCount.Value < 0)
            return Result.Failure<InboundRuleTriggerPage>($"TotalCount returned from the Postmark Inbound Rules API was invalid. Received {response.TotalCount.Value}.");

        if (response.InboundRules is null)
            return Result.Failure<InboundRuleTriggerPage>("InboundRules were not returned from the Postmark Inbound Rules API.");

        var rules = new List<InboundRuleTrigger>(response.InboundRules.Count);
        for (var index = 0; index < response.InboundRules.Count; index++)
        {
            var mapped = CreateInboundRuleTrigger(response.InboundRules[index], $"inbound rule trigger list item {index}");
            if (mapped.IsFailure(out var error, out var rule))
                return Result.Failure<InboundRuleTriggerPage>(error);

            rules.Add(rule);
        }

        return Result.Success(new InboundRuleTriggerPage { TotalCount = response.TotalCount.Value, InboundRules = rules });
    }

    private static Result<InboundRuleTrigger> CreateInboundRuleTrigger(InboundRuleResponseModel? response, string context)
    {
        if (response is null)
            return Result.Failure<InboundRuleTrigger>($"{context} returned from the Postmark Inbound Rules API was null.");

        if (response.Id is null)
            return Result.Failure<InboundRuleTrigger>($"ID was not returned from the Postmark Inbound Rules API for {context}.");

        if (response.Id.Value <= 0)
            return Result.Failure<InboundRuleTrigger>($"ID returned from the Postmark Inbound Rules API for {context} was invalid. Received {response.Id.Value}.");

        if (string.IsNullOrWhiteSpace(response.Rule))
            return Result.Failure<InboundRuleTrigger>($"Rule was not returned from the Postmark Inbound Rules API for {context}.");

        return Result.Success(new InboundRuleTrigger { Id = response.Id.Value, Rule = response.Rule });
    }

    private static Result<InboundRuleTriggerDeletion> CreateInboundRuleTriggerDeletion(InboundRuleDeletionModel response)
    {
        if (response.ErrorCode is null)
            return Result.Failure<InboundRuleTriggerDeletion>("ErrorCode was not returned from the Postmark Inbound Rules API.");

        if (string.IsNullOrWhiteSpace(response.Message))
            return Result.Failure<InboundRuleTriggerDeletion>("Message was not returned from the Postmark Inbound Rules API.");

        if (response.ErrorCode.Value != 0)
            return Result.Failure<InboundRuleTriggerDeletion>(new PostmarkError(response.ErrorCode.Value, response.Message));

        return Result.Success(new InboundRuleTriggerDeletion { Message = response.Message });
    }

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to list inbound rule triggers.")]
    private partial void LogListInboundRuleTriggersException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to list inbound rule triggers. {Message}")]
    private partial void LogListInboundRuleTriggersError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to create the inbound rule trigger.")]
    private partial void LogCreateInboundRuleTriggerException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to create the inbound rule trigger. {Message}")]
    private partial void LogCreateInboundRuleTriggerError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to delete the inbound rule trigger.")]
    private partial void LogDeleteInboundRuleTriggerException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to delete the inbound rule trigger. {Message}")]
    private partial void LogDeleteInboundRuleTriggerError(string message, [LogProperties] IError error);
}