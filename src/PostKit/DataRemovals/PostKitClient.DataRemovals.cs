using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.Common;
using PostKit.DataRemovals;
using PostKit.Postmark;
using DataRemovalRequestModel = PostKit.Postmark.DataRemovals.DataRemovalCreateRequest;
using DataRemovalResponseModel = PostKit.Postmark.DataRemovals.DataRemovalResponse;

namespace PostKit;

// ReSharper disable once CheckNamespace
internal sealed partial class PostKitClient
{
    public async Task<Result<DataRemoval>> CreateDataRemovalAsync(DataRemovalCreateParameters parameters, CancellationToken cancellationToken = default)
    {
        if (parameters is null)
            throw new ArgumentNullException(nameof(parameters), "The data removal create parameters cannot be null.");

        var mappedRequest = CreateDataRemovalRequest(parameters);
        if (mappedRequest.IsFailure(out var requestError, out var requestModel))
            return Result.Failure<DataRemoval>(requestError);

        Result<DataRemovalResponseModel> response;
        try
        {
            response = await postmark.PostAsync<DataRemovalRequestModel, DataRemovalResponseModel>(PostmarkTokenScope.Account, "/data-removals", requestModel, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogCreateDataRemovalException(ex);
            return Result.Failure<DataRemoval>(ex);
        }

        if (response.IsFailure(out var error, out var dataRemovalModel))
        {
            LogCreateDataRemovalError(error.Message, error);
            return Result.Failure<DataRemoval>(error);
        }

        var mapped = CreateDataRemoval(dataRemovalModel);
        if (mapped.IsFailure(out var mappingError, out var dataRemoval))
        {
            LogCreateDataRemovalError(mappingError.Message, mappingError);
            return Result.Failure<DataRemoval>(mappingError);
        }

        return Result.Success(dataRemoval);
    }

    public async Task<Result<DataRemoval>> GetDataRemovalAsync(long id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            return Result.Failure<DataRemoval>($"The data removal request ID must be greater than zero. Received {id}.");

        Result<DataRemovalResponseModel> response;
        try
        {
            response = await postmark.GetAsync<DataRemovalResponseModel>(PostmarkTokenScope.Account, $"/data-removals/{id}", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogGetDataRemovalException(ex);
            return Result.Failure<DataRemoval>(ex);
        }

        if (response.IsFailure(out var error, out var dataRemovalModel))
        {
            LogGetDataRemovalError(error.Message, error);
            return Result.Failure<DataRemoval>(error);
        }

        var mapped = CreateDataRemoval(dataRemovalModel);
        if (mapped.IsFailure(out var mappingError, out var dataRemoval))
        {
            LogGetDataRemovalError(mappingError.Message, mappingError);
            return Result.Failure<DataRemoval>(mappingError);
        }

        return Result.Success(dataRemoval);
    }

    private static Result<DataRemovalRequestModel> CreateDataRemovalRequest(DataRemovalCreateParameters parameters)
    {
        var validationError = ValidationExtensions.ValidateDataRemovalEmailAddress(parameters.RequestedBy, nameof(DataRemovalCreateParameters.RequestedBy), "The data removal create parameters requested-by email address") ??
                              ValidationExtensions.ValidateDataRemovalEmailAddress(parameters.RequestedFor, nameof(DataRemovalCreateParameters.RequestedFor), "The data removal create parameters requested-for email address");

        if (validationError is not null)
            return Result.Failure<DataRemovalRequestModel>(validationError);

        return Result.Success(new DataRemovalRequestModel
        {
            RequestedBy = parameters.RequestedBy,
            RequestedFor = parameters.RequestedFor,
            NotifyWhenCompleted = parameters.NotifyWhenCompleted
        });
    }

    private static Result<DataRemoval> CreateDataRemoval(DataRemovalResponseModel response)
    {
        if (response.Id is null)
            return Result.Failure<DataRemoval>("ID was not returned from the Postmark Data Removal API.");

        if (response.Id.Value <= 0)
            return Result.Failure<DataRemoval>($"ID returned from the Postmark Data Removal API was invalid. Received {response.Id.Value}.");

        if (string.IsNullOrWhiteSpace(response.Status))
            return Result.Failure<DataRemoval>("Status was not returned from the Postmark Data Removal API.");

        var status = TryMapDataRemovalStatus(response.Status);
        if (status.IsFailure(out var statusError, out var mappedStatus))
            return Result.Failure<DataRemoval>(statusError);

        return Result.Success(new DataRemoval { Id = response.Id.Value, Status = mappedStatus });
    }

    private static Result<DataRemovalStatus> TryMapDataRemovalStatus(string status)
    {
        var mappedStatus = status switch
        {
            "Pending" => DataRemovalStatus.Pending,
            "Done" => DataRemovalStatus.Done,
            _ => (DataRemovalStatus?)null
        };

        if (mappedStatus is null)
            return Result.Failure<DataRemovalStatus>($"Status value '{status}' returned from the Postmark Data Removal API is not supported.");

        return Result.Success(mappedStatus.Value);
    }

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to create the data removal request.")]
    private partial void LogCreateDataRemovalException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to create the data removal request. {Message}")]
    private partial void LogCreateDataRemovalError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to get the data removal request.")]
    private partial void LogGetDataRemovalException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to get the data removal request. {Message}")]
    private partial void LogGetDataRemovalError(string message, [LogProperties] IError error);
}
