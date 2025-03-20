using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public const int SortingOrder = 100;

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

    public enum Direction
    {
        LeftTop,     Top,    RightTop,
        Left,                Right,
        RightBottom, Bottom, LeftBottom,
        Max
    }

    public int index = 0;
    public Type type = Type.None;
    public Rect rect;
    public int cost = 1;
    public Room room;   // 타일이 속해 있는 방. 통로 타일의 경우 room이 없다.
    public Tile[] neighbors = new Tile[(int)Direction.Max];
    public bool visible = false;

    public SpriteRenderer spriteRenderer;

    public DungeonObject dungeonObject;
    public Actor actor;
    
    public Vector3 position
    {
        get { return new Vector3(rect.x, rect.y); }
    }

    public void Visible(bool flag)
    {
        if (null == spriteRenderer)
        {
            return;
        }

        float alpha = 1.0f;
        if (false == flag)
        {
            alpha = 0.5f;
        }

        Color color = this.spriteRenderer.color;
        color.a = alpha;
        this.spriteRenderer.color = color;

        if (null != dungeonObject)
        {
            dungeonObject.Visible(flag);
        }

        if(null != actor)
        {
            actor.Visible(flag);
        }

        visible = flag;
    }

    public static Sprite CreateSprite(Tile tile)
    {
        if (Tile.Type.Floor == tile.type)
        {
            return FloorSprite.GetSprite(tile);
        }

        if (Tile.Type.Wall == tile.type)
        {
            return WallSprite.GetSprite(tile);
        }

        return null;
    }

    private static Sprite GetRandomSprite(List<Sprite> sprites)
    {
        if (0 == sprites.Count)
        {
            return null;
        }

        return sprites[Random.Range(0, sprites.Count)];
    }

    public static class FloorSprite
    {
        public static List<Sprite> InnerNormal = new List<Sprite>();
        public static List<Sprite> CornerInnerLeftTop = new List<Sprite>();
        public static List<Sprite> CornerInnerRightTop = new List<Sprite>();
        public static List<Sprite> CornerInnerLeftBottom = new List<Sprite>();
        public static List<Sprite> CornerInnerRightBottom = new List<Sprite>();
        public static List<Sprite> HorizontalTop = new List<Sprite>();
        public static List<Sprite> HorizontalBottom = new List<Sprite>();
        public static List<Sprite> VerticalRight = new List<Sprite>();
        public static List<Sprite> VerticalLeft = new List<Sprite>();

        public static Sprite GetSprite(Tile tile)
        {
            var leftTop = tile.neighbors[(int)Tile.Direction.LeftTop];
            var top = tile.neighbors[(int)Tile.Direction.Top];
            var rightTop = tile.neighbors[(int)Tile.Direction.RightTop];
            var left = tile.neighbors[(int)Tile.Direction.Left];
            var right = tile.neighbors[(int)Tile.Direction.Right];
            var leftBottom = tile.neighbors[(int)Tile.Direction.LeftBottom];
            var bottom = tile.neighbors[(int)Tile.Direction.Bottom];
            var rightBottom = tile.neighbors[(int)Tile.Direction.RightBottom];

            System.Func<Tile, bool> IsWall = (Tile tile) => { return (null != tile && Tile.Type.Wall == tile.type); };

            if (true == IsWall(top) && true == IsWall(left))
            {
                return GetRandomSprite(CornerInnerLeftTop);
            }

            if (true == IsWall(top) && true == IsWall(right))
            {
                return GetRandomSprite(CornerInnerRightTop);
            }

            if (true == IsWall(bottom) && true == IsWall(left))
            {
                return GetRandomSprite(CornerInnerLeftBottom);
            }

            if (true == IsWall(bottom) && true == IsWall(right))
            {
                return GetRandomSprite(CornerInnerRightBottom);
            }

            if (true == IsWall(top))
            {
                return GetRandomSprite(HorizontalTop);
            }

            if (true == IsWall(left))
            {
                return GetRandomSprite(VerticalLeft);
            }

            if (true == IsWall(right))
            {
                return GetRandomSprite(VerticalRight);
            }

            if (true == IsWall(bottom))
            {
                return GetRandomSprite(HorizontalBottom);
            }

            return GetRandomSprite(InnerNormal);
        }
    }

    public static class WallSprite
    {
        public static List<Sprite> HorizontalTop = new List<Sprite>();
        public static List<Sprite> HorizontalBottom = new List<Sprite>();
        public static List<Sprite> VerticalTop = new List<Sprite>();
        public static List<Sprite> VerticalRight = new List<Sprite>();
        public static List<Sprite> VerticalSplit = new List<Sprite>();
        public static List<Sprite> VerticalLeft = new List<Sprite>();
        public static List<Sprite> CornerInnerLeftTop = new List<Sprite>();
        public static List<Sprite> CornerInnerRightTop = new List<Sprite>();
        public static List<Sprite> CornerInnerLeftBottom = new List<Sprite>();
        public static List<Sprite> CornerInnerRightBottom = new List<Sprite>();
        public static List<Sprite> CornerOuterLeftTop = new List<Sprite>();
        public static List<Sprite> CornerOuterRightTop = new List<Sprite>();

        public static Sprite GetSprite(Tile tile)
        {
            int x = (int)tile.rect.x;
            int y = (int)tile.rect.y;

            var leftTop = tile.neighbors[(int)Tile.Direction.LeftTop];
            var top = tile.neighbors[(int)Tile.Direction.Top];
            var rightTop = tile.neighbors[(int)Tile.Direction.RightTop];
            var left = tile.neighbors[(int)Tile.Direction.Left];
            var right = tile.neighbors[(int)Tile.Direction.Right];
            var leftBottom = tile.neighbors[(int)Tile.Direction.LeftBottom];
            var bottom = tile.neighbors[(int)Tile.Direction.Bottom];
            var rightBottom = tile.neighbors[(int)Tile.Direction.RightBottom];

            System.Func<Tile, bool> IsWall = (Tile tile) => { return (null != tile && Tile.Type.Wall == tile.type); };
            System.Func<Tile, bool> IsFloor = (Tile tile) => { return (null != tile && Tile.Type.Floor == tile.type); };

            bool[] floorsAroundWall = new bool[9] {
                IsFloor(leftTop),
                IsFloor(top),
                IsFloor(rightTop),
                IsFloor(left),
                false,
                IsFloor(right),
                IsFloor(leftBottom),
                IsFloor(bottom),
                IsFloor(rightBottom)
            };

            bool[] wallSpriteFlags = new bool[4] { false, false, false, false };

            if (true == floorsAroundWall[0]) { wallSpriteFlags[0] = true; }
            if (true == floorsAroundWall[1]) { wallSpriteFlags[0] = true; wallSpriteFlags[1] = true; }
            if (true == floorsAroundWall[2]) { wallSpriteFlags[1] = true; }
            if (true == floorsAroundWall[3]) { wallSpriteFlags[0] = true; wallSpriteFlags[2] = true; }
            if (true == floorsAroundWall[5]) { wallSpriteFlags[1] = true; wallSpriteFlags[3] = true; }
            if (true == floorsAroundWall[6]) { wallSpriteFlags[2] = true; }
            if (true == floorsAroundWall[7]) { wallSpriteFlags[2] = true; wallSpriteFlags[3] = true; }
            if (true == floorsAroundWall[8]) { wallSpriteFlags[3] = true; }

            // 0
            if (true == wallSpriteFlags[0] && false == wallSpriteFlags[1] && false == wallSpriteFlags[2] && false == wallSpriteFlags[3])
            {
                return GetRandomSprite(CornerInnerRightBottom);
            }

            // 1
            if (false == wallSpriteFlags[0] && true == wallSpriteFlags[1] && false == wallSpriteFlags[2] && false == wallSpriteFlags[3])
            {
                return GetRandomSprite(CornerInnerLeftBottom);
            }

            // 2
            if (false == wallSpriteFlags[0] && false == wallSpriteFlags[1] && true == wallSpriteFlags[2] && false == wallSpriteFlags[3])
            {
                return GetRandomSprite(CornerInnerRightTop);
            }

            // 3
            if (false == wallSpriteFlags[0] && false == wallSpriteFlags[1] && false == wallSpriteFlags[2] && true == wallSpriteFlags[3])
            {
                return GetRandomSprite(CornerInnerLeftTop);
            }

            // 01
            if (true == wallSpriteFlags[0] && true == wallSpriteFlags[1] && false == wallSpriteFlags[2] && false == wallSpriteFlags[3])
            {
                return GetRandomSprite(HorizontalBottom);
            }

            // 02
            if (true == wallSpriteFlags[0] && false == wallSpriteFlags[1] && true == wallSpriteFlags[2] && false == wallSpriteFlags[3])
            {
                return GetRandomSprite(VerticalRight);
            }

            // 03
            if (true == wallSpriteFlags[0] && false == wallSpriteFlags[1] && false == wallSpriteFlags[2] && true == wallSpriteFlags[3])
            {
                return GetRandomSprite(CornerOuterRightTop);
            }

            // 12
            if (false == wallSpriteFlags[0] && true == wallSpriteFlags[1] && true == wallSpriteFlags[2] && false == wallSpriteFlags[3])
            {
                return GetRandomSprite(CornerOuterLeftTop);
            }

            // 13
            if (false == wallSpriteFlags[0] && true == wallSpriteFlags[1] && false == wallSpriteFlags[2] && true == wallSpriteFlags[3])
            {
                return GetRandomSprite(VerticalLeft);
            }

            // 23
            if (false == wallSpriteFlags[0] && false == wallSpriteFlags[1] && true == wallSpriteFlags[2] && true == wallSpriteFlags[3])
            {
                if (true == IsWall(bottom))
                {
                    return GetRandomSprite(VerticalSplit);
                }
                return GetRandomSprite(HorizontalTop);
            }

            // 012
            if (true == wallSpriteFlags[0] && true == wallSpriteFlags[1] && true == wallSpriteFlags[2] && false == wallSpriteFlags[3])
            {
                return GetRandomSprite(CornerOuterLeftTop);
            }

            // 013
            if (true == wallSpriteFlags[0] && true == wallSpriteFlags[1] && false == wallSpriteFlags[2] && true == wallSpriteFlags[3])
            {
                return GetRandomSprite(CornerOuterRightTop);
            }

            // 023
            if (true == wallSpriteFlags[0] && false == wallSpriteFlags[1] && true == wallSpriteFlags[2] && true == wallSpriteFlags[3])
            {
                if (true == IsWall(bottom))
                {
                    return GetRandomSprite(VerticalSplit);
                }
                return GetRandomSprite(HorizontalTop);
            }

            // 123
            if (false == wallSpriteFlags[0] && true == wallSpriteFlags[1] && true == wallSpriteFlags[2] && true == wallSpriteFlags[3])
            {
                if (true == IsWall(bottom))
                {
                    return GetRandomSprite(VerticalSplit);
                }
                return GetRandomSprite(HorizontalTop);
            }

            // 0123
            if (true == wallSpriteFlags[0] && true == wallSpriteFlags[1] && true == wallSpriteFlags[2] && true == wallSpriteFlags[3])
            {
                if (true == IsWall(top) && false == IsWall(bottom))
                {
                    return GetRandomSprite(HorizontalTop);
                }

                if (true == IsWall(top) && true == IsWall(bottom))
                {
                    return GetRandomSprite(VerticalSplit);
                }

                if (true == IsWall(bottom))
                {
                    return GetRandomSprite(VerticalTop);
                }

                if (true == IsWall(right))
                {
                    return GetRandomSprite(HorizontalTop);
                }

                if (true == IsWall(left))
                {
                    return GetRandomSprite(HorizontalTop);
                }
            }

            return GetRandomSprite(HorizontalTop);
        }
    }
}