using UnityEngine;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[ExecuteInEditMode]
public abstract class ProceduralMesh : MonoBehaviour
{
    private Mesh trackMesh;

    public Mesh TrackMesh => trackMesh;

    protected virtual void Start()
    {
        CreateMesh();
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

    protected abstract Mesh CreateMesh();

    public virtual void UpdateMesh()
    {
        trackMesh = CreateMesh();
        GetComponent<MeshFilter>().mesh = trackMesh;
    }
}
