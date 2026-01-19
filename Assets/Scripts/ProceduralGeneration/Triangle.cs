using UnityEngine;

public class Triangle : ProceduralTrack
{
    protected override Mesh CreateTrackMesh()
    {
        Mesh mesh = new Mesh();
        mesh.hideFlags = HideFlags.DontSave;
        mesh.name = "Triangle";

        mesh.vertices = new Vector3[] 
        { 
            new Vector3(-0.5f, -0.5f, 0.0f),
            new Vector3(0.0f, 1.0f, 0.0f),
            new Vector3(0.5f, -0.5f, 0.0f)
        };

        mesh.colors = new Color[]
        {
            Color.red,
            Color.green,
            Color.blue
        };

        mesh.triangles = new int[]
        {
            0, 1, 2
        };

        mesh.RecalculateNormals();

        return mesh;
    }
}
