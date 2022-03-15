using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameState : GameBaseState
{
    public RectTransform pauseMenu;
    public Button soundButton;
    public Button musicButton;

    [SerializeField] private Sprite[] buttonImages;
    [SerializeField] private GameObject[] cubes;
    [SerializeField] private LineRenderer targetLine;
    [SerializeField] private LineRenderer bezierCurve;
    [SerializeField] private GameObject intro;
    [SerializeField] private GameObject ad;

    public Color[] colors;
    public int[] cubesScore = new int[14];
    private string[] cubesName = new string[14];
    private GameObject[] colcubes;

    //cube throw timing
    private const float throwDeltaTime = .8f;
    private float runTime;

    //random cube bounds
    private const int cubeLowerBound = 0;
    private const int cubeTopBound = 6;

    //score table 
    public int score;
    public int maxCubeIndex;
    public TextMeshProUGUI scoreText;
    public Image nextCubeImage;
    public TextMeshProUGUI nextCubeText;
    private int nextRandomCube;
    private int maxScore;
    private int goalCubeIndex;
    public string goalCubeName;

    private const float cubeStartingPosX = 0f;
    private const float cubeStartingPosY = 0.5f;
    private const float cubeStartingPosZ = -8f;

    //player move 
    private float horizontalInput;
    private float playerPosX;
    private float playerPosZ;
    private const float xBounds = 2.5f;
    private const float zSpeed = 0.25f;
    private const float xSpeed = 10.0f;
    private const float playerAngle = -18.0f;
    private const float playerForwardForce = 70.0f;
    private GameObject controlledCube;
    private Transform controlledTransform;
    private bool isCubeUnderControl;

    //Audio variables
    public bool soundStatus;
    public bool musicStatus;

    //combos and Particles
    [SerializeField] private RectTransform[] floatingTexts;
    private int changeBackgroundPoint;

    //Ad
    private float adTime;
    private const float adRangeTime = 35.0f;
    private int adIndex;

    //GameStatus
    enum GameStatus { Null, Start, Goal, Play, Ad, Over, Complete };
    private GameStatus currentGameStatus;
    private GameStatus previousGameStatus;

    //GameSpeed
    enum GameSpeed { Stop, Regular, Slow };
    private bool isPaused;
    private GameSpeed currentGameSpeed;

    public bool endGameStatus;
    public bool savedGames;

#if UNITY_ANDROID

    private float halfScreenWidth;
    private float xStep;
    private bool touchControl;

#endif

    public override void Enter(GameBaseState from)
    {
        previousGameStatus = GameStatus.Null;
        currentGameStatus = GameStatus.Null;
        if (from.GetName() == "Loadout") savedGames = false;
        ChangeGameStatus(GameStatus.Start);
    }

    public override string GetName()
    {
        return "Game";
    }

    public override void Exit(GameBaseState to)
    {
        GameDataPrefs.SetMaxScore(maxScore);
        CleanTable();
        AudioManager.Instance.Stop("Game");
        gameObject.SetActive(false);
    }

    public override void Tick()
    {
        if (isPaused) return;

        switch (currentGameStatus)
        {
            case GameStatus.Goal:
                if (CheckTap()) ChangeGameStatus(GameStatus.Play);
                break;

            case GameStatus.Ad:
                if (CheckTap()) ChangeGameStatus(GameStatus.Play);
                break;

            case GameStatus.Play:
                if (IsThereCube("GameOver")) ChangeGameStatus(GameStatus.Over);
                if (IsThereCube("LastCube")) ChangeGameStatus(GameStatus.Complete);
                if ((!IsThereCube("Area")) && (Time.time - runTime > throwDeltaTime)) SpawnRandomCube();
                CheckDestroyedCubes();
                if (isCubeUnderControl) InputUserControl();
                break;
        }
    }

    private void StatusIn(GameStatus inStatus)
    {
        switch (inStatus)
        {
            case GameStatus.Start:
                InitializeData();
                InitializeUI();
                if (savedGames) InitializeOldTable();
                    else InitializeNewTable();
                DrawCurveLine();
                UpdateUI();
                Invoke("InitializeMusic", .5f);
                ChangeGameStatus(GameStatus.Goal);
                break;

            case GameStatus.Goal:
                ShowGoalCube();
                break;

            case GameStatus.Play:
                if (previousGameStatus == GameStatus.Goal)
                {
                    HideGoalCube();
                    adTime = Time.time;
                }                   
                if (previousGameStatus == GameStatus.Ad) HideAdCube();
                break;

            case GameStatus.Ad:
                ShowAdCube();
                break;

            case GameStatus.Over:
                endGameStatus = false;
                if (previousGameStatus == GameStatus.Goal) HideGoalCube();
                if (previousGameStatus == GameStatus.Ad) HideAdCube();
                gameManager.SwitchState("GameOver");
                break;

            case GameStatus.Complete:
                endGameStatus = true;
                LevelComplete();
                break;
        }
    }


    #region Start

    private void InitializeData()
    {
        for (int i = 0; i < cubes.Length; i++)
        {
            cubesName[i] = cubes[i].name + "(Clone)";
            cubesScore[i] = (int)Mathf.Pow(2, i);
        }

#if UNITY_ANDROID

        halfScreenWidth = Screen.width * 0.5f;
        xStep = xBounds / halfScreenWidth;
        touchControl = false;

#endif

        runTime = 0;
        score = 0;
        changeBackgroundPoint = 999;
        adIndex = 7;
        maxScore = GameDataPrefs.GetMaxScore();
        maxCubeIndex = -1;
        isCubeUnderControl = false;
        endGameStatus = false;
        isPaused = false;
        AdjustGameSpeed(GameSpeed.Regular);
        soundStatus = GameDataPrefs.GetSoundStatus();
        musicStatus = GameDataPrefs.GetMusicStatus();
    }

    private void InitializeUI()
    {
        if (soundStatus) soundButton.GetComponent<Button>().image.sprite = buttonImages[1];
            else soundButton.GetComponent<Button>().image.sprite = buttonImages[0];

        if (musicStatus) musicButton.GetComponent<Button>().image.sprite = buttonImages[3];
            else musicButton.GetComponent<Button>().image.sprite = buttonImages[2];

        gameObject.SetActive(true);
        pauseMenu.gameObject.SetActive(false);

        ChangeBackground changeBackgroundScript;
        changeBackgroundScript = GameObject.Find("Background").GetComponent<ChangeBackground>();
        changeBackgroundScript.Init();
    }

    private void InitializeNewTable()
    {
        GameObject cube;
        int nextCube;
        float x;
        float z = 6f;
        int[] table = new int[6];
        int count = 0;

        nextRandomCube = UnityEngine.Random.Range(cubeLowerBound, cubeTopBound);
        goalCubeIndex = UnityEngine.Random.Range(10, 14);
        goalCubeName = cubesName[goalCubeIndex];

        for (int i = 1; i < 3; i++)
        {
            x = -2.75f;
            for (int j = 1; j < 4; j++)
            {
                nextCube = UnityEngine.Random.Range(cubeLowerBound, cubeTopBound - 1);
                cube = Instantiate(cubes[nextCube], new Vector3(x, cubeStartingPosY, z), Quaternion.identity);
                table[count] = nextCube;
                count++;
                cube.tag = "Player";
                x +=2.75f;
            }
            z -= 4.0f;
        }

        GameData data = new GameData(nextRandomCube, goalCubeIndex, table);
        SaveSystem.Instance.SaveGameData(data);
    }

    private void InitializeOldTable()
    {
        GameObject cube;
        int nextCube;
        float x;
        float z = 6f;
        int count = 0;

        GameData data = SaveSystem.Instance.LoadGameData();

        nextRandomCube = data.firstCube;
        goalCubeIndex = data.goalCube;
        goalCubeName = cubesName[goalCubeIndex];

        for (int i = 1; i < 3; i++)
        {
            x = -2.75f;
            for (int j = 1; j < 4; j++)
            {
                nextCube = data.tableCubes[count];
                cube = Instantiate(cubes[nextCube], new Vector3(x, cubeStartingPosY, z), Quaternion.identity);
                count++;
                cube.tag = "Player";
                x += 2.75f;
            }
            z -= 4.0f;
        }
    }

    private void DrawCurveLine()
    {
        Vector3 point1;
        Vector3 point2;
        Vector3 point3;
        
        float vertexCount = 20;
        float posX;
        float posY;
        float posZ;
        float offsetX = 3.5f;
        float offsetY = 2.5f;

        var pointList = new List<Vector3>();

        posX = bezierCurve.transform.localPosition.x;
        posY = bezierCurve.transform.localPosition.y;
        posZ = bezierCurve.transform.localPosition.z;

        point1 = new Vector3(posX - offsetX, posY, posZ);
        point2 = new Vector3(posX, posY, posZ - offsetY);
        point3 = new Vector3(posX + offsetX, posY, posZ);

        bezierCurve.gameObject.SetActive(true);

        for (float ratio = 0; ratio <= 1; ratio += 1 / vertexCount)
        {
            var tangent1 = Vector3.Lerp(point1, point2, ratio);
            var tangent2 = Vector3.Lerp(point2, point3, ratio);
            var curve = Vector3.Lerp(tangent1, tangent2, ratio);
            pointList.Add(curve);
        }

        bezierCurve.positionCount = pointList.Count;
        bezierCurve.SetPositions(pointList.ToArray());
    }

    private void InitializeMusic()
    {
        if (musicStatus) AudioManager.Instance.Play("Game");
        else AudioManager.Instance.Stop("Game");
    }

    #endregion


    #region Goal

    private void ShowGoalCube()
    {
        GameObject cube = Instantiate(cubes[goalCubeIndex], new Vector3(0, 7, -3), Quaternion.identity);
        cube.tag = "Goal";
        cube.AddComponent<Rotator>();
        intro.gameObject.SetActive(true);
    }

    private void HideGoalCube()
    {
        GameObject cube = GameObject.FindGameObjectWithTag("Goal");
        Destroy(cube);
        intro.gameObject.SetActive(false);
    }

    #endregion


    #region Play

    private void CubeThrowFromArea()
    {
        Rigidbody rb;
        CubeController cubeControllerScript;

        //out off the control, set throw time, set cube status
        rb = controlledCube.GetComponent<Rigidbody>();
        cubeControllerScript = controlledCube.GetComponent<CubeController>();
        controlledCube.tag = "Player";
        cubeControllerScript.isThrowCube = true;
        cubeControllerScript.SetTrailEffect(true);
        runTime = Time.time;

        //force applied to cube
        if (soundStatus) AudioManager.Instance.PlaySound("Throw");
        controlledTransform.position = new Vector3(controlledTransform.position.x, 0, controlledTransform.position.z);
        rb.AddForce(controlledTransform.forward * playerForwardForce, ForceMode.Impulse);
        isCubeUnderControl = false;

        UpdateTargetLine();
    }

    private void CubeMoveOnAreaPC()
    {
        horizontalInput = Input.GetAxis("Horizontal");

        if (((horizontalInput > 0) && (playerPosX < xBounds)) || ((horizontalInput < 0) && (playerPosX > -xBounds)))
        {
            //playerýn x ve  pozisyonlarý hesaplanýp, güncelleniyor.
            playerPosX = controlledTransform.position.x + (horizontalInput * xSpeed * Time.deltaTime);
            controlledTransform.position = new Vector3(playerPosX, controlledTransform.position.y, playerPosZ + Mathf.Abs(playerPosX) * zSpeed);

            //playerýn rotasyonu hesaplanýp, güncelleniyor.
            controlledTransform.rotation = Quaternion.Euler(new Vector3(0, playerPosX * playerAngle, 0));

            //Creat Target Line
            UpdateTargetLine();
        }
    }

    /*
    private void GetTouchInputII()
    {
        if (Input.touchCount == 1)
        {
            Touch touch;
            touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchControl = true;
                    break;

                case TouchPhase.Moved:
                    playerPosX += xStep * touch.deltaPosition.x;
                    if (playerPosX < -xBounds) playerPosX = -xBounds;
                    if (playerPosX > xBounds) playerPosX = xBounds;
                    controlledTransform.position = new Vector3(playerPosX, controlledTransform.position.y, playerPosZ + Mathf.Abs(playerPosX) * zSpeed);
                    controlledTransform.rotation = Quaternion.Euler(new Vector3(0, playerPosX * playerAngle, 0));
                    UpdateTargetLine();
                    break;

                case TouchPhase.Ended:
                    if (touchControl) CubeThrowFromArea();
                    break;
            }
        }
    }

    private void GetTouchInputI()
    {
        if (Input.touchCount == 1)
        {

            Touch touch;
            touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchControl = true;
                    break;

                case TouchPhase.Moved:
                    playerPosX = xStep * (touch.position.x - halfScreenWidth);
                    controlledTransform.position = new Vector3(playerPosX, controlledTransform.position.y, playerPosZ + Mathf.Abs(playerPosX) * zSpeed);
                    controlledTransform.rotation = Quaternion.Euler(new Vector3(0, playerPosX * playerAngle, 0));
                    UpdateTargetLine();
                    break;

                case TouchPhase.Ended:
                    if (touchControl) CubeThrowFromArea();
                    break;
            }
        }
    }
    */

    private void InputUserControl()
    {

#if UNITY_EDITOR_WIN

        if (Input.GetKeyUp(KeyCode.Space)) CubeThrowFromArea();
        else CubeMoveOnAreaPC();

#else
				
        //GetTouchInputII();

#endif
    }

    private void CheckDestroyedCubes()
    {
        colcubes = GameObject.FindGameObjectsWithTag("Destroyed");

        if (colcubes.Length == 0) return;

        int cubeNum;
        int comboValue = -1;
        bool comboEffect = false;
        CubeController cubeControllerScript1;
        CubeController cubeControllerScript2;

        int cubeIndex1 = -1;
        int cubeIndex2 = -1;
        string collCubeName = string.Empty;
        bool collStatus = false;

        if ((colcubes.Length == 2) && (colcubes[0].name == colcubes[1].name))
        {
            cubeIndex1 = 0;
            cubeIndex2 = 1;
            collStatus = true;
            collCubeName = colcubes[0].name;
        }

        if (colcubes.Length == 3)
        {
            if (colcubes[0].name == colcubes[1].name)
            {
                cubeIndex1 = 0;
                cubeIndex2 = 1;
                collStatus = true;
                collCubeName = colcubes[0].name;
            }
            else if (colcubes[0].name == colcubes[2].name)
            {
                cubeIndex1 = 0;
                cubeIndex2 = 2;
                collStatus = true;
                collCubeName = colcubes[0].name;
            }
            else if (colcubes[1].name == colcubes[2].name)
            {
                cubeIndex1 = 1;
                cubeIndex2 = 2;
                collStatus = true;
                collCubeName = colcubes[1].name;
            }
        }

        if (collStatus)
        {
            cubeNum = Array.IndexOf(cubesName, collCubeName);
            cubeControllerScript1 = colcubes[cubeIndex1].gameObject.GetComponent<CubeController>();
            cubeControllerScript2 = colcubes[cubeIndex2].gameObject.GetComponent<CubeController>();

            float x = ((colcubes[cubeIndex1].transform.position.x) + (colcubes[cubeIndex2].transform.position.x)) * 0.5f;
            float z = ((colcubes[cubeIndex1].transform.position.z) + (colcubes[cubeIndex2].transform.position.z)) * 0.5f;

            if (cubeControllerScript1.isComboCube)
            {
                comboEffect = true;
                comboValue = cubeControllerScript1.comboAmount;
            }

            if (!comboEffect)
            {
                if (cubeControllerScript2.isComboCube)
                {
                    comboEffect = true;
                    comboValue = cubeControllerScript2.comboAmount;
                }
            }

            if (comboValue > 5) comboValue = 5;

            Destroy(colcubes[cubeIndex1]);
            Destroy(colcubes[cubeIndex2]);

            if (comboEffect)
            {
                Instantiate(floatingTexts[comboValue], new Vector3(x, cubeStartingPosY + 3.0f, z), Quaternion.identity);
                EffectManager.Instance.Play(4, new Vector3(x, cubeStartingPosY, z));
                if (soundStatus) AudioManager.Instance.PlaySound("Combo" + (comboValue + 1));
                if ((comboValue == 2) || (comboValue == 5))
                {
                    EffectManager.Instance.Play(1, new Vector3(x, cubeStartingPosY, z));
                    EffectManager.Instance.Play(3, new Vector3(x, cubeStartingPosY, z));
                }
            }
            else
            {
                if (soundStatus) AudioManager.Instance.PlaySound("Collision");
                EffectManager.Instance.Play(0, new Vector3(x, cubeStartingPosY, z));
            }

            GameObject newCube = Instantiate(cubes[cubeNum + 1], new Vector3(x, cubeStartingPosY + 1.0f, z), Quaternion.identity);
            newCube.tag = "New";

            cubeControllerScript1 = newCube.gameObject.GetComponent<CubeController>();
            if (comboEffect) cubeControllerScript1.comboAmount = comboValue + 1;
            else cubeControllerScript1.comboAmount = 0;

            UpdateScore(cubesScore[cubeNum + 1], cubeNum + 1);
            UpdateUI();
            //CheckAd();
        }
        else
        {
            for (int i = 0; i < colcubes.Length; i++)
            {
                colcubes[i].tag = "Player";
            }
        }
    }

    private void SpawnRandomCube()
    {
        controlledCube = Instantiate(cubes[nextRandomCube], new Vector3(cubeStartingPosX, cubeStartingPosY, cubeStartingPosZ), Quaternion.identity);
        controlledTransform = controlledCube.GetComponent<Transform>();
        playerPosX = controlledTransform.position.x;
        playerPosZ = controlledTransform.position.z;
        nextRandomCube = UnityEngine.Random.Range(cubeLowerBound, cubeTopBound);
        UpdateUI();
        isCubeUnderControl = true;
        UpdateTargetLine();
    }

    private bool IsThereCube(string tagName)
    {
        if (GameObject.FindGameObjectWithTag(tagName) == null) return false;
        else return true;
    }

    private void CheckAd()
    {
        float elapsedTime;

        if (maxCubeIndex == adIndex)
        {
            elapsedTime = Time.time - adTime;

            if (elapsedTime > adRangeTime) ChangeGameStatus(GameStatus.Ad);
                else adIndex = maxCubeIndex + 1;
        }
    }

    private void UpdateScore(int addScore, int newCubeIndex)
    {
        score += addScore;
        if (score > maxScore) maxScore = score;
        if (newCubeIndex > maxCubeIndex) maxCubeIndex = newCubeIndex;
        if (score > changeBackgroundPoint)
        {
            ChangeBackground changeBackgroundScript;
            changeBackgroundScript = GameObject.Find("Background").GetComponent<ChangeBackground>();
            if (soundStatus) AudioManager.Instance.PlaySound("Level");
            changeBackgroundScript.Change();
            if (changeBackgroundPoint < 5000)
            {
                changeBackgroundPoint += 1000;
            }
            else if (changeBackgroundPoint < 10000)
            {
                changeBackgroundPoint += 1500;
            }
            else
            {
                changeBackgroundPoint += 2000;
            }
        }
    }

    private void UpdateUI()
    {
        scoreText.text = score.ToString();
        nextCubeImage.color = new Color(colors[nextRandomCube].r, colors[nextRandomCube].g, colors[nextRandomCube].b);
        nextCubeText.text = cubesScore[nextRandomCube].ToString();
    }

    private void UpdateTargetLine()
    {
        Vector3 startPos;
        Vector3 endPos;
        float length = 7f;

        if (isCubeUnderControl)
        {
            startPos = controlledTransform.position + controlledTransform.forward;
            endPos = startPos + (controlledTransform.forward * length);

            targetLine.SetPosition(0, startPos);
            targetLine.SetPosition(1, endPos);
            targetLine.startColor = colors[Array.IndexOf(cubesName, controlledTransform.name)];

            targetLine.gameObject.SetActive(true);
        }
        else
        {
            targetLine.gameObject.SetActive(false);
        }
    }

    #endregion


    #region Ad

    private void ShowAdCube()
    {
        GameObject cube = Instantiate(cubes[adIndex], new Vector3(0, 7, -3), Quaternion.identity);
        cube.tag = "Ad";
        cube.AddComponent<Rotator>();
        ad.gameObject.SetActive(true);
        AdjustGameSpeed(GameSpeed.Stop);
        adIndex++;
    }

    private void HideAdCube()
    {
        GameObject cube = GameObject.FindGameObjectWithTag("Ad");
        Destroy(cube);
        ad.gameObject.SetActive(false);
        AdjustGameSpeed(GameSpeed.Regular);
        adTime = Time.time;
    }

    #endregion


    #region Exit

    private void CleanTable()
    {
        GameObject[] tmp;

        tmp = GameObject.FindGameObjectsWithTag("GameOver");
        for (int i = 0; i < tmp.Length; i++)
        {
            Destroy(tmp[i]);
        }

        tmp = GameObject.FindGameObjectsWithTag("Destroyed");
        for (int i = 0; i < tmp.Length; i++)
        {
            Destroy(tmp[i]);
        }

        tmp = GameObject.FindGameObjectsWithTag("New");
        for (int i = 0; i < tmp.Length; i++)
        {
            Destroy(tmp[i]);
        }

        tmp = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < tmp.Length; i++)
        {
            Destroy(tmp[i]);
        }

        tmp = GameObject.FindGameObjectsWithTag("Area");
        for (int i = 0; i < tmp.Length; i++)
        {
            Destroy(tmp[i]);
        }

        tmp = GameObject.FindGameObjectsWithTag("LastCube");
        for (int i = 0; i < tmp.Length; i++)
        {
            Destroy(tmp[i]);
        }

        targetLine.gameObject.SetActive(false);
    }

    public void QuitTable()
    {
        if (soundStatus) AudioManager.Instance.PlaySound("Button");
        Pause();
        ChangeGameStatus(GameStatus.Over);
    }

    #endregion


    #region Complete

    private void LevelComplete()
    {
        GameObject lastCube = GameObject.FindGameObjectWithTag("LastCube");
        AdjustGameSpeed(GameSpeed.Slow);
        if (soundStatus) AudioManager.Instance.PlaySound("Level");
        EffectManager.Instance.Play(1, new Vector3(lastCube.transform.position.x, cubeStartingPosY, lastCube.transform.position.z));
        EffectManager.Instance.Play(3, new Vector3(lastCube.transform.position.x, cubeStartingPosY, lastCube.transform.position.z));
        lastCube.transform.localScale *= 2;
        Invoke("EndLevel", 1.25f);
    }

    private void EndLevel()
    {
        AdjustGameSpeed(GameSpeed.Regular);
        gameManager.SwitchState("GameOver");
    }

    #endregion


    #region Util

    private bool CheckTap()
    {
        bool result = false;

#if UNITY_EDITOR_WIN

        if (Input.GetKeyUp(KeyCode.Space)) result = true;

#else

        if (Input.touchCount == 1)
        {
            Touch touch;
            touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    //touchControl = true;
                    break;

                case TouchPhase.Ended:
                    //if (touchControl) result = true;
                    break;
            }
        }

