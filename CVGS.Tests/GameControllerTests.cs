using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace CVGS.Tests
{
    [TestFixture]
    public class GameControllerTests
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


        [TestCase("TestGame", "2020", "Test Description", "20.20", false, true, TestName ="AddGame_GameAdded")]
        [TestCase("Summer-Colored High School Adolescent Record – A Summer At School On An Island Where I Contemplate " +
            "How The First Day After I Transferred, I Ran Into A Childhood Friend And Was Forced To Join The Journalism " +
            "Club Where While My Days As A Paparazzi Kid With Great Scoops Made Me Rather Popular Among The Girls, But " +
            "Strangely My Camera Is Full Of Panty Shots, And Where My Candid Romance Is Going", "2015", "IntroducingD3 " +
            "Publisher's open world love-adventure and photography game! As you arrive on the island of Yumegashima for " +
            "your three month stint at Natsuiro High School you'll join a full cast of characters, including the " +
            "all-female journalism club that haveenlisted you as their new photographer! Players can freely explore " +
            "their new island home by choosing between over 150 quests to carry out, interacting with the 300 inhabitants," +
            " fishing for strange items, and of course taking pictures of whatever, or whomever, inspires you! Throughout" +
            " the gameplayers will see characters with an exclamation markover their head, which indicates that players" +
            " will have to make adecision when talking with them.This will often mean, but isnot limited to, choosing" +
            " whether or not to accompany a girl from the journalism club as their photographer.Visiting the correct" +
            " locations could mean changing the outing from work-related to a date! The protagonist doesn't always have" +
            " the best of intentions when he is behind the camera, and as a result the subject matter is as lewd as" +
            " you want to be;from panty shots to those of scenic vistasand everything in-between. You'll have to watch" +
            " your back though as there is a whole list of activities that could put the police, or your school" +
            " councillor, hot on your trail!", "14.99", false, true, TestName = "AddGame_LongTitle_GameAdded")]
        [TestCase("Microsoft Flight Simulator", "2021", "From light planes to wide-body jets, fly highly detailed and " +
            "accurate aircraft in the next generation of Microsoft Flight Simulator. Test your piloting skills against " +
            "the challenges of night flying, real-time atmospheric simulation and live weather in a dynamic and living " +
            "world.", "79.99", true, true, TestName = "AddGame_ReleaseYear2021_GameNotAdded")]
        public void AddGame_ValidData_GameCreated(string _gameTitle, string _releaseYear, string _description, string _price, bool invalid, bool _delete)
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

            if (invalid)
            {
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//span[@data-valmsg-for='ReleaseYear']")));
                string releaseYearErrorText = driver.FindElement(By.XPath("//span[@data-valmsg-for='ReleaseYear']")).Text;
                Assert.AreEqual("The release year must be between 1985 and 2020.", releaseYearErrorText);
            }
            else
            {
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


        [TestCase("1", false, false, TestName = "RateGame_1_GameRated1")] // making sure ratings work
        [TestCase("2", false, false, TestName = "RateGame_2_GameRated2")]
        [TestCase("3", false, false, TestName = "RateGame_3_GameRated3")]
        [TestCase("4", false, false, TestName = "RateGame_4_GameRated4")]
        [TestCase("5", false, false, TestName = "RateGame_5_GameRated5")]
        [TestCase("1", true, false, TestName = "RateGame_1_ReRateGame2_ClickSave_GameRated2")] // making sure same user rating is updated and not added
        [TestCase("1", true, true, TestName = "RateGame_1_ReRateGame2_ClickClose_GameRated1")] // making sure close button does not update rating
        public void RateGame_DifferentOptions_Successful(string _rating, bool reRate, bool close)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // make a new test game to rate
            AddGame_ValidData_GameCreated("RateGameTest","2020","RateTest","10.00", false, false);

            // click to nav home
            driver.FindElement(By.XPath("//a[@href='/']")).Click();

            // wait for featured and recommended header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Featured and Recommended']")));

            // click on new test game created
            driver.FindElement(By.XPath("//div[contains(text(),'RateGameTest')]")).Click();

            // wait for game header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='RateGameTest']")));

            // click rate game for test game
            driver.FindElement(By.XPath("//button[text()='Rate Game']")).Click();

            // wait for rate game popup to show
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h4[text()='How do you rate this game?']")));

            // click on rating drop down
            driver.FindElement(By.XPath("//select[@name='rating']")).Click();

            // click on rating for test
            driver.FindElement(By.XPath("//select[@name='rating']//option[@value='"+ _rating +"']")).Click();

            // click save on rating popup
            driver.FindElement(By.XPath("//button[@id='btnSaveRating']")).Click();

            // wait for game header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='RateGameTest']")));

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
            // get rating value from game page
            string ratingValue = driver.FindElement(By.XPath("//p[contains(text(),'Rated')]")).Text;

            // compare rating on page with rating given in the test
            Assert.AreEqual(ratingValue, "Rated " + _rating + ".00 out of 5");

            if (reRate)
            {
                // click rate game for test game
                driver.FindElement(By.XPath("//button[text()='Rate Game']")).Click();

                // wait for rate game popup to show
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h4[text()='How do you rate this game?']")));
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

                // click on rating drop down
                driver.FindElement(By.XPath("//select[@name='rating']")).Click();

                // click on rating for test
                driver.FindElement(By.XPath("//select[@name='rating']//option[@value='2']")).Click();

                if (close)
                {
                    // click close on rating popup
                    driver.FindElement(By.XPath("//div[@id='ratingModal']//button[text()='Close']")).Click();

                    // wait for game header
                    wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='RateGameTest']")));

                    // get rating value from game page
                    string reRatingValue = driver.FindElement(By.XPath("//p[contains(text(),'Rated')]")).Text;

                    // compare rating on page with rating given in the test
                    Assert.AreEqual(reRatingValue, "Rated " + _rating + ".00 out of 5");

                    // nav to manage games
                    driver.Navigate().GoToUrl(baseURL + "/Admin/Game");

                    // delete test game created for test
                    DeleteGame_fromManageGames("RateGameTest");
                }
                else
                {
                    // click save on rating popup
                    driver.FindElement(By.XPath("//button[@id='btnSaveRating']")).Click();

                    // wait for game header
                    wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='RateGameTest']")));

                    // get rating value from game page
                    string reRatingValue = driver.FindElement(By.XPath("//p[contains(text(),'Rated')]")).Text;

                    // compare rating on page with rating given in the test
                    Assert.AreEqual(reRatingValue, "Rated " + "2" + ".00 out of 5");
                }
            }

            if (!close)
            {
                // nav to manage games
                NavToManageGames();

                // delete test game created for test
                DeleteGame_fromManageGames("RateGameTest");
            }
        }
    }
}
