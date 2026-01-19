using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{

    public RawImage SpeedometerArrow;

    [Range(0, 1)]
    public float SpeedometerValue;
    public float minDegrees;
    public float maxDegrees;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        SpeedometerArrow.transform.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(minDegrees, maxDegrees, SpeedometerValue));

    }
}
