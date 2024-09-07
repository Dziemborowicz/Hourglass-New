// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InterfaceScaler.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Windows;

using System;
using System.Windows.Controls;

using Extensions;
using Timing;

/// <summary>
/// Scales a <see cref="TimerWindow"/> to ensure that controls shrink and grow with the window size, and updates a
/// timer interval to ensure smooth animation of the progress bar.
/// </summary>
public sealed class InterfaceScaler
{
    /// <summary>
    /// The default margin for <see cref="Button"/> elements in the <see cref="_timerWindow"/>.
    /// </summary>
    private const double BaseButtonMargin = 7;

    /// <summary>
    /// The default margin for the <see cref="Grid"/> control containing the controls of the <see
    /// cref="_timerWindow"/> when the current environment and platform is Windows 8.1 or older.
    /// </summary>
    private const double BaseControlsGridMarginForWindows81AndOlder = 10;

    /// <summary>
    /// The default margin for the <see cref="Grid"/> control containing the controls of the <see
    /// cref="_timerWindow"/> when the current environment and platform is Windows 10 or newer.
    /// </summary>
    private const double BaseControlsGridMarginForWindows10AndNewer = 13;

    /// <summary>
    /// The default margin for the <see cref="StackPanel"/> control containing the controls of the <see
    /// cref="_timerWindow"/>.
    /// </summary>
    private const double BaseControlsPanelMargin = 20;

    /// <summary>
    /// The default font size used to render controls in the <see cref="_timerWindow"/>.
    /// </summary>
    private const double BaseFontSize = 12;

    /// <summary>
    /// The default font size of the larger text control on the <see cref="_timerWindow"/>.
    /// </summary>
    private const double BasePrimaryTextControlFontSize = 18;

    /// <summary>
    /// The default additional margin of the larger text control on the <see cref="_timerWindow"/>, which is used
    /// when the base scale factor exceeds 1.0.
    /// </summary>
    private const double BasePrimaryTextControlAdditionalMargin = 10;

    /// <summary>
    /// The default top margin of the larger text control on the <see cref="_timerWindow"/>.
    /// </summary>
    private const double BasePrimaryTextControlTopMargin = 1;

    /// <summary>
    /// The default bottom margin of the larger text control on the <see cref="_timerWindow"/>.
    /// </summary>
    private const double BasePrimaryTextControlBottomMargin = 2;

    /// <summary>
    /// The default border thickness for the <see cref="Border"/> control that visualize validation errors and
    /// expired timer state.
    /// </summary>
    private const double BaseBorderThickness = 1;

    /// <summary>
    /// The default margin for the <see cref="Border"/> control that visualize validation errors and expired timer
    /// state.
    /// </summary>
    private const double BaseBorderMargin = 15;

    /// <summary>
    /// The default width of a <see cref="_timerWindow"/>.
    /// </summary>
    public const double BaseWindowWidth = 355;

    /// <summary>
    /// The default height of a <see cref="_timerWindow"/>.
    /// </summary>
    public const double BaseWindowHeight = 160;

    /// <summary>
    /// The reduction factor that relates the base scale factor with the reduced scale factor.
    /// </summary>
    private const double ReductionFactor = 0.55;

    /// <summary>
    /// A <see cref="TimerWindow"/>.
    /// </summary>
    private TimerWindow _timerWindow = null!;

    /// <summary>
    /// The <see cref="Grid"/> control that contains the <see cref="_controlsPanel"/>.
    /// </summary>
    private Grid _innerGrid = null!;

    /// <summary>
    /// The <see cref="StackPanel"/> control that contains the controls of the <see cref="_timerWindow"/>.
    /// </summary>
    private StackPanel _controlsPanel = null!;

    /// <summary>
    /// The larger <see cref="TextBox"/> on the <see cref="_timerWindow"/>.
    /// </summary>
    private SizeToFitTextBox _timerTextBox = null!;

    /// <summary>
    /// The smaller <see cref="TextBox"/> on the <see cref="_timerWindow"/>.
    /// </summary>
    private SizeToFitTextBox _titleTextBox = null!;

    /// <summary>
    /// The <see cref="Border"/> that animates to notify the user that the timer has expired or that the input was
    /// invalid.
    /// </summary>
    private Border _innerNotificationBorder = null!;

    /// <summary>
    /// An array of the <see cref="Button"/> elements on the <see cref="_timerWindow"/>.
    /// </summary>
    private Button[] _buttons = null!;

    /// <summary>
    /// The <see cref="Label"/> that contains the time elapsed since the timer expired when the timer has expired.
    /// </summary>
    private Label _timeExpiredLabel = null!;

    /// <summary>
    /// Binds the <see cref="InterfaceScaler"/> to a <see cref="TimerWindow"/>.
    /// </summary>
    /// <param name="window">A <see cref="TimerWindow"/>.</param>
    /// <exception cref="InvalidOperationException">The timer window is already created.</exception>
    public void Bind(TimerWindow window)
    {
        // Validate state
        if (_timerWindow is not null)
        {
            throw new InvalidOperationException(@"The timer window is already created");
        }

        // Initialize members
        _timerWindow = window ?? throw new ArgumentNullException(nameof(window));

        _innerGrid = _timerWindow.InnerGrid;
        _controlsPanel = _timerWindow.ControlsPanel;
        _timerTextBox = _timerWindow.TimerTextBox;
        _titleTextBox = _timerWindow.TitleTextBox;
        _innerNotificationBorder = _timerWindow.InnerNotificationBorder;
        _buttons =
        [
            _timerWindow.StartButton,
            _timerWindow.PauseButton,
            _timerWindow.ResumeButton,
            _timerWindow.StopButton,
            _timerWindow.RestartButton,
            _timerWindow.CloseButton,
            _timerWindow.CancelButton,
            _timerWindow.UpdateButton
        ];
        _timeExpiredLabel = _timerWindow.TimeExpiredLabel;

        // Hook up events
        _timerWindow.Loaded += ScaleEventHandler;
        _timerWindow.SizeChanged += ScaleEventHandler;
        _timerWindow.PropertyChanged += ScaleEventHandler;
        _timerTextBox.TextChanged += ScaleEventHandler;

        void ScaleEventHandler(object sender, EventArgs e) =>
            Scale();
    }

