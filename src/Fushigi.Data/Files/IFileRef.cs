using System.Diagnostics.CodeAnalysis;

namespace Fushigi.Data.Files;

public interface IFileRef
{
    internal static abstract (string inProduction, string shipped) Suffix { get; }
    internal string ValidatedRefPath { get; set; }
}