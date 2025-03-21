using static DungeonGizmo;
using System.Collections.Generic;

public class MinimumSpanningTree
{
	public class Edge
	{
		public Edge(Room p1, Room p2, float cost)
		{
			this.room1 = p1;
			this.room2 = p2;
			this.cost = cost;
		}

		public Room room1;
		public Room room2;
		public float cost;
	}

	private Dictionary<Room, Room> parents = new Dictionary<Room, Room>();
	public List<Edge> edges = new List<Edge>();
	public List<Edge> connections = new List<Edge>();

	public MinimumSpanningTree(List<Room> rooms)
	{
		foreach (Room room in rooms)
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
			Room srcParent = FindParent(edge.room1);
			Room destParent = FindParent(edge.room2);

			if (srcParent != destParent)
			{
				connections.Add(edge);
				Union(srcParent, destParent);
			}
		}
	}

	private Room FindParent(Room room)
	{
		var parent = parents[room];
		if (parent != room)
		{
			parents[room] = FindParent(parent);
		}
		return parents[room];
	}

	private void Union(Room src, Room dest)
	{
		Room srcParent = FindParent(src);
		Room destParent = FindParent(dest);
		parents[srcParent] = destParent;
	}
}