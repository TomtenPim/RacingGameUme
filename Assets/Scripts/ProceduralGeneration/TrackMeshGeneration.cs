using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BezierCurve))]
public class TrackMeshGeneration : ProceduralMesh
{
    [SerializeField, UnityEngine.Range(0.1f, 5.0f)]
    private float trackSubdivisionSpacing = 0.5f;

    [SerializeField, UnityEngine.Range(1.0f, 5.0f)]
    private float trackWidth = 2.0f;

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
        mesh.name = "Track";

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        GenerateTrack(vertices, triangles);

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    protected void GenerateTrack(List<Vector3> inVertices, List<int> inTriangles)
    {
        BezierCurve bezierCurve = GetComponent<BezierCurve>();

        for(float t = 0; t <= bezierCurve.TotalDistance; t += trackSubdivisionSpacing)
        {
            Pose pose = bezierCurve.GetPose(t);
            AddSection(pose, inVertices, inTriangles);
        }


    }

    protected void AddSection(Pose inPose, List<Vector3> inVertices, List<int> inTriangles)
    {
        int start = inVertices.Count;

        Vector3 right = inPose.right * (trackWidth * 0.5f);
        Vector3 up = inPose.up * (trackWidth * 0.5f);
        Vector3 forward = inPose.forward * (trackWidth * 0.5f);
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
