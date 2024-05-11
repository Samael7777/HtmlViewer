using System.Net;

namespace HtmlViewer.Interfaces;

public interface IWebDownloader : IDownloader
{
    public WebProxy? Proxy { get; set; }
}
