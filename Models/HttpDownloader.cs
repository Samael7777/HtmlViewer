using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlViewer.Interfaces;

namespace HtmlViewer.Models;

public class HttpDownloader : IWebDownloader
{
    private const string DefaultAgent =
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.15; rv:101.0) Gecko/20100101 Firefox/101.0";
    public HttpDownloader()
    {
        Content = "";
    }

    public WebProxy? Proxy { get; set; }
    public string Content { get; private set; }

    public async Task<ContentLoadStatus> GetContentAsync(string address)
    {   
        Content = "";
        ContentLoadStatus result;
        
        try
        {
            var builder = new UriBuilder(address);
            var url = builder.Uri;
            var httpClientHandler = new HttpClientHandler();

            if (Proxy != null)
                httpClientHandler.Proxy = Proxy;

            //Disable SSL verification
            httpClientHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            using var client = new HttpClient(httpClientHandler, true);
            client.DefaultRequestHeaders.Add("User-Agent", DefaultAgent);

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
                Content = await response.Content.ReadAsStringAsync();

            result = response.IsSuccessStatusCode 
                ? ContentLoadStatus.Loaded 
                : ContentLoadStatus.NotFound;
        }
        catch (Exception)
        {
            result = ContentLoadStatus.LoadError;
        }

        return result;
    }
}