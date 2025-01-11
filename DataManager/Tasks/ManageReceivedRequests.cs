using DataManager.Constant;
using DataManager.Constant.Enums;
using DataManager.Helper.Extension;
using DataManager.Helper.Utility;
using OpenQA.Selenium;
using System.Collections.ObjectModel;

namespace DataManager.Tasks;
public class ManageReceivedRequests() : BaseTaskHandler
{
    public override OperationType OperationType => OperationType.SeleniumBased;

    protected override void Execute(Dictionary<string, object> parameters)
    {
        IWebDriver webDriver = parameters.Parse<IWebDriver>("WebDriver");
        ManageData(webDriver);
    }

    private void ManageData(IWebDriver webDriver)
    {
        Console.WriteLine("\nYou can either confirm or delete all received requests.");

        string userInput = "Press 'c' to confirm all requests, or 'd' to delete all requests.".GetInput();
        Action<IWebDriver> action = userInput.ToLowerInvariant() switch
        {
            "c" => ConfirmAllReceivedRequests,
            "d" => DeleteAllReceivedRequests,
            _ => throw new NotImplementedException("Oops! Invalid command.")
        };

        PerformActionOnRequests(webDriver, action);
    }

    private void PerformActionOnRequests(IWebDriver webDriver, Action<IWebDriver> action)
    {
        "Starting the process of managing received requests...\n".WriteMessage(MessageType.Info);
        webDriver.EnsureDomLoaded();

        try
        {
            IWebElement? notificationSidebarElement = webDriver.FindWebElement(
                By.XPath(XPathConstants.NotificationsSidebarButton),
                WebElementPriorityType.Low);

            if (notificationSidebarElement != null)
            {
                notificationSidebarElement.Click();

                IWebElement? expandNotificationsButton = webDriver.FindWebElement(
                    By.XPath(XPathConstants.ExpandNotificationsButton),
                    WebElementPriorityType.Low);

                if (expandNotificationsButton != null)
                {
                    expandNotificationsButton.Click();
                    action.Invoke(webDriver);
                }
                else
                {
                    Console.WriteLine("No requests have been received as of yet.");
                }
            }
        }
        catch (Exception ex)
        {
            ex.LogException("Error during received request management.");
        }
    }

    private void ConfirmAllReceivedRequests(IWebDriver webDriver)
    {
        ReadOnlyCollection<IWebElement> confirmButtons = webDriver.FindElements(By.XPath(XPathConstants.ConfirmButton));

        if (confirmButtons.Count == 0)
        {
            "No requests found to confirm.".WriteMessage(MessageType.Info);
            return;
        }

        for (int i = 0; i < confirmButtons.Count; i++)
        {
            try
            {
                IWebElement confirmButton = confirmButtons[i];
                confirmButton.Click();

                $"Request #{i + 1} confirmed.".WriteMessage(MessageType.Success);
            }
            catch (Exception ex)
            {
                $"Error confirming Request #{i + 1}: {ex.Message}".WriteMessage(MessageType.Error);
            }
        }
    }

    private void DeleteAllReceivedRequests(IWebDriver webDriver)
    {
        ReadOnlyCollection<IWebElement> deleteButtons = webDriver.FindElements(By.XPath(XPathConstants.DeleteButton));

        if (deleteButtons.Count == 0)
        {
            "No requests found to delete.".WriteMessage(MessageType.Info);
            return;
        }

        for (int i = 0; i < deleteButtons.Count; i++)
        {
            try
            {
                IWebElement deleteButton = deleteButtons[i];
                deleteButton.Click();

                $"Request #{i + 1} deleted.".WriteMessage(MessageType.Success);
            }
            catch (Exception ex)
            {
                $"Error deleting Request #{i + 1}: {ex.Message}".WriteMessage(MessageType.Error);
            }
        }
    }
}
