using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class RaceManager : MonoBehaviour
{
    public enum RaceState
    {
        FirstCrossOver,
        NormalLap,
        FinalLap,
        RaceCompleted
    }

    public class CarRaceState
    {
        private int carId;
        public bool IsPlayer;
        private int racePosition;
        public int currentLap;
        public bool CompletedRace;
        public float FinishingTime;
        public RaceState RaceState;

        public Action<int> OnPositionChange;
        public Action<int> OnLapChange;

        public int CarId => carId;
        public int RacePosition
        {
            get
            {
                return racePosition;
            }
            set
            {
                racePosition = value;
                OnPositionChange?.Invoke(racePosition);
            }
        }

        public int CurrentLap
        {
            get
            {
                return currentLap;
            }
            set
            {
                currentLap = value;
                OnLapChange?.Invoke(currentLap);
            }
        }



        public CarRaceState(int inCarId, bool inIsPlayer = false, int inRacePosition = 0, int inCurrentLap = 1)
        {
            carId = inCarId;
            IsPlayer = inIsPlayer;
            racePosition = inRacePosition;
            currentLap = inCurrentLap;
            CompletedRace = false;
            FinishingTime = 0;
            RaceState = RaceState.FirstCrossOver;
        }
    }

    [Serializable]
    public class RaceData
    {
        [SerializeField] private int maxLaps;
        [NonSerialized] public float RaceTime;
        [SerializeField] private int amountOfCarsInRace;
        public int MaxLaps => maxLaps;
        public int AmountOfCarsInRace => amountOfCarsInRace;

        public RaceData(int inMaxLaps = 3, int inAmountOfCars = 5)
        {
            maxLaps = inMaxLaps;
            RaceTime = 0;
            amountOfCarsInRace = inAmountOfCars;
        }
    }

    public static RaceManager Instance;
    private Dictionary<CarController, CarRaceState> carsInRace = new();
    [SerializeField] private RaceData raceData = new(3, 2);
    public RaceData GetRaceData => raceData;
    [SerializeField] private GameObject aiCarPrefab;
    [SerializeField] private GameObject playerPrefab;
    private bool isRaceCompleted = false;


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

    private void Start()
    {
        if (UIManager.Instance == null)
        {
            Debug.LogError("UIManager, is not in scene or not instanced, functions may fail");
        }

        InitRace();
    }

    private void InitRace()
    {
        for (int i = 0; i < raceData.AmountOfCarsInRace; i++)
        {
            GameObject car = null;
            Vector3 spawnPoint = new(i * 2, 1, i * 2);
            if (i == 0)
            {
                car = Instantiate(playerPrefab, spawnPoint, Quaternion.identity);
            }
            else
            {
                car = Instantiate(aiCarPrefab, spawnPoint, Quaternion.identity);
            }

            if (!car.GetComponent<CarController>())
            {
                Debug.LogError($"{aiCarPrefab} dose not have a car controller");
                return;
            }
            car.name = car.name + ": " + (i + 1);

            CarController carController = car.GetComponent<CarController>();
            CarRaceState carRaceState = new CarRaceState((i + 1), false);
            carRaceState.IsPlayer = carController is PlayerController ? true : false;

            if (carRaceState.IsPlayer)
            {
                carRaceState.OnPositionChange += PlayerPositionChange;
                carRaceState.OnLapChange += PlayerLapChange;
            }

            carsInRace.Add(carController, carRaceState);
        }
        StartRace();
    }

    private void StartRace()
    {
        StartCoroutine(RaceUpdate());
    }

    private IEnumerator RaceUpdate()
    {

        while (!isRaceCompleted)
        {
            raceData.RaceTime += Time.deltaTime;
            UIManager.Instance.SetTimer(raceData.RaceTime);

            float totalCurveDistance = BezierCurve.Instance.TotalDistance;
            (CarRaceState raceState, float pointInTrack)[] positionData = new (CarRaceState raceState, float pointInTrack)[carsInRace.Count];
            int index = 0;
            foreach (var car in carsInRace)
            {
                try
                {
                    if (car.Key.CarTransform != null)
                    {
                        float blendValue = 0;
                        BezierCurve.Instance.GetClosestPoseFromLocation(car.Key.CarTransform.position, ref blendValue);
                        positionData[index] = new(car.Value, blendValue);
                        index++;
                    }
                }
                catch (System.Exception)
                {
                    goto NextFrame;
                    throw;
                }
            }

            HashSet<CarRaceState> addedPoint = new();
            (CarRaceState raceState, float pointInTrack) bestPoint = new(null, -1);


            for (int i = 0; i < positionData.Length; i++)
            {
                for (int j = 0; j < positionData.Length; j++)
                {
                    if (!addedPoint.Contains(positionData[j].raceState))
                    {
                        if (positionData[j].pointInTrack > bestPoint.pointInTrack)
                        {
                            bestPoint = positionData[j];
                        }
                    }
                }
                bestPoint.raceState.RacePosition = (i + 1);
                addedPoint.Add(bestPoint.raceState);
            }

        NextFrame:
            yield return new WaitForEndOfFrame();
        }
    }

    private void PlayerPositionChange(int position)
    {
        UIManager.Instance.SetRacePosition(position);
    }

    private void PlayerLapChange(int lap)
    {
        //call UIManager
    }

    public void CompletedLap(CarController inCarController)
    {
        if (!carsInRace.ContainsKey(inCarController))
        {
            Debug.LogError($"{inCarController.gameObject} CarController is not in the carControllers dictionary, somethings when wrong");
            return;
        }

        CarRaceState carRaceState = carsInRace[inCarController];

        switch (carRaceState.RaceState)
        {
            case RaceState.FirstCrossOver:
                carRaceState.RaceState = RaceState.NormalLap;
                break;

            case RaceState.NormalLap:

                carRaceState.CurrentLap++;
                if (carRaceState.CurrentLap >= raceData.MaxLaps)
                {
                    carRaceState.RaceState = RaceState.FinalLap;
                }
                Debug.Log(carRaceState.CurrentLap);

                break;

            case RaceState.FinalLap:
                CarCompletedRace(inCarController);
                return;
        }


    }

    private void CarCompletedRace(CarController inCarController)
    {
        CarRaceState carRaceState = carsInRace[inCarController];

        carRaceState.FinishingTime = raceData.RaceTime;
        carRaceState.CompletedRace = true;
        carRaceState.RaceState = RaceState.RaceCompleted;

        UIManager.Instance.AddToLeaderBoard(carRaceState.RacePosition, "" + carRaceState.CarId, carRaceState.FinishingTime);

        if (carRaceState.IsPlayer)
        {
            Debug.LogWarning("THIS IS PLAYER :]");
            UIManager.Instance.ShowEndScreen();
        }

        Debug.Log($"Car has completed race, final time: {raceData.RaceTime}");

        CheckIsRaceCompleted();
    }

    private void CheckIsRaceCompleted()
    {
        // check if all cars has finished the race  
        foreach (var carState in carsInRace)
        {
            if (!carState.Value.CompletedRace)
            {
                return;
            }
        }

        isRaceCompleted = true;
    }
}
