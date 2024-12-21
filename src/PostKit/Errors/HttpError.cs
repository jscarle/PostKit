using System.Net;
using LightResults;

namespace PostKit.Errors;

internal class HttpError : Error
{
    public HttpStatusCode StatusCode { get; }

    public HttpError(HttpStatusCode httpStatusCode)
    {
        StatusCode = httpStatusCode;
    }

    public HttpError(HttpStatusCode httpStatusCode, string message)
        : base(message)
    {
        StatusCode = httpStatusCode;
    }
}
