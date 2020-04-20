using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace FA2.Behaviors
{
    public sealed class WatermarkComboBoxBehavior
    {
        private readonly ComboBox _mComboBox;
        private TextBlockAdorner _mTextBlockAdorner;

        private WatermarkComboBoxBehavior(ComboBox comboBox)
        {
            if (comboBox == null)
                throw new ArgumentNullException("comboBox");

            _mComboBox = comboBox;
        }

        #region Behavior Internals

        private static WatermarkComboBoxBehavior GetWatermarkComboBoxBehavior(DependencyObject obj)
        {
            return (WatermarkComboBoxBehavior)obj.GetValue(WatermarkComboBoxBehaviorProperty);
        }

        private static void SetWatermarkComboBoxBehavior(DependencyObject obj, WatermarkComboBoxBehavior value)
        {
            obj.SetValue(WatermarkComboBoxBehaviorProperty, value);
        }

        private static readonly DependencyProperty WatermarkComboBoxBehaviorProperty =
            DependencyProperty.RegisterAttached("WatermarkComboBoxBehavior",
                typeof(WatermarkComboBoxBehavior), typeof(WatermarkComboBoxBehavior), new UIPropertyMetadata(null));

        public static bool GetEnableWatermark(ComboBox obj)
        {
            return (bool)obj.GetValue(EnableWatermarkProperty);
        }

        public static void SetEnableWatermark(ComboBox obj, bool value)
        {
            obj.SetValue(EnableWatermarkProperty, value);
        }

        public static readonly DependencyProperty EnableWatermarkProperty =
            DependencyProperty.RegisterAttached("EnableWatermark", typeof(bool),
                typeof(WatermarkComboBoxBehavior), new UIPropertyMetadata(false, OnEnableWatermarkChanged));

        private static void OnEnableWatermarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                var enabled = (bool)e.OldValue;

                if (enabled)
                {
                    var comboBox = (ComboBox)d;
                    var behavior = GetWatermarkComboBoxBehavior(comboBox);
                    behavior.Detach();

                    SetWatermarkComboBoxBehavior(comboBox, null);
                }
            }

            if (e.NewValue != null)
            {
                var enabled = (bool)e.NewValue;

                if (enabled)
                {
                    var comboBox = (ComboBox)d;
                    var behavior = new WatermarkComboBoxBehavior(comboBox);
                    behavior.Attach();

                    SetWatermarkComboBoxBehavior(comboBox, behavior);
                }
            }
        }

        private void Attach()
        {
            _mComboBox.Loaded += ComboBoxLoaded;
            _mComboBox.DragEnter += ComboBoxDragEnter;
            _mComboBox.DragLeave += ComboBoxDragLeave;
        }

        private void Detach()
        {
            _mComboBox.Loaded -= ComboBoxLoaded;
            _mComboBox.DragEnter -= ComboBoxDragEnter;
            _mComboBox.DragLeave -= ComboBoxDragLeave;
        }

        private void ComboBoxDragLeave(object sender, DragEventArgs e)
        {
            UpdateAdorner();
        }

        private void ComboBoxDragEnter(object sender, DragEventArgs e)
        {
            _mComboBox.TryRemoveAdorners<TextBlockAdorner>();
        }

        private void ComboBoxLoaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        #endregion

        #region Attached Properties

        #region Label

        public static string GetLabel(ComboBox obj)
        {
            return (string)obj.GetValue(LabelProperty);
        }

        public static void SetLabel(ComboBox obj, string value)
        {
            obj.SetValue(LabelProperty, value);
        }

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.RegisterAttached("Label", typeof(string), typeof(WatermarkComboBoxBehavior));

        #endregion

        #region LabelStyle

        public static Style GetLabelStyle(ComboBox obj)
        {
            return (Style)obj.GetValue(LabelStyleProperty);
        }

        public static void SetLabelStyle(ComboBox obj, Style value)
        {
            obj.SetValue(LabelStyleProperty, value);
        }

        public static readonly DependencyProperty LabelStyleProperty =
            DependencyProperty.RegisterAttached("LabelStyle", typeof(Style),
                                                typeof(WatermarkComboBoxBehavior));

        #endregion

        #endregion

        private void Init()
        {
            _mTextBlockAdorner = new TextBlockAdorner(_mComboBox, GetLabel(_mComboBox), GetLabelStyle(_mComboBox));
            UpdateAdorner();

            DependencyPropertyDescriptor focusProp = DependencyPropertyDescriptor.FromProperty(UIElement.IsFocusedProperty, typeof(ComboBox));
            if (focusProp != null)
            {
                focusProp.AddValueChanged(_mComboBox, (sender, args) => UpdateAdorner());
            }

            DependencyPropertyDescriptor focusKeyboardProp = DependencyPropertyDescriptor.FromProperty(UIElement.IsKeyboardFocusedProperty, typeof(ComboBox));
            if (focusKeyboardProp != null)
            {
                focusKeyboardProp.AddValueChanged(_mComboBox, (sender, args) => UpdateAdorner());
            }

            DependencyPropertyDescriptor focusKeyboardWithinProp = DependencyPropertyDescriptor.FromProperty(UIElement.IsKeyboardFocusWithinProperty, typeof(ComboBox));
            if (focusKeyboardWithinProp != null)
            {
                focusKeyboardWithinProp.AddValueChanged(_mComboBox, (sender, args) => UpdateAdorner());
            }

            DependencyPropertyDescriptor textProp = DependencyPropertyDescriptor.FromProperty(ComboBox.TextProperty, typeof(ComboBox));
            if (textProp != null)
            {
                textProp.AddValueChanged(_mComboBox, (sender, args) => UpdateAdorner());
            }

            DependencyPropertyDescriptor selectedIndexProp = DependencyPropertyDescriptor.FromProperty(Selector.SelectedIndexProperty, typeof(ComboBox));
            if (selectedIndexProp != null)
            {
                selectedIndexProp.AddValueChanged(_mComboBox, (sender, args) => UpdateAdorner());
            }

            DependencyPropertyDescriptor selectedItemProp = DependencyPropertyDescriptor.FromProperty(Selector.SelectedItemProperty, typeof(ComboBox));
            if (selectedItemProp != null)
            {
                selectedItemProp.AddValueChanged(_mComboBox, (sender, args) => UpdateAdorner());
            }
        }

        private void UpdateAdorner()
        {
            if (!string.IsNullOrEmpty(_mComboBox.Text) ||
                _mComboBox.IsFocused ||
                _mComboBox.IsKeyboardFocused ||
                _mComboBox.IsKeyboardFocusWithin ||
                _mComboBox.SelectedIndex != -1 ||
                _mComboBox.SelectedItem != null)
            {
                // Hide the Watermark Label if the adorner layer is visible
                _mComboBox.ToolTip = GetLabel(_mComboBox);
                _mComboBox.TryRemoveAdorners<TextBlockAdorner>();
            }
            else
            {
                // Show the Watermark Label if the adorner layer is visible
                _mComboBox.ToolTip = null;
                _mComboBox.TryAddAdorner<TextBlockAdorner>(_mTextBlockAdorner);
            }
        }
    }
}