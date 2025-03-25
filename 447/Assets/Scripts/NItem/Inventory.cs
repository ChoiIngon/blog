using UnityEngine;

namespace NItem
{
    public class Inventory : MonoBehaviour
    {
        public const int MaxSlotCount = 15;
        public Item[] items = new Item[MaxSlotCount];

        public int Count { get; private set; } = 0;

        public bool Insert(Item item)
        {
            for (int i = 0; i < MaxSlotCount; i++)
            {
                if (true == Insert(i, item))
                {
                    return true;
                }
            }

            return false;
        }

        public bool Insert(int index, Item item)
        {
            if (0 > index || MaxSlotCount <= index)
            {
                return false;
            }

            if (null != items[index])
            {
                return false;
            }

            if (null == item)
            {
                return false;
            }

            items[index] = item;
            Count += 1;
            item.gameObject.transform.SetParent(this.gameObject.transform, false);

            return true;
        }

        public Item Remove(int index)
        {
            if (0 > index || MaxSlotCount <= index)
            {
                return null;
            }

            if (null == items[index])
            {
                return null;
            }

            Item item = items[index];
            items[index] = null;
            Count -= 1;
            item.gameObject.transform.SetParent(null, false);

            return item;
        }

        public bool Swap(int from, int to)
        {
            if (0 > from || MaxSlotCount <= from)
            {
                return false;
            }

            if (0 > to || MaxSlotCount <= to)
            {
                return false;
            }

            Item temp = items[from];
            items[from] = items[to];
            items[to] = temp;

            return true;
        }

        public T Get<T>(int index) where T : Item
        {
            if (0 > index || MaxSlotCount <= index)
            {
                return null;
            }
            return items[index] as T;
        }
    }
}