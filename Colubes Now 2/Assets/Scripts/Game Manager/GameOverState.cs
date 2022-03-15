using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverState : GameBaseState
{
    private bool soundStatus;
    private bool musicStatus;
    
    public GameState gameStateScript;
    public Text scoreTextValue;
    public Text maxScoreTextValue;

    public override void Enter(GameBaseState from)
    {
        InitializeData();
        InitializeUI();
        Invoke("InitializeMusic", .5f);
    }

    private void InitializeData()
    {
        soundStatus = GameDataPrefs.GetSoundStatus();
        musicStatus = GameDataPrefs.GetMusicStatus();
    }

    private void InitializeUI()
    {
        int gameScore;
        ChangeBackground changeBackgroundScript;

        changeBackgroundScript = GameObject.Find("Background").GetComponent<ChangeBackground>();
        changeBackgroundScript.End();

        gameScore = gameStateScript.score;

        gameObject.SetActive(true);

        scoreTextValue.text = gameScore.ToString();
        maxScoreTextValue.text = GameDataPrefs.GetMaxScore().ToString();

        if (gameStateScript.endGameStatus)
        {
            Invoke("MakeEffext", 1f);
            Invoke("MakeEffext", 3f);
            Invoke("MakeEffext", 5f);
        }
    }

    private void MakeEffext()
    {
        EffectManager.Instance.Play(2, new Vector3(0, 12, 0));
        EffectManager.Instance.Play(5, new Vector3(0, 9, 5));
        EffectManager.Instance.Play(3, new Vector3(-3, 7, 10));
        EffectManager.Instance.Play(6, new Vector3(0, 2, 5));
    }

    private void InitializeMusic()
    {
        if (soundStatus)
        {
            if (gameStateScript.endGameStatus) AudioManager.Instance.PlaySound("LevelComplete");
                else AudioManager.Instance.PlaySound("GameOver");
        }
    }

    public override void Exit(GameBaseState to)
    {
        if (gameStateScript.endGameStatus) AudioManager.Instance.Stop("LevelComplete");
            else AudioManager.Instance.Stop("GameOver");
        gameObject.SetActive(false);
    }

    public override string GetName()
    {
        return "GameOver";
    }

    public override void Tick()
    {
    }

    public void ButtonPlayGame()
    {
        if (soundStatus) AudioManager.Instance.PlaySound("Button");
        gameStateScript.savedGames = false;
        gameManager.SwitchState("Game");
    }

    public void ButtonRestartGame()
    {
        if (soundStatus) AudioManager.Instance.PlaySound("Button");
        gameStateScript.savedGames = true;
        gameManager.SwitchState("Game");
    }

    public void ButtonHome()
    {
        if (soundStatus) AudioManager.Instance.PlaySound("Button");
        gameManager.SwitchState("Loadout");
    }
}
