using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public enum GameMode
{
    idle,
    playing,
    levelEnd
}


public class MissionDemolition : MonoBehaviour
{
    static private MissionDemolition S; // a private Singleton

    [Header("Game Over UI")]
    public GameObject gameOverPanel;   // Panel under Canvas (inactive by default)
    public TMPro.TMP_Text finalScoreText;    // e.g., "Shots: 17" (lower is better)
    public TMPro.TMP_Text bestScoreText;     // e.g., "Best: 12"

    [Header("HUD")]
    public TMPro.TMP_Text hudBestScoreText; // Always-visible HUD "Lowest Score"


    [Header("Scoring")]
    public int totalShots;             // sum across all levels


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
    public string showing = "Show Slingshot"; //FollowCam mode


//rrrr
const string BEST_KEY = "BestShots";

void SanitizeBestShots()
{
    int best = PlayerPrefs.GetInt(BEST_KEY, int.MaxValue);
    if (best == 0)
    {
        // 0 is not a valid "lowest shots" record for this game,
        // treat it as "no record yet"
        PlayerPrefs.DeleteKey(BEST_KEY);
        PlayerPrefs.Save();
        Debug.Log("[MD] Sanitized: removed invalid BestShots=0");
    }
}


// rrrr

    void Awake()
    {
        S = this;   // Define the Singleton   
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
        // Get rid of the old castle if one exists
        if (castle != null)
        {
            Destroy(castle);
        }

        // Destory old projectiles if they exist
        Projectile.DESTROY_PROJECTILES();
        foreach (var pl in FindObjectsOfType<ProjectileLine>())
        {
            Destroy(pl.gameObject);
        }


        // Instantiate the new castle
        castle = Instantiate<GameObject>(castles[level]);
        castle.transform.position = castlePos;

        // Reset the goal
        Goal.goalMet = false;

        UpdateGUI();

        mode = GameMode.playing;

        // Zoom out to show btoh
        FollowCam.SWITCH_VIEW(FollowCam.eView.both);

    }


    void UpdateGUI()
    {
        // SHow the data in the GUITexts
        uitLevel.text = "Level: " + (level + 1) + " of " + levelMax;
        uitShots.text = "Shots Taken: " + shotsTaken;
    }

    void Update()
    {
        UpdateGUI();

        // Check for level end
        if ((mode == GameMode.playing) && Goal.goalMet)
        {
            // Change mode to stop checking for level end
            mode = GameMode.levelEnd;

            // Zoom out to show btoh
            FollowCam.SWITCH_VIEW(FollowCam.eView.both);

            // Start the next level in 2 seconds
            Invoke("NextLevel", 2f);
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
        shotsTaken = 0;     // Reset per-level counter for the new level only
        StartLevel();
    }

    // Static method that allows code anywhere to increment shotsTaken
    static public void SHOT_FIRED()
{
    if (S == null)
    {
        S = Camera.main ? Camera.main.GetComponent<MissionDemolition>() : null;
        if (S == null) S = FindObjectOfType<MissionDemolition>();
        if (S == null) { Debug.LogError("MissionDemolition missing."); return; }
    }
    S.shotsTaken++;
    S.totalShots++;
}


    // Static method that allows code anywhere to get a reference to S.castle
    static public GameObject GET_CASTLE()
    {
        return S.castle;
    }

    void ShowGameOver()
    {
        int best = PlayerPrefs.GetInt(BEST_KEY, int.MaxValue);

        // Save only if we actually took shots and beat the record
        if (totalShots > 0 && totalShots < best)
        {
            best = totalShots;
            PlayerPrefs.SetInt(BEST_KEY, best);
            PlayerPrefs.Save();
            Debug.Log("[MD] New best saved: " + best);
        }

        if (finalScoreText != null) finalScoreText.text = $"Total Shots: {totalShots}";
        if (bestScoreText  != null) bestScoreText.text  =
            (best == int.MaxValue) ? "Lowest Score: —" : $"Lowest Score: {best}";

        if (hudBestScoreText != null) hudBestScoreText.gameObject.SetActive(false);
        if (gameOverPanel   != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }



    public void RestartGame()
    {
        Time.timeScale = 1f;
        level = 0;
        shotsTaken = 0;
        totalShots = 0;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // Show the HUD label again
        if (hudBestScoreText != null) hudBestScoreText.gameObject.SetActive(true);

        StartLevel();
        SanitizeBestShots();
        UpdateBestUI();
        Debug.Log("BestShots = " + PlayerPrefs.GetInt("BestShots", int.MaxValue));
    }


    void UpdateBestUI()
    {
        int best = PlayerPrefs.GetInt("BestShots", int.MaxValue);
        string msg = (best == int.MaxValue) ? "Lowest Score: —" : $"Lowest Score: {best}";

        if (bestScoreText != null) bestScoreText.text = msg; // panel label
        if (hudBestScoreText != null) hudBestScoreText.text = msg; // HUD label
    }

public void RestartLevel()
{
    // Make sure the game is running normally
    Time.timeScale = 1f;

    // If the Game Over panel is up for some reason, hide it
    if (gameOverPanel != null) gameOverPanel.SetActive(false);

    // Reset per-level shots and remove them from the run total so your score stays fair
    totalShots -= shotsTaken;
    if (totalShots < 0) totalShots = 0;
    shotsTaken = 0;

    // (Optional) ensure rubber bands are hidden before restart
    var sling = FindObjectOfType<Slingshot>();
    if (sling != null)
    {
        if (sling.lrFront) sling.lrFront.enabled = false;
        if (sling.lrBack)  sling.lrBack.enabled  = false;
    }

    // Relaunch the *current* level.
    // StartLevel() already destroys the old castle, destroys projectiles,
    // resets Goal.goalMet, updates UI, and sets mode = playing.
    StartLevel();

    // Keep HUD "Lowest Score" up to date
    UpdateBestUI();
}



}

  

