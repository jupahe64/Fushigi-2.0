﻿using System.Collections.Immutable;
using BymlLibrary;
using BymlLibrary.Nodes.Containers;
using Fushigi.Data.Files;

namespace Fushigi.Data.BymlSerialization;

public record UnexpectedRootTypeErrorInfo(BymlNodeType ExpectedNodeType, BymlNodeType ActualNodeType, 
    RomFS.RetrievedFileLocation FileLocation);

public record ContentErrorsFoundErrorInfo(IReadOnlyDictionary<Byml, BymlContentErrorInfo> ContentErrors, 
    RomFS.RetrievedFileLocation FileLocation);

[Flags]
public enum ContentErrorFlags
{
    None = 0,
    UnexpectedType = 1 << 0,
    MissingKeys = 1 << 1,
    MissingElements = 1 << 2,
    UnexpectedEnumValue = 1 << 3,
    InvalidRefPath = 1 << 4,
}
public record struct BymlContentErrorInfo(ContentErrorFlags Flags, BymlNodeType? ExpectedNodeType, 
    List<string>? MissingRequiredKeys, int? MinimumElementCount, 
    FileRefConversion.FileRefPathFormat? ExpectedFileRefPathFormat);

public interface IBymlDeserializeErrorHandler
{
    Task OnUnexpectedRootType(UnexpectedRootTypeErrorInfo info);
    Task OnContentErrorsFound(ContentErrorsFoundErrorInfo info);
}

public struct Deserializer : ISerializationContext
{
    /// <summary>
    /// If set to true this Deserializer instance and all it's subsequent instances will
    /// report missing keys and indices even if they are excluded from the standard error reporting
    /// </summary>
    public bool ReportAllMissingKeysAndIndices;
    public static async Task<(bool success, T value)> Deserialize<T>(Byml byml, Func<Deserializer, T> deserializeFunc,
        IBymlDeserializeErrorHandler errorHandler, RomFS.RetrievedFileLocation fileLocationInfo, 
        IReadOnlySet<PropertyPath>? ignoreMissingProperties = null)
    {
        if (byml.Value is not BymlMap)
        {
            await errorHandler.OnUnexpectedRootType(
                new UnexpectedRootTypeErrorInfo(BymlNodeType.Map, byml.Type, fileLocationInfo));
            return (false, default!);
        }
        
        var contentErrors = new Dictionary<Byml, BymlContentErrorInfo>();
        var deserializer = new Deserializer(byml, ImmutableStack<string>.Empty,
            contentErrors, ignoreMissingProperties);

        var deserializedObject = deserializeFunc(deserializer);

        if (contentErrors.Count > 0)
        {
            await errorHandler.OnContentErrorsFound(new ContentErrorsFoundErrorInfo(contentErrors, fileLocationInfo));
            return (false, default!);
        }
        
        return (true, deserializedObject);
    }
    
    private readonly Byml _node;
    private readonly ImmutableStack<string> _propertyPathStack;
    private readonly Dictionary<Byml, BymlContentErrorInfo> _contentErrors;
    private readonly IReadOnlySet<PropertyPath>? _ignoreMissingProperties;

    private Deserializer(Byml node, ImmutableStack<string> propertyPathStack, 
        Dictionary<Byml, BymlContentErrorInfo> contentErrors,
        IReadOnlySet<PropertyPath>? ignoreMissingProperties)
    {
        _node = node;
        _propertyPathStack = propertyPathStack;
        _contentErrors = contentErrors;
        _ignoreMissingProperties = ignoreMissingProperties;
    }

    #region Error Reporting
    public void ReportUnexpectedType(BymlNodeType? expectedNodeType = null)
    {
        var errorInfo = GetOrCreateContentError(_node);
        errorInfo.Flags |= ContentErrorFlags.UnexpectedType;
        if (expectedNodeType.HasValue)
            errorInfo.ExpectedNodeType = expectedNodeType;
        _contentErrors[_node] = errorInfo;
    }
    
