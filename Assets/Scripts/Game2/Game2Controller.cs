using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Game2Controller : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform optionsContainer;

    [SerializeField] private TMP_Text stimulusText;
    [SerializeField] private Image stimulusBackground;

    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text timerText;

    [SerializeField] private GameObject colorOptionPrefab;

    [Header("Game Settings")]
    [SerializeField] private int totalRounds = 10;

    [Header("Text Appearance")]
    [SerializeField] private float stimulusFontSize = 100f;
    [SerializeField] private float optionFontSize = 42f;

    [Header("Stimulus Box")]
    [SerializeField] private float stimulusPadding = 40f;

    /*
     * Six bright, highly saturated colors.
     */
    private readonly ColorData[] colors =
    {
        new ColorData("RED",    new Color(1.00f, 0.02f, 0.02f, 1.00f)),
        new ColorData("BLUE",   new Color(0.02f, 0.20f, 1.00f, 1.00f)),
        new ColorData("GREEN",  new Color(0.00f, 0.90f, 0.08f, 1.00f)),
        new ColorData("YELLOW", new Color(1.00f, 0.85f, 0.00f, 1.00f)),
        new ColorData("ORANGE", new Color(1.00f, 0.30f, 0.00f, 1.00f)),
        new ColorData("PURPLE", new Color(0.65f, 0.05f, 1.00f, 1.00f))
    };

    private Difficulty difficulty;

    private bool running;
    private bool roundAnswered;

    private int currentRound;
    private int score;

    private Coroutine gameRoutine;

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

        PrepareStimulusText();

        UpdateUI();

        gameRoutine =
            StartCoroutine(GameRoutine());
    }

    private IEnumerator GameRoutine()
    {
        while (
            running &&
            currentRound <= totalRounds
        )
        {
            yield return StartCoroutine(
                PlayRound()
            );

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

        /*
         * WORD COLOR
         *
         * Example:
         *
         * Word = RED
         * Font = BLUE
         */
        int wordIndex =
            Random.Range(
                0,
                colors.Length
            );

        int fontColorIndex =
            Random.Range(
                0,
                colors.Length
            );

        /*
         * The word and its font must
         * always be different.
         */
        while (
            fontColorIndex ==
            wordIndex
        )
        {
            fontColorIndex =
                Random.Range(
                    0,
                    colors.Length
                );
        }

        ColorData word =
            colors[wordIndex];

        ColorData fontColor =
            colors[fontColorIndex];

        /*
         * Find a third color for the
         * stimulus box.
         *
         * Stimulus has:
         *
         * 1. Box background
         * 2. Font color
         * 3. Word meaning
         *
         * All three are different.
         */
        int stimulusBackgroundIndex =
            GetDifferentColorIndex(
                wordIndex,
                fontColorIndex
            );

        ColorData stimulusBoxColor =
            colors[
                stimulusBackgroundIndex
            ];

        /*
         * Configure stimulus.
         */
        stimulusText.text =
            word.name;

        stimulusText.color =
            ForceOpaque(
                fontColor.color
            );

        stimulusText.fontSize =
            stimulusFontSize;

        stimulusText.fontStyle =
            FontStyles.Bold;

        stimulusText.fontWeight =
            FontWeight.Bold;

        stimulusText.gameObject.SetActive(
            true
        );

        /*
         * Set stimulus box color.
         */
        if (stimulusBackground != null)
        {
            stimulusBackground.color =
                ForceOpaque(
                    stimulusBoxColor.color
                );
        }

        /*
         * Make sure the stimulus box
         * is visible.
         */
        if (stimulusBackground != null)
        {
            stimulusBackground.gameObject
                .SetActive(true);
        }

        float stimulusTimer =
            stimulusDuration;

        while (
            stimulusTimer > 0f &&
            running
        )
        {
            stimulusTimer -=
                Time.deltaTime;

            UpdateTimer(
                stimulusTimer
            );

            yield return null;
        }

        stimulusText.gameObject.SetActive(
            false
        );

        if (stimulusBackground != null)
        {
            stimulusBackground.gameObject
                .SetActive(false);
        }

        if (!running)
        {
            yield break;
        }

        /*
         * EASY = 3 options
         * MEDIUM = 5 options
         */
        int optionCount =
            difficulty == Difficulty.Easy
                ? 3
                : 5;

        /*
         * The correct answer is the
         * WORD matching the stimulus
         * FONT COLOR.
         */
        List<string> optionNames =
            BuildOptions(
                fontColor.name,
                optionCount
            );

        /*
         * Create a shuffled list of
         * background colors.
         *
         * IMPORTANT:
         *
         * A background color can appear
         * ONLY ONCE in this round.
         */
        List<int> backgroundIndices =
            BuildUniqueBackgroundColors(
                optionCount
            );

        for (
            int i = 0;
            i < optionCount;
            i++
        )
        {
            GameObject optionObject =
                Instantiate(
                    colorOptionPrefab,
                    optionsContainer
                );

            ColorOption option =
                optionObject.GetComponent<
                    ColorOption
                >();

            string optionName =
                optionNames[i];

            bool isCorrect =
                optionName ==
                fontColor.name;

            /*
             * COLOR #1:
             *
             * The WORD itself.
             */
            int wordColorIndex =
                GetColorIndex(
                    optionName
                );

            /*
             * COLOR #2:
             *
             * Font color.
             *
             * Must be different from
             * the word color.
             */
            int fontIndex =
                GetDifferentColorIndex(
                    wordColorIndex
                );

            /*
             * COLOR #3:
             *
             * Background color.
             *
             * Must be different from
             * both word and font.
             *
             * Also comes from the unique
             * background list so it cannot
             * repeat another option's box.
             */
            int backgroundIndex =
                backgroundIndices[i];

            /*
             * Safety check:
             *
             * If the unique background
             * happens to equal the word
             * or font, find another one.
             */
            if (
                backgroundIndex ==
                wordColorIndex ||
                backgroundIndex ==
                fontIndex
            )
            {
                backgroundIndex =
                    FindSafeBackgroundColor(
                        wordColorIndex,
                        fontIndex,
                        backgroundIndices,
                        i
                    );
            }

            ColorData fontColorData =
                colors[fontIndex];

            ColorData backgroundColorData =
                colors[backgroundIndex];

            /*
             * Setup the option through
             * the existing ColorOption.
             */
            option.Setup(
                optionName,
                ForceOpaque(
                    fontColorData.color
                ),
                isCorrect,
                this
            );

            /*
             * DIRECTLY force the TMP text
             * color after ColorOption.Setup.
             *
             * This prevents the prefab from
             * making the text look black/dull.
             */
            TMP_Text optionText =
                optionObject
                    .GetComponentInChildren<
                        TMP_Text
                    >();

            if (optionText != null)
            {
                optionText.color =
                    ForceOpaque(
                        fontColorData.color
                    );

                optionText.fontSize =
                    optionFontSize;

                optionText.fontStyle =
                    FontStyles.Bold;

                optionText.fontWeight =
                    FontWeight.Bold;

                optionText.alpha = 1f;

                optionText.enableVertexGradient =
                    false;
            }

            /*
             * Set the box background.
             */
            Image optionImage =
                optionObject.GetComponent<Image>();

            if (optionImage == null)
            {
                optionImage =
                    optionObject
                        .GetComponentInChildren<Image>();
            }

            if (optionImage != null)
            {
                optionImage.color =
                    ForceOpaque(
                        backgroundColorData.color
                    );
            }

            RectTransform rect =
                optionObject.GetComponent<
                    RectTransform
                >();

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
                SetupMediumOption(
                    rect
                );
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
            answerTimer -=
                Time.deltaTime;

            UpdateTimer(
                answerTimer
            );

            yield return null;
        }

        if (
            !roundAnswered &&
            running
        )
        {
            /*
             * Timeout counts as incorrect.
             */
            roundAnswered = true;
        }

        ClearOptions();

        yield return new WaitForSeconds(
            0.08f
        );
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

        for (
            int i = 0;
            i < colors.Length;
            i++
        )
        {
            if (
                colors[i].name !=
                correctName
            )
            {
                distractors.Add(
                    colors[i].name
                );
            }
        }

        /*
         * Shuffle distractors.
         */
        for (
            int i =
                distractors.Count - 1;
            i > 0;
            i--
        )
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

        /*
         * Add enough distractors.
         */
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

        /*
         * Shuffle final answer order.
         */
        for (
            int i = result.Count - 1;
            i > 0;
            i--
        )
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

    private List<int> BuildUniqueBackgroundColors(
        int amount
    )
    {
        List<int> indices =
            new List<int>();

        /*
         * Start with all six colors.
         */
        for (
            int i = 0;
            i < colors.Length;
            i++
        )
        {
            indices.Add(i);
        }

        /*
         * Fisher-Yates shuffle.
         */
        for (
            int i = indices.Count - 1;
            i > 0;
            i--
        )
        {
            int randomIndex =
                Random.Range(
                    0,
                    i + 1
                );

            int temp =
                indices[i];

            indices[i] =
                indices[randomIndex];

            indices[randomIndex] =
                temp;
        }

        /*
         * Only use as many as needed.
         *
         * Therefore:
         *
         * 3 options = 3 unique backgrounds
         * 5 options = 5 unique backgrounds
         */
        return indices.GetRange(
            0,
            amount
        );
    }

    private int FindSafeBackgroundColor(
        int wordIndex,
        int fontIndex,
        List<int> usedBackgrounds,
        int currentIndex
    )
    {
        for (
            int i = 0;
            i < colors.Length;
            i++
        )
        {
            if (
                i == wordIndex ||
                i == fontIndex
            )
            {
                continue;
            }

            bool alreadyUsed =
                false;

            for (
                int j = 0;
                j < currentIndex;
                j++
            )
            {
                if (
                    usedBackgrounds[j] ==
                    i
                )
                {
                    alreadyUsed = true;
                    break;
                }
            }

            if (!alreadyUsed)
            {
                usedBackgrounds[
                    currentIndex
                ] = i;

                return i;
            }
        }

        return 0;
    }

    private int GetColorIndex(
        string colorName
    )
    {
        for (
            int i = 0;
            i < colors.Length;
            i++
        )
        {
            if (
                colors[i].name ==
                colorName
            )
            {
                return i;
            }
        }

        return 0;
    }

    private int GetDifferentColorIndex(
        int forbiddenIndex
    )
    {
        int result;

        do
        {
            result =
                Random.Range(
                    0,
                    colors.Length
                );

        } while (
            result ==
            forbiddenIndex
        );

        return result;
    }

    private int GetDifferentColorIndex(
        int forbiddenIndexA,
        int forbiddenIndexB
    )
    {
        int result;

        do
        {
            result =
                Random.Range(
                    0,
                    colors.Length
                );

        } while (
            result ==
            forbiddenIndexA ||
            result ==
            forbiddenIndexB
        );

        return result;
    }

    private Color ForceOpaque(
        Color color
    )
    {
        color.a = 1f;

        return color;
    }

    private void PrepareStimulusText()
    {
        if (stimulusText == null)
        {
            return;
        }

        stimulusText.fontSize =
            stimulusFontSize;

        stimulusText.fontStyle =
            FontStyles.Bold;

        stimulusText.fontWeight =
            FontWeight.Bold;

        stimulusText.alpha = 1f;

        stimulusText.enableVertexGradient =
            false;
    }

    private void SetupEasyOption(
        RectTransform rect,
        int index,
        int optionCount
    )
    {
        float spacing = 130f;

        float totalHeight =
            (optionCount - 1) *
            spacing;

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
                Random.Range(
                    xMin,
                    xMax
                ),
                Random.Range(
                    yMin,
                    yMax
                )
            );
    }

    public void SelectOption(
        bool correct
    )
    {
        if (
            !running ||
            roundAnswered
        )
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

    private void UpdateTimer(
        float value
    )
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
            int i =
                optionsContainer.childCount - 1;
            i >= 0;
            i--
        )
        {
            Destroy(
                optionsContainer
                    .GetChild(i)
                    .gameObject
            );
        }
    }

    private void EndGame()
    {
        running = false;

        ClearOptions();

        if (stimulusText != null)
        {
            stimulusText.gameObject
                .SetActive(false);
        }

        if (stimulusBackground != null)
        {
            stimulusBackground.gameObject
                .SetActive(false);
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
            StopCoroutine(
                gameRoutine
            );

            gameRoutine = null;
        }

        ClearOptions();

        if (stimulusText != null)
        {
            stimulusText.gameObject
                .SetActive(false);
        }

        if (stimulusBackground != null)
        {
            stimulusBackground.gameObject
                .SetActive(false);
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