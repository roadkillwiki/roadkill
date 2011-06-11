using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using WatiN.Core;

namespace Roadkill.Tests
{
	/// <summary>
	/// A base class for all Watin unit tests. Before calling any methods, ensure you have a session
	/// by calling (inside a using clause) NewSession().
	/// </summary>
	public class WatinTestBase
	{
		protected static Random _random = new Random();
		protected TestSession _testSession;

		/// <summary>
		/// The instance of IE that the <see cref="TestSession"/> is using.
		/// </summary>
		protected IE IE
		{
			get
			{
				EnsureSessionExists();
				return _testSession.IEInstance;
			}
		}

		/// <summary>
		/// Creates a new <see cref="TestSession"/> which encapsulates a single test unit.
		/// </summary>
		/// <returns></returns>
		protected TestSession NewSession()
		{
			IE ie = new IE();
			_testSession = new TestSession(ie);
			return _testSession;
		}

		/// <summary>
		/// Creates a full site URL from the file path
		/// </summary>
		/// <param name="path"></param>
		protected string FormatUrl(string path)
		{
			return string.Format("{0}{1}", Settings.BaseUrl, path);
		}

		/// <summary>
		/// Navigates to the url provided.
		/// </summary>
		protected void GoToUrl(string path)
		{
			EnsureSessionExists();

			string url = string.Format("{0}{1}", Settings.BaseUrl, path);
			_testSession.IEInstance.GoTo(url);
		}

		/// <summary>
		/// Logs in
		/// </summary>
		/// <param name="ie"></param>
		protected void Login()
		{
			EnsureSessionExists();

			GoToUrl("/user/login/");
			SetTextfieldValue("username", Settings.AdminUserEmail, "Unable to find the email textbox when logging in");
			SetTextfieldValue("password", Settings.AdminUserPassword, "Unable to find the password textbox when logging in");

			ClickButton("userbutton","Unable to click the login button");
		}

		/// <summary>
		/// Finds a textfield that ends with the provided id and sets its value. If the textfield doesn't exist an 
		/// Assert.Fail is performed with a fail message based on the id. This also calls keyup on the textfield.
		/// </summary>
		protected void SetTextfieldValue(string idEndsWith, string value)
		{
			SetTextfieldValue(idEndsWith, value, string.Format("Unable to find the {0} textfield", idEndsWith));
		}

		/// <summary>
		/// Asserts that the textfield that ends with the provided id has the given value.
		/// </summary>
		protected void AssertTextFieldEquals(string idEndsWith, string value)
		{
			EnsureSessionExists();

			TextField textField = _testSession.IEInstance.TextField(new Regex(".*" + idEndsWith));
			if (textField == null)
				Assert.Fail(string.Format("Unable to find {0} textfield", idEndsWith));

			Assert.AreEqual(value, textField.Value);
		}

		/// <summary>
		/// Finds a textfield that ends with the provided id and sets its value. If the textfield doesn't exist an Assert.Fail 
		/// is performed with the fail message. This also calls keyup on the textfield.
		/// </summary>
		protected void SetTextfieldValue(string idEndsWith, string value, string failMessage)
		{
			EnsureSessionExists();

			TextField textField = _testSession.IEInstance.TextField(new Regex(".*" + idEndsWith));
			if (textField == null)
				Assert.Fail(failMessage);

			textField.Value = value;
			textField.KeyUp(); // for the javascript validation for the save button
		}

		protected void PageShouldContainText(params string[] args)
		{
			EnsureSessionExists();

			for (int i = 0; i < args.Length; i++)
			{
				Assert.IsTrue(_testSession.IEInstance.ContainsText(args[i]),
					string.Format("The page does not contain the text '{0}'", args[i]));
			}
		}

		/// <summary>
		/// Finds a button that ends with the provided id and clicks it. If the button doesn't exist an Assert.Fail is 
		/// performed with the fail message.
		/// </summary>
		protected void ClickButton(string idEndsWith, string failMessage)
		{
			EnsureSessionExists();

			Button button = _testSession.IEInstance.Button(new Regex(".*" + idEndsWith));
			if (button == null)
				Assert.Fail(failMessage);

			button.Click();
		}

		/// <summary>
		/// Finds a checkbox that ends with the provided id and clicks it. If the checkbox doesn't exist an Assert.Fail is 
		/// performed with the fail message.
		/// </summary>
		protected void ChangeCheckboxValue(string idEndsWith, bool isChecked, string failMessage)
		{
			EnsureSessionExists();

			CheckBox checkBox = _testSession.IEInstance.CheckBox(new Regex(".*" + idEndsWith));
			if (checkBox == null)
				Assert.Fail(failMessage);

			checkBox.Checked = isChecked;
		}

		/// <summary>
		/// Finds a link that ends with the provided id and clicks it. If the link doesn't exist an Assert.Fail is 
		/// performed with the fail message.
		/// </summary>
		protected void ClickLinkById(string idStartsWith, string failMessage)
		{
			EnsureSessionExists();

			Link link = _testSession.IEInstance.Link(new Regex(".*userLink"));
			if (link == null)
				Assert.Fail(failMessage);

			link.Click();
		}

		/// <summary>
		/// Finds a link that has the text title and clicks it. If the link doesn't exist an Assert.Fail is 
		/// performed with the fail message.
		/// </summary>
		protected void ClickLink(string linkTitle, string failMessage)
		{
			EnsureSessionExists();

			Link link = _testSession.IEInstance.Link(l => !string.IsNullOrEmpty(l.Text) && l.Text.ToLower() == linkTitle.ToLower());
			if (link == null)
				Assert.Fail(failMessage);

			link.Click();
		}

		/// <summary>
		/// Selects an item from the selectlist that ends with the id provided. If the item isn't found, or the selectlist
		/// does not exist or has no items, an Assert.Fail is raised with the failmessage.
		/// </summary>
		protected void ChooseSelectListItem(string idEndsWith, string textContains, string failMessage)
		{
			SelectList selectList = IE.SelectList(new Regex(".*" + idEndsWith));

			if (selectList == null)
				Assert.Fail(string.Format("The select list '...{0}' could not be found", idEndsWith));

			if (selectList.Options.ToList().Count == 0)
				Assert.Fail(string.Format("The select list '...{0}' has no items", idEndsWith));

			try
			{
				selectList.Select(new Regex(".*" + textContains + ".*"));
			}
			catch (Exception)
			{
				Assert.Fail(string.Format("The select list '...{0}' does not contain an item with the text '{1}'", idEndsWith, textContains));
			}
		}

		private void EnsureSessionExists()
		{
			if (_testSession == null)
				throw new InvalidOperationException("No TestSession has been created. Make sure you use NewSession() first");
		}
	}
}
