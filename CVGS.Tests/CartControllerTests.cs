using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVGS.Tests
{
    [TestFixture]
    class CartControllerTests
    {
        private IWebDriver driver;
        private string baseURL;

        // instantiate FireFox driver
        [SetUp]
        public void SetUpMethod()
        {
            FirefoxOptions options = new FirefoxOptions();
            options.SetPreference("browser.download.folderList", 2);
            options.SetPreference("browser.download.dir", @"C:\Downloads");
            options.SetPreference("browser.helperApps.neverAsk.saveToDisk", "text/plain");
            driver = new FirefoxDriver(options);
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


        [TestCase("CartTest", 20.20, false, true, true, TestName = "AddToCart_Purchased")]
        [TestCase("CartTestAndCheckGamePage", 20.20, true, true, true, TestName = "AddToCart_Purchased_ConfirmedInLibrary")]
        [TestCase("CartTestDidNotPlaceOrder", 20.20, false, false, true, TestName = "AddToCart_NotPurchased_GameDetailsInCart")]
        public void Test_AddToCart_ItemPurchased(string testGameTitle, double testGamePrice, bool checkGamePage, bool placeOrder, bool deleteGame)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // Add a test game to add to cart
            Add_Game(testGameTitle, "2020", "Test Description", string.Format("{0:0.00}", testGamePrice), false);

            // click to nav home
            driver.FindElement(By.XPath("//a[@href='/']")).Click();

            // wait for featured and recommended header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Featured and Recommended']")));

            // click on new test game created
            driver.FindElement(By.XPath("//div[contains(text(),'"+ testGameTitle + "')]")).Click();

            // wait for game header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='"+ testGameTitle + "']")));

            // click add to cart
            driver.FindElement(By.XPath("//a[text()='Add to Cart']")).Click();

            // check to see if cart updates with number of games in cart
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//span[@id='lblCartCount' and text()='1']")));

            // click cart
            driver.FindElement(By.XPath("//a[@href='/Cart']")).Click();
            
            // wait for cart page header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Your Shopping Cart']")));

            // check test game title 
            string testGameTitleText = driver.FindElement(By.XPath("//div[text()='" + testGameTitle + "']")).Text;
            Assert.AreEqual(testGameTitleText, testGameTitle);

            // check test game price 
            string testGamePriceText = driver.FindElement(By.XPath("//div[text()='" + testGameTitle + "']/following-sibling::div")).Text;
            Assert.AreEqual(string.Format("${0:0.00}", testGamePrice), testGamePriceText);

            // click checkout 
            driver.FindElement(By.XPath("//a[@href='/Cart/Checkout']")).Click();

            // wait for checkout page header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Checkout']")));

            // check for game order total
            string orderTotalText = driver.FindElement(By.XPath("//div[text()='Order Total:']/following-sibling::div")).Text;
            Assert.AreEqual(string.Format("${0:0.00}", testGamePrice * 1.13), orderTotalText);

            // check for payment method header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h4[text()='Select a Payment Method']")));

            if (placeOrder)
            {
                // click place order button
                driver.FindElement(By.XPath("//input[@type='submit' and @value='Place Order']")).Click();

                // check for page header after checkout
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='The following games have been added to your library. Thank you for your purchase.']")));
            }
            else
            {
                // click home
                driver.FindElement(By.XPath("//a[@class='navbar-brand' and text()='Caesar Salad Gaming']")).Click();

                // wait for games to show up at home page
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//div[contains(text(), '" + testGameTitle + "')]")));

                // go to game almost purchased
                driver.FindElement(By.XPath("//div[contains(text(), '" + testGameTitle + "')]")).Click();

                // check for game header
                driver.FindElement(By.XPath("//h2[contains(text(), '" + testGameTitle + "')]")).Click();

                // check for this game is in your shopping cart
                string gameInCartText = driver.FindElement(By.XPath("//p[text()='This game is in your shopping cart.']")).Text;
                Assert.AreEqual("This game is in your shopping cart.", gameInCartText);
            }

            if (checkGamePage)
            {
                // click hello admin
                driver.FindElement(By.XPath("//ul[contains(@class,'navbar-right')]//a[@href='#']")).Click();

                // click My Library
                driver.FindElement(By.XPath("//a[@href='/Home/Library']")).Click();

                // wait for Games Library page header
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='My Games Library']")));

                // click new game recently purchased in library
                driver.FindElement(By.XPath("//div[contains(text(), '" + testGameTitle + "')]")).Click();

                // check for you own this game text on game page
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//p[text()='You Own This Game']")));

                // check download game button is present on game page of game purchased
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//button[@onclick='downloadGame();']")));
            }

            // cleanup
            if (deleteGame)
            {
                NavToManageGames();
                DeleteGame_fromManageGames(testGameTitle);
            }
        }

        [TestCase("TestDownloadFromCart", false, true, true, TestName = "GamePurchased_DownloadFromCart_Downloaded")] // test download from cart
        [TestCase("TestDownloadFromGamePage", true, true, true, TestName = "GamePurchased_DownloadFromGamePage_Downloaded")]  // test download from game page after checkout
        [TestCase("TestNoCheckoutNoDownloadButton", false, false, true, TestName = "GameNotPurchased_GameInCart_NoDownloadButton")]  // test no download button on games page if did not checkout
        public void Test_Download_FromGamePage_GameDownloaded(string testGameTitle, bool checkGamePage, bool placeOrder, bool deleteGame)
        {
            string expectedFilePath = @"C:/Downloads/" + testGameTitle + ".txt";
            bool fileExists = false;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // add game to test download
            Test_AddToCart_ItemPurchased(testGameTitle, 20.20, checkGamePage, placeOrder, false);

            if (placeOrder)
            {
                // click donwload
                driver.FindElement(By.XPath("//button[contains(@onclick,'downloadGame(')]")).Click();

                // wait for download
                wait.Until<bool>(x => fileExists = File.Exists(expectedFilePath));

                // check to see if file exists after download
                FileInfo fileInfo = new FileInfo(expectedFilePath);
                Assert.AreEqual(fileInfo.Name, testGameTitle + ".txt");
                Assert.AreEqual(fileInfo.FullName, @"C:\Downloads\" + testGameTitle + ".txt");
            }

            // clean up
            if (deleteGame)
            {
                NavToManageGames();
                DeleteGame_fromManageGames(testGameTitle);
                File.Delete(expectedFilePath);
            }
        }
    }
}
