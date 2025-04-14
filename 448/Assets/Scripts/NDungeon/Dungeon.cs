using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using static NDungeon.Gizmo;

namespace NDungeon
{
    public class Dungeon : MonoBehaviour
    {
        private const float TileSize = 4.0f; 

        private NTileMap.TileMap tileMap;
        public int roomCount;
        public int minRoomSize;
        public int maxRoomSize;
        public int randomSeed;

        public GameObject[] FloorPrefab;
        public GameObject[] WallStraightPrefab;
        public GameObject[] WallCrossPrefab;
        public GameObject[] WallCornerPrefab;
        public GameObject[] WallTSplitPrefab;
        public GameObject[] Doors;

        private Transform tileRoot;
        private Dictionary<int, Tile> tiles = new Dictionary<int, Tile>();

        private void Start()
        {
            tileRoot = new GameObject("TileRoot").transform;
            tileRoot.SetParent(transform, false);
            tileRoot.localPosition = Vector3.zero;
            tileRoot.localRotation = Quaternion.identity;
            tileRoot.localScale = Vector3.one;
            Generate();
        }

        public void Generate()
        {
            NDungeon.Gizmo.Clear();
            tiles.Clear();
            while (0 < tileRoot.childCount)
            {
                var child = tileRoot.GetChild(0);
                child.parent = null;
                GameObject.DestroyImmediate(child.gameObject);
            }

            if (0 == randomSeed)
            {
                randomSeed = (int)System.DateTime.Now.Ticks;
            }

            UnityEngine.Debug.Log($"Dungeon data generation process starts(random_seed:{randomSeed})");
            Random.InitState(randomSeed);
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            tileMap = new NTileMap.TileMap(roomCount, minRoomSize, maxRoomSize);
            stopWatch.Stop();
            UnityEngine.Debug.Log($"Dungeon data generation is complete(elapsed_time:{stopWatch.Elapsed})");
            
            if(0 == tileMap.rooms.Count)
            {
                UnityEngine.Debug.LogError("Dungeon data generation failed.");
                return;
            }

            {
                var room = tileMap.rooms[Random.Range(0, tileMap.rooms.Count)];
                var floorRect = room.GetFloorRect();
                int x = (int)Random.Range(floorRect.xMin, floorRect.xMax);
                int y = (int)Random.Range(floorRect.yMin, floorRect.yMax);
                GameManager.Instance.player.position = new Vector3(x, 0.0f, y);
            }

            for (int i = 0; i < tileMap.width * tileMap.height; i++)
            {
                var tileData = tileMap.GetTile(i);
                if (null == tileData)
                {
                    continue;
                }

                GameObject go = new GameObject($"Tile_{tileData.index}");
                go.transform.SetParent(tileRoot, false);
                go.transform.localPosition = new Vector3(tileData.rect.x, 0.0f, tileData.rect.y);

                Tile tile = go.AddComponent<Tile>();
                tile.data = tileData;
                tiles.Add(tile.index, tile);
            }

            foreach (var itr in tiles)
            {
                Tile tile = itr.Value;
                for (int i = 0; i < NTileMap.TileMap.Tile.Direction.Max; i++)
                {
                    var neighborData = tile.data.neighbors[i];
                    if (null == neighborData)
                    {
                        continue;
                    }

                    var neighbor = GetTile(neighborData.index);
                    tile.neighbors[i] = neighbor;
                }
            }

            foreach (var itr in tiles)
            {
                Tile tile = itr.Value;

                GameObject floor = GameObject.Instantiate(FloorPrefab[0], Vector3.zero, Quaternion.identity);
                floor.name = $"Floor_{tile.index}";
                floor.transform.localScale = new Vector3(1.0f / TileSize, 1.0f / TileSize, 1.0f / TileSize);
                floor.transform.SetParent(tile.transform, false);

                if (NTileMap.TileMap.Tile.Type.Wall == tile.type)
                {
                    CreateWall(tile);
                }
            }

            foreach (var corridor in tileMap.corridors)
            {
                foreach (var tileData in corridor.tiles)
                {
                    var tile = GetTile(tileData.index);
                    var meshRenderer = tile.gameObject.GetComponentInChildren<MeshRenderer>();
                    meshRenderer.material.color = Color.red;
                }
            }
        }

