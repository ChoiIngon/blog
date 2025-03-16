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

    public Dictionary<string, GameObject>       gizmos = new Dictionary<string, GameObject>();
    public Dictionary<int, DungeonGizmo.Block>  roomGizmos = new Dictionary<int, DungeonGizmo.Block>();
    public Dictionary<int, DungeonGizmo.Rect>   tileGizmos = new Dictionary<int, DungeonGizmo.Rect>();

    private DungeonTileMapGenerator tileMapGenerator    = new DungeonTileMapGenerator();
    private DungeonLevelGenerator levelGenerator        = new DungeonLevelGenerator();
    public TileMap tileMap;

    public ResourceManager Resources = new ResourceManager();

    private void Start()
    {
        gameObject.AddComponent<CameraDrag>();
        gameObject.AddComponent<CameraScale>();

        Resources.Load();
        DungeonTileMapGenerator.Init();
        DungeonObject.Init();
    }

    public void CreateDungeon()
    {
        InitRoomGizmo();
		InitTileGizmo();

        foreach (var pair in gizmos)
        {
            GameObject gizmoRoot = pair.Value;
            while (0 < gizmoRoot.transform.childCount)
            {
                Transform gizmoTransform = gizmoRoot.transform.GetChild(0);
                gizmoTransform.parent = null;
                GameObject gizmoGameObject = gizmoTransform.gameObject;
                GameObject.DestroyImmediate(gizmoGameObject);
            }
            gizmoRoot.transform.parent = null;
            GameObject.DestroyImmediate(gizmoRoot);
        }
        gizmos.Clear();

        DungeonGizmo.ClearAll();

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

        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.Enable(GameManager.EventName.RoomGizmo, false));
        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NGizmo.Enable(GameManager.EventName.TileGizmo, false));
    }

    private void InitTileGizmo()
    {
        foreach (var pair in tileGizmos)
        {
            var gizmo = pair.Value;
            gizmo.parent = null;
            DungeonGizmo.Destroy(gizmo);
        }

        tileGizmos.Clear();
    }

    private void InitRoomGizmo()
    {
        foreach (var pair in roomGizmos)
        {
            var gizmo = pair.Value;
            gizmo.parent = null;
            DungeonGizmo.Destroy(gizmo);
        }

        roomGizmos.Clear();
    }

    public static class SortingOrder
    {
        public static int BackgroundGrid = 1;
        public static int Room = 1;
        public static int SpanningTreeEdge = 8;
        public static int Corridor = 9;
        public static int Floor = 10;
        public static int Wall = 10;
        public static int Door = 10;
        public static int TriangleLine = 30;
        public static int TriangleInnerCircle = 30;
        public static int BiggestCircle = 31;
    }

    public static class EventName
    {
        public const string RoomGizmo = "RoomGizmo";
        public const string MiniumSpanningTreeGizmo = "MiniumSpanningTreeGizmo";
        public const string TileCostGizmo = "TileCostGizmo";
        public const string BackgroundGridGizmo = "BackgroundGridGizmo";
        public const string TileGizmo = "TileGizmo";
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
    }

    #endregion
}