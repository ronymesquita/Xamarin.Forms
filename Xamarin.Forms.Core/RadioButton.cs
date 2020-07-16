using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xamarin.Forms.Platform;
using Xamarin.Forms.Shapes;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_RadioButtonRenderer))]
	public class RadioButton : TemplatedView, IElementConfiguration<RadioButton>
	{
		#region ControlTemplate Stuff

		TapGestureRecognizer _tapGestureRecognizer;
		Shape _normalEllipse;
		Shape _checkMark;

		void Initialize()
		{
			_tapGestureRecognizer = new TapGestureRecognizer();

			UpdateIsEnabled();
		}

		protected override void OnParentSet()
		{
			base.OnParentSet();

			if (ControlTemplate == null)
				ControlTemplate = Application.Current.Resources["RadioButtonTemplate"] as ControlTemplate;
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_normalEllipse = GetTemplateChild("NormalEllipse") as Shape;
			_checkMark = GetTemplateChild("CheckMark") as Shape;
		}

		void UpdateIsEnabled()
		{
			if (IsEnabled)
			{
				_tapGestureRecognizer.Tapped += OnRadioButtonChecked;
				GestureRecognizers.Add(_tapGestureRecognizer);
			}
			else
			{
				_tapGestureRecognizer.Tapped -= OnRadioButtonChecked;
				GestureRecognizers.Remove(_tapGestureRecognizer);
			}
		}

		void OnRadioButtonChecked(object sender, EventArgs e)
		{
			IsChecked = !IsChecked;

			if (IsChecked)
			{
				if (_normalEllipse != null)
					_normalEllipse.Stroke = (Color)Application.Current.Resources["RadioButtonCheckMarkThemeColor"];

				if (_checkMark != null)
					_checkMark.Opacity = 1;
			}
			else
			{
				if (_normalEllipse != null)
					_normalEllipse.Stroke = (Color)Application.Current.Resources["RadioButtonThemeColor"];

				if (_checkMark != null)
					_checkMark.Opacity = 0;
			}
		}
		#endregion


		public static readonly BindableProperty ContentProperty =
		  BindableProperty.Create(nameof(Content), typeof(object), typeof(RadioButton), null);

		public object Content
		{
			get => GetValue(ContentProperty);
			set => SetValue(ContentProperty, value);
		}



		readonly Lazy<PlatformConfigurationRegistry<RadioButton>> _platformConfigurationRegistry;

		public const string IsCheckedVisualState = "IsChecked";

		public static readonly BindableProperty IsCheckedProperty = BindableProperty.Create(
			nameof(IsChecked), typeof(bool), typeof(RadioButton), false, propertyChanged: (b, o, n) => ((RadioButton)b).OnIsCheckedPropertyChanged((bool)n), defaultBindingMode: BindingMode.TwoWay);

		public static readonly BindableProperty GroupNameProperty = BindableProperty.Create(
			nameof(GroupName), typeof(string), typeof(RadioButton), null, propertyChanged: (b, o, n) => ((RadioButton)b).OnGroupNamePropertyChanged((string)o, (string)n));

		// TODO Needs implementations beyond Android
		//public static readonly BindableProperty ButtonSourceProperty = BindableProperty.Create(
		//	nameof(ButtonSource), typeof(ImageSource), typeof(RadioButton), null);

		public event EventHandler<CheckedChangedEventArgs> CheckedChanged;

		public bool IsChecked
		{
			get { return (bool)GetValue(IsCheckedProperty); }
			set { SetValue(IsCheckedProperty, value); }
		}

		public string GroupName
		{
			get { return (string)GetValue(GroupNameProperty); }
			set { SetValue(GroupNameProperty, value); }
		}

		// TODO Needs implementations beyond Android
		//public ImageSource ButtonSource
		//{
		//	get { return (ImageSource)GetValue(ButtonSourceProperty); }
		//	set { SetValue(ButtonSourceProperty, value); }
		//}

		public RadioButton()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<RadioButton>>(() => new PlatformConfigurationRegistry<RadioButton>(this));
			Initialize();
		}

		static bool isExperimentalFlagSet = false;
		internal static void VerifyExperimental([CallerMemberName] string memberName = "", string constructorHint = null)
		{
			if (isExperimentalFlagSet)
				return;

			ExperimentalFlags.VerifyFlagEnabled(nameof(RadioButton), ExperimentalFlags.RadioButtonExperimental, constructorHint, memberName);

			isExperimentalFlagSet = true;
		}

		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			VerifyExperimental(nameof(RadioButton));
			return base.OnMeasure(widthConstraint, heightConstraint);
		}

		public IPlatformElementConfiguration<T, RadioButton> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		protected internal override void ChangeVisualState()
		{
			if (IsEnabled && IsChecked)
				VisualStateManager.GoToState(this, IsCheckedVisualState);
			else
				base.ChangeVisualState();
		}

		#region Group Stuff

		void OnIsCheckedPropertyChanged(bool isChecked)
		{
			if (isChecked)
				UpdateRadioButtonGroup();

			CheckedChanged?.Invoke(this, new CheckedChangedEventArgs(isChecked));
			ChangeVisualState();
		}

		static Dictionary<string, List<WeakReference<RadioButton>>> _groupNameToElements;

		void OnGroupNamePropertyChanged(string oldGroupName, string newGroupName)
		{
			// Unregister the old group name if set
			if (!string.IsNullOrEmpty(oldGroupName))
				Unregister(this, oldGroupName);

			// Register the new group name is set
			if (!string.IsNullOrEmpty(newGroupName))
				Register(this, newGroupName);
		}

		void UpdateRadioButtonGroup()
		{
			string groupName = GroupName;
			if (!string.IsNullOrEmpty(groupName))
			{
				Element rootScope = GetVisualRoot(this);

				if (_groupNameToElements == null)
					_groupNameToElements = new Dictionary<string, List<WeakReference<RadioButton>>>(1);

				// Get all elements bound to this key and remove this element
				List<WeakReference<RadioButton>> elements = _groupNameToElements[groupName];
				for (int i = 0; i < elements.Count;)
				{
					WeakReference<RadioButton> weakRef = elements[i];
					if (weakRef.TryGetTarget(out RadioButton rb))
					{
						// Uncheck all checked RadioButtons different from the current one
						if (rb != this && (rb.IsChecked == true) && rootScope == GetVisualRoot(rb))
							rb.SetValueFromRenderer(IsCheckedProperty, false);

						i++;
					}
					else
					{
						// Remove dead instances
						elements.RemoveAt(i);
					}
				}
			}
			else // Logical parent should be the group
			{
				Element parent = Parent;
				if (parent != null)
				{
					// Traverse logical children
					IEnumerable children = parent.LogicalChildren;
					IEnumerator itor = children.GetEnumerator();
					while (itor.MoveNext())
					{
						var rb = itor.Current as RadioButton;
						if (rb != null && rb != this && string.IsNullOrEmpty(rb.GroupName) && (rb.IsChecked == true))
							rb.SetValueFromRenderer(IsCheckedProperty, false);
					}
				}
			}
		}

		static void Register(RadioButton radioButton, string groupName)
		{
			if (_groupNameToElements == null)
				_groupNameToElements = new Dictionary<string, List<WeakReference<RadioButton>>>(1);

			if (_groupNameToElements.TryGetValue(groupName, out List<WeakReference<RadioButton>> elements))
			{
				// There were some elements there, remove dead ones
				PurgeDead(elements, null);
			}
			else
			{
				elements = new List<WeakReference<RadioButton>>(1);
				_groupNameToElements[groupName] = elements;
			}

			elements.Add(new WeakReference<RadioButton>(radioButton));
		}

		static void Unregister(RadioButton radioButton, string groupName)
		{
			if (_groupNameToElements == null)
				return;

			// Get all elements bound to this key and remove this element
			if (_groupNameToElements.TryGetValue(groupName, out List<WeakReference<RadioButton>> elements))
			{
				PurgeDead(elements, radioButton);

				if (elements.Count == 0)
					_groupNameToElements.Remove(groupName);
			}
		}

		static void PurgeDead(List<WeakReference<RadioButton>> elements, object elementToRemove)
		{
			for (int i = 0; i < elements.Count;)
			{
				if (elements[i].TryGetTarget(out RadioButton rb) && rb == elementToRemove)
					elements.RemoveAt(i);
				else
					i++;
			}
		}

		static Element GetVisualRoot(Element element)
		{
			Element parent = element.Parent;
			while (parent != null && !(parent is Page))
				parent = parent.Parent;
			return parent;
		}
		#endregion
	}

	internal class RadioButtonGroupController
	{
		readonly WeakReference<Layout<View>> _layoutWeakReference;

		public RadioButtonGroupController(Layout<View> layout)
		{
			_layoutWeakReference = new WeakReference<Layout<View>>(layout);
		}

		string _groupName;
		RadioButton _selection;

		public string GroupName { get => _groupName; set => SetGroupName(value); }
		public RadioButton Selection { get => _selection; set => SetSelection(value); }

		// TODO this might not be necessary, simple setter might be fine
		void SetSelection(RadioButton radioButton)
		{
			_selection = radioButton;
		}

		void SetGroupName(string groupName)
		{
			_groupName = groupName;
		}
	}

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
			BindableProperty.Create("GroupName", typeof(string), typeof(Layout<View>), null, propertyChanged: (b, o, n) => { GetRadioButtonGroupController(b).GroupName = (string)n; });

		public static string GetGroupName(BindableObject b)
		{
			return (string)b.GetValue(GroupNameProperty);
		}

		public static readonly BindableProperty SelectionProperty =
			BindableProperty.Create("Selection", typeof(RadioButton), typeof(Layout<View>), null, propertyChanged: (b, o, n) => { GetRadioButtonGroupController(b).Selection = (RadioButton)n; });

		public static RadioButton GetSelection(BindableObject b)
		{
			return (RadioButton)b.GetValue(SelectionProperty);
		}

		static void OnControllerChanged(BindableObject b, RadioButtonGroupController oldC, RadioButtonGroupController newC)
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