    private void ReportUnexpectedType(Byml node, BymlNodeType expectedNodeType)
    {
        var errorInfo = GetOrCreateContentError(node);
        errorInfo.Flags |= ContentErrorFlags.UnexpectedType;
        errorInfo.ExpectedNodeType = expectedNodeType;
        _contentErrors[node] = errorInfo;
    }
    
    public void ReportMissingKey(string key)
    {
        var errorInfo = GetOrCreateContentError(_node);
        errorInfo.Flags |= ContentErrorFlags.MissingKeys;
        (errorInfo.MissingRequiredKeys ??= []).Add(key);
        _contentErrors[_node] = errorInfo;
    }
    
    public void ReportMissingElement(int idx)
    {
        var errorInfo = GetOrCreateContentError(_node);
        errorInfo.Flags |= ContentErrorFlags.MissingElements;
        errorInfo.MinimumElementCount = 
            errorInfo.MinimumElementCount == null ? 
                idx + 1 : 
                Math.Max(errorInfo.MinimumElementCount.Value, idx + 1);
        _contentErrors[_node] = errorInfo;
    }
    
    public void ReportUnexpectedEnumValue()
    {
        var errorInfo = GetOrCreateContentError(_node);
        errorInfo.Flags |= ContentErrorFlags.UnexpectedEnumValue;
        _contentErrors[_node] = errorInfo;
    }
    
    public void ReportInvalidRefPath(FileRefConversion.FileRefPathFormat expectedFileRefPathFormat)
    {
        var errorInfo = GetOrCreateContentError(_node);
        errorInfo.Flags |= ContentErrorFlags.InvalidRefPath;
        errorInfo.ExpectedFileRefPathFormat = expectedFileRefPathFormat;
        _contentErrors[_node] = errorInfo;
    }
    
    private BymlContentErrorInfo GetOrCreateContentError(Byml node)
    {
        return _contentErrors.TryGetValue(node, out var errorInfo) ? 
            errorInfo : new BymlContentErrorInfo();
    }
    #endregion

    #region Direct access API
    //everything you need for deserialization and error reporting
    public Byml GetNode() => _node;
    
    /// <summary>
    /// Creates a new <see cref="Deserializer"/> that reports errors for <paramref name="node"/>
    /// to the instance this method is called on
    /// <para>It is assumed that <paramref name="node"/> is part of the same byml file. (No checks are done)</para>
    /// </summary>
    public Deserializer CreateDeserializerFor(Byml node) => new(node, ImmutableStack<string>.Empty, 
        _contentErrors, _ignoreMissingProperties);
    #endregion
    
    #region Checked ref assignment API
    //it's convenient and it lets us easily implement ISerializationContext
    
    public void Set<TValue>(BymlConversion<TValue> conversion, ref TValue value, int idx)
    {
        if (Retrieve(conversion.RequiredNodeType, idx) is not (true, {} node))
            return; //errors (if any) have been reported, nothing to do here

        value = conversion.Deserialize(new Deserializer(node, ImmutableStack<string>.Empty, 
            _contentErrors, _ignoreMissingProperties));
    }
    
    public void Set<TValue>(BymlConversion<TValue> conversion, ref TValue value, string key, 
        bool optional = false)
    {
        if (Retrieve(conversion.RequiredNodeType, key, optional) is not (true, {} node))
            return; //errors (if any) have been reported, nothing to do here

        value = conversion.Deserialize(new Deserializer(node, _propertyPathStack.Push(key), 
            _contentErrors, _ignoreMissingProperties));
    }
    public void Set<TValue>(BymlConversion<TValue> conversion, ref TValue? value, string key, 
        bool optional = false)
    where TValue : struct
    {
        if (Retrieve(conversion.RequiredNodeType, key, optional) is not (true, {} node))
            return; //errors (if any) have been reported, nothing to do here

        value = conversion.Deserialize(new Deserializer(node, _propertyPathStack.Push(key), 
            _contentErrors, _ignoreMissingProperties));
    }
    public void SetArray<TItem>(ref List<TItem> value, string key, BymlConversion<TItem> conversion, 
        bool optional = false)
    {
        if (Retrieve(BymlNodeType.Array, key, optional) is (true, {} byml))
            value = ArrayToList(byml.GetArray(), conversion);
        //errors (if any) have been reported, nothing left to do here
    }
    public void SetMap<TItem>(ref Dictionary<string, TItem> value, string key, BymlConversion<TItem> conversion, 
        bool optional = false)
    {
        if (Retrieve(BymlNodeType.Map, key, optional) is (true, {} byml))
            value = MapToDict(byml.GetMap(), conversion);
        //errors (if any) have been reported, nothing left to do here
    }
    
