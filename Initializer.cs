using System.Text;
using XSOVRCParser.Helpers;

namespace XSOVRCParser;

internal class Initializer
{
    private FileInfo _lastWrittenFile;
    private FileSystemWatcher _watcher;
    public AutoResetEvent? AutoResetEvent;
    public DirectoryInfo VRCDirectory;

    public Initializer(FileSystemWatcher watcher)
    {
        _watcher = watcher;
        Console.OutputEncoding = Encoding.UTF8;

        VerifyDirectory();

        if (_lastWrittenFile == null) throw new NullReferenceException("LastWrittenFile FileInfo is null");
        if (VRCDirectory == null) throw new NullReferenceException("VRCDirectory DirectoryInfo is null");

        var startUpDateTime = DateTime.Now;

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"XSOVRCParser by abbey has initialized at {startUpDateTime}");

        SetupWatcher();
    }

    private void SetupWatcher()
    {
        _watcher = new FileSystemWatcher(VRCDirectory.FullName)
        {
            NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName |
                           NotifyFilters.DirectoryName,
            Filter = "*.*"
        };
        _watcher.Created += OnNewLogFileDetected;
        _watcher.EnableRaisingEvents = true;
    }

    private void OnNewLogFileDetected(object source, FileSystemEventArgs e)
    {
        if (e.ChangeType != WatcherChangeTypes.Created) return;

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"New log detected: {e.FullPath} -> {e.ChangeType}");
        _lastWrittenFile = new FileInfo(e.FullPath);
        CreateFileStream();
    }

    private void VerifyDirectory()
    {
        Console.ForegroundColor = ConsoleColor.White;
        try
        {
            VRCDirectory = GetVRChatDirectory();
            _lastWrittenFile = GetLatestFile(VRCDirectory);

            Console.WriteLine("Latest written file: " + _lastWrittenFile.Name);
        }
        catch (Exception e)
        {
            throw new Exception("An exception occurred while trying to verify VRChat's directory", e);
        }
    }

    private static FileInfo GetLatestFile(DirectoryInfo directory)
    {
        return directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
    }

    public void CreateFileStream()
    {
        try
        {
            using var fs = new FileStream(_lastWrittenFile.FullName, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite);
            using var sr = new StreamReader(fs);

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
                {
                    AutoResetEvent?.WaitOne(1000);
                }
            }
        }
        catch (IOException)
        {
            // Handle the exception here, for instance:
            Console.WriteLine("File is locked. Waiting and retrying...");
            Thread.Sleep(5000); // Wait for 5 seconds
            CreateFileStream(); // Recursive call (use with caution, consider setting a retry limit)
        }
    }


    public void WatchFolder()
    {
        var lastWritten = GetLatestFile(VRCDirectory);

        if (lastWritten.Name == _lastWrittenFile.Name) return;

        _lastWrittenFile = lastWritten;
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Discovered new written file: " + _lastWrittenFile.Name);

        CreateFileStream();
    }

    private static DirectoryInfo GetVRChatDirectory()
    {
        var localLowPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            .Replace("Roaming", "LocalLow");
        var vrchatDirectoryPath = Path.Combine(localLowPath, "VRChat", "VRChat");
        var vrchatDirectoryInfo = new DirectoryInfo(vrchatDirectoryPath);

        Console.WriteLine("Found VRChat directory path: " + vrchatDirectoryInfo);

        if (!vrchatDirectoryInfo.Exists)
            throw new DirectoryNotFoundException("VRChat directory couldn't be found or doesn't exist.");

        return vrchatDirectoryInfo;
    }
}