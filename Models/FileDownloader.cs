using System.IO;
using System.Threading.Tasks;
using HtmlViewer.Interfaces;

namespace HtmlViewer.Models;

public class FileDownloader : IDownloader
{
    public string Content { get; set; } = "";

    public async Task<ContentLoadStatus> GetContentAsync(string filename)
    {

        if (!File.Exists(filename))
            return ContentLoadStatus.NotFound;

        ContentLoadStatus result;
        try
        {
            using var sr = new StreamReader(filename);
            var content = await sr.ReadToEndAsync();
            Content = content;
            result = ContentLoadStatus.Loaded;
        }
        catch
        {
            result = ContentLoadStatus.LoadError;
        }

        return result;
    }

    public async Task<ContentLoadStatus> SaveToFileAsync(string filename)
    {
        ContentLoadStatus result;
        try
        {
            await using var sw = new StreamWriter(filename);
            await sw.WriteAsync(Content);
            await sw.FlushAsync();
            result = ContentLoadStatus.Saved;
        }
        catch
        {
            result = ContentLoadStatus.SaveError;
        }

        return result;
    }
}
