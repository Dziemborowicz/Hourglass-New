// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SizeToFitTextBox.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Windows;

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Extensions;

/// <summary>
/// A <see cref="TextBox"/> that automatically adjusts the font size to ensure that the text is entirely visible.
/// </summary>
public sealed class SizeToFitTextBox : TextBox
{
    private FormattedText? _formattedText;

    /// <summary>
    /// Identifies the minimum font size <see cref="DependencyProperty"/>.
    /// </summary>
    public static readonly DependencyProperty MinFontSizeProperty = DependencyProperty.Register(
        nameof(MinFontSize),
        typeof(double),
        typeof(SizeToFitTextBox),
        new(double.NaN /* defaultValue */, MinFontSizePropertyChanged));

    /// <summary>
    /// Identifies the maximum font size <see cref="DependencyProperty"/>.
    /// </summary>
    public static readonly DependencyProperty MaxFontSizeProperty = DependencyProperty.Register(
        nameof(MaxFontSize),
        typeof(double),
        typeof(SizeToFitTextBox),
        new(double.NaN /* defaultValue */, MaxFontSizePropertyChanged));

    /// <summary>
    /// Initializes a new instance of the <see cref="SizeToFitTextBox"/> class.
    /// </summary>
    public SizeToFitTextBox()
    {
        Loaded += UpdateFontSizeEventHandler;
        SizeChanged += UpdateFontSizeEventHandler;
        TextChanged += UpdateFontSizeEventHandler;

        void UpdateFontSizeEventHandler(object sender, EventArgs e) =>
            UpdateFontSize();
    }

    /// <summary>
    /// Gets or sets the minimum font size.
    /// </summary>
    public double MinFontSize
    {
        get => (double) GetValue(MinFontSizeProperty);
        set => SetValue(MinFontSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum font size.
    /// </summary>
    public double MaxFontSize
    {
        get => (double) GetValue(MaxFontSizeProperty);
        set => SetValue(MaxFontSizeProperty, value);
    }

    /// <summary>
    /// Invoked when the effective value of the <see cref="MinFontSizeProperty"/> changes.
    /// </summary>
    /// <param name="sender">The <see cref="DependencyObject"/> on which the <see cref="MinFontSizeProperty"/> has
    /// changed value.</param>
    /// <param name="e">Event data that is issued by any event that tracks changes to the effective value of this
    /// property.</param>
    private static void MinFontSizePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        ((SizeToFitTextBox)sender).UpdateFontSize();
    }

    /// <summary>
    /// Invoked when the effective value of the <see cref="MaxFontSizeProperty"/> changes.
    /// </summary>
    /// <param name="sender">The <see cref="DependencyObject"/> on which the <see cref="MaxFontSizeProperty"/> has
    /// changed value.</param>
    /// <param name="e">Event data that is issued by any event that tracks changes to the effective value of this
    /// property.</param>
    private static void MaxFontSizePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        ((SizeToFitTextBox)sender).UpdateFontSize();
    }

    /// <summary>
    /// Updates the <see cref="TextBox.FontSize"/> property to ensure that the text is entirely visible.
    /// </summary>
    private void UpdateFontSize()
    {
        if (!MinFontSize.IsFinite() || !MaxFontSize.IsFinite())
        {
            return;
        }

        double desiredFontSize = MathExtensions.LimitToRange(
            GetViewWidth() / GetTextWidth() * FontSize,
            MinFontSize,
            MaxFontSize);

        if (desiredFontSize.IsFinite() && desiredFontSize > 0.0)
        {
            FontSize = desiredFontSize;
        }
    }

    /// <summary>
    /// Returns the width of the text in the text box.
    /// </summary>
    /// <returns>The width of the text in the text box.</returns>
    private double GetTextWidth()
    {
        if (_formattedText is null)
        {
            Typeface typeface = new(
                FontFamily,
                FontStyle,
                FontWeight,
                FontStretch);

            _formattedText = new(
                Text,
                CultureInfo.CurrentCulture,
                FlowDirection,
                typeface,
                FontSize,
                Foreground,
                GetPixelsPerDip());
        }
        else
        {
            _formattedText.PixelsPerDip = GetPixelsPerDip();
        }

        return _formattedText.WidthIncludingTrailingWhitespace;

        double GetPixelsPerDip()
        {
            return VisualTreeHelper.GetDpi(this).PixelsPerDip;
        }
    }

    /// <summary>
    /// Returns the width of the control that contains the text.
    /// </summary>
    /// <returns>The width of the control that contains the text.</returns>
    private double GetViewWidth()
    {
        // This is the control closest to the text and gives the most accurate width
        DependencyObject? textBoxView = this.FindVisualChild(WindowExtensions.IsTextBoxView);

        // Since TextBoxView is internal, fall back to this if it is not found
        FrameworkElement view = textBoxView as FrameworkElement ?? this;

        return view.ActualWidth;
    }
}