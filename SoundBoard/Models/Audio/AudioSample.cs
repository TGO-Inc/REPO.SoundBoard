namespace SoundBoard.Models.Audio;

/// <summary>
///     Represents a sample of audio data.
/// </summary>
/// <param name="audio"><see cref="float" />[]</param>
/// <param name="length">
///     <see cref="int" />
/// </param>
public readonly struct AudioSample(float[] audio, int length)
{
    /// <summary>
    ///     The audio data.
    /// </summary>
    public readonly float[] Audio = audio;

    /// <summary>
    ///     The length of the audio data.
    /// </summary>
    public readonly int Length = length;
}