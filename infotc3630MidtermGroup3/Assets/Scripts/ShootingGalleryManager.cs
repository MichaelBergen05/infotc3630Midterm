using UnityEngine;
using UnityEngine.UI;
using TMPro;                
using System.Collections;


public class ShootingGalleryManager : MonoBehaviour
{
 
    [System.Serializable]
    public class LaneSettings
    {
        [Tooltip("Z distance from the player for this lane")]
        public float zDepth = 10f;

        [Tooltip("Min/max Y height for random spawn position")]
        public float minHeight = 1.5f;
        public float maxHeight = 4.5f;

        [Tooltip("X patrol bounds for UFOs in this lane")]
        public float leftBound  = -10f;
        public float rightBound =  10f;

        [Tooltip("Base movement speed for this lane (farther = slower feels right)")]
        public float baseSpeed = 4f;
    }

    [Header("UFO Setup")]
    public GameObject ufoPrefab;
    public LaneSettings[] lanes = new LaneSettings[3];   // Always exactly 3 lanes

    [Header("Respawn")]
    [Tooltip("Seconds before a new UFO appears in a lane after one is destroyed")]
    public float respawnDelay = 1.2f;

    [Header("Speed Multipliers")]
    public float slowMultiplier   = 0.4f;
    public float normalMultiplier = 1.0f;
    public float fastMultiplier   = 2.2f;

    [Header("Scoring")]
    public int pointsPerKill = 100;


    [Header("UI — Buttons")]
    public Button startButton;
    public Button resetButton;
    public Button slowButton;
    public Button normalButton;
    public Button fastButton;

    [Header("UI — Display")]
    public TextMeshProUGUI scoreText;       // Swap to Text if not using TMP
    public TextMeshProUGUI timerText;       // Optional countdown timer
    public TextMeshProUGUI speedLabel;      // Shows current speed setting

    [Header("Game Timer (0 = no timer)")]
    public float gameDuration = 60f;        // Set to 0 for endless mode



    private GameObject[] _activeUFOs;       // One slot per lane, null when dead/respawning
    private float        _currentMultiplier = 1f;
    private int          _score;
    private float        _timeRemaining;
    private bool         _gameRunning;


    void Awake()
    {
        if (lanes.Length != 3)
        {
            Debug.LogWarning("ShootingGalleryManager: lanes array should have exactly 3 entries.");
        }

        _activeUFOs = new GameObject[lanes.Length];
    }

    void OnEnable()
    {
        UFOTarget.OnUFODestroyed += HandleUFODestroyed;
    }

    void OnDisable()
    {
        UFOTarget.OnUFODestroyed -= HandleUFODestroyed;
    }

    void Start()
    {
        WireButtonCallbacks();
        SetUIForIdle();
    }

    void Update()
    {
        if (!_gameRunning) return;

        if (gameDuration > 0f)
        {
            _timeRemaining -= Time.deltaTime;
            UpdateTimerDisplay();

            if (_timeRemaining <= 0f)
                EndGame();
        }
    }


    public void StartGame()
    {
        if (_gameRunning) return;

        _score          = 0;
        _timeRemaining  = gameDuration;
        _gameRunning    = true;
        _currentMultiplier = normalMultiplier;

        UpdateScoreDisplay();
        //SetSpeedButtonLabels("Normal");

        startButton.interactable = false;
        resetButton.interactable = true;

        SpawnAllLanes();
    }

    public void ResetGame()
    {
        StopAllCoroutines();
        _gameRunning = false;

        // Destroy all active UFOs
        for (int i = 0; i < _activeUFOs.Length; i++)
        {
            if (_activeUFOs[i] != null)
            {
                Destroy(_activeUFOs[i]);
                _activeUFOs[i] = null;
            }
        }

        _score         = 0;
        _timeRemaining = gameDuration;

        UpdateScoreDisplay();
        UpdateTimerDisplay();
        SetUIForIdle();
    }

    public void SetSpeedSlow()
    {
        SetSpeedMultiplier(slowMultiplier, "Slow");
    }

    public void SetSpeedNormal()
    {
        SetSpeedMultiplier(normalMultiplier, "Normal");
    }

