using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileMap : MonoBehaviour
{
    public int width = 5;
    public int height = 5;
    public Tile[] tiles;

    public Tile from;
    public Tile to;
    public Tile select;
    public Recorder recorder;

    private static TileMap instance;

    public static TileMap GetInstance()
    {
        if (null == instance)
        {
            instance = FindFirstObjectByType<TileMap>();

            if (null == instance)
            {
                GameObject go = new GameObject();
                go.name = typeof(TileMap).Name;
                instance = go.AddComponent<TileMap>();
            }
        }
        return instance;
    }

    private void Start()
    {
        tiles = null;
        CreateTiles();
    }

    private void Update()
    {
        if (true == Input.GetMouseButton(0))
        {
            if (null != select)
            {
                if (Tile.TileType.Floor == select.type)
                {
                    select.SetColor(Tile.ColorType.Floor);
                }

                if (Tile.TileType.Wall == select.type)
                {
                    select.SetColor(Tile.ColorType.Wall);
                }

                if (to == select)
                {
                    select.SetColor(Tile.ColorType.To);
                }

                if (from == select)
                {
                    select.SetColor(Tile.ColorType.From);
                }
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit raycastHit;
            if (false == Physics.Raycast(ray, out raycastHit))
            {
                return;
            }

            if (null == raycastHit.transform)
            {
                return;
            }

            GameObject go = raycastHit.transform.gameObject;
            Tile tile = go.GetComponent<Tile>();
            if (null == tile)
            {
                return;
            }

            select = tile;
            select.SetColor(Tile.ColorType.Select);
        }
    }

    public void CreateTiles()
    {
        if (null != tiles)
        {
            foreach (Tile tile in tiles)
            {
                tile.transform.parent = null;
                GameObject.Destroy(tile.gameObject);
            }
        }

        tiles = new Tile[width * height];
        for (int i = 0; i < width * height; i++)
        {
            float y = i / width;
            float x = i % width;

            GameObject go = new GameObject();
            go.name = $"Tile_{x}_{y}";
            go.transform.position = new Vector3(x, y, 0.0f);
            go.transform.parent = transform;

            var tile = go.AddComponent<Tile>();
            tile.Init(i, Tile.TileType.Floor);
            tiles[i] = tile;
        }

        Camera.main.transform.position = new Vector3(width/2, height/2, Camera.main.transform.position.z);
    }

    public void Clear()
    {
        recorder = null;

        if (null != tiles)
        {
            foreach (Tile tile in tiles)
            {
                tile.Init(tile.index, tile.type);
            }
        }

        if (null != from)
        {
            from.SetColor(Tile.ColorType.From);
        }

        if (null != to)
        {
            to.SetColor(Tile.ColorType.To);
        }
    }

    public Tile GetTile(int x, int y)
    {
        if(0 > x || x >= width)
        {
            return null; 
        }

        if(0 > y || y >= height)
        {
            return null; 
        }

        return tiles[y * width + x];
    }

    public void FindPath()
    {
        if (null == TileMap.GetInstance().from)
        {
            return;
        }

        if (null == TileMap.GetInstance().to)
        {
            return;
        }

        Clear();

        if (null != recorder)
        {
            recorder.Stop();
        }

        this.recorder = new Recorder();
        
        AStarPathFinder aStarPathFinder = new AStarPathFinder();
        var paths = aStarPathFinder.FindPath(from, to);
        this.recorder.SetPath(paths);
        this.recorder.Next();
    }

    public class AStarPathFinder
    {
        public static Vector2Int[] LOOKUP_OFFSETS = {
            new Vector2Int( 1, 0),
            new Vector2Int( 0, 1),
            new Vector2Int(-1, 0),
            new Vector2Int( 0,-1),
        };

        public class Node
        {
            public Tile tile;
            public Node parent;
            public int index { get { return tile.index; } }
            public int pathCost;    // 출발 노드에서 현재 노드까지 도달하기 위한 최소 비용
            public int expectCost;  // 현재 노드에서 목표 노드까지 도달하기 위한 예상 비용
            public int cost { get { return pathCost + expectCost; } }

            public Node(Tile tile)
            {
                this.tile = tile;
                this.pathCost = 0;
                this.expectCost = 0;
                this.expectCost += (int)Mathf.Abs(TileMap.GetInstance().to.transform.position.x - tile.transform.position.x);
                this.expectCost += (int)Mathf.Abs(TileMap.GetInstance().to.transform.position.y - tile.transform.position.y);
            }

            public Node(Node node)
            {
                this.tile = node.tile;
                this.pathCost = node.pathCost;
                this.expectCost = node.expectCost;
                this.parent = node.parent;
            }
        }

        public List<Tile> FindPath(Tile from, Tile to)
        {
            List<Tile> path = new List<Tile>();
            Dictionary<int, Node> openNodes = new Dictionary<int, Node>(); // 열린 목록. 출발 타일 주변 장애물을 무시하고 지나갈 수 있는 타일 목록. 이 타일들은 출발 타일을 부모로 지정한다. 부모 노드는 경로를 다 탐색한 후 거슬러 올라가는 용도로 사용 된다.
            Dictionary<int, Node> closeNodes = new Dictionary<int, Node>(); // 닫힌 목록. 이미 검색을 끝내고 다시 볼 필요 없는 사각형들

            Node current = new Node(from);
            openNodes.Add(current.index, current);

            while (0 < openNodes.Count)
            {
                TileMap.GetInstance().recorder.Record(current, openNodes, closeNodes);

                List<Node> sortedNodes = openNodes.Values.ToList<Node>();
                if (0 == sortedNodes.Count)
                {
                    break;  // 경로 찾지 못함
                }

                sortedNodes.Sort((Node lhs, Node rhs) => {
                    if (lhs.cost > rhs.cost)
                    {
                        return 1;
                    }
                    else if (lhs.cost < rhs.cost)
                    {
                        return -1;
                    }
                    else if (lhs.expectCost > rhs.expectCost)
                    {
                        return 1;
                    }
                    else if (lhs.expectCost < rhs.expectCost)
                    {
                        return -1;
                    }
                    return 0;
                });

                current = sortedNodes[0];

                Vector3 position = current.tile.transform.position;
                List<Node> children = new List<Node>();
                int offsetIndex = UnityEngine.Random.Range(0, LOOKUP_OFFSETS.Length);
                for(int i = 0; i < LOOKUP_OFFSETS.Length; i++)// 장애물로 막혀 있는데 말고 갈 수 있는 타일들을 openNode 리스트에 넣는다
                {
                    var offset = LOOKUP_OFFSETS[offsetIndex];

                    int x = (int)position.x + offset.x;
                    int y = (int)position.y + offset.y;

                    offsetIndex += 1;
                    offsetIndex %= LOOKUP_OFFSETS.Length; // 다음 타일을 선택할 때 랜덤 성을 주기 위해

                    Tile tile = TileMap.GetInstance().GetTile(x, y);
                    if (null == tile)
                    {
                        continue;
                    }

                    if (to == tile)
                    {
                        TileMap.GetInstance().recorder.Record(current, openNodes, closeNodes);
                        path.Add(tile);
                        do
                        {
                            path.Add(current.tile);
                            current = current.parent;
                        } while (null != current);
                        return path;
                    }

                    if (Tile.TileType.Wall == tile.type)
                    {
                        continue;
                    }

                    if (true == closeNodes.ContainsKey(tile.index)) // 탐색을 끝내고 이미 닫힌 노드에 들어간 타일임
                    {
                        continue;
                    }

                    if (true == openNodes.ContainsKey(tile.index)) // 앞에서 한번 열린 노드에 들어 왔던 타일
                    {
                        Node openNode = openNodes[tile.index];
                        if (openNode.pathCost < current.pathCost)
                        {
                            current.pathCost = openNode.pathCost + 1;
                            current.parent = openNode;
                        }
                        continue;
                    }

                    Node child = new Node(tile);
                    child.parent = current;
                    child.pathCost = current.pathCost + 1;
                    openNodes.Add(child.index, child);
                }

                openNodes.Remove(current.index);
                closeNodes.Add(current.index, current);
            }

            return path;
        }
    }

    public class Recorder
    {
        private class SnapShot
        {
            public AStarPathFinder.Node currentNode;
            public List<AStarPathFinder.Node> openNodes = new List<AStarPathFinder.Node>();
            public List<AStarPathFinder.Node> closeNodes = new List<AStarPathFinder.Node>();
        }

        private List<Tile> paths = null;
        private List<SnapShot> snapShots = new List<SnapShot>();
        private IEnumerator enumerator;

        public void Record(AStarPathFinder.Node current, Dictionary<int, AStarPathFinder.Node> openNodes, Dictionary<int, AStarPathFinder.Node> closeNodes)
        {
            SnapShot snapShot = new SnapShot();
            snapShot.currentNode = new AStarPathFinder.Node(current);
            foreach (AStarPathFinder.Node openNode in openNodes.Values.ToList())
            {
                if (snapShot.currentNode.index == openNode.index)
                {
                    continue;
                }

                snapShot.openNodes.Add(new AStarPathFinder.Node(openNode));
            }

            foreach (AStarPathFinder.Node closeNode in closeNodes.Values.ToList())
            {
                snapShot.closeNodes.Add(new AStarPathFinder.Node(closeNode));
            }

            snapShots.Add(snapShot);
        }

        public void SetPath(List<Tile> paths)
        {
            this.paths = paths;
            this.enumerator = Replay();
        }

        public void Next()
        {
            if (null == enumerator)
            {
                return;
            }

            TileMap.GetInstance().StartCoroutine(enumerator);
        }

        public void Stop()
        {
            if (null == enumerator)
            {
                return;
            }

            paths = null;
            snapShots = null;
            enumerator = null;
            TileMap.GetInstance().StopCoroutine(enumerator);
        }

        private IEnumerator Replay()
        {
            int width = TileMap.GetInstance().width;
            int height = TileMap.GetInstance().height;

            foreach (Tile path in paths)
            {
                path.SetColor(Tile.ColorType.Path);
            }

            TileMap.GetInstance().from.SetColor(Tile.ColorType.From);
            TileMap.GetInstance().to.SetColor(Tile.ColorType.To);
            TileMap.GetInstance().StopCoroutine(enumerator);
            yield return null;

            while (0 < snapShots.Count)
            {
                SnapShot snapShot = snapShots[0];
                snapShots.RemoveAt(0);

                foreach (AStarPathFinder.Node openNode in snapShot.openNodes)
                {
                    Tile parentTile = openNode.parent.tile;
                    Tile openTile = openNode.tile;

                    openTile.SetArrow(parentTile);
                    openTile.SetColor(Tile.ColorType.Open);
                    openTile.pathCost = openNode.pathCost;
                    openTile.expectCost = openNode.expectCost;
                    openTile.cost = openNode.cost;
                }

                foreach (AStarPathFinder.Node closeNode in snapShot.closeNodes)
                {
                    Tile parentTile = null;
                    if (null != closeNode.parent)
                    {
                        parentTile = closeNode.parent.tile;
                    }

                    Tile closeTile = closeNode.tile;

                    closeTile.SetArrow(parentTile);
                    closeTile.SetColor(Tile.ColorType.Close);
                    closeTile.pathCost = closeNode.pathCost;
                    closeTile.expectCost = closeNode.expectCost;
                    closeTile.cost = closeNode.cost;
                }

                Tile currentTile = snapShot.currentNode.tile;

                currentTile.SetColor(Tile.ColorType.Current);
                currentTile.pathCost = snapShot.currentNode.pathCost;
                currentTile.expectCost = snapShot.currentNode.expectCost;
                currentTile.cost = snapShot.currentNode.cost;
                
                TileMap.GetInstance().StopCoroutine(enumerator);
                yield return null;
            }

            foreach (Tile path in paths)
            {
                path.SetColor(Tile.ColorType.Path);
            }

            TileMap.GetInstance().from.SetColor(Tile.ColorType.From);
            TileMap.GetInstance().to.SetColor(Tile.ColorType.To);
            TileMap.GetInstance().recorder = null;
            enumerator = null;
        }
    }
}
