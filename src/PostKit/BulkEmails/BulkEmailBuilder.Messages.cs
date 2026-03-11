namespace PostKit.BulkEmails;

partial class BulkEmailBuilder
{
    private readonly List<BulkEmailMessage> _messages = [];

    /// <inheritdoc/>
    public IBulkEmailBuilder AddMessage(BulkEmailMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        _messages.Add(message);

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailBuilder AddMessages(IEnumerable<BulkEmailMessage> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);

        foreach (var message in messages)
        {
            ArgumentNullException.ThrowIfNull(message);
            _messages.Add(message);
        }

        return this;
    }
}
