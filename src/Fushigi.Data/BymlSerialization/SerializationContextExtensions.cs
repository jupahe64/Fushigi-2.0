using System.Diagnostics;
using System.Runtime.CompilerServices;
using Fushigi.Data.Files.GymlTypes;

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

    public static void Set<TValue>(this ISerializationContext ctx,
        BymlConversion<TValue> conversion, ref INHERITED<TValue> value, string key,
        bool optional = false)
    {
        if (ctx is Deserializer deserializer)
        {
            value.IsPresent = deserializer.GetNode().GetMap().ContainsKey(key);
        }
        else if (ctx is Serializer) //should always be true
        {
            if (!value.IsPresent)
                return;
        }
            
        ctx.Set(conversion, ref value.Value, key, optional);
    }

    public static void Set<TValue>(this ISerializationContext ctx,
        BymlConversion<TValue> conversion, ref INHERITED<TValue?> value, string key,
        bool optional = false)
        where TValue : struct
    {
        if (ctx is Deserializer deserializer)
        {
            value.IsPresent = deserializer.GetNode().GetMap().ContainsKey(key);
        }
        else if (ctx is Serializer) //should always be true
        {
            if (!value.IsPresent)
                return;
        }
            
        ctx.Set(conversion, ref value.Value, key, optional);
    }
    
    public static void SetArray<TValue>(this ISerializationContext ctx,
        ref INHERITED<List<TValue>> value, string key, BymlConversion<TValue> conversion,
        bool optional = false)
    {
        if (ctx is Deserializer deserializer)
        {
            value.IsPresent = deserializer.GetNode().GetMap().ContainsKey(key);
        }
        else if (ctx is Serializer) //should always be true
        {
            if (!value.IsPresent)
                return;
        }
            
        ctx.SetArray(ref value.Value!, key, conversion, optional);
    }
    
    public static void SetMap<TValue>(this ISerializationContext ctx,
        ref INHERITED<Dictionary<string, TValue>?> value, string key, BymlConversion<TValue> conversion,
        bool optional = false)
    {
        if (ctx is Deserializer deserializer)
        {
            value.IsPresent = deserializer.GetNode().GetMap().ContainsKey(key);
        }
        else if (ctx is Serializer) //should always be true
        {
            if (!value.IsPresent)
                return;
        }
            
        ctx.SetMap(ref value.Value!, key, conversion, optional);
    }
    
    public static void SetObject<TObject>(this ISerializationContext ctx, ref INHERITED<TObject> value, string key,
        bool optional = false)
        where TObject : SerializableBymlObject<TObject>, new()
    {
        if (ctx is Deserializer deserializer)
        {
            value.IsPresent = deserializer.GetNode().GetMap().ContainsKey(key);
        }
        else if (ctx is Serializer) //should always be true
        {
            if (!value.IsPresent)
                return;
        }
        
        ctx.Set(SerializableBymlObject<TObject>.Conversion, ref value.Value, key, optional);
    }
}