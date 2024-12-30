using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance = null;
    public static GameManager Instance
    {
        get
        {
            if (null == _instance)
            {
                _instance = (GameManager)GameObject.FindObjectOfType(typeof(GameManager));
                if (!_instance)
                {
                    GameObject container = new GameObject();
                    container.name = typeof(GameManager).Name;
                    _instance = container.AddComponent<GameManager>();
                }
            }

            return _instance;
        }
    }

    public int roomCount;
    public int minRoomSize;
    public int maxRoomSize;

    public bool showBlockGizmo;
    public bool showCorridorGraph;
    public bool showAstarPath;
    public bool showAstarCost;
    public bool showTile;

    public Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

    [HideInInspector]
    public Dungeon dungeon;

    public void CreateDungeon()
    {
        this.showBlockGizmo = false;
        this.showCorridorGraph = false;
        this.showAstarPath = false;
        this.showAstarCost = false;
        this.showTile = true;

        dungeon.CreateDungeon(roomCount, minRoomSize, maxRoomSize);

        dungeon.EnableGizmo();
    }
    
    public void Clear()
    {
        dungeon.Clear();
    }

    private IEnumerator Start()
    {
        this.showBlockGizmo = true;
        this.showCorridorGraph = true;
        this.showAstarPath = true;
        this.showTile = true;

        {
            SpriteAtlas spriteAtlas = Resources.Load<SpriteAtlas>("DungeonTileset");
            Sprite[] laodedSprites = new Sprite[spriteAtlas.spriteCount];
            spriteAtlas.GetSprites(laodedSprites);
            if (0 < laodedSprites.Length)
            {
                foreach (Sprite sprite in laodedSprites)
                {
                    string name = sprite.name.Replace("(Clone)", "");
                    sprites.Add(name, sprite);
                }
            }
        }
        
        {
            GameObject go = new GameObject("Dungeon");
            go.transform.parent = transform;

            dungeon = go.AddComponent<Dungeon>();
        }
        {
            //GameObject go = new GameObject("Player");
            //go.transform.parent = transform;
            //
            //player = go.AddComponent<Player>();
        }

        yield return new WaitForEndOfFrame();

        CreateDungeon();
    }

    private void LoadResources()
    {
        Sprite[] loadedSprites = Resources.LoadAll<Sprite>("DungeonTileset");
        if (0 < loadedSprites.Length)
        {
            foreach (Sprite sprite in loadedSprites)
            {
                sprites.Add(sprite.name, sprite);
            }
        }
    }

    
}
