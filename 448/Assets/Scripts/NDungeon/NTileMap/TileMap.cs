using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.WSA;
using static NDungeon.NTileMap.TileMap;

namespace NDungeon.NTileMap
{
    public class TileMap
    {
        public class Tile
        {
            public enum Type
            {
                None,
                Floor,
                Wall
            }

            public static class Direction
            {
                /*
                LeftTop,    Top,    RightTop,
                Left,               Right,
                LeftBottom, Bottom, RightBottom,
                Max
                */
                public const int LeftTop = 0;
                public const int Top = 1;
                public const int RightTop = 2;
                public const int Left = 3;
                public const int Right = 4;
                public const int LeftBottom = 5;
                public const int Bottom = 6;
                public const int RightBottom = 7;
                public const int Max = 8;
            }

            public class PathCost
            {
                public const int MinCost = 1;
                public const int Default = 10;
                public const int Corridor = 64;
                public const int Floor = 128;
                public const int Wall = 255;
                public const int MaxCost = 255;
            }

            public readonly int index;
            public Rect rect;
            public Type type;
            public int cost;
            public Room room;
            public Tile[] neighbors;

            public static Vector3[] DirectionOffsets = new Vector3[] {
                new Vector3(-1, +1),
                new Vector3(0, +1),
                new Vector3(+1, +1),
                new Vector3(-1, 0),
                new Vector3(+1, 0),
                new Vector3(-1, -1),
                new Vector3(0, -1),
                new Vector3(+1, -1),
            };

            public Tile(int index)
            {
                this.index = index;
            }
        }

        public class Room
        {
            public readonly int index;
            public Rect rect;
            public List<Room> neighbors;
            public List<Tile> doors;

            public Room(int index, float x, float y, float width, float height)
            {
                this.index = index;
                this.rect = new Rect(x, y, width, height);
                this.neighbors = new List<Room>();
                this.doors = new List<Tile>();
            }

            public Vector3 position
            {
                get => new Vector3(this.rect.x, this.rect.y, 0.0f);
            }

            public float x
            {
                set => this.rect.x = value;
                get => this.rect.x;
            }

            public float y
            {
                set => this.rect.y = value;
                get => this.rect.y;
            }

            public Vector2 center
            {
                get => rect.center;
            }

            public Rect GetFloorRect()
            {
                Rect floorRect = new Rect(rect.x, rect.y, rect.width, rect.height);
                floorRect.xMin += 1;
                floorRect.xMax -= 1;
                floorRect.yMin += 1;
                floorRect.yMax -= 1;
                return floorRect;
            }
        }

        public class Corridor
        {
            public List<Tile> tiles = new List<Tile>();
        }

        private const int MinRoomSize = 5;
        public Tile[] tiles { get; private set; }
        public List<Room> rooms { get; private set; }
        public List<Corridor> corridors { get; private set; }
        private int roomCount;
        private int minRoomSize;
        private int maxRoomSize;
        private Rect rect;

        private static WeightRandom<int> RandomDepthCount = new WeightRandom<int>(new List<KeyValuePair<int, int>>()
        {
            new KeyValuePair<int, int>(1, 4),
            new KeyValuePair<int, int>(2, 3),
            new KeyValuePair<int, int>(3, 2),
            new KeyValuePair<int, int>(4, 1)
        });

        public int width
        {
            get => (int)rect.width;
        }

        public int height
        {
            get => (int)rect.height;
        }

