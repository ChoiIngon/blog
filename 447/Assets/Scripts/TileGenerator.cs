using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TileGenerator
{
    private const int MinRoomSize = 5;
    private static WeightRandom<int> RandomDepthCount = new WeightRandom<int>();

    private int roomCount;
    private int minRoomSize;
    private int maxRoomSize;

    private TileMap tileMap;
    private List<Room> rooms = new List<Room>();
    private List<Corridor> corridors = new List<Corridor>();
    
    public static void Init()
    {
        Tile.FloorSprite.CornerInnerLeftBottom.Add(GameManager.Instance.Resources.GetSprite("Floor.CornerInnerLeftBottom_1"));
        Tile.FloorSprite.CornerInnerLeftTop.Add(GameManager.Instance.Resources.GetSprite("Floor.CornerInnerLeftTop_1"));
        Tile.FloorSprite.CornerInnerRightBottom.Add(GameManager.Instance.Resources.GetSprite("Floor.CornerInnerRightBottom_1"));
        Tile.FloorSprite.CornerInnerRightTop.Add(GameManager.Instance.Resources.GetSprite("Floor.CornerInnerRightTop_1"));
        Tile.FloorSprite.HorizontalBottom.Add(GameManager.Instance.Resources.GetSprite("Floor.HorizontalBottom_1"));
        Tile.FloorSprite.HorizontalBottom.Add(GameManager.Instance.Resources.GetSprite("Floor.HorizontalBottom_2"));
        Tile.FloorSprite.HorizontalTop.Add(GameManager.Instance.Resources.GetSprite("Floor.HorizontalTop_1"));
        Tile.FloorSprite.HorizontalTop.Add(GameManager.Instance.Resources.GetSprite("Floor.HorizontalTop_2"));
        Tile.FloorSprite.InnerNormal.Add(GameManager.Instance.Resources.GetSprite("Floor.InnerNormal_1"));
        Tile.FloorSprite.InnerNormal.Add(GameManager.Instance.Resources.GetSprite("Floor.InnerNormal_2"));
        Tile.FloorSprite.VerticalLeft.Add(GameManager.Instance.Resources.GetSprite("Floor.VerticalLeft_1"));
        Tile.FloorSprite.VerticalRight.Add(GameManager.Instance.Resources.GetSprite("Floor.VerticalRight_1"));

        Tile.WallSprite.CornerInnerLeftBottom.Add(GameManager.Instance.Resources.GetSprite("Wall.CornerInnerLeftBottom_1"));
        Tile.WallSprite.CornerInnerLeftTop.Add(GameManager.Instance.Resources.GetSprite("Wall.CornerInnerLeftTop_1"));
        Tile.WallSprite.CornerInnerRightBottom.Add(GameManager.Instance.Resources.GetSprite("Wall.CornerInnerRightBottom_1"));
        Tile.WallSprite.CornerInnerRightTop.Add(GameManager.Instance.Resources.GetSprite("Wall.CornerInnerRightTop_1"));

        Tile.WallSprite.CornerOuterLeftTop.Add(GameManager.Instance.Resources.GetSprite("Wall.CornerOuterLeftTop_1"));
        Tile.WallSprite.CornerOuterLeftTop.Add(GameManager.Instance.Resources.GetSprite("Wall.CornerOuterLeftTop_2"));
        Tile.WallSprite.CornerOuterRightTop.Add(GameManager.Instance.Resources.GetSprite("Wall.CornerOuterRightTop_1"));
        Tile.WallSprite.CornerOuterRightTop.Add(GameManager.Instance.Resources.GetSprite("Wall.CornerOuterRightTop_2"));

        Tile.WallSprite.HorizontalTop.Add(GameManager.Instance.Resources.GetSprite("Wall.HorizontalTop_1"));
        Tile.WallSprite.HorizontalTop.Add(GameManager.Instance.Resources.GetSprite("Wall.HorizontalTop_2"));
        Tile.WallSprite.HorizontalTop.Add(GameManager.Instance.Resources.GetSprite("Wall.HorizontalTop_3"));
        Tile.WallSprite.HorizontalTop.Add(GameManager.Instance.Resources.GetSprite("Wall.HorizontalTop_4"));

        Tile.WallSprite.HorizontalBottom.Add(GameManager.Instance.Resources.GetSprite("Wall.HorizontalBottom_1"));
        Tile.WallSprite.HorizontalBottom.Add(GameManager.Instance.Resources.GetSprite("Wall.HorizontalBottom_2"));
        Tile.WallSprite.HorizontalBottom.Add(GameManager.Instance.Resources.GetSprite("Wall.HorizontalBottom_3"));
        Tile.WallSprite.HorizontalBottom.Add(GameManager.Instance.Resources.GetSprite("Wall.HorizontalBottom_4"));

        Tile.WallSprite.VerticalLeft.Add(GameManager.Instance.Resources.GetSprite("Wall.VerticalLeft_1"));
        Tile.WallSprite.VerticalLeft.Add(GameManager.Instance.Resources.GetSprite("Wall.VerticalLeft_2"));
        Tile.WallSprite.VerticalLeft.Add(GameManager.Instance.Resources.GetSprite("Wall.VerticalLeft_3"));

        Tile.WallSprite.VerticalRight.Add(GameManager.Instance.Resources.GetSprite("Wall.VerticalRight_1"));
        Tile.WallSprite.VerticalRight.Add(GameManager.Instance.Resources.GetSprite("Wall.VerticalRight_2"));
        Tile.WallSprite.VerticalRight.Add(GameManager.Instance.Resources.GetSprite("Wall.VerticalRight_3"));

        Tile.WallSprite.VerticalSplit.Add(GameManager.Instance.Resources.GetSprite("Wall.VerticalSplit_1"));
        Tile.WallSprite.VerticalSplit.Add(GameManager.Instance.Resources.GetSprite("Wall.VerticalSplit_2"));
        Tile.WallSprite.VerticalSplit.Add(GameManager.Instance.Resources.GetSprite("Wall.VerticalSplit_3"));
        Tile.WallSprite.VerticalSplit.Add(GameManager.Instance.Resources.GetSprite("Wall.VerticalSplit_4"));

        Tile.WallSprite.VerticalTop.Add(GameManager.Instance.Resources.GetSprite("Wall.VerticalTop_1"));

        RandomDepthCount.AddElement(10, 4);
        RandomDepthCount.AddElement(20, 3);
        RandomDepthCount.AddElement(30, 2);
        RandomDepthCount.AddElement(40, 1);
    }

    public void Generate(TileMap tileMap)
    {
        this.tileMap = tileMap;
        this.roomCount = tileMap.meta.roomCount;
        this.minRoomSize = Mathf.Max(MinRoomSize, tileMap.meta.minRoomSize);
        this.maxRoomSize = Mathf.Max(MinRoomSize, tileMap.meta.maxRoomSize);

        CreateRooms();
        SelectRooms();

        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.CreateGrid(DungeonGizmo.GroupName.BackgroundGrid, tileMap.rect));
        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.MoveCamera(tileMap.rect.center, GameManager.Instance.tickTime));
        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.RepositionRoom(new List<Room>(tileMap.rooms.Values)));

        CreateTiles();

        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.CreateGrid(DungeonGizmo.GroupName.BackgroundGrid, tileMap.rect));
        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.MoveCamera(tileMap.rect.center, GameManager.Instance.tickTime));
        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.RepositionRoom(new List<Room>(tileMap.rooms.Values)));

        ConnectRooms();

        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.EnableGizmo(DungeonGizmo.GroupName.RoomConnection, false));

        BuildWall();

        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.EnableGizmo(DungeonGizmo.GroupName.BackgroundGrid, false));
        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.EnableGizmo(DungeonGizmo.GroupName.Room, false));
        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.EnableGizmo(DungeonGizmo.GroupName.Corridor, false));
        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.ShowTileMap(tileMap, true));
        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.ShowTileMap(tileMap, false));
    }

    private void CreateRooms()
    {
        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.WriteDungeonLog("Room data generation process starts", Color.white));
        int roomIndex = 1;

        // 기준이 되는 방 3개 생성
        CreateRoom(roomIndex++, 0, 0);
        CreateRoom(roomIndex++, maxRoomSize * 2, 0);
        CreateRoom(roomIndex++, maxRoomSize / 2, maxRoomSize * 2);

        for (int i = 0; i < roomCount * 2; i++)
        {
            CreateRoom(roomIndex++);
        }
    }

    private void CreateRoom(int roomIndex, int x, int y)
    {
        int width = GetRandomRoomSize();
        int height = GetRandomRoomSize();
        Room room = new Room(roomIndex, x, y, width, height);
        rooms.Add(room);

        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.CreateRoom(room, TileMap.GetBoundaryRect(rooms), Color.blue));
    }

    private void CreateRoom(int roomIndex)
    {
        var triangulation = new DelaunayTriangulation(this.rooms);
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

        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.CreateTriangle(triangulation, biggestCircle));

        int width = GetRandomRoomSize();
        int height = GetRandomRoomSize();
        int x = (int)biggestCircle.center.x - width / 2;
        int y = (int)biggestCircle.center.y - height / 2;

        Room room = new Room(roomIndex++, (int)x, (int)y, width, height);
        this.rooms.Add(room);

        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.CreateRoom(room, TileMap.GetBoundaryRect(this.rooms), Color.red));

        RepositionBlocks(room.center, this.rooms);

        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.RepositionRoom(this.rooms));
        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.ChangeRoomColor(room, Color.blue));
    }

    private void RepositionBlocks(Vector3 center, List<Room> rooms)
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
                        Rect boundary = TileMap.GetBoundaryRect(rooms);
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
        if (boundary.width < boundary.height) // 블록 배치가 세로로 길게 되어 있음. 그래서 가로로 이동함
        {
            if (room1.center.x < room2.center.x) // 두 블록 중 block2가 오른쪽 있는 경우
            {
                if (center.x < room2.center.x)
                {
                    room2.x += 1; // block2가 중앙 보다 오른쪽에 있으면 block2를 오른쪽으로 1칸 이동
                }
                else
                {
                    room1.x -= 1; // block2가 중앙 보다 왼쪽에 있으면 block1을 왼쪽으로 1칸 이동
                }
            }
            else // 두 블록 중 block1이 오른쪽 있는 경우
            {
                if (center.x < room1.center.x)
                {
                    room1.x += 1; // block1이 중앙 보다 오른쪽에 있으면 block1를 오른쪽으로 1칸 이동
                }
                else
                {
                    room2.x -= 1; // block1가 중앙 보다 왼쪽에 있으면 block2를 왼쪽으로 1칸 이동
                }
            }
        }
        else // 블록 배치가 가로로 길게 되어 있음. 그래서 세로로 이동함
        {
            if (room1.center.y < room2.center.y)
            {
                if (center.y < room2.center.y)
                {
                    room2.y += 1; // block2가 중앙 보다 위에 있으면 block2를 윗쪽으로 1칸 이동
                }
                else
                {
                    room1.y -= 1; // block2가 중앙 보다 아래에 있으면 block1을 아래로 1칸 이동
                }
            }
            else
            {
                if (center.y < room1.center.y)
                {
                    room1.y += 1; // block1이 중앙 보다 위에 있으면 block1을 위로 1칸 이동
                }
                else
                {
                    room2.y -= 1;  // block1이 중앙 보다 아래에 있으면 block2를 아래로 1칸 이동
                }
            }
        }
    }

    private void SelectRooms()
    {
        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.WriteDungeonLog("Select room process starts", Color.white));

        var triangulation = new DelaunayTriangulation(this.rooms);
        var mst = new MinimumSpanningTree(this.rooms);
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

        Room startRoom = rooms[Random.Range(0, rooms.Count)];
        SelectRoom(startRoom, 1);

        while (this.roomCount > this.tileMap.rooms.Count)
        {
            Room room = rooms[0];
            if (false == this.tileMap.rooms.ContainsKey(room.index))
            {
                this.tileMap.rooms.Add(room.index, room);
                DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.ChangeRoomColor(room, Color.red));
            }
            rooms.RemoveAt(0);
        }

        foreach (Room room in rooms)
        {
            if (true == this.tileMap.rooms.ContainsKey(room.index))
            {
                continue;
            }

            DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.DestroyGizmo(DungeonGizmo.GroupName.Room, room.index));
        }
    }

    private void SelectRoom(Room room, int depth)
    {
        depth--;

        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.ChangeRoomColor(room, Color.yellow));

        if (0 == depth)
        {
            DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.WriteDungeonLog($"Room {room.index} is selected", Color.white));
            DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.ChangeRoomColor(room, Color.red));
        }
        else
        {
            DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.ChangeRoomColor(room, Color.blue));
        }

        if (0 == depth)
        {
            depth = GetRandomDepthCount();
            tileMap.rooms.Add(room.index, room);
        }

        while (0 < room.neighbors.Count && this.roomCount > tileMap.rooms.Count)
        {
            int index = Random.Range(0, room.neighbors.Count);
            Room neighbor = room.neighbors[index];

            neighbor.neighbors.Remove(room);
            room.neighbors.RemoveAt(index);

            SelectRoom(neighbor, depth);
        }
    }

    private int GetRandomRoomSize()
    {
        return Random.Range(minRoomSize, maxRoomSize + 1);
    }

    private int GetRandomDepthCount()
    {
        return TileGenerator.RandomDepthCount.Random();
    }
        
    #region ConnectRooms
    private void ConnectRooms()
    {
        foreach (var pair in this.tileMap.rooms)
        {
            Room room = pair.Value;
            room.neighbors.Clear();
        }

        List<Room> allRooms = new List<Room>(tileMap.rooms.Values);
        var triangulation = new DelaunayTriangulation(allRooms);
        var mst = new MinimumSpanningTree(allRooms);
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
            if (12.5f < UnityEngine.Random.Range(0.0f, 100.0f)) // 12.5 % 확률로 엣지 추가
            {
                continue;
            }

            if (true == mst.connections.Contains(edge))
            {
                continue;
            }

            mst.connections.Add(edge);
        }

        {
            var lines = new List<NDungeonEvent.NGizmo.CreateLine.Line>();
            foreach (var connection in mst.connections)
            {
                lines.Add(new NDungeonEvent.NGizmo.CreateLine.Line() { start = connection.room1.center, end = connection.room2.center });
            }
            DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.CreateLine(DungeonGizmo.GroupName.MiniumSpanningTree, lines, Color.green, DungeonGizmo.SortingOrder.SpanningTreeEdge, 0.5f));
        }

        foreach (var connection in mst.connections)
        {
            connection.room1.neighbors.Add(connection.room2);
            connection.room2.neighbors.Add(connection.room1);

            ConnectRoom(connection.room1, connection.room2);
        }

        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.EnableGizmo(DungeonGizmo.GroupName.MiniumSpanningTree, false));
    }
    private void ConnectRoom(Room a, Room b)
    {
        float xMin = Mathf.Max(a.rect.xMin, b.rect.xMin);
        float xMax = Mathf.Min(a.rect.xMax, b.rect.xMax);
        float yMin = Mathf.Max(a.rect.yMin, b.rect.yMin);
        float yMax = Mathf.Min(a.rect.yMax, b.rect.yMax);

        List<Vector3> positions = null;
        if (3 <= xMax - xMin) // x 축 겹칩. 세로 통로 만들기
        {
            positions = ConnectVerticalRoom(a, b);
        }
        else if (3 <= yMax - yMin) // y 축 겹침. 가로 통로 만들기
        {
            positions = ConnectHorizontalRoom(a, b);
        }
        else        // 꺾인 통로 만들어야 한다
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

        var start = tileMap.GetTile((int)startPosition.x, (int)startPosition.y);
        var end = tileMap.GetTile((int)endPosition.x, (int)endPosition.y);

        Rect searchBoundary = TileMap.GetBoundaryRect(new List<Room>() { a, b });
        AStarPathFinder pathFinder = new AStarPathFinder(tileMap, searchBoundary);
        Corridor corridor = new Corridor();
        corridor.tiles = pathFinder.FindPath(start, end);
        Debug.Assert(0 < corridor.tiles.Count);

        corridors.Add(corridor);

        foreach (var tile in corridor.tiles)
        {
            tile.type = Tile.Type.Floor;
            tile.cost = Tile.PathCost.MinCost;
        }

        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.CreateTile(DungeonGizmo.GroupName.Corridor, corridor.tiles, Color.blue, DungeonGizmo.SortingOrder.Corridor));
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
    #endregion

    private void CreateTiles()
    {
        tileMap.rect = TileMap.GetBoundaryRect(new List<Room>(tileMap.rooms.Values));
        tileMap.tiles = new Tile[tileMap.width * tileMap.height];
        // 전체 타일 초기화
        for (int i = 0; i < tileMap.width * tileMap.height; i++)
        {
            GameObject tileObject = new GameObject($"Tile_{i}");
            Tile tile = tileObject.AddComponent<Tile>();
            tile.index = i;
            tile.rect = new Rect(i % tileMap.width, i / tileMap.width, 1, 1);
            tile.type = Tile.Type.None;
            tile.cost = Tile.PathCost.MaxCost;
            tile.gameObject.transform.position = new Vector3(tile.rect.x, tile.rect.y);
            tile.gameObject.transform.SetParent(tileMap.gameObject.transform, false);
            tileMap.tiles[i] = tile;
        }

        foreach (var pair in tileMap.rooms)
        {
            Room room = pair.Value;
            // 블록들을 (0, 0) 기준으로 옮김
            room.rect.x -= tileMap.rect.xMin;
            room.rect.y -= tileMap.rect.yMin;

            for (int x = (int)room.rect.xMin; x < (int)room.rect.xMax; x++)
            {
                Tile top = tileMap.GetTile(x, (int)room.rect.yMax - 1);
                top.type = Tile.Type.Floor;
                top.cost = Tile.PathCost.MaxCost;
                top.room = room;

                Tile bottom = tileMap.GetTile(x, (int)room.rect.yMin);
                bottom.type = Tile.Type.Floor;
                bottom.cost = Tile.PathCost.MaxCost;
                bottom.room = room;
            }

            for (int y = (int)room.rect.yMin; y < (int)room.rect.yMax; y++)
            {
                Tile left = tileMap.GetTile((int)room.rect.xMin, y);
                left.type = Tile.Type.Floor;
                left.cost = Tile.PathCost.MaxCost;
                left.room = room;

                Tile right = tileMap.GetTile((int)room.rect.xMax - 1, y);
                right.type = Tile.Type.Floor;
                right.cost = Tile.PathCost.MaxCost;
                right.room = room;
            }

            {
                Tile lt = tileMap.GetTile((int)room.rect.xMin, (int)room.rect.yMax - 1);
                lt.type = Tile.Type.Wall;
                Tile rt = tileMap.GetTile((int)room.rect.xMax - 1, (int)room.rect.yMax - 1);
                rt.type = Tile.Type.Wall;
                Tile lb = tileMap.GetTile((int)room.rect.xMin, (int)room.rect.yMin);
                lb.type = Tile.Type.Wall;
                Tile rb = tileMap.GetTile((int)room.rect.xMax - 1, (int)room.rect.yMin);
                rb.type = Tile.Type.Wall;
            }

            // 방 내부 바닥 부분을 floor 타입으로 변경
            Rect floorRect = room.GetFloorRect();
            for (int y = (int)floorRect.yMin; y < (int)floorRect.yMax; y++)
            {
                for (int x = (int)floorRect.xMin; x < (int)floorRect.xMax; x++)
                {
                    Tile floor = tileMap.GetTile(x, y);
                    floor.type = Tile.Type.Floor;
                    floor.cost = Tile.PathCost.Floor;
                    floor.room = room;
                }
            }
        }

        tileMap.rect.x = 0;
        tileMap.rect.y = 0;
    }

    private void BuildWall()
    {
        foreach (var pair in tileMap.rooms)
        {
            Room room = pair.Value;
            for (int x = (int)room.rect.xMin; x < (int)room.rect.xMax; x++)
            {
                Tile top = tileMap.GetTile(x, (int)room.rect.yMax - 1);
                if (Tile.PathCost.MinCost < top.cost)
                {
                    top.type = Tile.Type.Wall;
                }
                else
                {
                    room.doors.Add(top);
                }

                Tile bottom = tileMap.GetTile(x, (int)room.rect.yMin);
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
                Tile left = tileMap.GetTile((int)room.rect.xMin, y);
                if (Tile.PathCost.MinCost < left.cost)
                {
                    left.type = Tile.Type.Wall;
                }
                else
                {
                    room.doors.Add(left);
                }

                Tile right = tileMap.GetTile((int)room.rect.xMax - 1, y);
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

                BuildWallOnTile(x - 1, y - 1);
                BuildWallOnTile(x - 1, y);
                BuildWallOnTile(x - 1, y + 1);
                BuildWallOnTile(x, y - 1);
                BuildWallOnTile(x, y + 1);
                BuildWallOnTile(x + 1, y - 1);
                BuildWallOnTile(x + 1, y);
                BuildWallOnTile(x + 1, y + 1);
            }
        }

        Vector3[] offsets = new Vector3[(int)Tile.Direction.Max];
        offsets[(int)Tile.Direction.LeftTop] = new Vector3(-1, +1);
        offsets[(int)Tile.Direction.Top] = new Vector3(0, +1);
        offsets[(int)Tile.Direction.RightTop] = new Vector3(+1, +1);
        offsets[(int)Tile.Direction.Left] = new Vector3(-1, 0);
        offsets[(int)Tile.Direction.Right] = new Vector3(+1, 0);
        offsets[(int)Tile.Direction.LeftBottom] = new Vector3(-1, -1);
        offsets[(int)Tile.Direction.Bottom] = new Vector3(0, -1);
        offsets[(int)Tile.Direction.RightBottom] = new Vector3(+1, -1);

        for (int i = 0; i < tileMap.width * tileMap.height; i++)
        {
            Tile tile = tileMap.GetTile(i);
            if (Tile.Type.None == tile.type)
            {
                tile.gameObject.transform.SetParent(null, false);
                GameObject.DestroyImmediate(tile.gameObject);
                tileMap.tiles[i] = null;
                continue;
            }

            tile.cost = Tile.PathCost.Floor;

            for (int direction = 0; direction < (int)Tile.Direction.Max; direction++)
            {
                tile.neighbors[direction] = null;

                var offset = offsets[direction];
                Tile neighbor = tileMap.GetTile((int)(tile.rect.x + offset.x), (int)(tile.rect.y + offset.y));
                if (null == neighbor)
                {
                    continue;
                }

                if (Tile.Type.None == neighbor.type)
                {
                    continue;
                }

                tile.neighbors[direction] = neighbor;
            }

            tile.spriteRenderer = tile.AddComponent<SpriteRenderer>();
            if (Tile.Type.Wall == tile.type)
            {
                tile.spriteRenderer.sprite = Tile.WallSprite.GetSprite(tile);
            }

            if (Tile.Type.Floor == tile.type)
            {
                tile.spriteRenderer.sprite = Tile.FloorSprite.GetSprite(tile);
            }

            tile.spriteRenderer.sortingOrder = Tile.SortingOrder;
            tile.spriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        }
    }

    private void BuildWallOnTile(int x, int y)
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
    }

    private class Rollback
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

        public void Execute()
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

        List<Tile> tiles = new List<Tile>();

        Rollback rollback = new Rollback();
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
                    var tile = tileMap.GetTile(x, y);
                    if (null == tile)
                    {
                        Debug.Log($"can not find tile(x:{x}, y:{y})");
                        rollback.Execute();
                        return false;
                    }

                    if (Tile.Type.Wall == tile.type)
                    {
                        Debug.Log($"fail to create path at tile_index:{tile.index}, x:{x}, y:{y}");
                        rollback.Execute();
                        return false;
                    }

                    rollback.Push(tile);
                    tile.cost = Mathf.Min(tile.cost, Tile.PathCost.Floor);
                    tiles.Add(tile);
                }
            }
        }

        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.CreateTile(DungeonGizmo.GroupName.RoomConnection, tiles, Color.white, DungeonGizmo.SortingOrder.Path));
        return true;
    }
}