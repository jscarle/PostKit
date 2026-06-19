using LightResults;

namespace PostKit.Postmark;

internal interface IPostmarkClient
{
    Task<Result<TResponse>> PostAsync<TRequest, TResponse>(PostmarkTokenScope tokenScope, string endpoint, TRequest body, CancellationToken cancellationToken = default);

    Task<Result<TResponse>> PostAsync<TResponse>(PostmarkTokenScope tokenScope, string endpoint, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("POST is not supported by this test double.");
    }

    Task<Result<TResponse>> GetAsync<TResponse>(PostmarkTokenScope tokenScope, string endpoint, CancellationToken cancellationToken = default);

    Task<Result<TResponse>> PutAsync<TResponse>(PostmarkTokenScope tokenScope, string endpoint, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("PUT is not supported by this test double.");
    }

    Task<Result<TResponse>> PutAsync<TRequest, TResponse>(PostmarkTokenScope tokenScope, string endpoint, TRequest body, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("PUT is not supported by this test double.");
    }

    Task<Result<TResponse>> PatchAsync<TRequest, TResponse>(PostmarkTokenScope tokenScope, string endpoint, TRequest body, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("PATCH is not supported by this test double.");
    }

    Task<Result<TResponse>> DeleteAsync<TResponse>(PostmarkTokenScope tokenScope, string endpoint, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("DELETE is not supported by this test double.");
    }
}