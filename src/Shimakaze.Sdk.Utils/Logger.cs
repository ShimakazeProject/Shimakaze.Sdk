using System.Diagnostics;
using System.Reflection;

using DebugLog = System.Diagnostics.Debug;

namespace Shimakaze.Sdk.Utils;

/// <summary>
/// Logger
/// </summary>
public static class Logger
{
    private const string COLORIZED_TEMPLATE = "[\x1b[38;2;0;0;255m{0}\x1b[0m]  {1}: {2}";
    private const string NO_COLORIZED_TEMPLATE = "[{0}]  {1}: {2}";

    /// <summary>
    /// Colorized Output Log.
    /// </summary>
    public static bool UseColor { get; set; }

    private static string ParseLog(this string sender, bool noColor, string message, params object?[]? args)
    {
        string msg = args is null || args.Length == 0 ? message : string.Format(message, args);
        return string.Format(noColor ? NO_COLORIZED_TEMPLATE : COLORIZED_TEMPLATE, DateTime.Now.ToString("O"), sender, msg);
    }

    /// <summary>
    /// Write a Log to Debug output.
    /// </summary>
    /// <param name="sender">Sender Name</param>
    /// <param name="message">Message</param>
    /// <param name="args">Format Args</param>
    [Conditional("DEBUG")]
    public static void Debug(string sender, string message, params object?[] args)
    {
        DebugLog.WriteLine(sender.ParseLog(true, message, args));
        Console.WriteLine(sender.ParseLog(UseColor, message, args));
    }

    /// <summary>
    /// Write a Log to output.
    /// </summary>
    /// <param name="sender">Sender Name</param>
    /// <param name="message">Message</param>
    /// <param name="args">Format Args</param>
    public static void Info(string sender, string message, params object?[] args)
    {
        Trace.WriteLine(sender.ParseLog(true, message, args));
        Console.WriteLine(sender.ParseLog(UseColor, message, args));
    }

    /// <summary>
    /// Write a Log to output.
    /// </summary>
    /// <param name="sender">Sender Name</param>
    /// <param name="message">Message</param>
    /// <param name="args">Format Args</param>
    public static void Warn(string sender, string message, params object?[] args)
    {
        Trace.WriteLine(sender.ParseLog(true, message, args));
        Console.Error.WriteLine(sender.ParseLog(UseColor, message, args));
    }

    /// <summary>
    /// Write a Log to output.
    /// </summary>
    /// <param name="sender">Sender Name</param>
    /// <param name="message">Message</param>
    /// <param name="args">Format Args</param>
    public static void Error(string sender, string message, params object?[] args)
    {
        Trace.WriteLine(sender.ParseLog(true, message, args));
        Console.Error.WriteLine(sender.ParseLog(UseColor, message, args));
    }

    /// <summary>
    /// Write a Log to output.
    /// </summary>
    /// <param name="sender">Sender Name</param>
    /// <param name="message">Message</param>
    /// <param name="args">Format Args</param>
    public static void Fatal(string sender, string message, params object?[] args)
    {
        Trace.WriteLine(sender.ParseLog(true, message, args));
        Console.Error.WriteLine(sender.ParseLog(UseColor, message, args));
    }
}
