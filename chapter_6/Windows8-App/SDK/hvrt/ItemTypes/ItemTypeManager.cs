// (c) Microsoft. All rights reserved

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Xml.Serialization;
using HealthVault.Foundation;
using HealthVault.Types;
using Windows.Data.Xml.Dom;
using Windows.Foundation;

namespace HealthVault.ItemTypes
{
    public sealed class ItemTypeMap
    {
        private readonly Dictionary<string, XmlElementAttribute> m_map;

        public ItemTypeMap()
        {
            m_map = new Dictionary<string, XmlElementAttribute>();
        }

        internal IEnumerable<XmlElementAttribute> SerializerAttributes
        {
            get { return m_map.Values; }
        }

        public void Add(string elementName, Type type)
        {
            Add(new XmlElementAttribute(elementName, type));
        }

        internal void Add(XmlElementAttribute attribute)
        {
            m_map[attribute.ElementName] = attribute;
        }

        internal void Add(ItemTypeMap map)
        {
            foreach (XmlElementAttribute attribute in map.SerializerAttributes)
            {
                Add(attribute);
            }
        }

        internal void AddPredefined()
        {
            PropertyInfo property = typeof(ItemData).GetRuntimeProperty("Typed");
            foreach (object attribute in property.GetCustomAttributes())
            {
                var elementAttribute = attribute as XmlElementAttribute;
                if (elementAttribute != null)
                {
                    Add(elementAttribute);
                }
            }
        }

        internal XmlAttributeOverrides GetSerializerOverrides()
        {
            var overrides = new XmlAttributeOverrides();
            var attributes = new XmlAttributes();

            foreach (XmlElementAttribute attribute in SerializerAttributes)
            {
                attributes.XmlElements.Add(attribute);
            }

            overrides.Add(typeof(ItemData), "Typed", attributes);

            return overrides;
        }
    }

    public static class ItemTypeManager
    {
        private static ItemTypeMap s_typeMap;

        internal static ItemTypeMap TypeMap
        {
            get
            {
                if (s_typeMap == null)
                {
                    s_typeMap = new ItemTypeMap();
                    s_typeMap.AddPredefined();
                }

                return s_typeMap;
            }
        }

        public static IAsyncOperation<IList<ItemTypeDefinition>> GetItemTypeDefinitions(HealthVaultApp app, ThingTypeGetParams getParams)
        {
            return AsyncInfo.Run<IList<ItemTypeDefinition>>(
                async cancelToken =>
                {
                    string xml = getParams.ToXml();
                    
                    // Strip off the <ThingTypeGetParams> root node
                    // because all of its properties should be nested under <info> (RequestBody)
                    var doc = new XmlDocument();
                    doc.LoadXml(xml);
                    var innerXml = new StringBuilder();
                    if (doc.ChildNodes != null && doc.ChildNodes.Length == 1)
                    {
                        var innerNodes = doc.ChildNodes[0].ChildNodes;
                        foreach (IXmlNode node in innerNodes)
                        {
                            innerXml.Append(node.GetXml());
                        }
                    }

                    ThingTypeGetResults result =
                        await app.Client.ServiceMethods.GetThingType<ThingTypeGetResults>(innerXml.ToString(), cancelToken);
                    return result.ItemTypeDefinitions;
                });
        }

        public static void Init()
        {
            RegisterSerializer();
        }

        public static void RegisterTypes(ItemTypeMap types)
        {
            if (types == null)
            {
                throw new ArgumentNullException("types");
            }

            TypeMap.Add(types);
            RegisterSerializer();
        }

        internal static void RegisterSerializer()
        {
            XmlAttributeOverrides overrides = TypeMap.GetSerializerOverrides();

            ISerializer serializer = HealthVaultClient.Serializer;

            serializer.SetSerializerForType(typeof(RecordItem), new XmlSerializer(typeof(RecordItem), overrides));
            serializer.SetSerializerForType(typeof(ItemData), new XmlSerializer(typeof(ItemData), overrides));
            serializer.SetSerializerForType(
                typeof(RecordQueryResponse), new XmlSerializer(typeof(RecordQueryResponse), overrides));
        }
    }
}