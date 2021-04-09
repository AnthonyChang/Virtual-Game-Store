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
    public class EventControllerTests
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


        [TestCase("TestEvent", "TestDescription", "50", "TestVenue", "2021-06-01T08:30", "2021-06-01T10:30", false, true, TestName = "AddEvent_EventAdded")]
        [TestCase("E3 - The Electronic Entertainment Expo", "E3, also known as the Electronic Entertainment Expo, " +
            "is a trade event for the video game industry. The Entertainment Software " +
            "Association (ESA) organizes and presents E3, which many developers, publishers, " +
            "hardware and accessory manufacturers use to introduce and advertise upcoming " +
            "games and game-related merchandise to retailers and to members of the press. E3 " +
            "includes an exhibition floor for developers, publishers, and manufacturers to " +
            "showcase titles and products for sale in the upcoming year. Before and during the " +
            "event, publishers and hardware manufacturers usually hold press conferences to " +
            "announce new games and products.", "500", "Los Angeles Convention Center",
            "2021-10-16T08:30", "2021-10-17T10:30", false, true, TestName = "AddEvent_LargeStrings_EventAdded")]
        [TestCase("TestEvent", "The Game Developers Conference is an annual conference for video " +
            "game developers. The event has learning, inspiration, and networking.", "20000",
            "San Jose Convention Center", "2022-02-27  12:30", "2022-02-28  13:30", true, false, TestName = "AddEvent_ImproperDateSyntax_EventNotAdded")]
        public void AddEvent(string eventTitle, string eventDescription, string eventAttendees, string eventLocation, string startTime, string endTime, bool improperDate, bool deleteEvent)
        {
            string[,] testValues = new string[,]
            {
                {"//input[@name='Title']", eventTitle},
                {"//textarea[@name='Description']", eventDescription},
                {"//input[@name='MaxAttendeeNumber']", eventAttendees},
                {"//input[@name='Location']", eventLocation}
            };

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // login 
            Login("Admin", "Qwe!23");

            // click Administration
            driver.FindElement(By.XPath("//a[contains(text(),'Administration')]")).Click();

            // click Manage Events
            driver.FindElement(By.XPath("//a[@href='/Admin/Event']")).Click();

            // wait for Events list to appear 
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Event List']")));

            // delete test event if it exists
            if (driver.FindElements(By.XPath("//td[text()='" + testValues[0, 1] + "']")).Count != 0)
            {
                DeleteEvent_fromManageEvents(testValues[0, 1]);
            }

            // click Add New Event
            driver.FindElement(By.XPath("//a[text()='Add New Event']")).Click();

            // wait for Add New Event page to appear 
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Add New Event']")));

            // input values for new event
            for (int i = 0; i < testValues.GetLength(0); i++)
            {
                driver.FindElement(By.XPath(testValues[i, 0])).Clear();
                driver.FindElement(By.XPath(testValues[i, 0])).SendKeys(testValues[i, 1]);
            }

            // input values to StartTime
            driver.FindElement(By.XPath("//input[@name='StartTime']")).SendKeys(startTime);

            // input values to EndTime
            driver.FindElement(By.XPath("//input[@name='EndTime']")).SendKeys(endTime);

            // click Save
            driver.FindElement(By.XPath("//input[@type='submit' and @value='Save']")).Click();

            if (improperDate)
            {
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//span[@id='StartTime-error']")));
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//span[@id='EndTime-error']")));
                string startTimeErrorText = driver.FindElement(By.XPath("//span[@id='StartTime-error']")).Text;
                string endTimeErrorText = driver.FindElement(By.XPath("//span[@id='EndTime-error']")).Text;
                Assert.AreEqual("The field Start Time must be a date.", startTimeErrorText);
                Assert.AreEqual("The field End Time must be a date.", endTimeErrorText);
            }
            else
            {
                //driver.Navigate().GoToUrl(baseURL + "/Admin/Event");

                // wait for Events list to appear
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Event List']")));

                // check to see if game successfully added
                string gameAdded = driver.FindElement(By.XPath("//div[@class='tempdata']")).Text;
                Assert.AreEqual(string.Format("Record for '{0}' successfully added", testValues[0, 1]), gameAdded);

                // check manage events table for Title, ReleaseYear and Price Values
                string eventTitleText = driver.FindElement(By.XPath("//tr[./td[text()='" + testValues[0, 1] +
                                                                   "']]/td[contains(text(),'" +
                                                                   testValues[0, 1] + "')]")).Text;
                string eventAttendeeNumberText = driver.FindElement(By.XPath("//tr[./td[text()='" + testValues[0, 1] +
                                                                   "']]/td[contains(text(),'" +
                                                                   testValues[2, 1] + "')]")).Text;

                Assert.AreEqual(testValues[0, 1], eventTitleText);
                Assert.AreEqual(testValues[2, 1], eventAttendeeNumberText);
            }

            if (deleteEvent)
            {
                DeleteEvent_fromManageEvents(testValues[0, 1]);
            }
        }


        public void DeleteEvent_fromManageEvents(string eventTitle)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // click delete
            driver.FindElement(By.XPath("//tr[./td[text()='" + eventTitle + "']]/td/a[contains(@href, 'Delete')]")).Click();

            // wait for delete game page
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Delete Event']")));

            // click delete
            driver.FindElement(By.XPath("//input[@type='submit' and @value='Delete']")).Click();

            // wait for manage games list page
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='Event List']")));
        }


        [TestCase(TestName = "TestEventDetails_EventDetailsCorrect")]
        public void Event_Details()
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // create an event
            AddEvent("TestEvent", "TestDescription", "50", "TestVenue", "2021-06-01T08:30", "2021-06-01T10:30", false, false);

            // click events
            driver.FindElement(By.XPath("//a[@href='/Events']")).Click();

            // check for test event
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//tr/td[contains(text(), 'TestEvent')]")));

            // click view details
            driver.FindElement(By.XPath("(//a[@title='View Event Details'])[1]")).Click();

            // check for event header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='TestEvent']")));

            // click back to events
            driver.FindElement(By.XPath("//a[text()='Back to Events']")).Click();

            // check for test event
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//tr/td[contains(text(), 'TestEvent')]")));

            // check for event header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//td[contains(text(),'TestEvent')]")));

            // check for event start
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//td[contains(text(),'2021-06-01 8:30:00 AM')]")));

            // check for event end
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//td[contains(text(),'2021-06-01 10:30:00 AM')]")));

            // check for event location
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//td[contains(text(),'Online')]")));

            //if all information present pass
            Assert.Pass();
        }



        [TestCase(TestName = "RegisterEvent_Registered")]
        public void Register_event()
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // create an event
            AddEvent("TestEvent", "TestDescription", "50", "TestVenue", "2021-06-01T08:30", "2021-06-01T10:30", false, false);

            // click events
            driver.FindElement(By.XPath("//a[@href='/Events']")).Click();

            // check for test event
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//tr/td[contains(text(), 'TestEvent')]")));

            // click view details
            driver.FindElement(By.XPath("(//a[@title='View Event Details'])[1]")).Click();

            // check for event header
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[text()='TestEvent']")));

            // click register
            driver.FindElement(By.XPath("//a[text()='Register for Event']")).Click();

            // check for You are registered for this event.
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//p[text()='You are registered for this event.']")));

            // click events
            driver.FindElement(By.XPath("//a[@href='/Events']")).Click();

            // check for test event
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//tr/td[contains(text(), 'TestEvent')]")));

            // check for test event registered checkmark
            try
            {
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//span[@class='glyphicon glyphicon-ok']")));
            }
            catch
            {
                Assert.Fail();
            }
            Assert.Pass();
        }

        [TestCase("TestEvent", true, TestName = "Unregister_EventUnregistered")]
        public void Unregister_event (string testName, bool deleteEvent)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // login 
            Login("Admin", "Qwe!23");

            // click events
            driver.FindElement(By.XPath("//a[@href='/Events']")).Click();

            // check for test event
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//tr/td[contains(text(), 'TestEvent')]")));

            // check for test event registered checkmark
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//span[@class='glyphicon glyphicon-ok']")));

            // click view details
            driver.FindElement(By.XPath("(//a[@title='View Event Details'])[1]")).Click();

            // click register
            driver.FindElement(By.XPath("//a[text()='Unsubscribe from Event']")).Click();

            // click events
            driver.FindElement(By.XPath("//a[@href='/Events']")).Click();

            // check for test event registered checkmark does not exist
            try
            {
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//span[@class='glyphicon glyphicon-ok']")));
                Assert.Fail();
            }
            catch
            {
                Assert.Pass();
                if (deleteEvent)
                {
                    DeleteEvent_fromManageEvents(testName);
                }
            }
        }
    }
}
