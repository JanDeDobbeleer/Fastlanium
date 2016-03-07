using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace Selenium.WindowsStore
{
    public enum ExitCodes
    {
        Success = 0,
        UnableToAuthenticate = 40,
        UnableToPrepareSubmission = 51,
        UnableToUpload = 52,
        UnableToSubmit = 53,
        InvalidArgs = 20
    }

    class FastLanium
    {
        private static string _userName;
        private static string _passWord;
        private static string _appUrl;
        private static readonly List<string> Packages = new List<string>();

        static int Main(string[] args)
        {
            if (!ValidateArgs(args))
                return (int)ExitCodes.InvalidArgs;
            var driver = new ChromeDriver();
            if (!Authenticate(driver))
                return (int)ExitCodes.UnableToAuthenticate;
            if (!CreateSubmissionAndSetupForUpload(driver, _appUrl))
                return (int)ExitCodes.UnableToPrepareSubmission;
            if (!UploadPackages(driver, Packages))
                return (int)ExitCodes.UnableToUpload;
            if (!Submit(driver))
                return (int)ExitCodes.UnableToSubmit;
            return (int)ExitCodes.Success;
        }

        private static bool ValidateArgs(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine($"Please provide the needed arguments.{Environment.NewLine}Usage:{Environment.NewLine}fastlanium <username> <password> <appId> <packageLocation>{Environment.NewLine}fastlanium <username> <password> <appId> <packageLocation> <packageLocation>");
                return false;
            }
            _userName = args[0];
            _passWord = args[1];
            _appUrl = $"https://dev.windows.com/en-us/dashboard/Apps/{args[2]}";
            for (var i = 3; i < args.Length; i++)
            {
                Packages.Add(args[i]);
            }
            return true;
        }

        private static bool Authenticate(RemoteWebDriver driver)
        {
            try
            {
                Console.WriteLine("Starting authentication");
                driver.Navigate().GoToUrl("https://dev.windows.com");
                var signinButton = driver.FindElementById("meControl");
                signinButton.Click();
                var link = driver.FindElementByLinkText("Sign in with a Microsoft account");
                link.Click();
                var emailInput = driver.FindElementById("i0116");
                emailInput.SendKeys(_userName);
                var passwordField = driver.FindElementById("i0118");
                passwordField.SendKeys(_passWord);
                passwordField.Submit();
                //hack for the stupid confirm account shit I have right now.
                if (driver.FindElementsById("iLandingViewAction").Any())
                {
                    var nextButton = driver.FindElementById("iLandingViewAction");
                    nextButton.Click();
                }
                Console.WriteLine("Authentication successful");
                return true;
            }
            catch (Exception e)
            {
                driver.Close();
                Console.WriteLine("Error while authenticating");
                Console.WriteLine(e.Message);
                return false;
            }
        }

        private static bool CreateSubmissionAndSetupForUpload(RemoteWebDriver driver, string appPackage)
        {
            try
            {
                Console.WriteLine("Preparing submission");
                driver.Navigate().GoToUrl(appPackage);
                var updateButton = driver.FindElementByLinkText("Update");
                updateButton.Click();
                var success = false;
                IWebElement packages = new RemoteWebElement(driver, string.Empty);
                while (!success)
                {
                    if (!driver.Url.Contains("submissions"))
                        continue;
                    packages = driver.FindElementById("appSubmissionPackagesLink");
                    success = true;
                }
                packages.Click();
                Console.WriteLine("Submission created");
                return true;
            }
            catch (Exception e)
            {
                driver.Close();
                Console.WriteLine("Error while creating submission");
                Console.WriteLine(e.Message);
                return false;
            }
        }

        private static bool UploadPackages(RemoteWebDriver driver, List<string> packages)
        {
            try
            {
                Console.WriteLine($"Starting package upload for {packages.Count} packages");
                var inputField = driver.FindElementByXPath("//input[@type='file']");
                foreach (var package in packages)
                {
                    Console.WriteLine($"Package {package} upload started");
                    inputField.SendKeys(package);
                }
                var success = false;
                driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(5));
                while (!success)
                {
                    var progressContainer = driver.FindElementsByClassName("progress-container");
                    if (!progressContainer.Any())
                        success = true;
                }
                Console.WriteLine("All packages uploaded");
                var saveButton = driver.FindElementById("formPackages");
                saveButton.Submit();
                try
                {
                    var popup = driver.SwitchTo().Alert();
                    popup.Accept();
                }
                catch (NoAlertPresentException)
                {
                    //Just making sure we do not crash when we do not have that annoying popup.
                }
                Console.WriteLine("Package upload completed");
                return true;
            }
            catch (Exception e)
            {
                driver.Close();
                Console.WriteLine("Error while uploading packages");
                Console.WriteLine(e.Message);
                return false;
            }
        }

        private static bool Submit(RemoteWebDriver driver)
        {
            try
            {
                Console.WriteLine("Verifying submission button status");
                var submitButton = driver.FindElementById("submitToTheStoreButton");
                if (!submitButton.Enabled)
                {
                    Console.WriteLine("The app cannot be submitted at this time, please review the submission and perform a manual submission");
                    return false;
                }
                submitButton.Click();
                driver.Quit();
                Console.WriteLine("Submission done, driver closed");
                return true;
            }
            catch (Exception e)
            {
                driver.Close();
                Console.WriteLine("Error while submitting app");
                Console.WriteLine(e.Message);
                return false;
            }
        }

    }
}
