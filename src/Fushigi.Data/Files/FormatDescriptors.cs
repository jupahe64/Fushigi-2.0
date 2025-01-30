using BymlLibrary;
using Fushigi.Data.BymlSerialization;

namespace Fushigi.Data.Files;

public static class FormatDescriptors
{
    public static FileFormatDescriptor<TGymlType> GetGymlFormat<TGymlType>()
        where TGymlType : SerializableBymlObject<TGymlType>, new()
        => new(IsCompressed: false, ReadGymlFile<TGymlType>);
    private static TGymlType ReadGymlFile<TGymlType>(ArraySegment<byte> bytes)
        where TGymlType : SerializableBymlObject<TGymlType>, new()
        => SerializableBymlObject<TGymlType>.DeserializeFrom(Byml.FromBinary(bytes));
}