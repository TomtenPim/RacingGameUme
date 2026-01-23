using NUnit.Framework;
using System.Collections.Generic;
using Unity.Hierarchy;
using UnityEngine;
using static UnityEditor.FilePathAttribute;

[RequireComponent(typeof(BezierCurve))]
public class TrackMeshGeneration : ProceduralMesh
{
    [SerializeField, UnityEngine.Range(0.1f, 5.0f)]
    private float trackSubdivisionSpacing = 0.5f;

    [SerializeField, UnityEngine.Range(0.1f, 1000.0f)]
    private float trackWidth = 2.0f;

    [SerializeField, UnityEngine.Range(0.1f, 1000.0f)]
    private float trackHeight = 0.1f;

    private float Scale = 1;

    private void OnValidate()
    {
        var bezierCurve = GetComponent<BezierCurve>();
        Scale = trackSubdivisionSpacing * bezierCurve.Scale;
    }

    protected override Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        MeshCollider collider = GetComponent<MeshCollider>();

        mesh.hideFlags = HideFlags.DontSave;
        mesh.name = "Track";

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        GenerateTrack(vertices, triangles);

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();


        if (collider != null)
        {
            collider.sharedMesh = mesh;
        }
        return mesh;
    }

    protected void GenerateTrack(List<Vector3> inVertices, List<int> inTriangles)
    {
        BezierCurve bezierCurve = GetComponent<BezierCurve>();

        Debug.Log(bezierCurve.TotalDistance);

        for (float t = 0; t < bezierCurve.TotalDistance; t += (Scale))
        {
            Pose pose = bezierCurve.GetPose(t);
            AddLoops(pose, inVertices, inTriangles);
        }

        for (int i = 0; i < inVertices.Count - 4; i += 4)
        {
            inTriangles.AddRange(new int[]
            {
                // top face
                i + 7, i + 6, i + 2,
                i + 7, i + 2, i + 3,
                // bottom face
                i + 1, i + 5, i + 4,
                i + 1, i + 4, i + 0,
                // left face
                i + 7, i + 3, i + 0,
                i + 0, i + 4, i + 7,
                // right face
                i + 1, i + 2, i + 6,
                i + 1, i + 6, i + 5
            });
        }

        // Close the loop
        inTriangles.AddRange(new int[]
        {
            // top face
            2, inVertices.Count - 2, inVertices.Count - 1,
            3, 2, inVertices.Count - 1,
            // bottom face
            inVertices.Count - 4, inVertices.Count - 3, 1,
            0, inVertices.Count - 4, 1,
            // left face
            0, 3, inVertices.Count - 1,
            inVertices.Count - 1, inVertices.Count - 4, 0,
            // right face
            inVertices.Count - 2, 2, 1,
            inVertices.Count - 3, inVertices.Count - 2, 1
        });
    }

    protected void AddLoops(Pose inPose, List<Vector3> inVertices, List<int> inTriangles)
    {
        int start = inVertices.Count;

        Vector3 right = inPose.right * (trackWidth * 0.5f);
        Vector3 up = inPose.up * trackHeight;
        Vector3 forward = inPose.forward * (trackWidth * 0.5f);

        inVertices.AddRange(new Vector3[]
        {
            inPose.position - right,
            inPose.position + right,
            inPose.position + right + up,
            inPose.position - right + up
        });
    }
}