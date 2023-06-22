using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public const string lit_tile = "Tile_7";
    public const string shadow_tile = "Tile_19";
    public const string default_tile = "Tile_17";

    public int x;
    public int y;

    public GameObject block;

    private SpriteRenderer spriteRenderer;

    public void Init(Transform parent, int x, int y)
    {
        this.block = null;
        this.transform.SetParent(parent);
        this.gameObject.name = $"tile_{x}_{y}";

        this.x = x;
        this.y = y;

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = GameManager.Instance.tileSprites[default_tile];
    }

    public void CreateBlock()
    {
        GameObject obj = Instantiate(GameManager.Instance.blockPrefab, transform.position, Quaternion.identity);
        obj.GetComponent<Block>().Init(this);
    }

    public void SetVisible(bool flag)
    {
        if (false == flag)
        {
            spriteRenderer.sprite = GameManager.Instance.tileSprites[shadow_tile];
        }
        else
        {
            spriteRenderer.sprite = GameManager.Instance.tileSprites[lit_tile];
        }
    }
}