        public TileMap(int roomCount, int minRoomSize, int maxRoomSize)
        {
            this.roomCount = roomCount;
            this.minRoomSize = Mathf.Max(minRoomSize, TileMap.MinRoomSize);
            this.maxRoomSize = Mathf.Max(maxRoomSize, TileMap.MinRoomSize);

            List<Room> candidateRooms = CreateRooms();
            List<Room> selectedRooms = SelectRooms(candidateRooms);
            CreateTiles(selectedRooms);
            ConnectRooms(selectedRooms);
            BuildWall();

            for (int i = 0; i < width * height; i++)
            {
                Tile tile = GetTile(i);
                if (Tile.Type.None == tile.type)
                {
                    tiles[i] = null;
                    continue;
                }

                tile.cost = Tile.PathCost.MinCost;
                tile.neighbors = new Tile[(int)Tile.Direction.Max];
            }

            for(int i = 0; i < width * height; i++)
            {
                Tile tile = GetTile(i);
                if (null == tile)
                {
                    continue;
                }

                for (int direction = 0; direction < (int)Tile.Direction.Max; direction++)
                {
                    var offset = Tile.DirectionOffsets[direction];
                    tile.neighbors[direction] = GetTile((int)(tile.rect.x + offset.x), (int)(tile.rect.y + offset.y));
                }
            }

            foreach (Room room in this.rooms)
            {
                Rect roomRect = room.rect;
                for (int y = (int)roomRect.yMin; y < (int)roomRect.yMax; y++)
                {
                    for (int x = (int)roomRect.xMin; x < (int)roomRect.xMax; x++)
                    {
                        Tile tile = GetTile(x, y);
                        tile.room = room;
                    }
                }
            }
            /*
            foreach (Corridor corridor in this.corridors)
            {
                foreach (Tile tile in corridor.tiles)
                {
                    if (null != tile.room)
                    {
                        Rect floorRect = tile.room.GetFloorRect();
                        if (true == floorRect.Contains(new Vector2(tile.rect.x, tile.rect.y)))
                        {
                            continue;
                        }
                    }
                }
            }
            */
        }

        public Tile GetTile(int index)
        {
            if (0 > index || index >= tiles.Length)
            {
                return null;
            }

            return tiles[index];
        }

        public Tile GetTile(int x, int y)
        {
            if (0 > x || x >= rect.width)
            {
                return null;
            }

            if (0 > y || y >= height)
            {
                return null;
            }

            return tiles[y * width + x];
        }

        public List<Tile> FindPath(Tile from, Tile to)
        {
            AStarPathFinder pathFinder = new AStarPathFinder(this, rect);
            List<Tile> path = pathFinder.FindPath(from, to);
            if (null == path || 0 == path.Count)
            {
                return null;
            }

            return path;
        }

        private List<Room> CreateRooms()
        {
            List<Room> candidateRooms = new List<Room>();
            int index = 1;
            candidateRooms.Add(CreateRoom(index++, 0, 0));
            candidateRooms.Add(CreateRoom(index++, this.maxRoomSize * 2, 0));
            candidateRooms.Add(CreateRoom(index++, this.maxRoomSize / 2, this.maxRoomSize * 2));

            for (int i = 0; i < this.roomCount * 2; i++)
            {
                Room room = CreateRoom(index++, candidateRooms);
                candidateRooms.Add(room);
                RepositionRoom(room.center, candidateRooms);
            }

            return candidateRooms;
        }

        private Room CreateRoom(int index, int x, int y)
        {
            int width = GetRandomRoomSize();
            int height = GetRandomRoomSize();
            return new Room(index, x, y, width, height);
        }

        private Room CreateRoom(int index, List<Room> existRooms)
        {
            var triangulation = new DelaunayTriangulation(existRooms);
            if (null == triangulation)
            {
                throw new System.Exception("can not build 'DelaunayTriangles'");
            }

            if (0 == triangulation.triangles.Count)
            {
                throw new System.Exception("can not find triangle");
            }

            // find the most biggest 'inner circle'
            DelaunayTriangulation.Circle biggestCircle = triangulation.triangles[0].innerCircle;
            foreach (var triangle in triangulation.triangles)
            {
                if (biggestCircle.radius <= triangle.innerCircle.radius)
                {
                    biggestCircle = triangle.innerCircle;
                }
            }

            int width = GetRandomRoomSize();
            int height = GetRandomRoomSize();
            int x = (int)biggestCircle.center.x - width / 2;
            int y = (int)biggestCircle.center.y - height / 2;

            return new Room(index, (int)x, (int)y, width, height);
        }

        private int GetRandomRoomSize()
        {
            return Random.Range(this.minRoomSize, this.maxRoomSize + 1);
        }

