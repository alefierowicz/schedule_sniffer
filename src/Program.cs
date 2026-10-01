using AngleSharp;
using AngleSharp.Dom;
class Program
{
    public static async Task<string> FetchSiteBody(HttpClient http, string url)
    {
        HttpResponseMessage response = await http.GetAsync(url);
        return await response.Content.ReadAsStringAsync();
    }
    public static async Task<Uri> GetURLFromBody(HttpClient http, IBrowsingContext context, string source)
    {
        IDocument doc = await context.OpenAsync(r => r.Content(source));

        var target = doc.QuerySelector("a[href*=\"/isi5.pdf\"]");
        string href = target.GetAttribute("href")!;
        string full_schedule_url = http.BaseAddress + href[2..];

        return new Uri(full_schedule_url);
    }
    public static async Task Main()
    {
        IBrowsingContext context = BrowsingContext.New(Configuration.Default);
        HttpClient http = new HttpClient
        {
            BaseAddress = new Uri("https://degra.wi.pb.edu.pl/rozklady/"),
            Timeout = TimeSpan.FromSeconds(15)
        };

        string source = await FetchSiteBody(http, "rozklad.php?page=st"); // todo make singleton
        Uri pdf_uri = await GetURLFromBody(http, context, source);

        System.Console.WriteLine(pdf_uri);
    }
}