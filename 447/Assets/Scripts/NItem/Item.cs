using System.Collections.Generic;
using UnityEngine;

namespace NItem
{
    public class Item : MonoBehaviour
    {
        public const int SortingOrder = Tile.SortingOrder + 1;

        public class Meta
        {
            public int index;
            public string code;
            public string name;
            public Sprite sprite;
        }

        public SpriteRenderer spriteRenderer;

        public void Visible(bool flag)
        {
            if (null == spriteRenderer)
            {
                return;
            }

            float alpha = 1.0f;
            if (false == flag)
            {
                alpha = 0.5f;
            }

            Color color = this.spriteRenderer.color;
            color.a = alpha;
            this.spriteRenderer.color = color;
        }

        public static Item Create(Meta meta)
        {
            GameObject gameObject = new GameObject(meta.name);
            Item item = gameObject.AddComponent<Item>();
            item.spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            item.spriteRenderer.sprite = meta.sprite;
            item.spriteRenderer.sortingOrder = SortingOrder;
            return item;
        }
        
        public class Manager
        {
            private Dictionary<int, Meta> indexToMeta = new Dictionary<int, Meta>();
            private Dictionary<string, Meta> codeToMeta = new Dictionary<string, Meta>();

            public void Init()
            {
                Add(new Meta() { index = 1001, code = "flasks_1_1", name = "flasks_1_1", sprite = GameManager.Instance.Resources.GetSprite("flasks_1_1") });
                Add(new Meta() { index = 1002, code = "flasks_1_2", name = "flasks_1_2", sprite = GameManager.Instance.Resources.GetSprite("flasks_1_2") });
            }

            public void Add(Meta meta)
            {
                indexToMeta.Add(meta.index, meta);
                codeToMeta.Add(meta.code, meta);
            }

            public Meta Find(int index)
            {
                return indexToMeta[index];
            }

            public Meta Find(string code)
            {
                return codeToMeta[code];
            }
        }
    }
}