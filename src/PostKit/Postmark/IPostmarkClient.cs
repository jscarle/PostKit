using LightResults;

namespace PostKit.Postmark;

internal interface IPostmarkClient
{
    Task<Result<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest body, CancellationToken cancellationToken = default);
}
