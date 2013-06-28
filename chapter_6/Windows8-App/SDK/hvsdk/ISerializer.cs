// (c) Microsoft. All rights reserved
using System;
using System.IO;
using System.Xml.Serialization;

namespace HealthVault.Foundation
{
    public interface ISerializer
    {
        XmlSerializer SerializerForType(Type type);
        void SetSerializerForType(Type type, XmlSerializer serializer);

        void Serialize(TextWriter writer, object obj, object context);
        object Deserialize(TextReader reader, Type type, object context);
    }
}