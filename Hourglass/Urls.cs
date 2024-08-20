using System;

using Hourglass.Properties;

namespace Hourglass;

internal static class Urls
{
    public static readonly Uri FAQ      = new(Resources.FAQUrl);
    public static readonly Uri Usage    = new(Resources.UsageUrl);
    public static readonly Uri Readme   = new(Resources.ReadmeUrl);
    public static readonly Uri NewIssue = new(Resources.NewIssueUrl);
}
