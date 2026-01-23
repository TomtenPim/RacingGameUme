using System;
using System.Collections;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines.Interpolators;
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
    public GameObject PauseScreen;
    public TextMeshProUGUI PosEndUI;
    public TextMeshProUGUI NameEndUI;
    public TextMeshProUGUI TimeEndUI;
    public Button PlayAgainButton;
    public Button MainMenuButton;
    public Button ResumePauseButton;
    public Button RestartPauseButton;
    public Button MainMenuPauseButton;
    public TextMeshProUGUI LapText;
    public TextMeshProUGUI CountdownText;
    [Range(0f, 5f)]
    public float LapTextFadeOutTime = 1f;

    public bool StartTimerDone = false;

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

        PlayAgainButton.onClick.AddListener(PlayAgain);
        MainMenuButton.onClick.AddListener(MainMenu);
        ResumePauseButton.onClick.AddListener(Pause);
        RestartPauseButton.onClick.AddListener(PlayAgain);
        MainMenuPauseButton.onClick.AddListener(MainMenu);
        EndScreen.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateLapText(int lap)
    {
        LapText.text = "Lap: " + lap;

        StartCoroutine(LapTextEaseIn());

    }

    public void StartCountdown(int seconds)
    {
        StartCoroutine(CountdownTextEaseIn(seconds));

    }

    IEnumerator CountdownTextEaseIn(int seconds)
    {
        float t = LapTextFadeOutTime / 100;
        while (seconds > 0)
        {
            CountdownText.text = seconds.ToString() + "!";
            for (float i = 0; i < 1; i += 0.01f)
            {
                float alpha = Mathf.Lerp(1, 0, easeOutExpo(i));
                CountdownText.color = new Color(CountdownText.color.r, CountdownText.color.g, CountdownText.color.b, alpha);
                yield return new WaitForSeconds(t);
            }
            seconds--;
        }
        RaceManager.Instance.StartRace();
        CountdownText.text = "GO!";
        for (float i = 0; i < 1; i += 0.01f)
        {
            float alpha = Mathf.Lerp(1, 0, easeOutExpo(i));
            CountdownText.color = new Color(CountdownText.color.r, CountdownText.color.g, CountdownText.color.b, alpha);
            yield return new WaitForSeconds(t);
        }
    }

    private float easeIn(float x)
    {
        return 1 - Mathf.Cos((x * Mathf.PI) / 2);
    }

    private float easeOutExpo(float x)
    {
        return x == 0 ? 0 : Mathf.Pow(2, 10 * x - 10);
    }


    IEnumerator LapTextEaseIn()
    {
        float t = LapTextFadeOutTime / 100;
        for (float i = 0; i < 1; i += 0.01f)
        {
            float alpha = Mathf.Lerp(1, 0, easeOutExpo(i));
            LapText.color = new Color(LapText.color.r, LapText.color.g, LapText.color.b, alpha);
            yield return new WaitForSeconds(t);
        }
    }

    public void updateSpeedometer(float value)
    {
        SpeedometerValue = value;
        SpeedometerArrow.transform.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(minDegrees, maxDegrees, SpeedometerValue));
    }

    public void Pause()
    {
        if (Time.timeScale == 0)
        {
            Time.timeScale = 1;
            PauseScreen.gameObject.SetActive(false);
        }
        else
        {
            Time.timeScale = 0;
            PauseScreen.gameObject.SetActive(true);
        }

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
        SceneManager.LoadScene(0);
    }

    public void PlayAgain()
    {
        Time.timeScale = 1;
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
            if (GUILayout.Button("Lap test"))
            {
                UIManager.UpdateLapText(5);
            }
            if (GUILayout.Button("Countdown test"))
            {
                UIManager.StartCountdown(3);
            }
        }

    }
}
