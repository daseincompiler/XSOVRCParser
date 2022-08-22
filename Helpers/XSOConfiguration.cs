namespace XSOVRCParser.Helpers;

internal class XSOConfiguration
{
    public string JoinedInstanceIconPath { get; }
    public string PlayerJoinedInstancePath { get; }
    public string PlayerLeftIconPath { get; }

    public XSOConfiguration()
    {
        // I am really confused how do these work with mister bee's nuget package, so I need to ask him later..
        JoinedInstanceIconPath = @"";
        PlayerJoinedInstancePath = @"";
        PlayerLeftIconPath = @"";
    }
}