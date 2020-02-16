﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject player;
    public Vector3 playerStartPosition;
    public List<GameObject> enemyList;
    public List<int> enemyCountList;
    public List<float> stageLimitTimeList;
    public List<int> stageScoreList;
    private int currentStage = 0;
    private GameObject endZone;
    private GameObject startZone;
    public GameObject safeZoneObj;
    public int currentScore = 0;
    private UnityEngine.UI.Text scorePrefix;
    private UnityEngine.UI.Text scoreText;
    private UnityEngine.UI.Text stageText;
    private UnityEngine.UI.Text gameOverTitle;
    private UnityEngine.UI.Text clearTitle;
    private UnityEngine.UI.Text timeTitle;
    private UnityEngine.UI.Text timeText;
    public Canvas gameOverGUI;
    public Canvas gameStatusGUI;
    public Canvas mainEntryGUI;

    // Locker for user input
    private bool isUserInputLocked;
    public bool IsUserInputLocked
    {
        get { return isUserInputLocked; }
    }
    public void LockUserInput()
    {
        isUserInputLocked = true;
    }
    public void UnLockUserInput()
    {
        isUserInputLocked = false;
    }

    private int highestScore = 0;

    public BonusTimeController bonusTimeController;

    void Awake()
    {
        Screen.SetResolution(512, 512, false);
    }

    void Start()
    {
        // GameStatus GUI
        stageText = GameObject.FindGameObjectWithTag("StageText").
            GetComponent<UnityEngine.UI.Text>();
        scorePrefix = GameObject.FindGameObjectWithTag("ScorePrefix").
            GetComponent<UnityEngine.UI.Text>();
        scoreText = GameObject.FindGameObjectWithTag("ScoreText").
            GetComponent<UnityEngine.UI.Text>();
        timeText = GameObject.FindGameObjectWithTag("TimeText").
            GetComponent<UnityEngine.UI.Text>();
        timeTitle = GameObject.FindGameObjectWithTag("TimeTitle").
            GetComponent<UnityEngine.UI.Text>();
        // GameOver GUI
        gameOverTitle = GameObject.FindGameObjectWithTag("GameOverTitle").
            GetComponent<UnityEngine.UI.Text>();
        clearTitle = GameObject.FindGameObjectWithTag("ClearTitle").
            GetComponent<UnityEngine.UI.Text>();

        // MainEntry GUI
        mainEntryGUI.enabled = true;

        HideAllGameOverGUI();
        SetAllGameStatusGUI(false);
    }

    private void HideAllGameOverGUI()
    {
        gameOverTitle.enabled = false;
        clearTitle.enabled = false;
        gameOverGUI.enabled = false;
    }

    private void SetAllGameStatusGUI(bool show)
    {
        gameStatusGUI.enabled = show;
        stageText.enabled = show;
        scorePrefix.enabled = show;
        scoreText.enabled = show;
        bonusTimeController.enabled = show;
        timeText.enabled = show;
        timeTitle.enabled = show;
    }

    GameObject CreateSafeZone()
    {
        float radius = safeZoneObj.transform.localScale.z / 2f;
        float z;
        if (currentStage == 0)
            z = radius;
        else
            z = 100 * currentStage - radius;
        GameObject safeZone = Instantiate(safeZoneObj,
            new Vector3(0f, -3.5f, z),
            Quaternion.identity) as GameObject;
        return safeZone;
    }

    public Collider GetCurrentEndZoneCollider()
    {
        return endZone.GetComponent<Collider>();
    }

    public Collider GetCurrentStartZoneCollider()
    {
        return startZone.GetComponent<Collider>();
    }

    void StartNewStage()
    {
        if (startZone == null)
            startZone = CreateSafeZone();
        else
        {
            Destroy(startZone);
            startZone = endZone;
        }

        DestroyEnemies();
        currentStage += 1;
        UpdateStageText();

        endZone = CreateSafeZone();

        for (int i = 0; i < enemyCountList[currentStage - 1]; i++)
            SpawnOneEnemy(enemyList[currentStage - 1]);
        bonusTimeController.Reset(bonusTimesForEachStage[currentStage - 1]);
    }

    private void DestroyEnemies()
    {
        if (currentStage == 0)
            return;
        string enemyTag = string.Format("Stage{0}Enemy", currentStage);
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        foreach (GameObject enemy in enemies)
            Destroy(enemy);
    }

    private void SpawnOneEnemy(GameObject enemy)
    {
        Enemy moveInfo = enemy.GetComponent<Enemy>();
        Vector3 spawnPosition = new Vector3(
            Random.Range(moveInfo.minX, moveInfo.maxX),
            0,
            Random.Range(moveInfo.minZ, moveInfo.maxZ)
        );

        // rotation default = none
        Quaternion spawnRotation = Quaternion.identity;
        Instantiate(enemy, spawnPosition, spawnRotation);
    }

    public int GetCurrentStage()
    {
        return currentStage;
    }

    private int GetBonusScore(float playTime)
    {
        float remainingTime = bonusTimeController.GetTimeLeft();
        float bonusRate = (float)remainingTime / (float)stageLimitTimeList[currentStage - 1];
        int bonusScore = (int)((float)stageScoreList[currentStage - 1] * bonusRate);
        return bonusScore;
    }

    private void AddScore(int score)
    {
        currentScore += score;
    }

    private void UpdateScoreText()
    {
        scoreText.text = currentScore.ToString();
    }

    private void UpdateStageText()
    {
        stageText.text = "Stage " + currentStage;
    }

    public void UpdateFinalScore()
    {
        UnityEngine.UI.Text currentScoreText = GameObject.FindGameObjectWithTag("CurrentScore")
                    .GetComponent<UnityEngine.UI.Text>();
        currentScoreText.text = currentScore.ToString();

        if (currentScore >= highestScore)
        {
            highestScore = currentScore;
            UnityEngine.UI.Text highestScoreText = GameObject.FindGameObjectWithTag("HighestScore")
                    .GetComponent<UnityEngine.UI.Text>();
            highestScoreText.text = highestScore.ToString();
        }
    }

    public void GameOver(bool isCleared)
    {
        bonusTimeController.StopCountDown();
        UpdateFinalScore();
        gameOverTitle.enabled = !isCleared;
        clearTitle.enabled = isCleared;
        gameOverGUI.enabled = true;
    }

    public void ClearStage()
    {
        if (currentStage < enemyList.Count)
        {
            bonusTimeController.StopCountDown();
            float remainingTime = bonusTimeController.GetTimeLeft();
            AddScore(stageScoreList[currentStage - 1]);
            AddScore(GetBonusScore(remainingTime));
            UpdateScoreText();
            StartNewStage();
        }
        else
            GameOver(true);
    }

    public void ResetGame()
    {
        LockUserInput();
        DestroyEnemies();
        GameObject[] safezones = GameObject.FindGameObjectsWithTag("SafeZone");
        foreach (GameObject safezone in safezones)
            Destroy(safezone);
        startZone = null;
        endZone = null;

        currentScore = 0;
        UpdateScoreText();

        currentStage = 0;
        StartNewStage();

        player = Instantiate(playerPrefab,
                playerStartPosition,
                Quaternion.identity
        );

        HideAllGameOverGUI();
        SetAllGameStatusGUI(true);
    }

    public void StartGame()
    {
        mainEntryGUI.enabled = false;
        ResetGame();
    }
}
