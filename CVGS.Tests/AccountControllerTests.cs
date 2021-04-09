using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;

namespace CVGS.Tests
{
    [TestFixture]
    public class AccountControllerTests
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


        // test navigating to login page from the home page
        [Test]
        public void HomeIndexPage_GoToLoginPage_LogInAppearsInTitle()
        {
            // open the Home page
            driver.Navigate().GoToUrl(baseURL);

            // click on the "Log In"
            driver.FindElement(By.Id("loginLink")).Click();

            //wait for 10 seconds to check if the web page title is "Log in - Caesar Salad Gaming"
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.Title.Contains("Log in"));
        }


        // test log in - valid credentials - login successful
        [TestCase("Admin", "Qwe!23")]
        public void Login_ValidCredentials_Successful(string _displayName, string _password)
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

        public void NavToManageAccountDetails(string displayName, string password)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // login
            Login_ValidCredentials_Successful(displayName, password);

            // click 'Hello Admin!'
            driver.FindElement(By.XPath("//ul[contains(@class,'navbar-right')]//a[@href='#']")).Click();

            // click user account
            driver.FindElement(By.XPath("//a[@href='/Manage']")).Click();

            // wait for manage your account page
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Manage Your Account']")));
        }

        // test log in - invalid credentials - login fail
        [TestCase("Admins", "Qwe!23")]
        public void Login_InvalidCredentials_Fails(string _displayName, string _password)
        {
            var displayName = _displayName;
            var password = _password;

            driver.Navigate().GoToUrl(baseURL);

            driver.FindElement(By.Id("loginLink")).Click();
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.Title.Contains("Log in"));

            driver.FindElement(By.Id("DisplayName")).Clear();
            driver.FindElement(By.Id("DisplayName")).SendKeys(displayName);

            driver.FindElement(By.Id("Password")).Clear();
            driver.FindElement(By.Id("Password")).SendKeys(password);

            driver.FindElement(By.XPath("//input[@type='submit' and @value='Log in']")).Click();

            wait.Until(d => d.Title.Contains("Log in"));

            string error = driver.FindElement(By.XPath("//div[contains(@class,'validation-summary-errors')]//li")).Text;
            Assert.AreEqual("Invalid login attempt.", error);
        }

        // test creating account - invalid input - create fail
        [TestCase("test_user", "Qwe!23", "test@user", "Test", "User", "2022-02-27", "Male")]
        public void Register_InvalidInputs_Fails(string _displayName, string _password, string _email, string _firstName, string _lastName, string _dateOfBirth, string _sex)
        {
            string[,] testValues = new string[,]
            {
                {"DisplayName", _displayName},
                {"Password", _password},
                {"ConfirmPassword", _password},
                {"Email", _email},
                {"FirstName", _firstName},
                {"LastName", _lastName},
                {"BirthDate", _dateOfBirth}
            };

            driver.Navigate().GoToUrl(baseURL);

            driver.FindElement(By.Id("registerLink")).Click();
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.Title.Contains("Register"));

            for (int i = 0; i < testValues.GetLength(0); i++)
            {
                driver.FindElement(By.XPath("//input[@name='" + testValues[i, 0] + "']")).Clear();
                driver.FindElement(By.XPath("//input[@name='" + testValues[i, 0] + "']")).SendKeys(testValues[i, 1]);
            }

            driver.FindElement(By.XPath("//select[@name='Sex']")).Click();
            switch (_sex)
            {
                case "Male":
                    driver.FindElement(By.XPath("//select[@name='Sex']/option[@value='0']")).Click();
                    break;
                case "Female":
                    driver.FindElement(By.XPath("//select[@name='Sex']/option[@value='1']")).Click();
                    break;
                default:
                    driver.FindElement(By.XPath("//select[@name='Sex']/option[@value='2']")).Click();
                    break;
            }

            driver.FindElement(By.XPath("//input[@type='submit' and @value='Register']")).Click();

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

            string errorBirthDate = driver.FindElement(By.XPath("//span[@data-valmsg-for='BirthDate']")).Text;
            string errorEmail = driver.FindElement(By.XPath("//span[@data-valmsg-for='Email']")).Text;

            Assert.AreEqual("The Email field is not a valid e-mail address.", errorEmail);
            Assert.AreEqual("You must be at least 18 years of age to register.", errorBirthDate);
        }

        // test creating account - invalid input - create fail
        [TestCase("test_user", "Qwe!23", "test@user.com", "Test", "User", "2000-02-27", "Male", TestName = "Register_ValidInputsTestUser_Successful")]
        [TestCase("test_user2", "Qwe!23", "test2@user.com", "Test", "User", "2000-02-27", "Male", TestName = "Register_ValidInputsTestUser2_Successful")]
        public void Register_ValidInputs_Successful(string _displayName, string _password, string _email, string _firstName, string _lastName, string _dateOfBirth, string _sex)
        {
            string[,] testValues = new string[,]
            {
                {"DisplayName", _displayName},
                {"Password", _password},
                {"ConfirmPassword", _password},
                {"Email", _email},
                {"FirstName", _firstName},
                {"LastName", _lastName},
                {"BirthDate", _dateOfBirth}
            };

            driver.Navigate().GoToUrl(baseURL);

            driver.FindElement(By.Id("registerLink")).Click();
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.Title.Contains("Register"));

            for (int i = 0; i < testValues.GetLength(0); i++)
            {
                driver.FindElement(By.XPath("//input[@name='" + testValues[i, 0] + "']")).Clear();
                driver.FindElement(By.XPath("//input[@name='" + testValues[i, 0] + "']")).SendKeys(testValues[i, 1]);
            }

            driver.FindElement(By.XPath("//select[@name='Sex']")).Click();
            switch (_sex)
            {
                case "Male":
                    driver.FindElement(By.XPath("//select[@name='Sex']/option[@value='0']")).Click();
                    break;
                case "Female":
                    driver.FindElement(By.XPath("//select[@name='Sex']/option[@value='1']")).Click();
                    break;
                default:
                    driver.FindElement(By.XPath("//select[@name='Sex']/option[@value='2']")).Click();
                    break;
            }

            driver.FindElement(By.XPath("//input[@type='submit' and @value='Register']")).Click();

            wait.Until(d => d.Title.Contains("Manage Your Account"));
        }


        [TestCase("4567876545678765", 12, "2020", true, false, TestName="ValidVisaCard_Added")]
        [TestCase("5555555555554444", 11, "2021", true, false, TestName="ValidMasterCard_Added")]
        [TestCase("555555555555444412345", 11, "2021", false, true, TestName="InvalidCreditCard_NotAdded")]
        public void Test_CreditCard(string cardNumber, int expiryMonth, string expiryYear, bool deleteCard, bool invalid)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

            string[] months = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            string formattedCreditCard = cardNumber.Substring(0, 4) + ' ' + cardNumber.Substring(4, 4) + ' ' +
                                         cardNumber.Substring(8, 4) + ' ' + cardNumber.Substring(12, 4);


            // navigate to account details
            NavToManageAccountDetails("Admin", "Qwe!23");

            // click Add New Credit Card
            driver.FindElement(By.XPath("//button[text()='Add New Credit Card']")).Click();

            // wait for add new credit card popup
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h4[text()='Add Credit Card']")));

            // input credit card number
            driver.FindElement(By.XPath("//input[@id='creditCardNumber']")).SendKeys(cardNumber);

            // click expiration date input field
            driver.FindElement(By.XPath("//input[@id='expirationDate']")).Click();

            // click month select
            driver.FindElement(By.XPath("//select[@class='ui-datepicker-month']")).Click();

            // click month option in month select
            driver.FindElement(By.XPath("//select[@class='ui-datepicker-month']//option[text()='" + months[expiryMonth-1] + "']")).Click();

            // click year select
            driver.FindElement(By.XPath("//select[@class='ui-datepicker-year']")).Click();

            // click year option in year select
            driver.FindElement(By.XPath("//select[@class='ui-datepicker-year']//option[text()='" + expiryYear + "']")).Click();

            // click done in date picker
            driver.FindElement(By.XPath("//button[text()='Done']")).Click();

            // click add in Add Credit Card popup
            driver.FindElement(By.XPath("//button[@id='btnAdd']")).Click();

            if (invalid)
            {
                // wait to see if there is an error
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//span[@id='cardNumberValidationError']")));

                // check error matches
                string invalidCreditCardText = driver.FindElement(By.XPath("//span[@id='cardNumberValidationError']")).Text;
                Assert.AreEqual("Please provide a valid credit card number.", invalidCreditCardText);

                // click close
                driver.FindElement(By.XPath("//button[text()='Close']")).Click();
            }
            else
            {
                Thread.Sleep(100);

                driver.Navigate().GoToUrl(baseURL + "/Manage");

                // wait for manage your account page
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Manage Your Account']")));

                // check the new credit card has been added
                string expiryValue = driver.FindElement(By.XPath("//td[text()='" + formattedCreditCard + "']/following-sibling::td")).Text;
                Assert.AreEqual(expiryMonth.ToString() + "/" + expiryYear, expiryValue);
            }

            if (deleteCard)
            {
                // delete new credit card
                driver.FindElement(By.XPath("//tr[.//td[text()='" + formattedCreditCard + "']]/td/button[contains(@onclick,'deleteCreditCard')]")).Click();

                // press ok on delete alert
                driver.SwitchTo().Alert().Accept();
            }
        }
    }
}
