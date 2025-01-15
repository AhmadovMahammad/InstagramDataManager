using DataManager.Helper.Extension;
using DataManager.Model;
using OpenQA.Selenium;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace DataManager.Helper.Utility;
public class HttpRequestHandler
{
    private readonly HttpClient _httpClient;
    private string _userId = string.Empty;
    private readonly string _manageFollowersBaseUrl = "https://www.instagram.com/api/v1/friendships/";
    private readonly string _topSearchBaseUrl = "https://www.instagram.com/web/search/topsearch/?query=";

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
        var csrfToken = webDriver.Manage().Cookies.GetCookieNamed("csrftoken")?.Value ?? throw new InvalidOperationException("CSRF token not found.");

        _httpClient = new HttpClient(new HttpClientHandler { UseCookies = true, UseProxy = false })
        {
            Timeout = TimeSpan.FromSeconds(10),
        };

        _httpClient.DefaultRequestHeaders.Add("Cookie", cookies);
        _httpClient.DefaultRequestHeaders.Add("X-CSRFToken", csrfToken);
        //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", csrfToken);
        AddDefaultHeaders();
    }

    public string UsernameToSearch { get; set; } = string.Empty;

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
        _httpClient.DefaultRequestHeaders.Add("Referer", "https://www.instagram.com/");
        _httpClient.DefaultRequestHeaders.Add("Origin", "https://www.instagram.com");

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

        string url = $"{_manageFollowersBaseUrl}{_userId}/{endpoint}/?count={count}";

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
        _userId = string.IsNullOrEmpty(_userId) ? await FetchUserIdAsync(UsernameToSearch) : _userId;

        int counter = 1;
        string? maxId = null;
        bool hasNextPage = true;

        while (hasNextPage)
        {
            var url = ConstructUrl("following", maxId, count);
            (maxId, List<UserEntry> userEntries) = await FetchDataAsync(url);

            userEntries.ForEach(userEntry =>
            {
                userEntry.Id = counter++;
                result.Add(userEntry);
            });
            hasNextPage = maxId != null;
        }

        return result;
    }

    public async Task<HashSet<UserEntry>> FetchFollowersAsync(int count = 12)
    {
        var result = new HashSet<UserEntry>();
        _userId = string.IsNullOrEmpty(_userId) ? await FetchUserIdAsync(UsernameToSearch) : _userId;

        int counter = 1;
        string? maxId = null;
        bool hasNextPage = true;

        while (hasNextPage)
        {
            var url = ConstructUrl("followers", maxId, count);
            (maxId, List<UserEntry> userEntries) = await FetchDataAsync(url);

            userEntries.ForEach(userEntry =>
            {
                userEntry.Id = counter++;
                result.Add(userEntry);
            });
            hasNextPage = maxId != null;
        }

        return result;
    }

    private async Task<string> FetchUserIdAsync(string username)
    {
        string userId = string.Empty;

        try
        {
            string url = _topSearchBaseUrl + username;
            using var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            string jsonString = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(jsonString);

            if (jsonDocument.RootElement.TryGetProperty("users", out var usersArray) && usersArray.GetArrayLength() > 0)
            {
                var userElement = usersArray[0];
                userId = userElement.GetProperty("user").GetProperty("pk").GetString() ?? string.Empty;
            }
        }
        catch (Exception ex)
        {
            ex.LogException("Error fetching user ID");
        }

        return userId;
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
                        Username = user.GetProperty("username").GetString() ?? string.Empty,
                    });
                }
            }

            if (jsonDocument.RootElement.TryGetProperty("next_max_id", out var nextMaxIdElement)) maxId = nextMaxIdElement.GetString();
            else maxId = null;
        }
        catch (Exception ex)
        {
            ex.LogException("Error fetching data.");
        }

        return (maxId, result);
    }
}