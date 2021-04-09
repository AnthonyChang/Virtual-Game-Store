using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;

namespace CVGS.Tests
{
    [TestFixture]
    public class ManageControllerTests
    {

        private IWebDriver driver;
        private string baseURL;


        // instantiate FireFox driver
        [SetUp]
        public void SetUpMethod()
        {
            driver = new ChromeDriver();
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

        #region login as admin and navigate to Account Management page
        // login function
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
        }

        public void NavToManageAccountDetails(string displayName, string password)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // login
            Login(displayName, password);

            // navigate to Manage Account URL
            driver.Navigate().GoToUrl(baseURL + "/Manage");

            // wait for manage your account page
            wait.Until(d => d.Title.Contains("Manage Your Account"));
        }
        #endregion

        #region edit account information tests
        [TestCase("Admin", "Qwe!23", "002020-01-01")]
        public void A_EditAccountInformation_InvalidBirthDate_Fails(string _displayName, string _password, string _birthDate)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // navigate to account details
            Login("Admin", "Qwe!23");
            driver.Navigate().GoToUrl(baseURL + "/Manage/ChangeAccountDetails");

            // change the date of birth
            driver.FindElement(By.XPath("//input[@name='BirthDate']")).Clear();
            driver.FindElement(By.XPath("//input[@name='BirthDate']")).SendKeys(_birthDate);

            // click Save
            driver.FindElement(By.XPath("//input[@type='submit' and @value='Save']")).Click();

            // wait for 3 seconds
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

            // make sure we're still on the same page
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Edit Account Details']")));

            // check the error message
            string errorBirthDate = driver.FindElement(By.XPath("//span[@data-valmsg-for='BirthDate']")).Text;


