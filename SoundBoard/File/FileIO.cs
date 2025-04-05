using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace SoundBoard.File;

public static class FileIO
{
    public static void OpenFolder(string folderPath)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            // Under Windows, this will open Explorer.
            Application.OpenURL("file:///" + folderPath.Replace("\\", "/"));
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            // On Linux, use the file:// protocol
            Application.OpenURL("file://" + folderPath);
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            // On macOS, use the file:// protocol
            Application.OpenURL("file://" + folderPath);
        else
            throw new NotSupportedException("Unsupported OS platform");
    }

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