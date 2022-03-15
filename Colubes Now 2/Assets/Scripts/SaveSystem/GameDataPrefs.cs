using UnityEngine;

public static class GameDataPrefs 
{
    public static void CreateData()
    {
        if (!PlayerPrefs.HasKey("Sound")) PlayerPrefs.SetInt("Sound", 0);
        if (!PlayerPrefs.HasKey("Music")) PlayerPrefs.SetInt("Music", 0);
        if (!PlayerPrefs.HasKey("MaxScore")) PlayerPrefs.SetInt("MaxScore", 0);
    }

    public static bool GetSoundStatus()
    {
        if (PlayerPrefs.HasKey("Sound"))
        {
            if (PlayerPrefs.GetInt("Sound") == 0) return false;
                else return true;
        }
        else
        {
            Debug.LogError("[GameDataPrefs] Sound data not found!");
            return false;
        }
    }

    public static bool GetMusicStatus()
    {
        if (PlayerPrefs.HasKey("Music"))
        {
            if (PlayerPrefs.GetInt("Music") == 0) return false;
                else return true;
        }
        else
        {
            Debug.LogError("[GameDataPrefs] Music data not found!");
            return false;
        }
    }

    public static int GetMaxScore()
    {
        if (PlayerPrefs.HasKey("MaxScore"))
        {
            return PlayerPrefs.GetInt("MaxScore");
        }
        else
        {
            Debug.LogError("[GameDataPrefs] MaxScore data not found!");
            return -1;
        }
    }

    public static void SetMusicStatus(bool newStatus)
    {
        int newRecord = 0;

        if (newStatus) newRecord = 1;
        PlayerPrefs.SetInt("Music", newRecord);
    }

    public static void SetSoundStatus(bool newStatus)
    {
        int newRecord = 0;

        if (newStatus) newRecord = 1;
        PlayerPrefs.SetInt("Sound", newRecord);
    }

    public static void SetMaxScore(int newScore)
    {
        PlayerPrefs.SetInt("MaxScore", newScore);
    }
}
