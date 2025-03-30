using System.Diagnostics;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    
    public int roomCount;
    public int minRoomSize;
    public int maxRoomSize;

    public int randomSeed = 0;
    public float tickTime = 0.05f;

    //private DungeonTileMapGenerator tileMapGenerator    = new DungeonTileMapGenerator();
    private DungeonLevelGenerator levelGenerator        = new DungeonLevelGenerator();
    public TileMap tileMap;

    public ResourceManager Resources;
    public DungeonGizmo Gizmos;

    private void Start()
    {
        gameObject.AddComponent<CameraDrag>();
        gameObject.AddComponent<CameraScale>();

        Resources = new ResourceManager();
        Gizmos = new DungeonGizmo();
        //Gizmos.gameObject.transform.parent = transform;

        Resources.Load();
        TileGenerator.Init();
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

        TileMap.Meta meta = new TileMap.Meta();
        meta.level = 1;
        meta.roomCount = roomCount;
        meta.minRoomSize = minRoomSize;
        meta.maxRoomSize = maxRoomSize;
        if(null != tileMap)
        {
            tileMap.gameObject.transform.parent = null;
            GameObject.DestroyImmediate(tileMap.gameObject);
        }
        tileMap = new TileMap(meta);
        tileMap = levelGenerator.Generate(tileMap);
        tileMap.gameObject.transform.parent = transform;

        stopWatch.Stop();
        DungeonLog.Write($"Dungeon data generation is complete(elapsed_time:{stopWatch.Elapsed})");
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
    #endregion
}