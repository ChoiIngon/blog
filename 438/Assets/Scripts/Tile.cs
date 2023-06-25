using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int x;
    public int y;

    public GameObject block;

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
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.5f);
        }
        else
        {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1.0f);
        }
    }
}
