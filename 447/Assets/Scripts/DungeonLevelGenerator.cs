using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Recorder.OutputPath;

public class DungeonLevelGenerator
{
    public int level;
    public TileMap tileMap;
    public Room start;
    public Room end;
    public int minItemCount;
    public int maxitemCount;

    public struct Level
    {
        public Tile start;
        public Tile end;
    }

    public int endRoomLockProbabity = 100;
    public struct RoomPathKey
    {
        public RoomPathKey(Room start, Room end)
        {
            this.start = start;
            this.end = end;
        }

        public Room start;
        public Room end;
    }

    public Dictionary<RoomPathKey, List<Room>> paths = new Dictionary<RoomPathKey, List<Room>>();

    public DungeonLevelGenerator()
    {
    }

    public TileMap Generate(TileMap tileMap)
    {
        paths.Clear();
        this.tileMap = tileMap;
        List<Room> rooms = new List<Room>(tileMap.rooms.Values);
        for (int i = 0; i < rooms.Count; i++) 
        {
            for (int j = i + 1; j < rooms.Count; j++)
            {
                Room start = rooms[i];
                Room end = rooms[j];

                paths.Add(new RoomPathKey(start, end), tileMap.FindPath(start, end));
                paths.Add(new RoomPathKey(end, start), tileMap.FindPath(end, start));
            }
        }

        List<Room> furthestPath = new List<Room>();
        foreach (var pair in paths)
        {
            if (furthestPath.Count < pair.Value.Count)
            {
                this.start = pair.Key.start;
                this.end = pair.Key.end;
                furthestPath = pair.Value;
            }
        }

        if(start.doors.Count < end.doors.Count)
        {
            Room tmp = start;
            this.start = end;
            this.end = tmp;
        }

        {
            Rect floorRect = this.start.GetFloorRect();
            int x = (int)Random.Range(floorRect.xMin + 1, floorRect.xMax - 2);
            int y = (int)Random.Range(floorRect.yMin + 1, floorRect.yMax - 2);

            tileMap.start = tileMap.GetTile(x, y);
            tileMap.start.dungeonObject = new DownStair(tileMap.start);
            GameManager.Instance.EnqueueEvent(new GameManager.EnableTileSpriteEvent(tileMap.start));
        }
        {
            Rect floorRect = this.end.GetFloorRect();
            int x = (int)Random.Range(floorRect.xMin + 1, floorRect.xMax - 2);
            int y = (int)Random.Range(floorRect.yMin + 1, floorRect.yMax - 2);
            
            tileMap.end = tileMap.GetTile(x, y);
            tileMap.end.dungeonObject = new UpStair(tileMap.end);
            GameManager.Instance.EnqueueEvent(new GameManager.EnableTileSpriteEvent(tileMap.end));
        }
        
        rooms.Remove(start);
        rooms.Remove(end);

        this.minItemCount = 1;
        this.maxitemCount = 2;

        if (endRoomLockProbabity > Random.Range(0, 100))
        {
            LockEndRoom();
        }

        return tileMap;
    }

    // 마지막 방을 잠그는 기능
    private void LockEndRoom()
    {
        var path = FindPath(end, start);
        if (null == path)
        {
            return;
        }

        if (3 > path.Count)
        {
            return;
        }

        foreach (Tile door in end.doors)
        {
            door.dungeonObject = new Door(door);
            GameManager.Instance.EnqueueEvent(new GameManager.EnableTileSpriteEvent(door));
        }

        path.RemoveAt(0);

        Room room = path[Random.Range(0, path.Count)];
        Rect floorRect = room.GetFloorRect();

        int x = (int)Random.Range(floorRect.xMin, floorRect.xMax - 1);
        int y = (int)Random.Range(floorRect.yMin, floorRect.yMax - 1);

        Tile tile = tileMap.GetTile(x, y);

    }

    private List<Room> FindPath(Room from, Room to)
    {
        RoomPathKey key = new RoomPathKey(from, to);
        List<Room> path = null;
        if (false == paths.TryGetValue(key, out path)) 
        {
            return null;
        }

        return path;
    }

    private void CreateItem(Tile tile, string itmeCode)
    {
    }
}