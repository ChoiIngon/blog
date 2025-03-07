using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;

public class Dungeon : MonoBehaviour
{
    public static class SortingOrder
    {
        public static int Tile = 100;
        public static int Gimmick = 101;
        public static int Actor = 102;
    }

    Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
    public class TileSprite
    {
        public readonly GameObject gameObject;
        protected SpriteRenderer spriteRenderer;
        public Tile tile;

        public TileSprite(Tile tile)
        {
            this.tile = tile;
            this.gameObject = new GameObject($"TileSprite_{tile.index}");
            this.gameObject.transform.position = new Vector3(tile.rect.x + tile.rect.width / 2, tile.rect.y + tile.rect.height/2);
            this.spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            this.spriteRenderer.sortingOrder = SortingOrder.Tile;
            this.spriteRenderer.color = Color.white;
        }

        public void SetParent(Transform transform)
        {
            gameObject.transform.parent = transform;
        }

        public Color color
        {
            get
            {
                return this.spriteRenderer.color;
            }
            set
            {
                this.spriteRenderer.color = value;
            }
        }

        protected static Sprite GetRandomSprite(List<Sprite> sprites)
        {
            if (0 == sprites.Count)
            {
                return null;
            }

            return sprites[Random.Range(0, sprites.Count)];
        }
    }

    public class FloorSprite : TileSprite
    {
        public static List<Sprite> InnerNormal = new List<Sprite>();
        public static List<Sprite> CornerInnerLeftTop = new List<Sprite>();
        public static List<Sprite> CornerInnerRightTop = new List<Sprite>();
        public static List<Sprite> CornerInnerLeftBottom = new List<Sprite>();
        public static List<Sprite> CornerInnerRightBottom = new List<Sprite>();
        public static List<Sprite> HorizontalTop = new List<Sprite>();
        public static List<Sprite> HorizontalBottom = new List<Sprite>();
        public static List<Sprite> VerticalRight = new List<Sprite>();
        public static List<Sprite> VerticalLeft = new List<Sprite>();

        public FloorSprite(Tile tile) : base(tile)
        {
            this.spriteRenderer.sprite = GetSprite(tile);
        }

        private Sprite GetSprite(Tile tile)
        {
            var leftTop = tile.neighbors[(int)Tile.Direction.LeftTop];
            var top = tile.neighbors[(int)Tile.Direction.Top];
            var rightTop = tile.neighbors[(int)Tile.Direction.RightTop];
            var left = tile.neighbors[(int)Tile.Direction.Left];
            var right = tile.neighbors[(int)Tile.Direction.Right];
            var leftBottom = tile.neighbors[(int)Tile.Direction.LeftBottom];
            var bottom = tile.neighbors[(int)Tile.Direction.Bottom];
            var rightBottom = tile.neighbors[(int)Tile.Direction.RightBottom];

            if (true == IsWall(top) && true == IsWall(left))
            {
                return GetRandomSprite(CornerInnerLeftTop);
            }

            if (true == IsWall(top) && true == IsWall(right))
            {
                return GetRandomSprite(CornerInnerRightTop);
            }

            if (true == IsWall(bottom) && true == IsWall(left))
            {
                return GetRandomSprite(CornerInnerLeftBottom);
            }

            if (true == IsWall(bottom) && true == IsWall(right))
            {
                return GetRandomSprite(CornerInnerRightBottom);
            }

            if (true == IsWall(top))
            {
                return GetRandomSprite(HorizontalTop);
            }

            if (true == IsWall(left))
            {
                return GetRandomSprite(VerticalLeft);
            }

            if (true == IsWall(right))
            {
                return GetRandomSprite(VerticalRight);
            }

            if (true == IsWall(bottom))
            {
                return GetRandomSprite(HorizontalBottom);
            }

            return GetRandomSprite(InnerNormal);
        }

        private bool IsWall(Tile tile)
        {
            if (null == tile)
            {
                return false;
            }

            if (Tile.Type.Wall != tile.type)
            {
                return false;
            }

            return true;
        }
    }

