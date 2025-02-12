using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Fushigi.Data;
using Fushigi.Data.Files;
using Fushigi.Data.RomFSExtensions;
using Fushigi.Logic;

namespace Fushigi.GUI;

public partial class MainWindow : Window, IRomFSLoadingErrorHandler
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

    private class LoadWorldmapAndCourseErrorHandler : IGymlFileLoadingErrorHandler
    {
        public Task OnFileNotFound(FilePathResolutionErrorInfo info)
            => throw new System.NotImplementedException();
        
        public Task OnFileDecompressionFailed(FileDecompressionErrorInfo info) 
            => throw new System.NotImplementedException();

        public Task OnFileReadFailed(FileFormatReaderErrorInfo info)
            => throw new System.NotImplementedException();

        public Task OnInvalidFileRefPath(InvalidFileRefPathErrorInfo info)
            => throw new System.NotImplementedException();

        public Task OnGymlTypeMismatch(LoadedGymlTypeMismatchErrorInfo info)
            => throw new System.NotImplementedException();

        public Task OnCyclicInheritance(CyclicInheritanceErrorInfo info)
            => throw new System.NotImplementedException();
    }

    private async void Open_OnClick(object? sender, RoutedEventArgs e)
    {
        var errorHandler = new LoadWorldmapAndCourseErrorHandler();
        //this is just for testing purposes
        if (await _loadedGame!.LoadWorldList(errorHandler) 
            is not (true, { } worldList)) return;
        
        if (await worldList!.Worlds[0].LoadCourse(0, errorHandler) 
            is not (true, { } course)) return;
        
    }
    
    public async void LoadGame(string baseGameRomFSPath, string? modRomFSPath)
    {
        if (await Game.Load(baseGameRomFSPath, modRomFSPath,
                errorHandler: this) is (true, { } loadedGame))
        {
            _loadedGame = loadedGame;
            await ShowSimpleDialog("Success", "Successfully loaded RomFS");
        }
    }

    public Task OnBaseGameAndModPathsIdentical() =>
        ShowSimpleDialog("Error", "Basegame and Mod romFS paths cannot be the same.");

    public Task OnRootDirectoryNotFound(RootDirectoryNotFoundErrorInfo e)
    {
        Debug.Fail("Should never happen");
        return Task.CompletedTask;
    }

    public Task OnMissingSubDirectory(MissingSubDirectoryErrorInfo e) =>
        ShowSimpleDialog("Invalid RomFS", $"{e.RootDirectory} is missing a {e.SubDirectory} directory.");

    public Task OnMissingSystemFile(MissingSystemFileErrorInfo e) =>
        ShowSimpleDialog($"Missing System File {e.Kind}",
            e.DirectoryExists
                ? $"File like {e.FileNamePattern} not found in {e.Directory}."
                : $"Directory {e.Directory} does not exist");

    public Task OnFileDecompressionFailed(FileDecompressionErrorInfo e) =>
        ShowSimpleDialog($"Failed to decompress {Path.GetFileName(e.FilePathFS)}",
            $"{e.InternalException.GetType().Name}: {e.InternalException.Message}");

    public Task OnFileReadFailed(FileFormatReaderErrorInfo e) => 
        ShowSimpleDialog($"Failed to read {e.FilePath[^1]}",
            $"{e.InternalException.GetType().Name}: {e.InternalException.Message}");
}