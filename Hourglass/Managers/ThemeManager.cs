// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThemeManager.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Managers;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Linq;

using Properties;
using Timing;

/// <summary>
/// Manages themes.
/// </summary>
public sealed class ThemeManager : Manager
{
    private const string DefaultThemeIdentifier = "accent";
    private const string DefaultDarkThemeIdentifier = $"{DefaultThemeIdentifier}-dark";

    /// <summary>
    /// Singleton instance of the <see cref="ThemeManager"/> class.
    /// </summary>
    public static readonly ThemeManager Instance = new();

    /// <summary>
    /// A collection of themes.
    /// </summary>
    private readonly List<Theme> _themes = [];

    /// <summary>
    /// Prevents a default instance of the <see cref="ThemeManager"/> class from being created.
    /// </summary>
    private ThemeManager()
    {
    }

    /// <summary>
    /// Gets the default theme.
    /// </summary>
    public Theme DefaultTheme => GetThemeByIdentifier(DefaultThemeIdentifier)!;

    /// <summary>
    /// Gets the default dark theme.
    /// </summary>
    public Theme DefaultDarkTheme => GetThemeByIdentifier(DefaultDarkThemeIdentifier)!;

    /// <summary>
    /// Gets a collection of the themes stored in the assembly.
    /// </summary>
#pragma warning disable S2365
    public IList<Theme> BuiltInThemes => _themes.Where(static t => t.Type != ThemeType.UserProvided).ToList();
#pragma warning restore S2365

    /// <summary>
    /// Gets a collection of the light themes stored in the assembly.
    /// </summary>
#pragma warning disable S2365
    public IList<Theme> BuiltInLightThemes => _themes.Where(static t => t.Type == ThemeType.BuiltInLight).ToList();
#pragma warning restore S2365

    /// <summary>
    /// Gets a collection of the dark themes stored in the assembly.
    /// </summary>
#pragma warning disable S2365
    public IList<Theme> BuiltInDarkThemes => _themes.Where(static t => t.Type == ThemeType.BuiltInDark).ToList();
#pragma warning restore S2365

    /// <summary>
    /// Gets a collection of the themes defined by the user ordered by name.
    /// </summary>
#pragma warning disable S2365
#pragma warning disable IDE0305
    public IList<Theme> UserProvidedThemes => _themes.Where(static t => t.Type == ThemeType.UserProvided).OrderBy(t => t.Name).ToList();
#pragma warning restore IDE0305
#pragma warning restore S2365

    /// <summary>
    /// Initializes the class.
    /// </summary>
    public override void Initialize()
    {
        _themes.Clear();
        AddRange(GetBuiltInThemes());
        AddRange(GetUserProvidedThemes());
    }

    /// <summary>
    /// Persists the state of the class.
    /// </summary>
    public override void Persist()
    {
        Settings.Default.UserProvidedThemes = UserProvidedThemes;
    }

    /// <summary>
    /// Adds a theme.
    /// </summary>
    /// <param name="theme">A <see cref="Theme"/>.</param>
    public void Add(Theme theme)
    {
        if (GetThemeByIdentifier(theme.Identifier) is null)
        {
            _themes.Add(theme);
        }
    }

    /// <summary>
    /// Adds the themes of the specified collection.
    /// </summary>
    /// <param name="collection">A collection of <see cref="Theme"/>s.</param>
    public void AddRange(IEnumerable<Theme> collection)
    {
        foreach (Theme theme in collection)
        {
            Add(theme);
        }
    }

    /// <summary>
    /// Adds a theme based on another theme.
    /// </summary>
    /// <param name="theme">A <see cref="Theme"/>.</param>
    /// <returns>The newly added theme.</returns>
    public Theme AddThemeBasedOnTheme(Theme theme)
    {
        string identifier = Guid.NewGuid().ToString();
        string name = Resources.ThemeManagerNewTheme;
        Theme newTheme = Theme.FromTheme(ThemeType.UserProvided, identifier, name, theme);
        Add(newTheme);
        return newTheme;
    }

    /// <summary>
    /// Returns the theme for the specified identifier, or <c>null</c> if no such theme is loaded.
    /// </summary>
    /// <param name="identifier">The identifier for the theme.</param>
    /// <returns>The theme for the specified identifier, or <c>null</c> if no such theme is loaded.</returns>
    public Theme? GetThemeByIdentifier(string? identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return null;
        }

