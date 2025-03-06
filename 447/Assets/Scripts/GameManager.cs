using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using static Dungeon;

public class GameManager : MonoBehaviour
{
    public int roomCount;
    public int minRoomSize;
    public int maxRoomSize;

    public int randomSeed = 0;
    public float displayStepInterval = 0.05f;

    private GameObject tileGizmoRoot;
    private Dictionary<int, DungeonGizmo.Rect> tileGizmos = new Dictionary<int, DungeonGizmo.Rect>();
    
    private GameObject roomGizmoRoot;
    private Dictionary<int, DungeonGizmo.Block> roomGizmos = new Dictionary<int, DungeonGizmo.Block>();

    private GameObject corridorGizmoRoot;
    private GameObject mstGizmoRoot;

    private DungeonGizmo.Grid backgroundGridGizmo;

    private DungeonGenerator generator = new DungeonGenerator();
    public TileMap tileMap;
    public Dungeon dungeon;
    private Coroutine coroutine;

    private void Start()
    {
        gameObject.AddComponent<CameraDrag>();
        gameObject.AddComponent<CameraScale>();

        tileGizmoRoot = new GameObject("TileGiamoRoot");
        tileGizmoRoot.transform.parent = gameObject.transform;

        roomGizmoRoot = new GameObject("RoomGizmoRoot");
        roomGizmoRoot.transform.parent = gameObject.transform;

        corridorGizmoRoot = new GameObject("CorridorGizmoRoot");
        corridorGizmoRoot.transform.parent = gameObject.transform;

        mstGizmoRoot = new GameObject("MinimumSpanningTreeRoot");
        mstGizmoRoot.transform.parent = gameObject.transform;
    }

