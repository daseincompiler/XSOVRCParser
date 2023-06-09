using System.Text;
using System.Text.RegularExpressions;

namespace XSOVRCParser;

internal static partial class XSOLog
{
    public static readonly StringBuilder Log = new();

    private static string _logDateTime = null!, _errorDateTime = null!;

    private static readonly string[] IgnoredErrors = { "AmplitudeAPI", "cdp.cloud.unity3d.com", "[API]", "Curl error",
        "[AVProVideo]", "NewFeatureCallouts", "Failed to get texture", "Error auto blending a playable at slot",
        "Can't push runtime controller onto a null avatar animator", "could not use Station"};

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
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (inputType)
        {
            case InputType.Log:
                _logDateTime = LogRegex().Match(s).Groups[1].Value;
                break;

            // case InputType.Warning:
            //     _warningDateTime = Regex.Match(s, @"(.+\S) Warning").Groups[1].Value;
            //     break;

            case InputType.Error:
                _errorDateTime = ErrorRegex().Match(s).Groups[1].Value;
                break;
        }

        var match = ErrorMessageRegex().Match(s).Groups[1].Value;

        if (string.IsNullOrEmpty(match)) return;

        if (IgnoredErrors.Any(t => match.Contains(t)))
        {
            return;
        }

        PrintError(match);
    }

    public static void PrintLog(Match regexMatch, ConsoleColor consoleColor = ConsoleColor.Cyan)
    {
        if (regexMatch == null) throw new NullReferenceException("Regex Match is null");

        PrintLog(regexMatch.Value, consoleColor);
    }

    public static void PrintLog(string value, ConsoleColor consoleColor)
    {
        DateTime.TryParse(_logDateTime, out var dateTime);

        // //might be inaccurate maybe?
        // XSONotifications.ShouldNotify = Initializer.StartUpDateTime.Minute.Equals(dateTime.Minute);

        Log.AppendLine($"[{dateTime}/PrintLog]: {value}\n");

        Console.ForegroundColor = consoleColor;
        Console.WriteLine($"{dateTime} Log - {value}\n");
    }

    private static void PrintError(string text)
    {
        DateTime.TryParse(_errorDateTime, out var dateTime);

        Log.AppendLine($"[{dateTime}/PrintError]: {text}\n");

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{dateTime} Error - {text}\n");
    }

    [GeneratedRegex(@"(.+\S) Log")]
    private static partial Regex LogRegex();

    [GeneratedRegex(@"(.+\S) Error")]
    private static partial Regex ErrorRegex();

    [GeneratedRegex(@"Error *- *(.+)")]
    private static partial Regex ErrorMessageRegex();
}