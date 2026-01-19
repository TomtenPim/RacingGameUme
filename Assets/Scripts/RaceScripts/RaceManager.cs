using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class RaceManager : MonoBehaviour
{
    public enum RaceState
    {
        FirstCrossOver,
        NormalLap,
        FinalLap
    }

    public static RaceManager Instance;
    private List<CharacterController> characterControllers = new();
    [SerializeField] private int maxLaps = 2;
    private int currentLap = 1;
    private RaceState raceState = RaceState.FirstCrossOver;


    private void Awake()
    {
        if (RaceManager.Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void CompletedLap()
    {
        switch (raceState)
        {
            case RaceState.FirstCrossOver:
                raceState = RaceState.NormalLap;
                break;
            case RaceState.NormalLap:
                currentLap++;
                break;
            case RaceState.FinalLap:
                RaceCompleted();
                return;
        }
        Debug.Log($"Current lap: {currentLap}");
        if (currentLap >= maxLaps)
        {
            raceState = RaceState.FinalLap;
        }
    }

    private void RaceCompleted()
    {
        Debug.Log("Race was completed");
    }

}
