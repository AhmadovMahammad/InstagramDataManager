using DataManager.Constant;
using DataManager.Constant.Enums;
using DataManager.Helper.Extension;
using DataManager.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Text.Json;

namespace DataManager.Handler.ManageHandlers;

public class ManageFollowersHandler : BaseCommandHandler
{
    private const string FollowButtonXPath = "//div[contains(@class,'_ap3a') and normalize-space(text())='Follow']";
    private const string UsernameXPath = "//span[@class='_ap3a _aaco _aacw _aacx _aad7 _aade' and @dir='auto']";
    private const string NotAvailableNotificationXPath = "//span[@class='x1lliihq x1plvlek xryxfnj x1n2onr6' and @dir='auto' and normalize-space(text())='Sorry, this page isn't available.')]";

    private string _username = "";
    private int _followersCount = 0;
    private int _followingCount = 0;

    public override OperationType OperationType => OperationType.SeleniumBased;

    protected override void Execute(Dictionary<string, object> parameters)
    {
        if (!parameters.TryGetValue("WebDriver", out var webDriverObj) || webDriverObj is not IWebDriver webDriver)
            throw new ArgumentException("WebDriver is missing or invalid in parameters.");

        string username = AskForUsername();
        if (string.IsNullOrWhiteSpace(username)) return;

        if (!NavigateToProfile(webDriver, username))
        {
            "Unable to access the profile. Please make sure the profile is valid and accessible.".WriteMessage(MessageType.Warning);
            return;
        }

        if (!IsFollowingProfile(webDriver))
        {
            "You can only process profiles you are following. Please follow the profile and try again.".WriteMessage(MessageType.Error);
            return;
        }

        var userData = FetchUserData(webDriver, username);
        if (userData == null)
        {
            "Failed to fetch followers data.".WriteMessage(MessageType.Error);
            return;
        }

        HandleDataComparison(userData);
    }

