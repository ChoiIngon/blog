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

        {
            this.depthRandom = new WeightRandom<int>();
            this.depthRandom.AddElement( 5, 4);
            this.depthRandom.AddElement(15, 3);
            this.depthRandom.AddElement(30, 2);
            this.depthRandom.AddElement(10, 1);
        }

        int roomIndex = 1;
        List<Room> existRooms = new List<Room>();

        Room baseRoom1 = CreateRoom(roomIndex++, 0, 0);
        existRooms.Add(baseRoom1);
#if UNITY_EDITOR
        GameManager.Instance.EnqueueEvent(new GameManager.CreateRoomEvent(baseRoom1, GetBoundaryRect(existRooms), Color.blue));
#endif

        Room baseRoom2 = CreateRoom(roomIndex++, maxRoomSize * 2, 0);
        existRooms.Add(baseRoom2);
#if UNITY_EDITOR
        GameManager.Instance.EnqueueEvent(new GameManager.CreateRoomEvent(baseRoom2, GetBoundaryRect(existRooms), Color.blue));
#endif

        Room baseRoom3 = CreateRoom(roomIndex++, maxRoomSize / 2, maxRoomSize * 2);
        existRooms.Add(baseRoom3);
#if UNITY_EDITOR
        GameManager.Instance.EnqueueEvent(new GameManager.CreateRoomEvent(baseRoom3, GetBoundaryRect(existRooms), Color.blue));
#endif

        for (int i = 0; i < roomCount * 2; i++)
        {
            Room room = AddRoom(roomIndex++, existRooms);
            existRooms.Add(room);
#if UNITY_EDITOR
            GameManager.Instance.EnqueueEvent(new GameManager.CreateRoomEvent(room, GetBoundaryRect(existRooms), Color.red));
#endif
            DungeonGenerator.RepositionBlocks(room.center, existRooms);

#if UNITY_EDITOR
            GameManager.Instance.EnqueueEvent(new GameManager.ChangeRoomColorEvent(room, Color.blue));
#endif
        }

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

        foreach (Room room in rooms)
        {
            GameManager.Instance.EnqueueEvent(new GameManager.BuildRoomWallEvent(room));
        }
#endif
		ConnectRoom();

        System.Action<int, int> IfNotNullBuildWall = (int x, int y) =>
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

        foreach (var corridor in corridors)
        {
            foreach (Tile tile in corridor.path)
            {
                tile.type = Tile.Type.Floor;
            }
        }

        foreach(var corridor in corridors)
        {
            foreach (Tile tile in corridor.path)
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

#if UNITY_EDITOR
        //GameManager.Instance.EnqueueEvent(new GameManager.CreateMinimumSpanningTreeEvent(null, Color.white));
        //GameManager.Instance.EnqueueEvent(new GameManager.ClearCorridorGizmoEvent());
        GameManager.Instance.EnqueueEvent(new GameManager.BuildCorridorWallEvent(corridors));
#endif
        return tileMap;
    }

    public Room CreateRoom(int roomIndex, int x, int y)
    {
        int width = GetRandomSize();
        int height = GetRandomSize();
        return new Room(roomIndex, x, y, width, height);
    }

    public Room AddRoom(int roomIndex, List<Room> exists)
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

    public void SelectRoom(List<Room> exists)
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
    public void SelectRoom(Room room, int depth)
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

    public void ConnectRoom()
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
    public void ConnectRoom(Room a, Room b)
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

    private Corridor ConnectVerticalRoom(Room a, Room b)
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
        Corridor corridor = AdjustTileCostOnCorridor(new List<Vector3>() { start, end });
