using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public readonly int index;
    public Rect rect;
    public List<Room> neighbors = new List<Room>();

    public Room(int index, float x, float y, float width, float height)
    {
        this.index = index;
        this.rect = new Rect(x, y, width, height);
    }

    public Rect GetFloorRect()
    {
        return new Rect(rect.x + 1, rect.y + 1, rect.width - 1, rect.height - 1);
    }

    public Vector3 position
    {
        set
        {
            this.rect.x = value.x; this.rect.y = value.y;
        }

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