    private string AskForUsername()
    {
        while (true)
        {
            Console.WriteLine("Enter the Username to process (or type 'exit' to quit):");
            Console.Write("> ");
            _username = Console.ReadLine()?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(_username) || _username.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                "Operation cancelled by user.".WriteMessage(MessageType.Warning);
                return string.Empty;
            }

            return _username;
        }
    }

    private bool NavigateToProfile(IWebDriver webDriver, string username)
    {
        try
        {
            string url = $"https://www.instagram.com/{username}";
            webDriver.Navigate().GoToUrl(url);
            WebDriverExtension.EnsureDomLoaded(webDriver);

            IWebElement? notAvailableNotification = webDriver.FindElementWithRetries(string.Empty, By.XPath(NotAvailableNotificationXPath), 1, logMessage: false);
            return notAvailableNotification is null;
        }
        catch (Exception ex)
        {
            ex.LogException("Failed to navigate to profile.");
            return false;
        }
    }

    private bool IsFollowingProfile(IWebDriver webDriver)
    {
        try
        {
            var followButton = webDriver.FindElementWithRetries("Follow Button", By.XPath(FollowButtonXPath), retries: 2, initialDelay: 1000);
            return followButton == null;
        }
        catch
        {
            return false;
        }
    }

    private UserData? FetchUserData(IWebDriver webDriver, string username)
    {
        try
        {
            (string followersXPath, string followingXPath) = InitializeData(webDriver, username);

            var followers = FetchList(webDriver, followersXPath, _followersCount, "Followers");
            var following = FetchList(webDriver, followingXPath, _followingCount, "Following");

            return new UserData(followers, following, DateTime.Now);
        }
        catch (Exception ex)
        {
            ex.LogException("Error fetching user data.");
            return null;
        }
    }

    private (string followersXPath, string followingXPath) InitializeData(IWebDriver webDriver, string username)
    {
        try
        {
            string followersXPath = $"//a[contains(@href,'/{username}/followers/')]/span/span[normalize-space(text())]";
            string followingXPath = $"//a[contains(@href,'/{username}/following/')]/span/span[normalize-space(text())]";

            _followersCount = int.Parse(webDriver.FindElement(By.XPath(followersXPath)).Text.Trim());
            _followingCount = int.Parse(webDriver.FindElement(By.XPath(followingXPath)).Text.Trim());

            new List<string>
            {
                username, _followersCount.ToString(), _followingCount.ToString()
            }.DisplayAsTable(null, "Username", "Followers", "Following");

            return (followersXPath, followingXPath);
        }
        catch (Exception ex)
        {
            ex.LogException("Failed to initialize data. Ensure the profile URL is valid and accessible.");
            throw;
        }
    }

    // todo: handle all users dynamically
    private HashSet<UserEntry> FetchList(IWebDriver webDriver, string itemXPath, int totalCount, string action)
    {
        var listItems = new HashSet<UserEntry>();
        int id = 1;

        try
        {
            var modalButton = webDriver.FindElementWithRetries("Modal Page Button", By.XPath(itemXPath), retries: 3, initialDelay: 1500);
            modalButton?.Click();

            var wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10));
            while (listItems.Count < totalCount)
            {
                // todo:
                wait.Until(driver =>
                {
                    var currentElements = driver.FindElements(By.XPath(UsernameXPath));
                    var newText = currentElements.Select(e => e.Text.Trim()).ToHashSet();
                    var previousText = listItems.Select(e => e.Username).ToHashSet();
                    return newText.Except(previousText).Any();
                });

                var currentElements = webDriver.FindElements(By.XPath(UsernameXPath));
                foreach (var element in currentElements)
                {
                    string username = element.Text.Trim();
                    listItems.Add(new UserEntry(id++, username));
                }

                var lastElement = currentElements.LastOrDefault();
                if (lastElement != null)
                {
                    webDriver.ScrollToElement(lastElement);
                    Task.Delay(1000).Wait();
                }
                else
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            ex.LogException($"Error occurred while fetching {action} list.");
        }

        return listItems;
    }

    private void HandleDataComparison(UserData currentData)
    {
        Console.WriteLine("Do you have previous data to compare? (y/n):");
        Console.Write("> ");
        bool hasPreviousData = Console.ReadLine()?.Trim().Equals("y", StringComparison.OrdinalIgnoreCase) ?? false;

        if (hasPreviousData)
        {
            Console.WriteLine("Enter the file path for previous data:");
            string filePath = Console.ReadLine()?.Trim() ?? string.Empty;

            if (File.Exists(filePath))
            {
                var previousData = LoadUserData(filePath);
                if (previousData != null)
                {
                    AnalyzeAndDisplayChanges(currentData, previousData);
                }
                else
                {
                    "Failed to load previous data.".WriteMessage(MessageType.Error);
                }
            }
            else
            {
                "File not found.".WriteMessage(MessageType.Error);
            }
        }
        else
        {
            if (TrySaveUserData(currentData, out string fileName))
            {
                $"Current followers data saved to {fileName}.".WriteMessage(MessageType.Success);
            }
        }
    }

    private bool TrySaveUserData(UserData userData, out string fileName)
    {
        fileName = Path.Combine(AppConstant.ApplicationDataFolderPath, $"followers_data_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json");

        try
        {
            using FileStream fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);
            using TextWriter writer = new StreamWriter(fileStream);
            var data = JsonSerializer.Serialize(userData);
            writer.Write(JsonSerializer.Serialize(data));

            return true;
        }
        catch (Exception ex)
        {
            ex.LogException("");
            return false;
        }
    }

    private UserData? LoadUserData(string filePath)
    {
        try
        {
            return JsonSerializer.Deserialize<UserData>(File.ReadAllText(filePath));
        }
        catch (Exception ex)
        {
            ex.LogException("Failed to load user data.");
            return null;
        }
    }

    private void AnalyzeAndDisplayChanges(UserData currentData, UserData previousData)
    {
        // Analyze changes and display results (omitted for brevity)
    }
}
