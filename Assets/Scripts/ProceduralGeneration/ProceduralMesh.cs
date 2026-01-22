using UnityEngine;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[ExecuteInEditMode]
public abstract class ProceduralMesh : MonoBehaviour
{
    private Mesh proceduralMesh;

    public Mesh Mesh => proceduralMesh;

    protected virtual void Start()
    {
        UpdateMesh();
    }

    protected virtual void OnDestroy()
    {
        CleanUp();
    }
    
    void CleanUp()
    {
        if (proceduralMesh != null)
        {
            if(Application.isPlaying)
            {
                Destroy(proceduralMesh);
            }
            else
            {
                DestroyImmediate(proceduralMesh);
            }
        }
    }

    protected abstract Mesh CreateMesh();

    public virtual void UpdateMesh()
    {
        proceduralMesh = CreateMesh();
        GetComponent<MeshFilter>().mesh = proceduralMesh;
    }
}