    public class WallSprite : TileSprite
    {
        public static List<Sprite> HorizontalTop = new List<Sprite>();
        public static List<Sprite> HorizontalBottom = new List<Sprite>();
        public static List<Sprite> VerticalTop = new List<Sprite>();
        public static List<Sprite> VerticalRight = new List<Sprite>();
        public static List<Sprite> VerticalSplit = new List<Sprite>();
        public static List<Sprite> VerticalLeft = new List<Sprite>();
        public static List<Sprite> CornerInnerLeftTop = new List<Sprite>();
        public static List<Sprite> CornerInnerRightTop = new List<Sprite>();
        public static List<Sprite> CornerInnerLeftBottom = new List<Sprite>();
        public static List<Sprite> CornerInnerRightBottom = new List<Sprite>();
        public static List<Sprite> CornerOuterLeftTop = new List<Sprite>();
        public static List<Sprite> CornerOuterRightTop = new List<Sprite>();

        public WallSprite(Tile tile) : base(tile)
        {
            this.spriteRenderer.sprite = GetSprite(tile);
        }

        private Sprite GetSprite(Tile tile)
        {
            int x = (int)tile.rect.x;
            int y = (int)tile.rect.y;

            var leftTop     = tile.neighbors[(int)Tile.Direction.LeftTop];
            var top         = tile.neighbors[(int)Tile.Direction.Top];
            var rightTop    = tile.neighbors[(int)Tile.Direction.RightTop];
            var left        = tile.neighbors[(int)Tile.Direction.Left];
            var right       = tile.neighbors[(int)Tile.Direction.Right];
            var leftBottom  = tile.neighbors[(int)Tile.Direction.LeftBottom];
            var bottom      = tile.neighbors[(int)Tile.Direction.Bottom];
            var rightBottom = tile.neighbors[(int)Tile.Direction.RightBottom];

            bool[] floorsAroundWall = new bool[9] {
                IsFloor(leftTop),
                IsFloor(top),
                IsFloor(rightTop),
                IsFloor(left),
                false,
                IsFloor(right),
                IsFloor(leftBottom),
                IsFloor(bottom),
                IsFloor(rightBottom)
            };

            bool[] wallSpriteFlags = new bool[4] { false, false, false, false };

            if (true == floorsAroundWall[0]) { wallSpriteFlags[0] = true; }
            if (true == floorsAroundWall[1]) { wallSpriteFlags[0] = true; wallSpriteFlags[1] = true; }
            if (true == floorsAroundWall[2]) { wallSpriteFlags[1] = true; }
            if (true == floorsAroundWall[3]) { wallSpriteFlags[0] = true; wallSpriteFlags[2] = true; }
            if (true == floorsAroundWall[5]) { wallSpriteFlags[1] = true; wallSpriteFlags[3] = true; }
            if (true == floorsAroundWall[6]) { wallSpriteFlags[2] = true; }
            if (true == floorsAroundWall[7]) { wallSpriteFlags[2] = true; wallSpriteFlags[3] = true; }
            if (true == floorsAroundWall[8]) { wallSpriteFlags[3] = true; }

            // 0
            if (true == wallSpriteFlags[0] && false == wallSpriteFlags[1] && false == wallSpriteFlags[2] && false == wallSpriteFlags[3])
            {
                return GetRandomSprite(CornerInnerRightBottom);
            }

            // 1
            if (false == wallSpriteFlags[0] && true == wallSpriteFlags[1] && false == wallSpriteFlags[2] && false == wallSpriteFlags[3])
            {
                return GetRandomSprite(CornerInnerLeftBottom);
            }

            // 2
            if (false == wallSpriteFlags[0] && false == wallSpriteFlags[1] && true == wallSpriteFlags[2] && false == wallSpriteFlags[3])
            {
                return GetRandomSprite(CornerInnerRightTop);
            }

            // 3
            if (false == wallSpriteFlags[0] && false == wallSpriteFlags[1] && false == wallSpriteFlags[2] && true == wallSpriteFlags[3])
            {
                return GetRandomSprite(CornerInnerLeftTop);
            }

            // 01
            if (true == wallSpriteFlags[0] && true == wallSpriteFlags[1] && false == wallSpriteFlags[2] && false == wallSpriteFlags[3])
            {
                return GetRandomSprite(HorizontalBottom);
            }

            // 02
            if (true == wallSpriteFlags[0] && false == wallSpriteFlags[1] && true == wallSpriteFlags[2] && false == wallSpriteFlags[3])
            {
                return GetRandomSprite(VerticalRight);
            }

            // 03
            if (true == wallSpriteFlags[0] && false == wallSpriteFlags[1] && false == wallSpriteFlags[2] && true == wallSpriteFlags[3])
            {
                return GetRandomSprite(CornerOuterRightTop);
            }

            // 12
            if (false == wallSpriteFlags[0] && true == wallSpriteFlags[1] && true == wallSpriteFlags[2] && false == wallSpriteFlags[3])
            {
                return GetRandomSprite(CornerOuterLeftTop);
            }

            // 13
            if (false == wallSpriteFlags[0] && true == wallSpriteFlags[1] && false == wallSpriteFlags[2] && true == wallSpriteFlags[3])
            {
                return GetRandomSprite(VerticalLeft);
            }

            // 23
            if (false == wallSpriteFlags[0] && false == wallSpriteFlags[1] && true == wallSpriteFlags[2] && true == wallSpriteFlags[3])
            {
                if (true == IsWall(bottom))
                {
                    return GetRandomSprite(VerticalSplit);
                }
                return GetRandomSprite(HorizontalTop);
            }

            // 012
            if (true == wallSpriteFlags[0] && true == wallSpriteFlags[1] && true == wallSpriteFlags[2] && false == wallSpriteFlags[3])
            {
                return GetRandomSprite(CornerOuterLeftTop);
            }

            // 013
            if (true == wallSpriteFlags[0] && true == wallSpriteFlags[1] && false == wallSpriteFlags[2] && true == wallSpriteFlags[3])
            {
                return GetRandomSprite(CornerOuterRightTop);
            }

            // 023
            if (true == wallSpriteFlags[0] && false == wallSpriteFlags[1] && true == wallSpriteFlags[2] && true == wallSpriteFlags[3])
            {
                if (true == IsWall(bottom))
                {
                    return GetRandomSprite(VerticalSplit);
                }
                return GetRandomSprite(HorizontalTop);
            }

            // 123
            if (false == wallSpriteFlags[0] && true == wallSpriteFlags[1] && true == wallSpriteFlags[2] && true == wallSpriteFlags[3])
            {
                if (true == IsWall(bottom))
                {
                    return GetRandomSprite(VerticalSplit);
                }
                return GetRandomSprite(HorizontalTop);
            }

            // 0123
            if (true == wallSpriteFlags[0] && true == wallSpriteFlags[1] && true == wallSpriteFlags[2] && true == wallSpriteFlags[3])
            {
                if (true == IsWall(top) && false == IsWall(bottom))
                {
                    return GetRandomSprite(HorizontalTop);
                }

                if (true == IsWall(top) && true == IsWall(bottom))
                {
                    return GetRandomSprite(VerticalSplit);
                }

                if (true == IsWall(bottom))
                {
                    return GetRandomSprite(VerticalTop);
                }

                if (true == IsWall(right))
                {
                    return GetRandomSprite(HorizontalTop);
                }

                if (true == IsWall(left))
                {
                    return GetRandomSprite(HorizontalTop);
                }
            }

            return GetRandomSprite(HorizontalTop);
        }

