using System.Collections.Generic;
using UnityEngine;

public class TileMap
{
    public class Meta
    {
        public int level;
        public int roomCount;
        public int minRoomSize;
        public int maxRoomSize;
    }

    public GameObject gameObject;
    public Meta meta { get; private set; } = null;

    public Tile[] tiles;
    public Dictionary<int, Room> rooms = new Dictionary<int, Room>();
    public Rect rect;

    public int width
    {
        get { return (int)rect.width; }
    }
    public int height
    {
        get { return (int)rect.height; }
    }

    public Tile startTile;
    public Tile endTile;
    public Player player;
    public Monster.MonsterManager monsters = new Monster.MonsterManager();

    public TileMap(Meta meta)
    {
        this.meta = meta;
        this.gameObject = new GameObject("TileMap");

        TileGenerator tileGenerator = new TileGenerator();
        tileGenerator.Generate(this);
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

        path.Add(to);
        Room parent = parents[to];
        while (null != parent)
        {
            path.Add(parent);
            parent = parents[parent];
        }

        path.Reverse(); // 시작점부터 출력하도록 뒤집기

        return path;
    }

    public ShadowCast CastLight(int x, int y, int sightRange)
    {
        ShadowCast sight = new ShadowCast(this);
        sight.CastLight(x, y, sightRange);
        return sight;
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
}
