using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{

    public RawImage SpeedometerArrow;
    public TextMeshProUGUI TimeUI;
    public TextMeshProUGUI RacePositionUI;


    [Range(0, 1)]
    public float SpeedometerValue;
    public float minDegrees;
    public float maxDegrees;

    // might be moved to raceManager
    private float Timer = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // might be moved to raceManager
        Timer += Time.deltaTime;
        SetTimer(Timer);
        SetRacePosition(1 + (int)Timer % 12);

        SpeedometerArrow.transform.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(minDegrees, maxDegrees, SpeedometerValue));

    }

    public void SetTimer(float inTimer)
    {
        TimeSpan time = TimeSpan.FromSeconds((double)inTimer);
        TimeUI.text = time.ToString(@"mm\:ss\:fff");
    }

    public void SetRacePosition(int pos)
    {
        RacePositionUI.text = "#"+pos.ToString();
    }

}
