using System.Buffers;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PostKit.Common;

internal sealed class CountingBufferWriter(int initialSize = 256) : IBufferWriter<byte>, IDisposable
{
    private byte[] _buffer = ArrayPool<byte>.Shared.Rent(Math.Max(initialSize, 1));
    public int BytesWritten { get; private set; }

    public void Advance(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        BytesWritten += count;
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        EnsureCapacity(sizeHint);
        return _buffer.AsMemory(BytesWritten);
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        EnsureCapacity(sizeHint);
        return _buffer.AsSpan(BytesWritten);
    }

    public void Dispose()
    {
        if (_buffer.Length == 0)
            return;

        ArrayPool<byte>.Shared.Return(_buffer);
        _buffer = Array.Empty<byte>();
    }

    public void Reset()
    {
        BytesWritten = 0;
    }

    private void EnsureCapacity(int sizeHint)
    {
        sizeHint = Math.Max(sizeHint, 1);

        var requiredSize = BytesWritten + sizeHint;
        if (_buffer.Length >= requiredSize)
            return;

        var newBuffer = ArrayPool<byte>.Shared.Rent(requiredSize);
        _buffer.AsSpan(0, BytesWritten)
            .CopyTo(newBuffer);

        ArrayPool<byte>.Shared.Return(_buffer);
        _buffer = newBuffer;
    }
}

internal static class JsonSizeEstimator
{
    public static int GetSerializedSize(JsonNode node, JsonSerializerOptions? options = null, bool indented = false)
    {
        using var buffer = new CountingBufferWriter();
        using var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions { Indented = indented });

        node.WriteTo(writer, options);
        writer.Flush();

        return buffer.BytesWritten;
    }
}