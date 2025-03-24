using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonLevelGenerator
{
    const int MaxJourneyTileCount = 50;
    public int level;
    public TileMap tileMap;
    public Room startRoom;
    public Room endRoom;
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

    public TileMap Generate(TileMap tileMap)
    {
        Init(tileMap);

        List<Room> rooms = new List<Room>(tileMap.rooms.Values);

        foreach (Room room in rooms)
        {
            if (30 >= Random.Range(0, 100) + 1)
            {
                CreateBoneDecorator(room);
            }

            if (30 >= Random.Range(0, 100) + 1)
            {
                CreateShackleDecorator(room);
            }

            if (30 >= Random.Range(0, 100) + 1)
            {
                CreateTorchDecorator(room);
            }
        }

        GenerateGate();

        rooms.Remove(this.startRoom);
        rooms.Remove(this.endRoom);

        var player = Player.Create(tileMap);
                
        this.minItemCount = 1;
        this.maxitemCount = 2;

        if (endRoomLockProbabity > Random.Range(0, 100))
        {
            LockEndRoom();
        }

        foreach (Room room in rooms)
        {
            CreateMonster(room);
        }
        return tileMap;
    }

    // 마지막 방을 잠그는 기능
    private void LockEndRoom()
    {
        var path = FindPath(endRoom, startRoom);
        if (null == path)
        {
            return;
        }

        if (3 > path.Count)
        {
            return;
        }

        foreach (Tile door in endRoom.doors)
        {
            door.dungeonObject = new Door(door);
        }

        Room room = path[1];
        Rect floorRect = room.GetFloorRect();

        int x = (int)Random.Range(floorRect.xMin, floorRect.xMax);
        int y = (int)Random.Range(floorRect.yMin, floorRect.yMax);

        Tile tile = tileMap.GetTile(x, y);
        tile.dungeonObject = new Key(tile);
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

    private void CreateBoneDecorator(Room room)
    {
        int boneCount = Random.Range(0, 3);
        for (int i = 0; i < boneCount; i++)
        {
            Rect floorRect = room.GetFloorRect();
            int x = Random.Range((int)floorRect.xMin, (int)floorRect.xMax);
            int y = Random.Range((int)floorRect.yMin, (int)floorRect.yMax);

            var tile = tileMap.GetTile(x, y);
            if (null == tile)
            {
                continue;
            }

            tile.dungeonObject = new Bone(tile);
        }
    }

    private void CreateShackleDecorator(Room room)
    {
        int gimmickCount = Random.Range(0, 3);
        for (int i = 0; i < gimmickCount; i++)
        {
            int x = UnityEngine.Random.Range((int)room.rect.xMin + 1, (int)room.rect.xMax - 2);
            int y = (int)room.rect.yMax - 1;

            var tile = tileMap.GetTile(x, y);
            if (null == tile)
            {
                continue;
            }

            if (Tile.Type.Wall != tile.type)
            {
                continue;
            }

            tile.dungeonObject = new Shackle(tile);
        }
    }

    private void CreateTorchDecorator(Room room)
    {
        int gimmickCount = Random.Range(0, 3);
        for (int i = 0; i < gimmickCount; i++)
        {
            int x = UnityEngine.Random.Range((int)room.rect.xMin + 1, (int)room.rect.xMax - 2);
            int y = (int)room.rect.yMax - 1;

            var tile = tileMap.GetTile(x, y);
            if (null == tile)
            {
                continue;
            }

            if (Tile.Type.Wall != tile.type)
            {
                continue;
            }

            if (null != tile.dungeonObject)
            {
                continue;
            }

            tile.dungeonObject = new Torch(tile);
        }
    }

    private void CreateMonster(Room room)
    {
        Rect floorRect = room.GetFloorRect();
        int x = (int)Random.Range(floorRect.xMin + 1, floorRect.xMax - 1);
        int y = (int)Random.Range(floorRect.yMin + 1, floorRect.yMax - 1);
        var monster = Monster.Create(this.tileMap, new Vector3(x, y));

        this.tileMap.monsters.Add(monster);
    }

    private void Init(TileMap tileMap)
    {
        this.startRoom = null;
        this.endRoom = null;
        this.paths.Clear();
        this.tileMap = tileMap;
        // 방들간 경로 미리 구하기
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
    }

    private List<Room> GetFurthestPath()
    {
        List<Room> furthestPath = new List<Room>();
        foreach (var pair in paths)
        {
            if (furthestPath.Count < pair.Value.Count)
            {
                furthestPath = pair.Value;
            }
        }
        
        return furthestPath;
    }

    private void GenerateGate()
    {
        List<Room> furthestPath = GetFurthestPath();
        this.startRoom = furthestPath[0];
        this.endRoom = furthestPath[furthestPath.Count - 1];

        if (this.startRoom.doors.Count < this.endRoom.doors.Count)
        {
            Room tmp = startRoom;
            this.startRoom = endRoom;
            this.endRoom = tmp;
        }

        Tile startTile = tileMap.GetTile((int)this.startRoom.rect.center.x, (int)this.startRoom.rect.center.y);
        Tile endTile = tileMap.GetTile((int)this.endRoom.rect.center.x, (int)this.endRoom.rect.center.y);
        var tilePath = tileMap.FindPath(endTile, startTile);

        if (tilePath.Count > MaxJourneyTileCount)
        {
            for (int i = MaxJourneyTileCount; i < tilePath.Count; i++)
            {
                var tile = tilePath[i];
                if (null != tile.room)
                {
                    this.startRoom = tile.room;
                    break;
                }
            }
        }

        tileMap.startTile = GetRandomTileInRoom(this.startRoom, -1);
        tileMap.startTile.dungeonObject = new EnterGate(tileMap.startTile);

        tileMap.endTile = GetRandomTileInRoom(this.endRoom, -1);
        tileMap.endTile.dungeonObject = new ExitGate(tileMap.endTile);
    }

    private Tile GetRandomTileInRoom(Room room, int offset)
    {
        Rect floorRect = room.GetFloorRect();

        int x = (int)Random.Range(floorRect.xMin - offset, floorRect.xMax + offset);
        int y = (int)Random.Range(floorRect.yMin - offset, floorRect.yMax + offset);

        return tileMap.GetTile(x, y);
    }

    private void LockRoom(Room room)
    {
        var path = FindPath(room, startRoom);
        if (null == path)
        {
            return;
        }

        if (3 > path.Count)
        {
            return;
        }

        /*
        foreach (Tile door in room.doors)
        {
            door.dungeonObject = new Door(door);
        }
        

        foreach (Tile door in endRoom.doors)
        {
            door.dungeonObject = new Door(door);
        }

        Room room = path[1];
        Rect floorRect = room.GetFloorRect();

        int x = (int)Random.Range(floorRect.xMin, floorRect.xMax);
        int y = (int)Random.Range(floorRect.yMin, floorRect.yMax);

        Tile tile = tileMap.GetTile(x, y);
        tile.dungeonObject = new Key(tile);
        */
    }
}