using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CircumCircle : MonoBehaviour
{
    private List<Vector3> points = new List<Vector3>();
    private LineRenderer triangle;
    private List<GameObject> childrenGameObjects = new List<GameObject>();

    private static readonly string[] PointNames = new string[]
    {
        "A",
        "B",
        "C"
    };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AddTriangePoint(new Vector3(0.0f, 3.0f, 0.0f));
        AddTriangePoint(new Vector3(1.0f, 0.0f, 0.0f));
        AddTriangePoint(new Vector3(4.0f, 2.0f, 0.0f));

		triangle.positionCount = points.Count + 1;
		for (int i = 0; i < points.Count; i++)
		{
			triangle.SetPosition(i, points[i]);
		}

		triangle.SetPosition(points.Count, points[0]);
		CreateCircumCircle(points);
	}

    void Update()
    {
        if (true == Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (false == Physics.Raycast(ray, out hit))
            {
                return;
            }

            if (null == hit.transform)
            {
                return;
            }

            AddTriangePoint(hit.point);
            
            if (2 == points.Count)
            {
                triangle.positionCount = points.Count;
                for (int i = 0; i < points.Count; i++)
                {
                    triangle.SetPosition(i, points[i]);
                }
            }

            if (3 == points.Count)
            {
                triangle.positionCount = points.Count + 1;
                for (int i = 0; i < points.Count; i++)
                {
                    triangle.SetPosition(i, points[i]);
                }
                triangle.SetPosition(points.Count, points[0]);
                CreateCircumCircle(points);
            }
        }
    }

    int AddTriangePoint(Vector3 point)
    {
        if (3 == points.Count)
        {
            Clear();
        }

        if (null == triangle)
        {
            triangle = CreateLineRenderer("Triangle", Color.blue);
        }

        point.z = 0.0f; // z 좌표는 사용하지 않으므로 0

        string pointName = PointNames[points.Count];

        points.Add(point);

        var pointObject = CreatePoint($"Point_{pointName}", Color.blue, 0.1f, point);
        pointObject.transform.parent = triangle.transform;

        var text = CreateText($"{pointName}<size=3>({point.x.ToString("0.00")},{point.y.ToString("0.00")})</size>");
        text.transform.SetParent(pointObject.transform, false);
        text.transform.localPosition = new Vector3(0.0f, -0.3f, 0.0f);

        return points.Count;
    }

    void Clear()
    {
        foreach (var go in childrenGameObjects)
        {
            GameObject.Destroy(go);
        }
        childrenGameObjects.Clear();
        triangle = null;
        points.Clear();
    }

    LineRenderer CreateCircumCircle(List<Vector3> points)
    {
        if (3 > points.Count)
        {
            return null;
        }

        Vector3 a = points[0];
        Vector3 b = points[1];
        Vector3 c = points[2];

        // 직선 ab
        float mab = (b.x - a.x) / (b.y - a.y) * -1.0f;
        float a1 = (b.x + a.x) / 2.0f;
        float b1 = (b.y + a.y) / 2.0f;

        // 직선 bc
        float mbc = (b.x - c.x) / (b.y - c.y) * -1;
        float a2 = (b.x + c.x) / 2.0f;
        float b2 = (b.y + c.y) / 2.0f;

        float x = (mab * a1 - mbc * a2 + b2 - b1) / (mab - mbc);
        float y = mab * (x - a1) + b1;

        Vector3 center = new Vector3(x, y, 0.0f);
        float radius = Vector3.Distance(center, a);

        {
            var middlePoint = CreatePoint($"MiddlePointAB", Color.red, 0.1f, new Vector3(a1, b1, 0.0f));

            var perpendicularBisector = CreateLineRenderer("PerpendicularBisectorAB", Color.red);
            perpendicularBisector.positionCount = 2;
            perpendicularBisector.SetPosition(0, new Vector3(2.0f * a1 - x, 2.0f * b1 - y, 0.0f));
            perpendicularBisector.SetPosition(1, new Vector3(2.0f * x - a1, 2.0f * y - b1, 0.0f));
        }

        {
            var middlePoint = CreatePoint($"MiddlePointBC", Color.red, 0.1f, new Vector3(a2, b2, 0.0f));

            var perpendicularBisector = CreateLineRenderer("PerpendicularBisectorBC", Color.red);
            perpendicularBisector.positionCount = 2;
            perpendicularBisector.SetPosition(0, new Vector3(2.0f * a2 - x, 2.0f * b2 - y, 0.0f));
            perpendicularBisector.SetPosition(1, new Vector3(2.0f * x - a2, 2.0f * y - b2, 0.0f));
        }

        var circumCircle = CreateLineRenderer("CircumCircle", Color.black);
            
        float theta_scale = 0.01f;  // Circle resolution
        float theta = 0.0f;

        circumCircle.positionCount = (int)(1.0f / theta_scale + 1.0f);
        for (int i = 0; i < circumCircle.positionCount; i++)
        {
            theta += (2.0f * Mathf.PI * theta_scale);
            float rx = radius * Mathf.Cos(theta) + center.x;
            float ry = radius * Mathf.Sin(theta) + center.y;

            circumCircle.SetPosition(i, new Vector3(rx, ry, 0.0f));
        }

        var circumCenter = CreatePoint($"CircumCenter", Color.red, 0.1f, center);
        circumCenter.transform.parent = circumCircle.transform;

        var circumCenterText = CreateText($"O<size=3>({center.x.ToString("0.00")},{center.y.ToString("0.00")})</size>");
        circumCenterText.transform.SetParent(circumCenter.transform, false);
        circumCenterText.transform.localPosition = new Vector3(0.0f, -0.3f, 0.0f);
        return circumCircle;
    }

    TextMeshPro CreateText(string text)
    {
        GameObject go = new GameObject();
        childrenGameObjects.Add(go);
        go.name = text;

        var textMeshPro = go.AddComponent<TextMeshPro>();
        textMeshPro.rectTransform.sizeDelta = new Vector2(2.0f, 0.5f);
        textMeshPro.fontSize = 4;
        textMeshPro.text = text;
        textMeshPro.alignment = TextAlignmentOptions.Center;
        textMeshPro.verticalAlignment = VerticalAlignmentOptions.Bottom;
        textMeshPro.color = Color.black;

        return textMeshPro;
    }
    
    LineRenderer CreateLineRenderer(string name, Color color)
    {
        const float lineWidth = 0.02f;
        var go = new GameObject();
        childrenGameObjects.Add(go);
        go.name = name;
        go.transform.parent = transform;

        var lineRenderer = go.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.sortingOrder = 1;

        return lineRenderer;
    }

    SpriteRenderer CreatePoint(string name, Color color, float size, Vector3 position)
    {
        float imageSize = 100.0f * size;

        GameObject go = new GameObject();
        childrenGameObjects.Add(go);
        go.name = name;
        go.transform.parent = transform;
        go.transform.position = new Vector3(position.x - size / 2.0f, position.y - size / 2.0f, 0.0f);

        var texture = new Texture2D((int)imageSize, (int)imageSize);
        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 100, 0, SpriteMeshType.FullRect, Vector4.zero, false);

        var spriteRenderer = go.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = color;
        spriteRenderer.sortingOrder = 2;
        
        return spriteRenderer;
    }
}
