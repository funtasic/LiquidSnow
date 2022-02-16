﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Thismaker.Goro
{
    /// <summary>
    /// Extensions for controls
    /// </summary>
    public static class ControlExtensions
    {
        #region Header
        /// <summary>
        /// Registers Header as an attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty HeaderProperty=
            DependencyProperty.RegisterAttached("Header", typeof(string), typeof(ControlExtensions), new PropertyMetadata());

        /// <summary>
        /// Sets the attached Header property of the <see cref="UIElement"/>
        /// </summary>
        public static void SetHeader(UIElement element, string header)
        {
            element.SetValue(HeaderProperty, header);
        }

        /// <summary>
        /// Gets the attached Header property of the <see cref="UIElement"/>
        /// </summary>
        public static string GetHeader(UIElement element)
        {
            return (string)element.GetValue(HeaderProperty);
        }
        #endregion

        #region Placeholder
        /// <summary>
        /// Registers PlaceholderText as an attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty PlaceholderTextProperty =
           DependencyProperty.RegisterAttached("PlaceholderText", typeof(string), typeof(ControlExtensions), new PropertyMetadata());

        /// <summary>
        /// Sets the attached PlaceholderText property of the <see cref="UIElement"/>
        /// </summary>
        public static void SetPlaceholderText(UIElement element, string header)
        {
            element.SetValue(PlaceholderTextProperty, header);
        }

        /// <summary>
        /// Gets the attached PlaceholderText property of the <see cref="UIElement"/>
        /// </summary>
        public static string GetPlaceholderText(UIElement element)
        {
            return (string)element.GetValue(PlaceholderTextProperty);
        }
        #endregion

        #region PasswordLength
        /// <summary>
        /// Registers the PasswordLength as a dependency property and obtains the key.
        /// </summary>
        internal static readonly DependencyPropertyKey PasswordLengthKey =
            DependencyProperty.RegisterAttachedReadOnly("PasswordLength", typeof(int), typeof(ControlExtensions), new PropertyMetadata(0));

        /// <summary>
        /// The PasswordLength dependency property
        /// </summary>
        public static readonly DependencyProperty PasswordLengthProperty =
            PasswordLengthKey.DependencyProperty;

        /// <summary>
        /// Returns the passwordLength of a <see cref="PasswordBox"/>
        /// </summary>
        public static int GetPasswordLength(UIElement element)
        {
            return ((PasswordBox)element).Password.Length;
        }
        #endregion

        #region Icon

        /// <summary>
        /// Defines Icon as a dependency property
        /// </summary>
        public static readonly DependencyProperty IconProperty =
           DependencyProperty.RegisterAttached("Icon", typeof(Icon), typeof(ControlExtensions), new PropertyMetadata(Icon.None));

        /// <summary>
        /// Sets the attached Icon property of the <see cref="UIElement"/>
        /// </summary>
        public static void SetIcon(UIElement element, Icon icon)
        {
            element.SetValue(IconProperty, icon);
        }

        /// <summary>
        /// Gets the attached Icon property of the <see cref="UIElement"/>
        /// </summary>
        public static Icon GetIcon(UIElement element)
        {
            return (Icon)element.GetValue(IconProperty);
        }
        #endregion

        #region Design
        /// <summary>
        /// Registers Design as an attached dependency property
        /// </summary>
        public static readonly DependencyProperty DesignProperty =
           DependencyProperty.RegisterAttached("Design", typeof(IconDesign), typeof(ControlExtensions), new PropertyMetadata(IconDesign.Segoe));

        /// <summary>
        /// Sets the attached Design property of the <see cref="UIElement"/>
        /// </summary>
        public static void SetDesign(UIElement element, IconDesign Design)
        {
            element.SetValue(DesignProperty, Design);
        }

        /// <summary>
        /// Gets the attached Design property of the <see cref="UIElement"/>
        /// </summary>
        public static IconDesign GetDesign(UIElement element)
        {
            return (IconDesign)element.GetValue(DesignProperty);
        }
        #endregion

        #region DialogResult

        /// <summary>
        /// Registers DialogResult as an attached dependency property
        /// </summary>
        public static readonly DependencyProperty DialogResultProperty =
            DependencyProperty.RegisterAttached(
                "DialogResult",
                typeof(bool?),
                typeof(ControlExtensions),
                new PropertyMetadata(DialogResultChanged));

        private static void DialogResultChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window)
                window.DialogResult = e.NewValue as bool?;
        }

        /// <summary>
        /// Sets the attached DialogResult property of the UIElement
        /// </summary>
        public static void SetDialogResult(Window target, bool? value)
        {
            target.SetValue(DialogResultProperty, value);
        }
        #endregion

    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class InputMonitor : DependencyObject
    {
        public static bool GetIsMonitoring(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsMonitoringProperty);
        }

        public static void SetIsMonitoring(DependencyObject obj, bool value)
        {
            obj.SetValue(IsMonitoringProperty, value);
        }

        public static readonly DependencyProperty IsMonitoringProperty =
            DependencyProperty.RegisterAttached("IsMonitoring", typeof(bool), typeof(InputMonitor), new UIPropertyMetadata(false, OnIsMonitoringChanged));


        public static bool GetIsEmpty(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEmptyProperty);
        }

        public static void SetIsEmpty(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEmptyProperty, value);
        }

        public static readonly DependencyProperty IsEmptyProperty =
            DependencyProperty.RegisterAttached("IsEmpty", typeof(bool), typeof(InputMonitor), new UIPropertyMetadata(true));

        private static void OnIsMonitoringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d == null)
            {
                return;
            }
            if ((bool)e.NewValue)
            {
                if (d.GetType() == typeof(RichTextBox))
                {
                    ((RichTextBox)d).TextChanged += InputChanged;
                }
                else if (d.GetType() == typeof(TextBox))
                {
                    ((TextBox)d).TextChanged += InputChanged;
                }
                else if(d.GetType()==typeof(PasswordBox))
                {
                    ((PasswordBox)d).PasswordChanged += InputChanged;
                }
            }
            else
            {

                if (d.GetType() == typeof(RichTextBox))
                {
                    ((RichTextBox)d).TextChanged -= InputChanged;
                }
                else if (d.GetType() == typeof(TextBox))
                {
                    ((TextBox)d).TextChanged -= InputChanged;
                }
                else if (d.GetType() == typeof(PasswordBox))
                {
                    ((PasswordBox)d).PasswordChanged -= InputChanged;
                }
            }
        }

        static void InputChanged(object sender, RoutedEventArgs e)
        {
            bool isEmpty;


            if (sender.GetType() == typeof(TextBox))
            {
                isEmpty = string.IsNullOrEmpty(((TextBox)sender).Text);
            }
            else if (sender.GetType() == typeof(RichTextBox))
            {
                isEmpty = ((RichTextBox)sender).Document.IsEmpty();
            }
            else if(sender.GetType()==typeof(PasswordBox))
            {
                isEmpty = string.IsNullOrEmpty(((PasswordBox)sender).Password);
            }
            else
            {
                return;
            }

            SetIsEmpty((DependencyObject)sender, isEmpty);

        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}