#if UNITY_EDITOR
        Color color = Color.white;
        if (null == corridor)
        {
            color = Color.blue;
        }
        GameManager.Instance.EnqueueEvent(
            new GameManager.CreateCorridorGizmoEvent(
                $"Corridor_Vertical_{a.index}_{b.index}",
                new Vector3(x + 0.5f, upperRoom.rect.yMin + 1, 0.0f),
                new Vector3(x + 0.5f, bottomRoom.rect.yMax - 1, 0.0f),
                color, 0.5f, GameManager.SortingOrder.Corridor
            )
        );

        //GameManager.Instance.EnqueueEvent(new GameManager.CreateTileGizmoEvent(startTile, Color.yellow, 1.0f, GameManager.SortingOrder.Door));
        //GameManager.Instance.EnqueueEvent(new GameManager.CreateTileGizmoEvent(endTile, Color.yellow, 1.0f, GameManager.SortingOrder.Door));
#endif
        return corridor;
    }

    private Corridor ConnectHorizontalRoom(Room a, Room b)
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
		Corridor corridor = AdjustTileCostOnCorridor(new List<Vector3>() { start, end });
#if UNITY_EDITOR
		Color color = Color.white;
		if (null == corridor)
		{
			color = Color.blue;
		}
		GameManager.Instance.EnqueueEvent(
            new GameManager.CreateCorridorGizmoEvent(
                $"Corridor_Horizontal_{a.index}_{b.index}",
                new Vector3(leftX, y + 0.5f),
                new Vector3(rightX + 1, y + 0.5f),
                color, 0.5f, GameManager.SortingOrder.Corridor
            )
        );
        //GameManager.Instance.EnqueueEvent(new GameManager.CreateTileGizmoEvent(startTile, Color.yellow, 1.0f, GameManager.SortingOrder.Door));
        //GameManager.Instance.EnqueueEvent(new GameManager.CreateTileGizmoEvent(endTile, Color.yellow, 1.0f, GameManager.SortingOrder.Door));
