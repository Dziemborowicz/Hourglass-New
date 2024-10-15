// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Watermark.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Windows;

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

/// <summary>
/// Provides a <see cref="DependencyProperty"/> that allows a watermark to be applied to <see cref="TextBox"/> and
/// <see cref="ComboBox"/> controls when the control has no actual value.
/// </summary>
/// <seealso cref="WatermarkAdorner"/>
public static class Watermark
{
    /// <summary>
    /// A <see cref="DependencyProperty"/> that specifies the content of the watermark to be displayed when the
    /// control has no actual value and no keyboard focus, typically a <see cref="string"/>.
    /// </summary>
    public static readonly DependencyProperty HintProperty = DependencyProperty.RegisterAttached(
        "Hint",
        typeof(object),
        typeof(Watermark),
        new FrameworkPropertyMetadata(DependencyPropertyChanged));

    /// <summary>
    /// A <see cref="DependencyProperty"/> that specifies the foreground of the watermark to be displayed when the
    /// control has no actual value and no keyboard focus.
    /// </summary>
    public static readonly DependencyProperty HintBrushProperty = DependencyProperty.RegisterAttached(
        "HintBrush",
        typeof(Brush),
        typeof(Watermark),
        new FrameworkPropertyMetadata(DependencyPropertyChanged));

    /// <summary>
    /// Returns the value of the <see cref="HintProperty"/>.
    /// </summary>
    /// <param name="control">A <see cref="Control"/>.</param>
    /// <returns>The value of the <see cref="HintProperty"/>.</returns>
    public static object? GetHint(Control control)
    {
        return control.GetValue(HintProperty);
    }

    /// <summary>
    /// Sets the value of the <see cref="HintProperty"/>.
    /// </summary>
    /// <param name="control">A <see cref="Control"/>.</param>
    /// <param name="value">The value to set.</param>
    public static void SetHint(Control control, object? value)
    {
        control.SetValue(HintProperty, value);
    }

    /// <summary>
    /// Returns the value of the <see cref="HintBrushProperty"/>.
    /// </summary>
    /// <param name="control">A <see cref="Control"/>.</param>
    /// <returns>The value of the <see cref="HintBrushProperty"/>.</returns>
    public static Brush? GetHintBrush(Control control)
    {
        return (Brush)control.GetValue(HintBrushProperty);
    }

    /// <summary>
    /// Sets the value of the <see cref="HintBrushProperty"/>.
    /// </summary>
    /// <param name="control">A <see cref="Control"/>.</param>
    /// <param name="value">The value to set.</param>
    public static void SetHintBrush(Control control, Brush? value)
    {
        control.SetValue(HintBrushProperty, value);
    }

