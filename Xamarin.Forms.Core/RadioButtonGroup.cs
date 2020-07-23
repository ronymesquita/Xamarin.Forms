namespace Xamarin.Forms
{
	public static class RadioButtonGroup
	{
		static readonly BindableProperty RadioButtonGroupControllerProperty =
			BindableProperty.CreateAttached("RadioButtonGroupController", typeof(RadioButtonGroupController), typeof(Layout<View>), default(RadioButtonGroupController),
			defaultValueCreator: (b) => new RadioButtonGroupController((Layout<View>)b),
			propertyChanged: (b, o, n) => OnControllerChanged(b, (RadioButtonGroupController)o, (RadioButtonGroupController)n));

		static RadioButtonGroupController GetRadioButtonGroupController(BindableObject b)
		{
			return (RadioButtonGroupController)b.GetValue(RadioButtonGroupControllerProperty);
		}

		public static readonly BindableProperty GroupNameProperty =
			BindableProperty.Create("GroupName", typeof(string), typeof(Layout<View>), null, 
			propertyChanged: (b, o, n) => { GetRadioButtonGroupController(b).GroupName = (string)n; });

		public static string GetGroupName(BindableObject b)
		{
			return (string)b.GetValue(GroupNameProperty);
		}

		public static readonly BindableProperty SelectionProperty =
			BindableProperty.Create("Selection", typeof(RadioButton), typeof(Layout<View>), null, 
			propertyChanged: (b, o, n) => { GetRadioButtonGroupController(b).Selection = (RadioButton)n; });

		public static RadioButton GetSelection(BindableObject b)
		{
			return (RadioButton)b.GetValue(SelectionProperty);
		}

		static void OnControllerChanged(BindableObject b, RadioButtonGroupController oldC, 
			RadioButtonGroupController newC)
		{
			if (newC == null)
			{
				return;
			}

			newC.GroupName = GetGroupName(b);
			newC.Selection = GetSelection(b);
		}
	}
}