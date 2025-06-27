using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using Sirenix.OdinInspector;

public class GameDataManager : Singleton<GameDataManager>
{
    public GameData gamedata;
    private void Awake()
    {
        if(Exists())
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
    public void NewGame()
    {
        Debug.Log("Load New Game Data");
        gamedata = new GameData();
    }

    public void SaveData()
    {
        string gameData = JsonUtility.ToJson(gamedata);
        string filePath = Path.Combine(Application.persistentDataPath, "GameData.json");
        File.WriteAllText(filePath, gameData);
        Debug.Log("DAta is save at: " + filePath + Environment.StackTrace);
    }

    public void LoadData()
    {
        string filePart = Path.Combine(Application.persistentDataPath, "GameData.json");
        Debug.Log("Load Data from: " + filePart);
        if (!File.Exists(filePart))
        {
            gamedata.InitNewGame();
            return;
        }
        string gameData = File.ReadAllText(filePart);

        gamedata = JsonUtility.FromJson<GameData>(gameData);
    }
#if UNITY_EDITOR
    [Button]
    public void OpenGameDataFile()
    {
        string path = Path.Combine(Application.persistentDataPath, "GameData.json");

        if (File.Exists(path))
        {
            // M? file b?ng ch??ng trình m?c ??nh (Notepad, VSCode, etc.)
            System.Diagnostics.Process.Start(path);
        }
        else
        {
            UnityEngine.Debug.LogWarning("File not found: " + path);
        }
    }
#endif
}
