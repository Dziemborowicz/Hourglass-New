// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThemeInfo.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Serialization;

using System.Windows.Media;

using Timing;

/// <summary>
/// The representation of a <see cref="Theme"/> used for XML serialization.
/// </summary>
public sealed class ThemeInfo
{
    /// <summary>
    /// Gets or sets the unique identifier for this theme.
    /// </summary>
    public string Identifier { get; set; } = null!;

    /// <summary>
    /// Gets or sets the friendly name for this theme, or <c>null</c> if no friendly name is specified.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the background color of the window.
    /// </summary>
    public Color BackgroundColor { get; set; }

    /// <summary>
    /// Gets or sets the color of the progress bar.
    /// </summary>
    public Color ProgressBarColor { get; set; }

    /// <summary>
    /// Gets or sets the background color of the progress bar.
    /// </summary>
    public Color ProgressBackgroundColor { get; set; }

    /// <summary>
    /// Gets or sets the color that is flashed on expiration.
    /// </summary>
    public Color ExpirationFlashColor { get; set; }

    /// <summary>
    /// Gets or sets the color of the primary text.
    /// </summary>
    public Color PrimaryTextColor { get; set; }

    /// <summary>
    /// Gets or sets the color of the watermark in the primary text box.
    /// </summary>
    public Color PrimaryHintColor { get; set; }

    /// <summary>
    /// Gets or sets the color of any secondary text.
    /// </summary>
    public Color SecondaryTextColor { get; set; }

    /// <summary>
    /// Gets or sets the color of the watermark in any secondary text box.
    /// </summary>
    public Color SecondaryHintColor { get; set; }

    /// <summary>
    /// Gets or sets the color of the button text.
    /// </summary>
    public Color ButtonColor { get; set; }

    /// <summary>
    /// Gets or sets the color of the button text when the user hovers over the button.
    /// </summary>
    public Color ButtonHoverColor { get; set; }

    /// <summary>
    /// Gets or sets whether the user theme is the dark one.
    /// </summary>
    public bool IsUserThemeDark { get; set; }

    /// <summary>
    /// Returns a <see cref="ThemeInfo"/> for the specified <see cref="Theme"/>.
    /// </summary>
    /// <param name="options">A <see cref="Theme"/>.</param>
    /// <returns>A <see cref="ThemeInfo"/> for the specified <see cref="Theme"/>.</returns>
    public static ThemeInfo? FromTheme(Theme? options)
    {
        return options?.ToThemeInfo();
    }
}