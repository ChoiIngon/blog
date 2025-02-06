using System.Collections.Generic;
using UnityEngine;

public static class DungeonGizmo
{
    private static Shader shader = Shader.Find("Sprites/Default");
    public static GameObject root = new GameObject("GizmoRoot");

    public static void ClearAll()
    {
        while(0 < root.transform.childCount) 
        {
            Transform transform = root.transform.GetChild(0);
            transform.parent = null;
            GameObject gameObject = transform.gameObject;
            GameObject.DestroyImmediate(gameObject);
        }
    }

    public static void Destroy(Gizmo shape)
    {
        shape.gameObject.transform.parent = null;
        GameObject.DestroyImmediate(shape.gameObject);
    }

    public class Gizmo
    {
        public readonly GameObject gameObject;

        protected Gizmo(string name)
        {
            this.gameObject = new GameObject(name);
            this.gameObject.transform.parent = root.transform;
        }

        public void SetParent(Transform transform)
        {
            gameObject.transform.parent = transform;
        }

        public virtual void SetPosition(Vector3 position)
        {
            gameObject.transform.position = position;
        }
    }

    public class Point : Gizmo
    {
        private float size = 0.5f;
        private SpriteRenderer spriteRenderer;

        public Point(string name, Color color, float size = 0.5f) : base(name)
        {
            this.size = size;

            float imageSize = 100.0f * size;
            var texture = new Texture2D((int)imageSize, (int)imageSize);
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 100, 0, SpriteMeshType.FullRect, Vector4.zero, false);
            
            spriteRenderer = this.gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = color;

            SetPosition(Vector3.zero);
        }

        public Color color
        {
            get
            {
                return this.spriteRenderer.color;
            }
            set
            {
                this.spriteRenderer.color = value;
            }
        }

        public int sortingOrder
        {
            set { spriteRenderer.sortingOrder = value; }
        }

        public override void SetPosition(Vector3 position)
        {
            gameObject.transform.position = new Vector3(position.x - size / 2, position.y - size / 2);
        }
    }

    public class Line : Gizmo
    {
		private LineRenderer lineRenderer;
		public Line(string name, Color color, Vector3 start, Vector3 end, float width = 0.08f) : base(name)
        {
			this.lineRenderer = gameObject.AddComponent<LineRenderer>();
            this.lineRenderer.material = new Material(shader); ;
            this.lineRenderer.startWidth = width;
            this.lineRenderer.endWidth = width;
            this.lineRenderer.startColor = color;
			this.lineRenderer.endColor = new Color(color.r, color.g, color.b, color.a / 2);
            this.lineRenderer.useWorldSpace = false;

            this.lineRenderer.positionCount = 2;
			this.lineRenderer.SetPosition(0, start);
            this.lineRenderer.SetPosition(1, end);
		}

		public int sortingOrder
		{
			set { lineRenderer.sortingOrder = value; }
		}
	}

	public class Block : Gizmo
	{
        private SpriteRenderer backgroundRenderer;
        private MeshRenderer gridRenderer;
        private LineRenderer outlineRenderer;
        private const float OutlineWidth = 0.2f;

        public Block(string name, Color color, float width, float height) : base(name)
        {
            #region background
            {
                GameObject backgroundObj = new GameObject("background");
                backgroundObj.transform.parent = gameObject.transform;
                backgroundObj.transform.localScale = new Vector3(width, height, 1.0f);

                var texture = new Texture2D(1, 1);
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 1, 0, SpriteMeshType.FullRect, Vector4.zero, false);

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

        public Color color
        {
            set
            {
                this.backgroundRenderer.color = new Color(value.r, value.g, value.b, value.a);
                this.gridRenderer.material.color = new Color(value.r, value.g, value.b, (value.a + 1.0f) / 2.0f);
                this.outlineRenderer.startColor = Color.white;
                this.outlineRenderer.endColor = Color.white;
            }
        }

        public int sortingOrder
        {
            set {
                this.backgroundRenderer.sortingOrder = value;
                this.gridRenderer.sortingOrder = value + 1;
                this.outlineRenderer.sortingOrder = value + 1;
            }
        }
    }

    public class Circle : Gizmo
    {
        private LineRenderer lineRenderer;

        public Circle(string name, Color color, float radius, float lineWidth = 0.05f) : base(name)
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

        public int sortingOrder
        {
            set
            {
                this.lineRenderer.sortingOrder = value;
            }
        }
    }

    public class Triangle : Gizmo
    {
        private LineRenderer lineRenderer;

        public Triangle(string name, Color color, Vector3 p1, Vector3 p2, Vector3 p3, float lineWidth = 0.05f) : base(name)
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

		public int sortingOrder
		{
			set
			{
				this.lineRenderer.sortingOrder = value;
			}
		}
	}

    public class Grid : Gizmo 
    {
        public Grid(string name, int width, int height) : base(name)
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

            MeshRenderer meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(shader);
            meshRenderer.material.color = Color.gray;

            var boxCollider = this.gameObject.AddComponent<BoxCollider>();
            //go.transform.position = new Vector3(-width / 2, -height / 2);
        }
    }
}