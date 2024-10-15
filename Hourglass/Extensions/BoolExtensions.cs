using System.Windows;

namespace Hourglass.Extensions;

internal static class BoolExtensions
{
    public static Visibility ToVisibility(this bool visible) =>
        visible ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility ToVisibilityReversed(this bool visible) =>
        ToVisibility(!visible);
}
