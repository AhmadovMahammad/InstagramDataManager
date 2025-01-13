using DataManager.Constant;
using DataManager.Constant.Enums;
using DataManager.DesignPattern.Builder;
using DataManager.Helper.Extension;
using DataManager.Helper.Utility;
using DataManager.Model;
using OpenQA.Selenium;
using System.Text.Json;

namespace DataManager.Tasks;
public class ManageFollowers : BaseTaskHandler
{
    private HttpRequestHandler _httpRequestHandler = null!;
    private string _username = string.Empty;

    public override OperationType OperationType => OperationType.SeleniumBased;

    protected override void Execute(Dictionary<string, object> parameters)
    {
        IWebDriver webDriver = parameters.Parse<IWebDriver>("WebDriver");
        _httpRequestHandler = new HttpRequestHandler(webDriver);

        _username = AskForUsername();
        if (string.IsNullOrWhiteSpace(_username)) return;

        bool success = false;

        var taskBuilder = new SeleniumTaskBuilder(webDriver);
        taskBuilder
            .PerformAction(d => ValidateInput(d, out success))
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
            _username = "Enter the Username to process (or type 'exit' to quit)".GetInput();
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
        try
        {
            string url = $"https://www.instagram.com/{username}";
            webDriver.Navigate().GoToUrl(url);
            webDriver.EnsureDomLoaded();

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
        var followButton = webDriver.FindWebElement(By.XPath(XPathConstants.FollowButton), WebElementPriorityType.Low);
        return followButton == null;
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
            var following = _httpRequestHandler.FetchFollowingAsync().Result;
            var followers = _httpRequestHandler.FetchFollowersAsync().Result;

            return new UserData(followers, following, DateTime.Now);
        }
        catch (Exception ex)
        {
            ex.LogException("Error fetching user data.");
            return null;
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
        fileName = Path.Combine(AppConstant.ApplicationDataFolderPath, "ProfilesData", $"followers_data_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json");

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
        var newFollowers = currentData.Followers.Except(previousData.Followers);
        var removedFollowers = previousData.Followers.Except(currentData.Followers);

        var newFollowing = currentData.Following.Except(previousData.Following);
        var removedFollowing = previousData.Following.Except(currentData.Following);

        int maxLength = new int[]
        {
            newFollowers.Count(),
            removedFollowers.Count(),
            newFollowing.Count(),
            removedFollowing.Count()
        }.Max();

    }
}
