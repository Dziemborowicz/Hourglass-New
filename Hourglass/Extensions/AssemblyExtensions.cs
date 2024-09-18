using System.IO;
using System.Reflection;

namespace Hourglass.Extensions;

internal static class AssemblyExtensions
{
    public static string GetExecutableDirectoryName()
    {
        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase) ?? ".";

        return path.StartsWith("file:")
            ? path.Remove(0, 6) // file:\
            : path;
    }
}

