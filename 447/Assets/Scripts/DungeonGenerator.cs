using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator
{
    public class Corridor
    {
        public List<Tile> path;
    }

    const int MinRoomSize = 5;
    int roomCount = 0;
    int minRoomSize = 0;
    int maxRoomSize = 0;
    WeightRandom<int> depthRandom;
    List<Room> rooms;
    List<Corridor> corridors;
    TileMap tileMap;

    public TileMap Generate(int roomCount, int minRoomSize, int maxRoomSize, int randomSeed = 0)
    {
        if (0 < randomSeed)
        {
			Random.InitState(randomSeed);
		}
		
		if (maxRoomSize < minRoomSize)
        {
            throw new System.ArgumentException("maximum room size should be equal or bigger than minimum room size");
        }

        if (0 >= roomCount)
        {
            return null;
        }

        this.roomCount = roomCount;  
        this.minRoomSize = minRoomSize;
        this.maxRoomSize = maxRoomSize;
        if (MinRoomSize > this.minRoomSize)
        {
            this.minRoomSize = MinRoomSize;
        }

        if (MinRoomSize > this.maxRoomSize)
        {
            this.maxRoomSize = MinRoomSize;
        }

        this.rooms = new List<Room>();
        this.corridors = new List<Corridor>();

        this.depthRandom = new WeightRandom<int>();
        this.depthRandom.AddElement(10, 3);
        this.depthRandom.AddElement(30, 2);
        this.depthRandom.AddElement(60, 1);

        List<Room> existRooms = CreateRooms(roomCount, minRoomSize, maxRoomSize);
        SelectRoom(existRooms);
        tileMap = new TileMap(rooms);

#if UNITY_EDITOR
        foreach (Room room in existRooms)
        {
            if (true == rooms.Contains(room))
            {
                continue;
            }

            GameManager.Instance.EnqueueEvent(new GameManager.DestroyRoomEvent(room));
        }

		GameManager.Instance.EnqueueEvent(new GameManager.CreateGridGizmoEvent(tileMap.rect));
		GameManager.Instance.EnqueueEvent(new GameManager.MoveCameraEvent(tileMap.rect.center, tileMap.rect));

		foreach (Room room in rooms)
        {
            GameManager.Instance.EnqueueEvent(new GameManager.MoveRoomEvent(room, tileMap.rect));
        }
#endif
		ConnectRoom();
        BuildWall();
#if UNITY_EDITOR
        GameManager.Instance.EnqueueEvent(new GameManager.CreateMinimumSpanningTreeEvent(null, Color.white));
        GameManager.Instance.EnqueueEvent(new GameManager.ClearCorridorGizmoEvent());
        foreach (Room room in rooms)
        {
            GameManager.Instance.EnqueueEvent(new GameManager.BuildRoomWallEvent(room));
        }
        GameManager.Instance.EnqueueEvent(new GameManager.BuildCorridorWallEvent(corridors));
#endif
        return tileMap;
    }

    private List<Room> CreateRooms(int roomCount, int minRoomSize, int maxRoomSize)
    {
        int roomIndex = 1;
        var rooms = new List<Room>();
        Room baseRoom1 = CreateRoom(roomIndex++, 0, 0);
        rooms.Add(baseRoom1);
#if UNITY_EDITOR
        GameManager.Instance.EnqueueEvent(new GameManager.CreateRoomEvent(baseRoom1, GetBoundaryRect(rooms), Color.blue));
#endif

        Room baseRoom2 = CreateRoom(roomIndex++, maxRoomSize * 2, 0);
        rooms.Add(baseRoom2);
#if UNITY_EDITOR
        GameManager.Instance.EnqueueEvent(new GameManager.CreateRoomEvent(baseRoom2, GetBoundaryRect(rooms), Color.blue));
#endif

        Room baseRoom3 = CreateRoom(roomIndex++, maxRoomSize / 2, maxRoomSize * 2);
        rooms.Add(baseRoom3);
#if UNITY_EDITOR
        GameManager.Instance.EnqueueEvent(new GameManager.CreateRoomEvent(baseRoom3, GetBoundaryRect(rooms), Color.blue));
#endif

        for (int i = 0; i < roomCount * 2; i++)
        {
            Room room = AddRoom(roomIndex++, rooms);
            rooms.Add(room);
#if UNITY_EDITOR
            GameManager.Instance.EnqueueEvent(new GameManager.CreateRoomEvent(room, GetBoundaryRect(rooms), Color.red));
#endif
            RepositionBlocks(room.center, rooms);

#if UNITY_EDITOR
            GameManager.Instance.EnqueueEvent(new GameManager.ChangeRoomColorEvent(room, Color.blue));
#endif
        }

        return rooms;
    }

    private Room CreateRoom(int roomIndex, int x, int y)
    {
        int width = GetRandomSize();
        int height = GetRandomSize();
        return new Room(roomIndex, x, y, width, height);
    }

    private Room AddRoom(int roomIndex, List<Room> exists)
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

#if UNITY_EDITOR
        {
            GameManager.FindRoomPositionEvent evt = new GameManager.FindRoomPositionEvent();
            evt.triangulation = triangulation;
            evt.biggestCircle = biggestCircle;
            GameManager.Instance.EnqueueEvent(evt);
        }
#endif

        int width   = GetRandomSize();
        int height  = GetRandomSize();
        int x       = (int)biggestCircle.center.x - width / 2;
        int y       = (int)biggestCircle.center.y - height / 2;

        return new Room(roomIndex++, (int)x, (int)y, width, height);
    }

    private void SelectRoom(List<Room> exists)
    {
        var triangulation = new DelaunayTriangulation(exists);
        var mst = new MinimumSpanningTree(exists);
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

        Room room = exists[Random.Range(0, exists.Count)];
        SelectRoom(room, 1);

        while (this.roomCount > this.rooms.Count)
        {
            Room block = exists[0];
            if (false == this.rooms.Contains(block))
            {
                this.rooms.Add(block);
#if UNITY_EDITOR
                GameManager.Instance.EnqueueEvent(new GameManager.ChangeRoomColorEvent(block, Color.red));
#endif
            }
            exists.RemoveAt(0);
        }
    }
    private void SelectRoom(Room room, int depth)
    {
        depth--;
#if UNITY_EDITOR
        GameManager.Instance.EnqueueEvent(new GameManager.ChangeRoomColorEvent(room, Color.yellow));

        if (0 == depth)
        {
            GameManager.Instance.EnqueueEvent(new GameManager.ChangeRoomColorEvent(room, Color.red));
        }
        else
        {
            GameManager.Instance.EnqueueEvent(new GameManager.ChangeRoomColorEvent(room, Color.blue));
        }
#endif

        if (0 == depth)
        {
            depth = GetRandomDepth();
            rooms.Add(room);
        }
        
        while (0 < room.neighbors.Count && this.roomCount > rooms.Count)
        {
            int index = Random.Range(0, room.neighbors.Count);
            Room neighbor = room.neighbors[index];

            neighbor.neighbors.Remove(room);
            room.neighbors.RemoveAt(index);

            SelectRoom(neighbor, depth);
        }
    }

    private void ConnectRoom()
    {
        foreach (Room room in rooms)
        {
            room.neighbors.Clear();
        }

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

#if UNITY_EDITOR
        GameManager.Instance.EnqueueEvent(new GameManager.CreateMinimumSpanningTreeEvent(mst, Color.green));
#endif
        foreach (var connection in mst.connections)
        {
            connection.p1.neighbors.Add(connection.p2);
            connection.p2.neighbors.Add(connection.p1);

            ConnectRoom(connection.p1, connection.p2);
        }
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

        Rect searchBoundary = DungeonGenerator.GetBoundaryRect(new List<Room>() { a, b });
        AStarPathFinder pathFinder = new AStarPathFinder(tileMap, searchBoundary, new AStarPathFinder.RandomLookup());
		Corridor corridor = new Corridor();
		corridor.path = pathFinder.FindPath(start, end);
        Debug.Assert(0 < corridor.path.Count);

        corridors.Add(corridor);

		foreach (var tile in corridor.path)
		{
			tile.type = Tile.Type.Floor;
			tile.cost = Tile.PathCost.MinCost;
#if UNITY_EDITOR
			GameManager.Instance.EnqueueEvent(new GameManager.CreateTileGizmoEvent(tile, Color.blue, GameManager.SortingOrder.Floor));
#endif
		}
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

        int upperY = (int)upperRoom.rect.yMin;
        int bottomY = (int)bottomRoom.rect.yMax - 1;

        Vector3 start = new Vector3(x, upperY);
        Vector3 end = new Vector3(x, bottomY);
        AdjustTileCostOnCorridor(new List<Vector3>() { start, end });
#if UNITY_EDITOR
        GameManager.Instance.EnqueueEvent(
            new GameManager.CreateCorridorGizmoEvent(
                $"Corridor_Vertical_{a.index}_{b.index}",
                new Vector3(x + 0.5f, upperRoom.rect.yMin + 1, 0.0f),
                new Vector3(x + 0.5f, bottomRoom.rect.yMax - 1, 0.0f),
                Color.white, 0.5f, GameManager.SortingOrder.Corridor
            )
        );
#endif
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

        int leftX = (int)leftRoom.rect.xMax - 1;
        int rightX = (int)rightRoom.rect.xMin;

        Vector3 start = new Vector3(leftX, y);
        Vector3 end = new Vector3(rightX, y);
		AdjustTileCostOnCorridor(new List<Vector3>() { start, end });
#if UNITY_EDITOR
		GameManager.Instance.EnqueueEvent(
            new GameManager.CreateCorridorGizmoEvent(
                $"Corridor_Horizontal_{a.index}_{b.index}",
                new Vector3(leftX, y + 0.5f),
                new Vector3(rightX + 1, y + 0.5f),
                Color.white, 0.5f, GameManager.SortingOrder.Corridor
            )
        );
#endif
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
                new Vector3(leftRoomX, leftRoom.rect.yMin, 0.0f),
                new Vector3(leftRoomX, bottomRoomY),
                new Vector3(rightRoom.rect.xMin, bottomRoomY)
            };

            var corridorLT = new List<Vector3>()
            {
                new Vector3(leftRoom.rect.xMax - 1, upperRoomY, 0.0f),
                new Vector3(rightRoomX, upperRoomY),
                new Vector3(rightRoomX, rightRoom.rect.yMax - 1)
            };

            if (0 == Random.Range(0, 2))
            {
                bool result = AdjustTileCostOnCorridor(corridorBL);
                if (false == result)
                {
                    AdjustTileCostOnCorridor(corridorLT);
                }
            }
            else
            {
                bool result = AdjustTileCostOnCorridor(corridorLT);
                if (false == result)
                {
                    AdjustTileCostOnCorridor(corridorBL);
                }
            }

