using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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
        private int currentLap;
        public bool CompletedRace = false;
        public float FinishingTime = 0;
        public RaceState RaceState = RaceState.FirstCrossOver;
        public bool IsAbleToFinishLap = true;

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
            amountOfCarsInRace = inAmountOfCars;
        }
    }

    public static RaceManager Instance;
    private Dictionary<CarController, CarRaceState> carsInRace = new();
    [SerializeField] private RaceData raceData = new(3, 2);
    public RaceData GetRaceData => raceData;
    [SerializeField] private GameObject aiCarPrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject CheckeredLine;
    [SerializeField] private GameObject HalfPointLine;
    [SerializeField] float carSpawnOffset = 4;
    [SerializeField] float playerRaceSpawnPosition = 5;
    string[] randomNames =
    {
        "Lloyd Harris", "Cornell Rivera", "Stacie Moon",
        "Penelope York", "Millard Juarez", "Alejandra Bonilla",
        "Jennie Howe", "Milton Gordon", "Alissa Avila",
        "Rodrigo Wilcox", "Jaime Vance", "Conrad Contreras",
        "Douglass Larson", "Vincenzo Dyer", "Domingo Frank",
        "Irving Garza", "Frankie Mccarty", "Shannon Krause",
        "Emmanuel Beard", "Keith Johnston", "Norris Frederick",
    };


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
        // Spawn cars
        Pose startCarSpawnPose = BezierCurve.Instance.GetPose(0);

        Vector3 backDirection = startCarSpawnPose.position - (startCarSpawnPose.position - (startCarSpawnPose.forward * carSpawnOffset));
        Vector3 sideDirection = startCarSpawnPose.position - (startCarSpawnPose.position - (startCarSpawnPose.right * carSpawnOffset));

        int spawnSide = 1;
        for (int i = 0; i < raceData.AmountOfCarsInRace; i++)
        {
            GameObject car = null;
            Vector3 spawnPoint = new Vector3(startCarSpawnPose.position.x, 1, startCarSpawnPose.position.z) - ((backDirection * i) - (sideDirection * spawnSide));
            spawnSide *= -1;

            if (i == playerRaceSpawnPosition)
            {
                car = Instantiate(playerPrefab, Vector3.zero, startCarSpawnPose.rotation);
            }
            else
            {
                car = Instantiate(aiCarPrefab, Vector3.zero, startCarSpawnPose.rotation);
            }
            car.transform.Find("Car").transform.position = spawnPoint;

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

        while (true)
        {
            bool done = true;
            foreach (var car in carsInRace)
            {
                try
                {
                    if (car.Key is not null)
                        car.Key.enabled = false;
                }
                catch (System.Exception)
                {
                    done = false;
                    throw;
                }
            }
            if (done) break;
        }

        // Spawn Checkered line
        Pose linePose = BezierCurve.Instance.GetPose(10);
        GameObject lineObject = Instantiate(CheckeredLine, linePose.position - new Vector3(0, 1, 0), linePose.rotation);
        lineObject.transform.localScale = Vector3.one * (BezierCurve.Instance.Scale / 3);

        // Half Point Line
        linePose = BezierCurve.Instance.GetPose((BezierCurve.Instance.TotalDistance / 2));
        lineObject = Instantiate(HalfPointLine, linePose.position - new Vector3(0, 1, 0), linePose.rotation);
        lineObject.transform.localScale = Vector3.one * (BezierCurve.Instance.Scale / 3);

        UIManager.Instance.StartCountdown(3);
    }

    public void StartRace()
    {
        foreach (var car in carsInRace)
        {
            car.Key.enabled = true;
        }

        StartCoroutine(RaceUpdate());
    }

    private IEnumerator RaceUpdate()
    {
        int updatePosition = 0;
        while (!isRaceCompleted)
        {
            raceData.RaceTime += Time.deltaTime;
            UIManager.Instance.SetTimer(raceData.RaceTime);


            // Get all cars position on the map spline 

            if (updatePosition % 60 != 0)
            {
                updatePosition++;
                goto WaitForFrame;
            }
            (CarRaceState raceState, float pointInTrack)[] positionData = new (CarRaceState raceState, float pointInTrack)[carsInRace.Count];
            int index = 0;
            foreach (var car in carsInRace)
            {
                if (car.Key.CarTransform != null)
                {
                    float blendValue = 0;
                    BezierCurve.Instance.GetClosestPoseFromLocation(car.Key.CarTransform.position, ref blendValue);
                    positionData[index] = new(car.Value, blendValue);
                    index++;
                }
            }

            HashSet<CarRaceState> addedPoint = new();
            (CarRaceState raceState, float pointInTrack) bestPoint = new(new CarRaceState(-1), -1);

            // Set the cars position in the race, based on position on the map spline 
            for (int i = 0; i < positionData.Length; i++)
            {
                for (int j = 0; j < positionData.Length; j++)
                {
                    if (!addedPoint.Contains(positionData[j].raceState))
                    {
                        if (positionData[j].raceState.CurrentLap < bestPoint.raceState.CurrentLap)
                        {
                            continue;
                        }
                        if (positionData[j].raceState.CurrentLap > bestPoint.raceState.CurrentLap || positionData[j].pointInTrack > bestPoint.pointInTrack)
                        {
                            bestPoint = positionData[j];
                        }
                    }
                }
                bestPoint.raceState.RacePosition = (i + 1);
                addedPoint.Add(bestPoint.raceState);
                bestPoint = new(new CarRaceState(-1), -1);
            }

        WaitForFrame:
            yield return new WaitForEndOfFrame();
        }
    }

    private void PlayerPositionChange(int position)
    {
        UIManager.Instance.SetRacePosition(position);
    }

    private void PlayerLapChange(int lap)
    {
        UIManager.Instance.UpdateLapText(lap);
    }

    public void CarPassedHalfway(CarController inCarController)
    {
        if (!carsInRace.ContainsKey(inCarController))
        {
            Debug.LogError($"{inCarController.gameObject} CarController is not in the carControllers dictionary, somethings when wrong");
            return;
        }

        CarRaceState carRaceState = carsInRace[inCarController];
        carRaceState.IsAbleToFinishLap = true;
    }

    public void CompletedLap(CarController inCarController)
    {
        if (!carsInRace.ContainsKey(inCarController))
        {
            Debug.LogError($"{inCarController.gameObject} CarController is not in the carControllers dictionary, somethings when wrong");
            return;
        }

        CarRaceState carRaceState = carsInRace[inCarController];
        if (!carRaceState.IsAbleToFinishLap)
        {
            return;
        }

        carRaceState.IsAbleToFinishLap = false;
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

        string carName = carRaceState.IsPlayer ? "The Player" : randomNames[UnityEngine.Random.Range(0, randomNames.Length)];

        UIManager.Instance.AddToLeaderBoard(carRaceState.RacePosition, carName, carRaceState.FinishingTime);

        if (carRaceState.IsPlayer)
        {
            UIManager.Instance.ShowEndScreen();
        }

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
