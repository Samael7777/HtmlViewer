using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;


namespace HtmlViewer.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
// ReSharper disable once RedundantExtendsListEntry
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ProxyPort_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        const string pattern = @"\D";
        var matches = Regex.Matches(pattern, e.Text);
        e.Handled = matches.Count > 0;
    }

    private void ProxyPort_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = !IsKeyNumericOrControl(e.Key);
    }

    private static bool IsKeyNumericOrControl(Key key)
    {
        return key switch
        {
            >= Key.D0 and <= Key.D9 => true,
            >= Key.NumPad0 and <= Key.NumPad9 => true,
            Key.Delete or Key.Back => true,
            Key.Left or Key.Right or Key.Up or Key.Down => true,
            _ => false
        };
    }
}