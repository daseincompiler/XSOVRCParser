using XSOVRCParser;

var pIn = new Initializer();

VRCEventHandler.AssignEvents();

pIn.AutoResetEvent = new AutoResetEvent(false);

var fsw = new FileSystemWatcher(pIn.VRCDirectory.FullName);
fsw.EnableRaisingEvents = true;
fsw.Changed += (_, _) =>
{
    pIn.AutoResetEvent.Set();
    pIn.WatchFolder();
};

pIn.CreateFileStream();