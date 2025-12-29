using System.Windows;
using ImageToWebPConverter.Core;
using ImageToWebPConverter.UI.Services;
using ImageToWebPConverter.UI.ViewModels;

namespace ImageToWebPConverter.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel(new DialogService(), new ImageConversionService());
    }
}