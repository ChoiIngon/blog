using System.Collections.Generic;
using UnityEngine;

public class Dungeon : MonoBehaviour
{
    public static class SortingOrder
    {
        public static int Block = 0;
        public static int CorridorGraph = 10;
        public static int CorridorPath = 20;
        public static int CorridorCost = 21;

        public static int Tile = 100;
        public static int Gimmick = 110;
        public static int Player = 120;
    }
        
    public class Tile
    {
        public readonly GameObject gameObject;
        
        protected SpriteRenderer spriteRenderer;
        public Gimmick gimmick;

        public Tile(Data.Tile data)
        {
            this.gameObject = new GameObject($"Tile_{data.index}");
            this.gameObject.transform.position = new Vector3(data.rect.x + 0.5f, data.rect.y + 0.5f);
            this.spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            this.spriteRenderer.sortingOrder = SortingOrder.Tile;
        }

        public void SetParent(Transform transform)
        {
            gameObject.transform.parent = transform;
        }

        public void Visible(bool flag)
        {
            float alpha = 1.0f;
            if (false == flag)
            {
                alpha = 0.5f;
            }

            color = new Color(color.r, color.g, color.b, alpha);

            if (null != gimmick)
            {
                gimmick.Visible(flag);
            }
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

            return GetRandomSprite(HorizontalTop);
        }
    }

    public class Gimmick
    {
        public readonly GameObject gameObject;
        protected SpriteRenderer spriteRenderer;

        public Gimmick(Data.Tile tile)
        {
            this.gameObject = new GameObject($"Gimmick_{tile.index}");
            this.gameObject.transform.position = new Vector3(tile.rect.x + 0.5f, tile.rect.y + 0.5f);

            this.spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            this.spriteRenderer.sortingOrder = SortingOrder.Gimmick;
        }

        public void SetParent(Transform transform)
        {
            this.gameObject.transform.parent = transform;
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

        public void Visible(bool flag)
        {
            float alpha = 1.0f;
            if (false == flag)
            {
                alpha = 0.5f;
            }

            color = new Color(color.r, color.g, color.b, alpha);
        }
    }

    public class Bone : Gimmick
    {
        public static List<Sprite> Sprites = new List<Sprite>();

        public Bone(Data.Tile tile) : base(tile)
        {
            gameObject.name = $"Bone_{tile.index}";
            spriteRenderer.sprite = GetRandomSprite(Sprites);
            color = new Color(color.r, color.g, color.b, 0.0f);
        }
    }

    public class Shackle : Gimmick
    {
        public static List<Sprite> Sprites = new List<Sprite>();

        public Shackle(Data.Tile tile) : base(tile)
        {
            gameObject.name = $"Shackle_{tile.index}";
            spriteRenderer.sprite = GetRandomSprite(Sprites);
            color = new Color(color.r, color.g, color.b, 0.0f);
        }
    }

    public Data.Dungeon data;
    public Data.Block start;
    public Player player;

    public Transform tileRoot;
    public Transform gimmickRoot;
    public Tile[] tiles;

    private BoxCollider boxCollider;

    public List<DungeonGizmo.Gizmo> blockGizmo = new List<DungeonGizmo.Gizmo>();
    public List<DungeonGizmo.Gizmo> corridorGraphGizmo = new List<DungeonGizmo.Gizmo>();
    public List<DungeonGizmo.Gizmo> astarCostGizmo = new List<DungeonGizmo.Gizmo>();
    public List<DungeonGizmo.Gizmo> astarPathGizmo = new List<DungeonGizmo.Gizmo>();
    public List<DungeonGizmo.Gizmo> tileGizmo = new List<DungeonGizmo.Gizmo>();

    private void Start()
    {
        GameObject tileRootGameObject = new GameObject("TileRoot");
        tileRootGameObject.transform.parent = transform;
        this.tileRoot = tileRootGameObject.transform;

        GameObject gimmickGameObject = new GameObject("GimmickRoot");
        gimmickGameObject.transform.parent = transform;
        this.gimmickRoot = gimmickGameObject.transform;

        boxCollider = gameObject.AddComponent<BoxCollider>();
        gameObject.AddComponent<CameraDrag>();
        gameObject.AddComponent<CameraScale>();

        LoadSprite();
    }

    public void CreateDungeon(int roomCount, int minRoomSize, int maxRoomSize)
    {
        Clear();

        data = new Data.Dungeon(roomCount, minRoomSize, maxRoomSize);
        tiles = new Tile[data.width * data.height];

        boxCollider.size = new Vector3(data.width, data.height);
        boxCollider.center = new Vector3(data.width / 2, data.height / 2);

        InitTile();
        IniitGimmick();
        InitGizmo();

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

        while (0 < gimmickRoot.childCount)
        {
            Transform gimmick = gimmickRoot.GetChild(0);
            gimmick.parent = null;
            GameObject.Destroy(gimmick.gameObject);
        }

        blockGizmo.Clear();
        corridorGraphGizmo.Clear();
        astarPathGizmo.Clear();
        astarCostGizmo.Clear();

        DungeonGizmo.ClearAll();
    }

