using LightResults;

namespace PostKit.Postmark;

internal interface IPostmarkClient
{
    Task<Result<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest body, CancellationToken cancellationToken = default);

    Task<Result<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default);
}
