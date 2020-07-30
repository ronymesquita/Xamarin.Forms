using System;

namespace Xamarin.Forms
{
	internal class RadioButtonGroupController
	{
		readonly Layout<View> _layout;

		string _groupName;
		private object _selection;

		public string GroupName { get => _groupName; set => SetGroupName(value); }
		public object SelectedValue { get => _selection; set => SetSelection(value); }

		public RadioButtonGroupController(Layout<View> layout)
		{
			if (layout is null)
			{
				throw new ArgumentNullException(nameof(layout));
			}

			_layout = layout;
			layout.ChildAdded += ChildAdded;

			if (!string.IsNullOrEmpty(_groupName))
			{
				UpdateGroupNames(layout, _groupName);
			}

			MessagingCenter.Subscribe<RadioButton, RadioButtonGroupSelectionChanged>(this, 
				RadioButtonGroup.RadioButtonGroupSelectionChanged, HandleRadioButtonGroupSelectionChanged);
			MessagingCenter.Subscribe<RadioButton, RadioButtonGroupNameChanged>(this, RadioButtonGroup.RadioButtonGroupNameChanged,
				HandleRadioButtonGroupNameChanged);
			MessagingCenter.Subscribe<RadioButton, RadioButtonValueChanged>(this, RadioButtonGroup.RadioButtonValueChanged,
				HandleRadioButtonValueChanged);
		}

		void HandleRadioButtonGroupSelectionChanged(RadioButton selected, RadioButtonGroupSelectionChanged args)
		{
			if (selected.GroupName != _groupName)
			{
				return;
			}

			var controllerScope = RadioButtonGroup.GetVisualRoot(_layout);
			if (args.Scope != controllerScope)
			{
				return;
			}

			_layout.SetValue(RadioButtonGroup.SelectedValueProperty, selected.Value);
		}

		void HandleRadioButtonGroupNameChanged(RadioButton radioButton, RadioButtonGroupNameChanged args) 
		{
			if (args.OldName != _groupName)
			{
				return;
			}

			var controllerScope = RadioButtonGroup.GetVisualRoot(_layout);
			if (args.Scope != controllerScope)
			{
				return;
			}

			_layout.ClearValue(RadioButtonGroup.SelectedValueProperty);
		}

		void HandleRadioButtonValueChanged(RadioButton radioButton, RadioButtonValueChanged args)
		{
			if (radioButton.GroupName != _groupName)
			{
				return;
			}

			var controllerScope = RadioButtonGroup.GetVisualRoot(_layout);
			if (args.Scope != controllerScope)
			{
				return;
			}

			_layout.SetValue(RadioButtonGroup.SelectedValueProperty, radioButton.Value);
		}

		void ChildAdded(object sender, ElementEventArgs e)
		{
			if (string.IsNullOrEmpty(_groupName))
			{
				return;
			}

			if(!(e.Element is RadioButton radioButton))
			{
				return;
			}

			UpdateGroupName(radioButton, _groupName);

			if (radioButton.IsChecked)
			{
				_layout.SetValue(RadioButtonGroup.SelectedValueProperty, radioButton.Value);
			}
		}

		void UpdateGroupName(Element element, string name, string oldName = null)
		{
			if (!(element is RadioButton radioButton))
			{
				return;
			}

			var currentName = radioButton.GroupName;

			if (string.IsNullOrEmpty(currentName) || currentName == oldName)
			{
				radioButton.GroupName = name;
			}
		}

		void UpdateGroupNames(Layout<View> layout, string name, string oldName = null) 
		{
			foreach (var descendant in layout.Descendants())
			{
				UpdateGroupName(descendant, name, oldName);
			}
		}

		void SetSelection(object radioButtonValue)
		{
			_selection = radioButtonValue;

			if (radioButtonValue != null)
			{
				var radioButton = RadioButtonGroup.GetRadioButtonWithValue(radioButtonValue, _groupName);
				if (radioButton != null)
				{
					radioButton.IsChecked = true;
				}
			}
		}

		void SetGroupName(string groupName)
		{
			var oldName = _groupName;
			_groupName = groupName;
			UpdateGroupNames(_layout, _groupName, oldName);
		}
	}
}