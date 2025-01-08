using DataManager.Constant;
using DataManager.Constant.Enums;
using DataManager.DesignPattern.Builder;
using DataManager.Helper.Extension;
using DataManager.Helper.Utility;
using DataManager.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Text.Json;

namespace DataManager.Handler.ManageHandlers;
public class ManageFollowersHandler() : BaseCommandHandler
{
    private const string FollowButtonXPath = "//div[contains(@class,'_ap3a') and normalize-space(text())='Follow']";
    private const string UsernameXPath = "//span[@class='_ap3a _aaco _aacw _aacx _aad7 _aade' and @dir='auto']";

    private string _username = "";
    private string _followersXPath = "";
    private string _followingXPath = "";
    private int _followersCount = 0;
    private int _followingCount = 0;

    public override OperationType OperationType => OperationType.SeleniumBased;

    protected override void Execute(Dictionary<string, object> parameters)
    {
        IWebDriver webDriver = parameters.Parse<IWebDriver>("WebDriver");
        ManageData(webDriver);
    }

    private void ManageData(IWebDriver webDriver)
    {
        string profileUrl = AskForProfileUrl(webDriver);
        if (string.IsNullOrEmpty(profileUrl)) return;

        UserData? followersData = ProcessFollowersData(webDriver);
        if (followersData == null) return;

        HandleDataComparison(followersData);
    }

    private string AskForProfileUrl(IWebDriver webDriver)
    {
        while (true)
        {
            Console.WriteLine("Enter the Username to stalk (or type 'exit' to quit):");
            Console.Write("> ");
            _username = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrEmpty(_username) || _username.Equals("exit", StringComparison.CurrentCultureIgnoreCase))
            {
                "Operation cancelled by user.".WriteMessage(MessageType.Warning);
                return string.Empty;
            }

            try
            {
                webDriver.Navigate().GoToUrl($"https://www.instagram.com/{_username}");
                Console.WriteLine($"Navigating to profile > {_username}");
                WebDriverExtension.EnsureDomLoaded(webDriver);

                // Check if the "Follow" button exists
                "\nChecking to see whether you follow this profile. It may require a few seconds.\n".WriteMessage(MessageType.Info);

                var followButton = webDriver.FindElementWithRetries("Follow Button", By.XPath(FollowButtonXPath), retries: 2, initialDelay: 1000, logMessage: false);
                if (followButton != null)
                {
                    "You can only stalk profiles you are already following. Please try again.".WriteMessage(MessageType.Error);
                    continue;
                }

                InitializeData(webDriver, _username);
                return _username;
            }
            catch (Exception ex)
            {
                ex.LogException("Failed to verify the profile. Please ensure the URL is valid.");
            }
        }
    }

    private void InitializeData(IWebDriver webDriver, string profileUrl)
    {
        try
        {
            _followersXPath = $"//a[contains(@href,'/{profileUrl}/followers/')]/span/span[normalize-space(text())]";
            _followingXPath = $"//a[contains(@href,'/{profileUrl}/following/')]/span/span[normalize-space(text())]";

            // Fetch and parse the followers count
            var followersElement = webDriver.FindElement(By.XPath(_followersXPath));
            _followersCount = int.Parse(followersElement.Text.Trim());

            // Fetch and parse the following count
            var followingElement = webDriver.FindElement(By.XPath(_followingXPath));
            _followingCount = int.Parse(followingElement.Text.Trim());

            // Display Data
            new List<string>
            {
                _username, _followersCount.ToString(), _followingCount.ToString()
            }.DisplayAsTable(null, "Username", "Followers", "Following");
        }
        catch (Exception ex)
        {
            ex.LogException("Failed to initialize data. Please ensure the profile URL is valid and accessible.");
        }
    }

    private UserData? ProcessFollowersData(IWebDriver webDriver)
    {
        var taskBuilder = new SeleniumTaskBuilder(webDriver);

        try
        {
            var followers = new HashSet<UserEntry>();
            var following = new HashSet<UserEntry>();

            taskBuilder
                .PerformAction((webDriver) =>
                {
                    WebDriverExtension.EnsureDomLoaded(webDriver);
                    followers = FetchList(webDriver, _followersXPath, _followersCount, "Followers");
                })
                .PerformAction((webDriver) => webDriver.Navigate().Refresh())
                .PerformAction((webDriver) =>
                {
                    WebDriverExtension.EnsureDomLoaded(webDriver);
                    following = FetchList(webDriver, _followingXPath, _followingCount, "Following");
                }).ExecuteTasks();

            return new UserData(followers, following, DateTime.Now);
        }
        catch (Exception ex)
        {
            ex.LogException("Error processing followers data.");
            return null;
        }
    }

    private HashSet<UserEntry> FetchList(IWebDriver webDriver, string itemXPath, int totalCount, string action = "")
    {
        var listItems = new HashSet<UserEntry>();
        int previousElementCount = 0;
        int id = 1;

        try
        {
            IWebElement? modalButton = webDriver.FindElementWithRetries("Modal Page Button", By.XPath(itemXPath), retries: 3, initialDelay: 1500, false);
            if (modalButton == null) return listItems;
            modalButton.Click();

            var wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10));
            while (listItems.Count < totalCount)
            {
                try
                {
                    wait.Until(driver =>
                    {
                        var currentElements = driver.FindElements(By.XPath(UsernameXPath));

                        var newText = currentElements.Select(e => e.Text.Trim()).ToHashSet();
                        var previousText = listItems.Select(e => e.Username).ToHashSet();

                        return newText.Except(previousText).Any();
                    });
                }
                catch (WebDriverTimeoutException)
                {
                    Console.WriteLine($"Proceeding with available data. ({listItems.Count})");
                    break;
                }

                var currentElements = webDriver.FindElements(By.XPath(UsernameXPath));
                foreach (var element in currentElements)
                {
                    string username = element.Text?.Trim() ?? string.Empty;
                    if (!string.IsNullOrEmpty(username) && listItems.All(x => x.Username != username))
                    {
                        var userEntry = new UserEntry(id++, username);
                        listItems.Add(userEntry);
                    }

                    if (listItems.Count >= totalCount)
                    {
                        return listItems;
                    }
                }

                previousElementCount = currentElements.Count;
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
            ex.LogException($"Error occurred while fetching list for {action}.");
        }

        return listItems;
    }

    private void HandleDataComparison(UserData currentData)
    {
        Console.WriteLine("Do you have previous data to compare? (y/n):");
        Console.Write("> ");
        bool hasPreviousData = Console.ReadLine()?.Trim().ToLower() == "y";

        if (hasPreviousData)
        {
            Console.WriteLine("Enter the file path for previous data:");
            string filePath = Console.ReadLine()?.Trim() ?? string.Empty;

            if (File.Exists(filePath))
            {
                using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var previousData = JsonSerializer.Deserialize<UserData>(fileStream);

                AnalyzeAndDisplayChanges(currentData, previousData);
            }
            else
            {
                "File not found.".WriteMessage(MessageType.Error);
            }
        }
        else
        {
            string fileName = Path.Combine(AppConstant.ApplicationDataFolderPath, $"followers_data_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json");

            using (FileStream fileStream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
            using (TextWriter writer = new StreamWriter(fileStream))
            {
                writer.Write(JsonSerializer.Serialize(currentData));
            }

            $"Current followers data saved to {fileName}.".WriteMessage(MessageType.Success);
        }
    }

    private void AnalyzeAndDisplayChanges(UserData currentData, UserData? previousData)
    {

    }
}