        return _themes.Find(t => t.Identifier == identifier);
    }

    /// <summary>
    /// Returns the theme for the specified identifier, or <see cref="DefaultTheme"/> if no such theme is loaded.
    /// </summary>
    /// <param name="identifier">The identifier for the theme.</param>
    /// <returns>The theme for the specified identifier, or <see cref="DefaultTheme"/> if no such theme is loaded.
    /// </returns>
    public Theme GetThemeOrDefaultByIdentifier(string? identifier)
    {
        return GetThemeByIdentifier(identifier) ?? DefaultTheme;
    }

    /// <summary>
    /// Returns the first theme for the specified name, or <c>null</c> if no such theme is loaded.
    /// </summary>
    /// <param name="name">The name for the theme.</param>
    /// <param name="stringComparison">One of the enumeration values that specifies how the strings will be
    /// compared.</param>
    /// <returns>The first theme for the specified name, or <c>null</c> if no such theme is loaded.</returns>
    public Theme? GetThemeByName(string name, StringComparison stringComparison = StringComparison.Ordinal)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return _themes.Find(t => string.Equals(t.Name, name, stringComparison));
    }

    /// <summary>
    /// Returns the light variant of a theme.
    /// </summary>
    /// <param name="theme">A theme.</param>
    /// <returns>The light variant of the <paramref name="theme"/>.</returns>
    public Theme GetLightVariantForTheme(Theme theme)
    {
        return theme.Type switch
        {
            ThemeType.BuiltInLight => theme,
            ThemeType.BuiltInDark  => GetThemeOrDefaultByIdentifier(theme.Identifier.Replace("-dark", string.Empty)),
            ThemeType.UserProvided => DefaultTheme,
            _ => throw new ArgumentException(@$"Unknown theme {theme.Type}", nameof(theme))
        };
    }

    /// <summary>
    /// Returns the dark variant of a theme.
    /// </summary>
    /// <param name="theme">A theme.</param>
    /// <returns>The dark variant of the <paramref name="theme"/>.</returns>
    public Theme GetDarkVariantForTheme(Theme theme)
    {
        return theme.Type switch
        {
            ThemeType.BuiltInLight => GetThemeOrDefaultByIdentifier(theme.Identifier + "-dark"),
            ThemeType.BuiltInDark  => theme,
            ThemeType.UserProvided => DefaultDarkTheme,
            _ => throw new ArgumentException(@$"Unknown theme {theme.Type}", nameof(theme))
        };
    }

    /// <summary>
    /// Removes a theme, and updates any timers using the theme to use the default theme.
    /// </summary>
    /// <param name="theme">A <see cref="Theme"/>.</param>
    public void Remove(Theme theme)
    {
        foreach (Timer timer in TimerManager.Instance.Timers.Where(t => t.Options.Theme == theme))
        {
            timer.Options.Theme = DefaultTheme;
        }

        _themes.Remove(theme);
    }

    // https://learn.microsoft.com/windows-hardware/customize/desktop/unattend/microsoft-windows-shell-setup-themes-windowcolor#values
    private static readonly Color DefaultWindowsAccentColor = Color.FromArgb(0xff, 0x00, 0x78, 0xd7);

    private static string GetWindowsAccentColor()
    {
        Color color;

        try
        {
            DwmGetColorizationParameters(out var colors);
            color = Color.FromArgb((int)colors.ColorizationColor);
        }
        catch
        {
            color = DefaultWindowsAccentColor;
        }

        return ColorTranslator.ToHtml(color);

        // Undocumented. Gets DWM colorization parameters.
        [DllImport("dwmapi.dll", EntryPoint = "#127", PreserveSig = false, CharSet = CharSet.Unicode)]
        static extern void DwmGetColorizationParameters([Out] out DWMCOLORIZATIONPARAMS dwParameters);
    }

    /// <summary>
    /// Represents the current DWM color accent settings.
    /// </summary>
    /// <remarks><a href="https://github.com/lepoco/wpfui/blob/main/src/Wpf.Ui/Interop/Dwmapi.cs"/></remarks>
    [StructLayout(LayoutKind.Sequential)]
