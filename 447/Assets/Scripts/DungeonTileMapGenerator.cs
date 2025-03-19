using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DungeonTileMapGenerator
{
    const int MinRoomSize = 5;

    int roomCount = 0;
    int minRoomSize = 0;
    int maxRoomSize = 0;
    WeightRandom<int> depthRandom;
    List<Corridor> corridors;
    TileMap tileMap;

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
    }

    public TileMap Generate(int roomCount, int minRoomSize, int maxRoomSize, int randomSeed)
    {
		Random.InitState(randomSeed);
		
		if (maxRoomSize < minRoomSize)
        {
            throw new System.ArgumentException("maximum room size should be equal or bigger than minimum room size");
        }

        if (0 >= roomCount)
        {
            return null;
        }

        this.roomCount = roomCount;  
        this.minRoomSize = Mathf.Max(MinRoomSize, minRoomSize);
        this.maxRoomSize = Mathf.Max(MinRoomSize, maxRoomSize);

        this.depthRandom = new WeightRandom<int>();
        this.depthRandom.AddElement(10, 4);
        this.depthRandom.AddElement(20, 3);
        this.depthRandom.AddElement(30, 2);
        this.depthRandom.AddElement(40, 1);

        this.corridors = new List<Corridor>();

        List<Room> mockupRooms      = CreateRooms(roomCount, minRoomSize, maxRoomSize);
        List<Room> selectedRooms    = SelectRooms(mockupRooms);

        if (null != tileMap)
        {
            tileMap.Clear();
        }

        tileMap = new TileMap(selectedRooms);

        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.CreateGrid(DungeonGizmo.GroupName.BackgroundGrid, tileMap.rect));
        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.MoveCamera(tileMap.rect.center, GameManager.Instance.tickTime));
        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.RepositionRoom(selectedRooms));

        ConnectRooms(tileMap);

        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.EnableGizmo(DungeonGizmo.GroupName.Path, false));

        BuildWall(tileMap);

        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.EnableGizmo(DungeonGizmo.GroupName.BackgroundGrid, false));

		//DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.Enable(DungeonGizmo.GroupName.Tile, false));
		//DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.CreatedRoomWall(new List<Room>(tileMap.rooms.Values)));
		//DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.CreateCorridorWall(corridors));
		DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.EnableGizmo(DungeonGizmo.GroupName.Room, false));
		DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.EnableGizmo(DungeonGizmo.GroupName.Corridor, false));
		DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.ShowTileMap(tileMap, true));
		return tileMap;
    }

    #region CreateRooms
    private List<Room> CreateRooms(int roomCount, int minRoomSize, int maxRoomSize)
    {
        int roomIndex = 1;
        var rooms = new List<Room>();

        Room baseRoom1 = CreateRoom(roomIndex++, 0, 0);
        rooms.Add(baseRoom1);

        Room baseRoom2 = CreateRoom(roomIndex++, maxRoomSize * 2, 0);
        rooms.Add(baseRoom2);

        Room baseRoom3 = CreateRoom(roomIndex++, maxRoomSize / 2, maxRoomSize * 2);
        rooms.Add(baseRoom3);

        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.WriteDungeonLog("Room data generation process starts", Color.white));
        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.CreateRoom(baseRoom1, GetBoundaryRect(rooms), Color.blue));
        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.CreateRoom(baseRoom2, GetBoundaryRect(rooms), Color.blue));
        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.CreateRoom(baseRoom3, GetBoundaryRect(rooms), Color.blue));

        for (int i = 0; i < roomCount * 2; i++)
        {
            Room room = CreateRoom(roomIndex++, rooms);
            rooms.Add(room);
            DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.CreateRoom(room, GetBoundaryRect(rooms), Color.red));

            RepositionBlocks(room.center, rooms);

            DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.RepositionRoom(rooms));
            DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.ChangeRoomColor(room, Color.blue));
        }

        return rooms;
    }
    private Room CreateRoom(int roomIndex, int x, int y)
    {
        int width = GetRandomSize();
        int height = GetRandomSize();
        return new Room(roomIndex, x, y, width, height);
    }
    private Room CreateRoom(int roomIndex, List<Room> exists)
    {
        var triangulation = new DelaunayTriangulation(exists);
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

        int width   = GetRandomSize();
        int height  = GetRandomSize();
        int x       = (int)biggestCircle.center.x - width / 2;
        int y       = (int)biggestCircle.center.y - height / 2;

        return new Room(roomIndex++, (int)x, (int)y, width, height);
    }
    #endregion

    #region SelectRooms
    private List<Room> SelectRooms(List<Room> mockupRooms)
    {
        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.WriteDungeonLog("Select room process starts", Color.white));

        var triangulation = new DelaunayTriangulation(mockupRooms);
        var mst = new MinimumSpanningTree(mockupRooms);
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
            connection.p1.neighbors.Add(connection.p2);
            connection.p2.neighbors.Add(connection.p1);
        }

        List<Room> selectedRooms = new List<Room>();

        Room startRoom = mockupRooms[Random.Range(0, mockupRooms.Count)];
        SelectRoom(startRoom, 1, selectedRooms);

        while (this.roomCount > selectedRooms.Count)
        {
            Room room = mockupRooms[0];
            if (false == selectedRooms.Contains(room))
            {
                selectedRooms.Add(room);
                DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.ChangeRoomColor(room, Color.red));
            }
            mockupRooms.RemoveAt(0);
        }

        foreach (Room room in mockupRooms)
        {
            if (true == selectedRooms.Contains(room))
            {
                continue;
            }

            DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.DestroyGizmo(DungeonGizmo.GroupName.Room, room.index));
        }

        return selectedRooms;
    }
    private void SelectRoom(Room room, int depth, List<Room> selectedRooms)
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
            depth = GetRandomDepth();
            selectedRooms.Add(room);
        }
        
        while (0 < room.neighbors.Count && this.roomCount > selectedRooms.Count)
        {
            int index = Random.Range(0, room.neighbors.Count);
            Room neighbor = room.neighbors[index];

            neighbor.neighbors.Remove(room);
            room.neighbors.RemoveAt(index);

            SelectRoom(neighbor, depth, selectedRooms);
        }
    }
    #endregion

    #region ConnectRooms
    private void ConnectRooms(TileMap tileMap)
    {
        foreach (var pair in tileMap.rooms)
        {
            Room room = pair.Value;
            room.neighbors.Clear();
        }

        List<Room> rooms = new List<Room>(tileMap.rooms.Values);
        var triangulation = new DelaunayTriangulation(rooms);
        var mst = new MinimumSpanningTree(rooms);
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
                lines.Add(new NDungeonEvent.NGizmo.CreateLine.Line() { start = connection.p1.center, end = connection.p2.center });
            }
            DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.CreateLine(DungeonGizmo.GroupName.MiniumSpanningTree, lines, Color.green, DungeonGizmo.SortingOrder.SpanningTreeEdge, 0.5f));
        }

        foreach (var connection in mst.connections)
        {
            connection.p1.neighbors.Add(connection.p2);
            connection.p2.neighbors.Add(connection.p1);

            ConnectRoom(connection.p1, connection.p2);
        }

        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.EnableGizmo(DungeonGizmo.GroupName.MiniumSpanningTree, false));
    }
    private void ConnectRoom(Room a, Room b)
    {
        float xMin = Mathf.Max(a.rect.xMin, b.rect.xMin);
        float xMax = Mathf.Min(a.rect.xMax, b.rect.xMax);
        float yMin = Mathf.Max(a.rect.yMin, b.rect.yMin);
        float yMax = Mathf.Min(a.rect.yMax, b.rect.yMax);

        if (3 <= xMax - xMin) // x 축 겹칩. 세로 통로 만들기
        {
			ConnectVerticalRoom(a, b);
        }
        else if (3 <= yMax - yMin) // y 축 겹침. 가로 통로 만들기
        {
            ConnectHorizontalRoom(a, b);
        }
        else        // 꺾인 통로 만들어야 한다
        {
            ConnectDiagonalRoom(a, b);
        }

        var start = tileMap.GetTile((int)a.center.x, (int)a.center.y);
        var end = tileMap.GetTile((int)b.center.x, (int)b.center.y);

        Rect searchBoundary = DungeonTileMapGenerator.GetBoundaryRect(new List<Room>() { a, b });
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
	private void ConnectVerticalRoom(Room a, Room b)
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
        AdjustTileCostOnCorridor(new List<Vector3>() { start, end });
    }
    private void ConnectHorizontalRoom(Room a, Room b)
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
		AdjustTileCostOnCorridor(new List<Vector3>() { start, end });
    }
    private void ConnectDiagonalRoom(Room a, Room b)
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

        int upperRoomY  = (int)Random.Range(upperRoom.rect.yMin + 1 + yOverlap, upperRoom.rect.yMax - 2);
        int bottomRoomY = (int)Random.Range(bottomRoom.rect.yMin + 1, bottomRoom.rect.yMax - 2 - yOverlap);
        int leftRoomX   = (int)Random.Range(leftRoom.rect.xMin + 1, leftRoom.rect.xMax - 2 - xOverlap);
        int rightRoomX  = (int)Random.Range(rightRoom.rect.xMin + 1 + xOverlap, rightRoom.rect.xMax - 2);

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

            if (0 == Random.Range(0, 2))
            {
                bool result = AdjustTileCostOnCorridor(corridorBL);
                if (false == result)
                {
                    AdjustTileCostOnCorridor(corridorRT);
                }
            }
            else
            {
                bool result = AdjustTileCostOnCorridor(corridorRT);
                if (false == result)
                {
                    AdjustTileCostOnCorridor(corridorBL);
                }
            }
            return;
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

            if (0 == Random.Range(0, 2))
            {
                bool result = AdjustTileCostOnCorridor(corridorTL);
                if (false == result)
                {
                    AdjustTileCostOnCorridor(corridorRB);
                }
            }
            else
            {
                bool result = AdjustTileCostOnCorridor(corridorRB);
                if (false == result)
                {
                    AdjustTileCostOnCorridor(corridorTL);
                }
            }
            return;
        }
    }
    #endregion

    private void BuildWall(TileMap tileMap)
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
        offsets[(int)Tile.Direction.LeftTop]     = new Vector3(-1, +1);
        offsets[(int)Tile.Direction.Top]         = new Vector3( 0, +1);
        offsets[(int)Tile.Direction.RightTop]    = new Vector3(+1, +1);
        offsets[(int)Tile.Direction.Left]        = new Vector3(-1,  0);
        offsets[(int)Tile.Direction.Right]       = new Vector3(+1,  0);
        offsets[(int)Tile.Direction.LeftBottom]  = new Vector3(-1, -1);
        offsets[(int)Tile.Direction.Bottom]      = new Vector3( 0, -1);
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

    public static Rect GetBoundaryRect(List<Room> rooms)
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
					tile.cost = Tile.PathCost.Floor;

                    tiles.Add(tile);
                }
            }
		}

        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.CreateTile(DungeonGizmo.GroupName.Path, tiles, Color.white, DungeonGizmo.SortingOrder.Path));
        return true;
	}
    
    private int GetRandomSize()
    {
        return Random.Range(minRoomSize, maxRoomSize + 1);
    }
    private int GetRandomDepth()
    {
        return depthRandom.Random();
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
}