    /// <summary>
    /// Scales the <see cref="TimerWindow"/> to ensure that controls shrink and grow with the window size, and
    /// updates the timer interval to ensure smooth animation of the progress bar.
    /// </summary>
    public void Scale()
    {
        ScaleControls();
        ScaleTimerInterval();
    }

    /// <summary>
    /// Scales the <see cref="_timerWindow"/> and its controls.
    /// </summary>
    private void ScaleControls()
    {
        if (!_timerWindow.IsVisible)
        {
            return;
        }

        double baseScaleFactor = GetBaseScaleFactor();
        double reducedScaleFactor = GetReducedScaleFactor(baseScaleFactor, ReductionFactor);
        ScaleControls(baseScaleFactor, reducedScaleFactor);
    }

    /// <summary>
    /// Scales the <see cref="_timerWindow"/> and its controls using the specified factors.
    /// </summary>
    /// <param name="baseScaleFactor">The base scale factor.</param>
    /// <param name="reducedScaleFactor">The reduced scale factor.</param>
    /// <seealso cref="GetBaseScaleFactor"/>
    /// <seealso cref="GetReducedScaleFactor"/>
    private void ScaleControls(double baseScaleFactor, double reducedScaleFactor)
    {
        double baseControlsGridMargin = EnvironmentExtensions.IsWindows10OrNewer
            ? BaseControlsGridMarginForWindows10AndNewer
            : BaseControlsGridMarginForWindows81AndOlder;
        _innerGrid.Margin = new(Math.Max(reducedScaleFactor * baseControlsGridMargin, (_innerGrid.GetMinTrackHeight() + 1) / 2));

        _controlsPanel.Margin = new(
            left: reducedScaleFactor * BaseControlsPanelMargin,
            top: 0.0,
            right: reducedScaleFactor * BaseControlsPanelMargin,
            bottom: 0.0);

        _timerTextBox.MaxFontSize = baseScaleFactor * BasePrimaryTextControlFontSize;
        _timerTextBox.Margin = new(
            left: 0.0,
            top: baseScaleFactor * BasePrimaryTextControlTopMargin + (baseScaleFactor - 1.0) * BasePrimaryTextControlAdditionalMargin,
            right: 0.0,
            bottom: baseScaleFactor * BasePrimaryTextControlBottomMargin + (baseScaleFactor - 1.0) * BasePrimaryTextControlAdditionalMargin);

        _titleTextBox.MaxFontSize = reducedScaleFactor * BaseFontSize;

        _innerNotificationBorder.BorderThickness = new(reducedScaleFactor * BaseBorderThickness);
        _innerNotificationBorder.Margin = new(reducedScaleFactor * BaseBorderMargin);

        foreach (Button button in _buttons)
        {
            button.FontSize = reducedScaleFactor * BaseFontSize;
            button.Margin = new(
                left: baseScaleFactor * BaseButtonMargin,
                top: 0.0,
                right: baseScaleFactor * BaseButtonMargin,
                bottom: 0.0);
        }

        _timeExpiredLabel.FontSize = reducedScaleFactor * BaseFontSize;
    }

    /// <summary>
    /// Returns the base scale factor for the user interface based on the width and height of the <see
    /// cref="_timerWindow"/>.
    /// </summary>
    /// <returns>The base scale factor.</returns>
    private double GetBaseScaleFactor()
    {
        double widthFactor = Math.Max(_timerWindow.ActualWidth / BaseWindowWidth, 1.0);
        double heightFactor = Math.Max(_timerWindow.ActualHeight / BaseWindowHeight, 1.0);
        return Math.Min(widthFactor, heightFactor);
    }

    /// <summary>
    /// Returns the reduced scale factor. The reduced scale factor is computed by reducing the portion of the scale
    /// factor that exceeds 1.0 by the reduction factor. If the base scale factor is less than or equal to 1.0, the
    /// reduced scale factor is 1.0.
    /// </summary>
    /// <param name="baseScaleFactor">The base scale factor.</param>
    /// <param name="reductionFactor">The reduction factor.</param>
    /// <returns>The reduced scale factor.</returns>
    private static double GetReducedScaleFactor(double baseScaleFactor, double reductionFactor)
    {
        double difference = baseScaleFactor - 1.0;
        return 1.0 + difference * reductionFactor;
    }

    /// <summary>
    /// Updates the timer interval to ensure smooth animation of the progress bar.
    /// </summary>
    private void ScaleTimerInterval()
    {
        if (!_timerWindow.IsVisible)
        {
            return;
        }

        Timer timer = _timerWindow.Timer;

        if (timer.TotalTime.HasValue)
        {
            double interval = timer.TotalTime.Value.TotalMilliseconds / _timerWindow.ActualWidth / 2.0;
            interval = MathExtensions.LimitToRange(interval, 10.0, TimerBase.DefaultInterval.TotalMilliseconds);
            timer.Interval = TimeSpan.FromMilliseconds(interval);
        }
        else
        {
            timer.Interval = TimerBase.DefaultInterval;
        }
    }
}