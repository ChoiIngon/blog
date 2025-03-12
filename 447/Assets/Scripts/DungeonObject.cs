using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

public class DungeonObject
{
    public enum Interaction
    {
        Open,   // 열기 : 잠겨있지 않은 문을 연다 or 상자를 열어 아이템을 획득한다.
        Unlock, // 잠금 해제 : 열쇠 또는 도적의 잠금 해제 스킬을 사용하여 잠긴 문을 연다 or 자물쇠가 걸린 상자를 열쇠 또는 도적의 스킬로 연다.
        Break,  // 부수기 : 무기 또는 특정 기술을 사용해 문을 강제로 연다 or 힘을 사용해 부수거나 폭탄을 사용한다.
        Inspect, // 조사하기 : 잠겨있는지, 함정이 있는지 확인한다
        Close, // 열린 문을 다시 닫는다.
        Disarm, // 함정이 있는 경우 이를 해제한다.
        Loot, // 줍다
        Drag,
        TurnOn,
        TurnOff,
        Max,
    }

    public const int SortingOrder = Tile.SortingOrder + 1;

    public static void Init()
    {
        System.AppDomain appDomain = System.AppDomain.CurrentDomain;
        System.Reflection.Assembly[] assemblies = appDomain.GetAssemblies();

        Debug.Log(System.AppDomain.CurrentDomain.FriendlyName);
        string executingAssemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        foreach (var assembly in assemblies)
        {
            if (true == assembly.GetName().Name.Equals(executingAssemblyName))
            {
                List<System.Type> dungeonObjectTypes = new List<System.Type>();
                foreach (var type in assembly.GetTypes())
                {
                    if (false == typeof(DungeonObject).IsAssignableFrom(type))
                    {
                        continue;
                    }

                    if (type == typeof(DungeonObject))
                    {
                        continue;
                    }

                    if (false == type.IsClass)
                    {
                        continue;
                    }
                    
                    MethodInfo method = type.GetMethod("Init", BindingFlags.Static | BindingFlags.Public);
                    if (null != method)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }
    }
    protected static Sprite GetRandomSprite(List<Sprite> sprites)
    {
        if (0 == sprites.Count)
        {
            return null;
        }

        return sprites[Random.Range(0, sprites.Count)];
    }

    public GameObject gameObject;
    public SpriteRenderer spriteRenderer;
    public System.Action[] interactions = new System.Action[(int)Interaction.Max];

    public DungeonObject(Tile tile)
    {
        gameObject = new GameObject();
        if (null == tile)
        {
            Debug.Log($"DungeonObject on null tile(x:{tile.rect.x}, y:{tile.rect.y}");
            return;
        }
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
    }
}

public class Door : DungeonObject
{
    public static new void Init()
    {
        Horizontal.Add(GameManager.Instance.Resources.GetSprite("Door.Horizontal"));
        Vertical.Add(GameManager.Instance.Resources.GetSprite("Door.Vertical"));
    }

    private static List<Sprite> Horizontal = new List<Sprite>();
    private static List<Sprite> Vertical = new List<Sprite>();

    public Door(Tile tile) : base(tile)
    {
        Tile top = tile.neighbors[(int)Tile.Direction.Top];
        Tile bottom = tile.neighbors[(int)Tile.Direction.Bottom];

        if (null != top && Tile.Type.Floor == top.type && null != bottom && Tile.Type.Floor == bottom.type)
        {
            // 위 아래로로 난 문
            spriteRenderer.sprite = GetRandomSprite(Horizontal);
            gameObject.name = "Door.Horizontal";
        }

        Tile left = tile.neighbors[(int)Tile.Direction.Left];
        Tile right = tile.neighbors[(int)Tile.Direction.Right];

        if (null != left && Tile.Type.Floor == left.type && null != right && Tile.Type.Floor == right.type)
        {
            spriteRenderer.sprite = GetRandomSprite(Vertical);
            gameObject.name = "Door.Vertical";
        }

        spriteRenderer.sortingOrder = SortingOrder;
        Color color = spriteRenderer.color;
        color.a = 0.0f;
        spriteRenderer.color = color;
    }
}

public class UpStair : DungeonObject
{
    public UpStair(Tile tile) : base(tile)
    {
        gameObject.name = "Stair.Up";
        spriteRenderer.sprite = GameManager.Instance.Resources.GetSprite("Stair.Up");
        spriteRenderer.sortingOrder = SortingOrder;
        Color color = spriteRenderer.color;
        color.a = 0.0f;
        spriteRenderer.color = color;
    }
}

public class DownStair : DungeonObject
{
    public DownStair(Tile tile) : base(tile)
    {
        gameObject.name = "Stair.Down";
        spriteRenderer.sprite = GameManager.Instance.Resources.GetSprite("Stair.Down");
        spriteRenderer.sortingOrder = SortingOrder;
        Color color = spriteRenderer.color;
        color.a = 0.0f;
        spriteRenderer.color = color;
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
