using System.Reflection;

namespace SoundBoard.Helpers;

public static class DataHelper
{
    private static byte[] LoadEmbeddedResource(string resourceName)
    {
        // Get the assembly where the resource is embedded.
        var assembly = Assembly.GetExecutingAssembly();
        
        var names = assembly.GetManifestResourceNames();
        var stream = assembly.GetManifestResourceStream(names.First(n => n.EndsWith(resourceName)));
        var data = new byte[stream!.Length];
        _ = stream.Read(data, 0, data.Length);
        return data;
    }
}