#if UNITY_EDITOR
            GameManager.Instance.EnqueueEvent(
                new GameManager.CreateCorridorGizmoEvent(
                    $"Corridor_{a.index}_{b.index}_BL_세로",
                    new Vector3(corridorBL[0].x + 0.5f, corridorBL[0].y),
                    new Vector3(corridorBL[1].x + 0.5f, corridorBL[1].y),
                    Color.white, 0.5f, GameManager.SortingOrder.Corridor
                )
            );
            GameManager.Instance.EnqueueEvent(
                new GameManager.CreateCorridorGizmoEvent(
                    $"Corridor_{a.index}_{b.index}_BL_가로",
                    new Vector3(corridorBL[1].x, corridorBL[1].y + 0.5f),
                    new Vector3(corridorBL[2].x, corridorBL[2].y + 0.5f),
                    Color.white, 0.5f, GameManager.SortingOrder.Corridor
                )
            );
            GameManager.Instance.EnqueueEvent(
                new GameManager.CreateCorridorGizmoEvent(
                    $"Corridor_{a.index}_{b.index}_RT_가로",
                    new Vector3(corridorLT[0].x + 1, corridorLT[0].y + 0.5f),
                    new Vector3(corridorLT[1].x + 1, corridorLT[1].y + 0.5f),
                    Color.white, 0.5f, GameManager.SortingOrder.Corridor
                )
            );
            GameManager.Instance.EnqueueEvent(
                new GameManager.CreateCorridorGizmoEvent(
                    $"Corridor_{a.index}_{b.index}_RT_세로",
                    new Vector3(corridorLT[1].x + 0.5f, corridorLT[1].y + 1),
                    new Vector3(corridorLT[2].x + 0.5f, corridorLT[2].y + 1),
                    Color.white, 0.5f, GameManager.SortingOrder.Corridor
                )
            );
