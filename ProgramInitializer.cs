using System.Runtime.InteropServices;
using System.Text;

namespace XSOVRCParser;

internal class ProgramInitializer
{
    public ProgramInitializer()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            throw new PlatformNotSupportedException(
                "Linux is currently NOT supported, feel free to make a PR request if you are a linux user");
        }

        Console.OutputEncoding = Encoding.UTF8;

        // if (Process.GetProcessesByName("VRChat").Length == 0)
        // {
        //     throw new Exception("VRChat isn't running...., launch VRChat and launch this program again..");
        // }

        VerifyDirectory();

        if (LastWrittenFile == null) throw new NullReferenceException("LastWrittenFile FileInfo is null");

        if (VRCDirectory == null) throw new NullReferenceException("VRCDirectory DirectoryInfo is null");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[{DateTime.Now}] XSOVRCParser by abbey has initialized");
    }

    public FileInfo LastWrittenFile;
    public DirectoryInfo VRCDirectory;

    private void VerifyDirectory()
    {
        Console.ForegroundColor = ConsoleColor.White;
        try
        {
            VRCDirectory = GetVRChatDirectory();
            var files = VRCDirectory.GetFiles();
            Console.WriteLine($"Found files in total: {files.Length}");
            for (var i = 0; i < files.Length; i++)
            {
                Console.WriteLine($"File {i}: {files[i].Name}");
            }
            LastWrittenFile = files.OrderByDescending(f => f.LastWriteTime)
                .First();
            Console.WriteLine("Latest written file: " + LastWrittenFile.Name);
        }
        catch (Exception e)
        {
            throw new Exception("An exception occurred while trying to verify VRChat's directory", e);
        }
    }

    private static DirectoryInfo GetVRChatDirectory()
    {
        var localLowPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow");
        var vrchatDirectoryInfo = new DirectoryInfo(localLowPath + "\\VRChat\\VRChat");
        Console.WriteLine("Found VRChat directory path: " + vrchatDirectoryInfo);
        if (vrchatDirectoryInfo.Exists) return vrchatDirectoryInfo;
        throw new NullReferenceException("vrchat directory couldn't be found or doesn't exist");
    }
}