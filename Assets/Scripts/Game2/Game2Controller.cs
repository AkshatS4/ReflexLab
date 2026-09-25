using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Game2Controller : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform optionsContainer;
    [SerializeField] private TMP_Text stimulusText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text timerText;

    [SerializeField] private GameObject colorOptionPrefab;

    [Header("Game Settings")]
    [SerializeField] private int totalRounds = 10;

    private Difficulty difficulty;

    private bool running;
    private bool roundAnswered;

    private int currentRound;
    private int score;

    private Coroutine gameRoutine;

    private readonly ColorData[] colors =
    {
        new ColorData("RED", new Color(0.90f, 0.08f, 0.08f)),
        new ColorData("BLUE", new Color(0.08f, 0.35f, 0.95f)),
        new ColorData("GREEN", new Color(0.08f, 0.75f, 0.25f)),
        new ColorData("YELLOW", new Color(0.95f, 0.80f, 0.05f)),
        new ColorData("ORANGE", new Color(1.00f, 0.45f, 0.05f)),
        new ColorData("PURPLE", new Color(0.55f, 0.20f, 0.80f))
    };

    private float stimulusDuration;
    private float answerDuration;

    public void Begin(Difficulty selectedDifficulty)
    {
        Stop();

        difficulty = selectedDifficulty;

        currentRound = 1;
        score = 0;

        stimulusDuration =
            difficulty == Difficulty.Easy
                ? 1.6f
                : 0.9f;

        answerDuration =
            difficulty == Difficulty.Easy
                ? 3.0f
                : 2.0f;

        running = true;

        UpdateUI();

        gameRoutine = StartCoroutine(GameRoutine());
    }

    private IEnumerator GameRoutine()
    {
        while (running && currentRound <= totalRounds)
        {
            yield return StartCoroutine(PlayRound());

            if (!running)
            {
                yield break;
            }

            currentRound++;

            UpdateUI();
        }

        if (running)
        {
            EndGame();
        }
    }

    private IEnumerator PlayRound()
    {
        roundAnswered = false;

        ClearOptions();

        int wordIndex =
            Random.Range(0, colors.Length);

        int fontColorIndex =
            Random.Range(0, colors.Length);

        // Make the Stroop conflict guaranteed.
        while (fontColorIndex == wordIndex)
        {
            fontColorIndex =
                Random.Range(0, colors.Length);
        }

        ColorData word =
            colors[wordIndex];

        ColorData fontColor =
            colors[fontColorIndex];

        // Example:
        // RED
        // rendered in ORANGE.
        stimulusText.text = word.name;
        stimulusText.color = fontColor.color;
        stimulusText.gameObject.SetActive(true);

        float stimulusTimer =
            stimulusDuration;

        while (
            stimulusTimer > 0f &&
            running
        )
        {
            stimulusTimer -= Time.deltaTime;

            UpdateTimer(stimulusTimer);

            yield return null;
        }

        stimulusText.gameObject.SetActive(false);

        if (!running)
        {
            yield break;
        }

        int optionCount =
            difficulty == Difficulty.Easy
                ? 3
                : 5;

        List<string> optionNames =
            BuildOptions(
                fontColor.name,
                optionCount
            );

        for (int i = 0;
             i < optionCount;
             i++)
        {
            GameObject optionObject =
                Instantiate(
                    colorOptionPrefab,
                    optionsContainer
                );

            ColorOption option =
                optionObject.GetComponent<ColorOption>();

            string optionName =
                optionNames[i];

            bool isCorrect =
                optionName == fontColor.name;

            ColorData renderedColor =
                colors[
                    Random.Range(
                        0,
                        colors.Length
                    )
                ];

            option.Setup(
                optionName,
                renderedColor.color,
                isCorrect,
                this
            );

            RectTransform rect =
                optionObject.GetComponent<RectTransform>();

            if (difficulty == Difficulty.Easy)
            {
                SetupEasyOption(
                    rect,
                    i,
                    optionCount
                );
            }
            else
            {
                SetupMediumOption(rect);
            }
        }

        float answerTimer =
            answerDuration;

        while (
            answerTimer > 0f &&
            !roundAnswered &&
            running
        )
        {
            answerTimer -= Time.deltaTime;

            UpdateTimer(answerTimer);

            yield return null;
        }

        if (!roundAnswered && running)
        {
            // Timeout is an incorrect answer.
            roundAnswered = true;
        }

        ClearOptions();

        yield return new WaitForSeconds(0.08f);
    }

    private List<string> BuildOptions(
        string correctName,
        int optionCount
    )
    {
        List<string> result =
            new List<string>();

        result.Add(correctName);

        List<string> distractors =
            new List<string>();

        for (int i = 0;
             i < colors.Length;
             i++)
        {
            if (colors[i].name != correctName)
            {
                distractors.Add(
                    colors[i].name
                );
            }
        }

        // Fisher-Yates shuffle.
        for (int i = distractors.Count - 1;
             i > 0;
             i--)
        {
            int randomIndex =
                Random.Range(
                    0,
                    i + 1
                );

            string temp =
                distractors[i];

            distractors[i] =
                distractors[randomIndex];

            distractors[randomIndex] =
                temp;
        }

        for (
            int i = 0;
            i < optionCount - 1;
            i++
        )
        {
            result.Add(
                distractors[i]
            );
        }

        // Shuffle final answer list.
        for (int i = result.Count - 1;
             i > 0;
             i--)
        {
            int randomIndex =
                Random.Range(
                    0,
                    i + 1
                );

            string temp =
                result[i];

            result[i] =
                result[randomIndex];

            result[randomIndex] =
                temp;
        }

        return result;
    }

    private void SetupEasyOption(
        RectTransform rect,
        int index,
        int optionCount
    )
    {
        float spacing = 130f;

        float totalHeight =
            (optionCount - 1) * spacing;

        float y =
            totalHeight * 0.5f -
            index * spacing;

        rect.anchoredPosition =
            new Vector2(
                0f,
                y
            );
    }

    private void SetupMediumOption(
        RectTransform rect
    )
    {
        Rect area =
            optionsContainer.rect;

        float margin = 140f;

        float width =
            rect.sizeDelta.x;

        float height =
            rect.sizeDelta.y;

        float xMin =
            -area.width * 0.5f +
            margin +
            width * 0.5f;

        float xMax =
            area.width * 0.5f -
            margin -
            width * 0.5f;

        float yMin =
            -area.height * 0.5f +
            margin +
            height * 0.5f;

        float yMax =
            area.height * 0.5f -
            margin -
            height * 0.5f;

        rect.anchoredPosition =
            new Vector2(
                Random.Range(xMin, xMax),
                Random.Range(yMin, yMax)
            );
    }

    public void SelectOption(bool correct)
    {
        if (!running || roundAnswered)
        {
            return;
        }

        roundAnswered = true;

        if (correct)
        {
            score++;
        }

        UpdateUI();
    }

    private void UpdateTimer(float value)
    {
        if (timerText != null)
        {
            timerText.text =
                $"TIME  {Mathf.Max(0f, value):0.0}";
        }
    }

    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text =
                $"SCORE  {score}";
        }

        if (roundText != null)
        {
            roundText.text =
                $"ROUND  {currentRound} / {totalRounds}";
        }
    }

    private void ClearOptions()
    {
        if (optionsContainer == null)
        {
            return;
        }

        for (
            int i = optionsContainer.childCount - 1;
            i >= 0;
            i--
        )
        {
            Destroy(
                optionsContainer.GetChild(i).gameObject
            );
        }
    }

    private void EndGame()
    {
        running = false;

        ClearOptions();

        if (stimulusText != null)
        {
            stimulusText.gameObject.SetActive(false);
        }

        GameManager.Instance.ShowGame2Results(
            score,
            totalRounds
        );
    }

    public void Stop()
    {
        running = false;

        if (gameRoutine != null)
        {
            StopCoroutine(gameRoutine);
            gameRoutine = null;
        }

        ClearOptions();

        if (stimulusText != null)
        {
            stimulusText.gameObject.SetActive(false);
        }
    }

    private class ColorData
    {
        public string name;
        public Color color;

        public ColorData(
            string colorName,
            Color colorValue
        )
        {
            name = colorName;
            color = colorValue;
        }
    }
}