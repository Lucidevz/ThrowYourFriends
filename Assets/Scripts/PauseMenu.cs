using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class PauseMenu : MonoBehaviour {
    private bool isPaused;

    [SerializeField]
    private Animator resumeButtonAnim;
    [SerializeField]
    private Animator quitButtonAnim;
    [SerializeField]
    private Animator cancelQuitButtonAnim;
    [SerializeField]
    private Animator confirmQuitButtonAnim;

    [SerializeField]
    private Canvas pauseMenuScreen;
    [SerializeField]
    private Canvas quitMenuScreen;

    [SerializeField]
    private PlayerManager playerManager;
    [SerializeField]
    private EnemySpawner enemySpawner;
    [SerializeField]
    private GameObject[] allThrowableObjects;
    [SerializeField]
    private List<Animator> animsToPause;
    [SerializeField]
    private List<ParticleSystem> particlesToPause;
    [SerializeField]
    private BossPatrol bossPatrol;

    [Header("Sound Varaibles")]
    [SerializeField]
    private MusicLooper mainLevelMusic;
    [SerializeField]
    private MusicLooper puzzleMusic;

    private enum PauseButtons {
        resume,
        quit
    }

    private enum QuitButtons {
        cancel,
        yes
    }

    [SerializeField]
    private PauseButtons currentPauseButton;
    [SerializeField]
    private QuitButtons currentQuitButton;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        allThrowableObjects = GameObject.FindGameObjectsWithTag("PickUp");

        currentPauseButton = PauseButtons.resume;
        SetPauseButtonAnimations(true, false);
    }

    void SetPauseButtonAnimations(bool resumeHighlighted, bool quitHighlighted) {
        resumeButtonAnim.SetBool("Highlighted", resumeHighlighted);
        quitButtonAnim.SetBool("Highlighted", quitHighlighted);
    }

    void SetQuitButtonAnimations(bool cancelHighlighted, bool confirmHighlighted) {
        cancelQuitButtonAnim.SetBool("Highlighted", cancelHighlighted);
        confirmQuitButtonAnim.SetBool("Highlighted", confirmHighlighted);
    }

    public void MoveUp() {
        if (currentPauseButton != PauseButtons.resume) {
            currentPauseButton--;
        }
    }

    public void MoveDown() {
        if (currentPauseButton != PauseButtons.quit) {
            currentPauseButton++;
        }
    }

    public void MoveLeft() {
        if (currentQuitButton != QuitButtons.cancel) {
            currentQuitButton--;
        }
    }

    public void moveRight() {
        if (currentQuitButton != QuitButtons.yes) {
            currentQuitButton++;
        }
    }

    public void SelectButton() {
        if (isPaused) {
            if (pauseMenuScreen.enabled && !quitMenuScreen.enabled) {
                switch (currentPauseButton) {
                    case (PauseButtons.resume):
                        ResumeGame();
                        break;
                    case (PauseButtons.quit):
                        OpenQuitMenu();
                        break;
                }
            } else if (pauseMenuScreen.enabled && quitMenuScreen.enabled) {
                switch (currentQuitButton) {
                    case (QuitButtons.cancel):
                        BackToPauseMenu();
                        break;
                    case (QuitButtons.yes):
                        ReturnToMenu();
                        break;
                }
            }
        }
    }

    void Update()
    {
        if (pauseMenuScreen.enabled && !quitMenuScreen.enabled) {
            switch (currentPauseButton) {
                case (PauseButtons.resume):
                    SetPauseButtonAnimations(true, false);
                    break;
                case (PauseButtons.quit):
                    SetPauseButtonAnimations(false, true);
                    break;
            }
        } 
         else if (pauseMenuScreen.enabled && quitMenuScreen.enabled) {
            switch (currentQuitButton) {
                case (QuitButtons.cancel):
                    SetQuitButtonAnimations(true, false);
                    break;
                case (QuitButtons.yes):
                    SetQuitButtonAnimations(false, true);
                    break;
            }
        }
    }

    public void PauseTheGame() {
        currentPauseButton = PauseButtons.resume;
        mainLevelMusic.PauseAllMusic();
        puzzleMusic.PauseAllMusic();
        foreach (GameObject throwableObejct in allThrowableObjects) {
            throwableObejct.GetComponent<Throwable>().PausePhysics();
        }
        foreach (GameObject player in playerManager.players) {
            PlayerController playerController = player.GetComponent<PlayerController>();
            playerController.stateBeforePaused = playerController.playerState;
            playerController.playerState = PlayerController.playerMode.stationary;
            playerController.PausePhysics();
        }
        foreach (GameObject enemy in enemySpawner.allEnemies) {
            enemy.GetComponent<EnemyPatrol>().FreezeAgent();
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            enemyHealth.PausePhysics();
            if (enemyHealth.isDead) {
                enemyHealth.CancelDeath();
            }
        }
        foreach(Animator anim in animsToPause) {
            anim.speed = 0;
        }
        for(int i = 0; i < particlesToPause.Count; i++) {
            if(particlesToPause[i].IsDestroyed()) {
                particlesToPause.RemoveAt(i);
            } else {
                particlesToPause[i].Pause();
            }
            //if(i > particlesToPause.Count) {
            //    break;
            //}
        }
        if(bossPatrol != null) {
            bossPatrol.FreezeAgent();
        }
        isPaused = true;
        pauseMenuScreen.enabled = true;
    }

    public void ResumeGame() {
        mainLevelMusic.ResumeAllMusic();
        puzzleMusic.ResumeAllMusic();
        foreach (GameObject player in playerManager.players) {
            PlayerController playerController = player.GetComponent<PlayerController>();
            playerController.playerState = playerController.stateBeforePaused;
            playerController.ResumePhysics();
            playerController.CancelStrafe();
            playerController.LowerShield();
        }
        foreach (GameObject throwableObejct in allThrowableObjects) {
            throwableObejct.GetComponent<Throwable>().ResumePhysics();
        }
        foreach (GameObject enemy in enemySpawner.allEnemies) {
            enemy.GetComponent<EnemyPatrol>().UnFreezeAgent();
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            enemyHealth.ResumePhysics();
            if (enemyHealth.isDead) {
                enemyHealth.StartDeathCountdown();
            }
        }
        foreach (Animator anim in animsToPause) {
            anim.speed = 1;
        }
        for (int i = 0; i < particlesToPause.Count; i++) {
            if (particlesToPause[i].IsDestroyed()) {
                particlesToPause.Remove(particlesToPause[i]);
            } else {
                particlesToPause[i].Play();
            }
            //if (i > particlesToPause.Count) {
            //   break;
            //}
        }
        if (bossPatrol != null) {
            bossPatrol.UnFreezeAgent();
        }
        isPaused = false;
        pauseMenuScreen.enabled = false;
    }

    private void OpenQuitMenu() {
        currentQuitButton = QuitButtons.cancel;
        quitMenuScreen.enabled = true;
    }

    private void BackToPauseMenu() {
        currentPauseButton = PauseButtons.quit;
        quitMenuScreen.enabled = false;
    }

    public void ReturnToMenu() {
        playerManager.FadeInBlackScreen();
        StartCoroutine(LoadSceneWithDelay(1));
    }

    IEnumerator LoadSceneWithDelay(float delay) {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(0);
    }

    // Stops the game running but without the use of a pause menu
    public void HaltGame() {
        isPaused = true;
    }

    public bool IsTheGamePaused() {
        return isPaused;
    }

    public void AddItemToPause(Animator animatorToAdd) {
        animsToPause.Add(animatorToAdd);
    }

    public void AddItemToPause(ParticleSystem particles) {
        particlesToPause.Add(particles);
    }
}
