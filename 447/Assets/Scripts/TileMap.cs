using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TileMap
{
    public GameObject gameObject;

    public readonly Tile[] tiles;
    public readonly Dictionary<int, Room> rooms = new Dictionary<int, Room>();
    public readonly Rect rect;

    public int width
    {
        get { return (int)rect.width; }
    }
    public int height
    {
        get { return (int)rect.height; }
    }

    public Tile start;
    public Tile end;

    public TileMap(List<Room> rooms)
    {
        this.gameObject = new GameObject("TileMap");
        this.rect = DungeonTileMapGenerator.GetBoundaryRect(rooms);
        this.tiles = new Tile[width * height];
        // 전체 타일 초기화
        for (int i = 0; i < width * height; i++)
        {
            GameObject tileObject = new GameObject($"Tile_{i}");
            Tile tile = tileObject.AddComponent<Tile>();
            tile.index = i;
            tile.rect = new Rect(i % width, i / width, 1, 1);
            tile.type = Tile.Type.None;
            tile.cost = Tile.PathCost.MaxCost;
            tile.gameObject.transform.position = new Vector3(tile.rect.x + tile.rect.width/2, tile.rect.y + tile.rect.height / 2);
            tile.gameObject.transform.SetParent(gameObject.transform, false);   
            tiles[i] = tile;
        }
        
        foreach (Room room in rooms)
        {
            this.rooms.Add(room.index, room);
            // 블록들을 (0, 0) 기준으로 옮김
            room.rect.x -= rect.xMin;
            room.rect.y -= rect.yMin;

			for (int x = (int)room.rect.xMin; x < (int)room.rect.xMax; x++)
            {
                Tile top = GetTile(x, (int)room.rect.yMax - 1);
                top.type = Tile.Type.Floor;
                top.cost = Tile.PathCost.MaxCost;
                top.room = room;

                Tile bottom = GetTile(x, (int)room.rect.yMin);
                bottom.type = Tile.Type.Floor;
                bottom.cost = Tile.PathCost.MaxCost;
                bottom.room = room;
            }

            for (int y = (int)room.rect.yMin; y < (int)room.rect.yMax; y++)
            {
                Tile left = GetTile((int)room.rect.xMin, y);
                left.type = Tile.Type.Floor;
                left.cost = Tile.PathCost.MaxCost;
                left.room = room;

                Tile right = GetTile((int)room.rect.xMax - 1, y);
                right.type = Tile.Type.Floor;
                right.cost = Tile.PathCost.MaxCost;
                right.room = room;
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

			// 방 내부 바닥 부분을 floor 타입으로 변경
			Rect floorRect = room.GetFloorRect();
            for (int y = (int)floorRect.yMin; y < (int)floorRect.yMax -1; y++)
            {
                for (int x = (int)floorRect.xMin; x < (int)floorRect.xMax - 1; x++)
                {
                    Tile floor = GetTile(x, y);
                    floor.type = Tile.Type.Floor;
                    floor.cost = Tile.PathCost.Floor;
                    floor.room = room;
                }
            }
        }
        
        rect.x = 0;
        rect.y = 0;
    }

    public void Clear()
    {
        if (null != gameObject)
        {
            gameObject.transform.parent = null;
            GameObject.DestroyImmediate(gameObject);
            gameObject = null;
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

    public Room GetRoom(int index)
    {
        Room room = null;
        if (false == rooms.TryGetValue(index, out room))
        {
            return null;
        }
        return room;
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

    public List<Room> FindPath(Room from, Room to)
    {
        if (null == to)
        {
            return null;
        }

        Dictionary<Room, Room> parents = new Dictionary<Room, Room>(); // 부모 노드 저장
        Queue<Room> queue = new Queue<Room>();
        queue.Enqueue(from);
        parents[from] = null;  // 시작점의 부모는 없음

        while (queue.Count > 0)
        {
            Room room = queue.Dequeue();
            if (room == to) // 목표 노드 도착
            {
                break;
            }

            foreach (Room neighbor in room.neighbors)
            {
                if (false == parents.ContainsKey(neighbor)) // 방문하지 않은 노드
                {
                    parents[neighbor] = room; // 부모 노드 기록
                    queue.Enqueue(neighbor);
                }
            }
        }

        // 목표 노드까지 경로 추적
        if (false == parents.ContainsKey(to))
        {
            return null; // 도달 불가
        }

        List<Room> path = new List<Room>();

        Room parent = parents[to];
        while (null != parent)
        {
            path.Add(parent);
            parent = parents[parent];
        }

        path.Reverse(); // 시작점부터 출력하도록 뒤집기

        return path;
    }
}
