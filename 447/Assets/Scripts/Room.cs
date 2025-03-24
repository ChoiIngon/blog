using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public readonly int index;
    public Rect rect;
    public List<Room> neighbors = new List<Room>();
    public List<Tile> doors = new List<Tile>();
    
    public Room(int index, float x, float y, float width, float height)
    {
        this.index = index;
        this.rect = new Rect(x, y, width, height);
    }

    public Rect GetFloorRect()
    {
        Rect floorRect = new Rect(rect.x, rect.y, rect.width, rect.height);
        floorRect.xMin += 1;
        floorRect.xMax -= 1;
        floorRect.yMin += 1;
        floorRect.yMax -= 1;
        return floorRect;
    }

    public Vector3 position
    {
        get
        {
            return new Vector3(this.rect.x, this.rect.y, 0.0f);
        }
    }

    public float x
    {
        set
        {
            this.rect.x = value;
        }
        get
        {
            return rect.x;
        }
    }

    public float y
    {
        set
        {
            this.rect.y = value;
        }
        get
        {
            return rect.y;
        }
    }

    public Vector2 center
    {
        get
        {
            return rect.center;
        }
    }
}