using System;
using TMPro;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;


    public RawImage SpeedometerArrow;
    public TextMeshProUGUI TimeUI;
    public TextMeshProUGUI RacePositionUI;


    [Range(0, 1)]
    public float SpeedometerValue;
    public float minDegrees;
    public float maxDegrees;

    public GameObject EndScreen;
    public TextMeshProUGUI PosEndUI;
    public TextMeshProUGUI NameEndUI;
    public TextMeshProUGUI TimeEndUI;
    public Button PlayAgainButton;
    public Button MainMenuButton;

    // might be moved to raceManager
    private float Timer = 0;

    private void Awake()
    {
        if (UIManager.Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (RaceManager.Instance == null)
        {
            Debug.LogError("RaceManager, is not in scene or not instanced, functions may fail");
        }

        // PlayAgainButton.onClick.AddListener(PlayAgain);
        // MainMenuButton.onClick.AddListener(MainMenu);
        EndScreen.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        SpeedometerArrow.transform.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(minDegrees, maxDegrees, SpeedometerValue));

    }

    public void SetTimer(float inTimer)
    {
        TimeSpan time = TimeSpan.FromSeconds((double)inTimer);
        TimeUI.text = time.ToString(@"mm\:ss\:fff");
    }

    public void SetRacePosition(int pos)
    {
        RacePositionUI.text = "#" + pos.ToString();
    }

    public void ShowEndScreen()
    {
        EndScreen.gameObject.SetActive(true);
    }

    public void AddToLeaderBoard(int InPosition, string InName, float InTime)
    {
        TimeSpan time = TimeSpan.FromSeconds((double)InTime);
        PosEndUI.text += InPosition + "\n";
        NameEndUI.text += InName + "\n";
        TimeEndUI.text += time.ToString(@"mm\:ss\:fff") + "\n";
    }

    public void ClearLeaderBoard()
    {
        PosEndUI.text = "";
        NameEndUI.text = "";
        TimeEndUI.text = "";
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(1);
    }

    public void PlayAgain()
    {
        int activeSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(activeSceneIndex);
    }

    [CustomEditor(typeof(UIManager))]
    public class customButton : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            UIManager UIManager = (UIManager)target;
            if (GUILayout.Button("Toggle End Screen"))
            {
                UIManager.ShowEndScreen();
            }

            if (GUILayout.Button("Add to Leaderboard"))
            {
                UIManager.AddToLeaderBoard(1, "goat", Time.time);
            }
            if (GUILayout.Button("Clear Leaderboard"))
            {
                UIManager.ClearLeaderBoard();
            }
        }

    }
}