    /// <summary>
    /// Invoked when the effective value of a <see cref="DependencyProperty"/> changes.
    /// </summary>
    /// <param name="sender">The <see cref="DependencyObject"/> on which the <see cref="DependencyProperty"/> has
    /// changed value.</param>
    /// <param name="e">Event data that is issued by any event that tracks changes to the effective value of this
    /// property.</param>
    private static void DependencyPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not TextBox && sender is not ComboBox)
        {
            return;
        }

        Control control = (Control)sender;
        BindControl(control);
        UpdateWatermark(control);
    }

    /// <summary>
    /// Binds the event handlers for to a <see cref="Control"/>.
    /// </summary>
    /// <param name="control">A <see cref="Control"/>.</param>
    private static void BindControl(Control control)
    {
        control.Loaded -= ControlLoaded;
        control.Loaded += ControlLoaded;

        control.GotKeyboardFocus -= ControlGotKeyboardFocus;
        control.GotKeyboardFocus += ControlGotKeyboardFocus;

        control.LostKeyboardFocus -= ControlLostKeyboardFocus;
        control.LostKeyboardFocus += ControlLostKeyboardFocus;

        switch (control)
        {
            case TextBox textBox:
                textBox.TextChanged -= TextBoxTextChanged;
                textBox.TextChanged += TextBoxTextChanged;
                break;
            case ComboBox comboBox:
                comboBox.SelectionChanged -= ComboBoxSelectionChanged;
                comboBox.SelectionChanged += ComboBoxSelectionChanged;

                comboBox.RemoveHandler(TextBoxBase.TextChangedEvent, new RoutedEventHandler(ComboBoxTextChanged));
                comboBox.AddHandler(TextBoxBase.TextChangedEvent, new RoutedEventHandler(ComboBoxTextChanged));
                break;
        }
    }

    /// <summary>
    /// Invoked when the control is laid out, rendered, and ready for interaction.
    /// </summary>
    /// <param name="sender">The control.</param>
    /// <param name="e">The event data.</param>
    private static void ControlLoaded(object sender, RoutedEventArgs e)
    {
        UpdateWatermark(sender);
    }

    /// <summary>
    /// Invoked when the keyboard is focused on a control.
    /// </summary>
    /// <param name="sender">The control.</param>
    /// <param name="e">The event data.</param>
    private static void ControlGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        UpdateWatermark(sender);
    }

    /// <summary>
    /// Invoked when the keyboard is no longer focused on a control.
    /// </summary>
    /// <param name="sender">The control.</param>
    /// <param name="e">The event data.</param>
    private static void ControlLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        UpdateWatermark(sender);
    }

    /// <summary>
    /// Invoked when content changes in a text box control.
    /// </summary>
    /// <param name="sender">The control.</param>
    /// <param name="e">The event data.</param>
    private static void TextBoxTextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateWatermark(sender);
    }

    /// <summary>
    /// Invoked when the selection of a combo box control changes.
    /// </summary>
    /// <param name="sender">The control.</param>
    /// <param name="e">The event data.</param>
    private static void ComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateWatermark(sender);
    }

    /// <summary>
    /// Invoked when content changes in a combo box control changes.
    /// </summary>
    /// <param name="sender">The control.</param>
    /// <param name="e">The event data.</param>
    private static void ComboBoxTextChanged(object sender, RoutedEventArgs e)
    {
        UpdateWatermark(sender);
    }

    /// <summary>
    /// Adds a <see cref="WatermarkAdorner"/> to a <see cref="Control"/>.
    /// </summary>
    /// <param name="control">A <see cref="Control"/>.</param>
    /// <param name="hint">The content of the watermark, typically a <see cref="string"/>.</param>
    /// <param name="brush">The foreground of the watermark.</param>
    private static void AddWatermarkAdorner(Control control, object hint, Brush brush)
    {
        AdornerLayer.GetAdornerLayer(control)?.Add(new WatermarkAdorner(control, hint, brush));
    }

    /// <summary>
    /// Removes the <see cref="WatermarkAdorner"/> from a <see cref="Control"/>.
    /// </summary>
    /// <param name="control">A <see cref="Control"/>.</param>
    private static void RemoveWatermarkAdorner(Control control)
    {
        AdornerLayer? layer = AdornerLayer.GetAdornerLayer(control);
        if (layer is null)
        {
            return;
        }

        Adorner[]? adorners = layer.GetAdorners(control);
        if (adorners is null)
        {
            return;
        }

        foreach (WatermarkAdorner adorner in adorners.OfType<WatermarkAdorner>())
        {
            layer.Remove(adorner);
        }
    }

    private static void UpdateWatermark(object sender) => UpdateWatermark((Control) sender);

    /// <summary>
    /// Updates the <see cref="WatermarkAdorner"/> on the <see cref="Control"/>.
    /// </summary>
    /// <param name="control">A <see cref="Control"/>.</param>
    private static void UpdateWatermark(Control control)
    {
        object? hint = GetHint(control);
        Brush? brush = GetHintBrush(control);

        if (!control.IsKeyboardFocused && !HasActualValue(control) && hint is not null && brush is not null)
        {
            WatermarkAdorner? watermarkAdorner = GetWatermarkAdorner(control);

            if (watermarkAdorner is null)
            {
                AddWatermarkAdorner(control, hint, brush);
            }
            else
            {
                watermarkAdorner.Hint = hint;
                watermarkAdorner.Brush = brush;
                UpdateAdornerLayer(control);
            }
        }
        else
        {
            RemoveWatermarkAdorner(control);
        }
    }

    /// <summary>
    /// Returns a value indicating whether a <see cref="Control"/> has an actual value (where the text property is
    /// not <c>null</c> or empty).
    /// </summary>
    /// <param name="control">A <see cref="Control"/>.</param>
    /// <returns>A value indicating whether the <see cref="Control"/> has an actual value (where the text property is not <c>null</c> or empty).</returns>
    private static bool HasActualValue(Control control)
    {
        if (control is TextBox textBox)
        {
            return !string.IsNullOrWhiteSpace(textBox.Text);
        }

        if (control is ComboBox comboBox)
        {
            return !string.IsNullOrWhiteSpace(comboBox.Text);
        }

        return true;
    }

    /// <summary>
    /// Returns a <see cref="WatermarkAdorner"/> for a <see cref="Control"/>, or <c>null</c> if none exists.
    /// </summary>
    /// <param name="control">A <see cref="Control"/>.</param>
    /// <returns>A <see cref="WatermarkAdorner"/> for the <see cref="Control"/>, or <c>null</c> if none exists.</returns>
    private static WatermarkAdorner? GetWatermarkAdorner(Control control)
    {
        return AdornerLayer.GetAdornerLayer(control)?
            .GetAdorners(control)?
            .OfType<WatermarkAdorner>()
            .FirstOrDefault();
    }

    /// <summary>
    /// Forces the <see cref="AdornerLayer"/> to re-render.
    /// </summary>
    /// <param name="control">A <see cref="Control"/>.</param>
    private static void UpdateAdornerLayer(Control control)
    {
        AdornerLayer.GetAdornerLayer(control)?.Update();
    }
}