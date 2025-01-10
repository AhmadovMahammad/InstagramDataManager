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

public class ManageFollowersHandler : BaseCommandHandler
{
    private string _username = string.Empty;
    private int _followersCount = 0;
    private int _followingCount = 0;

    public override OperationType OperationType => OperationType.SeleniumBased;

    protected override void Execute(Dictionary<string, object> parameters)
    {
        IWebDriver webDriver = parameters.Parse<IWebDriver>("WebDriver");

        _username = AskForUsername();
        if (string.IsNullOrWhiteSpace(_username)) return;

        bool success = false;

        var taskBuilder = new SeleniumTaskBuilder(webDriver);
        taskBuilder
            .PerformAction(d => ValidateInput(webDriver, out success))
            .PerformAction(d =>
            {
                if (success)
                {
                    ManageData(d);
                }
            }).ExecuteTasks();
    }

    private string AskForUsername()
    {
        while (true)
        {
            Console.WriteLine("\nEnter the Username to process (or type 'exit' to quit):");
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

    private void ValidateInput(IWebDriver webDriver, out bool success)
    {
        success = true;

        "Checking to see whether you have access to this profile. It may require a few seconds.".WriteMessage(MessageType.Info, addNewLine: false);
        if (!NavigateToProfile(webDriver, _username))
        {
            ConsoleExtension.ClearLine();
            success = false;

            "Unable to access the profile. Please make sure the profile is valid and accessible.".WriteMessage(MessageType.Warning);
            return;
        }

        if (!HasAccessToProfile(webDriver))
        {
            ConsoleExtension.ClearLine();
            success = false;

            "You can only process profiles you are following. Please follow the profile and try again.".WriteMessage(MessageType.Error);
            return;
        }

        ConsoleExtension.ClearLine();
    }

    private bool NavigateToProfile(IWebDriver webDriver, string username)
    {
        Console.Write("");

        try
        {
            string url = $"https://www.instagram.com/{username}";
            webDriver.Navigate().GoToUrl(url);
            WebDriverExtension.EnsureDomLoaded(webDriver);

            var notAvailableNotification = webDriver.FindWebElement(By.XPath(XPathConstants.NotAvailableNotification), WebElementPriorityType.Low);
            return notAvailableNotification is null;
        }
        catch (Exception ex)
        {
            ex.LogException("Failed to navigate to profile.");
            return false;
        }
    }

    private bool HasAccessToProfile(IWebDriver webDriver)
    {
        try
        {
            var followButton = webDriver.FindWebElement(By.XPath(XPathConstants.FollowButton), WebElementPriorityType.Low);
            return followButton == null;
        }
        catch
        {
            return false;
        }
    }

    private void ManageData(IWebDriver webDriver)
    {
        var userData = FetchUserData(webDriver, _username);
        if (userData == null)
        {
            "Failed to fetch followers data.".WriteMessage(MessageType.Error);
            return;
        }

        HandleDataComparison(userData);
    }

    private UserData? FetchUserData(IWebDriver webDriver, string username)
    {
        try
        {
            (string followersXPath, string followingXPath) = InitializeData(webDriver, username);

            var followers = FetchUserList(webDriver, followersXPath, _followersCount, "Followers");
            var following = FetchUserList(webDriver, followingXPath, _followingCount, "Following");

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

    private HashSet<UserEntry> FetchUserList(IWebDriver webDriver, string itemXPath, int totalCount, string action)
    {
        var listItems = new HashSet<UserEntry>();
        int id = 1;

        try
        {
            var modalButton = webDriver.FindWebElement(By.XPath(itemXPath), WebElementPriorityType.Medium);
            if (modalButton == null) return listItems;

            modalButton.Click();
            webDriver.EnsureDomLoaded();

            var scrollbarElement = webDriver.FindWebElement(By.XPath(XPathConstants.Scrollbar), WebElementPriorityType.Low);
            if (scrollbarElement == null) return listItems;

            var wait = new WebDriverWait(webDriver, AppConstant.ExplicitWait);
            wait.IgnoreExceptionTypes(typeof(WebDriverTimeoutException));

            while (listItems.Count < totalCount)
            {
                try
                {
                    wait.Until(driver =>
                    {
                        var currentElements = driver.FindElements(By.XPath(XPathConstants.ManagableUsername));
                        return currentElements.Count != 0;
                    });
                }
                catch (WebDriverTimeoutException)
                {
                }

                var currentElements = webDriver.FindElements(By.XPath(XPathConstants.ManagableUsername));
                AddUniqueUsernames(currentElements, listItems, ref id);

                if (listItems.Count < totalCount)
                {
                    webDriver.ScrollToBottom(scrollbarElement, 200);
                    Thread.Sleep(500);
                }
            }
        }
        catch (Exception ex)
        {
            ex.LogException($"Error occurred while fetching {action} list.");
        }
        finally
        {
            webDriver.Navigate().Refresh();
        }

        return listItems;
    }

    private void AddUniqueUsernames(IReadOnlyCollection<IWebElement> elements, HashSet<UserEntry> listItems, ref int id)
    {
        foreach (var element in elements)
        {
            string username = element.Text.Trim();
            if (listItems.All(userEntry => userEntry.Username != username))
            {
                listItems.Add(new UserEntry(id++, username));
            }
        }
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
                if (previousData == null)
                {
                    "Failed to load previous data.".WriteMessage(MessageType.Error);
                    return;
                }

                AnalyzeAndDisplayChanges(currentData, previousData);
            }
            else
            {
                "File not found.".WriteMessage(MessageType.Error);
            }
        }
        else if (TrySaveUserData(currentData, out string fileName))
        {
            $"Current followers data saved to {fileName}.".WriteMessage(MessageType.Success);
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
            writer.Write(data);

            return true;
        }
        catch (Exception ex)
        {
            ex.LogException("An error occurred while saving user data as JSON file.");
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

    }
}