#endif
            return;
        }

        if (bottomRoom == leftRoom)
        {
            var corridorTL = new List<Vector3>()
            {
                new Vector3(leftRoomX, bottomRoom.rect.yMax - 1, 0.0f),
                new Vector3(leftRoomX, upperRoomY),
                new Vector3(rightRoom.rect.xMin, upperRoomY)
            };

            var corridorRB = new List<Vector3>()
            {
                new Vector3(leftRoom.rect.xMax - 1, bottomRoomY, 0.0f),
                new Vector3(rightRoomX, bottomRoomY),
                new Vector3(rightRoomX, upperRoom.rect.yMin)
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
#if UNITY_EDITOR
			GameManager.Instance.EnqueueEvent(
                new GameManager.CreateCorridorGizmoEvent(
                    $"Corridor_{a.index}_{b.index}_TL_세로",
                    new Vector3(corridorRB[0].x + 0.5f, corridorRB[0].y + 1),
                    new Vector3(corridorRB[1].x + 0.5f, corridorRB[1].y + 1),
                    Color.white, 0.5f, GameManager.SortingOrder.Corridor
                )
            );
            GameManager.Instance.EnqueueEvent(
                new GameManager.CreateCorridorGizmoEvent(
                    $"Corridor_{a.index}_{b.index}_TL_가로",
                    new Vector3(corridorRB[1].x, corridorRB[1].y + 0.5f),
                    new Vector3(corridorRB[2].x, corridorRB[2].y + 0.5f),
                    Color.white, 0.5f, GameManager.SortingOrder.Corridor
                )
            );
            GameManager.Instance.EnqueueEvent(
                new GameManager.CreateCorridorGizmoEvent(
                    $"Corridor_{a.index}_{b.index}_RB_가로",
                    new Vector3(corridorTL[0].x + 1, corridorTL[0].y + 0.5f),
                    new Vector3(corridorTL[1].x + 1, corridorTL[1].y + 0.5f),
                    Color.white, 0.5f, GameManager.SortingOrder.Corridor
                )
            );
            GameManager.Instance.EnqueueEvent(
                new GameManager.CreateCorridorGizmoEvent(
                    $"Corridor_{a.index}_{b.index}_RB_세로",
                    new Vector3(corridorTL[1].x + 0.5f, corridorTL[1].y),
                    new Vector3(corridorTL[2].x + 0.5f, corridorTL[2].y),
                    Color.white, 0.5f, GameManager.SortingOrder.Corridor
                )
            );
#endif
            return;
        }
    }

    private void BuildWall()
    {
        foreach (Room room in rooms)
        {
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
            foreach (Tile tile in corridor.path)
            {
                tile.type = Tile.Type.Floor;
                tile.cost = Tile.PathCost.Floor;
            }
        }

        foreach (var corridor in corridors)
        {
            foreach (Tile tile in corridor.path)
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

        foreach (Room room in rooms)
        {
            Rect floorRect = room.GetFloorRect();
            for (int y = (int)floorRect.yMin; y < (int)floorRect.yMax - 1; y++)
            {
                for (int x = (int)floorRect.xMin; x < (int)floorRect.xMax - 1; x++)
                {
                    Tile floor = tileMap.GetTile(x, y);
                    floor.type = Tile.Type.Floor;
                    floor.cost = Tile.PathCost.Floor;
                }
            }
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
				}
            }
		}

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

#if UNITY_EDITOR
        foreach (Room room in rooms)
        {
            GameManager.Instance.EnqueueEvent(new GameManager.MoveRoomEvent(room, GetBoundaryRect(rooms)));
        }
#endif
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