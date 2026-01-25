using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProceduralMesh), true)]
public class ProceduralMeshEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(20);

        
        if (GUILayout.Button("Update Track Mesh"))
        {
            ProceduralMesh pm = target as ProceduralMesh;

            pm.UpdateMesh();
        }
    }

}
