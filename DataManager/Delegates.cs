using DataManager.Model;
using OpenQA.Selenium;

namespace DataManager;
public class Delegates
{
    public delegate LoginOutcome ConditionDelegate(IWebDriver webDriver);
}
