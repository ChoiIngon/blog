using System.Collections.Generic;
using UnityEngine;

public class Skin : ScriptableObject
{
    public class SpriteSheet
    {
        public bool loop; // 반복 재생 여부
        public float playTime;  // 애니메이션 재생 시간
        public List<Sprite> sprites;

        public SpriteSheet(List<Sprite> sprites, float playTime, bool loop = true)
        {
            this.loop = loop;
            this.playTime = playTime;
            this.sprites = sprites;
        }
    }

    private List<List<SpriteSheet>> actionSpriteSheets;

    public Skin()
    {
        actionSpriteSheets = new List<List<SpriteSheet>>();
        for (int i = 0; i < (int)Actor.Action.Max; i++)
        {
            List<SpriteSheet> spriteSheets = new List<SpriteSheet>();
            for (int j = 0; j < Actor.Direction.Max; j++)
            {
                spriteSheets.Add(null);
            }
            actionSpriteSheets.Add(spriteSheets);
        }
    }

    public void AddSpriteSheet(Actor.Action action, int direction, SpriteSheet spriteSheet)
    {
        var spriteSheets = actionSpriteSheets[(int)action];
        spriteSheets[direction] = spriteSheet;
    }

    public SpriteSheet GetSpriteSheet(Actor.Action action, int direction)
    {
        var spriteSheets = actionSpriteSheets[(int)action];
        return spriteSheets[direction];
    }
}