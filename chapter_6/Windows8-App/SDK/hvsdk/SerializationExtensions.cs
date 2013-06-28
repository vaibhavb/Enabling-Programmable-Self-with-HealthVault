// (c) Microsoft. All rights reserved
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace HealthVault.Foundation
{
    public static class SerializationExtensions
    {
        public static void Serialize<T>(this ISerializer serializer, XmlWriter writer, T value)
        {
            serializer.SerializerForType(typeof (T)).Serialize(writer, value);
        }

        public static T Deserialize<T>(this ISerializer serializer, XmlReader reader)
        {
            return (T) serializer.Deserialize(reader, typeof (T));
        }

        public static object Deserialize(this ISerializer serializer, XmlReader reader, Type type)
        {
            return serializer.SerializerForType(type).Deserialize(reader);
        }

        public static string ToXml(this object obj)
        {
            using (var writer = new StringWriter())
            {
                HealthVaultClient.Serializer.Serialize(writer, obj, null);
                return writer.ToString();
            }
        }

        public static string ToXml(this object obj, string root)
        {
            using (var textWriter = new StringWriter())
            {
                 XmlSerializer serializer = new XmlSerializer(
                    obj.GetType(), 
                    new XmlRootAttribute(root));

                 using (var xmlWriter = (HealthVaultXmlWriter)HealthVaultXmlWriter.Create(textWriter))
                 {
                     xmlWriter.Context = null;
                     serializer.Serialize(xmlWriter, obj);
                 }

                 return textWriter.ToString();
            }
        }

        public static string ToXml(this Array array, bool nested)
        {
            if (nested)
            {
                return array.ToXml();
            }

            using (var writer = new StringWriter())
            {
                foreach (object item in array)
                {
                    HealthVaultClient.Serializer.Serialize(writer, item, null);
                }
                return writer.ToString();
            }
        }

        public static T FromXml<T>(this ISerializer serializer, string xml)
        {
            return (T) serializer.FromXml(xml, typeof (T));
        }

        public static object FromXml(this ISerializer serializer, string xml, Type type)
        {
            using (var reader = new StringReader(xml))
            {
                return serializer.Deserialize(reader, type, null);
            }
        }
    }
}