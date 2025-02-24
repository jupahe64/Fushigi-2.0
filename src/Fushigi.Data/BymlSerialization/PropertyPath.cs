using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Fushigi.Data.BymlSerialization;

public readonly struct PropertyPath(ImmutableStack<string> path) : IEquatable<PropertyPath>
{
    private readonly ImmutableStack<string> _path = path;
    private readonly int _computedHashCode = ComputeHash(path);

    public override bool Equals([NotNullWhen(true)] object? obj) 
        => obj is PropertyPath other && Equals(other);

    public override int GetHashCode() => _computedHashCode;

    public bool Equals(PropertyPath other)
    {
        var enumerator1 = _path.GetEnumerator();
        var enumerator2 = other._path.GetEnumerator();
        
        //break if either has finished
        while (enumerator1.MoveNext() && enumerator2.MoveNext())
        {
            if (!enumerator1.Current.Equals(enumerator2.Current))
                return false;
        }
        
        //only return true if BOTH finished
        return !enumerator1.MoveNext() && !enumerator2.MoveNext(); 
    }
    
    private static int ComputeHash(ImmutableStack<string> path)
    {
        int length = path.Sum(segment => segment.Length);
        Span<char> span = stackalloc char[length];
        var idx = 0;
        foreach (string segment in path)
        {
            segment.AsSpan().CopyTo(span[idx..]);
            idx+=segment.Length;
        }
        return string.GetHashCode(span);
    }
}