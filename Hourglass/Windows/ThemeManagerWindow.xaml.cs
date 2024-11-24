// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThemeManagerWindow.xaml.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Windows;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Extensions;
using Managers;
using Timing;

// ReSharper disable LocalSuppression

/// <summary>
/// The state of the window used to manage themes.
/// </summary>
public enum ThemeManagerWindowState
{
    /// <summary>
    /// The window is displaying a built-in theme that cannot be edited.
    /// </summary>
    BuiltInTheme,

    /// <summary>
    /// The window is displaying a user-provided theme that has not yet been edited.
    /// </summary>
    UserThemeUnedited,

    /// <summary>
    /// The window is displaying a user-provided theme that has been edited.
    /// </summary>
    UserThemeEdited
}

/// <summary>
/// The window used to manage themes.
/// </summary>
public sealed partial class ThemeManagerWindow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeManagerWindow"/> class.
    /// </summary>
    /// <param name="timerWindow">The <see cref="TimerWindow"/> to edit the theme for.</param>
    public ThemeManagerWindow(TimerWindow timerWindow)
    {
        InitializeComponent();
        BindThemesComboBox();

        TimerWindow = timerWindow;
    }

    /// <summary>
    /// Gets the state of the window.
    /// </summary>
    public ThemeManagerWindowState State
    {
        get;

        private set
        {
            field = value;
            BindState();
        }
    }

    /// <summary>
    /// Gets the <see cref="TimerWindow"/> that will be updated when a theme is selected in this window.
    /// </summary>
    public TimerWindow TimerWindow
    {
        get;

        private set
        {
            field = value;
            BindTimerWindow();
        }
    }

    /// <summary>
    /// Gets the currently selected theme.
    /// </summary>
    public Theme SelectedTheme
    {
        get;

        private set
        {
            field = value;
            BindSelectedTheme();
        }
    } = null!;

    /// <summary>
    /// Gets or sets a copy of the currently selected theme. The changes to this copy are applied to the selected
    /// theme when the user saves their changes.
    /// </summary>
    private Theme EditedTheme
    {
        get;

        set
        {
            field = value;
            DataContext = field;
        }
    } = null!;

    /// <summary>
    /// Brings the window to the front.
    /// </summary>
    public void BringToFront()
    {
        try
        {
            Show();
            Topmost = true;
            Topmost = false;
        }
        catch (InvalidOperationException)
        {
            // This happens if the window is closing when this method is called.
        }
    }

    /// <summary>
    /// Brings the window to the front, activates it, and focuses it.
    /// </summary>
    public void BringToFrontAndActivate()
    {
        BringToFront();
        Activate();
    }

    /// <summary>
    /// Sets the <see cref="TimerWindow"/> that will be updated when a theme is selected in this window.
    /// </summary>
    /// <param name="newTimerWindow">The <see cref="TimerWindow"/> to set.</param>
    public void SetTimerWindow(TimerWindow newTimerWindow)
    {
        if (PromptToSaveIfRequired())
        {
            TimerWindow = newTimerWindow;
        }
    }

    /// <summary>
    /// Removes focus from all controls.
    /// </summary>
    private void UnfocusAll()
    {
        foreach (ColorControl control in ColorsGrid.GetAllVisualChildren().OfType<ColorControl>())
        {
            control.Unfocus();
        }
    }

    /// <summary>
    /// Binds the <see cref="TimerWindow"/> to the controls.
    /// </summary>
    private void BindTimerWindow()
    {
        if (TimerWindow.Theme?.Type == ThemeType.UserProvided)
        {
            EditedTheme = CloneThemeForEditing(TimerWindow.Theme);
            SelectedTheme = TimerWindow.Theme;
            State = ThemeManagerWindowState.UserThemeUnedited;
        }
        else
        {
            EditedTheme = TimerWindow.Theme!;
            SelectedTheme = TimerWindow.Theme!;
            State = ThemeManagerWindowState.BuiltInTheme;
        }
    }

    /// <summary>
    /// Binds the selected theme to the <see cref="ThemesComboBox"/>.
    /// </summary>
    private void BindSelectedTheme()
    {
        for (int i = 0; i < ThemesComboBox.Items.Count; i++)
        {
            ComboBoxItem item = (ComboBoxItem)ThemesComboBox.Items[i];
            Theme theme = (Theme)item.Tag;
            if (theme is not null && SelectedTheme is not null && theme.Identifier == SelectedTheme.Identifier)
            {
                ThemesComboBox.SelectedIndex = i;
                break;
            }
        }
    }

    /// <summary>
    /// Binds the <see cref="ThemeManagerWindowState"/> to the controls.
    /// </summary>
    private void BindState()
    {
        switch (State)
        {
            case ThemeManagerWindowState.BuiltInTheme:
                NameTextBox.IsEnabled = false;
                DeleteButton.IsEnabled = false;

                foreach (ColorControl control in ColorsGrid.GetAllVisualChildren().OfType<ColorControl>())
                {
                    control.IsEnabled = false;
                }

                SaveButton.Visibility = Visibility.Collapsed;
                CancelButton.Visibility = Visibility.Collapsed;
                CloseButton.Visibility = Visibility.Visible;
                break;

            case ThemeManagerWindowState.UserThemeUnedited:
                NameTextBox.IsEnabled = true;
                DeleteButton.IsEnabled = true;

                foreach (ColorControl control in ColorsGrid.GetAllVisualChildren().OfType<ColorControl>())
                {
                    control.IsEnabled = true;
                }

                SaveButton.Visibility = Visibility.Collapsed;
                CancelButton.Visibility = Visibility.Collapsed;
                CloseButton.Visibility = Visibility.Visible;
                break;

            case ThemeManagerWindowState.UserThemeEdited:
                NameTextBox.IsEnabled = true;
                DeleteButton.IsEnabled = true;

                foreach (ColorControl control in ColorsGrid.GetAllVisualChildren().OfType<ColorControl>())
                {
                    control.IsEnabled = true;
                }

                SaveButton.Visibility = Visibility.Visible;
                CancelButton.Visibility = Visibility.Visible;
                CloseButton.Visibility = Visibility.Collapsed;
                break;
        }
    }

    /// <summary>
    /// Binds the themes from the <see cref="ThemeManager"/> to the <see cref="ThemesComboBox"/>.
    /// </summary>
    private void BindThemesComboBox()
    {
        ThemesComboBox.Items.Clear();

        AddThemesToComboBox(
            Properties.Resources.ThemeManagerWindowLightThemesSectionHeader,
            ThemeManager.Instance.BuiltInLightThemes);

        AddThemesToComboBox(
            Properties.Resources.ThemeManagerWindowDarkThemesSectionHeader,
            ThemeManager.Instance.BuiltInDarkThemes);

        AddThemesToComboBox(
            Properties.Resources.ThemeManagerWindowUserProvidedThemesSectionHeader,
            ThemeManager.Instance.UserProvidedThemes);

        BindSelectedTheme();
    }

    /// <summary>
    /// Adds the specified themes to the <see cref="ThemesComboBox"/>.
    /// </summary>
    /// <param name="title">The section title.</param>
    /// <param name="themes">The themes to add to the <see cref="ThemesComboBox"/>.</param>
    private void AddThemesToComboBox(string title, IReadOnlyCollection<Theme> themes)
    {
        if (themes.Count == 0)
        {
            return;
        }

        // Spacing between sections
        if (ThemesComboBox.Items.Count > 0)
        {
            ThemesComboBox.Items.Add(new ComboBoxItem { IsEnabled = false });
        }

        // Section header
        ThemesComboBox.Items.Add(new ComboBoxItem
        {
            Content = title,
            IsEnabled = false,
            FontStyle = FontStyles.Italic,
            FontWeight = FontWeights.Bold
        });

        // Themes in section
        foreach (Theme theme in themes)
        {
            ComboBoxItem item = new()
            {
                Content = theme.Name,
                Tag = theme
            };
            ThemesComboBox.Items.Add(item);
        }
    }

    /// <summary>
    /// Prompts the user to save unsaved changes to the selected theme, if there are any.
    /// </summary>
    /// <returns><c>true</c> if the theme has been saved or the user has elected to discard unsaved changes, or
    /// <c>false</c> if the user has elected to cancel the operation.</returns>
    private bool PromptToSaveIfRequired()
    {
        if (State == ThemeManagerWindowState.UserThemeEdited)
        {
            MessageBoxResult result = this.ShowTaskDialog(
                Properties.Resources.ThemeManagerWindowSaveTaskDialogInstruction,
                Properties.Resources.ThemeManagerWindowSaveTaskDialogCommand,
                Properties.Resources.ThemeManagerWindowDontSaveTaskDialogCommand);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    SaveChanges();
                    return true;

                case MessageBoxResult.No:
                    return true;

                case MessageBoxResult.Cancel:
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Saves changes to the currently selected theme.
    /// </summary>
    private void SaveChanges()
    {
        if (State == ThemeManagerWindowState.UserThemeEdited)
        {
            SelectedTheme.Set(EditedTheme);
            State = ThemeManagerWindowState.UserThemeUnedited;
            BindThemesComboBox();
            UnfocusAll();
        }
    }

    /// <summary>
    /// Reverts changes to the currently selected theme.
    /// </summary>
    private void RevertChanges()
    {
        if (State == ThemeManagerWindowState.UserThemeEdited)
        {
            EditedTheme = CloneThemeForEditing(SelectedTheme);
            State = ThemeManagerWindowState.UserThemeUnedited;
            UnfocusAll();
        }
    }

    /// <summary>
    /// Clones a theme. This creates a clone of an existing <see cref="Theme"/>, but with a new identifier and a
    /// <see cref="ThemeType.UserProvided"/> type.
    /// </summary>
    /// <param name="theme">A <see cref="Theme"/>.</param>
    /// <returns>The cloned theme.</returns>
    private static Theme CloneThemeForEditing(Theme theme)
    {
        string identifier = Guid.NewGuid().ToString();
        return Theme.FromTheme(ThemeType.UserProvided, identifier, theme.Name, theme);
    }

    /// <summary>
    /// Invoked when the selection in the <see cref="ThemesComboBox"/> changes.
    /// </summary>
    /// <param name="sender">The <see cref="ThemesComboBox"/>.</param>
    /// <param name="e">The event data.</param>
    private void ThemesComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ComboBoxItem selectedItem = (ComboBoxItem)ThemesComboBox.SelectedItem;
        if (selectedItem is null)
        {
            return;
        }

        Theme newSelectedTheme = (Theme)selectedItem.Tag;
        if (newSelectedTheme.Identifier == SelectedTheme.Identifier)
        {
            return;
        }

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (TimerWindow.Options is null || !PromptToSaveIfRequired())
        {
            // Revert the selection
            BindSelectedTheme();
            return;
        }

        TimerWindow.Options.Theme = newSelectedTheme;
        BindTimerWindow();
    }

    /// <summary>
    /// Invoked when the <see cref="NewButton"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="NewButton"/>.</param>
    /// <param name="e">The event data.</param>
    private void NewButtonClick(object sender, RoutedEventArgs e)
    {
        if (!PromptToSaveIfRequired())
        {
            return;
        }

        TimerWindow.Options.Theme = ThemeManager.Instance.AddThemeBasedOnTheme(SelectedTheme);
        BindThemesComboBox();
        BindTimerWindow();
    }

    /// <summary>
    /// Invoked when the <see cref="TextBox.Text"/> property value changes in the <see cref="NameTextBox"/>.
    /// </summary>
    /// <param name="sender">The <see cref="NameTextBox"/>.</param>
    /// <param name="e">The event data.</param>
    private void NameTextBoxTextChanged(object sender, TextChangedEventArgs e)
    {
        if (State == ThemeManagerWindowState.UserThemeUnedited && EditedTheme.Name != SelectedTheme.Name)
        {
            State = ThemeManagerWindowState.UserThemeEdited;
        }
    }

    /// <summary>
    /// Invoked when the <see cref="DeleteButton"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="DeleteButton"/>.</param>
    /// <param name="e">The event data.</param>
    private void DeleteButtonClick(object sender, RoutedEventArgs e)
    {
        if (SelectedTheme.Type == ThemeType.UserProvided)
        {
            MessageBoxResult result = this.ShowTaskDialog(
                Properties.Resources.ThemeManagerWindowDeleteTaskDialogInstruction,
                Properties.Resources.ThemeManagerWindowDeleteTaskDialogCommand);

            if (result == MessageBoxResult.Yes)
            {
                ThemeManager.Instance.Remove(SelectedTheme);
                BindThemesComboBox();
                BindTimerWindow();
            }
        }
    }

    /// <summary>
    /// Invoked when the <see cref="ColorControl.Color"/> property changes in a <see cref="ColorControl"/>.
    /// </summary>
    /// <param name="sender">The <see cref="ColorControl"/>.</param>
    /// <param name="e">The event data.</param>
    private void ColorControlColorChanged(object sender, EventArgs e)
    {
        if (State == ThemeManagerWindowState.UserThemeUnedited)
        {
            State = ThemeManagerWindowState.UserThemeEdited;
        }
    }

    /// <summary>
    /// Invoked when the <see cref="SaveButton"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="SaveButton"/>.</param>
    /// <param name="e">The event data.</param>
    private void SaveButtonClick(object sender, RoutedEventArgs e)
    {
        SaveChanges();
    }

    /// <summary>
    /// Invoked when the <see cref="CancelButton"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="CancelButton"/>.</param>
    /// <param name="e">The event data.</param>
    private void CancelButtonClick(object sender, RoutedEventArgs e)
    {
        RevertChanges();
    }

    /// <summary>
    /// Invoked when the <see cref="CloseButton"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="CloseButton"/>.</param>
    /// <param name="e">The event data.</param>
    private void CloseButtonClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// Invoked directly after <see cref="Window.Close"/> is called, and can be handled to cancel window closure.
    /// </summary>
    /// <param name="sender">The <see cref="ThemeManagerWindow"/>.</param>
    /// <param name="e">The event data.</param>
    private void WindowClosing(object sender, CancelEventArgs e)
    {
        e.Cancel = !PromptToSaveIfRequired();
    }
}