using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using HtmlViewer.Interfaces;
using HtmlViewer.Models;
using Ookii.Dialogs.Wpf;

namespace HtmlViewer.ViewModels;

public class MainVm : BaseVm
{
    private INode? _selectedNode;
    private IHtmlDocument? _document;
    private string _content;

    private string _siteUrl;
    private string _nodeContent;
    private string _nodesPredicate;
    private string _xpath;
    private string _status;
    private Brush _statusColor;
    private bool _isText;
    private Cursor _mainCursor;
    private bool _useChromeBrowser;
    private bool _isProxyEnabled;
    private int _proxySelectedIndex;
    private string _proxyHost;
    private string _proxyPort;

    public MainVm()
    {
        _siteUrl = "";
        _nodeContent = "";
        _nodesPredicate = "*";
        _xpath = "";
        _selectedNode = null;
        _isText = false;
        _status = "";
        _statusColor = Brushes.Black;
        _mainCursor = Cursors.Arrow;
        _document = null;
        _content = "";
        _useChromeBrowser = false;
        _proxyHost = "";
        _proxyPort = "";
        _isProxyEnabled = false;
        _proxySelectedIndex = 0;
        InitializeCommands();
        InitializeProxyTypes();
        SetPageStatus(ContentLoadStatus.NotLoaded);
        SetBusyStatus(false);

        //todo test
        ProxyHost = "localhost";
        ProxyPort = "9050";
        SetProxy(0);
    }

    #region Properties

