using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NDungeon.NTileMap
{
    public class AStarPathFinder
    {
        private static Vector2Int[] LOOKUP_OFFSETS = {
            new Vector2Int(-1, 0),  // left
            new Vector2Int( 0,-1),  // down
            new Vector2Int( 1, 0),  // right
            new Vector2Int( 0, 1)   // up
        };

        public class Node
        {
            public TileMap.Tile tile;
            public Node parent;
            public int index { get => this.tile.index; }
            public int pathCost;
            public int expectCost;
            public int cost { get => this.pathCost + this.expectCost; }

            public Node(TileMap.Tile tile)
            {
                this.tile = tile;
                this.pathCost = 0;
                this.expectCost = 0;
            }
        }

        private TileMap tileMap;
        private Rect boundary;

        public List<TileMap.Tile> path = new List<TileMap.Tile>();

        public AStarPathFinder(TileMap tileMap, Rect pathFindBoundary)
        {
            this.tileMap = tileMap;
            this.boundary = pathFindBoundary;
        }

        public List<TileMap.Tile> FindPath(TileMap.Tile from, TileMap.Tile to)
        {
            Dictionary<int, Node> openNodes = new Dictionary<int, Node>();
            Dictionary<int, Node> closeNodes = new Dictionary<int, Node>();

            Node currentNode = new Node(from);
            currentNode.expectCost += (int)Mathf.Abs(to.rect.x - from.rect.x);
            currentNode.expectCost += (int)Mathf.Abs(to.rect.y - from.rect.y);
            openNodes.Add(currentNode.index, currentNode);

            while (0 < openNodes.Count)
            {
                List<Node> sortedNodes = openNodes.Values.ToList<Node>();
                if (0 == sortedNodes.Count)
                {
                    break;  // ��� ã�� ����
                }

                sortedNodes.Sort((Node lhs, Node rhs) =>
                {
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

                currentNode = sortedNodes[0];

                List<Node> children = new List<Node>();
                int offsetIndex = UnityEngine.Random.Range(0, LOOKUP_OFFSETS.Length);
                for (int i = 0; i < LOOKUP_OFFSETS.Length; i++)// ��ֹ��� ���� �ִµ� ���� �� �� �ִ� Ÿ�ϵ��� openNode ����Ʈ�� �ִ´�
                {
                    var offset = LOOKUP_OFFSETS[offsetIndex];

                    int x = currentNode.index % tileMap.width + offset.x;
                    int y = currentNode.index / tileMap.width + offset.y;

                    offsetIndex += 1;
                    offsetIndex %= LOOKUP_OFFSETS.Length;

                    var tile = this.GetTile(x, y);
                    if (null == tile)
                    {
                        continue;
                    }

                    if (to == tile)
                    {
                        path.Insert(0, tile);
                        do
                        {
                            path.Insert(0, currentNode.tile);
                            currentNode = currentNode.parent;
                        } while (null != currentNode);
                        return path;
                    }

                    if (TileMap.Tile.Type.Wall == tile.type)
                    {
                        continue;
                    }

                    if (true == closeNodes.ContainsKey(tile.index)) // Ž���� ������ �̹� ���� ��忡 �� Ÿ����
                    {
                        continue;
                    }

                    if (true == openNodes.ContainsKey(tile.index)) // �տ��� �ѹ� ���� ��忡 ��� �Դ� Ÿ��
                    {
                        Node openNode = openNodes[tile.index];
                        if (openNode.pathCost + tile.cost < currentNode.pathCost)
                        {
                            currentNode.pathCost = openNode.pathCost + tile.cost;
                            currentNode.parent = openNode;
                        }
                        continue;
                    }

                    Node child = new Node(tile);
                    child.parent = currentNode;
                    child.pathCost = currentNode.pathCost + tile.cost;
                    child.expectCost += (int)Mathf.Abs(to.rect.x - tile.rect.x);
                    child.expectCost += (int)Mathf.Abs(to.rect.y - tile.rect.y);

                    openNodes.Add(child.index, child);
                }

                openNodes.Remove(currentNode.index);
                closeNodes.Add(currentNode.index, currentNode);
            }

            return path;
        }

        private TileMap.Tile GetTile(int x, int y)
        {
            if (boundary.xMin > x || x >= boundary.xMax)
            {
                return null;
            }

            if (boundary.yMin > y || y >= boundary.yMax)
            {
                return null;
            }

            return tileMap.GetTile(x, y);
        }
    }

}