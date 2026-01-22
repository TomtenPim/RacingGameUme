using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class HardCube : ProceduralMesh
{
    public int cubeCount = 1000;
    public int cubeRange = 1000;

    private static readonly Vector3Int[] directions = new Vector3Int[]
    {
        Vector3Int.left, Vector3Int.right,
        Vector3Int.up, Vector3Int.down,
        Vector3Int.forward, Vector3Int.back
    };

    private static readonly Vector3Int[] upDirections = new Vector3Int[]
    {
        Vector3Int.up, Vector3Int.up,
        Vector3Int.forward, Vector3Int.forward,
        Vector3Int.up, Vector3Int.up
    };


    protected override Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.hideFlags = HideFlags.DontSave;
        mesh.name = "HardCube";

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        float f1 = 0.5f;
        float f2 = cubeRange;

        List<Vector3> cubePositions = new List<Vector3>();

        for(int x = 0; x <= cubeCount; x++)
        {
            cubePositions.Add(new Vector3(Random.Range(f1, f2), Random.Range(f1, f2), Random.Range(f1, f2)));
        }


        foreach (Vector3 cubePosition in cubePositions)
        {
            for (int i = 0; i < directions.Length; i++)
            {
                AddQuad(vertices, triangles, cubePosition, directions[i], upDirections[i]);
            }
        }
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    protected void AddQuad(List<Vector3> vertices, List<int> triangles, Vector3 cubePosition, Vector3Int forward, Vector3Int up)
    {
        Vector3 right = Vector3.Cross(forward, up).normalized;
        int vertexStartIndex = vertices.Count;

        vertices.AddRange(new Vector3[]
        {
            (cubePosition + -right - up + forward),
            (cubePosition +  right - up + forward),
            (cubePosition +  right + up + forward),
            (cubePosition + -right + up + forward)
        });

        triangles.AddRange(new int[]
        {
            vertexStartIndex + 0, vertexStartIndex + 2, vertexStartIndex + 1,
            vertexStartIndex + 0, vertexStartIndex + 3, vertexStartIndex + 2
        });
    }
}
