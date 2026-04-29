using System.Buffers;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PostKit.Postmark.Common;

internal sealed class CountingBufferWriter(int initialSize = 256) : IBufferWriter<byte>, IDisposable
{
    private byte[] _buffer = ArrayPool<byte>.Shared.Rent(Math.Max(initialSize, 1));
    private int _count;

    public int BytesWritten => _count;

    public void Advance(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        _count += count;
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        EnsureCapacity(sizeHint);
        return _buffer.AsMemory(_count);
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        EnsureCapacity(sizeHint);
        return _buffer.AsSpan(_count);
    }

    private void EnsureCapacity(int sizeHint)
    {
        sizeHint = Math.Max(sizeHint, 1);

        var requiredSize = _count + sizeHint;
        if (_buffer.Length >= requiredSize)
            return;

        var newBuffer = ArrayPool<byte>.Shared.Rent(requiredSize);
        _buffer.AsSpan(0, _count)
            .CopyTo(newBuffer);

        ArrayPool<byte>.Shared.Return(_buffer);
        _buffer = newBuffer;
    }

    public void Reset()
    {
        _count = 0;
    }

    public void Dispose()
    {
        if (_buffer.Length == 0)
            return;

        ArrayPool<byte>.Shared.Return(_buffer);
        _buffer = Array.Empty<byte>();
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
