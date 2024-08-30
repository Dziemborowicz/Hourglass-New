// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Theme.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Timing;

using System.ComponentModel;
using System.Linq;
using System.Windows.Media;

using Extensions;
using Managers;
using Serialization;

#pragma warning disable IDE0290

/// <summary>
/// The type of theme.
/// </summary>
public enum ThemeType
{
    /// <summary>
    /// A built-in theme with a light background.
    /// </summary>
    BuiltInLight,

    /// <summary>
    /// A built-in theme with a dark background.
    /// </summary>
    BuiltInDark,

    /// <summary>
    /// A theme that is provided by the user.
    /// </summary>
    UserProvided
}

/// <summary>
/// A theme for the timer window.
/// </summary>
public sealed class Theme : INotifyPropertyChanged
{
    #region Private Members

    /// <summary>
    /// The friendly name for this theme, or <c>null</c> if no friendly name is specified.
    /// </summary>
    private string _name;

    /// <summary>
    /// The background color of the window.
    /// </summary>
    private Color _backgroundColor;

    /// <summary>
    /// The brush used to paint the background color of the window.
    /// </summary>
    private Brush? _backgroundBrush;

    /// <summary>
    /// The color of the progress bar.
    /// </summary>
    private Color _progressBarColor;

    /// <summary>
    /// The brush used to paint the color of the progress bar.
    /// </summary>
    private Brush? _progressBarBrush;

    /// <summary>
    /// The background color of the progress bar.
    /// </summary>
    private Color _progressBackgroundColor;

    /// <summary>
    /// The brush used to paint the background color of the progress bar.
    /// </summary>
    private Brush? _progressBackgroundBrush;

    /// <summary>
    /// The color that is flashed on expiration.
    /// </summary>
    private Color _expirationFlashColor;

    /// <summary>
    /// The brush used to paint the color that is flashed on expiration.
    /// </summary>
    private Brush? _expirationFlashBrush;

    /// <summary>
    /// The color of the primary text.
    /// </summary>
    private Color _primaryTextColor;

    /// <summary>
    /// The brush used to paint the color of the primary text.
    /// </summary>
    private Brush? _primaryTextBrush;

    /// <summary>
    /// The color of the watermark in the primary text box.
    /// </summary>
    private Color _primaryHintColor;

    /// <summary>
    /// The brush used to paint the color of the watermark in the primary text box.
    /// </summary>
    private Brush? _primaryHintBrush;

    /// <summary>
    /// The color of any secondary text.
    /// </summary>
    private Color _secondaryTextColor;

    /// <summary>
    /// The brush used to paint the color of any secondary text.
    /// </summary>
    private Brush? _secondaryTextBrush;

    /// <summary>
    /// The color of the watermark in any secondary text box.
    /// </summary>
    private Color _secondaryHintColor;

    /// <summary>
    /// The brush used to paint the color of the watermark in any secondary text box.
    /// </summary>
    private Brush? _secondaryHintBrush;

    /// <summary>
    /// The color of the button text.
    /// </summary>
    private Color _buttonColor;

    /// <summary>
    /// The brush used to paint the color of the button text.
    /// </summary>
    private Brush? _buttonBrush;

    /// <summary>
    /// The color of the button text when the user hovers over the button.
    /// </summary>
    private Color _buttonHoverColor;

    /// <summary>
    /// The brush used to paint the color of the button text when the user hovers over the button.
    /// </summary>
    private Brush? _buttonHoverBrush;

