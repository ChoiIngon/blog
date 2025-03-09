using System.Collections.Generic;
using UnityEngine;

public class DungeonLevelGenerator
{
    public int level;
    public TileMap tileMap;
    public Room start;
    public Room end;
    public int minItemCount;
    public int maxitemCount;

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
        }
        {
            Rect floorRect = this.end.GetFloorRect();
            int x = (int)Random.Range(floorRect.xMin + 1, floorRect.xMax - 2);
            int y = (int)Random.Range(floorRect.yMin + 1, floorRect.yMax - 2);
            tileMap.end = tileMap.GetTile(x, y);
        }
        
        rooms.Remove(start);
        rooms.Remove(end);

        this.minItemCount = 1;
        this.maxitemCount = 2;

        return tileMap;
    }
}