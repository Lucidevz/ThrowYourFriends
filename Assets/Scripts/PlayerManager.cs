using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public enum level{ 
        Lobby,
        Level1,
        Level2,
        Level3,
        Boss
    }

    public level currentLevel;

    public int numberOfPlayers;
    public List <GameObject> players;
    public Color[] playerColors;
    private int numberOfActivePlayers;
    [Tooltip("The amount of players required to reach the end trigger to move onto the next level")]
    public int numberOfPlayersToProgress;

    public int numberOfEnemiesKilled;
    public int numberOfEnemiesInLevel;

    [SerializeField]
    private PlayerInputManager inputManager;
    [SerializeField]
    private EnemySpawner enemySpawner;
    [SerializeField]
    private GameOver gameOver;
    [SerializeField]
    private Camera cam;
    [SerializeField]
    private PauseMenu pauseMenu;

    public int playersReadyToStart;
    private float levelStartCounter;
    [Tooltip("The time in seconds to start the level once all players are ready")]
    public float timeToStartLevel;

    [Header("Lobby Start Levels")]
    public Vector3 cameraPositionLobby;
    public Vector3[] playerPositionsLobby;
    public int playersToStartLevel1;
    [Header("Level 1 Start Variables")]
    public Vector3 cameraPositionLev1;
    public Vector3[] playerPositionsLev1;
    public int playersToStartLevel2;
    [Header("Level 2 Start Varaibles")]
    public Vector3 cameraPositionLev2;
    public Vector3[] playerPositionsLev2;
    public int playersToStartLevel3;
    [Header("Level 3 Start Variables")]
    public Vector3 cameraPositionLev3;
    public Vector3[] playerPositionsLev3;
    public int playersToStartBoss;
    [Header("Boss Room Start Variables")]
    public Vector3 cameraPositionBoss;
    public Vector3[] playerPositionsBoss;
    [SerializeField]
    private BossController boss;

    [Header("Puzzle Buttons")]
    [SerializeField]
    private PuzzleButton[] levelButtons;

    public bool helmetPowerUpInLevel, bombPowerUpInLevel, chickenPowerUpInLevel;

    public GameObject joinGamePrompt;
    public GameObject loadingNextLevelText;
    public Image loadingLevelBar;

    public TextMeshProUGUI enemySpawnTimerText;
    private float timeUntillEnemiesSpawn;

    public Image blackOverlay;
    public bool loadingLevel;

    private bool isGameOver;

    [Header("Sound Variables")]
    [SerializeField]
    private MusicLooper mainLevelMusic;
    [SerializeField]
    private MusicLooper puzzleMusic;
    private bool isPuzzleMusicPlaying;


    // Start is called before the first frame update
    void Start()
    {
        currentLevel = level.Lobby;
        players = new List <GameObject>();
        if(SceneManager.GetActiveScene().name == "Training") {
            enemySpawner.PrepareToSpawn(1, 10000000);
            timeUntillEnemiesSpawn = enemySpawner.startDelay;
        } else {
            enemySpawner.PrepareToSpawn(0, numberOfPlayers);
        }
        FadeOutBlackScreen();
    }

    private void Update() {
        if (pauseMenu.IsTheGamePaused()) {
            return;
        }
        if (numberOfActivePlayers == 0) {
            if(currentLevel != level.Lobby) {
                foreach(GameObject player in players) {
                    player.GetComponent<PlayerController>().DisableControls();
                }
                gameOver.EndGame();
                isGameOver = true;
            }
        }

        timeUntillEnemiesSpawn -= Time.deltaTime;
        if(timeUntillEnemiesSpawn > 0) {
            int enemySpawnTimeInt = (int)timeUntillEnemiesSpawn;
            enemySpawnTimerText.text = "Enemies spawning in " + enemySpawnTimeInt.ToString();
        } else {
            enemySpawnTimerText.enabled = false;
        }

        int TotalEnemiesToSpawnInLevel = enemySpawner.TotalEnemiesInLevel();
        if (numberOfEnemiesKilled == TotalEnemiesToSpawnInLevel) {
            ActivateButton();
            if (puzzleMusic != null && !isPuzzleMusicPlaying) {
                puzzleMusic.StopAllMusic();
                mainLevelMusic.FadeOutMusic(0.5f);
                puzzleMusic.SetAllAudioVolume(puzzleMusic.maxVolume);
                puzzleMusic.StartMusicLoop(2f);
                switch (currentLevel) {
                    case (level.Level1):
                        SetAllPlayerPositions(playerPositionsLev1);
                        break;
                    case (level.Level2):
                        SetAllPlayerPositions(playerPositionsLev2);
                        break;
                    case (level.Level3):
                        SetAllPlayerPositions(playerPositionsLev3);
                        break;
                }
            }
            isPuzzleMusicPlaying = true;
        }

        if((playersReadyToStart == numberOfPlayersToProgress) && players.Count > 0 && !loadingLevel && !isGameOver) {
            if(numberOfEnemiesKilled == enemySpawner.numberOfEnemiesSpawned) {
                levelStartCounter += Time.deltaTime;
                loadingNextLevelText.SetActive(true);
                loadingLevelBar.rectTransform.localScale = new Vector2(levelStartCounter * (100 / timeToStartLevel) / 100, loadingLevelBar.rectTransform.localScale.y);
                loadingLevelBar.enabled = true;
            }
        } else {
            levelStartCounter = 0;
            loadingNextLevelText.SetActive(false);
            loadingLevelBar.enabled = false;
            loadingLevelBar.rectTransform.localScale = new Vector2(0, loadingLevelBar.rectTransform.localScale.y);
        }

        // If in the lobby set number of players to start level the amount of players
        if (currentLevel == level.Lobby) {
            numberOfPlayersToProgress = players.Count;
            if(players.Count < 4) {
                joinGamePrompt.SetActive(true);
            } else {
                joinGamePrompt.SetActive(false);
            }
        } // Otherwise set the amount of players required to start next level the amount of players still alive 
        else {
            numberOfPlayersToProgress = numberOfActivePlayers;
        }

        if(levelStartCounter > timeToStartLevel && numberOfEnemiesKilled == enemySpawner.numberOfEnemiesSpawned) {
            switch (currentLevel) {
                case (level.Lobby):
                    joinGamePrompt.SetActive(false);
                    StartCoroutine(ChangeLevel(1, cameraPositionLev1, playerPositionsLev1));
                    numberOfPlayersToProgress = playersToStartLevel1;
                    currentLevel = level.Level1;
                    enemySpawner.PrepareToSpawn(1, numberOfPlayers);
                    StartCoroutine(enemySpawner.SpawnEnemies());
                    break;
                case (level.Level1):
                    StopCoroutine(enemySpawner.SpawnEnemies());
                    StartCoroutine(ChangeLevel(2, cameraPositionLev2, playerPositionsLev2));
                    numberOfPlayersToProgress = playersToStartLevel2;
                    currentLevel = level.Level2;
                    enemySpawner.PrepareToSpawn(2, numberOfPlayers);
                    StartCoroutine(enemySpawner.SpawnEnemies());
                    break;
                case (level.Level2):
                    StopCoroutine(enemySpawner.SpawnEnemies());
                    StartCoroutine(ChangeLevel(3, cameraPositionLev3, playerPositionsLev3));
                    numberOfPlayersToProgress = playersToStartLevel3;
                    currentLevel = level.Level3;
                    enemySpawner.PrepareToSpawn(3, numberOfPlayers);
                    StartCoroutine(enemySpawner.SpawnEnemies());
                    break;
                case (level.Level3):
                    StopCoroutine(enemySpawner.SpawnEnemies());
                    StartCoroutine(ChangeLevel(4, cameraPositionBoss, playerPositionsBoss));
                    numberOfPlayersToProgress = playersToStartBoss;
                    currentLevel = level.Boss;
                    enemySpawner.PrepareToSpawn(4, numberOfPlayers);
                    StartCoroutine(enemySpawner.SpawnEnemies());
                    boss.StartBossBattle();
                    break;
                case (level.Boss):
                    StartCoroutine(ChangeLevel(5, cameraPositionLobby, playerPositionsLobby));
                    currentLevel = level.Lobby;
                    joinGamePrompt.SetActive(true);
                    break;
            }
        }

        if(inputManager != null) {
            if(numberOfPlayers < players.Count - 1) {
                inputManager.enabled = true;
            }
        }
    }

    private IEnumerator ChangeLevel(int level, Vector3 cameraPosition, Vector3[] playerPositions) {
        if (pauseMenu.IsTheGamePaused()) {
            yield break;
        }
        mainLevelMusic.StopAllMusic();
        puzzleMusic.FadeOutMusic(0.5f);
        loadingLevel = true;
        blackOverlay.CrossFadeColor(Color.black, 1f, false, true);
        yield return new WaitForSeconds(1f);
        if (pauseMenu.IsTheGamePaused()) {
            yield return null;
        }
        cam.transform.position = cameraPosition;
        numberOfEnemiesKilled = 0;
        for (int i = 0; i < numberOfPlayers; i++) {
            players[i].GetComponent<PlayerController>().ResetPlayerVariables();
            PlayerHealth playerHealth = players[i].GetComponent<PlayerHealth>();
            players[i].transform.position = playerPositions[i];
            players[i].SetActive(true);
            if (playerHealth.isDead) {
                ChangeNumberOfActivePlayers(1);
                playerHealth.SetHealthToMax();
                playerHealth.isDead = false;
            }
        }
        
        isPuzzleMusicPlaying = false;
        puzzleMusic.StopAllMusic();
        levelStartCounter = 0;
        loadingLevel = false;
        FadeOutBlackScreen();
        mainLevelMusic.SetAllAudioVolume(mainLevelMusic.maxVolume);
        mainLevelMusic.StartFromIntro();
        timeUntillEnemiesSpawn = enemySpawner.startDelay;
        enemySpawnTimerText.enabled = true;
    }

    void SetAllPlayerPositions(Vector3[] positions) {
        for (int i = 0; i < numberOfPlayers; i++) {
            PlayerHealth playerHealth = players[i].GetComponent<PlayerHealth>();
            players[i].SetActive(true);
            if (playerHealth.isDead) {
                players[i].GetComponent<PlayerController>().ResetPlayerVariables();
                ChangeNumberOfActivePlayers(1);
                playerHealth.SetHealthToMax();
                players[i].transform.position = positions[i];
                playerHealth.isDead = false;
            }
        }
    }
    void ActivateButton() {
        switch (currentLevel) {
            case (level.Level1):
                levelButtons[0].canBePressed = true;
                break;
            case (level.Level2):
                levelButtons[1].canBePressed = true;
                break;
            case (level.Level3):
                levelButtons[2].canBePressed = true;
                break;
        }
    }

    public void FadeInBlackScreen() {
        blackOverlay.CrossFadeColor(Color.black, 1f, false, true);
    }

    public void FadeOutBlackScreen() {
        blackOverlay.CrossFadeColor(Color.clear, 1f, false, true);
    }

    public void ChangeNumberOfActivePlayers(int amount) {
        numberOfActivePlayers += amount;
    }

    public int GetNumberOfActivePlayers() {
        return numberOfActivePlayers;
    }

    public bool GetIsGameOver() {
        return isGameOver;
    }

    public void SetIsGameOver(bool value) {
        isGameOver = value;
    }
}
