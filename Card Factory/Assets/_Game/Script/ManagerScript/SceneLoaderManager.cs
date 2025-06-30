using UnityEngine;

public class SceneLoaderManager : MonoBehaviour
{
    void Start()
    {
        if(GameManager.Ins.isFirstTime)
        {
            GameManager.Ins.OnChangeGameState(GameManager.GameState.GamePlay);
        }
        else
        {
            GameManager.Ins.OnChangeGameState(GameManager.GameState.MainMenu);
        }
    }
}
