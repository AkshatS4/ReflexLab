using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Game1Controller : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private RectTransform gameArea;
    [SerializeField] private Transform targetContainer;

    [SerializeField] private GameObject circleTargetPrefab;
    [SerializeField] private GameObject keyTargetPrefab;

    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text mouseAccuracyText;
    [SerializeField] private TMP_Text keyAccuracyText;

    [Header("Game Settings")]
    [SerializeField] private float gameDuration = 30f;

    [Header("Medium Shrink Settings")]
    [SerializeField] private float mediumShrinkDelay = 0.25f;
    [SerializeField] private float mediumShrinkDuration = 0.70f;

    [Header("Spawn Safety")]
    [SerializeField] private int positionAttempts = 30;
    [SerializeField] private float extraSpawnPadding = 10f;

    private Difficulty difficulty;

    private float timeRemaining;

    private float circleSpawnTimer;
    private float keySpawnTimer;

    private bool running;

    private int score;

    private int mouseHits;
    private int mouseExpired;

    private int keyHits;
    private int keyMisses;

    private readonly List<CircleTarget> activeCircles = new();
    private readonly List<KeyTarget> activeKeys = new();

    private const int MaxCircles = 2;
    private const int MaxKeys = 2;

    private const float EasyCircleLifetime = 1.60f;
    private const float MediumCircleLifetime = 0.95f;

    private const float EasyKeyLifetime = 1.60f;
    private const float MediumKeyLifetime = 0.95f;

    private const float EasyCircleSpawnInterval = 0.75f;
    private const float MediumCircleSpawnInterval = 0.55f;

    private const float EasyKeySpawnInterval = 0.75f;
    private const float MediumKeySpawnInterval = 0.55f;

    public void Begin(Difficulty selectedDifficulty)
    {
        difficulty = selectedDifficulty;

        ClearTargets();

        running = true;

        score = 0;

        mouseHits = 0;
        mouseExpired = 0;

        keyHits = 0;
        keyMisses = 0;

        timeRemaining = gameDuration;

        circleSpawnTimer = 0f;
        keySpawnTimer = 0f;

        UpdateUI();

        // Start with one of each type.
        SpawnCircle();
        SpawnKey();
    }

    private void Update()
    {
        if (!running)
        {
            return;
        }

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            EndGame();
            return;
        }

        HandleKeyboardInput();

        UpdateSpawnTimers();

        UpdateUI();
    }

    private void UpdateSpawnTimers()
    {
        circleSpawnTimer -= Time.deltaTime;

        if (circleSpawnTimer <= 0f)
        {
            if (activeCircles.Count < MaxCircles)
            {
                SpawnCircle();
            }

            circleSpawnTimer = GetCircleSpawnInterval();
        }

        keySpawnTimer -= Time.deltaTime;

        if (keySpawnTimer <= 0f)
        {
            if (activeKeys.Count < MaxKeys)
            {
                SpawnKey();
            }

            keySpawnTimer = GetKeySpawnInterval();
        }
    }

    private void HandleKeyboardInput()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        if (keyboard.wKey.wasPressedThisFrame)
        {
            HandleKeyPress(KeyTargetLetter.W);
        }
        else if (keyboard.aKey.wasPressedThisFrame)
        {
            HandleKeyPress(KeyTargetLetter.A);
        }
        else if (keyboard.sKey.wasPressedThisFrame)
        {
            HandleKeyPress(KeyTargetLetter.S);
        }
        else if (keyboard.dKey.wasPressedThisFrame)
        {
            HandleKeyPress(KeyTargetLetter.D);
        }
    }

    private void HandleKeyPress(KeyTargetLetter pressedLetter)
    {
        if (activeKeys.Count == 0)
        {
            keyMisses++;
            return;
        }

        KeyTarget matchingTarget = null;

        for (int i = 0; i < activeKeys.Count; i++)
        {
            if (activeKeys[i].Letter == pressedLetter)
            {
                matchingTarget = activeKeys[i];
                break;
            }
        }

        if (matchingTarget != null)
        {
            matchingTarget.ResolveHit();
            return;
        }

        // Wrong key:
        // remove whichever currently active key target appeared first.
        KeyTarget oldest = GetOldestKeyTarget();

        if (oldest != null)
        {
            oldest.ResolveWrongKey();
        }
    }

    private KeyTarget GetOldestKeyTarget()
    {
        KeyTarget oldest = null;
        float oldestTime = float.MaxValue;

        for (int i = 0; i < activeKeys.Count; i++)
        {
            KeyTarget target = activeKeys[i];

            if (target == null)
            {
                continue;
            }

            if (target.SpawnTime < oldestTime)
            {
                oldest = target;
                oldestTime = target.SpawnTime;
            }
        }

        return oldest;
    }

    private void SpawnCircle()
    {
        if (!running)
        {
            return;
        }

        if (activeCircles.Count >= MaxCircles)
        {
            return;
        }

        float size = difficulty == Difficulty.Easy ? 110f : 88f;

        Vector2 position;

        if (!TryGetSafeTargetPosition(
                new Vector2(size, size),
                out position))
        {
            return;
        }

        GameObject objectInstance =
            Instantiate(circleTargetPrefab, targetContainer);

        RectTransform rect =
            objectInstance.GetComponent<RectTransform>();

        rect.sizeDelta = new Vector2(size, size);
        rect.anchoredPosition = position;

        CircleTarget target =
            objectInstance.GetComponent<CircleTarget>();

        activeCircles.Add(target);

        float lifetime = GetCircleLifetime();

        bool shrink = difficulty == Difficulty.Medium;

        target.Initialize(
            this,
            lifetime,
            shrink
        );
    }

    private void SpawnKey()
    {
        if (!running)
        {
            return;
        }

        if (activeKeys.Count >= MaxKeys)
        {
            return;
        }

        float width = difficulty == Difficulty.Easy ? 125f : 105f;
        float height = difficulty == Difficulty.Easy ? 95f : 80f;

        Vector2 targetSize =
            new Vector2(width, height);

        Vector2 position;

        if (!TryGetSafeTargetPosition(
                targetSize,
                out position))
        {
            return;
        }

        GameObject objectInstance =
            Instantiate(keyTargetPrefab, targetContainer);

        RectTransform rect =
            objectInstance.GetComponent<RectTransform>();

        rect.sizeDelta = targetSize;
        rect.anchoredPosition = position;

        KeyTarget target =
            objectInstance.GetComponent<KeyTarget>();

        KeyTargetLetter letter =
            GetRandomKeyLetter();

        target.SetLetter(letter);

        TMP_Text label =
            objectInstance.GetComponentInChildren<TMP_Text>();

        if (label != null)
        {
            label.text = letter.ToString();
        }

        activeKeys.Add(target);

        float lifetime = GetKeyLifetime();

        bool shrink = difficulty == Difficulty.Medium;

        target.Initialize(
            this,
            lifetime,
            shrink
        );
    }

    private KeyTargetLetter GetRandomKeyLetter()
    {
        int random = Random.Range(0, 4);

        return (KeyTargetLetter)random;
    }

    private bool TryGetSafeTargetPosition(
        Vector2 targetSize,
        out Vector2 position)
    {
        Rect rect = gameArea.rect;

        float horizontalMargin = 80f;
        float verticalMargin = 80f;

        float halfWidth = rect.width * 0.5f;
        float halfHeight = rect.height * 0.5f;

        float xMin =
            -halfWidth +
            horizontalMargin +
            targetSize.x * 0.5f;

        float xMax =
            halfWidth -
            horizontalMargin -
            targetSize.x * 0.5f;

        float yMin =
            -halfHeight +
            verticalMargin +
            targetSize.y * 0.5f;

        float yMax =
            halfHeight -
            verticalMargin -
            targetSize.y * 0.5f;

        for (int attempt = 0; attempt < positionAttempts; attempt++)
        {
            Vector2 candidate = new Vector2(
                Random.Range(xMin, xMax),
                Random.Range(yMin, yMax)
            );

            if (IsPositionSafe(candidate, targetSize))
            {
                position = candidate;
                return true;
            }
        }

        // If no safe position was found after all attempts,
        // do not spawn the object.
        position = Vector2.zero;
        return false;
    }

    private bool IsPositionSafe(
        Vector2 candidate,
        Vector2 candidateSize)
    {
        float candidateHalfWidth =
            candidateSize.x * 0.5f;

        float candidateHalfHeight =
            candidateSize.y * 0.5f;

        for (int i = 0; i < activeCircles.Count; i++)
        {
            CircleTarget circle = activeCircles[i];

            if (circle == null)
            {
                continue;
            }

            RectTransform rect =
                circle.GetComponent<RectTransform>();

            if (rect == null)
            {
                continue;
            }

            if (RectanglesOverlap(
                    candidate,
                    candidateHalfWidth,
                    candidateHalfHeight,
                    rect.anchoredPosition,
                    rect.rect.width * 0.5f,
                    rect.rect.height * 0.5f))
            {
                return false;
            }
        }

        for (int i = 0; i < activeKeys.Count; i++)
        {
            KeyTarget key = activeKeys[i];

            if (key == null)
            {
                continue;
            }

            RectTransform rect =
                key.GetComponent<RectTransform>();

            if (rect == null)
            {
                continue;
            }

            if (RectanglesOverlap(
                    candidate,
                    candidateHalfWidth,
                    candidateHalfHeight,
                    rect.anchoredPosition,
                    rect.rect.width * 0.5f,
                    rect.rect.height * 0.5f))
            {
                return false;
            }
        }

        return true;
    }

    private bool RectanglesOverlap(
        Vector2 positionA,
        float halfWidthA,
        float halfHeightA,
        Vector2 positionB,
        float halfWidthB,
        float halfHeightB)
    {
        float horizontalDistance =
            Mathf.Abs(positionA.x - positionB.x);

        float verticalDistance =
            Mathf.Abs(positionA.y - positionB.y);

        float requiredHorizontalDistance =
            halfWidthA +
            halfWidthB +
            extraSpawnPadding;

        float requiredVerticalDistance =
            halfHeightA +
            halfHeightB +
            extraSpawnPadding;

        return horizontalDistance < requiredHorizontalDistance &&
               verticalDistance < requiredVerticalDistance;
    }

    public float GetMediumShrinkDelay()
    {
        return mediumShrinkDelay;
    }

    public float GetMediumShrinkDuration()
    {
        return mediumShrinkDuration;
    }

    public void TargetHit(Game1TargetBase target)
    {
        if (!running)
        {
            return;
        }

        score++;

        if (target is CircleTarget circle)
        {
            mouseHits++;

            activeCircles.Remove(circle);

            Destroy(circle.gameObject);

            // A successful hit immediately creates a new opportunity.
            SpawnCircle();
        }
        else if (target is KeyTarget key)
        {
            keyHits++;

            activeKeys.Remove(key);

            Destroy(key.gameObject);

            // A successful hit immediately creates a new opportunity.
            SpawnKey();
        }
    }

    public void TargetExpired(Game1TargetBase target)
    {
        if (!running)
        {
            return;
        }

        if (target is CircleTarget circle)
        {
            mouseExpired++;

            activeCircles.Remove(circle);

            Destroy(circle.gameObject);

            // No immediate replacement.
        }
        else if (target is KeyTarget key)
        {
            keyMisses++;

            activeKeys.Remove(key);

            Destroy(key.gameObject);
        }
    }

    public void TargetWrongKey(Game1TargetBase target)
    {
        if (!running)
        {
            return;
        }

        if (target is KeyTarget key)
        {
            keyMisses++;

            activeKeys.Remove(key);

            Destroy(key.gameObject);

            // No immediate replacement after a wrong key.
        }
    }

    private float GetCircleLifetime()
    {
        return difficulty == Difficulty.Easy
            ? EasyCircleLifetime
            : MediumCircleLifetime;
    }

    private float GetKeyLifetime()
    {
        return difficulty == Difficulty.Easy
            ? EasyKeyLifetime
            : MediumKeyLifetime;
    }

    private float GetCircleSpawnInterval()
    {
        return difficulty == Difficulty.Easy
            ? EasyCircleSpawnInterval
            : MediumCircleSpawnInterval;
    }

    private float GetKeySpawnInterval()
    {
        return difficulty == Difficulty.Easy
            ? EasyKeySpawnInterval
            : MediumKeySpawnInterval;
    }

    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"SCORE  {score}";
        }

        if (timerText != null)
        {
            timerText.text =
                $"TIME  {Mathf.CeilToInt(timeRemaining)}";
        }

        if (mouseAccuracyText != null)
        {
            int mouseAttempts =
                mouseHits + mouseExpired;

            float accuracy =
                mouseAttempts > 0
                    ? (float)mouseHits / mouseAttempts * 100f
                    : 100f;

            mouseAccuracyText.text =
                $"MOUSE  {accuracy:0}%";
        }

        if (keyAccuracyText != null)
        {
            int keyAttempts =
                keyHits + keyMisses;

            float accuracy =
                keyAttempts > 0
                    ? (float)keyHits / keyAttempts * 100f
                    : 100f;

            keyAccuracyText.text =
                $"KEYS  {accuracy:0}%";
        }
    }

    private void EndGame()
    {
        if (!running)
        {
            return;
        }

        running = false;

        ClearTargets();

        GameManager.Instance.ShowGame1Results(
            score,
            mouseHits,
            mouseExpired,
            keyHits,
            keyMisses
        );
    }

    public void Stop()
    {
        running = false;

        ClearTargets();
    }

    private void ClearTargets()
    {
        for (int i = targetContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(targetContainer.GetChild(i).gameObject);
        }

        activeCircles.Clear();
        activeKeys.Clear();
    }
}