    public Cursor MainCursor
    {
        get => _mainCursor;
        set => SetField(ref _mainCursor, value);
    }
    public bool IsText
    {
        get => _isText;
        set
        {
            SetField(ref _isText, value);
            ShowNodeInfo(_selectedNode);
        }
    }
    public bool IsHtml
    {
        get => !_isText;
        set
        {
            SetField(ref _isText, !value);
            ShowNodeInfo(_selectedNode);
        }
    }
    public string Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }
    public Brush StatusColor
    {
        get => _statusColor;
        set => SetField(ref _statusColor, value);
    }
    public string SiteUrl
    {
        get => _siteUrl;
        set => SetField(ref _siteUrl, value);
    }
    public string NodeContent
    {
        get => _nodeContent;
        set => SetField(ref _nodeContent, value);
    }
    public string NodesPredicate
    {
        get => _nodesPredicate;
        set => SetField(ref _nodesPredicate, value.Trim());
    }
    public string Selector
    {
        get => _xpath;
        set => SetField(ref _xpath, value);
    }
    public bool UseChromeBrowser
    {
        get => _useChromeBrowser;
        set => SetField(ref _useChromeBrowser, value);
    }
    public string ProxyHost
    {
        get => _proxyHost;
        set => SetField(ref _proxyHost, value);
    }
    public string ProxyPort
    {
        get => _proxyPort;
        set => SetField(ref _proxyPort, value);
    }
    public bool IsProxyEnabled
    {
        get => _isProxyEnabled;
        set => SetField(ref _isProxyEnabled, value);
    }
    public int ProxySelectedIndex
    {
        get => _proxySelectedIndex;
        set => SetField(ref _proxySelectedIndex, value);
    }
    public ObservableCollection<IElement> RootNode { get; } = new();
    public ObservableCollection<IAttr> Attributes { get; } = new();
    public ObservableCollection<string> ProxyTypes { get; } = new();

    #endregion

    #region Commands
    public RelayCommand GetPageCmd { get; private set; }
    public RelayCommand GetNodesCmd { get; private set; }
    public RelayCommand ShowNodeInfoCmd { get; private set; }
    public RelayCommand SaveToFileCmd { get; private set; }
    public RelayCommand LoadFromFileCmd { get; private set; }
    public RelayCommand SetProxyCmd { get; private set; }
    public RelayCommand ExitCmd { get; private set; }

    [MemberNotNull(nameof(GetNodesCmd), nameof(GetPageCmd), nameof(ShowNodeInfoCmd),
        nameof(LoadFromFileCmd), nameof(SaveToFileCmd), nameof(SetProxyCmd), nameof(ExitCmd))]
    private void InitializeCommands()
    {
        GetPageCmd = new RelayCommand(async _ => await GetPageAsync());
        GetNodesCmd = new RelayCommand(_ => GetNodesTree());
        ShowNodeInfoCmd = new RelayCommand(ShowNodeInfo);
        LoadFromFileCmd = new RelayCommand(async _ => await LoadFromFile());
        SaveToFileCmd = new RelayCommand(async _ => await SaveToFile());
        SetProxyCmd = new RelayCommand(SetProxy);
        ExitCmd = new RelayCommand(_ => Application.Current.Shutdown());
    }

    #endregion
    
    private void InitializeProxyTypes()
    {
        ProxyTypes.Add("(нет)");
        ProxyTypes.Add("HTTP");
        ProxyTypes.Add("HTTPS");
        ProxyTypes.Add("SOCKS4");
        ProxyTypes.Add("SOCKS5");
        ProxySelectedIndex = 0;
    }

    private void SetProxy(object? param)
    {
        if (param is not int index) return;
        
        ProxySelectedIndex = index;
        IsProxyEnabled = index > 0;
    }

    private async Task SaveToFile()
    {
        if (string.IsNullOrWhiteSpace(_content))
        {
            ShowErrorBox("Нет загруженного контента.");
            return;
        }

        var downloader = new FileDownloader
        {
            Content = _content
        };

        var fileDialog = new VistaSaveFileDialog
        {
            Title = "Сохранить контент как...",
            AddExtension = true,
            DefaultExt = "html",
            Filter = "Веб-страница (*.html)|*.html|Все файлы (*.*)|*.*",
            OverwritePrompt = true,
            CheckPathExists = true,
            FileName = AppDomain.CurrentDomain.BaseDirectory
        };

        if (!fileDialog.ShowDialog() ?? false) return;
        
        var filename = fileDialog.FileName;

        SetPageStatus(ContentLoadStatus.Saving);
        SetBusyStatus(true);
        var status = await downloader.SaveToFileAsync(filename);
        SetBusyStatus(false);
        SetPageStatus(status);
    }

    private Task LoadFromFile()
    {
        var downloader = new FileDownloader();
        
        var fileDialog = new VistaOpenFileDialog
        {
            DefaultExt = "html",
            AddExtension = true,
            CheckPathExists = true,
            Filter = "Веб-страница (*.html)|*.html|Все файлы (*.*)|*.*",
            FileName = AppDomain.CurrentDomain.BaseDirectory
        };
        if (!fileDialog.ShowDialog() ?? false) 
            return new Task(() => { });
        
        var filename = fileDialog.FileName;
        
        var uri = new Uri(filename);
        SiteUrl = uri.AbsoluteUri;
        return GetContentAsync(downloader, filename);
    }

    private Task GetPageAsync()
    {
        var downloader = new HttpDownloader();
        //IWebDownloader downloader = UseChromeBrowser
        //    ? new ChromeDownloader()
        //    : new HttpDownloader();

        if (IsProxyEnabled)
        {
            if (!IsProxyInfoValid()) 
                return new Task(() => { });


            var proxyBuilder = new UriBuilder()
            {
                Scheme = ProxyTypes[ProxySelectedIndex].ToLower(),
                Host = ProxyHost,
                Port = int.Parse(ProxyPort)
            };
            var proxy = new WebProxy(proxyBuilder.Uri)
            {
                BypassProxyOnLocal = true
            };
            downloader.Proxy = proxy;
        }

        return GetContentAsync(downloader, SiteUrl);
    }

    private async Task GetContentAsync(IDownloader downloader, string source)
    {
        NodesPredicate = "";

        _document = null;

        SetBusyStatus(true);
        SetPageStatus(ContentLoadStatus.Loading);
        var status = await downloader.GetContentAsync(source);
        SetPageStatus(status);
        SetBusyStatus(false);

        if (status == ContentLoadStatus.Loaded)
        {
            _content = downloader.Content;
            await LoadHtmlDocumentAsync(downloader.Content);
        }
        
        GetNodesTree();
    }

    private async Task LoadHtmlDocumentAsync(string content)
    {
        var parser = new HtmlParser();
        _document = await parser.ParseDocumentAsync(content);
        var prettifier = new Prettifier(_document);
        await prettifier.Prettify();
    }

    private void GetNodesTree()
    {
        RootNode.Clear();
        if (_document == null) return;
        if (string.IsNullOrWhiteSpace(NodesPredicate))
        {
            var root = _document.DocumentElement;
            RootNode.Add(root);
            return;
        }

        try
        {
            var selectedNodes = _document.QuerySelectorAll(NodesPredicate);
            foreach (var node in selectedNodes)
            {
                RootNode.Add(node);
            }
        }
        catch (DomException e)
        {
            ShowErrorBox(e.Message);
        }
    }

    private void ShowNodeInfo(object? arg)
    {
        if (arg is not IElement node) return;
        if (_selectedNode == null || !node.Equals(_selectedNode))
            _selectedNode = node;

        Selector = node.GetSelector();
        
        Attributes.Clear();
        foreach (var attribute in node.Attributes)
        {
            Attributes.Add(attribute);
        }

        var content = IsHtml? node.OuterHtml : node.TextContent;
        NodeContent = content.Trim();
    }

    private void SetBusyStatus(bool status)
    {
        MainCursor = status ? Cursors.Wait : Cursors.Arrow;
    }

    private void SetPageStatus(ContentLoadStatus status)
    {
        switch (status)
        {
            case ContentLoadStatus.NotLoaded:
                StatusColor = Brushes.Blue;
                Status = "Страница не загружена";
                break;
            case ContentLoadStatus.Loading:
                StatusColor = Brushes.DarkGoldenrod;
                Status = "Загрузка страницы...";
                break;
            case ContentLoadStatus.Loaded:
                StatusColor = Brushes.Green;
                Status = "Страница успешно загружена";
                break;
            case ContentLoadStatus.NotFound:
                StatusColor = Brushes.Red;
                Status = "Страница не найдена";
                break;
            case ContentLoadStatus.LoadError:
                StatusColor = Brushes.Red;
                Status = "Ошибка загрузки страницы";
                break;
            case ContentLoadStatus.Saving:
                StatusColor = Brushes.DarkGoldenrod;
                Status = "Сохранение контента...";
                break;
            case ContentLoadStatus.Saved:
                StatusColor = Brushes.Green;
                Status = "Контент сохранен в файл";
                break;
            case ContentLoadStatus.SaveError:
                StatusColor = Brushes.Red;
                Status = "Ошибка сохранения контента";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }
    }

    private bool IsProxyInfoValid()
    {
        if (string.IsNullOrWhiteSpace(ProxyHost))
        {
            ShowErrorBox("Не задан адрес прокси.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(ProxyPort) 
            || !int.TryParse(ProxyPort, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
        {
            ShowErrorBox("Неверно задан порт прокси.");
            return false;
        }

        return true;
    }

    private static void ShowErrorBox(string message)
    {
        MessageBox.Show(message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}