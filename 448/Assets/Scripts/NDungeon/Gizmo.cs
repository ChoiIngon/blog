using System.Collections.Generic;
using UnityEngine;

namespace NDungeon
{
    public static class Gizmo
    {
        private static Shader shader = Shader.Find("Sprites/Default");
        private static GameObject gameObject = new GameObject("DungeonGizmo");
        private static Dictionary<string, Group> groups = new Dictionary<string, Group>();

        public static Group GetGroup(string name)
        {
            if (false == groups.TryGetValue(name, out Group group))
            {
                group = new Group(name);
                group.gameObject.transform.SetParent(gameObject.transform, false);
                groups[name] = group;
            }
            return group;
        }

        public static void Clear()
        {
            foreach (var itr in groups)
            {
                var group = itr.Value;
                group.Clear();
            }

            groups.Clear();

            while (0 < gameObject.transform.childCount)
            {
                var childTransform = gameObject.transform.GetChild(0);
                childTransform.parent = null;
                GameObject.DestroyImmediate(childTransform.gameObject);
            }
        }

        public abstract class Base
        {
            public readonly GameObject gameObject;

            protected Base(GameObject gameObject)
            {
                this.gameObject = gameObject;
                this.gameObject.transform.SetParent(Gizmo.gameObject.transform, false);
            }

            ~Base()
            {
                this.gameObject.transform.parent = null;
                GameObject.DestroyImmediate(this.gameObject);
            }

            public string name
            {
                get => gameObject.name;
            }

            public Transform parent
            {
                get => gameObject.transform.parent;
                set => gameObject.transform.parent = value;
            }

            public Vector3 position
            {
                get => gameObject.transform.position;
                set => gameObject.transform.position = value;
            }

            public abstract int sortingOrder
            {
                set;
            }

            public abstract Color color
            {
                set;
            }
        }

        public class Point : Base
        {
            private float diameter = 0.5f;
            private MeshRenderer meshRenderer = null;
            public Point(string name, Color color, float diameter) : base(GameObject.CreatePrimitive(PrimitiveType.Sphere))
            {
                this.diameter = diameter;
                this.gameObject.transform.localScale = Vector3.one * diameter;
                this.gameObject.name = name;

                this.meshRenderer = gameObject.GetComponent<MeshRenderer>();
                this.meshRenderer.material = new Material(shader);
                this.meshRenderer.material.color = color;
            }

            public override int sortingOrder
            {
                set { meshRenderer.sortingOrder = value; }
            }

            public override Color color
            {
                set
                {
                    meshRenderer.material.color = value;
                }
            }
        }

        public class Line : Base
        {
            private LineRenderer lineRenderer;
            public Line(string name, Color color, Vector3 start, Vector3 end, float width = 0.08f) : base(new GameObject(name))
            {
                this.lineRenderer = gameObject.AddComponent<LineRenderer>();
                this.lineRenderer.material = new Material(shader); ;
                this.lineRenderer.startWidth = width;
                this.lineRenderer.endWidth = width;
                this.lineRenderer.startColor = color;
                this.lineRenderer.endColor = color;
                this.lineRenderer.useWorldSpace = false;

                this.lineRenderer.positionCount = 2;
                this.lineRenderer.SetPosition(0, start);
                this.lineRenderer.SetPosition(1, end);
            }

            public override int sortingOrder
            {
                set { lineRenderer.sortingOrder = value; }
            }

            public override Color color
            {
                set
                {
                    this.lineRenderer.startColor = value;
                    this.lineRenderer.endColor = value;
                }
            }
        }

        public class Rect : Base
        {
            private SpriteRenderer spriteRenderer;

            public Rect(string name, Color color, float width = 1.0f, float height = 1.0f) : base(new GameObject(name))
            {
                float imageWidth = 100.0f * width;
                float imageHeight = 100.0f * height;
                var texture = new Texture2D((int)imageWidth, (int)imageHeight);
                var sprite = Sprite.Create(texture, new UnityEngine.Rect(0, 0, texture.width, texture.height), Vector2.zero, 100, 0, SpriteMeshType.FullRect, Vector4.zero, false);

                this.spriteRenderer = this.gameObject.AddComponent<SpriteRenderer>();
                this.spriteRenderer.sprite = sprite;
                this.spriteRenderer.color = color;

                this.position = Vector3.zero;
            }

            public override int sortingOrder
            {
                set { this.spriteRenderer.sortingOrder = value; }
            }

            public override Color color
            {
                set { this.spriteRenderer.color = value; }
            }
        }

