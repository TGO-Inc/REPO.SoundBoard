namespace SoundBoard.Models.Audio;

public interface IAudioStream
{
    /// <summary>
    /// The number of floats in the stream.
    /// </summary>
    long Length { get; }
    
    /// <summary>
    /// The sample rate of the audio stream.
    /// </summary>
    long SampleRate { get; }
    
    /// <summary>
    /// The current position in the stream.
    /// </summary>
    long Position { get; }

    /// <summary>
    /// Has the stream reached the end?
    /// </summary>
    bool EndOfStream { get; }

    /// <summary>
    /// Resets the stream to the beginning.
    /// </summary>
    void Reset();

    /// <summary>
    /// Reads a number of floats from the stream into the provided buffer.
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="count"></param>
    /// <param name="advance"></param>
    /// <returns></returns>
    int Read(float[] buffer, int count, bool advance);

    /// <summary>
    /// Reads a number of floats from the stream into the provided buffer, starting at the specified offset.
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    /// <param name="advance"></param>
    /// <returns></returns>
    int Read(float[] buffer, int offset, int count, bool advance);
}