    private List<TItem> ArrayToList<TItem>(BymlArray bymlArray, BymlConversion<TItem> conversion)
    {
        var list = new List<TItem>();

        foreach (var node in bymlArray)
        {
            if (conversion.RequiredNodeType.HasValue && node.Type != conversion.RequiredNodeType)
            {
                ReportUnexpectedType(node, conversion.RequiredNodeType.Value);
                continue;
            }
            
            list.Add(conversion.Deserialize(new Deserializer(node, ImmutableStack<string>.Empty, 
                _contentErrors, _ignoreMissingProperties)));
        }

        return list;
    }
    private Dictionary<string, TItem> MapToDict<TItem>(BymlMap bymlMap, BymlConversion<TItem> conversion)
    {
        var dict = new Dictionary<string, TItem>();

        foreach ((string bymlMapKey, var node) in bymlMap)
        {
            if (conversion.RequiredNodeType.HasValue && node.Type != conversion.RequiredNodeType)
            {
                ReportUnexpectedType(node, conversion.RequiredNodeType.Value);
                continue;
            }
            
            dict[bymlMapKey] = conversion.Deserialize(new Deserializer(node, ImmutableStack<string>.Empty, 
                _contentErrors, _ignoreMissingProperties));
        }

        return dict;
    }
    #endregion
    
    
    /// <summary>
    /// Retrieve the value in the BymlMap of _node by key and check type
    /// </summary>
    private (bool success, Byml? node) Retrieve(BymlNodeType? requiredNodeType, string key, bool isOptional)
    {
        if (_node.Value is not BymlMap map)
            throw new InvalidOperationException(
                $"Methods with {nameof(key)} parameter can only be called " +
                $"if byml node is a {nameof(BymlMap)}"
                );

        if (!map.TryGetValue(key, out var retrievedNode))
        {
            if (!isOptional && (ReportAllMissingKeysAndIndices || !CanIgnoreMissing(key)))
                ReportMissingKey(key);
            return (false, null);
        }

        if (requiredNodeType.HasValue && retrievedNode.Type != requiredNodeType.Value)
        {
            ReportUnexpectedType(retrievedNode, expectedNodeType: requiredNodeType.Value);
            return (false, null);
        }
        
        return (true, retrievedNode);
    }

    private bool CanIgnoreMissing(string key) => 
        _ignoreMissingProperties?.Contains(new PropertyPath(_propertyPathStack.Push(key))) ?? false;
    
    
    /// <summary>
    /// Retrieve the value in the BymlArray of _node by idx and check type
    /// </summary>
    private (bool success, Byml? node) Retrieve(BymlNodeType? requiredNodeType, int idx)
    {
        if (_node.Value is not BymlArray array)
            throw new InvalidOperationException(
                $"Methods with {nameof(idx)} parameter can only be called " +
                $"if byml node is a {nameof(BymlArray)}"
            );

        if (idx >= array.Count)
        {
            ReportMissingElement(idx);
            return (false, null);
        }
        var retrievedNode = array[idx];

        if (requiredNodeType.HasValue && retrievedNode.Type != requiredNodeType.Value)
        {
            ReportUnexpectedType(retrievedNode, expectedNodeType: requiredNodeType.Value);
            return (false, null);
        }
        
        return (true, retrievedNode);
    }
}