            Assert.AreEqual("You must be at least 18 years of age.", errorBirthDate);
        }

        [TestCase("Admin", "Qwe!23", "admin@email")]
        public void B_EditAccountInformation_InvalidEmailAddress_Fails(string _displayName, string _password, string _emailAddress)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            Login("Admin", "Qwe!23");
            driver.Navigate().GoToUrl(baseURL + "/Manage/ChangeAccountDetails");

            // change the date of birth
            driver.FindElement(By.XPath("//input[@name='Email']")).Clear();
            driver.FindElement(By.XPath("//input[@name='Email']")).SendKeys(_emailAddress);

            // click Save
            driver.FindElement(By.XPath("//input[@type='submit' and @value='Save']")).Click();

            // wait for 3 seconds
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

            // make sure we're still on the same page
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Edit Account Details']")));

            // check the error message
            string errorMessage = driver.FindElement(By.XPath("//span[@data-valmsg-for='Email']")).Text;

            StringAssert.AreEqualIgnoringCase("The Email field is not a valid e-mail address.", errorMessage);
        }

        [TestCase("Admin", "Qwe!23", "LastName")]
        public void C_EditAccountInformation_MissingRequiredField_Fails(string _displayName, string _password, string _fieldName)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // Navigate to Manage Account Details
            Login("Admin", "Qwe!23");
            driver.Navigate().GoToUrl(baseURL + "/Manage/ChangeAccountDetails");

            // change the date of birth
            driver.FindElement(By.XPath("//input[@name='" + _fieldName + "']")).Clear();
            driver.FindElement(By.XPath("//input[@name='" + _fieldName + "']")).SendKeys(string.Empty);

            // click Save
            driver.FindElement(By.XPath("//input[@type='submit' and @value='Save']")).Click();

            // wait for 3 seconds
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

            // make sure we're still on the same page
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Edit Account Details']")));

            // check the error message
            string errorMessage = driver.FindElement(By.XPath("//span[@data-valmsg-for='" + _fieldName + "']")).Text;


            StringAssert.Contains("required", errorMessage);
        }

        [TestCase("Admin", "Qwe!23", "Admin1", "Admin1@admin.com", "Admin1", "Admin2", "Password1!", "Male")]
        [TestCase("Admin1", "Password1!", "Admin", "Admin@admin.com", "Admin", "Admin", "Qwe!23", "I prefer not to say")]
        public void D_EditAccountInformation_ValidData_Successful(string initialDisplayName, string initialPassword, string _displayName, string _email, string _firstName, string _lastName, string _password, string _sex)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            string[,] testValues = new string[,]
            {
                {"DisplayName", _displayName},
                {"Email", _email},
                {"FirstName", _firstName},
                {"LastName", _lastName}
            };

            // Navigation to Manage Account Details
            NavToManageAccountDetails(initialDisplayName, initialPassword);

            // click Edit Account Details
            driver.FindElement(By.XPath("//a[@href='/Manage/ChangeAccountDetails']")).Click();

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
            // wait for Edit Account Details page
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Edit Account Details']")));

            // change account information (Display Name, Email, First Name, Last Name)
            for (int i = 0; i < testValues.GetLength(0); i++)
            {
                driver.FindElement(By.XPath("//input[@name='" + testValues[i, 0] + "']")).Clear();
                driver.FindElement(By.XPath("//input[@name='" + testValues[i, 0] + "']")).SendKeys(testValues[i, 1]);
            }

            // change sex
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


            // click Save
            driver.FindElement(By.XPath("//input[@type='submit' and @value='Save']")).Click();

            // wait for manage your account page
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Manage Your Account']")));

            // check display name in Manage Account
            string displayNameValue = driver.FindElement(By.XPath("//div[./b[text()='Display Name']]/following-sibling::div")).Text;
            Assert.AreEqual(testValues[0, 1], displayNameValue);

            // check email in Manage Account
            string emailValue = driver.FindElement(By.XPath("//div[./b[text()='" + testValues[1, 0] + "']]/following-sibling::div")).Text;
            Assert.AreEqual(testValues[1, 1], emailValue);

            // check actual name in Manage Account
            string actualNameValue = driver.FindElement(By.XPath("//div[./b/label[text()='Actual Name']]/following-sibling::div")).Text;
            Assert.AreEqual(testValues[2, 1] + " " + testValues[3, 1], actualNameValue);

            // check Sex
            string sexValue = driver.FindElement(By.XPath("//div[./b[text()='Sex']]/following-sibling::div")).Text;
            Assert.AreEqual(_sex, sexValue);

            // click Change your password
            driver.FindElement(By.XPath("//a[@href='/Manage/ChangePassword']")).Click();

            // wait for Change Password page
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Change Password.']")));

            // enter current password
            driver.FindElement(By.XPath("//input[@name='OldPassword']")).Clear();
            driver.FindElement(By.XPath("//input[@name='OldPassword']")).SendKeys(initialPassword);

            // enter new password
            driver.FindElement(By.XPath("//input[@name='NewPassword']")).Clear();
            driver.FindElement(By.XPath("//input[@name='NewPassword']")).SendKeys(_password);

            // confirm new password
            driver.FindElement(By.XPath("//input[@name='ConfirmPassword']")).Clear();
            driver.FindElement(By.XPath("//input[@name='ConfirmPassword']")).SendKeys(_password);

            // click Change Password
            driver.FindElement(By.XPath("//input[@type='submit' and @value='Change password']")).Click();

            // wait for manage your account page
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Manage Your Account']")));

            // click Log off
            driver.FindElement(By.XPath("//a[text()='Log off']")).Click();

            // navigation to Manage Account Details
            NavToManageAccountDetails(_displayName, _password);

            // check display name in Manage Account
            string displayNameValue2 = driver.FindElement(By.XPath("//div[./b[text()='Display Name']]/following-sibling::div")).Text;
            Assert.AreEqual(testValues[0, 1], displayNameValue2);
        }
        #endregion

        #region member preferences tests
        [TestCase("Admin", "Qwe!23")]
        public void E_MemberPreferences_SetSendPromotionalEmails_Successful(string _displayName, string _password)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            NavToManageAccountDetails(_displayName, _password);

            var emailNotificationBefore = driver.FindElement(By.XPath("//div[@class='col-md-2'][./b[text()='Email Notifications']]/following-sibling::div")).Text;

            // click on Edit Account Details button
            driver.FindElement(By.XPath("//a[@href='/Manage/ChangeAccountDetails']")).Click();

            // wait for Edit Account Details page to load
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Edit Account Details']")));

            // click on the checkbox
            driver.FindElement(By.XPath("//input[@name='SendPromotionalEmails']")).Click();

            // click Save
            driver.FindElement(By.XPath("//input[@type='submit' and @value='Save']")).Click();

            // wait for manage your account page to load
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Manage Your Account']")));

            // get the new value of the Send Promotional Emails checkbox
            var emailNotificationAfter = driver.FindElement(By.XPath("//div[@class='col-md-2'][./b[text()='Email Notifications']]/following-sibling::div")).Text;

            Assert.AreNotEqual(emailNotificationBefore, emailNotificationAfter);
        }

        [TestCase("Admin", "Qwe!23", new string[] { "Action", "Adventure", "RolePlaying", "Simulation", "Strategy", "Puzzle" })]
        public void F_MemberPreferences_SetGameCategories_Successful(string initialDisplayName, string initialPassword, string[] selectedPreferences)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            string[] check = new string[6];
            string[] checkAfter = new string[6];

            NavToManageAccountDetails(initialDisplayName, initialPassword);

            // get initial preference checked values
            for (int i = 0; i < selectedPreferences.GetLength(0); i++)
            {
                try
                {
                    var checkedElement = driver.FindElement(By.XPath("//div[@class='col-md-2'][./b[text()='Favorite Game Genres']]/following-sibling::div[contains(text(),'" + selectedPreferences[i] + "')]")).Text;
                    check[i] = "checked";
                }
                catch
                {
                    check[i] = " ";
                }
            }

            // click Edit Account Details button
            driver.FindElement(By.XPath("//a[@href='/Manage/ChangeAccountDetails']")).Click();

            // wait for Edit Account Details page to load
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Edit Account Details']")));

            for (int i = 0; i < selectedPreferences.GetLength(0); i++)
            {
                driver.FindElement(By.XPath("//input[@name='" + selectedPreferences[i] + "Checked']")).Click();
            }

            // click Save
            driver.FindElement(By.XPath("//input[@type='submit' and @value='Save']")).Click();

            // wait for manage your account page to load
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Manage Your Account']")));

            // get values of Game Category checkboxes
            for (int i = 0; i < selectedPreferences.GetLength(0); i++)
            {
                try
                {
                    var checkedElement = driver.FindElement(By.XPath("//div[@class='col-md-2'][./b[text()='Favorite Game Genres']]/following-sibling::div[contains(text(),'" + selectedPreferences[i] + "')]")).Text;
                    checkAfter[i] = "checked";
                }
                catch
                {
                    checkAfter[i] = " ";
                }
            }

            // check values are not the same as changed values
            for (int i = 0; i < selectedPreferences.GetLength(0); i++)
            {
                Assert.AreNotEqual(check[i], checkAfter[i]);
            }
        }

        [TestCase("Admin", "Qwe!23", new string[] { "PC", "PlayStation", "Xbox", "Nintendo", "Mobile" })]
        public void G_MemberPreferences_SetPlatformsToOpposite_Successful(string initialDisplayName, string initialPassword, string[] selectedPreferences)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            string[] check = new string[6];
            string[] checkAfter = new string[6];

            NavToManageAccountDetails(initialDisplayName, initialPassword);

            // get initial preference checked values
            for (int i = 0; i < selectedPreferences.GetLength(0); i++)
            {
                try
                {
                    var checkedElement = driver.FindElement(By.XPath("//div[@class='col-md-2'][./b[text()='Favorite Platforms']]/following-sibling::div[contains(text(),'" + selectedPreferences[i] + "')]")).Text;
                    check[i] = "checked";
                }
                catch
                {
                    check[i] = " ";
                }
            }

            // click Edit Account Details
            driver.FindElement(By.XPath("//a[@href='/Manage/ChangeAccountDetails']")).Click();

            // wait for Edit Account Details page to load
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Edit Account Details']")));

            for (int i = 0; i < selectedPreferences.GetLength(0); i++)
            {

                driver.FindElement(By.XPath("//input[@name='" + selectedPreferences[i] + "Checked']")).Click();
            }

            // click Save
            driver.FindElement(By.XPath("//input[@type='submit' and @value='Save']")).Click();

            // wait for manage your account page
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Manage Your Account']")));

            for (int i = 0; i < selectedPreferences.GetLength(0); i++)
            {
                try
                {
                    var checkedElement = driver.FindElement(By.XPath("//div[@class='col-md-2'][./b[text()='Favorite Platforms']]/following-sibling::div[contains(text(),'" + selectedPreferences[i] + "')]")).Text;
                    checkAfter[i] = "checked";
                }
                catch
                {
                    checkAfter[i] = " ";
                }
            }

            // check values are not the same as changed values
            for (int i = 0; i < selectedPreferences.GetLength(0); i++)
            {
                Assert.AreNotEqual(check[i], checkAfter[i]);
            }
        }

        [TestCase("Admin", "Qwe!23", new string[] { "Action", "Adventure", "RolePlaying", "Simulation", "Strategy", "Puzzle" })]
        [TestCase("Admin", "Qwe!23", new string[] { "PC", "PlayStation", "Xbox", "Nintendo", "Mobile" })]
        public void H_MemberPreferences_ClearPreferences_Successful(string initialDisplayName, string initialPassword, string[] selectedPreferences)
        {
            string[] genreList = new string[] { "Action", "Adventure", "RolePlaying", "Simulation", "Strategy", "Puzzle" };
            string[] platformList = new string[] { "PC", "PlayStation", "Xbox", "Nintendo", "Mobile" };

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            NavToManageAccountDetails(initialDisplayName, initialPassword);

            // click Edit Account Details button
            driver.FindElement(By.XPath("//a[@href='/Manage/ChangeAccountDetails']")).Click();

            // wait for Edit Account Details page to load
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Edit Account Details']")));

            for (int i = 0; i < selectedPreferences.GetLength(0); i++)
            {
                var checkedElement = driver.FindElement(By.XPath("//input[@name='" + selectedPreferences[i] + "Checked']")).GetAttribute("checked") ?? string.Empty;
                if (!string.IsNullOrEmpty(checkedElement))
                {
                    driver.FindElement(By.XPath("//input[@name='" + selectedPreferences[i] + "Checked']")).Click();
                }
            }

            // click Save
            driver.FindElement(By.XPath("//input[@type='submit' and @value='Save']")).Click();

            // wait for manage your account page to load
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Manage Your Account']")));

            // check values are not the same as changed values
            if (genreList == selectedPreferences)
            {
                try
                {
                    var checkedElement = driver.FindElement(By.XPath("//div[@class='col-md-2'][./b[text()='Favorite Game Genres']]/following-sibling::div[contains(text(),'None')]")).Text;
                    if (checkedElement == "None")
                    {
                        Assert.Pass();
                    }
                }
                catch
                {
                    Assert.Fail();
                }
            }
            if (platformList == selectedPreferences)
            {
                try
                {
                    var checkedElement = driver.FindElement(By.XPath("//div[@class='col-md-2'][./b[text()='Favorite Platforms']]/following-sibling::div[contains(text(),'None')]")).Text;
                    if (checkedElement == "None")
                    {
                        Assert.Pass();
                    }
                }
                catch
                {
                    Assert.Fail();
                }
            }
        }
        
        #endregion

        #region member addresses tests
        [TestCase("4", "123", "Main Street", "Toronto", "Ontario", "M6H 1A1", "4", "123", "Main Street", "Toronto", "Ontario", "M6H 1A1", true)]
        [TestCase("4", "123", "Main Street", "Toronto", "Ontario", "M6H 1A1", "3", "124", "Other Street", "Ottawa", "Ontario", "K2P 1Y1", false)]
        public void I_MemberAddresses_SetValidAddresses_Successful(string mApartment, string mStreetNumber, string mStreetName, string mCity, string mProvince, string mPostalCode,
                                 string sApartment, string sStreetNumber, string sStreetName, string sCity, string sProvince, string sPostalCode, bool sameAsMailing)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            bool sameAsMailingChecked = true;
            string shippingAddressTestValue = sApartment + "-" + sStreetNumber + " " + sStreetName + ", " + sCity + ", " + sProvince + ", " + sPostalCode;
            string mailingAddressTestValue = mApartment + "-" + mStreetNumber + " " + mStreetName + ", " + mCity + ", " + mProvince + ", " + mPostalCode;

            string[,] mailingTestValues = new string[,]
            {
                {"MailingAddressApartment", mApartment},
                {"MailingAddressStreetNumber", mStreetNumber},
                {"MailingAddressStreetName", mStreetName},
                {"MailingAddressCity", mCity},
                {"MailingAddressProvince", mProvince},
                {"MailingAddressPostalCode", mPostalCode}
            };

            string[,] shippingTestValues = new string[,]
            {
                {"ShippingAddressApartment", sApartment},
                {"ShippingAddressStreetNumber", sStreetNumber},
                {"ShippingAddressStreetName", sStreetName},
                {"ShippingAddressCity", sCity},
                {"ShippingAddressProvince", sProvince},
                {"ShippingAddressPostalCode", sPostalCode}
            };

            // navigate to account details
            NavToManageAccountDetails("Admin", "Qwe!23");

            // click edit address details
            driver.FindElement(By.XPath("//a[@href='/Manage/ChangeAddressDetails']")).Click();

            // wait for Change address details page
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Change Address Details']")));

            // click clear all
            driver.FindElement(By.XPath("//button[@onclick='clearAll()']")).Click();

            // check to see if same as mailing checkbox is checked
            string sameAsMailingBox = driver.FindElement(By.XPath("//input[@name='ShippingAddressSame']")).GetAttribute("Checked");

            // if it is not checked set sameAsMailingBox to false
            if (sameAsMailingBox == null)
            {
                sameAsMailingChecked = false;
            }

            // if shipping address is not same as mailing address and checkbox is checked, uncheck checkbox
            if (!sameAsMailing)
            {
                if (sameAsMailingChecked)
                {
                    driver.FindElement(By.XPath("//input[@name='ShippingAddressSame']")).Click();
                }

                // fill out shipping address
                for (int i = 0; i < shippingTestValues.GetLength(0); i++)
                {
                    if (shippingTestValues[i, 0] == "ShippingAddressProvince")
                    {
                        driver.FindElement(By.XPath("//select[@name='" + shippingTestValues[i, 0] + "']")).Click();
                        driver.FindElement(By.XPath("//select[@name='" + shippingTestValues[i, 0] + "']//option[text()='" + shippingTestValues[i, 1] + "']")).Click();
                    }
                    else
                    {
                        driver.FindElement(By.XPath("//input[@name='" + shippingTestValues[i, 0] + "']")).SendKeys(shippingTestValues[i, 1]);
                    }
                }
            }

            // if shipping address is the same and checkbox unchecked, click checkbox
            if (sameAsMailing && !sameAsMailingChecked)
            {
                driver.FindElement(By.XPath("//input[@name='ShippingAddressSame']")).Click();
            }

            // fill out mailing address
            for (int i = 0; i < mailingTestValues.GetLength(0); i++)
            {
                if (mailingTestValues[i, 0] == "MailingAddressProvince")
                {
                    driver.FindElement(By.XPath("//select[@name='" + mailingTestValues[i, 0] + "']")).Click();
                    driver.FindElement(By.XPath("//select[@name='" + mailingTestValues[i, 0] + "']//option[text()='" + mailingTestValues[i, 1] + "']")).Click();
                }
                else
                {
                    driver.FindElement(By.XPath("//input[@name='" + mailingTestValues[i, 0] + "']")).SendKeys(mailingTestValues[i, 1]);
                }

            }

            // click Save
            driver.FindElement(By.XPath("//input[@type='submit' and @value='Save']")).Click();

            // wait for manage your account page
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Manage Your Account']")));

            // check mailing address value on Manage account page
            string mailingAddressValue = driver.FindElement(By.XPath("//td[./b[text()='Mailing Address']]/following-sibling::td")).Text;
            Assert.AreEqual(mailingAddressValue, mailingAddressTestValue);

            // check shipping address value on Manage account page
            string shippingAddressValue = driver.FindElement(By.XPath("//td[./b[text()='Shipping Address']]/following-sibling::td")).Text;
            Assert.AreEqual(shippingAddressValue, shippingAddressTestValue);
        }

        [TestCase]
        public void J_MemberAddresses_ClearAddresses_Successful()
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            Login("Admin", "Qwe!23");
            driver.Navigate().GoToUrl(baseURL + "/Manage/ChangeAddressDetails");

            // click clear all
            driver.FindElement(By.XPath("//button[@onclick='clearAll()']")).Click();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

            // check the Same as Mailing Address checkbox
            driver.FindElement(By.XPath("//input[@name='ShippingAddressSame']")).Click();

            // click Save
            driver.FindElement(By.XPath("//input[@type='submit' and @value='Save']")).Click();

            // wait for manage your account page
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Manage Your Account']")));

            // check if the adresses table was drawn
            int numberOfAddressTables = driver.FindElements(By.XPath("//table[contains(@class,'borderless')]")).Count;

            Assert.AreEqual(0, numberOfAddressTables);
        }

        [TestCase("4", "Toronto")]
        public void K_MemberAddresses_SetOnlyAppartmentAndCity_Fails(string _apartment, string _city)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            // navigate to account details
            Login("Admin", "Qwe!23");

            driver.Navigate().GoToUrl(baseURL + "/Manage/ChangeAddressDetails");

            // click clear all
            driver.FindElement(By.XPath("//button[@onclick='clearAll()']")).Click();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

            // check the Same as Mailing Address checkbox
            driver.FindElement(By.XPath("//input[@name='ShippingAddressSame']")).Click();

            // set mailing address appartment number
            driver.FindElement(By.XPath("//input[@name='MailingAddressApartment']")).Clear();
            driver.FindElement(By.XPath("//input[@name='MailingAddressApartment']")).SendKeys(_apartment);

            // click Save
            driver.FindElement(By.XPath("//input[@type='submit' and @value='Save']")).Click();

            // wait for 3 seconds
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

            string errorMessage = driver.FindElement(By.XPath("//div[contains(@class,'validation-summary-errors')]//li")).Text;
            StringAssert.Contains("Please provide a full mailing address or clear all mailing address fields", errorMessage);
        }
        #endregion
    }
}
