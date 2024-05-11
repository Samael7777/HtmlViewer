using System.Threading.Tasks;
using HtmlViewer.Models;

namespace HtmlViewer.Interfaces;

public interface IDownloader
{
    public string Content { get; }
    public Task<ContentLoadStatus> GetContentAsync(string source);
}
