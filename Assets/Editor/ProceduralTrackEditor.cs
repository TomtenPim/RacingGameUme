using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProceduralTrack), true)]
public class ProceduralTrackEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(20);

        
        if (GUILayout.Button("Update Track Mesh"))
        {
            ProceduralTrack pm = target as ProceduralTrack;
            pm.UpdateTrackMesh();
        }
    }

}
