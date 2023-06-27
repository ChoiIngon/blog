using UnityEngine;

public class Block : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public void Init()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
    }
}
