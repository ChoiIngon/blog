using TMPro;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum TileType
    {
        None,
        Floor,
        Wall
    }

    public enum ColorType
    {
        Floor,
        Wall,
        From,
        Path,
        To,
        Select,
        Current,
        Open,
        Close
    }

    public TileType type;
    public int index;

    private TextMeshPro indexText;
    private TextMeshPro pathCostText;
    private TextMeshPro expectCostText;
    private TextMeshPro costText;
    private TextMeshPro arrowText;
    private SpriteRenderer background;
    private BoxCollider boxCollider;

    public void SetColor(ColorType color)
    {
        switch (color)
        {
            case ColorType.Floor:
                SetTileColor(Color.white);
                SetTextColor(Color.black);
                break;
            case ColorType.Wall:
                SetTileColor(Color.black);
                SetTextColor(Color.white);
                break;
            case ColorType.From:
                SetTileColor(HexToColor(0x3366FF));
                SetTextColor(Color.white);
                break;
            case ColorType.Path:
                SetTileColor(HexToColor(0xFFFF33));
                SetTextColor(Color.black);
                break;
            case ColorType.To:
                SetTileColor(HexToColor(0x3300FF));
                SetTextColor(Color.white);
                break;
            case ColorType.Select:
                SetTileColor(HexToColor(0xFF0033));
                SetTextColor(Color.white);
                break;
            case ColorType.Current:
                SetTileColor(Color.green);
                SetTextColor(Color.black);
                break;
            case ColorType.Open:
                SetTileColor(HexToColor(0x99FFFF));
                SetTextColor(Color.black);
                break;
            case ColorType.Close:
                SetTileColor(Color.gray);
                SetTextColor(Color.white);
                break;
        }
    }

    public void SetArrow(Tile parent)
    {
        if (null == parent)
        {
            this.arrowText.gameObject.SetActive(false);
            return;
        }

        this.arrowText.gameObject.SetActive(true);

        int gap = parent.index - this.index;
        if (1 == gap)    // аб
        {
            this.arrowText.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        }

        if (TileMap.GetInstance().width == gap) // ю╖
        {
            this.arrowText.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
        }

        if (-1 == gap)
        {
            this.arrowText.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
        }

        if (-TileMap.GetInstance().width == gap)
        {
            this.arrowText.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 270.0f);
        }
    }

    public int pathCost
    {
        set
        {
            pathCostText.text = value.ToString();
        }
    }

    public int expectCost
    {
        set
        {
            expectCostText.text = value.ToString();
        }
    }

    public int cost
    {
        set
        {
            costText.text = value.ToString();
        }
    }

    public void Init(int index, Tile.TileType type)
    {
        this.index = index;
        this.type = type;

        if (null == indexText)
        {
            indexText = CreateTextMeshPro("index", TextAlignmentOptions.Center, VerticalAlignmentOptions.Middle);
            indexText.sortingOrder = 1;
        }
        indexText.text = index.ToString();

        if (null == costText)
        {
            costText = CreateTextMeshPro("cost", TextAlignmentOptions.Left, VerticalAlignmentOptions.Top);
            costText.sortingOrder = 1;
        }
        costText.text = "";

        if (null == pathCostText)
        {
            pathCostText = CreateTextMeshPro("pathCost", TextAlignmentOptions.Left, VerticalAlignmentOptions.Bottom);
            pathCostText.sortingOrder = 1;
        }
        pathCostText.text = "";

        if (null == expectCostText)
        {
            expectCostText = CreateTextMeshPro("expectCost", TextAlignmentOptions.Right, VerticalAlignmentOptions.Bottom);
            expectCostText.sortingOrder = 1;
        }
        expectCostText.text = "";

        if (null == arrowText)
        {
            arrowText = CreateTextMeshPro("arraw", TextAlignmentOptions.Right, VerticalAlignmentOptions.Middle);
            arrowText.text = "->";
            arrowText.sortingOrder = 1;
        }
        arrowText.gameObject.SetActive(false);

        if (null == background)
        {
            background = CreateSpriteRenderer();
            background.gameObject.name = "backgroud";
            background.transform.localScale = new Vector3(0.95f, 0.95f, 1.0f);
        }

        if (null == boxCollider)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.center = new Vector3(0.5f, 0.5f, 0.0f);
        }

        switch (type)
        {
            case Tile.TileType.Wall:
                SetColor(ColorType.Wall);
                break;
            case Tile.TileType.Floor:
                SetColor(ColorType.Floor);
                break;
            default:
                throw new System.Exception("Undefine tile type");
        }
    }

    private void SetTileColor(Color color)
    {
        background.color = color;
    }

    private void SetTextColor(Color color)
    {
        indexText.color = color;
        costText.color = color;
        pathCostText.color = color;
        expectCostText.color = color;
        arrowText.color = color;
    }

    private Color HexToColor(int color)
    {
        float r = (float)(color >> 16 & 0x0000FF) / 255;
        float g = (float)(color >> 8 & 0x0000FF) / 255;
        float b = (float)(color >> 0 & 0x0000FF) / 255;
        return new Color(r, g, b);
    }

    private TextMeshPro CreateTextMeshPro(string name, TextAlignmentOptions hAlignment, VerticalAlignmentOptions vAlignment)
    {
        GameObject go = new GameObject();
        go.name = name;
        go.transform.parent = transform;
        go.transform.localPosition = new Vector3(0.5f, 0.5f, 0.0f);

        var textMeshPro = go.AddComponent<TextMeshPro>();
        textMeshPro.rectTransform.sizeDelta = new Vector2(0.9f, 0.9f);
        textMeshPro.alignment = hAlignment;
        textMeshPro.verticalAlignment = vAlignment;
        textMeshPro.fontSize = 2;

        return textMeshPro;
    }

    private SpriteRenderer CreateSpriteRenderer()
    {
        GameObject go = new GameObject();
        go.transform.parent = transform;
        go.transform.localPosition = Vector3.zero;

        var texture = new Texture2D(100, 100);
        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 100, 0, SpriteMeshType.FullRect, Vector4.zero, false);

        var spriteRenderer = go.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;

        return spriteRenderer;
    }
}