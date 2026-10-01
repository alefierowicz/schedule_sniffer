using System.Net;
using System.Runtime.Serialization;
using System.Xml.Schema;
using AngleSharp;
using AngleSharp.Attributes;
using AngleSharp.Dom;
class Program
{

    public static async Task<Uri> GetLatestScheduleURL(HttpClient http, string url)
    {
        IBrowsingContext context = BrowsingContext.New(Configuration.Default);
        HttpResponseMessage response = await http.GetAsync(url);
        var source = await response.Content.ReadAsStringAsync();

        IDocument doc = await context.OpenAsync(r => r.Content(source));

        var target = doc.QuerySelector("a[href*='/isi5.pdf']")
            ?? throw new InvalidOperationException("Schedule link not found on page.");

        string href = target.GetAttribute("href")
            ?? throw new InvalidOperationException("Schedule link has no href.");

        return new Uri(http.BaseAddress!, href[2..]);
    }
    public static async Task Main()
    {
        HttpClient http = new HttpClient
        {
            BaseAddress = new Uri("https://degra.wi.pb.edu.pl/rozklady/"),
            Timeout = TimeSpan.FromSeconds(15)
        };

        Uri pdf_uri = await GetLatestScheduleURL(http, "rozklad.php?page=st");

        // next step - making a head request to see last edit.
        var response = await http.SendAsync(new HttpRequestMessage(HttpMethod.Head, pdf_uri));
        DateTimeOffset? latest_edit_offset = (int)response.StatusCode switch
        {
            200 => response.Content.Headers.LastModified,
            _ => null
        };

        if (latest_edit_offset is null)
            throw new InvalidDataException("No headers received when making HEAD request to server.");

        System.Console.WriteLine(latest_edit_offset);
    }
}