        private void RepositionRoom(Vector3 center, List<Room> rooms)
        {
            while (true)
            {
                bool overlap = false;
                for (int i = 0; i < rooms.Count; i++)
                {
                    for (int j = i + 1; j < rooms.Count; j++)
                    {
                        if (true == rooms[i].rect.Overlaps(rooms[j].rect))
                        {
                            Rect boundary = GetBoundaryRect(rooms);
                            ResolveOverlap(center, boundary, rooms[i], rooms[j]);
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

        private void ResolveOverlap(Vector3 center, Rect boundary, Room room1, Room room2)
        {
            if (boundary.width < boundary.height) // ���� ��ġ�� ���η� ��� �Ǿ� ����. �׷��� ���η� �̵���
            {
                if (room1.center.x < room2.center.x) // �� ���� �� block2�� ������ �ִ� ���
                {
                    if (center.x < room2.center.x)
                    {
                        room2.x += 1; // block2�� �߾� ���� �����ʿ� ������ block2�� ���������� 1ĭ �̵�
                    }
                    else
                    {
                        room1.x -= 1; // block2�� �߾� ���� ���ʿ� ������ block1�� �������� 1ĭ �̵�
                    }
                }
                else // �� ���� �� block1�� ������ �ִ� ���
                {
                    if (center.x < room1.center.x)
                    {
                        room1.x += 1; // block1�� �߾� ���� �����ʿ� ������ block1�� ���������� 1ĭ �̵�
                    }
                    else
                    {
                        room2.x -= 1; // block1�� �߾� ���� ���ʿ� ������ block2�� �������� 1ĭ �̵�
                    }
                }
            }
            else // ���� ��ġ�� ���η� ��� �Ǿ� ����. �׷��� ���η� �̵���
            {
                if (room1.center.y < room2.center.y)
                {
                    if (center.y < room2.center.y)
                    {
                        room2.y += 1; // block2�� �߾� ���� ���� ������ block2�� �������� 1ĭ �̵�
                    }
                    else
                    {
                        room1.y -= 1; // block2�� �߾� ���� �Ʒ��� ������ block1�� �Ʒ��� 1ĭ �̵�
                    }
                }
                else
                {
                    if (center.y < room1.center.y)
                    {
                        room1.y += 1; // block1�� �߾� ���� ���� ������ block1�� ���� 1ĭ �̵�
                    }
                    else
                    {
                        room2.y -= 1;  // block1�� �߾� ���� �Ʒ��� ������ block2�� �Ʒ��� 1ĭ �̵�
                    }
                }
            }
        }

        private Rect GetBoundaryRect(List<Room> rooms)
        {
            Rect boundary = new Rect();
            boundary.xMin = float.MaxValue;
            boundary.yMin = float.MaxValue;
            boundary.xMax = float.MinValue;
            boundary.yMax = float.MinValue;
            foreach (Room room in rooms)
            {
                boundary.xMin = Mathf.Min(boundary.xMin, room.rect.xMin);
                boundary.yMin = Mathf.Min(boundary.yMin, room.rect.yMin);
                boundary.xMax = Mathf.Max(boundary.xMax, room.rect.xMax);
                boundary.yMax = Mathf.Max(boundary.yMax, room.rect.yMax);
            }
            return boundary;
        }

        private List<Room> SelectRooms(List<Room> candidateRooms)
        {
            this.rooms = new List<Room>();

            var triangulation = new DelaunayTriangulation(candidateRooms);
            var mst = new MinimumSpanningTree(candidateRooms);
            foreach (var triangle in triangulation.triangles)
            {
                foreach (var edge in triangle.edges)
                {
                    mst.AddEdge(new MinimumSpanningTree.Edge(edge.v0.room, edge.v1.room, Vector3.Distance(edge.v0.room.rect.center, edge.v1.room.rect.center)));
                }
            }

            mst.BuildTree();

            foreach (var connection in mst.connections)
            {
                connection.room1.neighbors.Add(connection.room2);
                connection.room2.neighbors.Add(connection.room1);
            }

            Room startRoom = candidateRooms[Random.Range(0, candidateRooms.Count)];

            SelectRoom(startRoom, 1);

            while (this.roomCount > this.rooms.Count)
            {
                Room room = candidateRooms[0];
                if (false == this.rooms.Contains(room))
                {
                    this.rooms.Add(room);
                }
                candidateRooms.RemoveAt(0);
            }

            foreach (Room room in this.rooms)
            {
                room.neighbors.Clear();
            }

            return this.rooms;
        }

        private void SelectRoom(Room room, int depth)
        {
            depth--;

            if (0 == depth)
            {
                depth = RandomDepthCount.Random();
                this.rooms.Add(room);
            }

            while (0 < room.neighbors.Count && this.roomCount > this.rooms.Count)
            {
                int index = Random.Range(0, room.neighbors.Count);
                Room neighbor = room.neighbors[index];

                neighbor.neighbors.Remove(room);
                room.neighbors.RemoveAt(index);

                SelectRoom(neighbor, depth);
            }
        }

        private void CreateTiles(List<Room> selectedRooms)
        {
            this.rect = GetBoundaryRect(selectedRooms);
            this.tiles = new Tile[width * height];

            for (int i = 0; i < width * height; i++)
            {
                Tile tile = new Tile(i);
                tile.rect = new Rect(i % width, i / width, 1, 1);
                tile.type = Tile.Type.None;
                tile.cost = Tile.PathCost.MaxCost;
                this.tiles[i] = tile;
            }

            foreach (Room room in this.rooms)
            {
                room.rect.x -= this.rect.xMin;
                room.rect.y -= this.rect.yMin;

                for (int x = (int)room.rect.xMin; x < (int)room.rect.xMax; x++)
                {
                    Tile top = GetTile(x, (int)room.rect.yMax - 1);
                    top.type = Tile.Type.Floor;
                    top.cost = Tile.PathCost.MaxCost;
                    
                    Tile bottom = GetTile(x, (int)room.rect.yMin);
                    bottom.type = Tile.Type.Floor;
                    bottom.cost = Tile.PathCost.MaxCost;
                    
                }

                for (int y = (int)room.rect.yMin; y < (int)room.rect.yMax; y++)
                {
                    Tile left = GetTile((int)room.rect.xMin, y);
                    left.type = Tile.Type.Floor;
                    left.cost = Tile.PathCost.MaxCost;

                    Tile right = GetTile((int)room.rect.xMax - 1, y);
                    right.type = Tile.Type.Floor;
                    right.cost = Tile.PathCost.MaxCost;
                }

                {
                    Tile lt = GetTile((int)room.rect.xMin, (int)room.rect.yMax - 1);
                    lt.type = Tile.Type.Wall;
                    Tile rt = GetTile((int)room.rect.xMax - 1, (int)room.rect.yMax - 1);
                    rt.type = Tile.Type.Wall;
                    Tile lb = GetTile((int)room.rect.xMin, (int)room.rect.yMin);
                    lb.type = Tile.Type.Wall;
                    Tile rb = GetTile((int)room.rect.xMax - 1, (int)room.rect.yMin);
                    rb.type = Tile.Type.Wall;
                }

                // �� ���� �ٴ� �κ��� floor Ÿ������ ����
                Rect floorRect = room.GetFloorRect();
                for (int y = (int)floorRect.yMin; y < (int)floorRect.yMax; y++)
                {
                    for (int x = (int)floorRect.xMin; x < (int)floorRect.xMax; x++)
                    {
                        Tile floor = GetTile(x, y);
                        floor.type = Tile.Type.Floor;
                        floor.cost = Tile.PathCost.Floor;
                    }
                }

                Rect roomRect = room.rect;
                for (int y = (int)roomRect.yMin; y < (int)roomRect.yMax; y++)
                {
                    for (int x = (int)roomRect.xMin; x < (int)roomRect.xMax; x++)
                    {
                        Tile tile = GetTile(x, y);
                        tile.room = room;
                    }
                }
            }

            this.rect.x = 0;
            this.rect.y = 0;
        }

        private void ConnectRooms(List<Room> selectedRooms)
        {
            this.corridors = new List<Corridor>();

            var triangulation = new DelaunayTriangulation(selectedRooms);
            var mst = new MinimumSpanningTree(selectedRooms);
            foreach (var triangle in triangulation.triangles)
            {
                foreach (var edge in triangle.edges)
                {
                    mst.AddEdge(new MinimumSpanningTree.Edge(edge.v0.room, edge.v1.room, Vector3.Distance(edge.v0.room.rect.center, edge.v1.room.rect.center)));
                }
            }

            mst.BuildTree();
            foreach (var edge in mst.edges)
            {
                if (12.5f < UnityEngine.Random.Range(0.0f, 100.0f)) // 12.5 % Ȯ���� ���� �߰�
                {
                    continue;
                }

                if (true == mst.connections.Contains(edge))
                {
                    continue;
                }

                mst.connections.Add(edge);
            }

            foreach (var connection in mst.connections)
            {
                connection.room1.neighbors.Add(connection.room2);
                connection.room2.neighbors.Add(connection.room1);

                ConnectRoom(connection.room1, connection.room2);
            }
        }

        private void ConnectRoom(Room a, Room b)
        {
            float xMin = Mathf.Max(a.rect.xMin, b.rect.xMin);
            float xMax = Mathf.Min(a.rect.xMax, b.rect.xMax);
            float yMin = Mathf.Max(a.rect.yMin, b.rect.yMin);
            float yMax = Mathf.Min(a.rect.yMax, b.rect.yMax);

            List<Vector3> positions = null;
            if (3 <= xMax - xMin) // x �� ��Ĩ. ���� ��� �����
            {
                positions = ConnectVerticalRoom(a, b);
            }
            else if (3 <= yMax - yMin) // y �� ��ħ. ���� ��� �����
            {
                positions = ConnectHorizontalRoom(a, b);
            }
            else        // ���� ��� ������ �Ѵ�
            {
                positions = ConnectDiagonalRoom(a, b);
            }

            Vector3 startPosition = a.center;
            Vector3 endPosition = b.center;

            if (null != positions && 2 <= positions.Count)
            {
                startPosition = positions[0];
                endPosition = positions[positions.Count - 1];
            }

            var start = GetTile((int)startPosition.x, (int)startPosition.y);
            var end = GetTile((int)endPosition.x, (int)endPosition.y);

            Rect searchBoundary = GetBoundaryRect(new List<Room>() { a, b });
            AStarPathFinder pathFinder = new AStarPathFinder(this, searchBoundary);
            var path = pathFinder.FindPath(start, end);

            Corridor corridor = new Corridor();
            corridor.tiles = path;
            
            Debug.Assert(0 < corridor.tiles.Count);

            corridors.Add(corridor);

            foreach (var tile in corridor.tiles)
            {
                tile.type = Tile.Type.Floor;
                tile.cost = Tile.PathCost.MinCost;
            }
        }

        private List<Vector3> ConnectVerticalRoom(Room a, Room b)
        {
            Room upperRoom = null;
            Room bottomRoom = null;

            if (a.center.y > b.center.y)
            {
                upperRoom = a;
                bottomRoom = b;
            }
            else
            {
                upperRoom = b;
                bottomRoom = a;
            }

            int xMin = (int)Mathf.Max(a.rect.xMin, b.rect.xMin);
            int xMax = (int)Mathf.Min(a.rect.xMax, b.rect.xMax);
            int x = Random.Range(xMin + 1, xMax - 1);

            int upperY = (int)upperRoom.rect.center.y;
            int bottomY = (int)bottomRoom.rect.center.y;

            Vector3 start = new Vector3(x, upperY);
            Vector3 end = new Vector3(x, bottomY);
            List<Vector3> positions = new List<Vector3>() { start, end };
            if (false == AdjustTileCostOnCorridor(positions))
            {
                return null;
            }
            return positions;
        }
        private List<Vector3> ConnectHorizontalRoom(Room a, Room b)
        {
            Room leftRoom = null;
            Room rightRoom = null;

            if (a.center.x > b.center.x)
            {
                rightRoom = a;
                leftRoom = b;
            }
            else
            {
                rightRoom = b;
                leftRoom = a;
            }

            int yMin = (int)Mathf.Max(a.rect.yMin, b.rect.yMin);
            int yMax = (int)Mathf.Min(a.rect.yMax, b.rect.yMax);
            int y = Random.Range(yMin + 1, yMax - 1);

            int leftX = (int)leftRoom.rect.center.x;
            int rightX = (int)rightRoom.rect.center.x;

            Vector3 start = new Vector3(leftX, y);
            Vector3 end = new Vector3(rightX, y);
            List<Vector3> positions = new List<Vector3>() { start, end };
            if (false == AdjustTileCostOnCorridor(positions))
            {
                return null;
            }
            return positions;
        }
        private List<Vector3> ConnectDiagonalRoom(Room a, Room b)
        {
            int xMin = (int)Mathf.Max(a.rect.xMin, b.rect.xMin);
            int xMax = (int)Mathf.Min(a.rect.xMax, b.rect.xMax);
            int xOverlap = xMax - xMin;
            if (0 > xOverlap)
            {
                xOverlap = 0;
            }
            int yMin = (int)Mathf.Max(a.rect.yMin, b.rect.yMin);
            int yMax = (int)Mathf.Min(a.rect.yMax, b.rect.yMax);
            int yOverlap = yMax - yMin;
            if (0 > yOverlap)
            {
                yOverlap = 0;
            }

            Room upperRoom = null;
            Room bottomRoom = null;
            if (a.center.y > b.center.y)
            {
                upperRoom = a;
                bottomRoom = b;
            }
            else
            {
                upperRoom = b;
                bottomRoom = a;
            }

            Room leftRoom = null;
            Room rightRoom = null;
            if (a.center.x > b.center.x)
            {
                rightRoom = a;
                leftRoom = b;
            }
            else
            {
                rightRoom = b;
                leftRoom = a;
            }

            int upperRoomY = (int)Random.Range(upperRoom.rect.yMin + 1 + yOverlap, upperRoom.rect.yMax - 2);
            int bottomRoomY = (int)Random.Range(bottomRoom.rect.yMin + 1, bottomRoom.rect.yMax - 2 - yOverlap);
            int leftRoomX = (int)Random.Range(leftRoom.rect.xMin + 1, leftRoom.rect.xMax - 2 - xOverlap);
            int rightRoomX = (int)Random.Range(rightRoom.rect.xMin + 1 + xOverlap, rightRoom.rect.xMax - 2);

            if (upperRoom == leftRoom)
            {
                var corridorBL = new List<Vector3>() {
                new Vector3(leftRoomX,               leftRoom.rect.center.y),
                new Vector3(leftRoomX,               bottomRoomY),
                new Vector3(rightRoom.rect.center.x, bottomRoomY)
            };

                var corridorRT = new List<Vector3>()
            {
                new Vector3(leftRoom.rect.center.x,  upperRoomY),
                new Vector3(rightRoomX,              upperRoomY),
                new Vector3(rightRoomX,              rightRoom.rect.center.y)
            };

                List<Vector3> positions = (0 == Random.Range(0, 2)) ? corridorBL : corridorRT;
                if (false == AdjustTileCostOnCorridor(positions))
                {
                    positions = (corridorBL == positions) ? corridorRT : corridorBL;
                    if (false == AdjustTileCostOnCorridor(positions))
                    {
                        positions = null;
                    }
                }

                return positions;
            }

            if (bottomRoom == leftRoom)
            {
                var corridorTL = new List<Vector3>()
            {
                new Vector3(leftRoomX,               bottomRoom.rect.center.y),
                new Vector3(leftRoomX,               upperRoomY),
                new Vector3(rightRoom.rect.center.x, upperRoomY)
            };

                var corridorRB = new List<Vector3>()
            {
                new Vector3(leftRoom.rect.center.x,  bottomRoomY),
                new Vector3(rightRoomX,              bottomRoomY),
                new Vector3(rightRoomX,              upperRoom.rect.center.y)
            };

                List<Vector3> positions = (0 == Random.Range(0, 2)) ? corridorTL : corridorRB;

                if (false == AdjustTileCostOnCorridor(positions))
                {
                    positions = (corridorTL == positions) ? corridorRB : corridorTL;
                    if (false == AdjustTileCostOnCorridor(positions))
                    {
                        positions = null;
                    }
                }
                return positions;
            }
            return null;
        }
        private class Journal
        {
            public class Original
            {
                public Original(Tile tile)
                {
                    this.tile = tile;
                    this.type = tile.type;
                    this.cost = tile.cost;
                }

                public Tile tile;
                public Tile.Type type;
                public int cost;
            }

            public Stack<Original> originals = new Stack<Original>();

            public void Rollback()
            {
                while (0 < originals.Count)
                {
                    var original = originals.Pop();
                    original.tile.type = original.type;
                    original.tile.cost = original.cost;
                }
            }

            public void Push(Tile tile)
            {
                originals.Push(new Original(tile));
            }
        }

        private bool AdjustTileCostOnCorridor(List<Vector3> positions)
        {
            if (2 > positions.Count)
            {
                return false;
            }

            Journal journal = new Journal();
            for (int start = 0; start < positions.Count - 1; start++)
            {
                Vector3 startPosition = positions[start];
                Vector3 endPosition = positions[start + 1];

                int xMin = (int)Mathf.Min(startPosition.x, endPosition.x);
                int xMax = (int)Mathf.Max(startPosition.x, endPosition.x);
                int yMin = (int)Mathf.Min(startPosition.y, endPosition.y);
                int yMax = (int)Mathf.Max(startPosition.y, endPosition.y);

                for (int y = yMin; y <= yMax; y++)
                 {
                    for (int x = xMin; x <= xMax; x++)
                    {
                        var tile = this.GetTile(x, y);
                        if (null == tile)
                        {
                            Debug.Log($"can not find tile(x:{x}, y:{y})");
                            journal.Rollback();
                            return false;
                        }

                        if (Tile.Type.Wall == tile.type)
                        {
                            Debug.Log($"fail to create path at tile_index:{tile.index}, x:{x}, y:{y}");
                            journal.Rollback();
                            return false;
                        }

                        journal.Push(tile);
                        tile.cost = Mathf.Min(tile.cost, Tile.PathCost.Corridor);
                    }
                }
            }

            return true;
        }

        private void BuildWall()
        {
            foreach (var room in this.rooms)
            {
                for (int x = (int)room.rect.xMin; x < (int)room.rect.xMax; x++)
                {
                    Tile top = GetTile(x, (int)room.rect.yMax - 1);
                    if (Tile.PathCost.MinCost < top.cost)
                    {
                        top.type = Tile.Type.Wall;
                    }
                    else
                    {
                        room.doors.Add(top);
                    }

                    Tile bottom = GetTile(x, (int)room.rect.yMin);
                    if (Tile.PathCost.MinCost < bottom.cost)
                    {
                        bottom.type = Tile.Type.Wall;
                    }
                    else
                    {
                        room.doors.Add(bottom);
                    }
                }

                for (int y = (int)room.rect.yMin; y < (int)room.rect.yMax; y++)
                {
                    Tile left = GetTile((int)room.rect.xMin, y);
                    if (Tile.PathCost.MinCost < left.cost)
                    {
                        left.type = Tile.Type.Wall;
                    }
                    else
                    {
                        room.doors.Add(left);
                    }

                    Tile right = GetTile((int)room.rect.xMax - 1, y);
                    if (Tile.PathCost.MinCost < right.cost)
                    {
                        right.type = Tile.Type.Wall;
                    }
                    else
                    {
                        room.doors.Add(right);
                    }
                }
            }

            foreach (var corridor in corridors)
            {
                foreach (Tile tile in corridor.tiles)
                {
                    int x = (int)tile.rect.x;
                    int y = (int)tile.rect.y;

                    foreach (var offset in Tile.DirectionOffsets)
                    {
                        BuildWallOnTile(x + (int)offset.x, y + (int)offset.y);
                    }
                }
            }
        }

        private void BuildWallOnTile(int x, int y)
        {
            Tile tile = GetTile(x, y);
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
        }
    }
}