    /// <summary>
    /// Is the user theme the dark one.
    /// </summary>
    private bool _isUserThemeDark;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Theme"/> class.
    /// </summary>
    /// <param name="type">The type of this theme.</param>
    /// <param name="identifier">A unique identifier for this theme.</param>
    /// <param name="name">The friendly name for this theme, or <c>null</c> if no friendly name is specified.</param>
    /// <param name="backgroundColor">The background color of the window.</param>
    /// <param name="progressBarColor">The color of the progress bar.</param>
    /// <param name="progressBackgroundColor">The background color of the progress bar.</param>
    /// <param name="expirationFlashColor">The color that is flashed on expiration.</param>
    /// <param name="primaryTextColor">The color of the primary text.</param>
    /// <param name="primaryHintColor">The color of the watermark in the primary text box.</param>
    /// <param name="secondaryTextColor">The color of any secondary text.</param>
    /// <param name="secondaryHintColor">The color of the watermark in any secondary text box.</param>
    /// <param name="buttonColor">The color of the button text.</param>
    /// <param name="buttonHoverColor">The color of the button text when the user hovers over the button.</param>
    /// <param name="isUserThemeDark">Is the user theme the dark one.</param>
    public Theme(
        ThemeType type,
        string identifier,
        string name,
        Color backgroundColor,
        Color progressBarColor,
        Color progressBackgroundColor,
        Color expirationFlashColor,
        Color primaryTextColor,
        Color primaryHintColor,
        Color secondaryTextColor,
        Color secondaryHintColor,
        Color buttonColor,
        Color buttonHoverColor,
        bool isUserThemeDark = false)
    {
        Type = type;
        Identifier = identifier;
        _name = name;

        _backgroundColor = backgroundColor;
        _progressBarColor = progressBarColor;
        _progressBackgroundColor = progressBackgroundColor;
        _expirationFlashColor = expirationFlashColor;
        _primaryTextColor = primaryTextColor;
        _primaryHintColor = primaryHintColor;
        _secondaryTextColor = secondaryTextColor;
        _secondaryHintColor = secondaryHintColor;
        _buttonColor = buttonColor;
        _buttonHoverColor = buttonHoverColor;
        _isUserThemeDark = isUserThemeDark;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Theme"/> class.
    /// </summary>
    /// <param name="type">The type of this theme.</param>
    /// <param name="identifier">A unique identifier for this theme.</param>
    /// <param name="name">The friendly name for this theme, or <c>null</c> if no friendly name is specified.</param>
    /// <param name="backgroundColor">The background color of the window.</param>
    /// <param name="progressBarColor">The color of the progress bar.</param>
    /// <param name="progressBackgroundColor">The background color of the progress bar.</param>
    /// <param name="expirationFlashColor">The color that is flashed on expiration.</param>
    /// <param name="primaryTextColor">The color of the primary text.</param>
    /// <param name="primaryHintColor">The color of the watermark in the primary text box.</param>
    /// <param name="secondaryTextColor">The color of any secondary text.</param>
    /// <param name="secondaryHintColor">The color of the watermark in any secondary text box.</param>
    /// <param name="buttonColor">The color of the button text.</param>
    /// <param name="buttonHoverColor">The color of the button text when the user hovers over the button.</param>
    public Theme(
        ThemeType type,
        string identifier,
        string name,
        string backgroundColor,
        string progressBarColor,
        string progressBackgroundColor,
        string expirationFlashColor,
        string primaryTextColor,
        string primaryHintColor,
        string secondaryTextColor,
        string secondaryHintColor,
        string buttonColor,
        string buttonHoverColor)
        : this(
            type,
            identifier,
            name,
            ColorExtensions.FromString(backgroundColor),
            ColorExtensions.FromString(progressBarColor),
            ColorExtensions.FromString(progressBackgroundColor),
            ColorExtensions.FromString(expirationFlashColor),
            ColorExtensions.FromString(primaryTextColor),
            ColorExtensions.FromString(primaryHintColor),
            ColorExtensions.FromString(secondaryTextColor),
            ColorExtensions.FromString(secondaryHintColor),
            ColorExtensions.FromString(buttonColor),
            ColorExtensions.FromString(buttonHoverColor))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Theme"/> class.
    /// </summary>
    /// <param name="type">The type of this theme.</param>
    /// <param name="identifier">A unique identifier for this theme.</param>
    /// <param name="name">The friendly name for this theme, or <c>null</c> if no friendly name is specified.</param>
    /// <param name="theme">A theme from which to copy colors.</param>
    public Theme(ThemeType type, string identifier, string name, Theme theme)
        : this(
            type,
            identifier,
            name,
            theme._backgroundColor,
            theme._progressBarColor,
            theme._progressBackgroundColor,
            theme._expirationFlashColor,
            theme._primaryTextColor,
            theme._primaryHintColor,
            theme._secondaryTextColor,
            theme._secondaryHintColor,
            theme._buttonColor,
            theme._buttonHoverColor,
            theme.Type == ThemeType.BuiltInDark || theme._isUserThemeDark)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Theme"/> class from a <see cref="ThemeInfo"/>.
    /// </summary>
    /// <param name="info">A <see cref="ThemeInfo"/>.</param>
    public Theme(ThemeInfo info)
        : this(
            ThemeType.UserProvided,
            info.Identifier,
            info.Name,
            info.BackgroundColor,
            info.ProgressBarColor,
            info.ProgressBackgroundColor,
            info.ExpirationFlashColor,
            info.PrimaryTextColor,
            info.PrimaryHintColor,
            info.SecondaryTextColor,
            info.SecondaryHintColor,
            info.ButtonColor,
            info.ButtonHoverColor,
            info.IsUserThemeDark)
    {
    }

    #endregion

    /// <summary>
    /// Raised when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    #region Properties

    /// <summary>
    /// Gets the default theme.
    /// </summary>
    public static Theme DefaultTheme => ThemeManager.Instance.DefaultTheme;

    /// <summary>
    /// Gets the type of this theme.
    /// </summary>
    public ThemeType Type { get; }

    /// <summary>
    /// Gets the unique identifier for this theme.
    /// </summary>
    public string Identifier { get; }

    /// <summary>
    /// Gets or sets the friendly name of this theme, or <c>null</c> if no friendly name is specified.
    /// </summary>
    public string Name
    {
        get => _name;

        set
        {
            if (_name == value)
            {
                return;
            }

            _name = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets the background color of the window.
    /// </summary>
    public Color BackgroundColor
    {
        get => _backgroundColor;

        set
        {
            if (_backgroundColor == value)
            {
                return;
            }

            _backgroundColor = value;
            _backgroundBrush = null;
            PropertyChanged.Notify(this);
            PropertyChanged.Notify(this, nameof(BackgroundBrush));
        }
    }

    /// <summary>
    /// Gets the brush used to paint the background color of the window.
    /// </summary>
    public Brush BackgroundBrush => _backgroundBrush ??= new SolidColorBrush(_backgroundColor);

    /// <summary>
    /// Gets or sets the color of the progress bar.
    /// </summary>
    public Color ProgressBarColor
    {
        get => _progressBarColor;

        set
        {
            if (_progressBarColor == value)
            {
                return;
            }

            _progressBarColor = value;
            _progressBarBrush = null;
            PropertyChanged.Notify(this);
            PropertyChanged.Notify(this, nameof(ProgressBarBrush));
        }
    }

    /// <summary>
    /// Gets the brush used to paint the color of the progress bar.
    /// </summary>
    public Brush ProgressBarBrush => _progressBarBrush ??= new SolidColorBrush(_progressBarColor);

    /// <summary>
    /// Gets or sets the background color of the progress bar.
    /// </summary>
    public Color ProgressBackgroundColor
    {
        get => _progressBackgroundColor;

        set
        {
            if (_progressBackgroundColor == value)
            {
                return;
            }

            _progressBackgroundColor = value;
            _progressBackgroundBrush = null;
            PropertyChanged.Notify(this);
            PropertyChanged.Notify(this, nameof(ProgressBackgroundBrush));
        }
    }

    /// <summary>
    /// Gets the brush used to paint the background color of the progress bar.
    /// </summary>
    public Brush ProgressBackgroundBrush => _progressBackgroundBrush ??= new SolidColorBrush(_progressBackgroundColor);

    /// <summary>
    /// Gets or sets the color that is flashed on expiration.
    /// </summary>
    public Color ExpirationFlashColor
    {
        get => _expirationFlashColor;

        set
        {
            if (_expirationFlashColor == value)
            {
                return;
            }

            _expirationFlashColor = value;
            _expirationFlashBrush = null;
            PropertyChanged.Notify(this);
            PropertyChanged.Notify(this, nameof(ExpirationFlashBrush));
        }
    }

    /// <summary>
    /// Gets the brush used to paint the color that is flashed on expiration.
    /// </summary>
    public Brush ExpirationFlashBrush => _expirationFlashBrush ??= new SolidColorBrush(_expirationFlashColor);

    /// <summary>
    /// Gets or sets the color of the primary text.
    /// </summary>
    public Color PrimaryTextColor
    {
        get => _primaryTextColor;

        set
        {
            if (_primaryTextColor == value)
            {
                return;
            }

            _primaryTextColor = value;
            _primaryTextBrush = null;
            PropertyChanged.Notify(this);
            PropertyChanged.Notify(this, nameof(PrimaryTextBrush));
        }
    }

    /// <summary>
    /// Gets the brush used to paint the color of the primary text.
    /// </summary>
    public Brush PrimaryTextBrush => _primaryTextBrush ??= new SolidColorBrush(_primaryTextColor);

    /// <summary>
    /// Gets or sets the color of the watermark in the primary text box.
    /// </summary>
    public Color PrimaryHintColor
    {
        get => _primaryHintColor;

        set
        {
            if (_primaryHintColor == value)
            {
                return;
            }

            _primaryHintColor = value;
            _primaryHintBrush = null;
            PropertyChanged.Notify(this);
            PropertyChanged.Notify(this, nameof(PrimaryHintBrush));
        }
    }

    /// <summary>
    /// Gets the brush used to paint the color of the watermark in the primary text box.
    /// </summary>
    public Brush PrimaryHintBrush => _primaryHintBrush ??= new SolidColorBrush(_primaryHintColor);

    /// <summary>
    /// Gets or sets the color of any secondary text.
    /// </summary>
    public Color SecondaryTextColor
    {
        get => _secondaryTextColor;

        set
        {
            if (_secondaryTextColor == value)
            {
                return;
            }

            _secondaryTextColor = value;
            _secondaryTextBrush = null;
            PropertyChanged.Notify(this);
            PropertyChanged.Notify(this, nameof(SecondaryTextBrush));
        }
    }

    /// <summary>
    /// Gets the brush used to paint the color of any secondary text.
    /// </summary>
    public Brush SecondaryTextBrush => _secondaryTextBrush ??= new SolidColorBrush(_secondaryTextColor);

    /// <summary>
    /// Gets or sets the color of the watermark in any secondary text box.
    /// </summary>
    public Color SecondaryHintColor
    {
        get => _secondaryHintColor;

        set
        {
            if (_secondaryHintColor == value)
            {
                return;
            }

            _secondaryHintColor = value;
            _secondaryHintBrush = null;
            PropertyChanged.Notify(this);
            PropertyChanged.Notify(this, nameof(SecondaryHintBrush));
        }
    }

    /// <summary>
    /// Gets the brush used to paint the color of the watermark in any secondary text box.
    /// </summary>
    public Brush SecondaryHintBrush => _secondaryHintBrush ??= new SolidColorBrush(_secondaryHintColor);

    /// <summary>
    /// Gets or sets the color of the button text.
    /// </summary>
    public Color ButtonColor
    {
        get => _buttonColor;

        set
        {
            if (_buttonColor == value)
            {
                return;
            }

            _buttonColor = value;
            _buttonBrush = null;
            PropertyChanged.Notify(this);
            PropertyChanged.Notify(this, nameof(ButtonBrush));
        }
    }

    /// <summary>
    /// Gets the brush used to paint the color of the button text.
    /// </summary>
    public Brush ButtonBrush => _buttonBrush ??= new SolidColorBrush(_buttonColor);

    /// <summary>
    /// Gets or sets the color of the button text when the user hovers over the button.
    /// </summary>
    public Color ButtonHoverColor
    {
        get => _buttonHoverColor;

        set
        {
            if (_buttonHoverColor == value)
            {
                return;
            }

            _buttonHoverColor = value;
            _buttonHoverBrush = null;
            PropertyChanged.Notify(this);
            PropertyChanged.Notify(this, nameof(ButtonHoverBrush));
        }
    }

    /// <summary>
    /// Gets or sets whether the user theme is the dark one.
    /// </summary>
    public bool IsUserThemeDark
    {
        get => _isUserThemeDark;

        set
        {
            if (_isUserThemeDark == value)
            {
                return;
            }

            _isUserThemeDark = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets the brush used to paint the color of the button text when the user hovers over the button.
    /// </summary>
    public Brush ButtonHoverBrush => _buttonHoverBrush ??= new SolidColorBrush(_buttonHoverColor);

    /// <summary>
    /// Gets the light variant of this theme.
    /// </summary>
    public Theme LightVariant => ThemeManager.Instance.GetLightVariantForTheme(this);

    /// <summary>
    /// Gets the dark variant of this theme.
    /// </summary>
    public Theme DarkVariant => ThemeManager.Instance.GetDarkVariantForTheme(this);

    #endregion

    #region Public Static Methods

    /// <summary>
    /// Returns the theme for the specified identifier, or <c>null</c> if no such theme is loaded.
    /// </summary>
    /// <param name="identifier">The identifier for the theme.</param>
    /// <returns>The theme for the specified identifier, or <c>null</c> if no such theme is loaded.</returns>
    public static Theme FromIdentifier(string? identifier)
    {
        return ThemeManager.Instance.GetThemeOrDefaultByIdentifier(identifier);
    }

    /// <summary>
    /// Returns a <see cref="Theme"/> that is a copy of another <see cref="Theme"/>.
    /// </summary>
    /// <param name="type">The type of this theme.</param>
    /// <param name="identifier">A unique identifier for this theme.</param>
    /// <param name="name">The friendly name for this theme, or <c>null</c> if no friendly name is specified.</param>
    /// <param name="theme">A theme from which to copy colors.</param>
    /// <returns>A <see cref="Theme"/> that is a copy of another <see cref="Theme"/>.</returns>
    public static Theme FromTheme(ThemeType type, string identifier, string name, Theme theme)
    {
        return new(type, identifier, name, theme);
    }

    /// <summary>
    /// Returns a <see cref="Theme"/> for the specified <see cref="ThemeInfo"/>, or <c>null</c> if the specified
    /// <see cref="ThemeInfo"/> is <c>null</c>.
    /// </summary>
    /// <param name="info">A <see cref="ThemeInfo"/>.</param>
    /// <returns>A <see cref="Theme"/> for the specified <see cref="ThemeInfo"/>, or <c>null</c> if the specified
    /// <see cref="ThemeInfo"/> is <c>null</c>.</returns>
    public static Theme? FromThemeInfo(ThemeInfo? info)
    {
        return info is not null ? new Theme(info) : null;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Returns the unique colors used in this theme.
    /// </summary>
    /// <returns>The unique colors used in this theme.</returns>
    public Color[] GetPalette()
    {
        Color[] allColors =
        [
            ProgressBarColor,
            ProgressBackgroundColor,
            BackgroundColor,
            ExpirationFlashColor,
            PrimaryTextColor,
            PrimaryHintColor,
            SecondaryTextColor,
            SecondaryHintColor,
            ButtonColor,
            ButtonHoverColor
        ];

        return allColors.Distinct().ToArray();
    }

    /// <summary>
    /// Sets all the properties, except for <see cref="Type"/> and <see cref="Identifier"/>, from another
    /// instance of the <see cref="Theme"/> class.
    /// </summary>
    /// <param name="theme">Another instance of the <see cref="Theme"/> class.</param>
    public void Set(Theme theme)
    {
        Name = theme.Name;
        BackgroundColor = theme.BackgroundColor;
        ProgressBarColor = theme.ProgressBarColor;
        ProgressBackgroundColor = theme.ProgressBackgroundColor;
        ExpirationFlashColor = theme.ExpirationFlashColor;
        PrimaryTextColor = theme.PrimaryTextColor;
        PrimaryHintColor = theme.PrimaryHintColor;
        SecondaryTextColor = theme.SecondaryTextColor;
        SecondaryHintColor = theme.SecondaryHintColor;
        ButtonColor = theme.ButtonColor;
        ButtonHoverColor = theme.ButtonHoverColor;
        IsUserThemeDark = theme.IsUserThemeDark;
    }

    /// <summary>
    /// Returns the representation of the <see cref="Theme"/> used for XML serialization.
    /// </summary>
    /// <returns>The representation of the <see cref="Theme"/> used for XML serialization.</returns>
    public ThemeInfo ToThemeInfo()
    {
        return new()
        {
            Identifier = Identifier,
            Name = _name,
            BackgroundColor = _backgroundColor,
            ProgressBarColor = _progressBarColor,
            ProgressBackgroundColor = _progressBackgroundColor,
            ExpirationFlashColor = _expirationFlashColor,
            PrimaryTextColor = _primaryTextColor,
            PrimaryHintColor = _primaryHintColor,
            SecondaryTextColor = _secondaryTextColor,
            SecondaryHintColor = _secondaryHintColor,
            ButtonColor = _buttonColor,
            ButtonHoverColor = _buttonHoverColor,
            IsUserThemeDark = _isUserThemeDark
        };
    }

    #endregion
}