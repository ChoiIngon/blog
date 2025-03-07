using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DungeonLog : MonoBehaviour
{
    public TMP_FontAsset font;
    public Sprite background;
    public Color color = Color.white;
    public int maxLineCount = 100;
    public int lineSpacing = 5;
    public float verticalScrollBarWidth = 0.0f;
    GameObject content;
    ScrollRect scrollRect;
    private void Start()
    {
        Init();
    }

    void Init()
    {
        // 1. Canvas 생성 (이미 존재하면 생략 가능)
        Canvas canvas = FindObjectOfType<Canvas>();
        if (null == canvas)
        {
            GameObject canvasObject = new GameObject("Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        transform.SetParent(canvas.transform, false);
        RectTransform parentRectTransform = gameObject.GetComponent<RectTransform>();

        GameObject scrollViewObject = UnityEngine.UI.DefaultControls.CreateScrollView(new DefaultControls.Resources() { background = this.background });
        scrollViewObject.transform.SetParent(transform, false);

        Image scrollViewImage = scrollViewObject.GetComponent<Image>();
        scrollViewImage.color = color;

        this.scrollRect = scrollViewObject.GetComponent<ScrollRect>();
        this.scrollRect.horizontal = false;

        RectTransform scrollViewRectTransform = scrollViewObject.GetComponent<RectTransform>();
        scrollViewRectTransform.anchorMin = Vector2.zero;
        scrollViewRectTransform.anchorMax = new Vector2(1.0f, 1.0f);
        scrollViewRectTransform.pivot = Vector2.zero;
        scrollViewRectTransform.sizeDelta = Vector2.zero;

        GameObject scrollBarVertical = scrollViewObject.transform.Find("Scrollbar Vertical").gameObject;
        var scrollBarVerticalRectTransform = scrollBarVertical.GetComponent<RectTransform>();
        scrollBarVerticalRectTransform.sizeDelta = new Vector2(verticalScrollBarWidth, 0);

        Transform viewPort = scrollViewObject.transform.Find("Viewport");
        this.content = viewPort.Find("Content").gameObject;

        RectTransform contentRectTransform = content.GetComponent<RectTransform>();
        contentRectTransform.anchorMin = new Vector2(0, 0);
        contentRectTransform.anchorMax = new Vector2(1, 0);
        contentRectTransform.pivot = Vector2.zero;
        contentRectTransform.sizeDelta = Vector2.zero;
        contentRectTransform.offsetMin = new Vector2(5, 0);

        VerticalLayoutGroup layoutGroup = content.AddComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = lineSpacing;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;

        ContentSizeFitter contentFitter = content.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    public static void Write(string text)
    {
        var content = DungeonLog.Instance.content;
        RectTransform contentRectTransform = content.GetComponent<RectTransform>();
        while (DungeonLog.Instance.maxLineCount <= content.transform.childCount)
        {
            Transform child = content.transform.GetChild(0);
            TextMeshProUGUI childTextMeshPro = child.gameObject.GetComponent<TextMeshProUGUI>();
            float childHeight = GetHeight(childTextMeshPro);
            contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, contentRectTransform.sizeDelta.y - childHeight);
            child.SetParent(null);
            GameObject.DestroyImmediate(child.gameObject);
        }

        GameObject go = new GameObject(text);
        go.transform.SetParent(DungeonLog.Instance.content.transform, false);

        TextMeshProUGUI textMeshPro = go.AddComponent<TextMeshProUGUI>();

        textMeshPro.text = text;
        textMeshPro.fontSize = 12;
        if (null != DungeonLog.Instance.font)
        {
            textMeshPro.font = DungeonLog.Instance.font;
        }

        textMeshPro.ForceMeshUpdate();
        float height = GetHeight(textMeshPro);
        
        contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, contentRectTransform.sizeDelta.y + height);
        DungeonLog.Instance.scrollRect.verticalNormalizedPosition = 0.0f;
    }

    private static float GetHeight(TextMeshProUGUI textMeshPro)
    {
        int lineCount = textMeshPro.textInfo.lineCount;

        float height = 0.0f;
        for (int i = 0; i < lineCount; i++)
        {
            height += textMeshPro.textInfo.lineInfo[i].lineHeight;
        }

        return height;
    }
    private static DungeonLog _instance = null;
    private static DungeonLog Instance
    {
        get
        {
            if (null == _instance)
            {
                _instance = (DungeonLog)GameObject.FindObjectOfType(typeof(DungeonLog));
                if (null == _instance)
                {
                    GameObject container = new GameObject();
                    container.name = typeof(DungeonLog).Name;
                    _instance = container.AddComponent<DungeonLog>();
                    _instance.Init();
                }
            }

            return _instance;
        }
    }
}
