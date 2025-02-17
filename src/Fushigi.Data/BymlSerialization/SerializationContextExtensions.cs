using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Fushigi.Data.BymlSerialization;

public static class SerializationContextExtensions
{
    [DebuggerHidden]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetObject<TObject>(this ISerializationContext ctx, ref TObject value, string key,
        bool optional = false)
        where TObject : SerializableBymlObject<TObject>, new()
    {
        ctx.Set(SerializableBymlObject<TObject>.Conversion, ref value, key, optional);
    }
}