#endif
        return corridor;
    }

    private Corridor ConnectDiagonalRoom(Room a, Room b)
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

        Corridor corridor = null;

        if (upperRoom == leftRoom)
        {
            Corridor path_BL = null;
            Corridor path_RT = null;

            {
                Vector3 start   = new Vector3(leftRoomX, leftRoom.rect.yMin, 0.0f);
                Vector3 middle  = new Vector3(leftRoomX, bottomRoomY);
                Vector3 end     = new Vector3(rightRoom.rect.xMin, bottomRoomY);

                path_BL = AdjustTileCostOnCorridor(new List<Vector3>() { start, middle, end });
#if UNITY_EDITOR
				Color color = Color.white;
				if (null == path_BL)
				{
					color = Color.blue;
				}

				GameManager.Instance.EnqueueEvent(
                    new GameManager.CreateCorridorGizmoEvent(
                        $"Corridor_{a.index}_{b.index}_BL_세로",
                        new Vector3(start.x + 0.5f, start.y),
                        new Vector3(middle.x + 0.5f, middle.y),
                        color, 0.5f, GameManager.SortingOrder.Corridor
                    )
                );
                GameManager.Instance.EnqueueEvent(
                    new GameManager.CreateCorridorGizmoEvent(
                        $"Corridor_{a.index}_{b.index}_BL_가로",
                        new Vector3(middle.x, middle.y + 0.5f),
                        new Vector3(end.x, end.y + 0.5f),
                        color, 0.5f, GameManager.SortingOrder.Corridor
                    )
                );
				//GameManager.Instance.EnqueueEvent(new GameManager.CreateTileGizmoEvent(startTile, Color.yellow, 1.0f, GameManager.SortingOrder.Door));
				//GameManager.Instance.EnqueueEvent(new GameManager.CreateTileGizmoEvent(endTile, Color.yellow, 1.0f, GameManager.SortingOrder.Door));
#endif
			}

			{
                Vector3 start   = new Vector3(leftRoom.rect.xMax - 1, upperRoomY, 0.0f);
                Vector3 middle  = new Vector3(rightRoomX, upperRoomY);
                Vector3 end     = new Vector3(rightRoomX, rightRoom.rect.yMax - 1);
                
                path_RT = AdjustTileCostOnCorridor(new List<Vector3>() { start, middle, end });
#if UNITY_EDITOR
				Color color = Color.white;
				if (null == path_RT)
				{
					color = Color.blue;
				}
				GameManager.Instance.EnqueueEvent(
                    new GameManager.CreateCorridorGizmoEvent(
                        $"Corridor_{a.index}_{b.index}_RT_가로",
                        new Vector3(start.x + 1, start.y + 0.5f),
                        new Vector3(middle.x + 1, middle.y + 0.5f),
                        color, 0.5f, GameManager.SortingOrder.Corridor
                    )
                );
                GameManager.Instance.EnqueueEvent(
                    new GameManager.CreateCorridorGizmoEvent(
                        $"Corridor_{a.index}_{b.index}_RT_세로",
                        new Vector3(middle.x + 0.5f, middle.y + 1),
                        new Vector3(end.x + 0.5f, end.y + 1),
                        color, 0.5f, GameManager.SortingOrder.Corridor
                    )
                );
				//GameManager.Instance.EnqueueEvent(new GameManager.CreateTileGizmoEvent(startTile, Color.yellow, 1.0f, GameManager.SortingOrder.Door));
				//GameManager.Instance.EnqueueEvent(new GameManager.CreateTileGizmoEvent(endTile, Color.yellow, 1.0f, GameManager.SortingOrder.Door));
#endif
			}

			Debug.Assert(null != path_BL || null != path_RT);

			int path_BL_count = null != path_BL ? path_BL.path.Count : int.MaxValue;
			int path_RT_count = null != path_RT ? path_RT.path.Count : int.MaxValue;

            corridor = path_BL;
			if (path_RT_count < path_BL_count)
			{
				corridor = path_RT;
			}
        }

        if (bottomRoom == leftRoom)
        {
            Corridor path_TL = null;
            Corridor path_RB = null;

            {
                Vector3 start = new Vector3(leftRoomX, bottomRoom.rect.yMax - 1, 0.0f);
                Vector3 middle = new Vector3(leftRoomX, upperRoomY);
                Vector3 end = new Vector3(rightRoom.rect.xMin, upperRoomY);
                
                path_TL = AdjustTileCostOnCorridor(new List<Vector3>() { start, middle, end });
#if UNITY_EDITOR
				Color color = Color.white;
				if (null == path_TL)
				{
					color = Color.blue;
				}
				GameManager.Instance.EnqueueEvent(
                    new GameManager.CreateCorridorGizmoEvent(
                        $"Corridor_{a.index}_{b.index}_TL_세로",
                        new Vector3(start.x + 0.5f, start.y + 1),
                        new Vector3(middle.x + 0.5f, middle.y + 1),
                        color, 0.5f, GameManager.SortingOrder.Corridor
                    )
                );
                GameManager.Instance.EnqueueEvent(
                    new GameManager.CreateCorridorGizmoEvent(
                        $"Corridor_{a.index}_{b.index}_TL_가로",
                        new Vector3(middle.x, middle.y + 0.5f),
                        new Vector3(end.x, end.y + 0.5f),
                        color, 0.5f, GameManager.SortingOrder.Corridor
                    )
                );
				//GameManager.Instance.EnqueueEvent(new GameManager.CreateTileGizmoEvent(startTile, Color.yellow, 1.0f, GameManager.SortingOrder.Door));
				//GameManager.Instance.EnqueueEvent(new GameManager.CreateTileGizmoEvent(endTile, Color.yellow, 1.0f, GameManager.SortingOrder.Door));
#endif
			}

			{
                Vector3 start = new Vector3(leftRoom.rect.xMax - 1, bottomRoomY, 0.0f);
                Vector3 middle = new Vector3(rightRoomX, bottomRoomY);
                Vector3 end = new Vector3(rightRoomX, upperRoom.rect.yMin);
                path_RB = AdjustTileCostOnCorridor(new List<Vector3>() { start, middle, end });
#if UNITY_EDITOR
				Color color = Color.white;
				if (null == path_RB)
				{
					color = Color.blue;
				}
				GameManager.Instance.EnqueueEvent(
                    new GameManager.CreateCorridorGizmoEvent(
                        $"Corridor_{a.index}_{b.index}_RB_가로",
                        new Vector3(start.x + 1, start.y + 0.5f),
                        new Vector3(middle.x + 1, middle.y + 0.5f),
                        color, 0.5f, GameManager.SortingOrder.Corridor
                    )
                );
                GameManager.Instance.EnqueueEvent(
                    new GameManager.CreateCorridorGizmoEvent(
                        $"Corridor_{a.index}_{b.index}_RB_세로",
                        new Vector3(middle.x + 0.5f, middle.y),
                        new Vector3(end.x + 0.5f, end.y),
                        color, 0.5f, GameManager.SortingOrder.Corridor
                    )
                );
				//GameManager.Instance.EnqueueEvent(new GameManager.CreateTileGizmoEvent(startTile, Color.yellow, 1.0f, GameManager.SortingOrder.Door));
				//GameManager.Instance.EnqueueEvent(new GameManager.CreateTileGizmoEvent(endTile, Color.yellow, 1.0f, GameManager.SortingOrder.Door));

#endif
			}

			Debug.Assert(null != path_TL || null != path_RB);

			int path_TL_count = null != path_TL ? path_TL.path.Count : int.MaxValue;
			int path_RB_count = null != path_RB ? path_RB.path.Count : int.MaxValue;

			corridor = path_TL;
			if (path_RB_count < path_TL_count)
			{
				corridor = path_RB;
			}
        }

        return corridor;
    }

    private Corridor AdjustTileCostOnCorridor(List<Vector3> positions)
    {
        if (2 > positions.Count)
        {
            return null;
        }

        Corridor corridor = new Corridor();
        corridor.path = new List<Tile>();

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
                        return null;
                    }

					if (Tile.Type.Wall == tile.type)
					{
						Debug.Log($"fail to create path at tile_index:{tile.index}, x:{x}, y:{y}");
						rollback.Execute();
						return null;
					}

					rollback.Push(tile);
					tile.cost = Tile.PathCost.Floor;
                    corridor.path.Add(tile);
				}
            }
		}

        return corridor;
	}

    private Corridor CreateCorridor(Tile start, Tile corner, Tile end)
    {
        Rollback rollback = new Rollback();

        rollback.Push(start);
        start.type = Tile.Type.Floor;
        start.cost = Tile.PathCost.Floor;
        
        rollback.Push(end);
        end.type = Tile.Type.Floor;
        end.cost = Tile.PathCost.Floor;

        Rect searchBoundary = DungeonGenerator.GetBoundaryRect(new List<Room>() { start.room, end.room });
        var startTile = tileMap.GetTile((int)start.room.center.x, (int)start.room.center.y);
        var endTile = tileMap.GetTile((int)end.room.center.x, (int)end.room.center.y);

        Corridor corridor = new Corridor();

        {
            int xMin = (int)Mathf.Min(start.rect.x, corner.rect.x);
            int xMax = (int)Mathf.Max(start.rect.x, corner.rect.x);
            int yMin = (int)Mathf.Min(start.rect.y, corner.rect.y);
            int yMax = (int)Mathf.Max(start.rect.y, corner.rect.y);

            for (int y = yMin; y <= yMax; y++)
            {
                for (int x = xMin; x <= xMax; x++)
                {
                    var tile = tileMap.GetTile(x, y);
                    if (null == tile)
                    {
                        rollback.Execute();
                        return null;
                    }

                    if (Tile.Type.Wall == tile.type)
                    {
						Debug.Log($"fail to create path at tile_index:{tile.index}, x:{x}, y:{y}");
						rollback.Execute();
                        return null;
                    }

                    rollback.Push(tile);
                    tile.type = Tile.Type.Floor;
                    tile.cost = Tile.PathCost.Floor;
                }
            }
        }

        {
            int xMin = (int)Mathf.Min(corner.rect.x, end.rect.x);
            int xMax = (int)Mathf.Max(corner.rect.x, end.rect.x);
            int yMin = (int)Mathf.Min(corner.rect.y, end.rect.y);
            int yMax = (int)Mathf.Max(corner.rect.y, end.rect.y);

            for (int y = yMin; y <= yMax; y++)
            {
                for (int x = xMin; x <= xMax; x++)
                {
                    var tile = tileMap.GetTile(x, y);
                    if (null == tile)
                    {
                        rollback.Execute();
                        return null;
                    }

                    if (Tile.Type.Wall == tile.type)
                    {
                        Debug.Log($"fail to create path at tile_index:{tile.index}, x:{x}, y:{y}");
                        rollback.Execute();
                        return null;
                    }

                    rollback.Push(tile);
                    tile.type = Tile.Type.Floor;
                    tile.cost = Tile.PathCost.Floor;
                }
            }
        }

        AStarPathFinder pathFinder = new AStarPathFinder(tileMap, searchBoundary, new AStarPathFinder.RandomLookup());
        corridor.path = pathFinder.FindPath(startTile, endTile);
        rollback.Execute();
        return corridor;
    }

    private int GetRandomSize()
    {
        return Random.Range(minRoomSize, maxRoomSize + 1);
    }
    private int GetRandomDepth()
    {
        return depthRandom.Random();
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
    public static void RepositionBlocks(Vector3 center, List<Room> rooms)
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
                        DungeonGenerator.ResolveOverlap(center, boundary, rooms[i], rooms[j]);
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
    
    public static void ResolveOverlap(Vector3 center, Rect boundary, Room room1, Room room2)
    {
        if (boundary.width < boundary.height) // 블록 배치가 세로로 길게 되어 있음. 그래서 가로로 이동함
        {
            if (room1.x < room2.x) // 두 블록 중 block2가 오른쪽 있는 경우
            {
                if (center.x < room2.x)
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
                if (center.x < room1.x)
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
            if (room1.y < room2.y)
            {
                if (center.y < room2.y)
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
                if (center.y < room1.y)
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

    // 두 개의 Vector2를 사용하여 선분과 선분이 교차하는지 검사
    public static bool LineIntersects(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
    {
        float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

        Vector2 r = p2 - p1;
        Vector2 s = q2 - q1;
        float rxs = Cross(r, s);
        Vector2 qp = q1 - p1;
        float qpxr = Cross(qp, r);

        if (rxs == 0 && qpxr == 0)
        {
            // 두 선이 일직선상에 있으며, 겹치는지 확인
            return !(Mathf.Max(p1.x, p2.x) < Mathf.Min(q1.x, q2.x) ||
                     Mathf.Max(p1.y, p2.y) < Mathf.Min(q1.y, q2.y));
        }

        if (rxs == 0) return false; // 두 선이 평행함

        float t = Cross(qp, s) / rxs;
        float u = qpxr / rxs;

        return (t >= 0 && t <= 1) && (u >= 0 && u <= 1);
    }

    // 선분과 Rect가 겹치는지 검사
    public static bool LineIntersectsRect(Vector3 start, Vector3 end, Rect rect)
    {
        // 1. 선분이 사각형 내부에 있는지 확인
        if (rect.Contains(start) || rect.Contains(end))
        {
            return true;
        }

        // 2. 사각형의 네 변을 확인
        Vector2 topLeft = new Vector2(rect.xMin, rect.yMax);
        Vector2 topRight = new Vector2(rect.xMax, rect.yMax);
        Vector2 bottomLeft = new Vector2(rect.xMin, rect.yMin);
        Vector2 bottomRight = new Vector2(rect.xMax, rect.yMin);

        // 선분과 사각형의 네 변 간의 충돌 검사
        return LineIntersects(start, end, topLeft, topRight) ||
               LineIntersects(start, end, topRight, bottomRight) ||
               LineIntersects(start, end, bottomRight, bottomLeft) ||
               LineIntersects(start, end, bottomLeft, topLeft);
    }
}