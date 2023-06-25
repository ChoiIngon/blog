using UnityEngine;
using UnityEngine.Assertions;

public class Block : MonoBehaviour
{
    public void Init(Tile tile)
    {
        Assert.IsNotNull(tile, $"no parent tile at x:{tile.x}, y:{tile.y}");

        tile.block = this.gameObject;

        this.transform.SetParent(tile.transform);
        this.gameObject.name = $"block_{tile.x}_{tile.y}";
    }
}
