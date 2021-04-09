using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace CVGS.Tests
{
    [TestFixture]
    public class HomeControllerTests
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

        [TestCase("Call of Duty: Warzone", "4ba993d2-6cc7-499f-87bd-f90190284830", TestName = "Select_CallOfDuty_CallOfDutyGameDetails")]
        [TestCase("Apex Legends", "7ca2647a-290c-4524-9760-16d24984e433", TestName = "Select_ApexLegends_ApexLegendsGameDetails")]
        [TestCase("Left 4 Dead 3", "809d59bd-0025-4349-abfb-f87fefb15b72", TestName = "Select_Left4Dead3_Left4Dead3GameDetails")]
        [TestCase("Civilization VI", "991804c1-ee86-405f-bbd5-2e36d43d2562", TestName = "Select_CivilizationVI_CivilizationVIGameDetails")]
        public void SelectGame_ValidGameName_NavigatesToGameDetailPage(string _title, string _id)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            driver.Navigate().GoToUrl(baseURL);
            var tile = driver.FindElement(By.XPath("//a[@href='/Home/ViewGame/" + _id + "']"));

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", tile);

            wait.Until(d => d.Title.Contains(_title));
            string title = driver.FindElement(By.Id("gameTitle")).Text;
            Assert.AreEqual(_title, title);
        }
    }
}
