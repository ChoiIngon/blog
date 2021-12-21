using System;
using System.Collections.Generic;
using UnityEngine;

namespace Breakout
{
    public class Block : MonoBehaviour
    {
        public class Collider : MonoBehaviour
        {
            Block self;
            private void Start()
            {
                self = GetComponent<Block>();
            }

            private void OnCollisionEnter(Collision collision)
            {
                Breakout.Ball ball = collision.gameObject.GetComponent<Ball>();
                if (null == ball)
                {
                    return;
                }

                self.room.SyncBall(ball);
                self.durability -= 1;

                self.room.SyncBlock(self);

                if (0 == self.durability)
                {
                    transform.SetParent(null);
                    GameObject.Destroy(gameObject);

                    //if (0 == GameManager.Instance.blocks.childCount)
                    //{
                    //    GameManager.Instance.Init();
                    //}
                }
            }
        }

        [Serializable]
        public class MetaData
        {
            public uint type;
            public uint score;
            public uint durability;
            public Color color;
        }

        public static readonly MetaData[] metaData = new MetaData[]
        {
            new MetaData { type = 1, score=50,   durability= 1,  color = Color.white },
            new MetaData { type = 2, score=60,   durability= 1,  color = new Color(255/255f, 150/255f, 60/255f) },
            new MetaData { type = 3, score=70,   durability= 1,  color = Color.cyan },
            new MetaData { type = 4, score=80,   durability= 1,  color = Color.green },
            new MetaData { type = 5, score=90,   durability= 1,  color = Color.red },
            new MetaData { type = 6, score=100,  durability= 1,  color = Color.blue },
            new MetaData { type = 7, score=110,  durability= 1,  color = new Color(193/255f, 46/255f, 209/255f) },
            new MetaData { type = 8, score=120,  durability= 1,  color = Color.yellow }
        };

        public uint id;
        public MetaData meta { get; private set; }
        public uint durability;
        public Room room;

        public void Init(Room room, uint blockType)
        {
            this.room = room;
            this.durability = 1;
            this.meta = metaData[blockType - 1];
            SetColor(meta.color);
        }

        private void SetColor(Color color)
        {
            Renderer renderer = GetComponent<Renderer>();
            renderer.material.SetColor("_Color", color);
        }
    }
}