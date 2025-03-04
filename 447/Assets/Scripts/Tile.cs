using UnityEngine;

public class Tile
{
    public class PathCost
    {
        public const int MinCost = 1;
        public const int Default = 10;
        public const int Floor = 128;
        public const int Corridor = 192;
        public const int Wall = 255;
        public const int MaxCost = 255;
    }

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
    public Room room;
}