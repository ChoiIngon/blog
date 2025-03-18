using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    
    public int roomCount;
    public int minRoomSize;
    public int maxRoomSize;

    public int randomSeed = 0;
    public float tickTime = 0.05f;

    private DungeonTileMapGenerator tileMapGenerator    = new DungeonTileMapGenerator();
    private DungeonLevelGenerator levelGenerator        = new DungeonLevelGenerator();
    public TileMap tileMap;

    public ResourceManager Resources;
    public Gizmo Gizmos;

    private void Start()
    {
        gameObject.AddComponent<CameraDrag>();
        gameObject.AddComponent<CameraScale>();

        Resources = new ResourceManager();
        Gizmos = new Gizmo();
        Gizmos.gameObject.transform.parent = transform;

        Resources.Load();
        DungeonTileMapGenerator.Init();
        DungeonObject.Init();
    }

    public void CreateDungeon()
    {
        Gizmos.Clear();

        Stopwatch stopWatch = new Stopwatch();
        if (0 == randomSeed)
        {
            randomSeed = (int)System.DateTime.Now.Ticks;
        }

        DungeonLog.Write($"Dungeon data generation process starts(random_seed:{randomSeed})");
        stopWatch.Start();

        tileMap = tileMapGenerator.Generate(roomCount, minRoomSize, maxRoomSize, randomSeed);
        tileMap = levelGenerator.Generate(tileMap);
        tileMap.gameObject.transform.parent = transform;

        stopWatch.Stop();
        DungeonLog.Write($"Dungeon data generation is complete(elapsed_time:{stopWatch.Elapsed})");

        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.Enable(GameManager.Gizmo.GroupName.Room, false));
        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.Enable(GameManager.Gizmo.GroupName.Tile, false));
    }

    #region hide
    private static GameManager _instance = null;
    public static GameManager Instance
    {
        get
        {
            if (null == _instance)
            {
                _instance = (GameManager)GameObject.FindObjectOfType(typeof(GameManager));
                if (null == _instance)
                {
                    GameObject container = new GameObject();
                    container.name = typeof(GameManager).Name;
                    _instance = container.AddComponent<GameManager>();
                }
            }

            return _instance;
        }
    }

    public static void AdjustOrthographicCamera(Rect boundary)
    {
        float halfHeight = boundary.height / 2;
        float halfWidth = boundary.width / 2;
        float aspect = Camera.main.aspect;

        Camera.main.orthographicSize = halfHeight + 10.0f;
        if (halfWidth / aspect > halfHeight)
        {
            Camera.main.orthographicSize = halfWidth / aspect + 10.0f;
        }

        Camera.main.transform.position = new Vector3(boundary.center.x, boundary.center.y, Camera.main.transform.position.z);
    }

    public class Gizmo
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

        public GameObject gameObject;
        private Dictionary<string, Group> gropus = new Dictionary<string, Group>();

        public Gizmo()
        {
            gameObject = new GameObject("DungeonGizmo");
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
    }
    #endregion
}