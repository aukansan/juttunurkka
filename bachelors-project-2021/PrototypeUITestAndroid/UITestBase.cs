/*Copyright 2025 Emmi Poutanen

This file is part of "Juttunurkka".

Juttunurkka is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, version 3 of the License.

Juttunurkka is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Juttunurkka.  If not, see <https://www.gnu.org/licenses/>.
*/

using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium;

namespace PrototypeUITestAndroid;

public class UITestBase
{
    /// <summary>
    ///     The Android driver instance.
    /// </summary>
    protected AndroidDriver _driver;

    [OneTimeSetUp]
    public void SetUp()
    {
        try
        {
            InitializeDriver();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing driver: {ex.Message}");
            throw;
        }
    }

    private void InitializeDriver()
    {
        // Default Appium server URI when running locally
        var serverUri = new Uri(Environment.GetEnvironmentVariable("APPIUM_HOST") ?? "http://127.0.0.1:4723/");
        var driverOptions = new AppiumOptions()
        {
            AutomationName = AutomationName.AndroidUIAutomator2,
            PlatformName = "Android",
            DeviceName = "Android Emulator",
        };

        driverOptions.AddAdditionalAppiumOption("appPackage", "com.companyname.prototype");
        driverOptions.AddAdditionalAppiumOption("appActivity", "crc64fc63109d11082323.MainActivity");
        driverOptions.AddAdditionalAppiumOption("noReset", true);

        _driver = new AndroidDriver(serverUri, driverOptions, TimeSpan.FromSeconds(180));
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        Console.WriteLine("Driver initialized successfully.");
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        if (_driver != null)
        {
            _driver.Dispose();
            Console.WriteLine("Driver disposed successfully.");
        }
        else
        {
            Console.WriteLine("Driver was not initialized.");
        }
    }