#endif

        return result;
    }

    private void ChangeGameStatus(GameStatus newStatus)
    {
        previousGameStatus = currentGameStatus;
        currentGameStatus = newStatus;
        StatusIn(currentGameStatus);
    }

    private void AdjustGameSpeed(GameSpeed newSpeed)
    {
        float speed = 0;
        currentGameSpeed = newSpeed;

        switch (currentGameSpeed)
        {
            case GameSpeed.Stop:
                speed = 0;
                break;

            case GameSpeed.Slow:
                speed = .4f;
                break;

            case GameSpeed.Regular:
                speed = 1;
                break;
        }
        Time.timeScale = speed;
    }

    #endregion


    #region Pause

    public void Pause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            GameDataPrefs.SetMaxScore(maxScore);
            AudioListener.pause = true;
            AdjustGameSpeed(GameSpeed.Stop);
            pauseMenu.gameObject.SetActive(true);
            //touchControl = false;
        }
        else
        {
            AudioListener.pause = false;
            if (currentGameStatus != GameStatus.Ad) AdjustGameSpeed(GameSpeed.Regular);
            pauseMenu.gameObject.SetActive(false);
            //touchControl = false;
        }
    }

    public void SwitchAudioButtons(string buttonName)
    {
        if (buttonName == "Music")
        {
            musicStatus = !musicStatus;
            GameDataPrefs.SetMusicStatus(musicStatus);

            if (musicStatus) musicButton.GetComponent<Button>().image.sprite = buttonImages[3];
            else musicButton.GetComponent<Button>().image.sprite = buttonImages[2];
        }
        else
        {
            soundStatus = !soundStatus;
            GameDataPrefs.SetSoundStatus(soundStatus);

            if (soundStatus) soundButton.GetComponent<Button>().image.sprite = buttonImages[1];
            else soundButton.GetComponent<Button>().image.sprite = buttonImages[0];
        }
        InitializeMusic();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause) Pause();
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus) Pause();
    }

    #endregion

}