using SoundBoard.Helpers;

namespace SoundBoard.Models.Audio;

/// <summary>
/// A short-lived audio file wrapper.
/// </summary>
/// <param name="FullName"><see cref="string"/></param>
public record AudioFile(string FullName) : IDisposable, IAsyncDisposable
{
    public string FullName { get; } = FullName;
    public Stream Stream { get; } = new FileStream(FullName, FileMode.Open, FileAccess.Read);
    public AudioFormat GuessedType { get; } = GuessTypeFromName(FullName);
    public void Dispose() => Stream.Dispose();
    public async ValueTask DisposeAsync() => await Stream.DisposeAsync();
    
    /// <summary>
    /// Guesses the audio file type from the given name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static AudioFormat GuessTypeFromName(string name)
    {
        var ext = Path.GetExtension(name).ToLowerInvariant();
        return SupportedExtensions.GetValueOrDefault(ext, AudioFormat.UNKNOWN);
    }
    
    /// <summary>
    /// Supported audio file types.
    /// </summary>
    public static readonly Dictionary<string, AudioFormat> SupportedExtensions = GetValues<AudioFormat>().ToDictionary(e => '.' + e.ToString().ToLowerInvariant(), e => e);
    
    private static TEnum[] GetValues<TEnum>() where TEnum : Enum
        => (TEnum[])Enum.GetValues(typeof(TEnum));
}