    public void CreateDungeon()
    {
		events.Clear();
		if (null != coroutine)
		{
			StopCoroutine(coroutine);
			coroutine = null;
		}

		InitTileGizmo();
        InitRoomGizmo();
        InitMinimumSpanningTreeGizmo();
        InitCorridorGizmo();
        DungeonGizmo.ClearAll();

        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();
        UnityEngine.Debug.Log($"Create Dungeon(random_seed:{randomSeed})");
        tileMap = generator.Generate(roomCount, minRoomSize, maxRoomSize, randomSeed);
        stopWatch.Stop();
        UnityEngine.Debug.Log($"elapsed time:{stopWatch.Elapsed}");

        GameObject go = new GameObject("Dungeon");
        go.transform.parent = transform;
        this.dungeon = go.AddComponent<Dungeon>();
        dungeon.AttachTile(tileMap);

        coroutine = StartCoroutine(ExecuteEvent());
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

    private void InitMinimumSpanningTreeGizmo()
    {
        while (0 < mstGizmoRoot.transform.childCount)
        {
            Transform child = mstGizmoRoot.transform.GetChild(0);
            child.parent = null;
            GameObject gameObject = child.gameObject;
            GameObject.DestroyImmediate(gameObject);
        }
    }

    private void InitCorridorGizmo()
    {
        while (0 < corridorGizmoRoot.transform.childCount)
        {
            Transform child = corridorGizmoRoot.transform.GetChild(0);
            child.parent = null;
            GameObject gameObject = child.gameObject;
            GameObject.DestroyImmediate(gameObject);
        }
    }

    public interface Event
    {
        public IEnumerator OnEvent();
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

    public class AttachTileSprite : Event
    {
        private Tile tile;

        public AttachTileSprite(Tile tile)
        {
            this.tile = tile;
        }

        public IEnumerator OnEvent()
        {
            if (Tile.Type.Floor == tile.type)
            {
                var tileSprite = new FloorSprite(tile);
                tileSprite.SetParent(GameManager.Instance.transform);
                GameManager.Instance.dungeon.tileSprites[tile.index] = tileSprite;
            }

            if (Tile.Type.Wall == tile.type)
            {
                var tileSprite = new WallSprite(tile);
                tileSprite.SetParent(GameManager.Instance.transform);
                GameManager.Instance.dungeon.tileSprites[tile.index] = tileSprite;
            }

            yield return new WaitForSeconds(GameManager.Instance.displayStepInterval/10);
        }
    }

    public class CreateRoomEvent : Event
    {
        public Room room;
        public Vector3 position;
        public Rect cameraBoundary;
        public Color color;

        public CreateRoomEvent(Room room, Rect cameraBoundary, Color color)
        {
            this.room = room;
            this.position = room.position;
            this.cameraBoundary = cameraBoundary;
            this.color = color;
        }

        public IEnumerator OnEvent()
        {
            if (0 >= room.index)
            {
                yield break;
            }

            var roomGizmo = new DungeonGizmo.Block($"Room_{room.index}", color, room.rect.width, room.rect.height);
            roomGizmo.sortingOrder = SortingOrder.Room;
            roomGizmo.parent = GameManager.Instance.roomGizmoRoot.transform;
            roomGizmo.position = new Vector3(position.x, position.y, 0.0f);
            GameManager.Instance.roomGizmos.Add(room.index, roomGizmo);
            GameManager.AdjustOrthographicCamera(cameraBoundary);
            Camera.main.transform.position = new Vector3(cameraBoundary.center.x, cameraBoundary.center.y, Camera.main.transform.position.z);
        }
    }

    public class MoveRoomEvent : Event
    {
        public Room room;
        public Vector3 position;
        public Rect cameraBoundary;

        public MoveRoomEvent(Room room, Rect cameraBoundary)
        {
            this.room = room;
            this.position = room.position;
            this.cameraBoundary = cameraBoundary;
        }

        public IEnumerator OnEvent()
        {
            DungeonGizmo.Block gizmo;

            if (false == GameManager.Instance.roomGizmos.TryGetValue(room.index, out gizmo))
            {
                yield break;
            }

            if (gizmo.position == position)
            {
                yield break;
            }

            float interpolation = 0.0f;
            Vector3 start = gizmo.position;
            while (1.0f > interpolation)
            {
                interpolation += Time.deltaTime / GameManager.Instance.displayStepInterval;

                gizmo.position = Vector3.Lerp(start, this.position, interpolation);
                yield return null;
            }

            gizmo.position = this.position;
            GameManager.AdjustOrthographicCamera(cameraBoundary);
            Camera.main.transform.position = new Vector3(cameraBoundary.center.x, cameraBoundary.center.y, Camera.main.transform.position.z);
        }
    }

    public class ChangeRoomColorEvent : Event
    {
        public Room room;
        public Color color;

        public ChangeRoomColorEvent(Room room, Color color)
        {
            this.room = room;
            this.color = color;
        }

        public IEnumerator OnEvent()
        {
            DungeonGizmo.Block gizmo;
            if (false == GameManager.Instance.roomGizmos.TryGetValue(room.index, out gizmo))
            {
                yield break;
            }

            gizmo.color = color;
            yield return new WaitForSeconds(GameManager.Instance.displayStepInterval);
        }
    }

    public class DestroyRoomEvent : Event
    {
        public Room room;

        public DestroyRoomEvent(Room room)
        {
            this.room = room;
        }

        public IEnumerator OnEvent()
        {
            DungeonGizmo.Block gizmo;
            if (false == GameManager.Instance.roomGizmos.TryGetValue(room.index, out gizmo))
            {
                yield break;
            }

            gizmo.parent = null;
            DungeonGizmo.Destroy(gizmo);
            GameManager.Instance.roomGizmos.Remove(room.index);
            yield return new WaitForSeconds(GameManager.Instance.displayStepInterval);
        }
    }

    public class FindRoomPositionEvent : Event
    {
        public DelaunayTriangulation triangulation;
        public DelaunayTriangulation.Circle biggestCircle;
        public IEnumerator OnEvent()
        {
            int index = 1;
            float interval = GameManager.Instance.displayStepInterval / triangulation.triangles.Count;
            foreach (var triangle in triangulation.triangles)
            {
                DungeonGizmo.Line line_ab = new DungeonGizmo.Line($"Triangle_{index}_ab", Color.green, triangle.a, triangle.b, 0.1f);
                line_ab.sortingOrder = SortingOrder.TriangleLine;

                DungeonGizmo.Line line_ac = new DungeonGizmo.Line($"Triangle_{index}_ac", Color.green, triangle.a, triangle.c, 0.1f);
                line_ac.sortingOrder = SortingOrder.TriangleLine;

                DungeonGizmo.Line line_bc = new DungeonGizmo.Line($"Triangle_{index}_bc", Color.green, triangle.b, triangle.c, 0.1f);
                line_bc.sortingOrder = SortingOrder.TriangleLine;

                DungeonGizmo.Circle innerCircle = new DungeonGizmo.Circle($"Triangle_{index}_InnerCircle", Color.green, triangle.innerCircle.radius, 0.1f);
                innerCircle.position = triangle.innerCircle.center;
                innerCircle.sortingOrder = SortingOrder.TriangleInnerCircle;
                yield return new WaitForSeconds(interval);
            }

            DungeonGizmo.Circle circle = new DungeonGizmo.Circle($"Biggest_{index}_InnerCircle", Color.red, biggestCircle.radius, 0.5f);
            circle.position = biggestCircle.center;
            circle.sortingOrder = SortingOrder.BiggestCircle;
            yield return new WaitForSeconds(GameManager.Instance.displayStepInterval);

            DungeonGizmo.ClearAll();
        }
    }

    public class BuildRoomWallEvent : Event
    {
        private Room room;
        public BuildRoomWallEvent(Room room)
        {
            this.room = room;
        }

        public IEnumerator OnEvent()
        {
            int tileCount = (int)room.rect.width * 2 + (int)room.rect.height * 2;
            float interval = GameManager.Instance.displayStepInterval / tileCount;
            // 방을 벽들로 막아 버림
            for (int x = (int)room.rect.xMin; x < (int)room.rect.xMax; x++)
            {
                BuildWallOnTile(x, (int)room.rect.yMax - 1);
                yield return new WaitForSeconds(interval);
            }

            for (int y = (int)room.rect.yMax - 2;  y >= (int)room.rect.yMin + 1; y--)
            {
                BuildWallOnTile((int)room.rect.xMax - 1, y);
                yield return new WaitForSeconds(interval);
            }

            for (int x = (int)room.rect.xMax - 1; x >= (int)room.rect.xMin; x--)
            {
                BuildWallOnTile(x, (int)room.rect.yMin);
                yield return new WaitForSeconds(interval);
            }

            for (int y = (int)room.rect.yMin + 1; y < (int)room.rect.yMax - 1; y++)
            {
                BuildWallOnTile((int)room.rect.xMin, y);
                yield return new WaitForSeconds(interval);
            }
        }

        private void BuildWallOnTile(int x, int y)
        {
            var tile = GameManager.Instance.tileMap.GetTile(x, y);
            if (Tile.Type.Wall == tile.type)
            {
                DungeonGizmo.Rect rect = new DungeonGizmo.Rect($"Tile_{tile.index}", Color.white, 1.0f, 1.0f);
                rect.sortingOrder = GameManager.SortingOrder.Floor;
                rect.position = new Vector3(x, y);
                rect.parent = GameManager.Instance.tileGizmoRoot.transform;
                GameManager.Instance.tileGizmos[tile.index] = rect;
            }
        }
    }

    public class BuildCorridorWallEvent : Event
    {
        private List<DungeonGenerator.Corridor> corridors;

        public BuildCorridorWallEvent(List<DungeonGenerator.Corridor> corridors)
        {
            this.corridors = corridors;
        }

        public IEnumerator OnEvent()
        {
            System.Action<int, int> IfWallChangeColor = (int x, int y) =>
            {
                Tile tile = GameManager.Instance.tileMap.GetTile(x, y);
                if (null == tile)
                {
                    return;
                }
                
                if (Tile.Type.Wall != tile.type)
                {
                    return;
                }

                DungeonGizmo.Rect rect = null;
                if (false == GameManager.Instance.tileGizmos.TryGetValue(tile.index, out rect))
                {
                    rect = new DungeonGizmo.Rect($"Tile_{tile.index}", Color.white, 1.0f, 1.0f);
                    rect.position = new Vector3(tile.rect.x, tile.rect.y);
                    rect.parent = GameManager.Instance.tileGizmoRoot.transform;
                    GameManager.Instance.tileGizmos.Add(tile.index, rect);
                    return;
                }

                rect.color = Color.white;
            };
            
            foreach (var corridor in corridors)
            {
                float interval = GameManager.Instance.displayStepInterval / corridor.path.Count;
                foreach (Tile tile in corridor.path)
                {
                    DungeonGizmo.Rect rect = null;
                    if (false == GameManager.Instance.tileGizmos.TryGetValue(tile.index, out rect))
                    {
                        yield break;
                    }

                    rect.color = Color.red;

                    int x = (int)tile.rect.x;
                    int y = (int)tile.rect.y;

                    IfWallChangeColor(x - 1, y - 1);
                    IfWallChangeColor(x - 1, y);
                    IfWallChangeColor(x - 1, y + 1);
                    IfWallChangeColor(x, y - 1);
                    IfWallChangeColor(x, y + 1);
                    IfWallChangeColor(x + 1, y - 1);
                    IfWallChangeColor(x + 1, y);
                    IfWallChangeColor(x + 1, y + 1);

                    yield return new WaitForSeconds(interval);
                }
            }

            GameManager.Instance.backgroundGridGizmo.parent = null;
            DungeonGizmo.Destroy(GameManager.Instance.backgroundGridGizmo);
        }
    }

    public class CreateMinimumSpanningTreeEvent : Event
    {
        public MinimumSpanningTree mst;
        public Color color;

        public CreateMinimumSpanningTreeEvent(MinimumSpanningTree mst, Color color)
        {
            this.mst = mst;
            this.color = color;
        }

        public IEnumerator OnEvent()
        {
            while (0 < GameManager.Instance.mstGizmoRoot.transform.childCount)
            {
                Transform child = GameManager.Instance.mstGizmoRoot.transform.GetChild(0);
                child.parent = null;
                GameObject gameObject = child.gameObject;
                GameObject.DestroyImmediate(gameObject);
            }

            if (null == mst)
            {
                yield break;
            }

            float interval = GameManager.Instance.displayStepInterval / mst.connections.Count;
            foreach (var connection in mst.connections)
            {
                DungeonGizmo.Line line = new DungeonGizmo.Line($"Edge_Mst_{connection.p1.index}_{connection.p2.index}", color, connection.p1.rect.center, connection.p2.rect.center, 0.5f);
                line.parent = GameManager.Instance.mstGizmoRoot.transform;
                line.sortingOrder = SortingOrder.SpanningTreeEdge;
                yield return new WaitForSeconds(GameManager.Instance.displayStepInterval);
            }
        }
    }

    public class CreateTileGizmoEvent : Event
    {
        private Tile tile;
        private Vector3 position;
        private Color color;
        private float width;
        private float height;
        private int sortingOrder;

        public CreateTileGizmoEvent(Tile tile, Color color, int sortingOrder)
        {
            this.tile = tile;
            this.position = new Vector3(tile.rect.x, tile.rect.y);
            this.color = color;
            this.width = tile.rect.width;
            this.height = tile.rect.height;
            this.sortingOrder = sortingOrder;
        }

        public IEnumerator OnEvent()
        {
            DungeonGizmo.Rect rect = null;
            if (false == GameManager.Instance.tileGizmos.TryGetValue(tile.index, out rect))
            {
                rect = new DungeonGizmo.Rect($"Tile_{tile.index}", color, width, height);
            }

            rect.parent = GameManager.Instance.tileGizmoRoot.transform;
            rect.position = position;
            rect.color = color;
            rect.sortingOrder = sortingOrder;
            GameManager.Instance.tileGizmos[tile.index] = rect;
            yield return new WaitForSeconds(GameManager.Instance.displayStepInterval/10);
        }
    }

    public class ChangeTileGizmoColorEvent : Event
    {
        private Tile tile;
        private Color color;

        public ChangeTileGizmoColorEvent(Tile tile, Color color)
        {
            this.tile = tile;
            this.color = color;
        }

        public IEnumerator OnEvent()
        {
            DungeonGizmo.Rect rect = null;
            if (false == GameManager.Instance.tileGizmos.TryGetValue(tile.index, out rect))
            {
                yield break;
            }

            rect.color = color;
            yield return new WaitForSeconds(GameManager.Instance.displayStepInterval);
        }
    }

    public class CreateCorridorGizmoEvent : Event
    {
        private string name;
        private Vector3 start;
        private Vector3 end;
        private Color color;
        private float width = 0.08f;
        private int sortingOrder = 0;

        public CreateCorridorGizmoEvent(string name, Vector3 start, Vector3 end, Color color, float width, int sortingOrder)
        {
            this.name = name;
            this.start = start;
            this.end = end;
            this.color = color;
            this.width = width;
            this.sortingOrder = sortingOrder;
        }

        public IEnumerator OnEvent()
        {
            DungeonGizmo.Line line = new DungeonGizmo.Line(name, color, start, end, width);
            line.sortingOrder = sortingOrder;
            line.parent = GameManager.Instance.corridorGizmoRoot.transform;
            yield return new WaitForSeconds(GameManager.Instance.displayStepInterval);
        }
    }

    public class ClearCorridorGizmoEvent : Event
    {
        public ClearCorridorGizmoEvent()
        {
        }

        public IEnumerator OnEvent()
        {
            GameManager.Instance.InitCorridorGizmo();
            yield break;
        }
    }

    public class CreateGridGizmoEvent : Event
    {
        public Rect rect;
        public CreateGridGizmoEvent(Rect rect)
        {
            this.rect = rect;
        }

        public IEnumerator OnEvent()
        {
            GameManager.Instance.backgroundGridGizmo = new DungeonGizmo.Grid("BackgroundGridGizmo", (int)rect.width, (int)rect.height);
            GameManager.Instance.backgroundGridGizmo.parent = GameManager.Instance.transform;
            yield break;
        }
    }

    public class MoveCameraEvent : Event
    {
        public Vector3 position;
        public Rect cameraBoundary;

        public MoveCameraEvent(Vector3 position, Rect cameraBoundary)
        {
            this.position = position;
            this.cameraBoundary = cameraBoundary;
        }

        public IEnumerator OnEvent()
        {
            float interpolation = 0.0f;
            Vector3 start = Camera.main.transform.position;
            position.z = Camera.main.transform.position.z;
            while (1.0f > interpolation)
            {
                interpolation += Time.deltaTime / GameManager.Instance.displayStepInterval;

                Camera.main.transform.position = Vector3.Lerp(start, this.position, interpolation);
                yield return null;
            }

            Camera.main.transform.position = this.position;
            GameManager.AdjustOrthographicCamera(cameraBoundary);
        }
    }

    public Queue<Event> events = new Queue<Event>();

    public void EnqueueEvent(Event evt)
    {
        events.Enqueue(evt);
    }

    public IEnumerator ExecuteEvent()
    {
        while (0 < events.Count)
        {
            var evt = events.Dequeue();
            yield return evt.OnEvent();
        }
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