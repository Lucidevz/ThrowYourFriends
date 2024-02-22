using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BossController : MonoBehaviour
{
    [SerializeField]
    private float totalBossHealth;
    private float bossHealth;
    [SerializeField]
    public Image bossHealthBar;
    public bool isDead;
    [SerializeField]
    private Vector3 bossStartPosition;
    [SerializeField]
    private MeshRenderer bossMesh;
    [SerializeField]
    private Canvas bossFace;
    [SerializeField]
    private CapsuleCollider bossCollision;
    [SerializeField]
    private ParticleSystem bossDefeatedParticles;
    [SerializeField]
    private Animator bossAnim;

    [SerializeField]
    private BossPatrol bossPatrol;
    [SerializeField]
    private BossPickUp bossPickUp;
    [SerializeField]
    private PlayerManager playerManager;
    [SerializeField]
    private PauseMenu pauseMenu;
    [SerializeField]
    private EnemySpawner enemySpawner;
    [SerializeField]
    private CameraShake camShake;

    [SerializeField]
    private List<GameObject> allPlayers = new List<GameObject>();

    [Header("Sound Variables")]
    [SerializeField]
    private AudioSource hitSound;
    [SerializeField]
    private MusicLooper music;

    public float healthBarWidth;

    private void Start() {
        ToggleBossVisuals(false);
        bossHealth = totalBossHealth;
    }

    void Update()
    {
        if(bossHealth <= 0) {
            if (!isDead) {
                bossHealthBar.enabled = false;
                StartCoroutine(BossDefeated());
            }
            isDead = true;
        } else {
            healthBarWidth = (bossHealth * (100 / totalBossHealth)) / 100;

            bossHealthBar.rectTransform.localScale = new Vector2(healthBarWidth, bossHealthBar.rectTransform.localScale.y);
        }
    }

    public void StartBossBattle() {
        StartCoroutine(ActivateBoss(14.75f));
    }

    IEnumerator ActivateBoss(float delay) {
        FindAllPlayers();
        StartCoroutine(FindClosestPlayer());

        yield return new WaitForSeconds(delay);
        transform.position = bossStartPosition;
        ToggleBossVisuals(true);
        bossPatrol.UnFreezeAgent();
        bossHealthBar.enabled = true;
    }

    void ToggleBossVisuals(bool value) {
        bossMesh.enabled = value;
        bossFace.enabled = value;
        bossCollision.enabled = value;
        bossPickUp.TogglePickUpCollision(value);
    }

    void FindAllPlayers() {
        allPlayers.Clear();
        allPlayers = playerManager.players;
    }

    public List<GameObject> GetAllPlayers() {
        return allPlayers;
    }

    IEnumerator FindClosestPlayer() {
        yield return new WaitForSeconds(5f);
        if (!pauseMenu.IsTheGamePaused()) {
            bossPatrol.SetTarget(bossPatrol.AssignNewTarget(allPlayers));
        }
        StartCoroutine(FindClosestPlayer());
    }

    private void OnTriggerEnter(Collider other) {
        if(other.tag == "Player") {
            bossPickUp.PickUpPlayer(other.gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.tag == "Player") {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            Throwable playerThrowable = collision.gameObject.GetComponent<Throwable>();
            if (player.playerState == PlayerController.playerMode.beingThrown && !playerThrowable.thrownByBoss) {
                hitSound.Play();
                bossHealth -= playerThrowable.damage;
                camShake.ShakeCamera(0.75f);
            } else {
                //player.RecieveDamage(this.gameObject, bossPatrol.damageToGive);
            }
        } else if (collision.gameObject.tag == "PickUp") {
            Throwable throwable = collision.gameObject.GetComponent<Throwable>();
            if (throwable.isBeingThrown && !throwable.thrownByBoss) {
                hitSound.Play();
                bossHealth -= throwable.damage;
                camShake.ShakeCamera(0.75f);
            }
        }
    }

    private IEnumerator BossDefeated() {
        music.FadeOutMusic(1f);
        foreach (GameObject player in GetAllPlayers()) {
            player.GetComponent<PlayerController>().playerState = PlayerController.playerMode.stationary;
        }
        foreach(GameObject enemy in enemySpawner.allEnemies) {
            enemy.GetComponent<EnemyPatrol>().FreezeAgent();
            enemy.GetComponent<EnemyHealth>().StartDeathCountdown();
        }
        bossPatrol.FreezeAgent();
        bossAnim.SetTrigger("BossDefeated");
        yield return new WaitForSeconds(4f);
        ParticleSystem bossDeathParticles = Instantiate(bossDefeatedParticles, transform.position, Quaternion.identity);
        bossMesh.enabled = false;
        bossFace.enabled = false;
        hitSound.Play();
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(LoadEndScreen(1, 4));
    }

    IEnumerator LoadEndScreen(float delay, int sceneIndex) {
        playerManager.FadeInBlackScreen();
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneIndex);
    }
}
