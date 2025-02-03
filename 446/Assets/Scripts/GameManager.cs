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

    public const float TurnPassSpeed = 0.1f;
    public int roomCount;
    
    public int minRoomSize;
    public int maxRoomSize;

    public bool showBlockGizmo;
    public bool showCorridorGraph;
    public bool showAstarPath;
    public bool showAstarCost;
    public bool showTile;

    public class Resources
    {
        private readonly List<string> spriteAtlasAssets;
        private Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        private Dictionary<string, ActorAnimation.Skin> skins = new Dictionary<string, ActorAnimation.Skin>();

        public Resources()
        {
            spriteAtlasAssets = new List<string>() {
                "DungeonTileset",
                "Character"
            };
        }

        public void Load()
        {
            LoadSpriteAsset();
            BuildActorAnimationSkin();
        }

        private void LoadSpriteAsset()
        {
            foreach (var spriteAtlasAssetName in spriteAtlasAssets)
            {
                SpriteAtlas spriteAtlas = UnityEngine.Resources.Load<SpriteAtlas>("SpriteAtlas/" + spriteAtlasAssetName);
                if (0 == spriteAtlas.spriteCount)
                {
                    continue;
                }

                Sprite[] laodedSprites = new Sprite[spriteAtlas.spriteCount];
                if (0 == spriteAtlas.GetSprites(laodedSprites))
                {
                    continue;
                }

                foreach (Sprite sprite in laodedSprites)
                {
                    string name = sprite.name.Replace("(Clone)", "");   // GetSprites는 Clone을 리턴하기 때문에, 이름에서 (Clone) postfix를 제거해준다.
                    sprites.Add(name, sprite);
                }
            }
        }

        private void BuildActorAnimationSkin()
        {
            {
                var skin = ScriptableObject.CreateInstance<ActorAnimation.Skin>();
                
                var idle_U = new ActorAnimation.SpriteSheet(new string[] { "Player.idle_back" }, TurnPassSpeed * 4, true);
                var idle_D = new ActorAnimation.SpriteSheet(new string[] { "Player.idle_front" }, TurnPassSpeed * 4, true);
                var idle_L = new ActorAnimation.SpriteSheet(new string[] { "Player.idle_left" }, TurnPassSpeed * 4, true);
                var idle_R = new ActorAnimation.SpriteSheet(new string[] { "Player.idle_right" }, TurnPassSpeed * 4, true);

                skin.SetIdleSprites(ActorAnimation.Direction.Up,    idle_U);
                skin.SetIdleSprites(ActorAnimation.Direction.Down,  idle_D);
                skin.SetIdleSprites(ActorAnimation.Direction.Left,  idle_L);
                skin.SetIdleSprites(ActorAnimation.Direction.Right, idle_R);
                
                var walk_D = new ActorAnimation.SpriteSheet(new string[] { "Player.walk_front_1", "Player.walk_front_2" }, TurnPassSpeed * 2, true);
                var walk_U = new ActorAnimation.SpriteSheet(new string[] { "Player.walk_back_1", "Player.walk_back_2" }, TurnPassSpeed * 2, true);
                var walk_L = new ActorAnimation.SpriteSheet(new string[] { "Player.walk_left_1", "Player.walk_left_2" }, TurnPassSpeed * 2, true);
                var walk_R = new ActorAnimation.SpriteSheet(new string[] { "Player.walk_right_1", "Player.walk_right_2" }, TurnPassSpeed * 2, true);

                skin.SetWalkSprites(ActorAnimation.Direction.Down, walk_D);
                skin.SetWalkSprites(ActorAnimation.Direction.Up,   walk_U);
                skin.SetWalkSprites(ActorAnimation.Direction.Left, walk_L);
                skin.SetWalkSprites(ActorAnimation.Direction.Right,walk_R);

                skins.Add("Player", skin);
            }
        }

        public Sprite GetSprite(string name)
        {
            return sprites[name];
        }
        public ActorAnimation.Skin GetSkin(string name)
        {
            return skins[name];
        }
    }

    [HideInInspector]
    public Dungeon dungeon;
    [HideInInspector]
    public Resources resources = new Resources();

    public GameManager()
    {
    }

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

        this.resources.Load();
        
        GameObject go = new GameObject("Dungeon");
        go.transform.parent = transform;
        dungeon = go.AddComponent<Dungeon>();

        yield return new WaitForEndOfFrame();

        CreateDungeon();
    }
}