    private void InitTile()
    {
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
    }
    private void IniitGimmick()
    {
        Data.WeightRandom<int> gimmickCountWeightRandom = new Data.WeightRandom<int>();
        gimmickCountWeightRandom.AddElement(2, 2);
        gimmickCountWeightRandom.AddElement(1, 1);
        gimmickCountWeightRandom.AddElement(1, 3);

        foreach (var room in data.rooms)
        {
            if (30.0f < UnityEngine.Random.Range(0, 100))
            {
                continue;
            }

            int gimmickCount = gimmickCountWeightRandom.Random();

            for (int i = 0; i < gimmickCount; i++)
            {
                int x = UnityEngine.Random.Range((int)room.rect.xMin + 1, (int)room.rect.xMax - 2);
                int y = UnityEngine.Random.Range((int)room.rect.yMin + 1, (int)room.rect.yMax - 2);

                var tileData = data.GetTile(x, y);
                if (null == tileData)
                {
                    continue;
                }

                var tile = tiles[tileData.index];
                if (null != tile.gimmick)
                {
                    continue;
                }

                Bone bone = new Bone(tileData);
                tile.gimmick = bone;
                bone.SetParent(gimmickRoot);
            }
        }

        foreach (var room in data.rooms)
        {
            if (30.0f < UnityEngine.Random.Range(0, 100))
            {
                continue;
            }

            int gimmickCount = gimmickCountWeightRandom.Random();

            for (int i = 0; i < gimmickCount; i++)
            {
                int x = UnityEngine.Random.Range((int)room.rect.xMin + 1, (int)room.rect.xMax - 2);
                int y = (int)room.rect.yMax - 1;

                var tileData = data.GetTile(x, y);
                if (null == tileData)
                {
                    continue;
                }

                if (Data.Tile.Type.Wall != tileData.type)
                {
                    continue;
                }

                var tile = tiles[tileData.index];
                if (null != tile.gimmick)
                {
                    continue;
                }

                Shackle shackle = new Shackle(tileData);
                tile.gimmick = shackle;
                shackle.SetParent(gimmickRoot);
            }
        }
    }
    private void InitGizmo()
    {
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

        foreach (int tileIndex in data.astarPathTiles)
        {
            var tile = data.tileMap.GetTile(tileIndex);
            DungeonGizmo.Point point = new DungeonGizmo.Point($"Path_{tile.index}", Color.white, 1.0f);
            point.SetPosition(new Vector3(tile.rect.x + 0.5f, tile.rect.y + 0.5f));
            point.sortingOrder = SortingOrder.CorridorPath;
            astarPathGizmo.Add(point);
        }

        for (int tileIndex = 0; tileIndex < data.tileMap.width * data.tileMap.height; tileIndex++)
        {
            var tile = data.tileMap.GetTile(tileIndex);
            float cost = data.astarPathCost[tileIndex];

            DungeonGizmo.Point point = new DungeonGizmo.Point($"Cost_{tile.index}", new Color(1.0f, 1.0f - cost / Data.Tile.PathCost.MaxCost, 0.0f), 1.0f);
            point.SetPosition(new Vector3(tile.rect.x + 0.5f, tile.rect.y + 0.5f));
            point.sortingOrder = SortingOrder.CorridorCost;
            astarCostGizmo.Add(point);
        }
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

        foreach (var shape in astarCostGizmo)
        {
            shape.gameObject.SetActive(GameManager.Instance.showAstarCost);
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

    private void LoadSprite()
    {
        Floor.CornerInnerLeftBottom.Add(GameManager.Instance.sprites["Floor.CornerInnerLeftBottom_1"]);
        Floor.CornerInnerLeftTop.Add(GameManager.Instance.sprites["Floor.CornerInnerLeftTop_1"]);
        Floor.CornerInnerRightBottom.Add(GameManager.Instance.sprites["Floor.CornerInnerRightBottom_1"]);
        Floor.CornerInnerRightTop.Add(GameManager.Instance.sprites["Floor.CornerInnerRightTop_1"]);
        Floor.HorizontalBottom.Add(GameManager.Instance.sprites["Floor.HorizontalBottom_1"]);
        Floor.HorizontalBottom.Add(GameManager.Instance.sprites["Floor.HorizontalBottom_2"]);
        Floor.HorizontalTop.Add(GameManager.Instance.sprites["Floor.HorizontalTop_1"]);
        Floor.HorizontalTop.Add(GameManager.Instance.sprites["Floor.HorizontalTop_2"]);
        Floor.InnerNormal.Add(GameManager.Instance.sprites["Floor.InnerNormal_1"]);
        Floor.InnerNormal.Add(GameManager.Instance.sprites["Floor.InnerNormal_2"]);
        Floor.VerticalLeft.Add(GameManager.Instance.sprites["Floor.VerticalLeft_1"]);
        Floor.VerticalRight.Add(GameManager.Instance.sprites["Floor.VerticalRight_1"]);

        Wall.CornerInnerLeftBottom.Add(GameManager.Instance.sprites["Wall.CornerInnerLeftBottom_1"]);
        Wall.CornerInnerLeftTop.Add(GameManager.Instance.sprites["Wall.CornerInnerLeftTop_1"]);
        Wall.CornerInnerRightBottom.Add(GameManager.Instance.sprites["Wall.CornerInnerRightBottom_1"]);
        Wall.CornerInnerRightTop.Add(GameManager.Instance.sprites["Wall.CornerInnerRightTop_1"]);

        Wall.CornerOuterLeftTop.Add(GameManager.Instance.sprites["Wall.CornerOuterLeftTop_1"]);
        Wall.CornerOuterLeftTop.Add(GameManager.Instance.sprites["Wall.CornerOuterLeftTop_2"]);
        Wall.CornerOuterRightTop.Add(GameManager.Instance.sprites["Wall.CornerOuterRightTop_1"]);
        Wall.CornerOuterRightTop.Add(GameManager.Instance.sprites["Wall.CornerOuterRightTop_2"]);

        Wall.HorizontalBottom.Add(GameManager.Instance.sprites["Wall.HorizontalBottom_1"]);
        Wall.HorizontalBottom.Add(GameManager.Instance.sprites["Wall.HorizontalBottom_2"]);
        Wall.HorizontalBottom.Add(GameManager.Instance.sprites["Wall.HorizontalBottom_3"]);
        Wall.HorizontalBottom.Add(GameManager.Instance.sprites["Wall.HorizontalBottom_4"]);

        Wall.HorizontalTop.Add(GameManager.Instance.sprites["Wall.HorizontalTop_1"]);
        Wall.HorizontalTop.Add(GameManager.Instance.sprites["Wall.HorizontalTop_2"]);
        Wall.HorizontalTop.Add(GameManager.Instance.sprites["Wall.HorizontalTop_3"]);
        Wall.HorizontalTop.Add(GameManager.Instance.sprites["Wall.HorizontalTop_4"]);

        Wall.VerticalLeft.Add(GameManager.Instance.sprites["Wall.VerticalLeft_1"]);
        Wall.VerticalLeft.Add(GameManager.Instance.sprites["Wall.VerticalLeft_2"]);
        Wall.VerticalLeft.Add(GameManager.Instance.sprites["Wall.VerticalLeft_3"]);

        Wall.VerticalRight.Add(GameManager.Instance.sprites["Wall.VerticalRight_1"]);
        Wall.VerticalRight.Add(GameManager.Instance.sprites["Wall.VerticalRight_2"]);
        Wall.VerticalRight.Add(GameManager.Instance.sprites["Wall.VerticalRight_3"]);

        Wall.VerticalSplit.Add(GameManager.Instance.sprites["Wall.VerticalSplit_1"]);
        Wall.VerticalSplit.Add(GameManager.Instance.sprites["Wall.VerticalSplit_2"]);
        Wall.VerticalSplit.Add(GameManager.Instance.sprites["Wall.VerticalSplit_3"]);
        Wall.VerticalSplit.Add(GameManager.Instance.sprites["Wall.VerticalSplit_4"]);

        Wall.VerticalTop.Add(GameManager.Instance.sprites["Wall.VerticalTop_1"]);

        Bone.Sprites.Add(GameManager.Instance.sprites["Bone_1"]);
        Bone.Sprites.Add(GameManager.Instance.sprites["Bone_2"]);

        Shackle.Sprites.Add(GameManager.Instance.sprites["Shackle_1"]);
        Shackle.Sprites.Add(GameManager.Instance.sprites["Shackle_2"]);
    }

    private static Sprite GetRandomSprite(List<Sprite> sprites)
    {
        if (0 == sprites.Count)
        {
            return null;
        }

        return sprites[UnityEngine.Random.Range(0, sprites.Count)];
    }

    private static Data.Tile GetTile(int x, int y)
    {
        return GameManager.Instance.dungeon.data.GetTile(x, y);
    }

    private static Data.Tile GetTile(int index)
    {
        return GameManager.Instance.dungeon.data.GetTile(index);
    }

    public class CameraScale : MonoBehaviour
    {
        private const float mouseWheelSpeed = 10.0f;
        private const float minFieldOfView = 20.0f;
        private const float maxFieldOfView = 120.0f;

        private void Update()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel") * mouseWheelSpeed;
            if (Camera.main.fieldOfView < minFieldOfView && scroll < 0.0f)
            {
                Camera.main.fieldOfView = minFieldOfView;
            }
            else if (Camera.main.fieldOfView > maxFieldOfView && scroll > 0.0f)
            {
                Camera.main.fieldOfView = maxFieldOfView;
            }
            else
            {
                Camera.main.fieldOfView -= scroll;
            }
        }
    }

    public class CameraDrag : MonoBehaviour 
    {
        private const float dragSpeed = 2;
        private Vector3 dragOrigin;

        private void Update()
        {
            if (true == Input.GetMouseButtonDown(1))
            {
                dragOrigin = Input.mousePosition;
                return;
            }

            if (false == Input.GetMouseButton(1))
            {
                return;
            }

            Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
            Vector3 move = new Vector3(pos.x * dragSpeed, pos.y * dragSpeed, 0.0f);

            Camera.main.transform.Translate(-move, Space.World);
        }
    }
}
