using System.Collections.Generic;

namespace SlipeLua.CodeGenerator;

public static class MetaGenerationKnownFilePriority
{
    public static List<string> PreUnknown { get; set; } =
    [
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/All.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Core.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Interfaces.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Exception.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Math.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Number.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Char.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/String.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Boolean.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Delegate.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Enum.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/TimeSpan.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/DateTime.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Collections/EqualityComparer.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Array.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Type.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Collections/List.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Collections/Dictionary.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Collections/Queue.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Collections/Stack.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Collections/HashSet.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Collections/LinkedList.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Collections/SortedSet.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Collections/SortedList.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Collections/SortedDictionary.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Collections/PriorityQueue.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Collections/Linq.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Convert.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Random.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Text/StringBuilder.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Console.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/IO/File.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Reflection/Assembly.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Threading/Timer.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Threading/Thread.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Threading/Task.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Utilities.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Globalization/Globalization.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Numerics/HashCodeHelper.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Numerics/Complex.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Numerics/Matrix3x2.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Numerics/Matrix4x4.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Numerics/Plane.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Numerics/Quaternion.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Numerics/Vector2.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Numerics/Vector3.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Numerics/Vector4.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Text/Encoding.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Xml/XmlNodeList.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Xml/XmlNode.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Xml/XmlAttribute.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Xml/XmlAttributeCollection.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Xml/XmlElement.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Xml/XmlDocument.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Net/IPAddress.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Net/Sockets/Socket.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Net/EndPoint.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Net/Http/FormUrlEncodedContent.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Net/Http/HttpClient.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Net/Http/HttpContent.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Net/Http/HttpResponseMessage.lua",
        "Lua/Dist/SlipeLua.MtaDefinitions/aCoreSystemLua/CoreSystem/Net/Http/StringContent.lua"
    ];

    public static List<string> PostUnknown { get; set; } =
    [
        "Lua/Dist/SlipeLua.MtaDefinitions/manifest.lua"
    ];
}
