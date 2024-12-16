using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class DungeonGenerator
{
    private const int MinRoomSize = 5;

    private int roomCount = 0;
    private int minRoomSize = 0;
    private int maxRoomSize = 0;

    public class Tile
    {
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
        public enum Type
        {
            None,
            Corridor,
            Room
        }

        public int index;
        public Rect rect;
        public Type type;
        public Vector3 connectPoint;
		
        public Block(int index, float x, float y, float width, float height)
        {
            this.index = index;
            this.type = Type.None;
            this.rect = new Rect(x, y, width, height);
            this.connectPoint = Vector3.zero;
        }
    }

    public TileMap tilemap = null;
	public DelaunayTriangulation triangulation = null;
	public MinimumSpanningTree graph = null;

    public List<Block> blocks = new List<Block>();
	private List<Block> rooms = new List<Block>();
		
	public DungeonGenerator(int roomCount, int minRoomSize, int maxRoomSize)
    {
        this.roomCount = roomCount;
        this.minRoomSize = Mathf.Max(MinRoomSize, minRoomSize);
        this.maxRoomSize = Mathf.Max(MinRoomSize, maxRoomSize);

		CreateBlocks();
		CreateConnectEdge();
		ConnectRooms();
		BuildWall();
    }

    #region Create Block ���� �Լ�
    private void CreateBlocks()
    {
        WeightRandom<int> weightRandom = CreateWeightRandom(minRoomSize, maxRoomSize);
        
        int roomIndex = 1;	// �� ���� ���� ��ȣ �Ҵ�
        
        float meanRoomSize = (minRoomSize + maxRoomSize) / 2;
        float rangeMultiply = 1.0f; // ��ġ�� �簢���� �߻��ϸ� �簢�� ���� ������ ���� �ֱ� ���� ����
        
        for (int i = 0; i < this.roomCount; i++)
        {
            float theta = 2.0f * Mathf.PI * UnityEngine.Random.Range(0.0f, 1.0f);   // https://kukuta.tistory.com/199
            float radius = meanRoomSize * rangeMultiply;

            int x = (int)(radius * Mathf.Cos(theta));
            int y = (int)(radius * Mathf.Sin(theta));
            int width = weightRandom.Random();
            int height = weightRandom.Random();
            Block block = new Block(roomIndex, x, y, width, height);

            int overlapCount = 0;	// ��ġ�� ���� ���� ���ϱ�
            foreach (Block other in blocks)
            {
                if (true == other.rect.Overlaps(block.rect))
                {
                    overlapCount++;
                }
            }
            
            if (5 <= overlapCount) // ��ġ�� �� ������ 5�� �̻��̸� ���� �Ǵ� ������ Ȯ���Ѵ�.
            {
                rangeMultiply += 1.0f;
                RepositionBlocks();	// ��ģ ���� ��ġ�� ���� ��ġ�� �ʴ� �������� �缳��
            }

            block.type = Block.Type.Room;
            blocks.Add(block);
            rooms.Add(block);
        }

        for (int i = 0; i < rooms.Count; i++)
        {
            Block room = rooms[i];
            for(int j = i+1; j < rooms.Count; j++) 
            {
                Block neighbor = rooms[j];
                
                float distance = Vector3.Distance(room.rect.center, neighbor.rect.center);
                float roomRadius = Vector3.Distance(room.rect.center, new Vector3(room.rect.x, room.rect.y));
                float neighorRadius = Vector3.Distance(neighbor.rect.center, new Vector3(neighbor.rect.x, neighbor.rect.y));
                
                if (distance < roomRadius + neighorRadius)
                {
                    Vector3 interpolation = Vector3.Lerp(room.rect.center, neighbor.rect.center, 0.5f);
                    int width = weightRandom.Random() / 2;
                    int height = weightRandom.Random() / 2;
                    int x = (int)(interpolation.x - width / 2);
                    int y = (int)(interpolation.y - height / 2);
                    
                    Block block = new Block(roomIndex++, x, y, width, height);
                    block.type = Block.Type.Corridor;
                    blocks.Add(block);
                }
            }
            RepositionBlocks();
        }

        this.tilemap = new TileMap(blocks);
    }

    private WeightRandom<int> CreateWeightRandom(int min, int max)
    {
        var weightRandom = new WeightRandom<int>();

        #region build room size random object // range�� ��� ������ �ٱ����� ���� �� ���� Ȯ���� �������� ��ġ
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
        blocks = Shuffle(blocks);

        while (true)
        {
            bool overlap = false;

            for (int i = 0; i < blocks.Count; i++)
            {
                for (int j = i + 1; j < blocks.Count; j++)
                {
                    if (true == blocks[i].rect.Overlaps(blocks[j].rect))
                    {
                        ResolveOverlap(blocks[i], blocks[j]);
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

    private void ResolveOverlap(Block block1, Block block2)
	{
		int dx = (int)Mathf.Min(
			Mathf.Abs(block1.rect.x + block1.rect.width - block2.rect.x),
			Mathf.Abs(block2.rect.x + block2.rect.width - block1.rect.x)
		);

		int dy = (int)Mathf.Min(
			Mathf.Abs(block1.rect.y + block1.rect.height - block2.rect.y),
			Mathf.Abs(block2.rect.y + block2.rect.height - block1.rect.y)
		);

		if (dx < dy) // x������ �̵�
		{
			if (block1.rect.x < block2.rect.x)
			{
				block2.rect.x += dx;
			}
			else
			{
				block1.rect.x += dx;
			}
		}
		else // y������ �̵�
		{
			if (block1.rect.y < block2.rect.y)
			{
				block2.rect.y += dy;
			}
			else
			{
				block1.rect.y += dy;
			}
		}
	}
	#endregion

	private void CreateConnectEdge()
	{
		// ����� �ﰢ���ҹ��� �̿��� ���� ����� ã�Ƴ�
        this.triangulation = new DelaunayTriangulation(rooms);
        this.graph = new MinimumSpanningTree(rooms);
		
		foreach (var triangle in triangulation.triangles)
		{
			foreach (var edge in triangle.edges) 
			{
                graph.AddEdge(new MinimumSpanningTree.Edge(edge.v0.block, edge.v1.block, Vector3.Distance(edge.v0.block.rect.center, edge.v1.block.rect.center)));
			}
		}

		graph.BuildTree();

		foreach (var edge in graph.edges)
		{
			if (12.5f < UnityEngine.Random.Range(0.0f, 100.0f)) // 12.5 % Ȯ���� ���� �߰�
			{
				continue;
			}

            if (true == graph.connections.Contains(edge))
			{
				continue;
			}

			graph.connections.Add(edge);
		}
    }

	private void ConnectRooms()
	{
        Dictionary<Block, List<Block>> searchLimitBoundaries = new Dictionary<Block, List<Block>>();
        foreach (var edge in graph.connections)
        {
			if (false == searchLimitBoundaries.ContainsKey(edge.p1))
			{
				searchLimitBoundaries.Add(edge.p1, new List<Block>());
            }

            if (false == searchLimitBoundaries.ContainsKey(edge.p2))
            {
                searchLimitBoundaries.Add(edge.p2, new List<Block>());
            }

            searchLimitBoundaries[edge.p1].Add(edge.p2);
            searchLimitBoundaries[edge.p2].Add(edge.p1);
        }

        long elapsedTime = 0;
        foreach (var edge in graph.connections)
		{
			Block src = edge.p1;
			Block dest = edge.p2;

        	Tile from = tilemap.GetTile((int)src.rect.center.x, (int)src.rect.center.y);
			Tile to = tilemap.GetTile((int)dest.rect.center.x, (int)dest.rect.center.y);

			// �ӵ��� ���̱� ���� AStar �˻� ������ �����Ѵ�f.
			Rect searchLimitBoundary = new Rect();
			searchLimitBoundary.xMin = Mathf.Min(src.rect.xMin, to.rect.xMin);
            searchLimitBoundary.xMax = Mathf.Max(src.rect.xMax, to.rect.xMax);
            searchLimitBoundary.yMin = Mathf.Min(src.rect.yMin, to.rect.yMin);
            searchLimitBoundary.yMax = Mathf.Max(src.rect.yMax, to.rect.yMax);

            foreach (var neighbor in searchLimitBoundaries[src])
			{
                searchLimitBoundary.xMin = Mathf.Min(searchLimitBoundary.xMin, neighbor.rect.xMin);
                searchLimitBoundary.xMax = Mathf.Max(searchLimitBoundary.xMax, neighbor.rect.xMax);
                searchLimitBoundary.yMin = Mathf.Min(searchLimitBoundary.yMin, neighbor.rect.yMin);
                searchLimitBoundary.yMax = Mathf.Max(searchLimitBoundary.yMax, neighbor.rect.yMax);
            }

            foreach (var neighbor in searchLimitBoundaries[dest])
            {
                searchLimitBoundary.xMin = Mathf.Min(searchLimitBoundary.xMin, neighbor.rect.xMin);
                searchLimitBoundary.xMax = Mathf.Max(searchLimitBoundary.xMax, neighbor.rect.xMax);
                searchLimitBoundary.yMin = Mathf.Min(searchLimitBoundary.yMin, neighbor.rect.yMin);
                searchLimitBoundary.yMax = Mathf.Max(searchLimitBoundary.yMax, neighbor.rect.yMax);
            }

            Stopwatch stopWatch = new Stopwatch();
			stopWatch.Start();
			var path = tilemap.FindPath(from, to, searchLimitBoundary);
			stopWatch.Stop();

			UnityEngine.Debug.Log($"{edge.p1.index} -> {edge.p2.index}, elasped time:{stopWatch.ElapsedMilliseconds}");
			elapsedTime += stopWatch.ElapsedMilliseconds;

            foreach (var tile in path)
			{
				tile.cost = 1;
                edge.path.Add(tile);
            }
		}

		UnityEngine.Debug.Log($"total elapsed time:{elapsedTime}, connection count:{graph.connections.Count}");
	}

	private void BuildWall()
	{
		foreach (Block room in rooms)
		{
			for (int y = (int)room.rect.yMin; y < (int)room.rect.yMax - 1; y++)
			{
				Tile left = tilemap.GetTile((int)room.rect.xMin, y);
				left.type = Tile.Type.Wall;

				Tile right = tilemap.GetTile((int)room.rect.xMax - 1, y);
				right.type = Tile.Type.Wall;
			}
			
			for (int x = (int)room.rect.xMin; x < (int)room.rect.xMax - 1; x++)
			{
				Tile upper = tilemap.GetTile(x, (int)room.rect.yMin);
				upper.type = Tile.Type.Wall;

				Tile bottom = tilemap.GetTile(x, (int)room.rect.yMax - 1);
				bottom.type = Tile.Type.Wall;
			}
		}

		foreach (var edge in graph.connections)
		{
			foreach (var tile in edge.path)
			{
				tile.type = Tile.Type.Floor;
			}
		}

        foreach (var edge in graph.connections)
        {
            foreach (var tile in edge.path)
            {
				int x = (int)tile.rect.x;
				int y = (int)tile.rect.y;
				
				Action<int, int> IfNotNullBuildWall = (int x, int y) => 
				{
                    Tile tile = tilemap.GetTile(x, y);
					if (null == tile)
					{
						return;
					}

					if (Tile.Type.None != tile.type)
					{
						return;
					}

					tile.type = Tile.Type.Wall;
                };

				IfNotNullBuildWall(x - 1, y - 1);
                IfNotNullBuildWall(x - 1, y);
                IfNotNullBuildWall(x - 1, y + 1);
                IfNotNullBuildWall(x    , y - 1);
                IfNotNullBuildWall(x	, y + 1);
                IfNotNullBuildWall(x + 1, y - 1);
                IfNotNullBuildWall(x + 1, y);
                IfNotNullBuildWall(x + 1, y + 1);
            }
        }
    }

	private List<T> Shuffle<T>(List<T> list)
	{
		var shuffled = new List<T>(list);
		for (int i = 0; i < shuffled.Count; ++i)
		{
			int random = UnityEngine.Random.Range(0, shuffled.Count);

			T temp = shuffled[i];
			shuffled[i] = shuffled[random];
			shuffled[random] = temp;
		}

		return shuffled;
	}

    public class TileMap
    {
        private class Node
        {
            public Tile tile;
            public Node parent;
            public int index { get { return tile.index; } }
            public int pathCost;
            public int expectCost;
            public int cost { get { return pathCost + expectCost; } }

            public Node(Tile tile)
            {
                this.tile = tile;
                this.pathCost = 0;
                this.expectCost = 0;
            }
        }

        public static Vector2Int[] LOOKUP_OFFSETS = {
            new Vector2Int(-1, 0),
            new Vector2Int( 0,-1),
            new Vector2Int( 1, 0),
            new Vector2Int( 0, 1)
        };

        private const int DefaultTileCost = 12;
        private const int FloorTileCost = 11;
        private const int CorridorTileWeight = -1;
        private const int WallTileCost = 13;

        public Tile[] tiles = null;
        public int width = 0;
        public int height = 0;

        public TileMap(List<Block> blocks)
        {
            int xMin = int.MaxValue, xMax = int.MinValue;
            int yMin = int.MaxValue, yMax = int.MinValue;
            foreach (Block block in blocks)
            {
                xMin = Mathf.Min((int)xMin, (int)block.rect.xMin);
                xMax = Mathf.Max((int)xMax, (int)block.rect.xMax);
                yMin = Mathf.Min((int)yMin, (int)block.rect.yMin);
                yMax = Mathf.Max((int)yMax, (int)block.rect.yMax);
            }

            this.width = xMax - xMin;
            this.height = yMax - yMin;
            tiles = new Tile[width * height];
            // ��ü Ÿ�� �ʱ�ȭ
            for (int i = 0; i < width * height; i++)
            {
                Tile tile = new Tile();
                tile.index = i;
                tile.rect = new Rect(i % width, i / width, 1, 1);
                tile.type = Tile.Type.None;
                tile.cost = DefaultTileCost;
                tiles[i] = tile;
            }

            foreach (Block block in blocks)
            {
                // ��ϵ��� (0, 0) �������� �ű�
                block.rect.x -= xMin;
                block.rect.y -= yMin;

                { // ��� Ÿ���� cost�� ���� ��� ���ַ� ���� ���鵵�� ��
                    for (int y = (int)block.rect.yMin; y < (int)block.rect.yMax; y++)
                    {
                        for (int x = (int)block.rect.xMin; x < (int)block.rect.xMax; x++)
                        {
                            Tile tile = GetTile(x, y);
                            tile.cost = FloorTileCost;
                        }
                    }
                }

                { // ����� ���� cost�� ���� ���� ���� ���� �ȵ��� ����
                    for (int x = (int)block.rect.xMin; x < (int)block.rect.xMax; x++)
                    {
                        Tile wallTop = GetTile(x, (int)block.rect.yMin);
                        wallTop.cost = WallTileCost;

                        Tile wallBottom = GetTile(x, (int)block.rect.yMax - 1);
                        wallBottom.cost = WallTileCost;
                    }

                    for (int y = (int)block.rect.yMin; y < (int)block.rect.yMax; y++)
                    {
                        Tile wallLeft = GetTile((int)block.rect.xMin, y);
                        wallLeft.cost = WallTileCost;

                        Tile wallRight = GetTile((int)block.rect.xMax - 1, y);
                        wallRight.cost = WallTileCost;
                    }
                }

                { // ����� ����� cost�� ���� ����� ����� ���� ������ ����
                    for (int x = (int)block.rect.xMin + 1; x < (int)block.rect.xMax - 1; x++)
                    {
                        Tile corridor = GetTile(x, (int)block.rect.center.y);
                        corridor.cost = FloorTileCost;
                        corridor.cost -= CorridorTileWeight;
                    }

                    for (int y = (int)block.rect.yMin + 1; y < (int)block.rect.yMax - 1; y++)
                    {
                        Tile corridor = GetTile((int)block.rect.center.x, y);
                        corridor.cost = FloorTileCost;
                        corridor.cost -= CorridorTileWeight;
                    }
                }

                // �� ����� ��� �濡 ���� �ʱ�ȭ
                if (Block.Type.Room == block.type)
                {
                    InitRoomBlock(block);
                }
            }
        }

        public List<Tile> FindPath(Tile from, Tile to, Rect? boundaryRect = null)
        {
            List<Tile> path = new List<Tile>();

            Dictionary<int, Node> openNodes = new Dictionary<int, Node>(); // ���� ���. ��� Ÿ�� �ֺ� ��ֹ��� �����ϰ� ������ �� �ִ� Ÿ�� ���. �� Ÿ�ϵ��� ��� Ÿ���� �θ�� �����Ѵ�. �θ� ���� ��θ� �� Ž���� �� �Ž��� �ö󰡴� �뵵�� ��� �ȴ�.
            Dictionary<int, Node> closeNodes = new Dictionary<int, Node>(); // ���� ���. �̹� �˻��� ������ �ٽ� �� �ʿ� ���� �簢����

            Node current = new Node(from);
            current.expectCost += (int)Mathf.Abs(to.index % this.width - from.index % this.width);
            current.expectCost += (int)Mathf.Abs(to.index / this.width - from.index / this.width);
            openNodes.Add(current.index, current);

            while (0 < openNodes.Count)
            {
                List<Node> sortedNodes = openNodes.Values.ToList<Node>();
                if (0 == sortedNodes.Count)
                {
                    break;  // ��� ã�� ����
                }

                sortedNodes.Sort((Node lhs, Node rhs) => {
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

                current = sortedNodes[0];

                List<Node> children = new List<Node>();
                //int offsetIndex = UnityEngine.Random.Range(0, LOOKUP_OFFSETS.Length);
                for (int i = 0; i < LOOKUP_OFFSETS.Length; i++)// ��ֹ��� ���� �ִµ� ���� �� �� �ִ� Ÿ�ϵ��� openNode ����Ʈ�� �ִ´�
                {
                    //var offset = LOOKUP_OFFSETS[offsetIndex];
                    var offset = LOOKUP_OFFSETS[i];

                    int x = current.index % this.width + offset.x;
                    int y = current.index / this.width + offset.y;

                    //offsetIndex += 1;
                    //offsetIndex %= LOOKUP_OFFSETS.Length; // ���� Ÿ���� ������ �� ���� ���� �ֱ� ����

                    Tile tile = this.GetTile(x, y, boundaryRect);
                    if (null == tile)
                    {
                        continue;
                    }

                    if (to == tile)
                    {
                        path.Add(tile);
                        do
                        {
                            path.Add(current.tile);
                            current = current.parent;
                        } while (null != current);
                        return path;
                    }

                    if (Tile.Type.Wall == tile.type)
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
                        if (openNode.pathCost + tile.cost < current.pathCost)
                        {
                            current.pathCost = openNode.pathCost + tile.cost;
                            current.parent = openNode;
                        }
                        continue;
                    }

                    Node child = new Node(tile);
                    child.parent = current;
                    child.pathCost = current.pathCost + tile.cost;
                    child.expectCost += (int)Mathf.Abs(to.index % this.width - tile.index % this.width);
                    child.expectCost += (int)Mathf.Abs(to.index / this.width - tile.index / this.width);
                    openNodes.Add(child.index, child);
                }

                openNodes.Remove(current.index);
                closeNodes.Add(current.index, current);
            }

            return path;
        }

        public Tile SearchTile(int index)
        {
            int x = index % width;
            int y = index / width;
            return GetTile(x, y);
        }

        public Tile GetTile(int x, int y, Rect? boundaryRect = null)
        {
            if (null != boundaryRect)
            {
                if (boundaryRect?.xMin > x || x >= boundaryRect?.xMax)
                {
                    return null;
                }

                if (boundaryRect?.yMin > y || y >= boundaryRect?.yMax)
                {
                    return null;
                }
            }

            if (0 > x || x >= width)
            {
                return null;
            }

            if (0 > y || y >= height)
            {
                return null;
            }

            return this.tiles[y * width + x];
        }

        private void InitRoomBlock(Block room)
        {
            for (int y = (int)room.rect.yMin; y < (int)room.rect.yMax; y++)
            {
                for (int x = (int)room.rect.xMin; x < (int)room.rect.xMax; x++)
                {
                    Tile tile = GetTile(x, y);
                    tile.type = Tile.Type.Floor;
                }
            }

            // ���� �𼭸��� ���� ����� ���� �����ϱ� ���� ��� �� ��տ� ���� �̸� ����
            int xMin = (int)room.rect.xMin;
            int yMin = (int)room.rect.yMin;
            int xMax = (int)room.rect.xMax;
            int yMax = (int)room.rect.yMax;

            // left bottom
            //GetTile(xMin, yMin + 1).type = Tile.Type.Wall;
            GetTile(xMin, yMin).type = Tile.Type.Wall;
            //GetTile(xMin + 1, yMin).type = Tile.Type.Wall;

            //GetTile(xMax - 1, yMin + 1).type = Tile.Type.Wall;
            GetTile(xMax - 1, yMin).type = Tile.Type.Wall;
            //GetTile(xMax - 2, yMin).type = Tile.Type.Wall;

            //GetTile(xMin, yMax - 2).type = Tile.Type.Wall;
            GetTile(xMin, yMax - 1).type = Tile.Type.Wall;
            //GetTile(xMin + 1, yMax - 1).type = Tile.Type.Wall;

            //GetTile(xMax - 1, yMax - 2).type = Tile.Type.Wall;
            GetTile(xMax - 1, yMax - 1).type = Tile.Type.Wall;
            //GetTile(xMax - 2, yMax - 1).type = Tile.Type.Wall;
        }
    }

    #region ��Ÿ ���� Ŭ������
    public class WeightRandom<T>
	{
		public class Element
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
	
	public class DelaunayTriangulation
	{
		public class Point
		{
			public Vector3 position;
			public Block block;

			public Point(Vector3 position, Block block)
			{
				this.position = position;
				this.block = block;
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

					return Vector3.Distance(v0.position, v1.position);
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
			public List<Edge> edges;

			public Triangle(Point p1, Point p2, Point p3)
			{
				this.a = p1.position;
				this.b = p2.position;
				this.c = p3.position;

				this.circumCircle = calcCircumCircle(a, b, c);
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

			private Circle calcCircumCircle(Vector3 a, Vector3 b, Vector3 c)
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
		}

		public Triangle superTriangle = null;
		public List<Triangle> triangles = new List<Triangle>();

		public DelaunayTriangulation(List<Block> blocks)
		{
			superTriangle = CreateSuperTriangle(blocks);
			if (null == superTriangle)
			{
				return;
			}

			triangles.Add(superTriangle);

			foreach(var block in blocks)
			{
				AddPoint(block);
			}

			RemoveSuperTriangle();
		}

		public void AddPoint(Block block)
		{
			Vector3 point = block.rect.center;

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
				Triangle triangle = CreateTriangle(edge.v0, edge.v1, new Point(point, block));
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

		private Triangle CreateSuperTriangle(List<Block> blocks)
		{
			float minX = float.MaxValue;
			float maxX = float.MinValue;
			float minY = float.MaxValue;
			float maxY = float.MinValue;

			foreach (Block block in blocks)
			{
				Vector3 point = block.rect.center;
				minX = Mathf.Min(minX, point.x);
				maxX = Mathf.Max(maxX, point.x);
				minY = Mathf.Min(minY, point.y);
				maxY = Mathf.Max(maxY, point.y);
			}

			float dx = maxX - minX;
			float dy = maxY - minY;

			// super triangle�� ����Ʈ ����Ʈ ���� ũ�� ��� ������
			// super triangle�� ���� ����Ʈ�� ��ġ�� �Ǹ� �ﰢ���� �ƴ� ������ �ǹǷ� ���γ� �ﰢ������ ������ �� ���� �����̴�.
			Vector3 a = new Vector3(minX - dx, minY - dy);
			Vector3 b = new Vector3(minX - dx, maxY + dy * 3);
			Vector3 c = new Vector3(maxX + dx * 3, minY - dy);

			// super triangle�� ������ ��� ����
			if (a == b || b == c || c == a)
			{
				return null;
			}

			return new Triangle(new Point(a, null), new Point(b, null), new Point(c, null));
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
			public Edge(Block p1, Block p2, float cost)
			{
				this.p1 = p1;
				this.p2 = p2;
				this.cost = cost;
				this.path = new List<Tile>();
			}
				
			public Block p1;
			public Block p2;
			public float cost;
			public List<Tile> path;
		}

		private Dictionary<Block, Block> parents = new Dictionary<Block, Block>();
		public List<Edge> edges = new List<Edge>();
		public List<Edge> connections = new List<Edge>();

		public MinimumSpanningTree(List<Block> rooms)
		{
			foreach (Block room in rooms)
			{
				parents.Add(room, room);
			}
		}

		public void AddEdge(Edge edge)
		{
			foreach (Edge other in edges)
			{
				if(true == (edge.p1 == other.p1 && edge.p2 == other.p2) || (edge.p1 == other.p2 && edge.p2 == other.p1))
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
				Block srcParent = FindParent(edge.p1);
				Block destParent = FindParent(edge.p2);

				if (srcParent != destParent)
				{
					connections.Add(edge);
					Union(srcParent, destParent);
				}
			}
		}

		private Block FindParent(Block room)
		{
			var parent = parents[room];
			if (parent != room)
			{
				parents[room] = FindParent(parent);
			}
			return parents[room];
		}

		private void Union(Block src, Block dest)
		{
			Block srcParent = FindParent(src);
			Block destParent = FindParent(dest);
			parents[srcParent] = destParent;
		}
	}
	#endregion
}
