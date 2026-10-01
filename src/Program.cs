using System.Collections;
using AngleSharp;
using AngleSharp.Attributes;
using AngleSharp.Dom;
class Program
{
    public static async Task<string> FetchSiteBody(string url)
    {
        HttpClient http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        HttpResponseMessage response = await http.GetAsync(url);
        return await response.Content.ReadAsStringAsync();

    }

    public static async Task Main()
    {
        string base_url = "https://degra.wi.pb.edu.pl";
        string source = await FetchSiteBody($"{base_url}/rozklady/rozklad.php?page=st"); // todo make singleton

        IBrowsingContext context = BrowsingContext.New(Configuration.Default);
        IDocument doc = await context.OpenAsync(r => r.Content(source));

        // no better way to select this specific element this i can figure out as of yet ;/
        IEnumerable target = doc.QuerySelector("a[href*=\"/isi5.pdf\"]")!.Attributes;
        Console.WriteLine(target);
    }
}