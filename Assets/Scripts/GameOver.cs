using UnityEngine;

public class GameOver : MonoBehaviour
{
    public GameObject gameOverScreen;
    [SerializeField]
    private Canvas mainUI;
    [SerializeField]
    private MusicLooper mainLevelMusic;
    [SerializeField]
    private MusicLooper puzzleMusic;
    [SerializeField]
    private AudioSource failSound;
    [SerializeField]
    private PauseMenu pauseMenu;

    public void EndGame() {
        if (!gameOverScreen.activeInHierarchy) {
            failSound.Play();
            mainUI.enabled = false;
            gameOverScreen.SetActive(true);
            GameObject.FindGameObjectWithTag("PlayerManager").GetComponent<PlayerManager>().SetIsGameOver(true);
            mainLevelMusic.StopAllMusic();
            puzzleMusic.StopAllMusic();
        }
    }
}
