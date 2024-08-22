using System.ComponentModel.DataAnnotations;
using Spectre.Console;

namespace TorSharp.BackgroundServices.UIService.AnsiConsoleExtensions.Components;

public class AnsiProgressView
{
    public record Settings
    {
        public char Fill { get; init; } = '#';
        [Range(0, 100)]
        public int ProgressBarMaxWidth { get; init; } = 100;
    }

    private readonly IAnsiConsole _console;
    private Settings _settings;
    private Grid _grid;

    public AnsiProgressView(IAnsiConsole console, Settings settings)
    {
        _console = console;
        _settings = settings;
        _grid = new Grid();
        _grid.AddColumn(new GridColumn().NoWrap());
        _grid.AddColumn(new GridColumn().NoWrap());
        _grid.AddColumn(new GridColumn().NoWrap().RightAligned());
    }

    public void Render()
    {
        _console.Write(_grid);
    }

    public AnsiProgressView AddProgressBar(string title, [Range(0, 100)] double progressPercentage)
    {
        var fillAmount = (int)double.Floor(_settings.ProgressBarMaxWidth / 100 * progressPercentage);
        var nonFillAmount = (int)double.Ceiling(_settings.ProgressBarMaxWidth - fillAmount);
        _grid.AddRow(title, $"{string.Join(string.Empty, Enumerable.Repeat(_settings.Fill, fillAmount))}{string.Join(string.Empty, Enumerable.Repeat('_', nonFillAmount))}", $"{progressPercentage}%");
        return this;
    }
}
