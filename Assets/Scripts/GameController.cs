using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // Status
    private int currentScore = 0;
    private int currentStage = 0;
    public int CurrentStage
    {
        get { return currentStage; }
    }

    // Objects for each stage
    public List<int> monsterNumbersForEachStage;
    public List<float> bonusTimesForEachStage;
    public List<int> baseScoresForEachStage;
    public List<GameObject> monstersForEachStage;
    private GameObject endZone;
    public Collider EndZoneCollider
    {
        get { return endZone.GetComponent<Collider>(); }
    }
    private GameObject startZone;
    public Collider StartZoneCollider
    {
        get { return startZone.GetComponent<Collider>(); }
    }
    public GameObject safeZone;

    // Player
    public GameObject playerPrefab;
    public Vector3 playerStartPosition;

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

    // UI for the main entry
    public Canvas mainEntryUI;
    private int highestScore = 0;

    // UI for in-game
    public Canvas inGameUI;
    public BonusTimeController bonusTimeController;
    public UnityEngine.UI.Text stageText;
    public UnityEngine.UI.Text scorePrefix;
    public UnityEngine.UI.Text scoreText;
    public UnityEngine.UI.Text timeText;
    public UnityEngine.UI.Text timeTitle;

    // UI for game over or all clear
    public Canvas gameOverUI;
    public UnityEngine.UI.Text gameOverTitle;
    public UnityEngine.UI.Text clearTitle;
    public UnityEngine.UI.Text finalScoreText;
    public UnityEngine.UI.Text highestScoreText;

    void Awake()
    {
        Screen.SetResolution(512, 512, false);
    }

    void Start()
    {
        ShowGameOverUI(false);
        ShowInGameUI(false);

        // MainEntry GUI
        mainEntryUI.enabled = true;
    }

    private void ShowGameOverUI(bool show)
    {
        gameOverTitle.enabled = show;
        clearTitle.enabled = show;
        gameOverUI.enabled = show;
    }

    private void ShowInGameUI(bool show)
    {
        inGameUI.enabled = show;
        stageText.enabled = show;
        scorePrefix.enabled = show;
        scoreText.enabled = show;
        bonusTimeController.enabled = show;
        timeText.enabled = show;
        timeTitle.enabled = show;
    }

    private GameObject CreateSafeZone()
    {
        float radius = this.safeZone.transform.localScale.z / 2f;
        float z;
        if (currentStage == 0)
            z = radius;
        else
            z = 100 * currentStage - radius;
        GameObject safeZone = Instantiate(this.safeZone,
            new Vector3(0f, -3.5f, z),
            Quaternion.identity) as GameObject;
        return safeZone;
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
        DestroyMonsters();

        currentStage += 1;
        UpdateStageText();

        endZone = CreateSafeZone();

        bonusTimeController.Reset(bonusTimesForEachStage[currentStage - 1]);

        SpawnMonsters(monstersForEachStage[currentStage - 1], monsterNumbersForEachStage[currentStage - 1]);
    }

    private void DestroyMonsters()
    {
        if (currentStage == 0)
            return;
        string tag = string.Format("MonsterOnStage{0}", currentStage);
        GameObject[] monsters = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject m in monsters)
            Destroy(m);
    }

    private void SpawnMonsters(GameObject monster, int number)
    {
        for (int i = 0; i < number; i++)
        {
            Monster m = monster.GetComponent<Monster>();
            Vector3 spawnPosition = new Vector3(
                Random.Range(m.minX, m.maxX),
                0,
                Random.Range(m.minZ, m.maxZ)
            );

            // Rotation default = none
            Quaternion spawnRotation = Quaternion.identity;
            Instantiate(monster, spawnPosition, spawnRotation);
        }
    }

    private int CalculateBonusScore(float playTime)
    {
        float remainingTime = bonusTimeController.GetTimeLeft();
        float bonusRate = (float)remainingTime / (float)bonusTimesForEachStage[currentStage - 1];
        int bonusScore = (int)((float)baseScoresForEachStage[currentStage - 1] * bonusRate);
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

    private void UpdateFinalScore()
    {
        finalScoreText.text = currentScore.ToString();

        if (currentScore >= highestScore)
        {
            highestScore = currentScore;
            highestScoreText.text = highestScore.ToString();
        }
    }

    public void GameOver(bool isCleared)
    {
        bonusTimeController.StopCountDown();
        UpdateFinalScore();
        gameOverTitle.enabled = !isCleared;
        clearTitle.enabled = isCleared;
        gameOverUI.enabled = true;
    }

    public void ClearStage()
    {
        if (currentStage < monstersForEachStage.Count)
        {
            bonusTimeController.StopCountDown();
            float remainingTime = bonusTimeController.GetTimeLeft();
            AddScore(baseScoresForEachStage[currentStage - 1]);
            AddScore(CalculateBonusScore(remainingTime));
            UpdateScoreText();
            StartNewStage();
        }
        else
            GameOver(true);
    }

    public void ResetGame()
    {
        LockUserInput();
        DestroyMonsters();
        GameObject[] safezones = GameObject.FindGameObjectsWithTag("SafeZone");
        foreach (GameObject safezone in safezones)
            Destroy(safezone);
        startZone = null;
        endZone = null;

        currentScore = 0;
        UpdateScoreText();

        currentStage = 0;
        StartNewStage();

        GameObject player = Instantiate(playerPrefab,
                                    playerStartPosition,
                                    Quaternion.identity);

        ShowGameOverUI(false);
        ShowInGameUI(true);
    }

    public void StartGame()
    {
        mainEntryUI.enabled = false;
        ResetGame();
    }
}
