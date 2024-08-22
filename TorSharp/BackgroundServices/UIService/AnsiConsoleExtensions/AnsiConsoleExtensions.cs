using Spectre.Console;
using TorSharp.BackgroundServices.UIService.AnsiConsoleExtensions.Components;

namespace TorSharp.BackgroundServices.UIService.AnsiConsoleExtensions;

public static class AnsiConsoleExtensions
{
    public static AnsiProgressView progressView(this IAnsiConsole console, AnsiProgressView.Settings? settings = null)
    {
        settings ??= new AnsiProgressView.Settings();
        return new AnsiProgressView(console, settings);
    }
}