using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Platform.Storage;
using Fushigi.Data;
using Fushigi.Logic;
using ReactiveUI;

namespace Fushigi.GUI
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            Width = 600;
            Height = 225;
            RomFSPath.Text = UserSettings.GetRomFSPath();
            ModRomFSPath.Text = UserSettings.GetModRomFSPath();
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            MinWidth = Width;
            MinHeight = Height;
            SizeToContent = SizeToContent.WidthAndHeight;
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            ModRomFSPath.Text = !string.IsNullOrEmpty(ModRomFSPath.Text) ? ModRomFSPath.Text:null;
            (bool baseGameValid, bool modValid, bool sameDirectory) = RomFS.ValidateRomFS(RomFSPath.Text ?? "", ModRomFSPath.Text);
            if (baseGameValid && !sameDirectory)
                UserSettings.SetRomFSPath(RomFSPath.Text);
            if (modValid && !sameDirectory)
                UserSettings.SetModRomFSPath(ModRomFSPath.Text);
            base.OnClosing(e);
        }

        // new public void Close(object? dialogResult)
        // {
        //     this.DesiredSize_dialogResult = dialogResult;
        //     CloseCore(WindowCloseReason.WindowClosing, true, false);
        //     string[] paths = {RomFSPath.Text, ModRomFSPath.Text};
        //     Close(paths);
        // }

        private async void PickRomFSPath(object? sender, RoutedEventArgs e)
        {
            var files = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select the basegame romfs folder",
            });
            
            if (files.Count == 0)
                return;

            RomFSPath.Text = files[0].TryGetLocalPath()!;
            if (string.IsNullOrEmpty(ModRomFSPath.Text))
                ModRomFSPath.Text = RomFSPath.Text;
            Debug.Assert(RomFSPath.Text != null);
            
            (bool baseGameValid, _, bool sameDirectory) = RomFS.ValidateRomFS(RomFSPath.Text, ModRomFSPath.Text);
            
            if (!baseGameValid)
                await ShowSimpleDialog("Info", "RomFS path is invalid.");
            else if (sameDirectory)
                await ShowSimpleDialog("Info", "mod and basegame RomFS paths are the same.");
        }
        private async void PickModRomFSPath(object? sender, RoutedEventArgs e)
        {
            var files = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select the mod romfs folder",
            });
            
            if (files.Count > 0)
            {
                ModRomFSPath.Text = files[0].TryGetLocalPath();
                Debug.Assert(ModRomFSPath.Text != null);
            }

            (_, bool modValid, bool sameDirectory) = RomFS.ValidateRomFS(RomFSPath.Text ?? "", ModRomFSPath.Text);
            
            if (!modValid)
                await ShowSimpleDialog("Info", "mod RomFS path is invalid. This should never happen.");
            else if (sameDirectory)
                await ShowSimpleDialog("Info", "mod and basegame RomFS paths are the same.");
        }

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
        //<Button Content="ModFolder" Click="PickModdedRomFSPath" Grid.Row="1" Grid.Column="2"/>
        //<Button Content="Folder" Clcik="PickRomFSPath" Grid.Row="O" Grid.Column="2"/>
    }
}