    public void SetSpeedFast()
    {
        SetSpeedMultiplier(fastMultiplier, "Fast");
    }



    private void SpawnAllLanes()
    {
        for (int i = 0; i < lanes.Length; i++)
            SpawnUFOInLane(i);
    }

    private void SpawnUFOInLane(int laneIndex)
    {
        if (laneIndex < 0 || laneIndex >= lanes.Length) return;
        if (ufoPrefab == null)
        {
            Debug.LogError("ShootingGalleryManager: ufoPrefab is not assigned!");
            return;
        }

        LaneSettings lane = lanes[laneIndex];

        float spawnX = Random.Range(lane.leftBound, lane.rightBound);
        float spawnY = Random.Range(lane.minHeight, lane.maxHeight);
        Vector3 spawnPos = new Vector3(spawnX, spawnY, lane.zDepth);

        GameObject ufo = Instantiate(ufoPrefab, spawnPos, Quaternion.identity);

        // Configure movement
        UFOMovement movement = ufo.GetComponent<UFOMovement>();
        if (movement != null)
        {
            movement.leftBound  = lane.leftBound;
            movement.rightBound = lane.rightBound;
            movement.baseSpeed  = lane.baseSpeed;
            movement.SetSpeedMultiplier(_currentMultiplier);
            movement.RandomizeStartDirection();
        }

        // Tag the UFO with its lane so we can respawn in the right slot
        UFOTarget target = ufo.GetComponent<UFOTarget>();
        if (target != null)
            target.laneIndex = laneIndex;

        _activeUFOs[laneIndex] = ufo;
    }


    private void HandleUFODestroyed(UFOTarget ufo)
    {
        if (!_gameRunning) return;

        _score += pointsPerKill;
        UpdateScoreDisplay();

        int lane = ufo.laneIndex;
        _activeUFOs[lane] = null;

        // Respawn after delay
        StartCoroutine(RespawnAfterDelay(lane, respawnDelay));
    }

    private IEnumerator RespawnAfterDelay(int laneIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (_gameRunning)
            SpawnUFOInLane(laneIndex);
    }


    private void SetSpeedMultiplier(float multiplier, string label)
    {
        _currentMultiplier = multiplier;
        SetSpeedButtonLabels(label);

        // Apply to all currently active UFOs
        foreach (GameObject ufo in _activeUFOs)
        {
            if (ufo == null) continue;
            UFOMovement movement = ufo.GetComponent<UFOMovement>();
            movement?.SetSpeedMultiplier(_currentMultiplier);
        }
    }


    private void EndGame()
    {
        _gameRunning = false;
        _timeRemaining = 0f;

        // Freeze all UFOs
        foreach (GameObject ufo in _activeUFOs)
        {
            if (ufo != null)
            {
                var mv = ufo.GetComponent<UFOMovement>();
                if (mv != null) mv.enabled = false;
            }
        }

        if (timerText != null)
            timerText.text = "TIME'S UP!";

        startButton.interactable = true;
    }


    private void WireButtonCallbacks()
    {
        startButton?.onClick.AddListener(StartGame);
        resetButton?.onClick.AddListener(ResetGame);
        slowButton?.onClick.AddListener(SetSpeedSlow);
        normalButton?.onClick.AddListener(SetSpeedNormal);
        fastButton?.onClick.AddListener(SetSpeedFast);
    }

    private void SetUIForIdle()
    {
        if (startButton != null) startButton.interactable = true;
        if (resetButton != null) resetButton.interactable = false;
        UpdateScoreDisplay();
        UpdateTimerDisplay();
        SetSpeedButtonLabels("Normal");
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
            scoreText.text = $"SCORE: {_score}";
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        if (gameDuration <= 0f)
        {
            timerText.text = "";
            return;
        }

        int seconds = Mathf.CeilToInt(Mathf.Max(0f, _timeRemaining));
        timerText.text = $"TIME: {seconds}";
    }

    private void SetSpeedButtonLabels(string active)
    {
        if (speedLabel != null)
            speedLabel.text = $"SPEED: {active.ToUpper()}";
    }
}
