using System.Collections.Generic;
using UnityEngine;

public class CoordinatePlane : MonoBehaviour
{
    public int width;
    public int height;

    void Start()
    {
        var mesh = new Mesh();
        var vertices = new List<Vector3>();
        var indices = new List<int>();

        // ºº∑Œ ¡Ÿ
        for (int x = 0; x <= width; x++)
        {
            vertices.Add(new Vector3(x, 0, 0));
            vertices.Add(new Vector3(x, height, 0));

            indices.Add(2 * x + 0);
            indices.Add(2 * x + 1);
        }

        for (int y = 0; y <= height; y++)
        {
            vertices.Add(new Vector3(0, y, 0));
            vertices.Add(new Vector3(width, y, 0));

            indices.Add(2 * y + 0 + (width + 1) * 2);
            indices.Add(2 * y + 1 + (width + 1) * 2);
        }

        MeshFilter filter = gameObject.AddComponent<MeshFilter>();
        mesh.vertices = vertices.ToArray();
        mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
        filter.mesh = mesh;

        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
        meshRenderer.material.color = Color.gray;
        meshRenderer.sortingOrder = 0;

        var boxCollider = gameObject.AddComponent<BoxCollider>();
        transform.position = new Vector3(-width/2, -height/2, width);
    }
}
