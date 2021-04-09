using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;

namespace CVGS.Tests
{
    [TestFixture]
    public class ReportControllerTests
    {
        private IWebDriver driver;
        private string baseURL;


        // instantiate FireFox driver
        [SetUp]
        public void SetUpMethod()
        {
            driver = new FirefoxDriver();
            baseURL = "http://localhost:56061/";
            driver.Manage().Window.Maximize();
        }


        // close browser and safely close the session
        [TearDown]
        public void TearDownMethod()
        {
            if (driver != null)
            {
                driver.Quit();
            }
        }


        public void Login(string _displayName, string _password)
        {
            var displayName = _displayName;
            var password = _password;

            // open the Home page
            driver.Navigate().GoToUrl(baseURL);

            // click on the "Log In"
            driver.FindElement(By.Id("loginLink")).Click();

            //wait for 10 seconds to check if the web page title is "Log in - Caesar Salad Gaming"
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.Title.Contains("Log in"));

            // fill out display name
            driver.FindElement(By.Id("DisplayName")).Clear();
            driver.FindElement(By.Id("DisplayName")).SendKeys(displayName);

            // fill out password
            driver.FindElement(By.Id("Password")).Clear();
            driver.FindElement(By.Id("Password")).SendKeys(password);

            // click on Submit button
            driver.FindElement(By.XPath("//input[@type='submit' and @value='Log in']")).Click();

            //wait for 10 seconds to check if the web page title is "Log in - Caesar Salad Gaming"
            wait.Until(d => d.Title.Contains("Home Page"));

            // check the greeting in the top right corner
            string greeting = driver.FindElement(By.XPath("//ul[contains(@class,'navbar-right')]//a[@href='#']")).Text;
            Assert.AreEqual(string.Format("Hello {0}!", displayName), greeting);
        }


        [TestCase("Game List", TestName = "TestGameListReport_GameListReportSent")]
        [TestCase("Game Sales", TestName = "TestGameSalesReport_GameSalesReportSent")]
        [TestCase("Wish List", TestName = "TestWishListReport_WishListReportSent")]
        [TestCase("Member List", TestName = "TestMemberListReport_MemberListReportSent")]
        [TestCase("Member Sales", TestName = "TestMemberSalesReport_MemberSalesReportSent")]
        [TestCase("Friend List", TestName = "TestFriendListReport_FriendListReportSent")]
        public void Test_Reports(string report)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            Login("Admin", "Qwe!23");

            // click Administration
            driver.FindElement(By.XPath("//a[contains(text(),'Administration')]")).Click();

            //click Generate Reports
            driver.FindElement(By.XPath("//a[@href='/Admin/Report']")).Click();

            //wait for Generate Reports header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Generate Reports']")));

            //click Report button
            driver.FindElement(By.XPath("//a[text()='"+ report + " Report']")).Click();

            try
            {
                //wait for Generate Reports header
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//span[text()='The "+ report + " Report has been generated and emailed to you.']")));
            }
            catch
            {
                Assert.Fail();
            }

            //if message shows up test passes
            Assert.Pass();
        }
    }
}
