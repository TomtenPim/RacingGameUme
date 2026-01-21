using UnityEngine;

public class Cube : ProceduralMesh
{
    protected override Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.hideFlags = HideFlags.DontSave;
        mesh.name = "Cube";

        mesh.vertices = new Vector3[]
        {
            new Vector3(-1, -1, -1),
            new Vector3(-1, -1, 1),
            new Vector3(1, -1, 1),
            new Vector3(1, -1, -1),
            new Vector3(-1, 1, -1),
            new Vector3(-1, 1, 1),
            new Vector3(1, 1, 1),
            new Vector3(1, 1, -1) 
        };


        mesh.colors = new Color[]
        {
            Color.green, Color.green, Color.green, Color.green, Color.red, Color.red, Color.red, Color.red
        };

        mesh.triangles = new int[]
        {
            0, 3, 1, 3, 2, 1,
            0, 4, 3, 3, 4, 7,
            0, 1, 4, 1, 5, 4,
            2, 5, 1, 2, 6, 5,
            3, 7, 2, 2, 7, 6,
            4, 5, 7, 7, 5, 6
        };

        mesh.normals = System.Array.ConvertAll(mesh.vertices, v => v.normalized);

        mesh.uv = System.Array.ConvertAll(mesh.vertices, v => new Vector2((v.x + 1.0f) * 0.5f,
                                                                             (v.y + 1.0f) * 0.5f));
        mesh.RecalculateBounds();

        return mesh;
    }
}
