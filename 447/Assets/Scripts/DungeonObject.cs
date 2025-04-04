using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DungeonObject
{
    public const int SortingOrder = Tile.SortingOrder + 1;
    
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

    public static void Init()
    {
        System.AppDomain appDomain = System.AppDomain.CurrentDomain;
        System.Reflection.Assembly[] assemblies = appDomain.GetAssemblies();

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

    public GameObject gameObject;
    public SpriteRenderer spriteRenderer { get; protected set; }
    public System.Action<Actor>[] interactions { get; protected set; }
    public bool blockWay { get; private set; }
    public bool blockLightCast { get; private set; }
    public Tile tile;
    public DungeonObject(Tile tile, bool blockWay, bool blockLightCast)
    {
        if (null == tile)
        {
            Debug.Log($"DungeonObject on null tile(x:{tile.rect.x}, y:{tile.rect.y}");
            return;
        }

        this.gameObject = new GameObject();
        this.gameObject.transform.SetParent(tile.transform, false);

        this.spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        this.spriteRenderer.sortingOrder = SortingOrder;

        this.interactions = new System.Action<Actor>[(int)Interaction.Max];

        this.tile = tile;
        this.blockWay = blockWay;
        this.blockLightCast = blockLightCast;
    }

    public System.Action<Actor> GetInteraction(Interaction interaction)
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

    protected static Sprite GetRandomSprite(List<Sprite> sprites)
    {
        if (0 == sprites.Count)
        {
            return null;
        }

        return sprites[Random.Range(0, sprites.Count)];
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

    public Door(Tile tile) : base(tile, true, true)
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

        Color color = spriteRenderer.color;
        color.a = 0.0f;
        spriteRenderer.color = color;

        interactions[(int)Interaction.Open] = OnOpen;
    }

    private void OnOpen(Actor actor)
    {
        var player = actor as Player;
        if (null == player)
        {
            return;
        }

        if (false == player.hasKey)
        {
            return;
        }

        tile.dungeonObject = null;
        gameObject.transform.parent = null;
        GameObject.DestroyImmediate(gameObject);
    }
}

public class ExitGate : DungeonObject
{
    public ExitGate(Tile tile) : base(tile, true, true)
    {
        gameObject.name = "Stair.Up";
        spriteRenderer.sprite = GameManager.Instance.Resources.GetSprite("Stair.Up");
        Color color = spriteRenderer.color;
        color.a = 0.0f;
        spriteRenderer.color = color;
    }
}

public class EnterGate : DungeonObject
{
    public EnterGate(Tile tile) : base(tile, true, false)
    {
        gameObject.name = "Stair.Down";
        spriteRenderer.sprite = GameManager.Instance.Resources.GetSprite("Stair.Down");
        Color color = spriteRenderer.color;
        color.a = 0.0f;
        spriteRenderer.color = color;
    }
}

public class Torch : DungeonObject
{
    public static new void Init()
    {
        Horizontal.Add(GameManager.Instance.Resources.GetSprite("Torch_1"));
        Vertical.Add(GameManager.Instance.Resources.GetSprite("Torch_2"));
    }

    private static List<Sprite> Horizontal = new List<Sprite>();
    private static List<Sprite> Vertical = new List<Sprite>();

    public Torch(Tile tile) : base(tile, false, false)
    {
        gameObject.name = "Torch";

        Tile left = tile.neighbors[(int)Tile.Direction.Left];
        Tile right = tile.neighbors[(int)Tile.Direction.Right];

        if (null != left && Tile.Type.Wall == left.type && null != right && Tile.Type.Wall == right.type)
        {
            spriteRenderer.sprite = GetRandomSprite(Horizontal);
        }

        Tile top = tile.neighbors[(int)Tile.Direction.Top];
        Tile bottom = tile.neighbors[(int)Tile.Direction.Bottom];

        if (null != top && Tile.Type.Wall == top.type && null != bottom && Tile.Type.Wall == bottom.type)
        {
            spriteRenderer.sprite = GetRandomSprite(Vertical);
        }

        Color color = spriteRenderer.color;
        color.a = 0.0f;
        spriteRenderer.color = color;
    }
}

public class Bone : DungeonObject
{
    public static new void Init()
    {
        Sprites.Add(GameManager.Instance.Resources.GetSprite("Bone_1"));
        Sprites.Add(GameManager.Instance.Resources.GetSprite("Bone_2"));
    }

    static List<Sprite> Sprites = new List<Sprite>();
    public Bone(Tile tile) : base(tile, false, false) 
    {
        gameObject.name = "Bone";
        spriteRenderer.sprite = GetRandomSprite(Sprites);
        Color color = spriteRenderer.color;
        color.a = 0.0f;
        spriteRenderer.color = color;
    }

    public void OnInspect()
    {
    }
}

public class Shackle : DungeonObject
{
    public static new void Init()
    {
        Sprites.Add(GameManager.Instance.Resources.GetSprite("Shackle_1"));
        Sprites.Add(GameManager.Instance.Resources.GetSprite("Shackle_2"));
    }

    public Shackle(Tile tile) : base(tile, false, false)
    {
        gameObject.name = "Shackle";
        spriteRenderer.sprite = GetRandomSprite(Sprites);
        Color color = spriteRenderer.color;
        color.a = 0.0f;
        spriteRenderer.color = color;
    }
    public static List<Sprite> Sprites = new List<Sprite>();
}

public class Chest : DungeonObject
{
    public Chest(Tile tile) : base(tile, true, false)
    {
    }
}

public class Key : DungeonObject
{
    public static new void Init()
    {
        Sprites.Add(GameManager.Instance.Resources.GetSprite("keys_1_1"));
    }

    private static List<Sprite> Sprites = new List<Sprite>();

    public Key(Tile tile) : base(tile, true, false)
    {
        gameObject.name = "Key";
        spriteRenderer.sprite = GetRandomSprite(Sprites);
        
        Color color = spriteRenderer.color;
        color.a = 0.0f;
        spriteRenderer.color = color;

        interactions[(int)Interaction.Loot] = OnLoot;
    }

    void OnLoot(Actor actor)
    {
        var player = actor as Player;
        if (null == player)
        {
            return;
        }

        player.hasKey = true;

        tile.dungeonObject = null;
        gameObject.transform.parent = null;
        GameObject.DestroyImmediate(gameObject);
    }
}

