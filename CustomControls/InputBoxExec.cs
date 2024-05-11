using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace HtmlViewer.CustomControls;

public class InputBoxExec : TextBox
{
    public InputBoxExec()
    {
        PreviewKeyDown += OnPreviewKeyDown;
    }
        
    public static readonly DependencyProperty OnEnterCommandProperty = 
        DependencyProperty.Register(
            nameof(OnEnterCommand), 
            typeof(ICommand), 
            typeof(InputBoxExec), 
            new PropertyMetadata(default));
        
    public ICommand OnEnterCommand
    {
        get => (ICommand)GetValue(OnEnterCommandProperty);
        set => SetValue(OnEnterCommandProperty, value);
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;

        if (OnEnterCommand.CanExecute(null))
            OnEnterCommand.Execute(null);
    }
}