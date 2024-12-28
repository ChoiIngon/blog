using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Data
{
    public class AStarPathFinder
    {
        private static Vector2Int[] LOOKUP_OFFSETS = {
            new Vector2Int(-1, 0),  // left
            new Vector2Int( 0,-1),  // down
            new Vector2Int( 1, 0),  // right
            new Vector2Int( 0, 1)   // up
        };

        public interface LookupOffsetPolicy
        {
            public int GetOffsetIndex(Node node);
        }

        public class StraightLookup : LookupOffsetPolicy
        {
            public int GetOffsetIndex(Node node)
            {
                if (null != node.parent)
                {
                    if (node.index < node.parent.index)
                    {
                        if (node.index + 1 == node.parent.index) // 좌측으로 이동 중
                        {
                            return 0;
                        }
                        return 1;
                    }

                    if (node.index > node.parent.index)
                    {
                        if (node.index - 1 == node.parent.index) // 우측으로 이동
                        {
                            return 2;
                        }

                        return 3;
                    }
                }
                return UnityEngine.Random.Range(0, LOOKUP_OFFSETS.Length);
            }
        }

        public class RandomLookup : LookupOffsetPolicy
        {
            public int GetOffsetIndex(Node node)
            {
                return UnityEngine.Random.Range(0, LOOKUP_OFFSETS.Length);
            }
        }

        public class Node
        {
            public Tile tile;
            public Node parent;
            public int index { get { return tile.index; } }
            public int pathCost;
            public int expectCost;
            public int cost { get { return pathCost + expectCost; } }

            public Node(Tile tile)
            {
                this.tile = tile;
                this.pathCost = 0;
                this.expectCost = 0;
            }
        }

        private TileMap tileMap;
        private Rect boundary;
        private LookupOffsetPolicy lookupOffsetPolicy;

        public List<Tile> tiles = new List<Tile>();

        public AStarPathFinder(TileMap tileMap, Rect pathFindBoundary, LookupOffsetPolicy offsetPolicy)
        {
            this.tileMap = tileMap;
            this.boundary = pathFindBoundary;
            this.lookupOffsetPolicy = offsetPolicy;
        }

        public List<Tile> FindPath(Tile from, Tile to)
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
                    break;  // 경로 찾지 못함
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
                int offsetIndex = lookupOffsetPolicy.GetOffsetIndex(currentNode);
                for (int i = 0; i < LOOKUP_OFFSETS.Length; i++)// 장애물로 막혀 있는데 말고 갈 수 있는 타일들을 openNode 리스트에 넣는다
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
                        tiles.Insert(0, tile);
                        do
                        {
                            tiles.Insert(0, currentNode.tile);
                            currentNode = currentNode.parent;
                        } while (null != currentNode);
                        return tiles;
                    }

                    if (Tile.Type.Wall == tile.type)
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

            return tiles;
        }

        private Tile GetTile(int x, int y)
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