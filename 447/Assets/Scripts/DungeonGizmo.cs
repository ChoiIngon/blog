using System.Collections.Generic;
using UnityEngine;

public class DungeonGizmo
{
	public static class GroupName
	{
		public const string BackgroundGrid = "BackgroundGrid";
		public const string Room = "Room";
		public const string Tile = "Tile";
		public const string TileCost = "TileCost";
		public const string MiniumSpanningTree = "MiniumSpanningTree";
		public const string Triangle = "Triangle";
	}

	public static class SortingOrder
	{
		public static int Room = 5;
		public static int Tile = 10;
		public static int Corridor = 11;
		public static int Wall = 15;
		public static int TileCost = 20;
		public static int SpanningTreeEdge = 25;
		public static int TriangleLine = 30;
		public static int TriangleInnerCircle = 30;
		public static int BiggestCircle = 31;
	}

	public class Group
	{
		public GameObject gameObject;
		public Dictionary<int, DungeonGizmo.Gizmo> gizmos;

		public Group(string name)
		{
			gameObject = new GameObject(name);
		}

		public void Add(int index, DungeonGizmo.Gizmo gizmo)
		{
			if (null == gizmos)
			{
				gizmos = new Dictionary<int, DungeonGizmo.Gizmo>();
			}

			gizmos[index] = gizmo;
			Add(gizmo);
		}

		public void Add(DungeonGizmo.Gizmo gizmo)
		{
			gizmo.parent = gameObject.transform;
		}

		public T Get<T>(int index) where T : DungeonGizmo.Gizmo
		{
			if (null == gizmos)
			{
				return null;
			}

			DungeonGizmo.Gizmo gizmo = null;
			if (false == gizmos.TryGetValue(index, out gizmo))
			{
				return null;
			}

			return gizmo as T;
		}

		public void Remove(int index)
		{
			DungeonGizmo.Gizmo gizmo = Get<DungeonGizmo.Gizmo>(index);
			if (null == gizmo)
			{
				return;
			}

			gizmo.gameObject.transform.parent = null;
			gizmos.Remove(index);
			DungeonGizmo.Destroy(gizmo);
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

			Enable(true);
		}

		public void Enable(bool flag)
		{
			gameObject.SetActive(flag);
		}
	}

    private static Shader shader = Shader.Find("Sprites/Default");
	private static GameObject gameObject = new GameObject("DungeonGizmo");
	private Dictionary<string, Group> gropus = new Dictionary<string, Group>();

    public static void ClearAll()
    {
        while (0 < gameObject.transform.childCount)
        {
            Transform childTranform = gameObject.transform.GetChild(0);
            childTranform.parent = null;
            GameObject childGameObject = childTranform.gameObject;
            GameObject.DestroyImmediate(childGameObject);
        }
    }

	public static void Destroy(Gizmo shape)
	{
		shape.gameObject.transform.parent = null;
		GameObject.DestroyImmediate(shape.gameObject);
	}

	public DungeonGizmo()
	{
	}

	public void Clear()
	{
		foreach (var group in gropus.Values)
		{
			group.Clear();
		}
	}

	public Group GetGroup(string name)
	{
		Group group = null;
		if (false == gropus.TryGetValue(name, out group))
		{
			group = new Group(name);
			group.gameObject.transform.parent = gameObject.transform;
			gropus.Add(name, group);
		}

		return group;
	}
	
    public class Gizmo
    {
        public readonly GameObject gameObject;

        protected Gizmo(string name)
        {
            this.gameObject = new GameObject(name);
            this.gameObject.transform.parent = gameObject.transform;
        }

        public string name
        {
            get
            {
                return gameObject.name;
            }
        }

        public virtual Vector3 position
        {
            set { gameObject.transform.position = value;  }
            get { return gameObject.transform.position; }
        }

        public virtual Color color
        {
            set { }
        }

        public Transform parent
        {
            set { gameObject.transform.parent = value; }
            get { return gameObject.transform.parent; }
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
            var sprite = Sprite.Create(texture, new UnityEngine.Rect(0, 0, texture.width, texture.height), Vector2.zero, 100, 0, SpriteMeshType.FullRect, Vector4.zero, false);

            spriteRenderer = this.gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = color;

            this.position = Vector3.zero;
        }

        public override Color color
        {
            set
            {
                this.spriteRenderer.color = value;
            }
        }

        public int sortingOrder
        {
            set { spriteRenderer.sortingOrder = value; }
        }

        public override Vector3 position 
        {
            set { gameObject.transform.position = new Vector3(value.x - size / 2, value.y - size / 2); }
            get { return gameObject.transform.position; }
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
            this.lineRenderer.endColor = color;
            this.lineRenderer.useWorldSpace = false;

            this.lineRenderer.positionCount = 2;
            this.lineRenderer.SetPosition(0, start);
            this.lineRenderer.SetPosition(1, end);
        }

        public int sortingOrder
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

    public class Rect : Gizmo
    {
        private SpriteRenderer spriteRenderer;

        public Rect(string name, Color color, float width = 1.0f, float height = 1.0f) : base(name)
        {
            float imageWidth = 100.0f * width;
            float imageHeight = 100.0f * height;
            var texture = new Texture2D((int)imageWidth, (int)imageHeight);
            var sprite = Sprite.Create(texture, new UnityEngine.Rect(0, 0, texture.width, texture.height), Vector2.zero, 100, 0, SpriteMeshType.FullRect, Vector4.zero, false);

            spriteRenderer = this.gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = color;

            this.position = Vector3.zero;
        }

        public override Color color
        {
            set
            {
                this.spriteRenderer.color = value;
            }
        }

        public int sortingOrder
        {
            set { spriteRenderer.sortingOrder = value; }
        }

        public override Vector3 position
        {
            set { gameObject.transform.position = value; }
            get { return gameObject.transform.position; }
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

        public int sortingOrder
        {
            set
            {
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

        public override Color color
        {
            set
            {
                this.lineRenderer.startColor = value;
                this.lineRenderer.endColor = value;
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

            this.gameObject.AddComponent<BoxCollider>();
        }
    }
}