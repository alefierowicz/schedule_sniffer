using AngleSharp;
using AngleSharp.Dom;
class Program
{

    public static async Task<Uri> GetLatestScheduleURI(HttpClient http, string url)
    {
        IBrowsingContext context = BrowsingContext.New(Configuration.Default);
        using HttpResponseMessage response = await http.GetAsync(url);

        response.EnsureSuccessStatusCode();

        var source = await response.Content.ReadAsStringAsync();

        IDocument doc = await context.OpenAsync(r => r.Content(source));

        var target = doc.QuerySelector("a[href*='/isi5.pdf']")
            ?? throw new InvalidOperationException("Schedule link not found on page.");

        string href = target.GetAttribute("href")
            ?? throw new InvalidOperationException("Schedule link has no href.");
        Uri uri = new(http.BaseAddress!, href);
        // Console.WriteLine(uri);
        return uri;
    }
    public static async Task<int> Main()
    {
        HttpClient http = new()
        {
            BaseAddress = new Uri("https://degra.wi.pb.edu.pl/rozklady/"),
            Timeout = TimeSpan.FromSeconds(15)
        };
        try
        {

            Uri pdf_uri = await GetLatestScheduleURI(http, "rozklad.php?page=st");
            using var response = await http.SendAsync(new HttpRequestMessage(HttpMethod.Head, pdf_uri));

            response.EnsureSuccessStatusCode();
            DateTimeOffset? latest_edit_offset;

            if (response.Content.Headers.LastModified is not null)
            {
                latest_edit_offset = response.Content.Headers.LastModified;
                Console.WriteLine(latest_edit_offset);
            }
            else
            {
                latest_edit_offset = null;
                Console.WriteLine("Server did not respond with a LastModified header");
            }
            return 0;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return 1;
        }
    }
}
// TODO: writing to sqlite.