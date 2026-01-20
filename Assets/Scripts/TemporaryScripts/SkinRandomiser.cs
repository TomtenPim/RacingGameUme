using UnityEngine;
using UnityEditor;
public class SkinRandomiser : MonoBehaviour
{

    public GameObject car;
    public Texture[] textures;

    public Color color;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void randomiseSkin()
    {
        int materialIndex = Random.Range(0, textures.Length);
        car.transform.Find("beetle_chassis").GetComponent<MeshRenderer>().material.mainTexture = textures[materialIndex];
    }

    private void randomiseColor()
    {
        color = new Color(Random.Range(0f,1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        int children = car.transform.childCount;
        for(int i = 0; i < children; i++)
        {
            car.transform.GetChild(i).GetComponent<MeshRenderer>().material.color = color;
        }
    }

    private void applyColor()
    {
        int children = car.transform.childCount;
        for (int i = 0; i < children; i++)
        {
            car.transform.GetChild(i).GetComponent<MeshRenderer>().material.color = color;
        }
    }

    [CustomEditor(typeof(SkinRandomiser))]
    public class customButton : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SkinRandomiser skinRandomiser = (SkinRandomiser)target;
            if (GUILayout.Button("Apply Color"))
            {
                skinRandomiser.applyColor();
            }

            if (GUILayout.Button("Randomise Color"))
            {
                skinRandomiser.randomiseColor();
            }
            if (GUILayout.Button("Randomise Skin"))
            {
                skinRandomiser.randomiseSkin();
            }

        }

    }
}
