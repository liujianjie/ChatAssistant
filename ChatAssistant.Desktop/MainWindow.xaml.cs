using System.Windows;
using ChatAssistant.Desktop.ViewModels;

namespace ChatAssistant.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}