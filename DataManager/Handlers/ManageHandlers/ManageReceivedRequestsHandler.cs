using DataManager.Constants.Enums;
using DataManager.Helpers.Extensions;
using DataManager.Helpers.Utilities;
using OpenQA.Selenium;

namespace DataManager.Handlers.ManageHandlers
{
    public class ManageReceivedRequestsHandler() : BaseCommandHandler
    {
        private const string NotificationsSidebarButtonXPath = "//*[name()='svg' and @aria-label='Notifications']";
        private const string ExpandNotificationsButtonXPath = "//*[name()='svg' and @aria-label='' and contains(@class, 'x1lliihq x1n2onr6 x1roi4f4')]";

        public override OperationType OperationType => OperationType.SeleniumBased;

        protected override void Execute(Dictionary<string, object> parameters)
        {
            IWebDriver webDriver = parameters.Parse<IWebDriver>("WebDriver");
            ManageData(webDriver);
        }

        private void ManageData(IWebDriver webDriver)
        {
            Console.WriteLine("\nYou can either confirm or delete all received requests.");
            Console.WriteLine("Press 'c' to confirm all requests, or 'd' to delete all requests.");
            Console.Write("> ");

            string userInput = (Console.ReadLine()?.ToLower() ?? string.Empty).Trim();
            Action<IWebDriver> action = userInput switch
            {
                "c" => ConfirmAllReceivedRequests,
                "d" => DeleteAllReceivedRequests,
                _ => throw new NotImplementedException("Oops! Invalid command.")
            };

            PerformActionOnRequests(webDriver, action);
        }

        private void PerformActionOnRequests(IWebDriver webDriver, Action<IWebDriver> action)
        {
            Console.WriteLine("Starting the process of managing received requests...");
            WebDriverExtension.EnsureDomLoaded(webDriver);

            try
            {
                IWebElement? notificationSidebarElement = webDriver.FindElementWithRetries("Notifications Button", By.XPath(NotificationsSidebarButtonXPath), 2, 1000);
                if (notificationSidebarElement != null)
                {
                    notificationSidebarElement.Click();
                    IWebElement? expandNotificationsButton = webDriver.FindElementWithRetries("Expand Notifications Button", By.XPath(ExpandNotificationsButtonXPath), 2, 1000);

                    if (expandNotificationsButton != null)
                    {
                        expandNotificationsButton.Click();
                        action.Invoke(webDriver);
                    }
                    else
                    {
                        Console.WriteLine("No expand button found, there might be no current requests to manage.");
                    }
                }
            }
            catch (Exception ex)
            {
                ex.LogException("Error during request management.");
            }
        }

        private void ConfirmAllReceivedRequests(IWebDriver webDriver)
        {
            Console.WriteLine("Confirming all received requests...");
            var confirmButtons = webDriver.FindElements(By.XPath("//div[contains(text(), 'Confirm')]"));

            if (confirmButtons.Count == 0)
            {
                Console.WriteLine("No requests found to confirm.");
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
            Console.WriteLine("Deleting all received requests...");
            var deleteButtons = webDriver.FindElements(By.XPath("//div[contains(text(), 'Delete')]"));

            if (deleteButtons.Count == 0)
            {
                Console.WriteLine("No requests found to delete.");
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
}