        public GameObject CreateWall(Tile tile)
        {
            var leftTop     = tile.neighbors[NTileMap.TileMap.Tile.Direction.LeftTop];
            var top         = tile.neighbors[NTileMap.TileMap.Tile.Direction.Top];
            var rightTop    = tile.neighbors[NTileMap.TileMap.Tile.Direction.RightTop];
            var left        = tile.neighbors[NTileMap.TileMap.Tile.Direction.Left];
            var right       = tile.neighbors[NTileMap.TileMap.Tile.Direction.Right];
            var leftBottom  = tile.neighbors[NTileMap.TileMap.Tile.Direction.LeftBottom];
            var bottom      = tile.neighbors[NTileMap.TileMap.Tile.Direction.Bottom];
            var rightBottom = tile.neighbors[NTileMap.TileMap.Tile.Direction.RightBottom];

            GameObject wall = null;
            if (true == IsWall(top) && true == IsWall(left) && true == IsWall(right) && true == IsWall(bottom))
            {
                wall = GameObject.Instantiate(WallCrossPrefab[0], Vector3.zero, Quaternion.Euler(0, 0, 0));
                wall.name = $"Wall_{tile.index}_¦«";
            }

            if (true == IsWall(top) && false == IsWall(left) && true == IsWall(right) && false == IsWall(bottom))
            {
                wall = GameObject.Instantiate(WallCornerPrefab[0], Vector3.zero, Quaternion.Euler(0, 0, 0));
                wall.name = $"Wall_{tile.index}_¦¦";
            }

            if (false == IsWall(top) && false == IsWall(left) && true == IsWall(right) && true == IsWall(bottom))
            {
                wall = GameObject.Instantiate(WallCornerPrefab[0], Vector3.zero, Quaternion.Euler(0, 90, 0));
                wall.name = $"Wall_{tile.index}_¦£";
            }

            if (false == IsWall(top) && true == IsWall(left) && false == IsWall(right) && true == IsWall(bottom))
            {
                wall = GameObject.Instantiate(WallCornerPrefab[0], Vector3.zero, Quaternion.Euler(0, 180, 0));
                wall.name = $"Wall_{tile.index}_¦¤";
            }

            if (true == IsWall(top) && true == IsWall(left) && false == IsWall(right) && false == IsWall(bottom))
            {
                wall = GameObject.Instantiate(WallCornerPrefab[0], Vector3.zero, Quaternion.Euler(0, 270, 0));
                wall.name = $"Wall_{tile.index}_¦¥";
            }

            if (false == IsWall(top) && true == IsWall(left) && true == IsWall(right) && false == IsWall(bottom))
            {
                wall = GameObject.Instantiate(WallStraightPrefab[0], Vector3.zero, Quaternion.Euler(0, 0, 0));
                wall.name = $"Wall_{tile.index}_¦¡";
            }

            if (true == IsWall(top) && false == IsWall(left) && false == IsWall(right) && true == IsWall(bottom))
            {
                wall = GameObject.Instantiate(WallStraightPrefab[0], Vector3.zero, Quaternion.Euler(0, 90, 0));
                wall.name = $"Wall_{tile.index}_¦¢";
            }

            if (true == IsWall(top) && true == IsWall(left) && true == IsWall(right) && false == IsWall(bottom))
            {
                wall = GameObject.Instantiate(WallTSplitPrefab[0], Vector3.zero, Quaternion.Euler(0, 0, 0));
                wall.name = $"Wall_{tile.index}_¦ª";
            }

            if (true == IsWall(top) && false == IsWall(left) && true == IsWall(right) && true == IsWall(bottom))
            {
                wall = GameObject.Instantiate(WallTSplitPrefab[0], Vector3.zero, Quaternion.Euler(0, 90, 0));
                wall.name = $"Wall_{tile.index}_¦§";
            }

            if (false == IsWall(top) && true == IsWall(left) && true == IsWall(right) && true == IsWall(bottom))
            {
                wall = GameObject.Instantiate(WallTSplitPrefab[0], Vector3.zero, Quaternion.Euler(0, 180, 0));
                wall.name = $"Wall_{tile.index}_¦¨";
            }

            if (true == IsWall(top) && true == IsWall(left) && false == IsWall(right) && true == IsWall(bottom))
            {
                wall = GameObject.Instantiate(WallTSplitPrefab[0], Vector3.zero, Quaternion.Euler(0, 270, 0));
                wall.name = $"Wall_{tile.index}_¦©";
            }

            if(null == wall)
            {
                return null;
            }

            wall.transform.localScale = new Vector3(1.0f / TileSize, 2.0f / TileSize, 1.0f / TileSize);
            wall.transform.SetParent(tile.transform, false);
            return wall;
        }

        private bool IsWall(Tile tile)
        {
            if (null == tile)
            {
                return false;
            }
            if (NTileMap.TileMap.Tile.Type.Wall != tile.type)
            {
                return false;
            }

            return true;
        }

        private bool IsFloor(Tile tile)
        {
            if (null == tile)
            {
                return false;
            }
            if (NDungeon.NTileMap.TileMap.Tile.Type.Floor != tile.type)
            {
                return false;
            }
            return true;
        }

        public Tile GetTile(int index)
        {
            if(false == tiles.TryGetValue(index, out Tile tile))
            {
                return null;
            }

            return tile;
        }

        public Tile GetTile(int x, int y)
        {
            if (0 > x || x >= tileMap.width)
            {
                return null;
            }

            if (0 > y || y >= tileMap.height)
            {
                return null;
            }

            return GetTile(y * tileMap.width + x);
        }

        public List<Tile> FindPath(Tile from, Tile to)
        {
            var tileDatas = tileMap.FindPath(from.data, to.data);
            List<Tile> path = new List<Tile>();
            foreach (NTileMap.TileMap.Tile data in tileDatas)
            {
                var tile = GetTile(data.index);
                path.Add(tile);
            }

            return path;
        }
    }
}