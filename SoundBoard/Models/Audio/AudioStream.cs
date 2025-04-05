namespace SoundBoard.Models.Audio;

public class AudioStream(float[] audio, int sampleRate) : IAudioStream
{
    public bool EndOfStream => Position >= Length && Length > 0;

    public void Reset()
    {
        Position = 0;
    }

    public int Read(float[] buffer, int count, bool advance)
    {
        if (count == 0)
            return 0;

        if (audio.Length == 0)
            return -1;

        if (Position >= Length)
            throw new IndexOutOfRangeException();

        if (buffer.Length < count)
            throw new ArgumentException("Buffer is too small to hold the requested number of samples.");

        var bytesToRead = Math.Min(count, Length - Position);
        Array.Copy(audio, Position, buffer, 0, bytesToRead);
        if (advance)
            Position += bytesToRead;
        return (int)bytesToRead;
    }

    public int Read(float[] buffer, int offset, int count, bool advance)
    {
        if (count == 0 || audio.Length == 0)
            return 0;

        if (Position >= Length)
            throw new IndexOutOfRangeException();

        if (buffer.Length < offset + count)
            throw new ArgumentException("Buffer is too small to hold the requested number of samples.");

        var bytesToRead = Math.Min(count, Length - Position);
        Array.Copy(audio, Position, buffer, offset, bytesToRead);
        if (advance)
            Position += bytesToRead;
        return (int)bytesToRead;
    }

    public long Length { get; } = audio.Length;
    public long SampleRate { get; } = sampleRate;
    public long Position { get; private set; }
}