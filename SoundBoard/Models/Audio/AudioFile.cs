namespace SoundBoard.Models.Audio;

/// <summary>
///     A short-lived audio file wrapper.
/// </summary>
/// <param name="FullName">
///     <see cref="string" />
/// </param>
public record AudioFile(string FullName) : IDisposable
{
    /// <summary>
    ///     Supported audio file types.
    /// </summary>
    public static readonly Dictionary<string, AudioFormat> SupportedExtensions =
        GetValues<AudioFormat>().ToDictionary(e => '.' + e.ToString().ToLowerInvariant(), e => e);

    public string FullName { get; } = FullName;
    public Stream Stream { get; } = new FileStream(FullName, FileMode.Open, FileAccess.Read);
    public AudioFormat GuessedType { get; } = GuessTypeFromName(FullName);

    public void Dispose()
    {
        Stream.Dispose();
    }
    // public async ValueTask DisposeAsync() => await Stream.Dispose();

    /// <summary>
    ///     Guesses the audio file type from the given name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static AudioFormat GuessTypeFromName(string name)
    {
        var ext = Path.GetExtension(name).ToLowerInvariant();
        return SupportedExtensions.TryGetValue(ext, out var ret) ? ret : AudioFormat.UNKNOWN;
    }

    private static TEnum[] GetValues<TEnum>() where TEnum : Enum
    {
        return (TEnum[])Enum.GetValues(typeof(TEnum));
    }
}