using System;
using System.Windows;
using HtmlViewer.ViewModels;
using HtmlViewer.Views;

namespace HtmlViewer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App 
{
    //private const string BrowserCache = "CefSharp\\Cache";
    //private const string LogFilename = "Chromium.log";

    private readonly Window _mainWindow;

    public App()
    {
        _mainWindow = new MainWindow();
        var appPath = AppDomain.CurrentDomain.BaseDirectory;
//        var settings = new CefSettings
//        {
//            //By default CefSharp will use an in-memory cache, you need to specify a Cache Folder to persist data
//            CachePath = Path.Combine(appPath, BrowserCache),
//            LogFile = Path.Combine(appPath, LogFilename),
            
//#if DEBUG
//            LogSeverity = LogSeverity.Warning
//#else
//            LogSeverity = LogSeverity.Default
//#endif
//        };
//        if (!Cef.IsInitialized) 
//            //Perform dependency check to make sure all relevant resources are in our output directory.
//            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
    }

    private void OnStartup(object sender, StartupEventArgs e)
    {
        _mainWindow.DataContext = new MainVm();
        _mainWindow.Show();
    }

    private void OnExit(object sender, ExitEventArgs e)
    {
        //Cef.Shutdown();
    }

}