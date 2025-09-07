using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
            new Vector3( 0, +1),
            new Vector3(+1, +1),
            new Vector3(-1,  0),
            new Vector3(+1,  0),
            new Vector3(-1, -1),
            new Vector3( 0, -1),
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
            tile.neighbors = new Tile[Tile.Direction.Max];
        }

        for (int i = 0; i < width * height; i++)
        {
            Tile tile = GetTile(i);
            if (null == tile)
            {
                continue;
            }

            for (int direction = 0; direction < Tile.Direction.Max; direction++)
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

        foreach (Corridor corridor in this.corridors)
        {
            for (int i = 0; i < corridor.tiles.Count; i++)
            {
                Tile tile = corridor.tiles[i];
                if (null == tile.room)
                {
                    continue;
                }

                Rect floorRect = tile.room.GetFloorRect();
                if (true != floorRect.Contains(new Vector2(tile.rect.x, tile.rect.y)))
                {
                    continue;
                }

                corridor.tiles.RemoveAt(i);
                i--;
            }
        }
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

    public class AStarPathFinder
    {
        private static Vector2Int[] LOOKUP_OFFSETS = {
            new Vector2Int(-1, 0),  // left
            new Vector2Int( 0,-1),  // down
            new Vector2Int( 1, 0),  // right
            new Vector2Int( 0, 1)   // up
        };

        public class Node
        {
            public TileMap.Tile tile;
            public Node parent;
            public int index { get => this.tile.index; }
            public int pathCost;
            public int expectCost;
            public int cost { get => this.pathCost + this.expectCost; }

            public Node(TileMap.Tile tile)
            {
                this.tile = tile;
                this.pathCost = 0;
                this.expectCost = 0;
            }
        }

        private TileMap tileMap;
        private Rect boundary;

        public List<TileMap.Tile> path = new List<TileMap.Tile>();

        public AStarPathFinder(TileMap tileMap, Rect pathFindBoundary)
        {
            this.tileMap = tileMap;
            this.boundary = pathFindBoundary;
        }

        public List<TileMap.Tile> FindPath(TileMap.Tile from, TileMap.Tile to)
        {
            Dictionary<int, Node> openNodes = new Dictionary<int, Node>();
            Dictionary<int, Node> closeNodes = new Dictionary<int, Node>();

            Node currentNode = new Node(from);
            currentNode.expectCost += (int)Mathf.Abs(to.rect.x - from.rect.x);
            currentNode.expectCost += (int)Mathf.Abs(to.rect.y - from.rect.y);
            openNodes.Add(currentNode.index, currentNode);

            while (0 < openNodes.Count)
            {
                List<Node> sortedNodes = openNodes.Values.ToList<Node>();
                if (0 == sortedNodes.Count)
                {
                    break;  // ��� ã�� ����
                }

                sortedNodes.Sort((Node lhs, Node rhs) =>
                {
                    if (lhs.cost > rhs.cost)
                    {
                        return 1;
                    }
                    else if (lhs.cost < rhs.cost)
                    {
                        return -1;
                    }
                    else if (lhs.expectCost > rhs.expectCost)
                    {
                        return 1;
                    }
                    else if (lhs.expectCost < rhs.expectCost)
                    {
                        return -1;
                    }
                    return 0;
                });

                currentNode = sortedNodes[0];

                List<Node> children = new List<Node>();
                int offsetIndex = UnityEngine.Random.Range(0, LOOKUP_OFFSETS.Length);
                for (int i = 0; i < LOOKUP_OFFSETS.Length; i++)// ��ֹ��� ���� �ִµ� ���� �� �� �ִ� Ÿ�ϵ��� openNode ����Ʈ�� �ִ´�
                {
                    var offset = LOOKUP_OFFSETS[offsetIndex];

                    int x = currentNode.index % tileMap.width + offset.x;
                    int y = currentNode.index / tileMap.width + offset.y;

                    offsetIndex += 1;
                    offsetIndex %= LOOKUP_OFFSETS.Length;

                    var tile = this.GetTile(x, y);
                    if (null == tile)
                    {
                        continue;
                    }

                    if (to == tile)
                    {
                        path.Insert(0, tile);
                        do
                        {
                            path.Insert(0, currentNode.tile);
                            currentNode = currentNode.parent;
                        } while (null != currentNode);
                        return path;
                    }

                    if (TileMap.Tile.Type.Wall == tile.type)
                    {
                        continue;
                    }

                    if (true == closeNodes.ContainsKey(tile.index)) // Ž���� ������ �̹� ���� ��忡 �� Ÿ����
                    {
                        continue;
                    }

                    if (true == openNodes.ContainsKey(tile.index)) // �տ��� �ѹ� ���� ��忡 ��� �Դ� Ÿ��
                    {
                        Node openNode = openNodes[tile.index];
                        if (openNode.pathCost + tile.cost < currentNode.pathCost)
                        {
                            currentNode.pathCost = openNode.pathCost + tile.cost;
                            currentNode.parent = openNode;
                        }
                        continue;
                    }

                    Node child = new Node(tile);
                    child.parent = currentNode;
                    child.pathCost = currentNode.pathCost + tile.cost;
                    child.expectCost += (int)Mathf.Abs(to.rect.x - tile.rect.x);
                    child.expectCost += (int)Mathf.Abs(to.rect.y - tile.rect.y);

                    openNodes.Add(child.index, child);
                }

                openNodes.Remove(currentNode.index);
                closeNodes.Add(currentNode.index, currentNode);
            }

            return path;
        }

        private TileMap.Tile GetTile(int x, int y)
        {
            if (boundary.xMin > x || x >= boundary.xMax)
            {
                return null;
            }

            if (boundary.yMin > y || y >= boundary.yMax)
            {
                return null;
            }

            return tileMap.GetTile(x, y);
        }
    }

    public class DelaunayTriangulation
    {
        public class Point
        {
            public TileMap.Room room;

            public Point(TileMap.Room room)
            {
                this.room = room;
            }

            public Vector2 position
            {
                get { return this.room.rect.center; }
            }
        }

        public class Edge
        {
            public Point v0;
            public Point v1;
            public float cost
            {
                get
                {
                    if (null == v0 || null == v1)
                    {
                        return 0.0f;
                    }

                    return Vector2.Distance(v0.position, v1.position);
                }
            }

            public Edge(Point v0, Point v1)
            {
                this.v0 = v0;
                this.v1 = v1;
            }

            public override bool Equals(object other)
            {
                if (false == (other is Edge))
                {
                    return false;
                }

                return Equals((Edge)other);
            }

            public bool Equals(Edge edge)
            {
                return ((this.v0.position.Equals(edge.v0.position) && this.v1.position.Equals(edge.v1.position)) || (this.v0.position.Equals(edge.v1.position) && this.v1.position.Equals(edge.v0.position)));
            }

            public override int GetHashCode()
            {
                return v0.GetHashCode() ^ (v1.GetHashCode() << 2);
            }
        }

        public class Circle
        {
            public Vector3 center;
            public float radius;

            public Circle(Vector3 center, float radius)
            {
                this.center = center;
                this.radius = radius;
            }

            public bool Contains(Vector3 point)
            {
                float d = Vector3.Distance(center, point);
                if (radius < d)
                {
                    return false;
                }

                return true;
            }
        }

        public class Triangle
        {
            public Vector3 a;
            public Vector3 b;
            public Vector3 c;
            public Circle circumCircle;
            public Circle innerCircle;
            public List<Edge> edges;

            public Triangle(Point p1, Point p2, Point p3)
            {
                this.a = p1.position;
                this.b = p2.position;
                this.c = p3.position;

                this.circumCircle = calcCircumCircle();
                this.innerCircle = calcInnerCircle();
                this.edges = new List<Edge>();
                this.edges.Add(new Edge(p1, p2));
                this.edges.Add(new Edge(p2, p3));
                this.edges.Add(new Edge(p3, p1));
            }

            public override bool Equals(object other)
            {
                if (false == (other is Triangle))
                {
                    return false;
                }

                return Equals((Triangle)other);
            }

            public override int GetHashCode()
            {
                return a.GetHashCode() ^ (b.GetHashCode() << 2) ^ (c.GetHashCode() >> 2);
            }

            public bool Equals(Triangle triangle)
            {
                return this.a == triangle.a && this.b == triangle.b && this.c == triangle.c;
            }

            private Circle calcCircumCircle()
            {
                // ��ó: �ﰢ�� ������ ���ϱ� - https://kukuta.tistory.com/444

                if (a == b || b == c || c == a) // ���� ���� ����. �ﰢ�� �ƴ�. ������ ���� �� ����.
                {
                    return null;
                }

                float mab = (b.x - a.x) / (b.y - a.y) * -1.0f;  // ���� ab�� �����̵�м��� ����
                float a1 = (b.x + a.x) / 2.0f;                  // ���� ab�� x�� �߽� ��ǥ
                float b1 = (b.y + a.y) / 2.0f;                  // ���� ab�� y�� �߽� ��ǥ

                // ���� bc
                float mbc = (b.x - c.x) / (b.y - c.y) * -1.0f;  // ���� bc�� �����̵�м��� ����
                float a2 = (b.x + c.x) / 2.0f;                  // ���� bc�� x�� �߽� ��ǥ
                float b2 = (b.y + c.y) / 2.0f;                  // ���� bc�� y�� �߽� ��ǥ

                if (mab == mbc)     // �� �����̵�м��� ���Ⱑ ����. ������. 
                {
                    return null;    // ���� ���� �� ����
                }

                float x = (mab * a1 - mbc * a2 + b2 - b1) / (mab - mbc);
                float y = mab * (x - a1) + b1;

                if (b.x == a.x)     // �����̵�м��� ���Ⱑ 0�� ���(����)
                {
                    x = a2 + (b1 - b2) / mbc;
                    y = b1;
                }

                if (b.y == a.y)     // �����̵�м��� ���Ⱑ ������ ���(������)
                {
                    x = a1;
                    if (0.0f == mbc)
                    {
                        y = b2;
                    }
                    else
                    {
                        y = mbc * (a1 - a2) + b2;
                    }
                }

                if (b.x == c.x)     // �����̵�м��� ���Ⱑ 0�� ���(����)
                {
                    x = a1 + (b2 - b1) / mab;
                    y = b2;
                }

                if (b.y == c.y)     // �����̵�м��� ���Ⱑ ������ ���(������)
                {
                    x = a2;
                    if (0.0f == mab)
                    {
                        y = b1;
                    }
                    else
                    {
                        y = mab * (a2 - a1) + b1;
                    }
                }

                Vector3 center = new Vector3(x, y, 0.0f);
                float radius = Vector3.Distance(center, a);

                return new Circle(center, radius);
            }

            private Circle calcInnerCircle()
            {
                float e1 = Mathf.Sqrt((this.b.x - this.c.x) * (this.b.x - this.c.x) + (this.b.y - this.c.y) * (this.b.y - this.c.y));
                float e2 = Mathf.Sqrt((this.c.x - this.a.x) * (this.c.x - this.a.x) + (this.c.y - this.a.y) * (this.c.y - this.a.y));
                float e3 = Mathf.Sqrt((this.a.x - this.b.x) * (this.a.x - this.b.x) + (this.a.y - this.b.y) * (this.a.y - this.b.y));

                float x = (e1 * a.x + e2 * b.x + e3 * c.x) / (e1 + e2 + e3);
                float y = (e1 * a.y + e2 * b.y + e3 * c.y) / (e1 + e2 + e3);

                Vector3 center = new Vector3(x, y, 0.0f);
                float semiperimeter = (e1 + e2 + e3) / 2;
                float area = Mathf.Sqrt(semiperimeter * (semiperimeter - e1) * (semiperimeter - e2) * (semiperimeter - e3));
                float radius = area / semiperimeter;
                return new Circle(center, radius);
            }
        }

        private Triangle superTriangle = null;
        public List<Triangle> triangles = new List<Triangle>();

        public DelaunayTriangulation(List<TileMap.Room> rooms)
        {
            superTriangle = CreateSuperTriangle(rooms);
            if (null == superTriangle)
            {
                return;
            }

            triangles.Add(superTriangle);

            foreach (var room in rooms)
            {
                AddPoint(room);
            }

            RemoveSuperTriangle();
        }

        public void AddPoint(TileMap.Room room)
        {
            Vector3 point = room.rect.center;

            List<Triangle> badTriangles = new List<Triangle>();
            foreach (var triangle in triangles)
            {
                if (true == triangle.circumCircle.Contains(point))
                {
                    badTriangles.Add(triangle);
                }
            }

            List<Edge> polygon = new List<Edge>();

            // first find all the triangles that are no longer valid due to the insertion
            foreach (var triangle in badTriangles)
            {
                List<Edge> edges = triangle.edges;

                foreach (Edge edge in edges)
                {
                    // find unique edge
                    bool unique = true;
                    foreach (var other in badTriangles)
                    {
                        if (true == triangle.Equals(other))
                        {
                            continue;
                        }

                        foreach (var otherEdge in other.edges)
                        {
                            if (true == edge.Equals(otherEdge))
                            {
                                unique = false;
                                break;
                            }
                        }

                        if (false == unique)
                        {
                            break;
                        }
                    }

                    if (true == unique)
                    {
                        polygon.Add(edge);
                    }
                }
            }

            foreach (var badTriangle in badTriangles)
            {
                triangles.Remove(badTriangle);
            }

            foreach (Edge edge in polygon)
            {
                Triangle triangle = CreateTriangle(edge.v0, edge.v1, new Point(room));
                if (null == triangle)
                {
                    continue;
                }
                triangles.Add(triangle);
            }
        }

        public void RemoveSuperTriangle()
        {
            if (null == superTriangle)
            {
                return;
            }

            List<Triangle> remove = new List<Triangle>();
            foreach (var triangle in triangles)
            {
                if (true == (triangle.a == superTriangle.a || triangle.a == superTriangle.b || triangle.a == superTriangle.c ||
                                triangle.b == superTriangle.a || triangle.b == superTriangle.b || triangle.b == superTriangle.c ||
                                triangle.c == superTriangle.a || triangle.c == superTriangle.b || triangle.c == superTriangle.c
                    )
                )
                {
                    remove.Add(triangle);
                }
            }

            foreach (var triangle in remove)
            {
                triangles.Remove(triangle);
            }
        }

        private Triangle CreateSuperTriangle(List<TileMap.Room> rooms)
        {
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            foreach (TileMap.Room room in rooms)
            {
                Vector2 point = room.rect.center;
                minX = Mathf.Min(minX, point.x);
                maxX = Mathf.Max(maxX, point.x);
                minY = Mathf.Min(minY, point.y);
                maxY = Mathf.Max(maxY, point.y);
            }

            float dx = maxX - minX;
            float dy = maxY - minY;

            // super triangle�� ����Ʈ ����Ʈ ���� ũ�� ��� ������
            // super triangle�� ���� ����Ʈ�� ��ġ�� �Ǹ� �ﰢ���� �ƴ� ������ �ǹǷ� ���γ� �ﰢ������ ������ �� ���� �����̴�.
            TileMap.Room a = new TileMap.Room(0, minX - dx, minY - dy, 0, 0);
            TileMap.Room b = new TileMap.Room(0, minX - dx, maxY + dy * 3, 0, 0);
            TileMap.Room c = new TileMap.Room(0, maxX + dx * 3, minY - dy, 0, 0);

            // super triangle�� ������ ��� ����
            if (a == b || b == c || c == a)
            {
                return null;
            }

            return new Triangle(new Point(a), new Point(b), new Point(c));
        }

        private Triangle CreateTriangle(Point a, Point b, Point c)
        {
            if (a == b || b == c || c == a)
            {
                return null;
            }

            return new Triangle(a, b, c);
        }
    }

    public class MinimumSpanningTree
    {
        public class Edge
        {
            public Edge(TileMap.Room p1, TileMap.Room p2, float cost)
            {
                this.room1 = p1;
                this.room2 = p2;
                this.cost = cost;
            }

            public TileMap.Room room1;
            public TileMap.Room room2;
            public float cost;
        }

        private Dictionary<TileMap.Room, TileMap.Room> parents = new Dictionary<TileMap.Room, TileMap.Room>();
        public List<Edge> edges = new List<Edge>();
        public List<Edge> connections = new List<Edge>();

        public MinimumSpanningTree(List<TileMap.Room> rooms)
        {
            foreach (TileMap.Room room in rooms)
            {
                parents.Add(room, room);
            }
        }

        public void AddEdge(Edge edge)
        {
            foreach (Edge other in edges)
            {
                if (true == (edge.room1 == other.room1 && edge.room2 == other.room2) || (edge.room1 == other.room2 && edge.room2 == other.room1))
                {
                    return;
                }
            }

            edges.Add(edge);
        }

        public void BuildTree()
        {
            edges.Sort((Edge e1, Edge e2) =>
            {
                if (e1.cost == e2.cost)
                {
                    return 0;
                }
                else if (e1.cost > e2.cost)
                {
                    return 1;
                }
                return -1;
            });

            foreach (Edge edge in edges)
            {
                TileMap.Room srcParent = FindParent(edge.room1);
                TileMap.Room destParent = FindParent(edge.room2);

                if (srcParent != destParent)
                {
                    connections.Add(edge);
                    Union(srcParent, destParent);
                }
            }
        }

        private TileMap.Room FindParent(TileMap.Room room)
        {
            var parent = parents[room];
            if (parent != room)
            {
                parents[room] = FindParent(parent);
            }
            return parents[room];
        }

        private void Union(TileMap.Room src, TileMap.Room dest)
        {
            TileMap.Room srcParent = FindParent(src);
            TileMap.Room destParent = FindParent(dest);
            parents[srcParent] = destParent;
        }
    }

    public class WeightRandom<T>
    {
        private class Element
        {
            public T value;

            public int weight = 0;
            public int min = 0;
            public int max = 0;

            public Element left = null;
            public Element right = null;
        }

        private int total_weight = 0;
        private Element root = null;
        private List<Element> elements = new List<Element>();

        public WeightRandom()
        {
            this.total_weight = 0;
            this.root = null;
            this.elements.Clear();
        }

        public WeightRandom(List<KeyValuePair<int, T>> elements)
        {
            this.total_weight = 0;
            this.root = null;
            this.elements.Clear();
            foreach (var element in elements)
            {
                int weight = element.Key;
                T value = element.Value;
                AddElement(weight, value);
            }
        }

        public void AddElement(int weight, T value)
        {
            if (0 == weight)
            {
                return;
            }

            Element elmt = new Element();
            elmt.value = value;
            elmt.weight = weight;

            elements.Add(elmt);

            root = null;
        }

        public T Random()
        {
            if (null == root)
            {
                BuildTree();
            }

            int weight = UnityEngine.Random.Range(1, total_weight + 1);
            return Search(weight).value;
        }

        private void BuildTree()
        {
            elements.Sort((Element lhs, Element rhs) =>
            {
                if (lhs.weight == rhs.weight)
                {
                    return 0;
                }
                else if (lhs.weight > rhs.weight)
                {
                    return -1;
                }
                return 1;
            });

            int j = 1;
            for (int i = 0; i < elements.Count; i++)
            {
                Element elmt = elements[i];

                if (i + j < elements.Count)
                {
                    elmt.left = elements[i + j];
                }

                if (i + j + 1 < elements.Count)
                {
                    elmt.right = elements[i + j + 1];
                }

                j++;
            }

            root = elements[0];
            elements.Clear();

            SortByTreeOrder(root);
        }

        private void SortByTreeOrder(Element element)
        {
            if (null == element)
            {
                return;
            }

            SortByTreeOrder(element.left);

            this.total_weight += element.weight;
            element.min = this.total_weight - element.weight + 1;
            element.max = this.total_weight;
            elements.Add(element);

            SortByTreeOrder(element.right);
        }

        private Element Search(int weight)
        {
            Element curr = root;

            while (null != curr)
            {
                if (curr.min <= weight && weight <= curr.max)
                {
                    return curr;
                }

                if (curr.min > weight)
                {
                    curr = curr.left;
                }
                else
                {
                    curr = curr.right;
                }
            }

            return null;
        }
    }
}

