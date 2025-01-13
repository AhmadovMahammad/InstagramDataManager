using DataManager.Model;
using OpenQA.Selenium;
using System.Text;
using System.Text.Json;

namespace DataManager.Helper.Utility;
public class HttpRequestHandler
{
    private readonly HttpClient _httpClient;
    private readonly string _userId;
    private readonly string _csrfToken;

    private static readonly string _baseUrl = "https://www.instagram.com/api/v1/friendships/";

    private static readonly Dictionary<string, string> _defaultHeaders = new()
    {
        { "Accept", "*/*" },
        { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)" },
        { "X-IG-App-ID", "936619743392459" },
        { "X-Requested-With", "XMLHttpRequest" }
    };

    public HttpRequestHandler(IWebDriver webDriver)
    {
        var cookies = InitializeCookies(webDriver);
        _userId = webDriver.Manage().Cookies.GetCookieNamed("ds_user_id")?.Value ?? throw new InvalidOperationException("User ID cookie not found.");
        _csrfToken = webDriver.Manage().Cookies.GetCookieNamed("csrftoken")?.Value ?? throw new InvalidOperationException("CSRF token not found.");

        _httpClient = new HttpClient(new HttpClientHandler { UseCookies = true, UseProxy = false })
        {
            Timeout = TimeSpan.FromSeconds(10),
        };

        _httpClient.DefaultRequestHeaders.Add("Cookie", cookies);
        _httpClient.DefaultRequestHeaders.Add("X-CSRFToken", _csrfToken);
        AddDefaultHeaders();
    }

    private string InitializeCookies(IWebDriver webDriver)
    {
        var stringBuilder = new StringBuilder();
        foreach (var cookie in webDriver.Manage().Cookies.AllCookies)
        {
            stringBuilder.Append($"{cookie.Name}={cookie.Value}; ");
        }
        return stringBuilder.ToString();
    }

    private void AddDefaultHeaders()
    {
        foreach (var header in _defaultHeaders)
        {
            _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
        }
    }

    private string ConstructUrl(string endpoint, string? maxId = null, int count = 12)
    {
        // EndPoints - analyzed from Network
        // https://www.instagram.com/api/v1/friendships/user_id/following/?count=12
        // https://www.instagram.com/api/v1/friendships/user_id/following/?count=12&max_id=24
        // https://www.instagram.com/api/v1/friendships/user_id/followers/?count=12&search_surface=follow_list_page
        // https://www.instagram.com/api/v1/friendships/user_id/followers/?count=12&max_id=stringType&search_surface=follow_list_page

        string url = $"{_baseUrl}{_userId}/{endpoint}/?count={count}";

        if (endpoint.Equals("following", StringComparison.OrdinalIgnoreCase))
        {
            if (maxId != null)
                url += $"&max_id={maxId}";
        }
        else
        {
            if (maxId != null)
                url += $"&max_id={maxId}";

            url += "&search_surface=follow_list_page";
        }

        return url;
    }

    public async Task<HashSet<UserEntry>> FetchFollowingAsync(int count = 12)
    {
        var result = new HashSet<UserEntry>();
        string? maxId = null;
        bool hasNextPage = true;

        while (hasNextPage)
        {
            var url = ConstructUrl("following", maxId, count);
            (maxId, List<UserEntry> userEntries) = await FetchDataAsync(url);

            userEntries.ForEach(userEntry => result.Add(userEntry));
            hasNextPage = maxId != null;
        }

        return result;
    }

    public async Task<HashSet<UserEntry>> FetchFollowersAsync(int count = 12)
    {
        var result = new HashSet<UserEntry>();
        string? maxId = null;
        bool hasNextPage = true;

        while (hasNextPage)
        {
            var url = ConstructUrl("followers", maxId, count);
            (maxId, List<UserEntry> userEntries) = await FetchDataAsync(url);

            userEntries.ForEach(userEntry => result.Add(userEntry));
            hasNextPage = maxId != null;
        }

        return result;
    }

    private async Task<(string?, List<UserEntry>)> FetchDataAsync(string url)
    {
        var result = new List<UserEntry>();
        string? maxId = null;

        try
        {
            using var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            string jsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var jsonDocument = JsonDocument.Parse(jsonString);

            if (jsonDocument.RootElement.TryGetProperty("users", out JsonElement users))
            {
                foreach (JsonElement user in users.EnumerateArray())
                {
                    result.Add(new UserEntry
                    {
                        Identifier = user.GetProperty("pk").GetString() ?? string.Empty,
                        Username = user.GetProperty("username").GetString() ?? string.Empty,
                        Fullname = user.GetProperty("full_name").GetString() ?? string.Empty
                    });
                }
            }

            if (jsonDocument.RootElement.TryGetProperty("next_max_id", out var nextMaxIdElement)) maxId = nextMaxIdElement.GetString();
            else maxId = null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching data: {ex.Message}");
        }

        return (maxId, result);
    }
}