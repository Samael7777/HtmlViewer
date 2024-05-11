/*
using System.Net;
using System;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.OffScreen;
using HtmlViewer.Interfaces;

namespace HtmlViewer.Models;

public class ChromeDownloader : IWebDownloader
{
    public ChromeDownloader()
    {
        Content = "";
        Proxy = null;
    }
    
    public WebProxy? Proxy { get; set; }
    public string Content { get; private set; }
    
    public async Task<ContentLoadStatus> GetContentAsync(string source)
    {
        ContentLoadStatus result;
        try
        {
            var builder = new UriBuilder(source);
            var url = builder.Uri;
            var requestContext = new RequestContext();

            if (Proxy != null)
                await SetProxy(requestContext, Proxy);
            
            var browser = new ChromiumWebBrowser(url.AbsoluteUri, requestContext:requestContext);
            var response = await browser.WaitForInitialLoadAsync();

            if (response.Success)
            {
                Content = await browser.GetSourceAsync();
                result = ContentLoadStatus.Loaded;
            }
            else
                result = ContentLoadStatus.NotFound;
        }
        catch (Exception)
        {
            result = ContentLoadStatus.LoadError;
        }

        return result;
    }

    private Task SetProxy(IRequestContext requestContext, WebProxy proxy)
    {
        return Cef.UIThreadTaskFactory.StartNew(() =>
        {
            var address = proxy.Address;
            if (address != null)
            {
                requestContext.SetProxy(address.Scheme, address.DnsSafeHost, address.Port, 
                    out _);
            }
        });
    }
}
*/