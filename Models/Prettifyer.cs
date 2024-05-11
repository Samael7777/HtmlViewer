using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using AngleSharp.Css;
using AngleSharp.Css.Parser;
using AngleSharp.Html;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Jsbeautifier;

namespace HtmlViewer.Models;

public class Prettifier
{
    private readonly IHtmlDocument _document;
    public Prettifier(IHtmlDocument document)
    {
        _document = document;
    }

    public async Task Prettify()
    {
        var nodes = _document.All;

       //foreach (var node in nodes)
        for (var i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];
            var content = node.InnerHtml;

            content = node.LocalName switch
            {
                "script" => PrettifyJs(content),
                "style" => await PrettifyCss(content),
                "html" => await PrettifyHtml(content),
                _ => content
            };

            node.InnerHtml = HttpUtility.HtmlDecode(content);
            foreach (var attribute in node.Attributes)
            {
                var value = attribute.Value;
                attribute.Value =  HttpUtility.UrlDecode(value);
            }
        }
    }
    
    private static async Task<string> PrettifyHtml(string content)
    {
        await using var sw = new StringWriter();
        var parser = new HtmlParser();
        var html = await parser.ParseDocumentAsync(content);
        html.ToHtml(sw, new PrettyMarkupFormatter()
        {
            NewLine = Environment.NewLine,
            Indentation = "  "
        });
        return sw.ToString();
    }

    private static async Task<string> PrettifyCss(string content)
    {
        await using var sw = new StringWriter();
        var parser = new CssParser();
        var styleSheet = await parser.ParseStyleSheetAsync(content);
        styleSheet.ToCss(sw, new PrettyStyleFormatter()
        {
            NewLine = Environment.NewLine,
            Indentation = "  "
        });
        return sw.ToString();
    }

    private static string PrettifyJs(string content)
    {
        var opts = new BeautifierOptions
        {
            BraceStyle = BraceStyle.Expand,
            KeepArrayIndentation = true,
            KeepFunctionIndentation = true,
            IndentWithTabs = false,
            IndentChar = ' ',
            IndentSize = 2
        };
        var jsb = new JsBeautifier(opts);
        return jsb.Beautify(content);
    }
}