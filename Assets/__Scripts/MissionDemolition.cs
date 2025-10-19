using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum GameMode
{
    idle,
    playing,
    levelEnd
}

public class MissionDemolition : MonoBehaviour
{
    static private MissionDemolition S; // Singleton

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TMP_Text finalScoreText;
    public TMP_Text bestScoreText;

    [Header("HUD")]
    public TMP_Text hudBestScoreText; // always-visible HUD "Lowest Score"

    [Header("Scoring")]
    public int totalShots; // sum across all levels

    [Header("Inscribed")]
    public TMP_Text uitLevel; // The UIText_Level Text
    public TMP_Text uitShots; // The UIText_Shots Text
    public Vector3 castlePos; // The place to put castles
    public GameObject[] castles; // An array of the castles

    [Header("Dyanmic")]
    public int level; // The current level
    public int levelMax; // The number of levels
    public int shotsTaken;
    public GameObject castle; // The current castle
    public GameMode mode = GameMode.idle;
    public string showing = "Show Slingshot"; // FollowCam mode

    const string BEST_KEY = "BestShots";

    void SanitizeBestShots()
    {
        int best = PlayerPrefs.GetInt(BEST_KEY, int.MaxValue);
        if (best == 0)
        {
            PlayerPrefs.DeleteKey(BEST_KEY);
            PlayerPrefs.Save();
        }
    }

    void Awake()
    {
        S = this; // set singleton early
    }

    void Start()
    {
        level = 0;
        shotsTaken = 0;
        levelMax = castles.Length;
        totalShots = 0;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        SanitizeBestShots();
        UpdateBestUI();

        StartLevel();
    }

    void StartLevel()
    {
        // Remove old castle
        if (castle != null) Destroy(castle);

        // Remove old projectiles & any stray trails
        Projectile.DESTROY_PROJECTILES();
        foreach (var pl in FindObjectsOfType<ProjectileLine>()) Destroy(pl.gameObject);

        // Spawn new castle
        castle = Instantiate(castles[level]);
        castle.transform.position = castlePos;

        // Reset goal and UI
        Goal.goalMet = false;
        UpdateGUI();

        mode = GameMode.playing;

        // Show both
        FollowCam.SWITCH_VIEW(FollowCam.eView.both);
    }

    void UpdateGUI()
    {
        if (uitLevel) uitLevel.text = "Level: " + (level + 1) + " of " + levelMax;
        if (uitShots) uitShots.text = "Shots Taken: " + shotsTaken;
    }

    void Update()
    {
        UpdateGUI();

        // Level end
        if (mode == GameMode.playing && Goal.goalMet)
        {
            mode = GameMode.levelEnd;
            FollowCam.SWITCH_VIEW(FollowCam.eView.both);
            Invoke(nameof(NextLevel), 2f);
        }
    }

    void NextLevel()
    {
        level++;
        if (level == levelMax)
        {
            ShowGameOver();
            return;
        }
        shotsTaken = 0;
        StartLevel();
    }

    // Called by Slingshot on release
    static public void SHOT_FIRED()
    {
        if (S == null) return; // tiny guard; Awake sets S
        S.shotsTaken++;
        S.totalShots++;
    }

    static public GameObject GET_CASTLE() => S.castle;

    void ShowGameOver()
    {
        int best = PlayerPrefs.GetInt(BEST_KEY, int.MaxValue);

        // Save only if we actually took shots and beat the record
        if (totalShots > 0 && totalShots < best)
        {
            best = totalShots;
            PlayerPrefs.SetInt(BEST_KEY, best);
            PlayerPrefs.Save();
        }

        if (finalScoreText) finalScoreText.text = $"Total Shots: {totalShots}";
        if (bestScoreText)  bestScoreText.text  = (best == int.MaxValue) ? "Lowest Score: —" : $"Lowest Score: {best}";

        if (hudBestScoreText) hudBestScoreText.gameObject.SetActive(false);
        if (gameOverPanel)    gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        level = 0;
        shotsTaken = 0;
        totalShots = 0;

        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (hudBestScoreText) hudBestScoreText.gameObject.SetActive(true);

        StartLevel();
        SanitizeBestShots();
        UpdateBestUI();
    }

    void UpdateBestUI()
    {
        int best = PlayerPrefs.GetInt(BEST_KEY, int.MaxValue);
        string msg = (best == int.MaxValue) ? "Lowest Score: —" : $"Lowest Score: {best}";
        if (bestScoreText)    bestScoreText.text    = msg; // panel label (ok even if hidden)
        if (hudBestScoreText) hudBestScoreText.text = msg; // HUD label
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;

        if (gameOverPanel) gameOverPanel.SetActive(false);

        // Reset this level’s shots from the run total
        totalShots -= shotsTaken;
        if (totalShots < 0) totalShots = 0;
        shotsTaken = 0;

        // Ensure bands are hidden
        var sling = FindObjectOfType<Slingshot>();
        if (sling != null)
        {
            if (sling.lrFront) sling.lrFront.enabled = false;
            if (sling.lrBack)  sling.lrBack.enabled  = false;
        }

        StartLevel();
        UpdateBestUI();
    }
}
