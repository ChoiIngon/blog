using System.Collections.Generic;
using UnityEngine;

public class TileMap
{
    private Tile[] tiles;
    public Rect rect;

    public int width
    {
        get { return (int)rect.width; }
    }

    public int height
    {
        get { return (int)rect.height; }
    }

    public TileMap(List<Room> rooms)
    {
        rect = DungeonGenerator.GetBoundaryRect(rooms);
        tiles = new Tile[width * height];
        // 전체 타일 초기화
        for (int i = 0; i < width * height; i++)
        {
            Tile tile = new Tile();
            tile.index = i;
            tile.rect = new Rect(i % width, i / width, 1, 1);
            tile.type = Tile.Type.None;
            tile.cost = Tile.PathCost.MaxCost;
            tiles[i] = tile;
        }
        
        foreach (Room room in rooms)
        {
            // 블록들을 (0, 0) 기준으로 옮김
            room.rect.x -= rect.xMin;
            room.rect.y -= rect.yMin;

            // 방을 벽들로 막아 버림
            for (int x = (int)room.rect.xMin; x < (int)room.rect.xMax; x++)
            {
                Tile top = GetTile(x, (int)room.rect.yMax - 1);
                top.type = Tile.Type.Wall;
                top.cost = Tile.PathCost.Wall;
                top.room = room;

                Tile bottom = GetTile(x, (int)room.rect.yMin);
                bottom.type = Tile.Type.Wall;
                bottom.cost = Tile.PathCost.Wall;
                bottom.room = room;
            }

            for (int y = (int)room.rect.yMin; y < (int)room.rect.yMax; y++)
            {
                Tile left = GetTile((int)room.rect.xMin, y);
                left.type = Tile.Type.Wall;
                left.cost = Tile.PathCost.Wall;
                left.room = room;

                Tile right = GetTile((int)room.rect.xMax - 1, y);
                right.type = Tile.Type.Wall;
                right.cost = Tile.PathCost.Wall;
                right.room = room;
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
}
