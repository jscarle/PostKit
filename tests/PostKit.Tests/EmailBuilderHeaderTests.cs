using PostKit.Emails;

namespace PostKit.Tests;

public class EmailBuilderHeaderTests
{
    [Fact]
    public void WithHeader_DuplicateDictionaryKeys_ThrowsArgumentExceptionWithHeaderMessage()
    {
        var builder = Email.CreateBuilder();
        var headers = new Dictionary<string, string> { ["X-Test"] = "A", ["x-test"] = "B" };

        var exception = Assert.Throws<ArgumentException>(() => builder.WithHeaders(headers));

        Assert.Equal("There are duplicate header entries. (Parameter 'headers')", exception.Message);
    }

    [Fact]
    public void WithHeader_BareCarriageReturnFold_ThrowsArgumentException()
    {
        var builder = Email.CreateBuilder();

        var exception = Assert.Throws<ArgumentException>(() => builder.WithHeader("X-Test", "bad\r value"));

        Assert.Equal("The header value is invalid. (Parameter 'value')", exception.Message);
    }

    [Fact]
    public void WithHeader_BareLineFeedFold_ThrowsArgumentException()
    {
        var builder = Email.CreateBuilder();

        var exception = Assert.Throws<ArgumentException>(() => builder.WithHeader("X-Test", "bad\n value"));

        Assert.Equal("The header value is invalid. (Parameter 'value')", exception.Message);
    }
}