#pragma warning disable S101
    private struct DWMCOLORIZATIONPARAMS
#pragma warning restore S101
    {
        public uint ColorizationColor;
        public uint ColorizationAfterglow;
        public uint ColorizationColorBalance;
        public uint ColorizationAfterglowBalance;
        public uint ColorizationBlurBalance;
        public uint ColorizationGlassReflectionIntensity;
        public bool ColorizationOpaqueBlend;
    }

    /// <summary>
    /// Loads the collection of themes defined in the assembly.
    /// </summary>
    /// <returns>A collection of themes defined in the assembly.</returns>
    private static IList<Theme> GetBuiltInThemes()
    {
        return
        [
            // Light themes
            new(
                ThemeType.BuiltInLight,
                "accent" /* identifier */,
                Resources.ThemeManagerAccentLightTheme /* name */,
                "#FFFFFF" /* backgroundColor */,
                GetWindowsAccentColor() /* progressBarColor */,
                "#EEEEEE" /* progressBackgroundColor */,
                "#C75050" /* expirationFlashColor */,
                "#000000" /* mainTextColor */,
                "#808080" /* mainHintColor */,
                "#808080" /* secondaryTextColor */,
                "#808080" /* secondaryHintColor */,
                "#0066CC" /* buttonColor */,
                "#FF0000" /* buttonHoverColor */),
            new(
                ThemeType.BuiltInLight,
                "red" /* identifier */,
                Resources.ThemeManagerRedLightTheme /* name */,
                "#FFFFFF" /* backgroundColor */,
                "#C75050" /* progressBarColor */,
                "#EEEEEE" /* progressBackgroundColor */,
                "#C75050" /* expirationFlashColor */,
                "#000000" /* mainTextColor */,
                "#808080" /* mainHintColor */,
                "#808080" /* secondaryTextColor */,
                "#808080" /* secondaryHintColor */,
                "#0066CC" /* buttonColor */,
                "#FF0000" /* buttonHoverColor */),
            new(
                ThemeType.BuiltInLight,
                "orange" /* identifier */,
                Resources.ThemeManagerOrangeLightTheme /* name */,
                "#FFFFFF" /* backgroundColor */,
                "#FF7F50" /* progressBarColor */,
                "#EEEEEE" /* progressBackgroundColor */,
                "#C75050" /* expirationFlashColor */,
                "#000000" /* mainTextColor */,
                "#808080" /* mainHintColor */,
                "#808080" /* secondaryTextColor */,
                "#808080" /* secondaryHintColor */,
                "#0066CC" /* buttonColor */,
                "#FF0000" /* buttonHoverColor */),
            new(
                ThemeType.BuiltInLight,
                "yellow" /* identifier */,
                Resources.ThemeManagerYellowLightTheme /* name */,
                "#FFFFFF" /* backgroundColor */,
                "#FFC800" /* progressBarColor */,
                "#EEEEEE" /* progressBackgroundColor */,
                "#C75050" /* expirationFlashColor */,
                "#000000" /* mainTextColor */,
                "#808080" /* mainHintColor */,
                "#808080" /* secondaryTextColor */,
                "#808080" /* secondaryHintColor */,
                "#0066CC" /* buttonColor */,
                "#FF0000" /* buttonHoverColor */),
            new(
                ThemeType.BuiltInLight,
                "green" /* identifier */,
                Resources.ThemeManagerGreenLightTheme /* name */,
                "#FFFFFF" /* backgroundColor */,
                "#57A64A" /* progressBarColor */,
                "#EEEEEE" /* progressBackgroundColor */,
                "#C75050" /* expirationFlashColor */,
                "#000000" /* mainTextColor */,
                "#808080" /* mainHintColor */,
                "#808080" /* secondaryTextColor */,
                "#808080" /* secondaryHintColor */,
                "#0066CC" /* buttonColor */,
                "#FF0000" /* buttonHoverColor */),
            new(
                ThemeType.BuiltInLight,
                "blue" /* identifier */,
                Resources.ThemeManagerBlueLightTheme /* name */,
                "#FFFFFF" /* backgroundColor */,
                "#3665B3" /* progressBarColor */,
                "#EEEEEE" /* progressBackgroundColor */,
                "#C75050" /* expirationFlashColor */,
                "#000000" /* mainTextColor */,
                "#808080" /* mainHintColor */,
                "#808080" /* secondaryTextColor */,
                "#808080" /* secondaryHintColor */,
                "#0066CC" /* buttonColor */,
                "#FF0000" /* buttonHoverColor */),
            new(
                ThemeType.BuiltInLight,
                "purple" /* identifier */,
                Resources.ThemeManagerPurpleLightTheme /* name */,
                "#FFFFFF" /* backgroundColor */,
                "#843179" /* progressBarColor */,
                "#EEEEEE" /* progressBackgroundColor */,
                "#C75050" /* expirationFlashColor */,
                "#000000" /* mainTextColor */,
                "#808080" /* mainHintColor */,
                "#808080" /* secondaryTextColor */,
                "#808080" /* secondaryHintColor */,
                "#0066CC" /* buttonColor */,
                "#FF0000" /* buttonHoverColor */),
            new(
                ThemeType.BuiltInLight,
                "gray" /* identifier */,
                Resources.ThemeManagerGrayLightTheme /* name */,
                "#FFFFFF" /* backgroundColor */,
                "#666666" /* progressBarColor */,
                "#EEEEEE" /* progressBackgroundColor */,
                "#C75050" /* expirationFlashColor */,
                "#000000" /* mainTextColor */,
                "#808080" /* mainHintColor */,
                "#808080" /* secondaryTextColor */,
                "#808080" /* secondaryHintColor */,
                "#0066CC" /* buttonColor */,
                "#FF0000" /* buttonHoverColor */),
            new(
                ThemeType.BuiltInLight,
                "black" /* identifier */,
                Resources.ThemeManagerBlackLightTheme /* name */,
                "#FFFFFF" /* backgroundColor */,
                "#000000" /* progressBarColor */,
                "#EEEEEE" /* progressBackgroundColor */,
                "#C75050" /* expirationFlashColor */,
                "#000000" /* mainTextColor */,
                "#808080" /* mainHintColor */,
                "#808080" /* secondaryTextColor */,
                "#808080" /* secondaryHintColor */,
                "#0066CC" /* buttonColor */,
                "#FF0000" /* buttonHoverColor */),

            // Dark themes
            new(
                ThemeType.BuiltInDark,
                "accent-dark" /* identifier */,
                Resources.ThemeManagerAccentDarkTheme /* name */,
                "#1E1E1E" /* backgroundColor */,
                GetWindowsAccentColor() /* progressBarColor */,
                "#2D2D30" /* progressBackgroundColor */,
                "#C75050" /* expirationFlashColor */,
                "#808080" /* mainTextColor */,
                "#505050" /* mainHintColor */,
                "#505050" /* secondaryTextColor */,
                "#505050" /* secondaryHintColor */,
                "#0066CC" /* buttonColor */,
                "#FF0000" /* buttonHoverColor */),
            new(
                ThemeType.BuiltInDark,
                "red-dark" /* identifier */,
                Resources.ThemeManagerRedDarkTheme /* name */,
                "#1E1E1E" /* backgroundColor */,
                "#C75050" /* progressBarColor */,
                "#2D2D30" /* progressBackgroundColor */,
                "#C75050" /* expirationFlashColor */,
                "#808080" /* mainTextColor */,
                "#505050" /* mainHintColor */,
                "#505050" /* secondaryTextColor */,
                "#505050" /* secondaryHintColor */,
                "#0066CC" /* buttonColor */,
                "#FF0000" /* buttonHoverColor */),
            new(
                ThemeType.BuiltInDark,
                "orange-dark" /* identifier */,
                Resources.ThemeManagerOrangeDarkTheme /* name */,
                "#1E1E1E" /* backgroundColor */,
                "#FF7F50" /* progressBarColor */,
                "#2D2D30" /* progressBackgroundColor */,
                "#C75050" /* expirationFlashColor */,
                "#808080" /* mainTextColor */,
                "#505050" /* mainHintColor */,
                "#505050" /* secondaryTextColor */,
                "#505050" /* secondaryHintColor */,
                "#0066CC" /* buttonColor */,
                "#FF0000" /* buttonHoverColor */),
            new(
                ThemeType.BuiltInDark,
                "yellow-dark" /* identifier */,
                Resources.ThemeManagerYellowDarkTheme /* name */,
                "#1E1E1E" /* backgroundColor */,
                "#FFC800" /* progressBarColor */,
                "#2D2D30" /* progressBackgroundColor */,
                "#C75050" /* expirationFlashColor */,
                "#808080" /* mainTextColor */,
                "#505050" /* mainHintColor */,
                "#505050" /* secondaryTextColor */,
                "#505050" /* secondaryHintColor */,
                "#0066CC" /* buttonColor */,
                "#FF0000" /* buttonHoverColor */),
            new(
                ThemeType.BuiltInDark,
                "green-dark" /* identifier */,
                Resources.ThemeManagerGreenDarkTheme /* name */,
                "#1E1E1E" /* backgroundColor */,
                "#57A64A" /* progressBarColor */,
                "#2D2D30" /* progressBackgroundColor */,
                "#C75050" /* expirationFlashColor */,
                "#808080" /* mainTextColor */,
                "#505050" /* mainHintColor */,
                "#505050" /* secondaryTextColor */,
                "#505050" /* secondaryHintColor */,
                "#0066CC" /* buttonColor */,
                "#FF0000" /* buttonHoverColor */),
            new(
                ThemeType.BuiltInDark,
                "blue-dark" /* identifier */,
                Resources.ThemeManagerBlueDarkTheme /* name */,
                "#1E1E1E" /* backgroundColor */,
                "#3665B3" /* progressBarColor */,
                "#2D2D30" /* progressBackgroundColor */,
                "#C75050" /* expirationFlashColor */,
                "#808080" /* mainTextColor */,
                "#505050" /* mainHintColor */,
                "#505050" /* secondaryTextColor */,
                "#505050" /* secondaryHintColor */,
                "#0066CC" /* buttonColor */,
                "#FF0000" /* buttonHoverColor */),
            new(
                ThemeType.BuiltInDark,
                "purple-dark" /* identifier */,
                Resources.ThemeManagerPurpleDarkTheme /* name */,
                "#1E1E1E" /* backgroundColor */,
                "#843179" /* progressBarColor */,
                "#2D2D30" /* progressBackgroundColor */,
                "#C75050" /* expirationFlashColor */,
                "#808080" /* mainTextColor */,
                "#505050" /* mainHintColor */,
                "#505050" /* secondaryTextColor */,
                "#505050" /* secondaryHintColor */,
                "#0066CC" /* buttonColor */,
                "#FF0000" /* buttonHoverColor */),
            new(
                ThemeType.BuiltInDark,
                "gray-dark" /* identifier */,
                Resources.ThemeManagerGrayDarkTheme /* name */,
                "#1E1E1E" /* backgroundColor */,
                "#666666" /* progressBarColor */,
                "#2D2D30" /* progressBackgroundColor */,
                "#C75050" /* expirationFlashColor */,
                "#808080" /* mainTextColor */,
                "#505050" /* mainHintColor */,
                "#505050" /* secondaryTextColor */,
                "#505050" /* secondaryHintColor */,
                "#0066CC" /* buttonColor */,
                "#FF0000" /* buttonHoverColor */),
            new(
                ThemeType.BuiltInDark,
                "black-dark" /* identifier */,
                Resources.ThemeManagerBlackDarkTheme /* name */,
                "#1E1E1E" /* backgroundColor */,
                "#000000" /* progressBarColor */,
                "#2D2D30" /* progressBackgroundColor */,
                "#C75050" /* expirationFlashColor */,
                "#808080" /* mainTextColor */,
                "#505050" /* mainHintColor */,
                "#505050" /* secondaryTextColor */,
                "#505050" /* secondaryHintColor */,
                "#0066CC" /* buttonColor */,
                "#FF0000" /* buttonHoverColor */)
        ];
    }

    /// <summary>
    /// Loads the collection of themes defined by the user.
    /// </summary>
    /// <returns>A collection of sounds defined by the user.</returns>
    private static IEnumerable<Theme> GetUserProvidedThemes()
    {
        return Settings.Default.UserProvidedThemes;
    }
}