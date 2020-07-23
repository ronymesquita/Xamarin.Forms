using System.Collections;
using System.Configuration;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class RadioButtonTests 
	{
		[SetUp]
		public void SetUp() 
		{
			Device.SetFlags(new[] 
			{ 
				ExperimentalFlags.RadioButtonExperimental, 
				ExperimentalFlags.ShapesExperimental  
			});
		}

		[Test]
		public void RadioButtonAddedToGroupGetsGroupName() 
		{
			var layout = new StackLayout();
			var groupName = "foo";
			var radioButton = new RadioButton();

			layout.SetValue(RadioButtonGroup.GroupNameProperty, groupName);
			layout.Children.Add(radioButton);

			Assert.That(radioButton.GroupName, Is.EqualTo(groupName));
		}

		[Test]
		public void RadioButtonAddedToGroupKeepsGroupName()
		{
			var layout = new StackLayout();
			var groupName = "foo";
			var oldName = "bar";
			var radioButton = new RadioButton() { GroupName = oldName };

			layout.SetValue(RadioButtonGroup.GroupNameProperty, groupName);
			layout.Children.Add(radioButton);

			Assert.That(radioButton.GroupName, Is.EqualTo(oldName));
		}

		[Test]
		public void LayoutGroupNameAppliesToExistingRadioButtons()
		{
			var layout = new StackLayout();
			var groupName = "foo";
			var radioButton = new RadioButton();

			layout.Children.Add(radioButton);
			layout.SetValue(RadioButtonGroup.GroupNameProperty, groupName);
			
			Assert.That(radioButton.GroupName, Is.EqualTo(groupName));
		}

		[Test]
		public void UpdatedGroupNameAppliesToRadioButtonsWithOldGroupName()
		{
			var layout = new StackLayout();
			var groupName = "foo";
			var updatedGroupName = "bar";
			var otherGroupName = "other";
			var radioButton1 = new RadioButton();
			var radioButton2 = new RadioButton() { GroupName = otherGroupName };

			layout.Children.Add(radioButton1);
			layout.Children.Add(radioButton2);
			layout.SetValue(RadioButtonGroup.GroupNameProperty, groupName);

			layout.SetValue(RadioButtonGroup.GroupNameProperty, updatedGroupName);

			Assert.That(radioButton1.GroupName, Is.EqualTo(updatedGroupName));
			Assert.That(radioButton2.GroupName, Is.EqualTo("other"));
		}
	}
}
