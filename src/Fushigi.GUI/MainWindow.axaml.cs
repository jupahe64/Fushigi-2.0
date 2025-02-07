using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Fushigi.Data;
using Fushigi.Logic;

namespace Fushigi.GUI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        UserSettings.Load();
        if (!string.IsNullOrEmpty(UserSettings.GetRomFSPath()))
        {
            LoadGame(UserSettings.GetRomFSPath(), UserSettings.GetModRomFSPath());
        }
    }
    
    private Game? _loadedGame;

    private async Task ShowSimpleDialog(string title, string message)
    {
        //this is just for testing purposes
        Button b;
        var w = new Window()
        {
            Title = title,
            Content = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Children =
                {
                    new TextBlock
                    {
                        Text = message,
                        Margin = new Thickness(10)
                    },
                    (b = new Button
                    {
                        Content = "OK"
                    })
                }
            },
            SizeToContent = SizeToContent.WidthAndHeight,
            MinWidth = 300,
        };
        b.Click += (_, _) => w.Close();
        await w.ShowDialog(this);
    }

    private async void ShowSettingsDialog(object? sender, RoutedEventArgs e)
    {
        var settingsDialog = new SettingsWindow();

        await settingsDialog.ShowDialog<string>(this);
        if (!string.IsNullOrEmpty(UserSettings.GetRomFSPath()))
        {
            LoadGame(UserSettings.GetRomFSPath(), UserSettings.GetModRomFSPath());
        }

        Debug.Write(settingsDialog.RomFSPath.Text);
    }

    private async void Open_OnClick(object? sender, RoutedEventArgs e)
    {
        //this is just for testing purposes
        var (success, area) = await _loadedGame.LoadCourse(
            ["BancMapUnit", "Course001_Course.bcett.byml.zs"],
            onFileNotFound: (info) => throw new(), 
            onFileDecompressionFailed: (info) => throw new(), 
            onFileReadFailed: (info) => throw new(),
            onGymlTypeMismatch: (info) => throw new(),
            onInvalidFileRefPath: (info) => throw new(),
            onCyclicInheritance: (info) => throw new()
            );
    }
    public async void LoadGame(string baseGameRomFSPath, string? modRomFSPath)
    {
        if (await Game.Load(baseGameRomFSPath, modRomFSPath,
                onBaseGameAndModPathsIdentical: () => 
                    ShowSimpleDialog("Error", "Basegame and Mod romFS paths cannot be the same."), 
                onRootDirectoryNotFound: _ => {
                    Debug.Fail("Should never happen");
                    return Task.CompletedTask;
                },
                onMissingSubDirectory: e => 
                    ShowSimpleDialog("Invalid RomFS", $"{e.RootDirectory} is missing a {e.SubDirectory} directory."), 
                onMissingSystemFile: e => 
                    ShowSimpleDialog($"Missing System File {e.Kind}", 
                        e.DirectoryExists ? $"File like {e.FileNamePattern} not found in {e.Directory}." : 
                            $"Directory {e.Directory} does not exist"),
                onFileDecompressionFailed: e => 
                    ShowSimpleDialog($"Failed to decompress {Path.GetFileName(e.FilePathFS)}", 
                        $"{e.InternalException.GetType().Name}: {e.InternalException.Message}"),
                onFileReadFailed: e => ShowSimpleDialog($"Failed to read {e.FilePath[^1]}", 
                    $"{e.InternalException.GetType().Name}: {e.InternalException.Message}")
            ) is (true, { } loadedGame))
        {
            _loadedGame = loadedGame;
            await ShowSimpleDialog("Success", "Successfully loaded RomFS");
        }
    }
}