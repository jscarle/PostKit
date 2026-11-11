using System.Buffers;
using System.Text.Json;

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
        return _buffer;
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        EnsureCapacity(sizeHint);
        return _buffer;
    }

    public void Dispose()
    {
        if (_buffer.Length == 0)
            return;

        ArrayPool<byte>.Shared.Return(_buffer);
        _buffer = Array.Empty<byte>();
    }

    private void EnsureCapacity(int sizeHint)
    {
        sizeHint = Math.Max(sizeHint, 1);

        if (_buffer.Length >= sizeHint)
            return;

        // Committed JSON is only counted; retain storage for the current writer segment.
        var newBuffer = ArrayPool<byte>.Shared.Rent(sizeHint);
        ArrayPool<byte>.Shared.Return(_buffer);
        _buffer = newBuffer;
    }
}

internal static class JsonSizeEstimator
{
    public static int GetSerializedSize<T>(T value, JsonSerializerOptions options)
    {
        using var buffer = new CountingBufferWriter();
        using var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions { Encoder = options.Encoder, Indented = options.WriteIndented, MaxDepth = options.MaxDepth });

        JsonSerializer.Serialize(writer, value, options);
        writer.Flush();

        return buffer.BytesWritten;
    }
}
