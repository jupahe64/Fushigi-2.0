using BymlLibrary;
using Fushigi.Data.BymlSerialization;

namespace Fushigi.Data.Files;

public static class FormatDescriptors
{
    public static readonly FileFormatDescriptor<Byml> BymlUncompressed = new(
        IsCompressed: false, x => Byml.FromBinary(x)
    );
    
    public static readonly FileFormatDescriptor<Byml> BymlCompressed = new(
        IsCompressed: true, x => Byml.FromBinary(x)
    );
}