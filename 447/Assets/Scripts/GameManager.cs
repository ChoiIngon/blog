using System;
using System.Collections;
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
    private TileMap tileMap;

    private Coroutine coroutine;

    public ResourceManager Resources = new ResourceManager();

    private void Start()
    {
        gameObject.AddComponent<CameraDrag>();
        gameObject.AddComponent<CameraScale>();

        Resources.Load();
        DungeonTileMapGenerator.Init();
    }

    public void CreateDungeon()
    {
		events.Clear();
		if (null != coroutine)
		{
			StopCoroutine(coroutine);
			coroutine = null;
		}

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

        GameManager.Instance.EnqueueEvent(new GameManager.EnableGizmoEvent(GameManager.EventName.RoomGizmo, false));
        GameManager.Instance.EnqueueEvent(new GameManager.EnableGizmoEvent(GameManager.EventName.TileGizmo, false));

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

    public static class EventName
    {
        public const string RoomGizmo = "RoomGizmo";
        public const string MiniumSpanningTreeGizmo = "MiniumSpanningTreeGizmo";
        public const string TileCostGizmo = "TileCostGizmo";
        public const string BackgroundGridGizmo = "BackgroundGridGizmo";
        public const string TileGizmo = "TileGizmo";
    }

    public class EnableTileSpriteEvent : Event
    {
        private Tile tile;

        public EnableTileSpriteEvent(Tile tile)
        {
            this.tile = tile;
        }

        public IEnumerator OnEvent()
        {
            tile.Visible(true);

            yield return new WaitForSeconds(GameManager.Instance.tickTime/10);
        }
    }

    public class CreateRoomGizmoEvent : Event
    {
        public Room room;
        public Vector3 position;
        public Rect cameraBoundary;
        public Color color;

        public CreateRoomGizmoEvent(Room room, Rect cameraBoundary, Color color)
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

            GameObject gizmoRoot = null;
            if (false == GameManager.Instance.gizmos.TryGetValue(EventName.RoomGizmo, out gizmoRoot))
            {
                gizmoRoot = new GameObject(EventName.RoomGizmo);
                gizmoRoot.transform.parent = GameManager.Instance.transform;

                GameManager.Instance.gizmos.Add(EventName.RoomGizmo, gizmoRoot);
            }

            var roomGizmo = new DungeonGizmo.Block($"{EventName.RoomGizmo}_{room.index}", color, room.rect.width, room.rect.height);
            roomGizmo.parent = gizmoRoot.transform;
            roomGizmo.position = new Vector3(position.x, position.y, 0.0f);
            roomGizmo.sortingOrder = SortingOrder.Room;

            GameManager.Instance.roomGizmos.Add(room.index, roomGizmo);

            GameManager.AdjustOrthographicCamera(cameraBoundary);
            Camera.main.transform.position = new Vector3(cameraBoundary.center.x, cameraBoundary.center.y, Camera.main.transform.position.z);

            DungeonLog.Write($"The room {room.index} has been created");
        }
    }

    public class MoveRoomGizmoEvent : Event
    {
        private struct Data
        {
            public int index;
            public Vector3 position;
        }

        private List<Data> datas = new List<Data>();
        private Rect cameraBoundary;
        
        public MoveRoomGizmoEvent(List<Room> rooms)
        {
            this.datas = new List<Data>();
            foreach (Room room in rooms)
            {
                this.datas.Add(new Data() { index = room.index, position = room.position });
            }

            this.cameraBoundary = DungeonTileMapGenerator.GetBoundaryRect(rooms);
        }

        public IEnumerator OnEvent()
        {
            GameObject gizmoRoot = null;
            if (false == GameManager.Instance.gizmos.TryGetValue(EventName.RoomGizmo, out gizmoRoot))
            {
                yield break;
            }

            foreach (Data data in this.datas)
            {
                DungeonGizmo.Block gizmo;
                if (false == GameManager.Instance.roomGizmos.TryGetValue(data.index, out gizmo))
                {
                    continue;
                }

                if (gizmo.position == data.position)
                {
                    continue;
                }

                float interpolation = 0.0f;
                Vector3 start = gizmo.position;
                while (1.0f > interpolation)
                {
                    interpolation += Time.deltaTime / GameManager.Instance.tickTime;
                    gizmo.position = Vector3.Lerp(start, data.position, interpolation);
                    yield return null;
                }

                gizmo.position = data.position;
            }

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
            yield return new WaitForSeconds(GameManager.Instance.tickTime);
        }
    }

    public class DestroyRoomGizmoEvent : Event
    {
        public Room room;

        public DestroyRoomGizmoEvent(Room room)
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
            yield return new WaitForSeconds(GameManager.Instance.tickTime);
        }
    }

    public class FindRoomPositionEvent : Event
    {
        private DelaunayTriangulation triangulation;
        private DelaunayTriangulation.Circle biggestCircle;

        public FindRoomPositionEvent(DelaunayTriangulation triangulation, DelaunayTriangulation.Circle biggestCircle)
        {
            this.triangulation = triangulation;
            this.biggestCircle = biggestCircle;
        }

        public IEnumerator OnEvent()
        {
            int index = 1;
            float interval = GameManager.Instance.tickTime / triangulation.triangles.Count;
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
            yield return new WaitForSeconds(GameManager.Instance.tickTime);

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
            float interval = GameManager.Instance.tickTime / tileCount;
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
            if (null == tile)
            {
                return;
            }

            if (Tile.Type.Wall == tile.type)
            {
                GameObject gizmoRoot = null;
                if (false == GameManager.Instance.gizmos.TryGetValue(EventName.TileGizmo, out gizmoRoot))
                {
                    return;
                }

                DungeonGizmo.Rect gizmo = new DungeonGizmo.Rect($"Tile_{tile.index}", Color.white, 1.0f, 1.0f);
                gizmo.sortingOrder = GameManager.SortingOrder.Floor;
                gizmo.position = new Vector3(x, y);
                gizmo.parent = gizmoRoot.transform;
                GameManager.Instance.tileGizmos[tile.index] = gizmo;
            }
        }
    }

    public class BuildCorridorWallEvent : Event
    {
        private List<DungeonTileMapGenerator.Corridor> corridors;

        public BuildCorridorWallEvent(List<DungeonTileMapGenerator.Corridor> corridors)
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

                GameObject gizmoRoot = null;
                if (false == GameManager.Instance.gizmos.TryGetValue(EventName.TileGizmo, out gizmoRoot))
                {
                    return;
                }

                DungeonGizmo.Rect gizmo = null;
                if (false == GameManager.Instance.tileGizmos.TryGetValue(tile.index, out gizmo))
                {
                    gizmo = new DungeonGizmo.Rect($"Tile_{tile.index}", Color.white, 1.0f, 1.0f);
                    gizmo.position = new Vector3(tile.rect.x, tile.rect.y);
                    gizmo.parent = gizmoRoot.transform;
                    GameManager.Instance.tileGizmos.Add(tile.index, gizmo);
                    return;
                }

                gizmo.color = Color.white;
            };
            
            foreach (var corridor in corridors)
            {
                float interval = GameManager.Instance.tickTime / corridor.path.Count;
                foreach (Tile tile in corridor.path)
                {
                    DungeonGizmo.Rect gizmo = null;
                    if (false == GameManager.Instance.tileGizmos.TryGetValue(tile.index, out gizmo))
                    {
                        yield break;
                    }

                    gizmo.color = Color.red;

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
        }
    }

    public class CreateLineGizmoEvent : Event
    {
        public struct Line
        {
            public Vector3 start;
            public Vector3 end;
        }

        private string name;
        private List<Line> lines;
        private Color color;
        private int sortingOrder;
        private float width;

        public CreateLineGizmoEvent(string name, List<Line> lines, Color color, int sortingOrder, float width)
        {
            this.name = name;
            this.lines = lines;
            this.color = color;
            this.sortingOrder = sortingOrder;
            this.width = width;
        }

        public IEnumerator OnEvent()
        {
            if (null == lines)
            {
                yield break;
            }

            GameObject giizmoRoot = new GameObject(name);
            giizmoRoot.transform.parent = GameManager.Instance.transform;

            GameManager.Instance.gizmos.Add(name, giizmoRoot);

            float interval = GameManager.Instance.tickTime / lines.Count;
            for(int i=0; i<lines.Count; i++)
            {
                Line line = lines[i];
                string gizmoName = $"{name}_{i}_({line.start.x},{line.start.y}) -> ({line.end.x},{line.end.y})";
                DungeonGizmo.Line gizmo = new DungeonGizmo.Line(gizmoName, color, line.start, line.end, width);
                gizmo.parent = giizmoRoot.transform;
                gizmo.sortingOrder = sortingOrder;
                yield return new WaitForSeconds(interval);
            }
        }
    }

    public class DestroyGizmoEvent : Event
    {
        private string name;

        public DestroyGizmoEvent(string name)
        {
            this.name = name;
        }

        public IEnumerator OnEvent()
        {
            GameObject gizmoRoot = null;
            if (false == GameManager.Instance.gizmos.TryGetValue(name, out gizmoRoot))
            {
                yield break;
            }

            while (0 < gizmoRoot.transform.childCount)
            {
                Transform gizmoTransform = gizmoRoot.transform.GetChild(0);
                gizmoTransform.parent = null;
                GameObject gizmoGameObject = gizmoTransform.gameObject;
                GameObject.DestroyImmediate(gizmoGameObject);
            }
            
            gizmoRoot.transform.parent = null;
            GameObject.DestroyImmediate(gizmoRoot);

            GameManager.Instance.gizmos.Remove(name);
        }
    }

    public class EnableGizmoEvent : Event
    {
        private string name;
        private bool enable;

        public EnableGizmoEvent(string name, bool enable)
        {
            this.name = name;
            this.enable = enable;
        }

        public IEnumerator OnEvent()
        {
            GameObject gizmoRoot = null;
            if (false == GameManager.Instance.gizmos.TryGetValue(name, out gizmoRoot))
            {
                yield break;
            }

            gizmoRoot.SetActive(enable);
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
            GameObject gizmoRoot = null;
            if (false == GameManager.Instance.gizmos.TryGetValue(EventName.TileGizmo, out gizmoRoot))
            {
                gizmoRoot = new GameObject(EventName.TileGizmo);
                gizmoRoot.transform.parent = GameManager.Instance.transform;

                GameManager.Instance.gizmos.Add(EventName.TileGizmo, gizmoRoot);
            }

            DungeonGizmo.Rect gizmo = null;
            if (false == GameManager.Instance.tileGizmos.TryGetValue(tile.index, out gizmo))
            {
                gizmo = new DungeonGizmo.Rect($"Tile_{tile.index}", color, width, height);
                GameManager.Instance.tileGizmos.Add(tile.index, gizmo);
            }

            gizmo.parent = gizmoRoot.transform;
            gizmo.position = position;
            gizmo.color = color;
            gizmo.sortingOrder = sortingOrder;
            
            yield return new WaitForSeconds(GameManager.Instance.tickTime/10);
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
            yield return new WaitForSeconds(GameManager.Instance.tickTime);
        }
    }

    public class CreateTileCostGizmoEvent : Event
    {
        private List<Tile> tiles;

        public CreateTileCostGizmoEvent(List<Tile> tiles)
        {
            this.tiles = tiles;
        }

        public IEnumerator OnEvent()
        {
            if (null == tiles)
            {
                yield break;
            }

            GameObject gizmoRoot = null;
            if (false == GameManager.Instance.gizmos.TryGetValue(EventName.TileCostGizmo, out gizmoRoot))
            {
                gizmoRoot = new GameObject(EventName.TileCostGizmo);
                gizmoRoot.transform.parent = GameManager.Instance.transform;

                GameManager.Instance.gizmos.Add(EventName.TileCostGizmo, gizmoRoot);
            }
            
            float interval = GameManager.Instance.tickTime / tiles.Count;
            for (int i = 0; i < tiles.Count; i++)
            {
                Tile tile = tiles[i];

                DungeonGizmo.Rect gizmo = new DungeonGizmo.Rect($"TileCost_{tile.index}", Color.white, tile.rect.width, tile.rect.height);
                gizmo.parent = gizmoRoot.transform;
                gizmo.sortingOrder = GameManager.SortingOrder.Corridor;
                gizmo.position = new Vector3(tile.rect.x, tile.rect.y);
                yield return new WaitForSeconds(interval);
            }
        }
    }

    public class CreateGridGizmoEvent : Event
    {
        private string name;
        private Rect rect;
        public CreateGridGizmoEvent(string name, Rect rect)
        {
            this.name = name;
            this.rect = rect;
        }

        public IEnumerator OnEvent()
        {
            GameObject gizmoRoot = new GameObject(name);
            gizmoRoot.transform.parent = GameManager.Instance.transform;
            GameManager.Instance.gizmos.Add(name, gizmoRoot);
            
            DungeonGizmo.Grid gizmo = new DungeonGizmo.Grid(name, (int)rect.width, (int)rect.height);
            gizmo.gameObject.transform.parent = gizmoRoot.transform;
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
                interpolation += Time.deltaTime / GameManager.Instance.tickTime;

                Camera.main.transform.position = Vector3.Lerp(start, this.position, interpolation);
                yield return null;
            }

            Camera.main.transform.position = this.position;
            GameManager.AdjustOrthographicCamera(cameraBoundary);
        }
    }

    public class WriteLog : Event
    {
        private string text;
        private Color color;
        private int fontSize;

        public WriteLog(string text, Color color, int fontSize = 12)
        {
            this.text = text;
            this.color = color;
            this.fontSize = fontSize;
        }

        public IEnumerator OnEvent()
        {
            string hexColor = ColorToHex(color);
            DungeonLog.Write($"<size={fontSize}><color={hexColor}>{text}");
            yield break; ;
        }

        private string ColorToHex(Color color)
        {
            int r = Mathf.RoundToInt(color.r * 255);
            int g = Mathf.RoundToInt(color.g * 255);
            int b = Mathf.RoundToInt(color.b * 255);
            return $"#{r:X2}{g:X2}{b:X2}"; // 2자리 HEX 문자열 변환
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