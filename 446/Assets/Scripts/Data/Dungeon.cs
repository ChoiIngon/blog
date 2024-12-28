using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    public class Tile
    {
        public class PathCost
        {
            public const int Default = 100;
            public const int Floor = 90;
            public const int Wall = 200;
            public const int PathCostDecrement = -50;
        }

        public enum Type
        {
            None,
            Floor,
            Wall
        }

        public int index = 0;
        public Type type = Type.None;
        public Rect rect;
        public int cost = 1;
    }

    public class Block
    {
        public const int MinSize = 5;
        public enum Type
        {
            None,
            Corridor,
            Room
        }

        public int index;
        public Type type;
        public Rect rect;
        public List<Block> neighbors = new List<Block>();

        public Block(int index, float x, float y, float width, float height)
        {
            this.index = index;
            this.type = Type.None;
            this.rect = new Rect(x, y, width, height);
        }
    }

    public class Dungeon
    {
        public List<Block> rooms;
        public List<Block> blocks;
        public TileMap tileMap;
        public CorridorGraph corridorGraph;

        public List<List<Tile>> astarPathFindResults = new List<List<Tile>>();

        public int width
        {
            get { return tileMap.width; }
        }

        public int height
        {
            get { return tileMap.height; }
        }

        public Dungeon(int roomCount, int minRoomSize, int maxRoomSize)
        {
            GenerateDungeon(roomCount, minRoomSize, maxRoomSize);
        }

        public Tile GetTile(int index)
        {
            return tileMap.GetTile(index);
        }

        public Tile GetTile(int x, int y)
        {
            return tileMap.GetTile(x, y);
        }

        public Block GetRoom(int x, int y)
        {
            foreach (Block room in rooms)
            {
                if (true == room.rect.Contains(new Vector2(x, y)))
                {
                    return room;
                }
            }
            return null;
        }

        public AStarPathFinder FindPath(Tile from, Tile to)
        {
            AStarPathFinder pathFinder = new AStarPathFinder(tileMap, tileMap.rect, new AStarPathFinder.RandomLookup());
            pathFinder.FindPath(from, to);
            return pathFinder;
        }

        public ShadowCast CastLight(int x, int y, int sightRange)
        {
            ShadowCast sight = new Data.ShadowCast(tileMap);
            sight.CastLight(x, y, sightRange);
            return sight;
        }

        private void GenerateDungeon(int roomCount, int minRoomSize, int maxRoomSize)
        {
            this.blocks = new List<Block>();
            this.rooms = new List<Block>();

            minRoomSize = Mathf.Min(maxRoomSize, minRoomSize);
            maxRoomSize = Mathf.Max(maxRoomSize, minRoomSize);
            minRoomSize = Mathf.Max(minRoomSize, Block.MinSize);
            maxRoomSize = Mathf.Max(maxRoomSize, Block.MinSize);
            
            var roomSizeWeightRandom = CreateRoomSizeWeightRandom(minRoomSize, maxRoomSize);
            var ratioWeightRandom = CreateRatioWeightRandom();

            CreateRooms(roomCount, minRoomSize, maxRoomSize, roomSizeWeightRandom, ratioWeightRandom);
            InsertCorridorBlocks(roomSizeWeightRandom);
            //MergeAdjacentWall();

            tileMap = new TileMap(blocks);
            corridorGraph = new CorridorGraph(this);

            GenerateCorridor(tileMap, corridorGraph);
            GenerateWall();
        }

        private void CreateRooms(int roomCount, int minRoomSize, int maxRoomSize, WeightRandom<int> roomSizeWeightRandom, WeightRandom<Tuple<float, float>> ratioWeightRandom)
        {
            // 방 블록의 넓이와 갯수에 따라 적절한 생성 영역 반지름 구하기
            float areaOfBlocks = maxRoomSize * maxRoomSize * roomCount;
            float roomCreateRadius = Mathf.Sqrt(areaOfBlocks / Mathf.PI);

            for (int i = 0; i < roomCount; i++)
            {
                float theta = 2.0f * Mathf.PI * UnityEngine.Random.Range(0.0f, 1.0f);   // https://kukuta.tistory.com/199
                float radius = UnityEngine.Random.Range(0.0f, roomCreateRadius);

                int x = (int)(radius * Mathf.Cos(theta));
                int y = (int)(radius * Mathf.Sin(theta));
                int width = 0;
                int height = 0;
                var range = ratioWeightRandom.Random();

                if (0 == UnityEngine.Random.Range(0, 100) % 2)
                {
                    width = roomSizeWeightRandom.Random();
                    height = (int)(width * UnityEngine.Random.Range(range.Item1, range.Item2));
                    height = Mathf.Max(minRoomSize, height);
                    height = Mathf.Min(maxRoomSize, height);
                }
                else
                {
                    height = roomSizeWeightRandom.Random();
                    width = (int)(height * UnityEngine.Random.Range(range.Item1, range.Item2));
                    width = Mathf.Max(minRoomSize, width);
                    width = Mathf.Min(maxRoomSize, width);
                }

                Block block = new Block(blocks.Count, x, y, width, height);
                block.type = Block.Type.Room;
                blocks.Add(block);
                rooms.Add(block);
            }

            RepositionBlocks();
        }

        private void InsertCorridorBlocks(WeightRandom<int> roomSizeWeightRandom)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                Block room = rooms[i];
                for (int j = i + 1; j < rooms.Count; j++)
                {
                    if (70 < UnityEngine.Random.Range(0, 100))
                    {
                        continue;
                    }

                    Block neighbor = rooms[j];

                    float distance = Vector3.Distance(room.rect.center, neighbor.rect.center);
                    float roomRadius = Vector3.Distance(room.rect.center, new Vector3(room.rect.x, room.rect.y));
                    float neighorRadius = Vector3.Distance(neighbor.rect.center, new Vector3(neighbor.rect.x, neighbor.rect.y));

                    if (distance < roomRadius + neighorRadius)
                    {
                        Vector3 interpolation = Vector3.Lerp(room.rect.center, neighbor.rect.center, 0.5f);
                        int width = roomSizeWeightRandom.Random() / 2;
                        int height = roomSizeWeightRandom.Random() / 2;
                        int x = (int)(interpolation.x - width / 2);
                        int y = (int)(interpolation.y - height / 2);

                        Block block = new Block(blocks.Count, x, y, width, height);
                        block.type = Block.Type.Corridor;
                        blocks.Add(block);
                    }
                }
            }

            RepositionBlocks();
        }

        private void MergeAdjacentWall()
        {
            // 인접한 블록들은 벽을 하나로 합쳐 버린다.
            for (int i = 0; i < rooms.Count; i++)
            {
                Block room = rooms[i];
                Rect extendRoomRect = new Rect(room.rect.x - 1, room.rect.y - 1, room.rect.width + 2, room.rect.height + 2);
                for (int j = i + 1; j < rooms.Count; j++)
                {
                    Block other = rooms[j];
                    if (false == extendRoomRect.Overlaps(other.rect))
                    {
                        continue;
                    }

                    if (other.rect.xMax == room.rect.xMin)
                    {
                        room.rect.xMin -= 1;
                    }

                    if (room.rect.xMax == other.rect.xMin)
                    {
                        room.rect.xMax += 1;
                    }

                    if (other.rect.yMax == room.rect.yMin)
                    {
                        room.rect.yMin -= 1;
                    }

                    if (room.rect.yMax == other.rect.yMin)
                    {
                        room.rect.yMax += 1;
                    }
                }
            }
        }

        private WeightRandom<Tuple<float, float>> CreateRatioWeightRandom()
        {
            var weightRandom = new WeightRandom<Tuple<float, float>>();
            int weight = 1;
            for (float rate = 3.0f; rate >= 0.5f; rate -= 0.1f)
            {
                var range = new Tuple<float, float>(rate - 0.1f, rate);
                weightRandom.AddElement(weight, range);
                if (1.0f < rate)
                {
                    weight++;
                }
                else
                {
                    weight--;
                }
            }

            return weightRandom;
        }

        private WeightRandom<int> CreateRoomSizeWeightRandom(int min, int max)
        {
            var weightRandom = new WeightRandom<int>();

            #region build room size random object // range의 가운데 값부터 바깥으로 점점 더 적은 확율을 가지도록 배치
            int delta = 0;
            int elmtCount = max - min + 1;
            if (0 == elmtCount % 2)
            {
                delta = 0;
                for (int i = (min + max) / 2; i >= min; i--)
                {
                    int weight = elmtCount / 2 - delta++;
                    weightRandom.AddElement(weight, i);
                }

                delta = 0;
                for (int i = (min + max) / 2 + 1; i <= max; i++)
                {
                    int weight = elmtCount / 2 - delta++;
                    weightRandom.AddElement(weight, i);
                }
            }
            else
            {
                delta = 0;
                for (int i = (min + max) / 2; i >= min; i--)
                {
                    int weight = elmtCount / 2 + 1 - delta++;
                    weightRandom.AddElement(weight, i);
                }

                delta = 1;
                for (int i = (min + max) / 2 + 1; i <= max; i++)
                {
                    int weight = elmtCount / 2 + 1 - delta++;
                    weightRandom.AddElement(weight, i);
                }
            }
            #endregion

            return weightRandom;
        }

        private void RepositionBlocks()
        {
            // 생성된 전체 블록들의 중심을 구한다.
            Vector2 center = Vector2.zero;
            foreach (Block block in blocks)
            {
                center += block.rect.center;
            }
            center /= blocks.Count;

            while (true)
            {
                bool overlap = false;

                for (int i = 0; i < blocks.Count; i++)
                {
                    for (int j = i + 1; j < blocks.Count; j++)
                    {
                        if (true == blocks[i].rect.Overlaps(blocks[j].rect))
                        {
                            ResolveOverlap(center, blocks[i], blocks[j]);
                            overlap = true;
                        }
                    }
                }

                if (false == overlap)
                {
                    break;
                }
            }
        }

        private void ResolveOverlap(Vector2 center, Block block1, Block block2)
        {
            int dx = (int)Mathf.Min(
                Mathf.Abs(block1.rect.x + block1.rect.width - block2.rect.x),
                Mathf.Abs(block2.rect.x + block2.rect.width - block1.rect.x)
            );
            int dy = (int)Mathf.Min(
                Mathf.Abs(block1.rect.y + block1.rect.height - block2.rect.y),
                Mathf.Abs(block2.rect.y + block2.rect.height - block1.rect.y)
            );

            // 거리가 가까운 축 기준으로 블록 이동
            if (dx < dy) // 두 블록이 x축으로 더 가까움
            {
                if (block1.rect.x < block2.rect.x) // 두 블록 중 block2가 오른쪽 있는 경우
                {
                    if (center.x < block2.rect.x)
                    {
                        block2.rect.x += 1; // block2가 중앙 보다 오른쪽에 있으면 block2를 오른쪽으로 1칸 이동
                    }
                    else
                    {
                        block1.rect.x -= 1; // block2가 중앙 보다 왼쪽에 있으면 block1을 왼쪽으로 1칸 이동
                    }
                }
                else // 두 블록 중 block1이 오른쪽 있는 경우
                {
                    if (center.x < block1.rect.x)
                    {
                        block1.rect.x += 1; // block1이 중앙 보다 오른쪽에 있으면 block1를 오른쪽으로 1칸 이동
                    }
                    else
                    {
                        block2.rect.x -= 1; // block1가 중앙 보다 왼쪽에 있으면 block2를 왼쪽으로 1칸 이동
                    }
                }
            }
            else // 두 블록이 y으로 더 가까움
            {
                if (block1.rect.y < block2.rect.y)
                {
                    if (center.y < block2.rect.y)
                    {
                        block2.rect.y += 1; // block2가 중앙 보다 위에 있으면 block2를 윗쪽으로 1칸 이동
                    }
                    else
                    {
                        block1.rect.y -= 1; // block2가 중앙 보다 아래에 있으면 block1을 아래로 1칸 이동
                    }
                }
                else
                {
                    if (center.y < block1.rect.y)
                    {
                        block1.rect.y += 1; // block1이 중앙 보다 위에 있으면 block1을 위로 1칸 이동
                    }
                    else
                    {
                        block2.rect.y -= 1;  // block1이 중앙 보다 아래에 있으면 block2를 아래로 1칸 이동
                    }
                }
            }
        }

        private void GenerateCorridor(TileMap tileMap, CorridorGraph corridorGraph)
        {
            foreach (var corridor in corridorGraph.corridors)
            {
                Block src = corridor.p1;
                Block dest = corridor.p2;

                Tile from = tileMap.GetTile((int)src.rect.center.x, (int)src.rect.center.y);
                Tile to = tileMap.GetTile((int)dest.rect.center.x, (int)dest.rect.center.y);

                Rect searchLimitBoundary = new Rect();

                searchLimitBoundary.xMin = Mathf.Min(src.rect.xMin, dest.rect.xMin);
                searchLimitBoundary.xMax = Mathf.Max(src.rect.xMax, dest.rect.xMax);
                searchLimitBoundary.yMin = Mathf.Min(src.rect.yMin, dest.rect.yMin);
                searchLimitBoundary.yMax = Mathf.Max(src.rect.yMax, dest.rect.yMax);

                foreach (Block neighbor in src.neighbors)
                {
                    searchLimitBoundary.xMin = Mathf.Min(searchLimitBoundary.xMin, neighbor.rect.xMin);
                    searchLimitBoundary.xMax = Mathf.Max(searchLimitBoundary.xMax, neighbor.rect.xMax);
                    searchLimitBoundary.yMin = Mathf.Min(searchLimitBoundary.yMin, neighbor.rect.yMin);
                    searchLimitBoundary.yMax = Mathf.Max(searchLimitBoundary.yMax, neighbor.rect.yMax);
                }

                foreach (Block neighbor in dest.neighbors)
                {
                    searchLimitBoundary.xMin = Mathf.Min(searchLimitBoundary.xMin, neighbor.rect.xMin);
                    searchLimitBoundary.xMax = Mathf.Max(searchLimitBoundary.xMax, neighbor.rect.xMax);
                    searchLimitBoundary.yMin = Mathf.Min(searchLimitBoundary.yMin, neighbor.rect.yMin);
                    searchLimitBoundary.yMax = Mathf.Max(searchLimitBoundary.yMax, neighbor.rect.yMax);
                }

                AStarPathFinder pathFinder = new AStarPathFinder(tileMap, searchLimitBoundary, new AStarPathFinder.StraightLookup());
                var path = pathFinder.FindPath(from, to);

                astarPathFindResults.Add(path);

                { // 구해지 경로 중 블록과 겹치는 부분을 뺀 실제 복도 타일만 구한다

                    // src 블록의 벽을 뺀 나머지 영역
                    Rect floorAreaOfSrc = new Rect(src.rect.xMin + 1, src.rect.yMin + 1, src.rect.width - 2, src.rect.height - 2);
                    // dest 블록의 벽을 뺀 나머지 영역
                    Rect floorAreaOfDest = new Rect(dest.rect.xMin + 1, dest.rect.yMin + 1, dest.rect.width - 2, dest.rect.height - 2);
                    foreach (var tile in path)
                    {
                        tile.cost += Tile.PathCost.PathCostDecrement;
                        tile.cost = Mathf.Max(1, tile.cost);

                        if (true == floorAreaOfSrc.Contains(new Vector2(tile.rect.x, tile.rect.y)) || true == floorAreaOfDest.Contains(new Vector2(tile.rect.x, tile.rect.y)))
                        {
                            continue;
                        }

                        corridor.tiles.Add(tile);
                    }
                }
            }
        }

        private void GenerateWall()
        {
            foreach (Block room in rooms)
            {
                for (int y = (int)room.rect.yMin; y < (int)room.rect.yMax - 1; y++)
                {
                    for (int x = (int)room.rect.xMin; x < (int)room.rect.xMax - 1; x++)
                    {
                        Tile tile = tileMap.GetTile(x, y);
                        tile.type = Tile.Type.Floor;
                        tile.cost = Tile.PathCost.Floor;
                    }
                }

                for (int y = (int)room.rect.yMin; y < (int)room.rect.yMax - 1; y++)
                {
                    Tile left = tileMap.GetTile((int)room.rect.xMin, y);
                    left.type = Tile.Type.Wall;
                    left.cost = Tile.PathCost.Wall;

                    Tile right = tileMap.GetTile((int)room.rect.xMax - 1, y);
                    right.type = Tile.Type.Wall;
                    right.cost = Tile.PathCost.Wall;
                }

                for (int x = (int)room.rect.xMin; x < (int)room.rect.xMax - 1; x++)
                {
                    Tile up = tileMap.GetTile(x, (int)room.rect.yMin);
                    up.type = Tile.Type.Wall;
                    up.cost = Tile.PathCost.Wall;

                    Tile down = tileMap.GetTile(x, (int)room.rect.yMax - 1);
                    down.type = Tile.Type.Wall;
                    down.cost = Tile.PathCost.Wall;
                }
            }

            foreach (var corridor in corridorGraph.corridors)
            {
                foreach (var tile in corridor.tiles)
                {
                    tile.type = Tile.Type.Floor;
                    tile.cost = Tile.PathCost.Floor;
                }
            }

            Action<int, int> IfNotNullBuildWall = (int x, int y) =>
            {
                Tile tile = tileMap.GetTile(x, y);
                if (null == tile)
                {
                    return;
                }

                if (Tile.Type.None != tile.type)
                {
                    return;
                }

                tile.type = Tile.Type.Wall;
                tile.cost = Tile.PathCost.Wall;
            };

            foreach (var corridor in corridorGraph.corridors)
            {
                foreach (var tile in corridor.tiles)
                {
                    int x = (int)tile.rect.x;
                    int y = (int)tile.rect.y;

                    IfNotNullBuildWall(x - 1, y - 1);
                    IfNotNullBuildWall(x - 1, y);
                    IfNotNullBuildWall(x - 1, y + 1);
                    IfNotNullBuildWall(x, y - 1);
                    IfNotNullBuildWall(x, y + 1);
                    IfNotNullBuildWall(x + 1, y - 1);
                    IfNotNullBuildWall(x + 1, y);
                    IfNotNullBuildWall(x + 1, y + 1);
                }
            }
        }
    }
}