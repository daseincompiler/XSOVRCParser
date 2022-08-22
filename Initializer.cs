using System.Runtime.InteropServices;
using System.Text;
using XSOVRCParser.Helpers;

namespace XSOVRCParser;

internal class Initializer
{
    public static DateTime StartUpDateTime;

    public AutoResetEvent? AutoResetEvent;

    public Initializer()
    {
        try
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

            if (_lastWrittenFile == null) throw new NullReferenceException("LastWrittenFile FileInfo is null");

            if (VRCDirectory == null) throw new NullReferenceException("VRCDirectory DirectoryInfo is null");

            StartUpDateTime = DateTime.Now;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"XSOVRCParser by abbey has initialized at {StartUpDateTime}");
        }
        catch (Exception e)
        {
            throw new Exception(e.ToString());
        }
    }

    private FileInfo _lastWrittenFile;
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
            _lastWrittenFile = files.OrderByDescending(f => f.LastWriteTime)
                .First();
            Console.WriteLine("Latest written file: " + _lastWrittenFile.Name);
        }
        catch (Exception e)
        {
            throw new Exception("An exception occurred while trying to verify VRChat's directory", e);
        }
    }

    public void CreateFileStream()
    {
        var fs = new FileStream(_lastWrittenFile.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var sr = new StreamReader(fs);
        {
            while (true)
            {
                var s = sr.ReadLine();
                if (s != null)
                {
                    var inputType = XSOLog.GetInputType(s.Trim(), out var outputToPrint);
                    XSOLog.ConsoleWrite(inputType, outputToPrint);
                    VRCEvents.GetEvents(outputToPrint);
                }
                else
                    AutoResetEvent?.WaitOne(1000);
            }
        }
        // ReSharper disable once FunctionNeverReturns
    }

    public void WatchFolder()
    {
        var files = VRCDirectory.GetFiles();

        //https://stackoverflow.com/a/1179987
        var lastWritten = (from f in files
            orderby f.LastWriteTime descending
            select f).First();

        if (lastWritten.Name == _lastWrittenFile.Name) return;

        _lastWrittenFile = files.OrderByDescending(f => f.LastWriteTime)
            .First();
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Discovered new written file: " + _lastWrittenFile.Name);

        CreateFileStream();
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