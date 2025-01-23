using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Platform.Storage;
using Fushigi.Data;
using Fushigi.Logic;

namespace Fushigi.GUI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
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

    private async void Open_OnClick(object? sender, RoutedEventArgs e)
    {
        //this is just for testing purposes
        var files = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select the basegame romfs folder",
        });
        
        if (files.Count == 0)
            return;

        string baseGameRomFSPath = files[0].TryGetLocalPath()!;
        Debug.Assert(baseGameRomFSPath != null);
        
        (bool baseGameValid, _, _) = RomFS.ValidateRomFS(baseGameRomFSPath, null);
        
        string? modRomFSPath = null;
        if (!baseGameValid)
            await ShowSimpleDialog("Info", "RomFS path is invalid.");
        else
        {
            files = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select the mod romfs folder (Optional)",
            });
            
            if (files.Count > 0)
            {
                modRomFSPath = files[0].TryGetLocalPath();
                Debug.Assert(modRomFSPath != null);
            }
            
            (_, bool modValid, bool sameDirectory) = RomFS.ValidateRomFS(baseGameRomFSPath, modRomFSPath);
            
            if (!modValid)
                await ShowSimpleDialog("Info", "mod RomFS path is invalid. This should never happen.");
            else if (sameDirectory)
                await ShowSimpleDialog("Info", "mod and basegame RomFS paths are the same.");
        }
        
        // ReSharper disable once InvertIf
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
                            $"Directory {e.Directory} does not exist"))
            is (true, { } loadedGame))
        {
            _loadedGame = loadedGame;
            await ShowSimpleDialog("Success", "Successfully loaded RomFS");
        }
    }
}