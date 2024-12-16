using System.Collections.Generic;
using UnityEngine;

public class Dungeon : MonoBehaviour
{
    public static class SortingOrder
    {
        public static int Background = 0;
        public static int Block = 10;
        public static int Wall = 11;
        public static int Corridor = 20;
        public static int Edge = 40;
    }

    private float mouseWheelSpeed = 10.0f;
    private float minFieldOfView = 20.0f;
    private float maxFieldOfView = 120.0f;

    private DungeonGenerator generator;

    public int roomCount;
    public int minRoomSize;
	public int maxRoomSize;

    public bool enableBlockGizmo = true;
    public bool enableEdgeGizmo = true;
    public bool enableCorridorGizmo = true;
    public bool enableTileGizmo = true;
    public List<DungeonGizmo.Shape> blocks = new List<DungeonGizmo.Shape>();
    public List<DungeonGizmo.Shape> edges = new List<DungeonGizmo.Shape>();
    public List<DungeonGizmo.Shape> corridors = new List<DungeonGizmo.Shape>();
    public List<DungeonGizmo.Shape> tiles = new List<DungeonGizmo.Shape>();
    
    public void CreateRooms()
    {
        Clear();

        generator = new DungeonGenerator(roomCount, minRoomSize, maxRoomSize);
        foreach (var block in generator.blocks)
        {
            Color color;
            if (DungeonGenerator.Block.Type.Room == block.type)
            {
                color = Color.red;
            }
            else
            {
                color = Color.blue;
            }

            DungeonGizmo.Block gizmo = new DungeonGizmo.Block($"Block_{block.index}", color, block.rect.width, block.rect.height);
            gizmo.sortingOrder = SortingOrder.Block;
            gizmo.SetPosition(new Vector3(block.rect.x, block.rect.y));
            blocks.Add(gizmo);
        }

        SetCameraCenter();
    }

    public void CreateConnectionEdge()
    {
        if (null == generator)
        {
            return;
        }

        var graph = generator.graph;
        foreach (var edge in graph.edges)
        {
            int from = Mathf.Min(edge.p1.index, edge.p2.index);
            int to = Mathf.Max(edge.p1.index, edge.p2.index);
            DungeonGizmo.Line gizmo = new DungeonGizmo.Line($"Edge_{from}_{to}", Color.white, edge.p1.rect.center, edge.p2.rect.center, 0.15f);
            gizmo.sortingOrder = SortingOrder.Edge;
            edges.Add(gizmo);
        }

        foreach (var edge in graph.connections)
        {
            int from = Mathf.Min(edge.p1.index, edge.p2.index);
            int to = Mathf.Max(edge.p1.index, edge.p2.index);
            DungeonGizmo.Line gizmo = new DungeonGizmo.Line($"Connection_{from}_{to}", Color.green, edge.p1.rect.center, edge.p2.rect.center, 0.5f);
            gizmo.sortingOrder = SortingOrder.Edge + 1;
            edges.Add(gizmo);
        }
    }

    public void CreateCorridor()
    {
        DungeonGizmo.Grid grid = new DungeonGizmo.Grid("grid", generator.tilemap.width, generator.tilemap.height);

        SetCameraCenter();

        var graph = generator.graph;
        foreach (var edge in graph.connections)
        {
            foreach (var tile in edge.path)
            {
                DungeonGizmo.Point point = new DungeonGizmo.Point($"Path_{tile.index}", Color.white, 1.0f);
                point.SetPosition(new Vector3(tile.rect.x + 0.5f, tile.rect.y + 0.5f));
                point.sortingOrder = SortingOrder.Corridor;
                corridors.Add(point);
            }
        }
    }

    public void BuildWall()
    {
        enableBlockGizmo = false;
        enableEdgeGizmo = false;
        enableCorridorGizmo = false;
        EnableGizmo();

        foreach (var tile in generator.tilemap.tiles)
        {
            if (DungeonGenerator.Tile.Type.Wall == tile.type)
            {
                DungeonGizmo.Point point = new DungeonGizmo.Point($"Wall_{tile.index}", Color.yellow, 1.0f);
                point.sortingOrder = SortingOrder.Wall;
                point.SetPosition(new Vector3(tile.rect.x + 0.5f, tile.rect.y + 0.5f));
                tiles.Add(point);
            }

            if (DungeonGenerator.Tile.Type.Floor == tile.type)
            {
                DungeonGizmo.Point point = new DungeonGizmo.Point($"Wall_{tile.index}", Color.white, 1.0f);
                point.sortingOrder = SortingOrder.Wall;
                point.SetPosition(new Vector3(tile.rect.x + 0.5f, tile.rect.y + 0.5f));
                tiles.Add(point);
            }
        }
    }

    public void Clear()
    {
        //Camera.main.fieldOfView = (minFieldOfView + maxFieldOfView) / 2;
        enableBlockGizmo = true;
        enableEdgeGizmo = true;
        enableCorridorGizmo = true;
        enableTileGizmo = true;

        blocks.Clear();
        edges.Clear();
        corridors.Clear();
        tiles.Clear();
        DungeonGizmo.ClearAll();
    }

    public void EnableGizmo()
    {
        foreach (var shape in blocks)
        {
            shape.gameObject.SetActive(enableBlockGizmo);
        }

        foreach (var shape in edges)
        {
            shape.gameObject.SetActive(enableEdgeGizmo);
        }

        foreach (var shape in corridors)
        {
            shape.gameObject.SetActive(enableCorridorGizmo);
        }

        foreach (var shape in tiles)
        {
            shape.gameObject.SetActive(enableTileGizmo);
        }
    }

    private void SetCameraCenter()
    {
        Vector2 center = Vector2.zero;
        foreach (var block in generator.blocks)
        {
            center += block.rect.center;
        }

        center = center / generator.blocks.Count;
        Camera.main.transform.position = new Vector3(center.x, center.y, Camera.main.transform.position.z);
    }

    private void Update()
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

            Vector3 cameraPosition = Camera.main.transform.position;
            cameraPosition.x = hit.point.x;
            cameraPosition.y = hit.point.y;
            Camera.main.transform.position = cameraPosition;
        }

        var camera = Camera.main;

        float scroll = Input.GetAxis("Mouse ScrollWheel") * mouseWheelSpeed;
        if (Camera.main.fieldOfView < minFieldOfView && scroll < 0.0f)
        {
            Camera.main.fieldOfView = minFieldOfView;
        }
        else if (Camera.main.fieldOfView > maxFieldOfView && scroll > 0.0f)
        {
            Camera.main.fieldOfView = maxFieldOfView;
        }
        else
        {
            Camera.main.fieldOfView -= scroll;
        }
    }
}
