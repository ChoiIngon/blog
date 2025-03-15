using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class ResourceManager
{
    private readonly List<string> spriteAtlasAssets = new List<string>()
    {
        "TileSet"
    };

    public void Load()
    {
        LoadSprite();
        BuildActorAnimationSkin();
    }

    public Sprite GetSprite(string name)
    {
        return sprites[name];
    }

    public Skin GetSkin(string name)
    {
        return skins[name];
    }

    private Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
    private Dictionary<string, Skin> skins = new Dictionary<string, Skin>();

    private void LoadSprite()
    {
        foreach (var spriteAtlasAsset in spriteAtlasAssets)
        {
            SpriteAtlas spriteAtlas = UnityEngine.Resources.Load<SpriteAtlas>("SpriteAtlas/" + spriteAtlasAsset);
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
            Debug.Log($"Load Resources/SpriteAtlas/{spriteAtlasAsset} complete");
        }
    }

    private void BuildActorAnimationSkin()
    {
        const float TurnPassSpeed = 0.1f;
        {
            var skin = ScriptableObject.CreateInstance<Skin>();

            {
                var idle_U = new Skin.SpriteSheet(new List<Sprite>() { GetSprite("player-idle_up") }, TurnPassSpeed * 4, true);
                var idle_D = new Skin.SpriteSheet(new List<Sprite>() { GetSprite("player-idle_down") }, TurnPassSpeed * 4, true);
                var idle_L = new Skin.SpriteSheet(new List<Sprite>() { GetSprite("player-idle_left") }, TurnPassSpeed * 4, true);
                var idle_R = new Skin.SpriteSheet(new List<Sprite>() { GetSprite("player-idle_right") }, TurnPassSpeed * 4, true);

                skin.AddSpriteSheet(Actor.Action.Idle, Actor.Direction.Up, idle_U);
                skin.AddSpriteSheet(Actor.Action.Idle, Actor.Direction.Down, idle_D);
                skin.AddSpriteSheet(Actor.Action.Idle, Actor.Direction.Left, idle_L);
                skin.AddSpriteSheet(Actor.Action.Idle, Actor.Direction.Right, idle_R);
            }
            {
                var walk_D = new Skin.SpriteSheet(new List<Sprite>() { GetSprite("player-walk_left_1"), GetSprite("player-idle_left"), GetSprite("player-walk_left_2") }, TurnPassSpeed, false);
                var walk_U = new Skin.SpriteSheet(new List<Sprite>() { GetSprite("player-walk_left_1"), GetSprite("player-idle_left"), GetSprite("player-walk_left_2") }, TurnPassSpeed, false);
                var walk_L = new Skin.SpriteSheet(new List<Sprite>() { GetSprite("player-walk_right_1"), GetSprite("player-idle_right"), GetSprite("player-walk_right_2") }, TurnPassSpeed, false);
                var walk_R = new Skin.SpriteSheet(new List<Sprite>() { GetSprite("player-walk_right_1"), GetSprite("player-idle_right"), GetSprite("player-walk_right_2") }, TurnPassSpeed, false);

                skin.AddSpriteSheet(Actor.Action.Walk, Actor.Direction.Down, walk_D);
                skin.AddSpriteSheet(Actor.Action.Walk, Actor.Direction.Up, walk_U);
                skin.AddSpriteSheet(Actor.Action.Walk, Actor.Direction.Left, walk_L);
                skin.AddSpriteSheet(Actor.Action.Walk, Actor.Direction.Right, walk_R);
            }
            {
                var attack_D = new Skin.SpriteSheet(new List<Sprite>() { GetSprite("player-attack_left_1"), GetSprite("player-attack_left_2") }, TurnPassSpeed * 2, false);
                var attack_U = new Skin.SpriteSheet(new List<Sprite>() { GetSprite("player-attack_left_1"), GetSprite("player-attack_left_2") }, TurnPassSpeed * 2, false);
                var attack_L = new Skin.SpriteSheet(new List<Sprite>() { GetSprite("player-attack_right_1"), GetSprite("player-attack_right_2") }, TurnPassSpeed * 2, false);
                var attack_R = new Skin.SpriteSheet(new List<Sprite>() { GetSprite("player-attack_right_1"), GetSprite("player-attack_right_2") }, TurnPassSpeed * 2, false);

                skin.AddSpriteSheet(Actor.Action.Attack, Actor.Direction.Down, attack_D);
                skin.AddSpriteSheet(Actor.Action.Attack, Actor.Direction.Up, attack_U);
                skin.AddSpriteSheet(Actor.Action.Attack, Actor.Direction.Left, attack_L);
                skin.AddSpriteSheet(Actor.Action.Attack, Actor.Direction.Right, attack_R);
            }

            skins.Add("Player", skin);
        }
    }

}