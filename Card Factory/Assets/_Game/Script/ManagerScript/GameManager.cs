using Lean.Pool;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public enum GameState
    {
        MainMenu,
        GamePlay,
        WinGame,
        GameOver,
    }

    public GameState state;
    public QueueManager QueueManager;
    public ConveyorManager ConveyorManager;
    public BoosterManager BoosterManager;
    public GameDataManager gameData;
    public ObjectPoolManager poolManager;

    public GoldConfig rewardConfig;
    public BoosterConfig boosterConfig;
    public FeatureUnlockConfig FeatureUnlockConfig;
    public FeatureUnlockData currentFeatureUnlock;
    private int currentFeatureid;
    public int currentGold;

    public int currentLevel;

    [Header("=== HeartSystem===")]
    [SerializeField] private int maxheart;
    [SerializeField] private float heartRefillTime;
    [SerializeField] private int currentHeart;
    private bool hasInfiteHeart;
    private long lastHeartRefillTimestamp;

    private bool isInit = false;

    public bool isPause => Time.timeScale == 0f;

    public bool isFirstTime;


    private void Awake()
    {
        if(Exists())
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void Start()
    {
        AudioManager.Ins.InitializeAudioManager();
        AudioManager.Ins.PlayMusic("BackGroundMenu");
        EventManager.OnLevelComplete.AddListener(CheckFeatureUnlock);
        if(isFirstTime)
        {
            OnChangeGameState(GameState.GamePlay);
        }
    }

    private void Update()
    {
        if (!hasInfiteHeart && state == GameState.MainMenu)
        {
            UpdateHeart();
        }
    }

    public void InitGame()
    {
        Time.timeScale = 1.0f;
        BoosterManager.addConveyCount = gameData.gamedata.GetBoosterData(BoosterType.AddConvey);
        BoosterManager.swapCount = gameData.gamedata.GetBoosterData(BoosterType.Swap);
        BoosterManager.removePackCount = gameData.gamedata.GetBoosterData(BoosterType.RemovePack);
        LevelManager.Ins.InitLevel();
        BoosterManager.InitBooster();
        UIManager.Ins.InitUI();
        isInit = true;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void OnChangeGameState(GameState gameState)
    {
        state = gameState;
        if (gameState == GameState.MainMenu)
        {
            SceneManager.LoadScene(0);
            AudioManager.Ins.StopSound("BackGroundInGameMusic");
            AudioManager.Ins.PlayMusic("BackGroundMenu");

        }
        else if (gameState == GameState.GamePlay)
        {
            SceneManager.LoadSceneAsync(1);
            AudioManager.Ins.PlayMusic("BackGroundInGameMusic");
            AudioManager.Ins.StopSound("BackGroundMenu");
        }
    }
    public bool IsPointerOverUIObject(params string[] tagsToIgnore)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        foreach (var result in results)
        {
            if (!System.Array.Exists(tagsToIgnore, tag => result.gameObject.CompareTag(tag)))
            {
                return true; // Pointer over a UI that is not in the ignore list
            }
        }
        return results.Count > 0;
    }

    public void SaveData()
    {
        gameData.SaveData();
    }

    public void GetData()
    {
        gameData.LoadData();
        currentGold = gameData.gamedata.currentCoin;
        currentLevel = gameData.gamedata.currentLevel;
        isFirstTime = gameData.gamedata.isFirstTime;
        currentFeatureid = gameData.gamedata.currentFeatureId;
        currentFeatureUnlock = FeatureUnlockConfig.GetFeatureData(currentFeatureid);
    }

    public void CheckFeatureUnlock()
    {
        if (currentLevel == currentFeatureUnlock.levelUnlock)
        {
            currentFeatureid++;
            gameData.gamedata.currentFeatureId = currentFeatureid;
            SaveData();
        }
    }

    public float GetFeatureProgress(int levelCheck)
    {
        int levelToGet = 0;
        if (currentFeatureUnlock == null)
        {
            levelToGet = 9999999;
        }
        else if(currentFeatureid == 1)
        {
            levelToGet = currentFeatureUnlock.levelUnlock;
        }
        else
        {
            levelToGet = currentFeatureUnlock.levelUnlock - FeatureUnlockConfig.GetFeatureData(currentFeatureid - 1).levelUnlock;
        }
        return (float)(levelCheck) / (float)levelToGet;
    }

    public void OnUpdateCoin(int amount)
    {
        currentGold += amount;
        gameData.gamedata.currentCoin = currentGold;
        SaveData();
        EventManager.OnCoinchange.Invoke(currentGold);
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainGamePlay")
        {
            BoosterManager = GameObject.FindAnyObjectByType<BoosterManager>();
            poolManager = GameObject.FindAnyObjectByType<ObjectPoolManager>();
            QueueManager = GameObject.FindAnyObjectByType<QueueManager>();
            TutorialInGameManager.Ins.currentTutIndex = gameData.gamedata.currentTutIndex;
            TutorialInGameManager.Ins.completeList = gameData.gamedata.tutorialComplete;
            InitGame();
        }
        if(scene.name == "MainMenu")
        {
            GetData();
            LoadHeartData();
            MainMenuManager.Ins.InitMenuUI();
        }
    }

    private void UpdateHeart()
    {
        if (hasInfiteHeart)
        {
            return;
        }

        if (IsHeartsFull())
        {
            return;
        }
        long currentTime = GetCurrentUnixTimestamp();
        long elapsedSeconds = currentTime - lastHeartRefillTimestamp;

        int heartToAdd = Mathf.FloorToInt(elapsedSeconds / (heartRefillTime * 60f));

        if (heartToAdd > 0)
        {
            int oldHeart = currentHeart;
            currentHeart = Mathf.Min(currentHeart + heartToAdd, Maxheart);

            lastHeartRefillTimestamp += (long)(heartToAdd * heartRefillTime * 60f);

            SaveHeartData();

            if(isInit)
            {
                EventManager.onHeartChange?.Invoke();
            }
        }
    }

    public bool SpendHeart()
    {
        if (hasInfiteHeart)
        {
            Debug.Log("Used heart (infinite mode)");
            return true;
        }

        if (currentHeart <= 0)
        {
            Debug.Log("Cannot spend heart - no hearts available");
            return false;
        }

        currentHeart--;

        if (currentHeart < Maxheart)
        {
            lastHeartRefillTimestamp = GetCurrentUnixTimestamp();
        }

        SaveHeartData();
        EventManager.onHeartChange?.Invoke();

        Debug.Log($"Heart spent. Current: {currentHeart}/{Maxheart}");
        return true;
    }

    public void AddHeart(int amount)
    {
        if (hasInfiteHeart || amount <= 0)
            return;

        int oldHeart = currentHeart;
        currentHeart = Mathf.Min(currentHeart + amount, Maxheart);

        if (currentHeart == Maxheart)
        {
            lastHeartRefillTimestamp = GetCurrentUnixTimestamp();
        }

        SaveHeartData();
        EventManager.onHeartChange?.Invoke();

#if UNITY_EDITOR
        Debug.Log($"Hearts added: {currentHeart - oldHeart}. Current: {currentHeart}/{Maxheart}");
#endif
    }

    public void RefillHearts()
    {
        if (hasInfiteHeart)
            return;

        currentHeart = Maxheart;
        lastHeartRefillTimestamp = GetCurrentUnixTimestamp();


        SaveHeartData();
        EventManager.onHeartChange?.Invoke();

#if UNITY_EDITOR
        Debug.Log("Hearts refilled to maximum");
#endif
    }

    public void SetInfiniteHearts(bool enable)
    {
        hasInfiteHeart = enable;

        EventManager.onHeartChange?.Invoke();
        Debug.Log($"Infinite hearts: {enable}");
    }

    public int GetCurrentHearts()
    {
        return hasInfiteHeart ? int.MaxValue : currentHeart;
    }


    public TimeSpan GetTimeUntilNextHeart()
    {
        if (hasInfiteHeart || IsHeartsFull())
            return TimeSpan.Zero;

        long currentTime = GetCurrentUnixTimestamp();
        float secondsSinceLastRefill = currentTime - lastHeartRefillTimestamp;
        float secondsPerHeart = heartRefillTime * 60f;

        long secondsUntilNext = (long)(secondsPerHeart - (secondsSinceLastRefill % secondsPerHeart));

        return TimeSpan.FromSeconds(Mathf.Max(0, secondsUntilNext));
    }


    public bool IsHeartsFull()
    {
        return currentHeart >= Maxheart || hasInfiteHeart;
    }


    public bool CanPlay()
    {
        return hasInfiteHeart || currentHeart > 0;
    }


    private long GetCurrentUnixTimestamp()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    private void SaveHeartData()
    {
        if (GameDataManager.Ins?.gamedata != null)
        {
            GameDataManager.Ins.gamedata.currentHeart = currentHeart;
            GameDataManager.Ins.gamedata.lastHeartRefillTimestamp = lastHeartRefillTimestamp;
            GameDataManager.Ins.SaveData();
        }
    }

    private void LoadHeartData()
    {
        if (GameDataManager.Ins?.gamedata != null)
        {
            currentHeart = GameDataManager.Ins.gamedata.currentHeart;
            lastHeartRefillTimestamp = GameDataManager.Ins.gamedata.lastHeartRefillTimestamp;
            currentHeart = Mathf.Clamp(currentHeart, 0, Maxheart);

            if (lastHeartRefillTimestamp == 0 && currentHeart < Maxheart)
            {
                lastHeartRefillTimestamp = GetCurrentUnixTimestamp();
                SaveHeartData();
            }
        }
        else
        {
            currentHeart = Maxheart;
            lastHeartRefillTimestamp = GetCurrentUnixTimestamp();
        }
    }

    void OnApplicationQuit()
    {
        SaveData();
    }
    bool isPaused = false;

    public int Maxheart { get => maxheart; set => maxheart = value; }

    void OnApplicationFocus(bool hasFocus)
    {
    }

    void OnApplicationPause(bool pauseStatus)
    {
        isPaused = pauseStatus;
        if (isPaused)
        {
            SaveData();
        }
    }

#if UNITY_EDITOR
    [Button]
    public void RestoreFullHeart()
    {
        currentHeart = maxheart;
    }
#endif
}
