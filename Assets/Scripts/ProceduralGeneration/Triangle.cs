using UnityEngine;

public class Triangle : ProceduralMesh
{
    protected override Mesh CreateMesh()
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

        mesh.uv = new Vector2[]
        {
            new Vector2(0.0f, 0.0f),
            new Vector2(0.5f, 1.0f),
            new Vector2(1.0f, 0.0f)
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}
