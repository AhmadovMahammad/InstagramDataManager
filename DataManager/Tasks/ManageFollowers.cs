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
    private readonly string _profilesDataPath;
    private string _username = string.Empty;

    public override OperationType OperationType => OperationType.SeleniumBased;

    public ManageFollowers()
    {
        _profilesDataPath = Path.Combine(AppConstant.ApplicationDataFolderPath, "ProfilesData");
        Directory.CreateDirectory(_profilesDataPath);
    }

    protected override void Execute(Dictionary<string, object> parameters)
    {
        IWebDriver webDriver = parameters.Parse<IWebDriver>("WebDriver");
        _httpRequestHandler = new HttpRequestHandler(webDriver);

        _username = PromptForUsername();
        if (string.IsNullOrWhiteSpace(_username)) return;

        bool success = false;

        var taskBuilder = new SeleniumTaskBuilder(webDriver);
        taskBuilder
            .PerformAction(d => ValidateProfileAccess(d, out success))
            .PerformAction(d =>
            {
                if (success)
                {
                    ManageUserData(d);
                }
            })
            .ExecuteTasks();
    }

    private string PromptForUsername()
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

    private void ValidateProfileAccess(IWebDriver webDriver, out bool success)
    {
        success = true;
        "Checking profile access, this may take a few seconds.".WriteMessage(MessageType.Info, addNewLine: false);

        if (!TryNavigateToProfile(webDriver, _username) || !IsProfileAccessible(webDriver))
        {
            success = false;
        }

        ConsoleExtension.ClearLine();
    }

    private bool TryNavigateToProfile(IWebDriver webDriver, string username)
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

    private bool IsProfileAccessible(IWebDriver webDriver)
    {
        var followButton = webDriver.FindWebElement(By.XPath(XPathConstants.FollowButton), WebElementPriorityType.Low);
        if (followButton == null)
        {
            return true;
        }

        "You can only process profiles you are following.".WriteMessage(MessageType.Error);
        return false;
    }

    private void ManageUserData(IWebDriver webDriver)
    {
        var userData = FetchUserData();
        if (userData == null)
        {
            "Failed to fetch followers data.".WriteMessage(MessageType.Error);
            return;
        }

        CompareAndSaveData(userData);
    }

    private UserData? FetchUserData()
    {
        try
        {
            "Fetching user data...".WriteMessage(MessageType.Info, addNewLine: false);
            _httpRequestHandler.UsernameToSearch = _username;

            var following = _httpRequestHandler.FetchFollowingAsync().Result;
            var followers = _httpRequestHandler.FetchFollowersAsync().Result;

            ConsoleExtension.ClearLine();
            return new UserData(followers, following, DateTime.Now);
        }
        catch (Exception ex)
        {
            ex.LogException("Error fetching user data.");
            return null;
        }
    }

    private void CompareAndSaveData(UserData currentData)
    {
        if (!"Do you have previous data to compare? (y/n)".AskToProceed())
        {
            if (TrySaveUserData(currentData, out string fileName))
            {
                $"Data saved to {fileName}.".WriteMessage(MessageType.Success);
            }
        }
        else
        {
            string filePath = "Enter the file path for previous data".GetInput();
            if (File.Exists(filePath))
            {
                var previousData = LoadUserData(filePath);
                if (previousData == null)
                {
                    "Failed to load previous data.".WriteMessage(MessageType.Error);
                    return;
                }

                DisplayComparison(currentData, previousData);
            }
            else
            {
                "File not found.".WriteMessage(MessageType.Error);
            }
        }
    }

    private bool TrySaveUserData(UserData userData, out string fileName)
    {
        fileName = Path.Combine(_profilesDataPath, $"followers_data_{DateTime.Now:yyyy-MM-dd--HH-mm-ss}.json");

        try
        {
            string data = JsonSerializer.Serialize(userData);
            File.WriteAllText(fileName, data);
            return true;
        }
        catch (Exception ex)
        {
            ex.LogException("An error occurred while saving user data.");
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

    private void DisplayComparison(UserData currentData, UserData previousData)
    {
        var newFollowers = currentData.Followers.Except(previousData.Followers).Select(m => m.Username);
        var removedFollowers = previousData.Followers.Except(currentData.Followers).Select(m => m.Username);

        var newFollowing = currentData.Following.Except(previousData.Following).Select(m => m.Username);
        var removedFollowing = previousData.Following.Except(currentData.Following).Select(m => m.Username);

        var analysisData = CreateComparisonData(newFollowers, newFollowing, removedFollowers, removedFollowing);

        Console.WriteLine(); // to differ table visualization from above inputs
        analysisData.DisplayAsTable(
            table => table.Options.EnableCount = false,
            "New Followers", "New Following", "Removed Followers", "Removed Following"
        );
    }

    private IEnumerable<FollowAnalyzeData> CreateComparisonData(
        IEnumerable<string> newFollowers,
        IEnumerable<string> newFollowing,
        IEnumerable<string> removedFollowers,
        IEnumerable<string> removedFollowing)
    {
        int maxRows = new[] { newFollowers.Count(), newFollowing.Count(), removedFollowers.Count(), removedFollowing.Count() }.Max();
        return Enumerable.Range(0, maxRows).Select(i => new FollowAnalyzeData
        {
            NewFollower = newFollowers.ElementAtOrDefault(i) ?? "---",
            NewFollowing = newFollowing.ElementAtOrDefault(i) ?? "---",
            RemovedFollower = removedFollowers.ElementAtOrDefault(i) ?? "---",
            RemovedFollowing = removedFollowing.ElementAtOrDefault(i) ?? "---"
        });
    }
}