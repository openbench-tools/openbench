using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ModbusSim.App.ViewModels;

namespace ModbusSim.App.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void OnExportCsv(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
            return;

        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export traffic log",
            SuggestedFileName = $"modbus-log-{DateTime.Now:yyyyMMdd-HHmmss}.csv",
            DefaultExtension = "csv",
            FileTypeChoices = [new FilePickerFileType("CSV") { Patterns = ["*.csv"] }],
        });

        if (file is null)
            return;

        try
        {
            await vm.ExportLogAsync(file.Path.LocalPath);
        }
        catch (Exception ex)
        {
            vm.StatusText = $"Export failed: {ex.Message}";
        }
    }
}
