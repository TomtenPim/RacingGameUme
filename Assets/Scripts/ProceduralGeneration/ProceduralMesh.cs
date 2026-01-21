using UnityEngine;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[ExecuteInEditMode]
public abstract class ProceduralMesh : MonoBehaviour
{
    private Mesh trackMesh;

    public Mesh TrackMesh => trackMesh;

    protected virtual void Start()
    {
        CreateTrackMesh();
    }

    protected virtual void OnDestroy()
    {
        CleanUp();
    }
    
    void CleanUp()
    {
        if (trackMesh != null)
        {
            if(Application.isPlaying)
            {
                Destroy(trackMesh);
            }
            else
            {
                DestroyImmediate(trackMesh);
            }
        }
    }

    protected abstract Mesh CreateTrackMesh();

    public virtual void UpdateMesh()
    {
        trackMesh = CreateTrackMesh();
        GetComponent<MeshFilter>().mesh = trackMesh;
    }
}
