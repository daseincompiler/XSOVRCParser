using System.Text;
using System.Text.RegularExpressions;

namespace XSOVRCParser;

internal static partial class XSOLog
{
    public enum InputType
    {
        Log,
        Warning,
        Error
    }

    public static readonly StringBuilder Log = new();
    private static DateTime? _logDateTime, _errorDateTime;

    private static readonly string[] IgnoredErrors =
    {
        "AmplitudeAPI", "cdp.cloud.unity3d.com", "[API]", "Curl error",
        "[AVProVideo]", "NewFeatureCallouts", "Failed to get texture", "Error auto blending a playable at slot",
        "Can't push runtime controller onto a null avatar animator", "could not use Station",
        "Could not locate Station",
        "Material doesn't have a texture property", "Failed to find translation for term",
        "Failed to find translation for key",
        "Attempted to access IsInVRMode before tracking was initialized!"
    };

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
        DateTime tempDateTime; // Intermediate non-nullable DateTime variable

        switch (inputType)
        {
            case InputType.Log when DateTime.TryParse(LogRegex().Match(s).Groups[1].Value, out tempDateTime):
                _logDateTime = tempDateTime;
                break;

            case InputType.Error when DateTime.TryParse(ErrorRegex().Match(s).Groups[1].Value, out tempDateTime):
                _errorDateTime = tempDateTime;
                break;

            case InputType.Warning:
                break;
        }

        var errorMessage = ErrorMessageRegex().Match(s).Groups[1].Value;

        if (string.IsNullOrWhiteSpace(errorMessage) || IgnoredErrors.Any(t => errorMessage.Contains(t))) return;

        PrintError(errorMessage);
    }

    public static void PrintLog(Match regexMatch, ConsoleColor consoleColor = ConsoleColor.Cyan)
    {
        if (regexMatch == null) throw new NullReferenceException(nameof(regexMatch));
        PrintLog(regexMatch.Value, consoleColor);
    }

    public static void PrintLog(string value, ConsoleColor consoleColor = ConsoleColor.Cyan)
    {
        AppendToLog($"[{_logDateTime}/PrintLog]: {value}", consoleColor);
    }

    private static void PrintError(string text)
    {
        AppendToLog($"[{_errorDateTime}/PrintError]: {text}", ConsoleColor.Red);
    }

    private static void AppendToLog(string message, ConsoleColor color)
    {
        Log.AppendLine($"{message}\n");
        Console.ForegroundColor = color;
        Console.WriteLine($"{message}\n");
    }

    [GeneratedRegex(@"(.+\S) Log")]
    private static partial Regex LogRegex();

    [GeneratedRegex(@"(.+\S) Error")]
    private static partial Regex ErrorRegex();

    [GeneratedRegex(@"Error *- *(.+)")]
    private static partial Regex ErrorMessageRegex();
}