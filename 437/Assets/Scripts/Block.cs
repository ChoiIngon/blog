using UnityEngine;
using UnityEngine.Assertions;

public class Block : MonoBehaviour
{
    public const string block = "Tile_14";
    public int x;
    public int y;

    private SpriteRenderer spriteRenderer;

    public void Init(Tile tile)
    {
        Assert.IsNotNull(tile, $"no parent tile at x:{tile.x}, y:{tile.y}");

        tile.block = this.gameObject;

        this.transform.SetParent(tile.transform);
        this.gameObject.name = $"block_{tile.x}_{tile.y}";

        this.x = tile.x;
        this.y = tile.y;

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = GameManager.Instance.tileSprites[block];
    }
}
