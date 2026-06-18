namespace PostKit.Postmark;

internal interface IPostmarkClientFactory
{
    PostmarkClient Create(string? key = null);
}
