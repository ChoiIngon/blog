using System.Collections.Generic;

namespace NDungeon.NTileMap
{
    public class MinimumSpanningTree
    {
        public class Edge
        {
            public Edge(TileMap.Room p1, TileMap.Room p2, float cost)
            {
                this.room1 = p1;
                this.room2 = p2;
                this.cost = cost;
            }

            public TileMap.Room room1;
            public TileMap.Room room2;
            public float cost;
        }

        private Dictionary<TileMap.Room, TileMap.Room> parents = new Dictionary<TileMap.Room, TileMap.Room>();
        public List<Edge> edges = new List<Edge>();
        public List<Edge> connections = new List<Edge>();

        public MinimumSpanningTree(List<TileMap.Room> rooms)
        {
            foreach (TileMap.Room room in rooms)
            {
                parents.Add(room, room);
            }
        }

        public void AddEdge(Edge edge)
        {
            foreach (Edge other in edges)
            {
                if (true == (edge.room1 == other.room1 && edge.room2 == other.room2) || (edge.room1 == other.room2 && edge.room2 == other.room1))
                {
                    return;
                }
            }

            edges.Add(edge);
        }

        public void BuildTree()
        {
            edges.Sort((Edge e1, Edge e2) =>
            {
                if (e1.cost == e2.cost)
                {
                    return 0;
                }
                else if (e1.cost > e2.cost)
                {
                    return 1;
                }
                return -1;
            });

            foreach (Edge edge in edges)
            {
                TileMap.Room srcParent = FindParent(edge.room1);
                TileMap.Room destParent = FindParent(edge.room2);

                if (srcParent != destParent)
                {
                    connections.Add(edge);
                    Union(srcParent, destParent);
                }
            }
        }

        private TileMap.Room FindParent(TileMap.Room room)
        {
            var parent = parents[room];
            if (parent != room)
            {
                parents[room] = FindParent(parent);
            }
            return parents[room];
        }

        private void Union(TileMap.Room src, TileMap.Room dest)
        {
            TileMap.Room srcParent = FindParent(src);
            TileMap.Room destParent = FindParent(dest);
            parents[srcParent] = destParent;
        }
    }
}