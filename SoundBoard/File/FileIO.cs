using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SoundBoard.File;

public static class FileIO
{
    public static void OpenFolder(string folderPath)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Under Windows, this will open Explorer.
            Process.Start("explorer.exe", folderPath);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // On Linux, use xdg-open to launch the native file manager.
            Process.Start("xdg-open", folderPath);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // On macOS, use the "open" command.
            Process.Start("open", folderPath);
        }
        else
        {
            throw new NotSupportedException("Unsupported OS platform");
        }
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