using DataManager.Model;
using OpenQA.Selenium;

namespace DataManager.Core.Delegates;
public class LoginDelegates
{
    public delegate LoginOutcome ConditionDelegate(IWebDriver webDriver);
}
