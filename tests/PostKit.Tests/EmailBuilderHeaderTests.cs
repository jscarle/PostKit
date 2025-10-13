using Xunit;

namespace PostKit.Tests;

public class EmailBuilderHeaderTests
{
    [Fact]
    public void WithHeader_DuplicateDictionaryKeys_ThrowsArgumentExceptionWithHeaderMessage()
    {
        var builder = Email.CreateBuilder();
        var headers = new Dictionary<string, string>
        {
            ["X-Test"] = "A",
            ["x-test"] = "B",
        };

        var exception = Assert.Throws<ArgumentException>(() => builder.WithHeader(headers));

        Assert.Equal("There are duplicate header entries. (Parameter 'headers')", exception.Message);
    }
}
