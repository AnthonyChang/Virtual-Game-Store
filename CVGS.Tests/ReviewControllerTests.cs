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
    public class ReviewControllerTests
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


        [TestCase ("Great Game", false, false, TestName = "AddReview_AdminApprove_ReviewAdded")]
        [TestCase("Not sure if I want to post this", true, false, TestName = "AddReview_CloseReview_NoReview")]
        [TestCase("Awful Game", false, true, TestName = "AddReview_AdminDeny_NoReview")]
        public void Test_AddReview(string reviewText, bool close, bool adminDeny)
        {

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(1000));

            Add_Game("ReviewTestGame", "2020", "Test Game For review", "10.00", false);

            // click to nav home
            driver.FindElement(By.XPath("//a[@href='/']")).Click();

            // wait for featured and recommended header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Featured and Recommended']")));

            // click on new test game created
            driver.FindElement(By.XPath("//div[contains(text(),'ReviewTestGame')]")).Click();

            // wait for game header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='ReviewTestGame']")));

            // click Add Review
            driver.FindElement(By.XPath("//button[text()='Add Review']")).Click();

            // wait for review popup header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h4[text()='Write a Review']")));

            // input review into review text area 
            driver.FindElement(By.XPath("//textarea[@id='reviewContent']")).SendKeys(reviewText);

            if (close)
            {
                // clear field
                driver.FindElement(By.XPath("//textarea[@id='reviewContent']")).Clear();

                // click add
                driver.FindElement(By.XPath("//button[@id='btnAddReview' and text()='Add']")).Click();

                // check to see if error is present
                string reviewErrorValue = driver.FindElement(By.XPath("//span[@id='reviewValidationError']")).Text;
                Assert.AreEqual("Content can not be empty.", reviewErrorValue);

                // click Close
                driver.FindElement(By.XPath("//button[text()='Close']")).Click();

                driver.Navigate().GoToUrl(baseURL + "/Admin/Review");
            }
            else
            {
                // click add
                driver.FindElement(By.XPath("//button[@id='btnAddReview' and text()='Add']")).Click();

                // wait for notice to popup
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h4[@id='noticeModalTitle']")));

                // click ok
                driver.FindElement(By.XPath("//button[@id='btnAddReview' and text()='Ok']")).Click();

                // click Administration
                driver.FindElement(By.XPath("//a[contains(text(),'Administration')]")).Click();

                // click Manage Reviews
                driver.FindElement(By.XPath("//a[@href='/Admin/Review']")).Click();
            }

            // wait for Reviews list to appear 
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Unapproved Reviews List']")));

            if (!close)
            {
                if (adminDeny)
                {
                    // click Decline
                    driver.FindElement(By.XPath("//tr[./td[text()='ReviewTestGame']]/td/a[text()='Decline']")).Click();
                }
                else
                {
                    // click approve
                    driver.FindElement(By.XPath("//tr[./td[text()='ReviewTestGame']]/td/a[text()='Approve']")).Click();
                }
            }

            // click to nav home
            driver.FindElement(By.XPath("//a[@href='/']")).Click();

            // wait for featured and recommended header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Featured and Recommended']")));

            // click on new test game created
            driver.FindElement(By.XPath("//div[contains(text(),'ReviewTestGame')]")).Click();

            // wait for game header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='ReviewTestGame']")));

            // check to see if review is posted
            string reviewValue;
            try
            {
                reviewValue = driver.FindElement(By.XPath("//div[@id='reviews']//div[contains(text(),'" + reviewText + "')]")).Text;
            }
            catch
            {
                reviewValue = " ";
            }


            if (close || adminDeny)
            {
                Assert.AreNotEqual(reviewText, reviewValue);
            }
            else
            {
                Assert.AreEqual(reviewText, reviewValue);

                // click on delete for new review
                driver.FindElement(By.XPath("//div[@id='reviews']//div[contains(text(),'" + reviewText + "')]/following-sibling::div/button[./span[contains(@class,'remove')]]")).Click();

                // wait for delete your review popup
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h4[text()='Delete Your Review:']")));

                // click delete
                driver.FindElement(By.XPath("//button[@id='btnDeleteReview']")).Click();

                // wait for game header
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='ReviewTestGame']")));
            }

            // navigate to manage games 
            NavToManageGames();

            // delete test game
            DeleteGame_fromManageGames("ReviewTestGame");
        }
    }
}
