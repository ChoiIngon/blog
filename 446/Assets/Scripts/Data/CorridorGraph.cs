using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    public class CorridorGraph
    {
        public class CorridorPath
        {
            public CorridorPath(Block p1, Block p2)
            {
                this.p1 = p1;
                this.p2 = p2;
                this.tiles = new List<Tile>();
            }

            public Block p1;
            public Block p2;
            public List<Tile> tiles;
        }

        public List<CorridorPath> corridors;
        
        public CorridorGraph(Dungeon dungeon)
        {
            var triangulation = new DelaunayTriangulation(dungeon.rooms);
            var mst = new MinimumSpanningTree(dungeon.rooms);

            foreach (var triangle in triangulation.triangles)
            {
                foreach (var edge in triangle.edges)
                {
                    mst.AddEdge(new MinimumSpanningTree.Edge(edge.v0.block, edge.v1.block, Vector3.Distance(edge.v0.block.rect.center, edge.v1.block.rect.center)));
                }
            }

            mst.BuildTree();

            foreach (var edge in mst.edges)
            {
                if (12.5f < UnityEngine.Random.Range(0.0f, 100.0f)) // 12.5 % 확률로 엣지 추가
                {
                    continue;
                }

                if (true == mst.connections.Contains(edge))
                {
                    continue;
                }

                mst.connections.Add(edge);
            }

            corridors = new List<CorridorPath>();
            foreach (var connection in mst.connections)
            {
                corridors.Add(new CorridorPath(connection.p1, connection.p2));

                Block block1 = connection.p1;
                Block block2 = connection.p2;
                block1.neighbors.Add(block2);
                block2.neighbors.Add(block1);
            }
        }

        public class DelaunayTriangulation
        {
            public class Point
            {
                public Vector3 position;
                public Block block;

                public Point(Vector3 position, Block block)
                {
                    this.position = position;
                    this.block = block;
                }
            }

            public class Edge
            {
                public Point v0;
                public Point v1;
                public float cost
                {
                    get
                    {
                        if (null == v0 || null == v1)
                        {
                            return 0.0f;
                        }

                        return Vector3.Distance(v0.position, v1.position);
                    }
                }

                public Edge(Point v0, Point v1)
                {
                    this.v0 = v0;
                    this.v1 = v1;
                }

                public override bool Equals(object other)
                {
                    if (false == (other is Edge))
                    {
                        return false;
                    }

                    return Equals((Edge)other);
                }

                public bool Equals(Edge edge)
                {
                    return ((this.v0.position.Equals(edge.v0.position) && this.v1.position.Equals(edge.v1.position)) || (this.v0.position.Equals(edge.v1.position) && this.v1.position.Equals(edge.v0.position)));
                }

                public override int GetHashCode()
                {
                    return v0.GetHashCode() ^ (v1.GetHashCode() << 2);
                }
            }

            public class Circle
            {
                public Vector3 center;
                public float radius;

                public Circle(Vector3 center, float radius)
                {
                    this.center = center;
                    this.radius = radius;
                }

                public bool Contains(Vector3 point)
                {
                    float d = Vector3.Distance(center, point);
                    if (radius < d)
                    {
                        return false;
                    }

                    return true;
                }
            }

            public class Triangle
            {
                public Vector3 a;
                public Vector3 b;
                public Vector3 c;
                public Circle circumCircle;
                public List<Edge> edges;

                public Triangle(Point p1, Point p2, Point p3)
                {
                    this.a = p1.position;
                    this.b = p2.position;
                    this.c = p3.position;

                    this.circumCircle = calcCircumCircle(a, b, c);
                    this.edges = new List<Edge>();
                    this.edges.Add(new Edge(p1, p2));
                    this.edges.Add(new Edge(p2, p3));
                    this.edges.Add(new Edge(p3, p1));
                }

                public override bool Equals(object other)
                {
                    if (false == (other is Triangle))
                    {
                        return false;
                    }

                    return Equals((Triangle)other);
                }

                public override int GetHashCode()
                {
                    return a.GetHashCode() ^ (b.GetHashCode() << 2) ^ (c.GetHashCode() >> 2);
                }

                public bool Equals(Triangle triangle)
                {
                    return this.a == triangle.a && this.b == triangle.b && this.c == triangle.c;
                }

                private Circle calcCircumCircle(Vector3 a, Vector3 b, Vector3 c)
                {
                    // 출처: 삼각형 외접원 구하기 - https://kukuta.tistory.com/444

                    if (a == b || b == c || c == a) // 같은 점이 있음. 삼각형 아님. 외접원 구할 수 없음.
                    {
                        return null;
                    }

                    float mab = (b.x - a.x) / (b.y - a.y) * -1.0f;  // 직선 ab에 수직이등분선의 기울기
                    float a1 = (b.x + a.x) / 2.0f;                  // 직선 ab의 x축 중심 좌표
                    float b1 = (b.y + a.y) / 2.0f;                  // 직선 ab의 y축 중심 좌표

                    // 직선 bc
                    float mbc = (b.x - c.x) / (b.y - c.y) * -1.0f;  // 직선 bc에 수직이등분선의 기울기
                    float a2 = (b.x + c.x) / 2.0f;                  // 직선 bc의 x축 중심 좌표
                    float b2 = (b.y + c.y) / 2.0f;                  // 직선 bc의 y축 중심 좌표

                    if (mab == mbc)     // 두 수직이등분선의 기울기가 같음. 평행함. 
                    {
                        return null;    // 교점 구할 수 없음
                    }

                    float x = (mab * a1 - mbc * a2 + b2 - b1) / (mab - mbc);
                    float y = mab * (x - a1) + b1;

                    if (b.x == a.x)     // 수직이등분선의 기울기가 0인 경우(수평선)
                    {
                        x = a2 + (b1 - b2) / mbc;
                        y = b1;
                    }

                    if (b.y == a.y)     // 수직이등분선의 기울기가 무한인 경우(수직선)
                    {
                        x = a1;
                        if (0.0f == mbc)
                        {
                            y = b2;
                        }
                        else
                        {
                            y = mbc * (a1 - a2) + b2;
                        }
                    }

                    if (b.x == c.x)     // 수직이등분선의 기울기가 0인 경우(수평선)
                    {
                        x = a1 + (b2 - b1) / mab;
                        y = b2;
                    }

                    if (b.y == c.y)     // 수직이등분선의 기울기가 무한인 경우(수직선)
                    {
                        x = a2;
                        if (0.0f == mab)
                        {
                            y = b1;
                        }
                        else
                        {
                            y = mab * (a2 - a1) + b1;
                        }
                    }

                    Vector3 center = new Vector3(x, y, 0.0f);
                    float radius = Vector3.Distance(center, a);

                    return new Circle(center, radius);
                }
            }

            private Triangle superTriangle = null;
            public List<Triangle> triangles = new List<Triangle>();

            public DelaunayTriangulation(List<Block> blocks)
            {
                superTriangle = CreateSuperTriangle(blocks);
                if (null == superTriangle)
                {
                    return;
                }

                triangles.Add(superTriangle);

                foreach (var block in blocks)
                {
                    AddPoint(block);
                }

                RemoveSuperTriangle();
            }

            public void AddPoint(Block block)
            {
                Vector3 point = block.rect.center;

                List<Triangle> badTriangles = new List<Triangle>();
                foreach (var triangle in triangles)
                {
                    if (true == triangle.circumCircle.Contains(point))
                    {
                        badTriangles.Add(triangle);
                    }
                }

                List<Edge> polygon = new List<Edge>();

                // first find all the triangles that are no longer valid due to the insertion
                foreach (var triangle in badTriangles)
                {
                    List<Edge> edges = triangle.edges;

                    foreach (Edge edge in edges)
                    {
                        // find unique edge
                        bool unique = true;
                        foreach (var other in badTriangles)
                        {
                            if (true == triangle.Equals(other))
                            {
                                continue;
                            }

                            foreach (var otherEdge in other.edges)
                            {
                                if (true == edge.Equals(otherEdge))
                                {
                                    unique = false;
                                    break;
                                }
                            }

                            if (false == unique)
                            {
                                break;
                            }
                        }

                        if (true == unique)
                        {
                            polygon.Add(edge);
                        }
                    }
                }

                foreach (var badTriangle in badTriangles)
                {
                    triangles.Remove(badTriangle);
                }

                foreach (Edge edge in polygon)
                {
                    Triangle triangle = CreateTriangle(edge.v0, edge.v1, new Point(point, block));
                    if (null == triangle)
                    {
                        continue;
                    }
                    triangles.Add(triangle);
                }
            }

            public void RemoveSuperTriangle()
            {
                if (null == superTriangle)
                {
                    return;
                }

                List<Triangle> remove = new List<Triangle>();
                foreach (var triangle in triangles)
                {
                    if (true == (triangle.a == superTriangle.a || triangle.a == superTriangle.b || triangle.a == superTriangle.c ||
                                 triangle.b == superTriangle.a || triangle.b == superTriangle.b || triangle.b == superTriangle.c ||
                                 triangle.c == superTriangle.a || triangle.c == superTriangle.b || triangle.c == superTriangle.c
                       )
                    )
                    {
                        remove.Add(triangle);
                    }
                }

                foreach (var triangle in remove)
                {
                    triangles.Remove(triangle);
                }
            }

            private Triangle CreateSuperTriangle(List<Block> blocks)
            {
                float minX = float.MaxValue;
                float maxX = float.MinValue;
                float minY = float.MaxValue;
                float maxY = float.MinValue;

                foreach (Block block in blocks)
                {
                    Vector3 point = block.rect.center;
                    minX = Mathf.Min(minX, point.x);
                    maxX = Mathf.Max(maxX, point.x);
                    minY = Mathf.Min(minY, point.y);
                    maxY = Mathf.Max(maxY, point.y);
                }

                float dx = maxX - minX;
                float dy = maxY - minY;

                // super triangle을 포인트 리스트 보다 크게 잡는 이유는
                // super triangle의 변과 포인트가 겹치게 되면 삼각형이 아닌 직선이 되므로 델로네 삼각분할을 적용할 수 없기 때문이다.
                Vector3 a = new Vector3(minX - dx, minY - dy);
                Vector3 b = new Vector3(minX - dx, maxY + dy * 3);
                Vector3 c = new Vector3(maxX + dx * 3, minY - dy);

                // super triangle이 직선인 경우 리턴
                if (a == b || b == c || c == a)
                {
                    return null;
                }

                return new Triangle(new Point(a, null), new Point(b, null), new Point(c, null));
            }

            private Triangle CreateTriangle(Point a, Point b, Point c)
            {
                if (a == b || b == c || c == a)
                {
                    return null;
                }

                return new Triangle(a, b, c);
            }
        }

        public class MinimumSpanningTree
        {
            public class Edge
            {
                public Edge(Block p1, Block p2, float cost)
                {
                    this.p1 = p1;
                    this.p2 = p2;
                    this.cost = cost;
                    this.path = new List<Tile>();
                }

                public Block p1;
                public Block p2;
                public float cost;
                public List<Tile> path;
            }

            private Dictionary<Block, Block> parents = new Dictionary<Block, Block>();
            public List<Edge> edges = new List<Edge>();
            public List<Edge> connections = new List<Edge>();

            public MinimumSpanningTree(List<Block> rooms)
            {
                foreach (Block room in rooms)
                {
                    parents.Add(room, room);
                }
            }

            public void AddEdge(Edge edge)
            {
                foreach (Edge other in edges)
                {
                    if (true == (edge.p1 == other.p1 && edge.p2 == other.p2) || (edge.p1 == other.p2 && edge.p2 == other.p1))
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
                    Block srcParent = FindParent(edge.p1);
                    Block destParent = FindParent(edge.p2);

                    if (srcParent != destParent)
                    {
                        connections.Add(edge);
                        Union(srcParent, destParent);
                    }
                }
            }

            private Block FindParent(Block room)
            {
                var parent = parents[room];
                if (parent != room)
                {
                    parents[room] = FindParent(parent);
                }
                return parents[room];
            }

            private void Union(Block src, Block dest)
            {
                Block srcParent = FindParent(src);
                Block destParent = FindParent(dest);
                parents[srcParent] = destParent;
            }
        }
    }
}