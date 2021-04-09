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
    class ProfileControllerTests
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
        
        public void NavToFriendList()
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // click Hello Admin
            driver.FindElement(By.XPath("//ul[contains(@class,'navbar-right')]//a[@href='#']")).Click();

            // wait for Friend List button
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//a[@href='/Manage/FriendList']")));

            // click Friend List
            driver.FindElement(By.XPath("//a[@href='/Manage/FriendList']")).Click();

            // wait for My Friends List header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='My Friends List']")));
        }

    
        [TestCase("test_user", TestName ="AddFriend_FriendAdded")]
        public void Test_AddFriend(string friend_name)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            Login("Admin", "Qwe!23");

            NavToFriendList();

            try
            {
                // check to see if friend list is empty
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//p[text()='Your friend list is empty.']")));
            }
            catch
            {
                // if not empty click remove friend for friend_name
                driver.FindElement(By.XPath("//div[contains(text(),'" + friend_name + "')]/following-sibling::div/div/a[@title='Unfriend']")).Click();
            }

            // search for friend_name
            driver.FindElement(By.Id("search")).SendKeys(friend_name);
            driver.FindElement(By.XPath("//input[@type='submit' and @value='Search']")).Click();

            // wait for friend_name profile header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='" + friend_name + "']")));

            // click add to friends list button
            driver.FindElement(By.XPath("//a[text()='Add to Friend List']")).Click();

            // wait for friend_name profile header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//p[text()='" + friend_name + " is on your friend list.']")));

            NavToFriendList();

            try
            {
                // check to see if friend list is empty
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//p[text()='Your friend list is empty.']")));
                Assert.Fail();
            }
            catch
            {
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//div[contains(text(),'" + friend_name + "')]")));
                Assert.Pass();
            }
        }

        [TestCase("test_user", TestName = "RemoveFriend_FriendAdded")]
        public void Test_RemoveFriend(string friend_name)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            bool friendPresent = false;

            Login("Admin", "Qwe!23");
            NavToFriendList();

            try
            {
                // check to see if friend list is empty
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//p[text()='Your friend list is empty.']")));
                // if above check passes then create a user to delete
                Test_AddFriend("test_user");
                friendPresent = true;
            }
            catch
            {
                // if not empty click remove friend for friend_name
                driver.FindElement(By.XPath("//div[contains(text(),'"+ friend_name + "')]/following-sibling::div/div/a[@title='Unfriend']")).Click();
            }

            // if user is created in try then delete the user after adding
            if (friendPresent == true)
            {
                driver.FindElement(By.XPath("//div[contains(text(),'" + friend_name + "')]/following-sibling::div/div/a[@title='Unfriend']")).Click();
            }

            // check to see if friend_name is still in friend list
            try
            {
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//div[contains(text(),'" + friend_name + "')]")));
                Assert.Fail();
            }
            catch
            {
                Assert.Pass();
            }
        }

        [TestCase("test_user", TestName = "CheckProfileDetails_DetailsPresent")]
        public void Test_RemoveMember_Profile_Details(string friend_name)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            Login("Admin", "Qwe!23");

            NavToFriendList();

            try
            {
                // check to see if friend list is empty
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//p[text()='Your friend list is empty.']")));
            }
            catch
            {
                // if not empty click remove friend for friend_name
                driver.FindElement(By.XPath("//div[contains(text(),'" + friend_name + "')]/following-sibling::div/div/a[@title='Unfriend']")).Click();
            }

            // search for friend_name
            driver.FindElement(By.Id("search")).SendKeys(friend_name);
            driver.FindElement(By.XPath("//input[@type='submit' and @value='Search']")).Click();

            // wait for friend_name profile header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='" + friend_name + "']")));

            //check for You must be on the user's Friend List to be able to see their Wish List and Games Owned.
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//p[contains(text(), 'Friend List to be able to see their Wish List and Games Owned.')]")));

            //check for Add to friend List button
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//a[text()='Add to Friend List']")));

            //if test_user details are found then pass
            Assert.Pass();
        }
    }
}
