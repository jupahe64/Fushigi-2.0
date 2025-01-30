using System.Numerics;
using BymlLibrary;
using BymlLibrary.Nodes.Containers;

namespace Fushigi.Data.BymlSerialization;

public static class SpecialConversions
{
    #region Float3

    public static readonly BymlConversion<Vector3> Float3 = new(ParseFloat3, SerializeFloat3);
    public static Vector3 ParseFloat3(Byml node)
    {
        var array = node.GetArray();
        return new Vector3(
            array[0].GetFloat(),
            array[1].GetFloat(),
            array[2].GetFloat()
            );
    }
    public static Byml SerializeFloat3(Vector3 value)
    {
        return new Byml([
            new Byml(value.X),
            new Byml(value.Y),
            new Byml(value.Z)
        ]);
    }
    #endregion

    #region Vector3
    public static readonly BymlConversion<Vector3> Vector3D = new(ParseVector3D, SerializeVector3D);
    public static Vector3 ParseVector3D(Byml node)
    {
        var map = node.GetMap();
        return new Vector3(
            map["X"].GetFloat(),
            map["Y"].GetFloat(),
            map["Z"].GetFloat()
            );
    }

    public static Byml SerializeVector3D(Vector3 value)
    {
        return new Byml(new BymlMap
        {
            ["X"] = new Byml(value.X),
            ["Y"] = new Byml(value.Y),
            ["Z"] = new Byml(value.Z)
        });
    }
    #endregion
}