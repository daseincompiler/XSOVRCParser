using System.Text;
using System.Text.RegularExpressions;

namespace XSOVRCParser;

internal static class XSOLog
{
    public static readonly StringBuilder Log = new();

    private static string _logDateTime = null!, _warningDateTime = null!, _errorDateTime = null!;

    public enum InputType
    {
        Log,
        Warning,
        Error
    }

    //https://stackoverflow.com/a/19436622 C++ would be probably way more ideal, but I don't have such skills yet...
    // TODO: fails to get anything that's more than one line because of the current regex
    public static InputType GetInputType(string text, out string toPrint)
    {
        if (text.Contains("Log"))
        {
            toPrint = text;
            return InputType.Log;
        }

        if (text.Contains("Warning"))
        {
            toPrint = text;
            return InputType.Warning;
        }

        if (text.Contains("Error"))
        {
            toPrint = text;
            return InputType.Error;
        }

        toPrint = string.Empty;
        return InputType.Log;
    }

    public static void ConsoleWrite(InputType inputType, string s)
    {
        switch (inputType)
        {
            case InputType.Log:
                _logDateTime = Regex.Match(s, @"(.+\S) Log").Groups[1].Value;
                break;

            case InputType.Warning:
                _warningDateTime = Regex.Match(s, @"(.+\S) Warning").Groups[1].Value;
                break;

            case InputType.Error:
                _errorDateTime = Regex.Match(s, @"(.+\S) Error").Groups[1].Value;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(inputType), inputType, "inputType is out of range");
        }

        var match = Regex.Match(s, @"Error *- *(.+)").Groups[1].Value;
        
        if (string.IsNullOrEmpty(match)) return;
        
        if (match.Contains("AmplitudeAPI") || match.Contains("cdp.cloud.unity3d.com") || match.Contains("[API]")) return;
        
        PrintError(match);
    }

    public static void PrintLog(Match regexMatch, ConsoleColor consoleColor = ConsoleColor.Cyan)
    {
        if (regexMatch == null) throw new NullReferenceException("Regex Match is null");

        PrintLog(regexMatch.Value, consoleColor);
    }

    public static void PrintLog(string value, ConsoleColor consoleColor)
    {
        if (!DateTime.TryParse(_logDateTime, out var dateTime))
            throw new Exception("Failed to parse logDateTime to dateTime");

        // //might be inaccurate maybe?
        // XSONotifications.ShouldNotify = Initializer.StartUpDateTime.Minute.Equals(dateTime.Minute);

        Log.AppendLine($"[{dateTime}/PrintLog]: {value}\n");

        Console.ForegroundColor = consoleColor;
        Console.WriteLine($"{dateTime} Log - {value}\n");
    }

    private static void PrintError(string text)
    {
        if (!DateTime.TryParse(_errorDateTime, out var dateTime))
            throw new Exception("Failed to parse errorDateTime to dateTime");

        Log.AppendLine($"[{dateTime}/PrintError]: {text}\n");

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{dateTime} Error - {text}\n");
    }

    private static void PrintWarning(string text)
    {
        if (!DateTime.TryParse(_warningDateTime, out var dateTime))
            throw new Exception("Failed to parse warningDateTime to dateTime");

        Log.AppendLine($"[{dateTime}/PrintWarning]: {text}\n");

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[{dateTime}] {text}\n");
    }
}