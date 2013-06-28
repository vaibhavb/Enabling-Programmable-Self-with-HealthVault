// (c) Microsoft. All rights reserved

using HealthVault.Types;

namespace HealthVault.ItemTypes
{
    internal struct ItemProxy
    {
        private readonly string m_typeID;
        private readonly IItemDataTyped m_typedData;
        private ItemData m_itemData;

        internal ItemProxy(string typeID, IItemDataTyped typedData)
        {
            m_typeID = typeID;
            m_typedData = typedData;
            m_itemData = null;
        }

        internal RecordItem Item
        {
            get { return EnsureItemData().Item; }
        }

        internal ItemData ItemData
        {
            get { return EnsureItemData(); }
            set { m_itemData = value; }
        }

        private ItemData EnsureItemData()
        {
            if (m_itemData == null)
            {
                var item = new RecordItem();
                item.Type = new ItemType(m_typeID);

                m_itemData = new ItemData(m_typedData);
                item.Data = m_itemData;
            }

            return m_itemData;
        }

        public static implicit operator RecordItem(ItemProxy proxy)
        {
            return proxy.Item;
        }
    }
}