        public class Block : Base
        {
            private SpriteRenderer backgroundRenderer;
            private MeshRenderer gridRenderer;
            private LineRenderer outlineRenderer;
            private const float OutlineWidth = 0.2f;

            public Block(string name, Color color, float width, float height) : base(new GameObject(name))
            {
                #region background
                {
                    GameObject backgroundObj = new GameObject("background");
                    backgroundObj.transform.parent = gameObject.transform;
                    backgroundObj.transform.localScale = new Vector3(width, height, 1.0f);

                    var texture = new Texture2D(1, 1);
                    var sprite = Sprite.Create(texture, new UnityEngine.Rect(0, 0, texture.width, texture.height), Vector2.zero, 1, 0, SpriteMeshType.FullRect, Vector4.zero, false);

                    this.backgroundRenderer = backgroundObj.AddComponent<SpriteRenderer>();
                    this.backgroundRenderer.sprite = sprite;

                }
                #endregion

                #region grid
                {
                    GameObject gridObj = new GameObject("grid");
                    gridObj.transform.parent = gameObject.transform;

                    var vertices = new List<Vector3>();
                    var indices = new List<int>();

                    for (int x = 1; x < width; x++)
                    {
                        vertices.Add(new Vector3(x, 0, 0));
                        vertices.Add(new Vector3(x, height, 0));

                        indices.Add(2 * (x - 1) + 0);
                        indices.Add(2 * (x - 1) + 1);
                    }

                    for (int y = 1; y < height; y++)
                    {
                        vertices.Add(new Vector3(0, y, 0));
                        vertices.Add(new Vector3(width, y, 0));

                        indices.Add(2 * (y - 1) + 0 + ((int)width - 1) * 2);
                        indices.Add(2 * (y - 1) + 1 + ((int)width - 1) * 2);
                    }

                    MeshFilter filter = gridObj.AddComponent<MeshFilter>();
                    filter.mesh = new Mesh();
                    filter.mesh.vertices = vertices.ToArray();
                    filter.mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);

                    this.gridRenderer = gridObj.AddComponent<MeshRenderer>();
                    this.gridRenderer.material = new Material(shader); ;
                }
                #endregion

                #region outline
                {
                    GameObject outlineObj = new GameObject("outline");
                    outlineObj.transform.parent = gameObject.transform;

                    this.outlineRenderer = outlineObj.AddComponent<LineRenderer>();
                    this.outlineRenderer.material = new Material(shader); ;
                    this.outlineRenderer.startWidth = OutlineWidth;
                    this.outlineRenderer.endWidth = OutlineWidth;
                    this.outlineRenderer.useWorldSpace = false;
                    this.outlineRenderer.positionCount = 5;
                    this.outlineRenderer.SetPosition(0, new Vector3(0.0f, 0.0f));
                    this.outlineRenderer.SetPosition(1, new Vector3(0.0f, height));
                    this.outlineRenderer.SetPosition(2, new Vector3(width, height));
                    this.outlineRenderer.SetPosition(3, new Vector3(width, 0.0f));
                    this.outlineRenderer.SetPosition(4, new Vector3(0.0f, 0.0f));
                }
                #endregion

                this.color = color;
            }

            public override int sortingOrder
            {
                set
                {
                    this.backgroundRenderer.sortingOrder = value;
                    this.gridRenderer.sortingOrder = value + 1;
                    this.outlineRenderer.sortingOrder = value + 1;
                }
            }

            public override Color color
            {
                set
                {
                    this.backgroundRenderer.color = new Color(value.r, value.g, value.b, 128);
                    this.gridRenderer.material.color = new Color(value.r, value.g, value.b, 200);
                    this.outlineRenderer.startColor = Color.white;
                    this.outlineRenderer.endColor = Color.white;
                }
            }
        }

        public class Circle : Base
        {
            private LineRenderer lineRenderer;

            public Circle(string name, Color color, float radius, float lineWidth = 0.05f) : base(new GameObject(name))
            {
                this.lineRenderer = this.gameObject.AddComponent<LineRenderer>();
                this.lineRenderer.material = new Material(shader);
                this.lineRenderer.startWidth = lineWidth;
                this.lineRenderer.endWidth = lineWidth;
                this.lineRenderer.useWorldSpace = false;
                this.lineRenderer.startColor = color;
                this.lineRenderer.endColor = color;

                float theta_scale = 0.01f;  // Circle resolution
                float theta = 0.0f;

                lineRenderer.positionCount = (int)(1.0f / theta_scale + 1.0f);
                for (int i = 0; i < lineRenderer.positionCount; i++)
                {
                    theta += (2.0f * Mathf.PI * theta_scale);
                    float rx = radius * Mathf.Cos(theta);
                    float ry = radius * Mathf.Sin(theta);

                    lineRenderer.SetPosition(i, new Vector3(rx, ry, 0.0f));
                }

            }

