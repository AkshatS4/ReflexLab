using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject game1Panel;
    [SerializeField] private GameObject game2Panel;
    [SerializeField] private GameObject resultsPanel;

    [Header("Menu")]
    [SerializeField] private TMP_Dropdown gameDropdown;
    [SerializeField] private TMP_Dropdown difficultyDropdown;

    [Header("Results")]
    [SerializeField] private TMP_Text resultsTitle;
    [SerializeField] private TMP_Text resultsScore;
    [SerializeField] private TMP_Text resultsDetails;

    [Header("Controllers")]
    [SerializeField] private Game1Controller game1Controller;
    [SerializeField] private Game2Controller game2Controller;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        SetupDropdowns();
        ReturnToMenu();
    }

    private void SetupDropdowns()
    {
        gameDropdown.ClearOptions();

        gameDropdown.AddOptions(
            new System.Collections.Generic.List<string>
            {
                "Game 1 - Reflex Rush",
                "Game 2 - Stroop Shift"
            }
        );

        difficultyDropdown.ClearOptions();

        difficultyDropdown.AddOptions(
            new System.Collections.Generic.List<string>
            {
                "Easy",
                "Medium"
            }
        );

        gameDropdown.value = 0;
        difficultyDropdown.value = 0;

        gameDropdown.RefreshShownValue();
        difficultyDropdown.RefreshShownValue();
    }

    public void StartSelectedGame()
    {
        GameMode mode =
            (GameMode)gameDropdown.value;

        Difficulty difficulty =
            (Difficulty)difficultyDropdown.value;

        menuPanel.SetActive(false);
        resultsPanel.SetActive(false);

        if (mode == GameMode.Game1)
        {
            game2Controller.Stop();

            game1Panel.SetActive(true);
            game2Panel.SetActive(false);

            game1Controller.Begin(
                difficulty
            );
        }
        else
        {
            game1Controller.Stop();

            game1Panel.SetActive(false);
            game2Panel.SetActive(true);

            game2Controller.Begin(
                difficulty
            );
        }
    }

    public void ShowGame1Results(
        int score,
        int mouseHits,
        int mouseMisses,
        int keyHits,
        int keyMisses
    )
    {
        game1Panel.SetActive(false);
        game2Panel.SetActive(false);
        menuPanel.SetActive(false);

        resultsPanel.SetActive(true);

        int mouseAttempts =
            mouseHits + mouseMisses;

        int keyAttempts =
            keyHits + keyMisses;

        float mouseAccuracy =
            mouseAttempts > 0
                ? (float)mouseHits / mouseAttempts * 100f
                : 100f;

        float keyAccuracy =
            keyAttempts > 0
                ? (float)keyHits / keyAttempts * 100f
                : 100f;

        resultsTitle.text =
            "REFLEX RUSH COMPLETE";

        resultsScore.text =
            $"SCORE\n{score}";

        resultsDetails.text =
            $"Mouse accuracy: {mouseAccuracy:0}%\n" +
            $"Key accuracy: {keyAccuracy:0}%";
    }

    public void ShowGame2Results(
        int score,
        int totalRounds
    )
    {
        game1Panel.SetActive(false);
        game2Panel.SetActive(false);
        menuPanel.SetActive(false);

        resultsPanel.SetActive(true);

        float accuracy =
            totalRounds > 0
                ? (float)score / totalRounds * 100f
                : 0f;

        resultsTitle.text =
            "STROOP SHIFT COMPLETE";

        resultsScore.text =
            $"SCORE\n{score} / {totalRounds}";

        resultsDetails.text =
            $"Accuracy: {accuracy:0}%";
    }

    public void ReturnToMenu()
    {
        game1Controller.Stop();
        game2Controller.Stop();

        menuPanel.SetActive(true);
        game1Panel.SetActive(false);
        game2Panel.SetActive(false);
        resultsPanel.SetActive(false);
    }
}