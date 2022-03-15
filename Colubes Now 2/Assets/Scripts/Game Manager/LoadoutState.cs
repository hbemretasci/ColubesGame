using UnityEngine;
using UnityEngine.UI;

public class LoadoutState : GameBaseState
{
    public Image soundImage;
    public Image musicImage;

    [SerializeField] Sprite[] buttonImages;
    private bool soundStatus;
    private bool musicStatus;

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
        if (soundStatus) soundImage.GetComponent<Image>().sprite = buttonImages[1];
            else soundImage.GetComponent<Image>().sprite = buttonImages[0];

        if (musicStatus) musicImage.GetComponent<Image>().sprite = buttonImages[3];
            else musicImage.GetComponent<Image>().sprite = buttonImages[2];

        gameObject.SetActive(true);
    }

    private void InitializeMusic()
    {
        if (musicStatus) AudioManager.Instance.Play("Menu");
            else AudioManager.Instance.Stop("Menu");
    }

    public override void Exit(GameBaseState to)
    {
        AudioManager.Instance.Stop("Menu");
        gameObject.SetActive(false);
    }

    public override string GetName()
    {
        return "Loadout";
    }

    public override void Tick()
    {
    }

    public void SwitchAudioButtons(string buttonName)
    {
        if (buttonName == "Music")
        {
            musicStatus = !musicStatus;
            GameDataPrefs.SetMusicStatus(musicStatus);

            if (musicStatus) musicImage.GetComponent<Image>().sprite = buttonImages[3];
                else musicImage.GetComponent<Image>().sprite = buttonImages[2];

            InitializeMusic();
        }
        else
        {
            soundStatus = !soundStatus;
            GameDataPrefs.SetSoundStatus(soundStatus);

            if (soundStatus)
            {
                soundImage.GetComponent<Image>().sprite = buttonImages[1];
                AudioManager.Instance.PlaySound("Button");
            }
            else
            {
                soundImage.GetComponent<Image>().sprite = buttonImages[0];
            }
        }
    }

    public void ButtonStartGame()
    {
        if (soundStatus) AudioManager.Instance.PlaySound("Button");
        gameManager.SwitchState("Game");
    }
}