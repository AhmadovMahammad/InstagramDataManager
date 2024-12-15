﻿using DataManager.DesignPatterns.ChainOfResponsibilityDP.Contracts;
using DataManager.DesignPatterns.ChainOfResponsibilityDP.Implementations;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

namespace DataManager.Automation;
public class SeleniumAutomation
{
    private readonly IWebDriver _driver;
    private readonly IChainHandler _validationChain;

    public SeleniumAutomation()
    {
        // Set up validation chain
        _validationChain = new ArgumentNotEmptyHandler()
            .SetNext(new FileExistsHandler())
            .SetNext(new FileExtensionHandler([".exe"])); // Expecting .exe file

        // Prompt user for Firefox executable path
        string firefoxDeveloperEditionPath = GetFirefoxExecutablePath(); // @"C:\Program Files\Firefox Developer Edition\firefox.exe";

        // Set Firefox options
        FirefoxOptions options = new FirefoxOptions { BinaryLocation = firefoxDeveloperEditionPath };
        options.AddArgument("--start-maximized");

        // Initialize the WebDriver with options
        _driver = new FirefoxDriver(options);
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
    }

    public void Execute()
    {
        // all operations here
    }

    private string GetFirefoxExecutablePath()
    {
        string? firefoxPath = string.Empty;

        while (string.IsNullOrWhiteSpace(firefoxPath) || !_validationChain.Handle(firefoxPath))
        {
            Console.WriteLine("Please enter the path to your Firefox Developer Edition executable (e.g., C:\\Program Files\\Firefox Developer Edition\\firefox.exe):");
            firefoxPath = Console.ReadLine();
        }

        return firefoxPath;
    }
}