        private bool IsFloor(Tile tile)
        {
            if (null == tile)
            {
                return false;
            }

            if (Tile.Type.Floor != tile.type)
            {
                return false;
            }

            return true;
        }

        private bool IsWall(Tile tile)
        {
            if (null == tile)
            {
                return false;
            }

            if (Tile.Type.Wall != tile.type)
            {
                return false;
            }

            return true;
        }
    }

    public TileSprite[] tileSprites;
    // Start is called before the first frame update
    public void AttachTile(TileMap tileMap)
    {
        Clear();

        SpriteAtlas spriteAtlas = Resources.Load<SpriteAtlas>("SpriteAtlas/TileSet");
        if (0 == spriteAtlas.spriteCount)
        {
            return;
        }

        Sprite[] laodedSprites = new Sprite[spriteAtlas.spriteCount];
        if (0 == spriteAtlas.GetSprites(laodedSprites))
        {
            return;
        }

        foreach (Sprite sprite in laodedSprites)
        {
            string name = sprite.name.Replace("(Clone)", "");   // GetSprites는 Clone을 리턴하기 때문에, 이름에서 (Clone) postfix를 제거해준다.
            sprites.Add(name, sprite);
        }

        FloorSprite.CornerInnerLeftBottom.Add(sprites["Floor.CornerInnerLeftBottom_1"]);
        FloorSprite.CornerInnerLeftTop.Add(sprites["Floor.CornerInnerLeftTop_1"]);
        FloorSprite.CornerInnerRightBottom.Add(sprites["Floor.CornerInnerRightBottom_1"]);
        FloorSprite.CornerInnerRightTop.Add(sprites["Floor.CornerInnerRightTop_1"]);
        FloorSprite.HorizontalBottom.Add(sprites["Floor.HorizontalBottom_1"]);
        FloorSprite.HorizontalBottom.Add(sprites["Floor.HorizontalBottom_2"]);
        FloorSprite.HorizontalTop.Add(sprites["Floor.HorizontalTop_1"]);
        FloorSprite.HorizontalTop.Add(sprites["Floor.HorizontalTop_2"]);
        FloorSprite.InnerNormal.Add(sprites["Floor.InnerNormal_1"]);
        FloorSprite.InnerNormal.Add(sprites["Floor.InnerNormal_2"]);
        FloorSprite.VerticalLeft.Add(sprites["Floor.VerticalLeft_1"]);
        FloorSprite.VerticalRight.Add(sprites["Floor.VerticalRight_1"]);

        WallSprite.CornerInnerLeftBottom.Add(sprites["Wall.CornerInnerLeftBottom_1"]);
        WallSprite.CornerInnerLeftTop.Add(sprites["Wall.CornerInnerLeftTop_1"]);
        WallSprite.CornerInnerRightBottom.Add(sprites["Wall.CornerInnerRightBottom_1"]);
        WallSprite.CornerInnerRightTop.Add(sprites["Wall.CornerInnerRightTop_1"]);

        WallSprite.CornerOuterLeftTop.Add(sprites["Wall.CornerOuterLeftTop_1"]);
        WallSprite.CornerOuterLeftTop.Add(sprites["Wall.CornerOuterLeftTop_2"]);
        WallSprite.CornerOuterRightTop.Add(sprites["Wall.CornerOuterRightTop_1"]);
        WallSprite.CornerOuterRightTop.Add(sprites["Wall.CornerOuterRightTop_2"]);

        WallSprite.HorizontalBottom.Add(sprites["Wall.HorizontalBottom_1"]);
        WallSprite.HorizontalBottom.Add(sprites["Wall.HorizontalBottom_2"]);
        WallSprite.HorizontalBottom.Add(sprites["Wall.HorizontalBottom_3"]);
        WallSprite.HorizontalBottom.Add(sprites["Wall.HorizontalBottom_4"]);

        WallSprite.HorizontalTop.Add(sprites["Wall.HorizontalTop_1"]);
        WallSprite.HorizontalTop.Add(sprites["Wall.HorizontalTop_2"]);
        WallSprite.HorizontalTop.Add(sprites["Wall.HorizontalTop_3"]);
        WallSprite.HorizontalTop.Add(sprites["Wall.HorizontalTop_4"]);

        WallSprite.VerticalLeft.Add(sprites["Wall.VerticalLeft_1"]);
        WallSprite.VerticalLeft.Add(sprites["Wall.VerticalLeft_2"]);
        WallSprite.VerticalLeft.Add(sprites["Wall.VerticalLeft_3"]);

        WallSprite.VerticalRight.Add(sprites["Wall.VerticalRight_1"]);
        WallSprite.VerticalRight.Add(sprites["Wall.VerticalRight_2"]);
        WallSprite.VerticalRight.Add(sprites["Wall.VerticalRight_3"]);

        WallSprite.VerticalSplit.Add(sprites["Wall.VerticalSplit_1"]);
        WallSprite.VerticalSplit.Add(sprites["Wall.VerticalSplit_2"]);
        WallSprite.VerticalSplit.Add(sprites["Wall.VerticalSplit_3"]);
        WallSprite.VerticalSplit.Add(sprites["Wall.VerticalSplit_4"]);

        WallSprite.VerticalTop.Add(sprites["Wall.VerticalTop_1"]);

        tileSprites = new TileSprite[tileMap.width * tileMap.height];
        for (int i = 0; i < tileSprites.Length; i++)
        {
            var tile = tileMap.GetTile(i);
            if (null == tile)
            {
                continue;
            }

            if (Tile.Type.None == tile.type)
            {
                continue;
            }

            GameManager.Instance.EnqueueEvent(new GameManager.AttachTileSprite(tile));
        }
    }

    public void Clear()
    {
        sprites.Clear();
        if (null != tileSprites)
        {
            foreach (var tileSprite in tileSprites)
            {
                if (null == tileSprite)
                {
                    continue;
                }
                tileSprite.gameObject.transform.SetParent(null, false);
                GameObject.DestroyImmediate(tileSprite.gameObject);
            }
        }
    }
}