            public override int sortingOrder
            {
                set
                {
                    this.lineRenderer.sortingOrder = value;
                }
            }

            public override Color color
            {
                set
                {
                    this.lineRenderer.startColor = value;
                    this.lineRenderer.endColor = value;
                }
            }
        }

        public class Triangle : Base
        {
            private LineRenderer lineRenderer;

            public Triangle(string name, Color color, Vector3 p1, Vector3 p2, Vector3 p3, float lineWidth = 0.05f) : base(new GameObject(name))
            {
                this.lineRenderer = this.gameObject.AddComponent<LineRenderer>();
                this.lineRenderer.material = new Material(shader);
                this.lineRenderer.startWidth = lineWidth;
                this.lineRenderer.endWidth = lineWidth;
                this.lineRenderer.useWorldSpace = false;
                this.lineRenderer.startColor = color;
                this.lineRenderer.endColor = color;

                lineRenderer.positionCount = 4;
                lineRenderer.SetPosition(0, p1);
                lineRenderer.SetPosition(1, p2);
                lineRenderer.SetPosition(2, p3);
                lineRenderer.SetPosition(3, p1);
            }

            public override int sortingOrder
            {
                set
                {
                    this.lineRenderer.sortingOrder = value;
                }
            }

            public override Color color
            {
                set
                {
                    this.lineRenderer.startColor = value;
                    this.lineRenderer.endColor = value;
                }
            }
        }

        public class Grid : Base
        {
            private MeshRenderer meshRenderer = null;

            public Grid(string name, int width, int height) : base(new GameObject(name))
            {
                var vertices = new List<Vector3>();
                var indices = new List<int>();

                for (int x = 0; x <= width; x++)
                {
                    vertices.Add(new Vector3(x, 0.0f));
                    vertices.Add(new Vector3(x, height));

                    indices.Add(2 * x + 0);
                    indices.Add(2 * x + 1);
                }

                for (int y = 0; y <= height; y++)
                {
                    vertices.Add(new Vector3(0.0f, y));
                    vertices.Add(new Vector3(width, y));

                    indices.Add(2 * y + 0 + (width + 1) * 2);
                    indices.Add(2 * y + 1 + (width + 1) * 2);
                }

                MeshFilter filter = this.gameObject.AddComponent<MeshFilter>();
                filter.mesh = new Mesh();
                filter.mesh.vertices = vertices.ToArray();
                filter.mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);

                this.meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
                this.meshRenderer.material = new Material(shader);
                this.meshRenderer.material.color = Color.gray;

                this.gameObject.AddComponent<BoxCollider>();
            }

            public override int sortingOrder
            {
                set
                {
                    this.meshRenderer.sortingOrder = value;
                }
            }

            public override Color color
            {
                set
                {
                    this.meshRenderer.material.color = Color.gray;
                }
            }
        }

        public class Group
        {
            public GameObject gameObject = null;
            private Dictionary<int, Base> gizmos = null;

            public Group(string name)
            {
                gameObject = new GameObject(name);
            }

            public void Add(int index, Base gizmo)
            {
                if (null == gizmos)
                {
                    gizmos = new Dictionary<int, Base>();
                }

                gizmos[index] = gizmo;
                gizmo.parent = gameObject.transform;
            }

            public T Get<T>(int index) where T : Base
            {
                if (null == gizmos)
                {
                    return null;
                }

                if (false == gizmos.TryGetValue(index, out Base gizmo))
                {
                    return null;
                }

                return gizmo as T;
            }

            public void Remove(int index)
            {
                Base gizmo = Get<Base>(index);
                if (null == gizmo)
                {
                    return;
                }

                gizmos.Remove(index);
                gizmo.gameObject.transform.parent = null;
                GameObject.DestroyImmediate(gizmo.gameObject);
            }

            public void Clear()
            {
                if (null != gizmos)
                {
                    gizmos.Clear();
                }

                while (0 < gameObject.transform.childCount)
                {
                    var childTransform = gameObject.transform.GetChild(0);
                    childTransform.parent = null;
                    GameObject.DestroyImmediate(childTransform.gameObject);
                }
            }

            public void Enable(bool flag)
            {
                gameObject.SetActive(flag);
            }
        }
    }
    

    
}