    /// <summary>
    ///     Reset the application to its main screen
    /// </summary>
    protected void ResetToMainScreen()
    {
        try
        {
            // Use back button to navigate to main screen
            //This method should be changed in the future, not very optimal
            for (int i = 0; i < 2; i++) // Press back up to 2 times to return to main screen
            {
                try
                {
                    _driver.Navigate().Back();
                    System.Threading.Thread.Sleep(1000); // Wait for navigation
                }
                catch
                {
                    break; // Break if back button doesn't work
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error resetting app: {ex.Message}");
            // As a last resort, reinitialize the driver
            _driver.Dispose();
            InitializeDriver();
        }
    }
    /// <summary>
    ///     Click continue button on UI
    /// </summary>
    protected void ClickContinueButton()
    {
        var continueButton = _driver.FindElement(By.XPath("//*[@text='Jatka']"));
        continueButton.Click();
    }

    /// <summary>
    ///     Navigate to teacher login screen
    /// </summary>
    protected void NavigateToTeacherLogin()
    {
        var teacherButton = _driver.FindElement(By.XPath("//*[@text='Opettaja']"));
        teacherButton.Click();
    }
}

[TestFixture, Order(1)]

public class LoginTests : UITestBase
{
    [Test, Order (1)]
    public void TestLoginToTeacherView_FaultyCredentials_ThrowsError()
    {
        // Navigate to teacher login
        NavigateToTeacherLogin();

        var loginButton = _driver.FindElement(By.XPath("//*[@text='Kirjaudu']"));
        Assert.IsNotNull(loginButton, "Login button should be displayed.");

        // Locate the username and password input fields and enter the wrong credentials
        var usernameField = _driver.FindElement(By.XPath("(//android.widget.EditText)[1]"));
        var passwordField = _driver.FindElement(By.XPath("(//android.widget.EditText)[2]"));
        usernameField.SendKeys("wrongUsername");
        passwordField.SendKeys("wrongPassword");

        loginButton.Click();

        // Assert: Check that login fails and error message is displayed
        var errorText = _driver.FindElement(By.XPath("//*[@text='VIRHE']"));
        Assert.IsNotNull(errorText, "Error message should be displayed after failed login.");

        // Reset to main screen for next test
        ResetToMainScreen();
    }

    [Test, Order(2)]
    public void TestLoginToTeacherView_CorrectCredentials_Logins()
    {
        // Navigate to teacher login
        NavigateToTeacherLogin();

        var loginButton = _driver.FindElement(By.XPath("//*[@text='Kirjaudu']"));
        Assert.IsNotNull(loginButton, "Login button should be displayed.");

        // Locate the username and password input fields and enter the credentials
        var usernameField = _driver.FindElement(By.XPath("(//android.widget.EditText)[1]"));
        var passwordField = _driver.FindElement(By.XPath("(//android.widget.EditText)[2]"));
        usernameField.SendKeys("opettaja");
        passwordField.SendKeys("opehuone");

        loginButton.Click();

        // Assert: Check that the teacher dashboard is displayed after successful login
        var createNewButton = _driver.FindElement(By.XPath("//*[@text='Luo uusi juttunurkka']"));
        Assert.IsNotNull(createNewButton, "Teacher Dashboard should be displayed after successful login.");

        ResetToMainScreen();

    }
}

[TestFixture, Order (2)]
public class JuttunurkkaTests : UITestBase
{
    [OneTimeSetUp]
    public new void SetUp()
    {
        // Call base setup to initialize driver
        base.SetUp();

        // Login as teacher before running Juttunurkka tests
        LoginAsTeacher();
    }

    private void LoginAsTeacher()
    {
        NavigateToTeacherLogin();

        var loginButton = _driver.FindElement(By.XPath("//*[@text='Kirjaudu']"));

        // Login with correct credentials
        var usernameField = _driver.FindElement(By.XPath("(//android.widget.EditText)[1]"));
        var passwordField = _driver.FindElement(By.XPath("(//android.widget.EditText)[2]"));
        usernameField.SendKeys("opettaja");
        passwordField.SendKeys("opehuone");

        loginButton.Click();

        // Verify login successful
        var createNewButton = _driver.FindElement(By.XPath("//*[@text='Luo uusi juttunurkka']"));
        Assert.IsNotNull(createNewButton, "Teacher Dashboard should be displayed after login.");
    }

    [Test]
    public void TestCreatingJuttunurkka()
    {
        var createNewButton = _driver.FindElement(By.XPath("//*[@text='Luo uusi juttunurkka']"));
        createNewButton.Click();

        var questionPicker = _driver.FindElement(By.XPath("//*[@text='Aseta kysymys']"));
        questionPicker.Click();

        var pickerItem = _driver.FindElement(By.XPath("//*[@text='Mit‰ kuuluu t‰n‰‰n?']"));
        pickerItem.Click();

        var selectedItem = _driver.FindElement(By.XPath("//*[@text='Mit‰ kuuluu t‰n‰‰n?']"));
        Assert.IsNotNull(selectedItem, "Selected item should be displayed.");

        ClickContinueButton();

        // Locate the CollectionView and the specific CheckBox elements
        var emojiCheckBox1 = _driver.FindElement(By.XPath("(//android.widget.CheckBox)[1]"));
        var emojiCheckBox2 = _driver.FindElement(By.XPath("(//android.widget.CheckBox)[2]"));

        emojiCheckBox1.Click();
        emojiCheckBox2.Click();

        ClickContinueButton();

        var activityPicker_first = _driver.FindElement(By.XPath("//*[@text='Jokainen kertoo mik‰ on kivaa']"));
        activityPicker_first.Click();

        var activityPicker_second = _driver.FindElement(By.XPath("//*[@text='Kehu vieress‰ istuvaa']"));
        activityPicker_second.Click();

        ClickContinueButton();

        var activity2Picker_first = _driver.FindElement(By.XPath("//*[@text='Kerrotaan ohjaajalle mik‰ h‰mm‰stytt‰‰']"));
        activity2Picker_first.Click();

        var activity2Picker_second = _driver.FindElement(By.XPath("//*[@text='Kerrotaan ryhm‰lle mik‰ h‰mm‰stytt‰‰']"));
        activity2Picker_second.Click();

        ClickContinueButton();

        var queryNameField = _driver.FindElement(By.XPath("(//android.widget.EditText)[1]"));
        queryNameField.SendKeys("Test1");

        var queryCodeField = _driver.FindElement(By.XPath("(//android.widget.EditText)[2]"));
        queryCodeField.SendKeys("123");

        ClickContinueButton();

        var readyButton = _driver.FindElement(By.XPath("//*[@text='Valmis']"));
        readyButton.Click();

        var openQueryButton = _driver.FindElement(By.XPath("//*[@text='Kyll‰']"));
        openQueryButton.Click();

        // Assert: Check that waiting participants is displayed
        var waitingParticipantsText = _driver.FindElement(By.XPath("//*[@text='Odotetaan osallistujia']"));
        Assert.IsNotNull(waitingParticipantsText, "Waiting screen should be displayed.");

    }
}