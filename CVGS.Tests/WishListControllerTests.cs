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
    public class WishListControllerTests
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
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
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


        public void NavToManageGames()
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // click Administration
            driver.FindElement(By.XPath("//a[contains(text(),'Administration')]")).Click();

            // click Manage Games
            driver.FindElement(By.XPath("//a[@href='/Admin/Game']")).Click();

            // wait for Games list to appear 
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Game List']")));
        }


        public void DeleteGame_fromManageGames(string gameTitle)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // click delete
            driver.FindElement(By.XPath("//tr[./td[text()='" + gameTitle + "']]/td/a[contains(@href, 'Delete')]")).Click();

            // wait for delete game page
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Delete Game']")));

            // click delete
            driver.FindElement(By.XPath("//input[@type='submit' and @value='Delete']")).Click();

            // wait for manage games list page
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Game List']")));
        }


        public void Add_Game(string _gameTitle, string _releaseYear, string _description, string _price, bool _delete)
        {
            string[,] testValues = new string[,]
            {
            {"//input[@name='Title']", _gameTitle},
            {"//input[@name='ReleaseYear']", _releaseYear},
            {"//textarea[@name='Description']", _description},
            {"//input[@name='Price']", _price},
            {"//input[@name='ImageUrl']", "https://cdn.cloudflare.steamstatic.com/steam/apps/771710/header.jpg?t=1525391321"}
            };

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // login 
            Login("Admin", "Qwe!23");

            // navigate to Manage Games Page
            NavToManageGames();

            // delete test game if already avaliable
            if (driver.FindElements(By.XPath("//td[text()='" + testValues[0, 1] + "']")).Count != 0)
            {
                DeleteGame_fromManageGames(testValues[0, 1]);
            }

            // if test game is still present delete didn't work. Fail
            if (driver.FindElements(By.XPath("//td[text()='" + testValues[0, 1] + "']")).Count != 0)
            {
                Assert.Fail();
            }

            // click Add New Game
            driver.FindElement(By.XPath("//a[text()='Add New Game']")).Click();

            // wait for Add New game page to appear 
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Add New Game']")));

            // input values for new game
            for (int i = 0; i < testValues.GetLength(0); i++)
            {
                driver.FindElement(By.XPath(testValues[i, 0])).Clear();
                driver.FindElement(By.XPath(testValues[i, 0])).SendKeys(testValues[i, 1]);
            }

            // click Save
            driver.FindElement(By.XPath("//input[@type='submit' and @value='Save']")).Click();

            // wait for Games list to appear
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Game List']")));

            // check to see if game successfully added
            string gameAdded = driver.FindElement(By.XPath("//div[@class='tempdata']")).Text;
            Assert.AreEqual(string.Format("Record for '{0}' successfully added", testValues[0, 1]), gameAdded);

            // check manage games table for Title, ReleaseYear and Price Values
            for (int i = 0; i < testValues.GetLength(0); i++)
            {
                if ((new[] {"//input[@name='Title']",
                        "//input[@name='ReleaseYear']",
                        "//input[@name='Price']" }).Contains(testValues[i, 0]))
                {
                    string gameAttribute = driver.FindElement(By.XPath("//tr[./td[text()='" + testValues[1, 1] +
                                                                        "']]/td[contains(text(),'" +
                                                                        testValues[i, 1] + "')]")).Text;
                    if (testValues[i, 0] == "//input[@name='Price']")
                    {
                        Assert.AreEqual("$" + testValues[i, 1], gameAttribute);
                    }
                    else
                    {
                        Assert.AreEqual(testValues[i, 1], gameAttribute);
                    }
                }
            }

            if (_delete)
            {
                // cleanup Delete testGame from database
                DeleteGame_fromManageGames(testValues[0, 1]);
            }
        }

        [TestCase(TestName = "NavToWishList_WishlistDisplayed")]
        public void Test_NavToWishlist()
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            //login
            Login("Admin", "Qwe!23");

            // click Hello Admin
            driver.FindElement(By.XPath("//ul[contains(@class,'navbar-right')]//a[@href='#']")).Click();

            // wait for Wish List button
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//a[@href='/WishLists']")));

            // click Wish List
            driver.FindElement(By.XPath("//a[@href='/WishLists']")).Click();

            // wait My Wish List header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='My Wish List']")));

            Assert.Pass();
        }

        [TestCase(TestName = "AddToWishList_GameAddedToWishlist")]
        public void Test_AddToWishList()
        {

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            Add_Game("ReviewTestGame", "2020", "Test Game For review", "10.00", false);

            // click to nav home
            driver.FindElement(By.XPath("//a[@href='/']")).Click();

            // wait for featured and recommended header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Featured and Recommended']")));

            // click on new test game created
            driver.FindElement(By.XPath("//div[contains(text(),'ReviewTestGame')]")).Click();

            // wait for game header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='ReviewTestGame']")));

            // click Add To Wishlist
            driver.FindElement(By.XPath("//a[text()='Add to Wish List']")).Click();

            // click Hello Admin
            driver.FindElement(By.XPath("//ul[contains(@class,'navbar-right')]//a[@href='#']")).Click();

            // wait for Wish List button
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//a[@href='/WishLists']")));

            // click Wish List
            driver.FindElement(By.XPath("//a[@href='/WishLists']")).Click();
                
            // wait My Wish List header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='My Wish List']")));

            // click on new test game created
            driver.FindElement(By.XPath("//div[contains(text(),'ReviewTestGame')]")).Click();

            // wait for game header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='ReviewTestGame']")));

            // check for remove from wishlist button
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//a[text()='Remove from Wish List']")));

            Assert.Pass();
        }

        [TestCase(TestName = "DeleteFromWishList_GameDeletedFromWishlist")]
        public void Test_Delete_FromWishlist()
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // login 
            Login("Admin", "Qwe!23");

            // click Hello Admin
            driver.FindElement(By.XPath("//ul[contains(@class,'navbar-right')]//a[@href='#']")).Click();

            // wait for Wish List button
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//a[@href='/WishLists']")));

            // click Wish List
            driver.FindElement(By.XPath("//a[@href='/WishLists']")).Click();

            // wait My Wish List header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='My Wish List']")));

            // click on new test game created
            driver.FindElement(By.XPath("//div[contains(text(),'ReviewTestGame')]")).Click();

            // wait for game header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='ReviewTestGame']")));

            // check for remove from wishlist button
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//a[text()='Remove from Wish List']")));

            // click on remove from wishlist button
            driver.FindElement(By.XPath("//a[text()='Remove from Wish List']")).Click();

            // click Hello Admin
            driver.FindElement(By.XPath("//ul[contains(@class,'navbar-right')]//a[@href='#']")).Click();

            // wait for Wish List button
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//a[@href='/WishLists']")));

            // click Wish List
            driver.FindElement(By.XPath("//a[@href='/WishLists']")).Click();

            // check for remove from wishlist button
            var noWishListGames = driver.FindElement(By.XPath("//p[text()='You currently do not have any games in your wish list.']")).Text;

            Assert.AreEqual(noWishListGames, "You currently do not have any games in your wish list.");

            // navigate to manage games 
            NavToManageGames();

            // delete test game
            DeleteGame_fromManageGames("ReviewTestGame");
        }

        [TestCase("Call of Duty: Warzone", TestName = "ViewFriendWishList_CallOfDutyInWishlist")]
        [TestCase("Apex Legends", TestName = "ViewFriendWishList_ApexLegendsInWishlist")]
        [TestCase("Left 4 Dead 3", TestName = "ViewFriendWishList_Left4Dead3InWishlist")]
        public void Test_View_Wish_list(string gameTitle)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            
            Add_Each_other();

            AddGame_ToWishlist("test_user", "Qwe!23", gameTitle);
            AddGame_ToWishlist("test_user2", "Qwe!23", gameTitle);

            Login("test_user", "Qwe!23");

            NavToFriendList();

            // click View profile for test_user2
            driver.FindElement(By.XPath("//a[contains(text(),'VIEW PROFILE')]")).Click();

            // wait for test_user2 profile header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='test_user2']")));

            // check for game in wishlist
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//div[contains(text(),'" + gameTitle + "')]")));
            
            // log off
            driver.FindElement(By.XPath("//a[text()='Log off']")).Click();

            Login("test_user2", "Qwe!23");

            NavToFriendList();

            // click View profile for test_user2
            driver.FindElement(By.XPath("//a[contains(text(),'VIEW PROFILE')]")).Click();

            // wait for test_user2 profile header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='test_user']")));

            // check for game in wishlist
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//div[contains(text(),'" + gameTitle + "')]")));

            // log off
            driver.FindElement(By.XPath("//a[text()='Log off']")).Click();

            // remove game from wishlist
            RemoveGame_FromWishlist("test_user", "Qwe!23", gameTitle);
            RemoveGame_FromWishlist("test_user2", "Qwe!23", gameTitle);

            Assert.Pass();

        }

        public void Add_Each_other()
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // login 
            Login("test_user", "Qwe!23");

            NavToFriendList();

            try
            {
                // check to see if friend list is empty
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//p[text()='Your friend list is empty.']")));
            }
            catch
            {
                // if not empty click remove friend for friend_name
                driver.FindElement(By.XPath("//div[contains(text(),'test_user2')]/following-sibling::div/div/a[@title='Unfriend']")).Click();
            }

            // search for friend_name
            driver.FindElement(By.Id("search")).SendKeys("test_user2");
            driver.FindElement(By.XPath("//input[@type='submit' and @value='Search']")).Click();

            // wait for friend_name profile header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='test_user2']")));

            // click add to friends list button
            driver.FindElement(By.XPath("//a[text()='Add to Friend List']")).Click();

            // wait for friend_name profile header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//p[contains(text(),'test_user2')]")));

            // log off
            driver.FindElement(By.XPath("//a[text()='Log off']")).Click();

            // login 
            Login("test_user2", "Qwe!23");

            NavToFriendList();

            try
            {
                // check to see if friend list is empty
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//p[text()='Your friend list is empty.']")));
            }
            catch
            {
                // if not empty click remove friend for friend_name
                driver.FindElement(By.XPath("//div[contains(text(),'test_user')]/following-sibling::div/div/a[@title='Unfriend']")).Click();
            }

            // search for friend_name
            driver.FindElement(By.Id("search")).SendKeys("test_user");
            driver.FindElement(By.XPath("//input[@type='submit' and @value='Search']")).Click();

            // wait for friend_name profile header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='test_user']")));

            // click add to friends list button
            driver.FindElement(By.XPath("//a[text()='Add to Friend List']")).Click();

            // wait for friend_name profile header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//p[contains(text(),'test_user')]")));

            // log off
            driver.FindElement(By.XPath("//a[text()='Log off']")).Click();
        }

        public void AddGame_ToWishlist(string login, string password, string gameTitle)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // login 
            Login(login, password);

            // click to nav home
            driver.FindElement(By.XPath("//a[@href='/']")).Click();

            // wait for featured and recommended header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Featured and Recommended']")));

            // click on game
            driver.FindElement(By.XPath("//div[contains(text(),'"+ gameTitle + "')]")).Click();

            // wait for game header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='"+ gameTitle + "']")));

            // click Add To Wishlist
            driver.FindElement(By.XPath("//a[text()='Add to Wish List']")).Click();

            // log off
            driver.FindElement(By.XPath("//a[text()='Log off']")).Click();
        }


        public void RemoveGame_FromWishlist(string login, string password, string gameTitle)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // login 
            Login(login, password);

            // click to nav home
            driver.FindElement(By.XPath("//a[@href='/']")).Click();

            // wait for featured and recommended header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Featured and Recommended']")));

            // click on game
            driver.FindElement(By.XPath("//div[contains(text(),'" + gameTitle + "')]")).Click();

            // wait for game header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='" + gameTitle + "']")));

            // click Remove from Wishlist
            driver.FindElement(By.XPath("//a[text()='Remove from Wish List']")).Click();

            // log off
            driver.FindElement(By.XPath("//a[text()='Log off']")).Click();
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

    }
}
