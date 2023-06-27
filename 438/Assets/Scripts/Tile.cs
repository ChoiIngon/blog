using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int x;
    public int y;

    public Block block;

    private SpriteRenderer spriteRenderer;

    public void Init(Transform parent, int x, int y)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        this.block = null;
        this.transform.SetParent(parent);
        this.gameObject.name = $"tile_{x}_{y}";

        this.x = x;
        this.y = y;

        SetVisible(false);
    }

    public void SetVisible(bool flag)
    {
        if (false == flag)
        {
            spriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
            if (null != block)
            {
                block.spriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
            }
        }
        else
        {
            spriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            if (null != block)
            {
                block.spriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            }
        }
    }

    public void SetBlock(Block block)
    {
        block.transform.position = transform.position;
        block.transform.SetParent(transform);
        this.block = block;
    }
}
