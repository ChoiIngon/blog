using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dungeon : MonoBehaviour
{
    public static class SortingOrder
    {
        public static int Block = 0;
        public static int CorridorGraph = 10;
        public static int CorridorPath = 20;

        public static int Floor = 100;
        public static int Wall = 110;
        public static int Player = 120;
    }

    public class Tile
    {
        public static Func<int, int, Data.Tile> GetTile;

        public readonly GameObject gameObject;
        
        protected SpriteRenderer spriteRenderer;

        public Tile(Data.Tile data)
        {
            this.gameObject = new GameObject($"Tile_{data.index}");
            this.gameObject.transform.position = new Vector3(data.rect.x + 0.5f, data.rect.y + 0.5f);
            this.spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
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

            return sprites[UnityEngine.Random.Range(0, sprites.Count)];
        }
        
        protected bool IsWall(Data.Tile data)
        {
            if (null == data)
            {
                return false;
            }

            if (Data.Tile.Type.Wall != data.type)
            {
                return false;
            }

            return true;
        }

        protected bool IsFloor(Data.Tile data)
        {
            if (null == data)
            {
                return false;
            }

            if (Data.Tile.Type.Floor != data.type)
            {
                return false;
            }

            return true;
        }
    }

    public class Floor : Tile
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

        public Floor(Data.Tile data) : base(data)
        {
            spriteRenderer.sprite = GetSprite(data);
            spriteRenderer.sortingOrder = Dungeon.SortingOrder.Floor;
        }

        private Sprite GetSprite(Data.Tile tile)
        {
            int x = (int)tile.rect.x;
            int y = (int)tile.rect.y;

            var leftTop = GetTile(x - 1, y + 1);
            var top = GetTile(x, y + 1);
            var rightTop = GetTile(x + 1, y + 1);
            var left = GetTile(x - 1, y);
            var right = GetTile(x + 1, y);
            var leftBottom = GetTile(x - 1, y - 1);
            var bottom = GetTile(x, y - 1);
            var rightBottom = GetTile(x + 1, y - 1);

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
    }

    public class Wall : Tile
    {
        public static List<Sprite> Undefined = new List<Sprite>();
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
        
        public Wall(Data.Tile tile) : base(tile)
        {
            spriteRenderer.sprite = GetSprite(tile);
            spriteRenderer.sortingOrder = Dungeon.SortingOrder.Floor;
        }

        private Sprite GetSprite(Data.Tile tile)
        {
            int x = (int)tile.rect.x;
            int y = (int)tile.rect.y;

            var leftTop = GetTile(x - 1, y + 1);
            var top = GetTile(x, y + 1);
            var rightTop = GetTile(x + 1, y + 1);
            var left = GetTile(x - 1, y);
            var right = GetTile(x + 1, y);
            var leftBottom = GetTile(x - 1, y - 1);
            var bottom = GetTile(x, y - 1);
            var rightBottom = GetTile(x + 1, y - 1);

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
            }

            // 12
            if (false == wallSpriteFlags[0] && true == wallSpriteFlags[1] && true == wallSpriteFlags[2] && false == wallSpriteFlags[3])
            {
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

            return GetRandomSprite(Undefined);
        }
    }

    public Data.Dungeon data;
    public Data.Block start;
    public Player player;

    public Transform tileRoot;
    public Tile[] tiles;

    private BoxCollider boxCollider;

    public List<DungeonGizmo.Gizmo> blockGizmo = new List<DungeonGizmo.Gizmo>();
    public List<DungeonGizmo.Gizmo> corridorGraphGizmo = new List<DungeonGizmo.Gizmo>();
    public List<DungeonGizmo.Gizmo> astarPathGizmo = new List<DungeonGizmo.Gizmo>();
    public List<DungeonGizmo.Gizmo> tileGizmo = new List<DungeonGizmo.Gizmo>();

    private void Start()
    {
        GameObject tileRootGameObject = new GameObject("TileRoot");
        tileRootGameObject.transform.parent = transform;
        this.tileRoot = tileRootGameObject.transform;

        boxCollider = gameObject.AddComponent<BoxCollider>();

        Tile.GetTile = (int x, int y) =>
        {
            return data.GetTile(x, y);
        };

        Floor.CornerInnerLeftTop.Add(GameManager.Instance.sprites["DungeonTileset_7"]);
        Floor.HorizontalTop.Add(GameManager.Instance.sprites["DungeonTileset_8"]);
        Floor.HorizontalTop.Add(GameManager.Instance.sprites["DungeonTileset_9"]);
        Floor.CornerInnerRightTop.Add(GameManager.Instance.sprites["DungeonTileset_10"]);
        Floor.VerticalLeft.Add(GameManager.Instance.sprites["DungeonTileset_13"]);
        Floor.VerticalRight.Add(GameManager.Instance.sprites["DungeonTileset_16"]);
        Floor.CornerInnerLeftBottom.Add(GameManager.Instance.sprites["DungeonTileset_19"]);
        Floor.HorizontalBottom.Add(GameManager.Instance.sprites["DungeonTileset_20"]);
        Floor.HorizontalBottom.Add(GameManager.Instance.sprites["DungeonTileset_21"]);
        Floor.CornerInnerRightBottom.Add(GameManager.Instance.sprites["DungeonTileset_22"]);
        Floor.InnerNormal.Add(GameManager.Instance.sprites["DungeonTileset_14"]);
        Floor.InnerNormal.Add(GameManager.Instance.sprites["DungeonTileset_15"]);
        
        Wall.Undefined.Add(GameManager.Instance.sprites["DungeonTileset_42"]);
        Wall.Undefined.Add(GameManager.Instance.sprites["DungeonTileset_43"]);
        Wall.Undefined.Add(GameManager.Instance.sprites["DungeonTileset_44"]);

        Wall.CornerInnerLeftTop.Add(GameManager.Instance.sprites["DungeonTileset_0"]);
        Wall.CornerInnerRightTop.Add(GameManager.Instance.sprites["DungeonTileset_5"]);
        Wall.CornerInnerLeftBottom.Add(GameManager.Instance.sprites["DungeonTileset_24"]);
        Wall.CornerInnerRightBottom.Add(GameManager.Instance.sprites["DungeonTileset_29"]);

        Wall.CornerOuterLeftTop.Add(GameManager.Instance.sprites["DungeonTileset_30"]);
        Wall.CornerOuterLeftTop.Add(GameManager.Instance.sprites["DungeonTileset_34"]);
        Wall.CornerOuterRightTop.Add(GameManager.Instance.sprites["DungeonTileset_33"]);
        Wall.CornerOuterRightTop.Add(GameManager.Instance.sprites["DungeonTileset_35"]);

        Wall.HorizontalTop.Add(GameManager.Instance.sprites["DungeonTileset_1"]);
        Wall.HorizontalTop.Add(GameManager.Instance.sprites["DungeonTileset_2"]);
        Wall.HorizontalTop.Add(GameManager.Instance.sprites["DungeonTileset_3"]);
        Wall.HorizontalTop.Add(GameManager.Instance.sprites["DungeonTileset_4"]);

        Wall.HorizontalBottom.Add(GameManager.Instance.sprites["DungeonTileset_25"]);
        Wall.HorizontalBottom.Add(GameManager.Instance.sprites["DungeonTileset_26"]);
        Wall.HorizontalBottom.Add(GameManager.Instance.sprites["DungeonTileset_27"]);
        Wall.HorizontalBottom.Add(GameManager.Instance.sprites["DungeonTileset_28"]);

        Wall.VerticalTop.Add(GameManager.Instance.sprites["DungeonTileset_42"]);

        Wall.VerticalLeft.Add(GameManager.Instance.sprites["DungeonTileset_6"]);
        Wall.VerticalLeft.Add(GameManager.Instance.sprites["DungeonTileset_12"]);
        Wall.VerticalLeft.Add(GameManager.Instance.sprites["DungeonTileset_18"]);

        Wall.VerticalSplit.Add(GameManager.Instance.sprites["DungeonTileset_36"]);
        Wall.VerticalSplit.Add(GameManager.Instance.sprites["DungeonTileset_37"]);
        Wall.VerticalSplit.Add(GameManager.Instance.sprites["DungeonTileset_38"]);
        Wall.VerticalSplit.Add(GameManager.Instance.sprites["DungeonTileset_39"]);

        Wall.VerticalRight.Add(GameManager.Instance.sprites["DungeonTileset_11"]);
        Wall.VerticalRight.Add(GameManager.Instance.sprites["DungeonTileset_17"]);
        Wall.VerticalRight.Add(GameManager.Instance.sprites["DungeonTileset_23"]);

        GameManager.Instance.OnClick += (Vector3 point) => {
            Debug.Log(point.ToString());
        };
    }

    public void CreateDungeon(int roomCount, int minRoomSize, int maxRoomSize)
    {
        Clear();

        data = new Data.Dungeon(roomCount, minRoomSize, maxRoomSize);
        tiles = new Tile[data.width * data.height];

        boxCollider.size = new Vector3(data.width, data.height);
        boxCollider.center = new Vector3(data.width / 2, data.height / 2);

        for (int i = 0; i < tiles.Length; i++)
        {
            var tileData = data.GetTile(i);
            Tile tile = null;
            if (Data.Tile.Type.Floor == tileData.type)
            {
                tile = new Floor(tileData);
            }

            if (Data.Tile.Type.Wall == tileData.type)
            {
                tile = new Wall(tileData);
            }

            if (null != tile)
            {
                tile.SetParent(tileRoot);
                tile.color = new Color(tile.color.r, tile.color.g, tile.color.b, 0.0f);
                tiles[i] = tile;
            }
        }

        foreach (var block in data.blocks)
        {
            Color color;
            if (Data.Block.Type.Room == block.type)
            {
                color = Color.red;
            }
            else
            {
                color = Color.blue;
            }

            DungeonGizmo.Block gizmo = new DungeonGizmo.Block($"Block_{block.index}", color, block.rect.width, block.rect.height);
            gizmo.sortingOrder = SortingOrder.Block;
            gizmo.SetPosition(new Vector3(block.rect.x, block.rect.y));
            blockGizmo.Add(gizmo);
        }

        foreach (var corridor in data.corridorGraph.corridors)
        {
            int from = Mathf.Min(corridor.p1.index, corridor.p2.index);
            int to = Mathf.Max(corridor.p1.index, corridor.p2.index);
            DungeonGizmo.Line line = new DungeonGizmo.Line($"Connection_{from}_{to}", Color.green, corridor.p1.rect.center, corridor.p2.rect.center, 0.5f);
            line.sortingOrder = SortingOrder.CorridorGraph;
            corridorGraphGizmo.Add(line);
        }

        foreach (var pathTiles in data.astarPathFindResults)
        {
            foreach (var tile in pathTiles)
            {
                DungeonGizmo.Point point = new DungeonGizmo.Point($"Path_{tile.index}", Color.white, 1.0f);
                point.SetPosition(new Vector3(tile.rect.x + 0.5f, tile.rect.y + 0.5f));
                point.sortingOrder = SortingOrder.CorridorPath;
                astarPathGizmo.Add(point);
            }
        }

        int startRoomIndex = UnityEngine.Random.Range(0, data.rooms.Count);
        start = data.rooms[startRoomIndex];

        player = CreatePlayer();
        
    }

    public void Clear()
    {
        if (null != tiles)
        {
            foreach (var tile in tiles)
            {
                if (null == tile)
                {
                    continue;
                }

                tile.gameObject.transform.parent = null;
                GameObject.DestroyImmediate(tile.gameObject);
            }

            tiles = null;
        }

        if (null != player)
        {
            player.transform.parent = null;
            GameObject.DestroyImmediate(player.gameObject);
        }

        blockGizmo.Clear();
        corridorGraphGizmo.Clear();
        astarPathGizmo.Clear();
        
        DungeonGizmo.ClearAll();
    }

    public void EnableGizmo()
    {
        foreach (var shape in blockGizmo)
        {
            shape.gameObject.SetActive(GameManager.Instance.showBlockGizmo);
        }

        foreach (var shape in corridorGraphGizmo)
        {
            shape.gameObject.SetActive(GameManager.Instance.showCorridorGraph);
        }

        foreach (var shape in astarPathGizmo)
        {
            shape.gameObject.SetActive(GameManager.Instance.showAstarPath);
        }

        tileRoot.gameObject.SetActive(GameManager.Instance.showTile);
    }

    public Player CreatePlayer()
    {
        GameObject go = new GameObject("Player");
        go.transform.parent = transform;
        var player = go.AddComponent<Player>();
        player.sightRange = GameManager.Instance.maxRoomSize;
        player.gizmo = new DungeonGizmo.Point("Sprite", Color.red, 1.0f);
        player.gizmo.sortingOrder = SortingOrder.Player;
        player.Move((int)start.rect.center.x, (int)start.rect.center.y);

        return player;
    }
}
