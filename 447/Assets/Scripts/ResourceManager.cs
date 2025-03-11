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
    }

    public Sprite GetSprite(string name)
    {
        return sprites[name];
    }

    private Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

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

    
}