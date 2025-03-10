using System.Collections.Generic;
using UnityEngine;

public class DungeonObject
{
    public enum Interaction
    {
        Open,   // ���� : ������� ���� ���� ���� or ���ڸ� ���� �������� ȹ���Ѵ�.
        Unlock, // ��� ���� : ���� �Ǵ� ������ ��� ���� ��ų�� ����Ͽ� ��� ���� ���� or �ڹ��谡 �ɸ� ���ڸ� ���� �Ǵ� ������ ��ų�� ����.
        Break,  // �μ��� : ���� �Ǵ� Ư�� ����� ����� ���� ������ ���� or ���� ����� �μ��ų� ��ź�� ����Ѵ�.
        Inspect, // �����ϱ� : ����ִ���, ������ �ִ��� Ȯ���Ѵ�
        Close, // ���� ���� �ٽ� �ݴ´�.
        Disarm, // ������ �ִ� ��� �̸� �����Ѵ�.
        Loot, // �ݴ�
        Drag,
        TurnOn,
        TurnOff,
        Max,
    }

    public GameObject gameObject;
    public SpriteRenderer spriteRenderer;
    public System.Action[] interactions = new System.Action[(int)Interaction.Max];

    public DungeonObject(Tile tile)
    {
        gameObject = new GameObject();
        gameObject.transform.SetParent(tile.transform, false);
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
    }

    public System.Action GetInteraction(Interaction interaction)
    {
        if (0 > (int)interaction && interaction >= Interaction.Max)
        {
            return null;
        }

        return interactions[(int)interaction];
    }
}

public class Bone : DungeonObject
{
    static List<Sprite> Sprites = new List<Sprite>();
    
    public Bone(Tile tile) : base(tile) 
    {
        gameObject.name = "Bone";

        interactions[(int)Interaction.Inspect] = OnInspect;
    }

    public void OnInspect()
    {
    }
}

public class Door : DungeonObject
{
    public static List<Sprite> Horizontal = new List<Sprite>();
    public static List<Sprite> Vertical = new List<Sprite>();

    public Door(Tile tile) : base(tile)
    {
        Tile top = tile.neighbors[(int)Tile.Direction.Top];
        Tile bottom = tile.neighbors[(int)Tile.Direction.Bottom];

        if (null != top && Tile.Type.Floor == top.type && null != bottom && Tile.Type.Floor == bottom.type)
        {
            // �� �Ʒ��η� �� ��
            //spriteRenderer.sprite = GetRandomSprite(Horizontal);
        }

        Tile left = tile.neighbors[(int)Tile.Direction.Left];
        Tile right = tile.neighbors[(int)Tile.Direction.Right];

        if (null != left && Tile.Type.Floor == left.type && null != right && Tile.Type.Floor == right.type)
        {
            //spriteRenderer.sprite = GetRandomSprite(Vertical);
        }
    }
}

public class Chest : DungeonObject
{
    public Chest(Tile tile) : base(tile)
    {
    }
}

public class Key : DungeonObject
{
    public Key(Tile tile) : base(tile)
    {
    }
}
