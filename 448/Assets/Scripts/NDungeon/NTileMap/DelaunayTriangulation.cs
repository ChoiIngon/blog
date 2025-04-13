using System.Collections.Generic;
using UnityEngine;

namespace NDungeon.NTileMap
{
    public class DelaunayTriangulation
    {
        public class Point
        {
            public TileMap.Room room;

            public Point(TileMap.Room room)
            {
                this.room = room;
            }

            public Vector2 position
            {
                get { return this.room.rect.center; }
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

                    return Vector2.Distance(v0.position, v1.position);
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
            public Circle innerCircle;
            public List<Edge> edges;

            public Triangle(Point p1, Point p2, Point p3)
            {
                this.a = p1.position;
                this.b = p2.position;
                this.c = p3.position;

                this.circumCircle = calcCircumCircle();
                this.innerCircle = calcInnerCircle();
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

            private Circle calcCircumCircle()
            {
                // ��ó: �ﰢ�� ������ ���ϱ� - https://kukuta.tistory.com/444

                if (a == b || b == c || c == a) // ���� ���� ����. �ﰢ�� �ƴ�. ������ ���� �� ����.
                {
                    return null;
                }

                float mab = (b.x - a.x) / (b.y - a.y) * -1.0f;  // ���� ab�� �����̵�м��� ����
                float a1 = (b.x + a.x) / 2.0f;                  // ���� ab�� x�� �߽� ��ǥ
                float b1 = (b.y + a.y) / 2.0f;                  // ���� ab�� y�� �߽� ��ǥ

                // ���� bc
                float mbc = (b.x - c.x) / (b.y - c.y) * -1.0f;  // ���� bc�� �����̵�м��� ����
                float a2 = (b.x + c.x) / 2.0f;                  // ���� bc�� x�� �߽� ��ǥ
                float b2 = (b.y + c.y) / 2.0f;                  // ���� bc�� y�� �߽� ��ǥ

                if (mab == mbc)     // �� �����̵�м��� ���Ⱑ ����. ������. 
                {
                    return null;    // ���� ���� �� ����
                }

                float x = (mab * a1 - mbc * a2 + b2 - b1) / (mab - mbc);
                float y = mab * (x - a1) + b1;

                if (b.x == a.x)     // �����̵�м��� ���Ⱑ 0�� ���(����)
                {
                    x = a2 + (b1 - b2) / mbc;
                    y = b1;
                }

                if (b.y == a.y)     // �����̵�м��� ���Ⱑ ������ ���(������)
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

                if (b.x == c.x)     // �����̵�м��� ���Ⱑ 0�� ���(����)
                {
                    x = a1 + (b2 - b1) / mab;
                    y = b2;
                }

                if (b.y == c.y)     // �����̵�м��� ���Ⱑ ������ ���(������)
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

            private Circle calcInnerCircle()
            {
                float e1 = Mathf.Sqrt((this.b.x - this.c.x) * (this.b.x - this.c.x) + (this.b.y - this.c.y) * (this.b.y - this.c.y));
                float e2 = Mathf.Sqrt((this.c.x - this.a.x) * (this.c.x - this.a.x) + (this.c.y - this.a.y) * (this.c.y - this.a.y));
                float e3 = Mathf.Sqrt((this.a.x - this.b.x) * (this.a.x - this.b.x) + (this.a.y - this.b.y) * (this.a.y - this.b.y));

                float x = (e1 * a.x + e2 * b.x + e3 * c.x) / (e1 + e2 + e3);
                float y = (e1 * a.y + e2 * b.y + e3 * c.y) / (e1 + e2 + e3);

                Vector3 center = new Vector3(x, y, 0.0f);
                float semiperimeter = (e1 + e2 + e3) / 2;
                float area = Mathf.Sqrt(semiperimeter * (semiperimeter - e1) * (semiperimeter - e2) * (semiperimeter - e3));
                float radius = area / semiperimeter;
                return new Circle(center, radius);
            }
        }

        private Triangle superTriangle = null;
        public List<Triangle> triangles = new List<Triangle>();

        public DelaunayTriangulation(List<TileMap.Room> rooms)
        {
            superTriangle = CreateSuperTriangle(rooms);
            if (null == superTriangle)
            {
                return;
            }

            triangles.Add(superTriangle);

            foreach (var room in rooms)
            {
                AddPoint(room);
            }

            RemoveSuperTriangle();
        }

        public void AddPoint(TileMap.Room room)
        {
            Vector3 point = room.rect.center;

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
                Triangle triangle = CreateTriangle(edge.v0, edge.v1, new Point(room));
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

        private Triangle CreateSuperTriangle(List<TileMap.Room> rooms)
        {
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            foreach (TileMap.Room room in rooms)
            {
                Vector2 point = room.rect.center;
                minX = Mathf.Min(minX, point.x);
                maxX = Mathf.Max(maxX, point.x);
                minY = Mathf.Min(minY, point.y);
                maxY = Mathf.Max(maxY, point.y);
            }

            float dx = maxX - minX;
            float dy = maxY - minY;

            // super triangle�� ����Ʈ ����Ʈ ���� ũ�� ��� ������
            // super triangle�� ���� ����Ʈ�� ��ġ�� �Ǹ� �ﰢ���� �ƴ� ������ �ǹǷ� ���γ� �ﰢ������ ������ �� ���� �����̴�.
            TileMap.Room a = new TileMap.Room(0, minX - dx, minY - dy, 0, 0);
            TileMap.Room b = new TileMap.Room(0, minX - dx, maxY + dy * 3, 0, 0);
            TileMap.Room c = new TileMap.Room(0, maxX + dx * 3, minY - dy, 0, 0);

            // super triangle�� ������ ��� ����
            if (a == b || b == c || c == a)
            {
                return null;
            }

            return new Triangle(new Point(a), new Point(b), new Point(c));
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
}