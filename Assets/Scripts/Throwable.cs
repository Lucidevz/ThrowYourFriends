using System.Collections;
using UnityEngine;

public class Throwable : MonoBehaviour
{
    public bool isBeingThrown;
    public bool isBeingHeld;
    public bool thrownByBoss;
    public bool hasHitSomething;
    public bool hitSpring;

    [SerializeField]
    private PlayerController player;
    [SerializeField]
    private PlayerManager playerManager;
    [SerializeField]
    private PlayerHealth playerHealth;
    [SerializeField]
    private Rigidbody rb;

    public float damage;
    private float originalDamage;
    public Vector3 thrownDirection;

    private CameraShake cameraShake;
    private PauseMenu pauseMenu;
    private Vector3 velocityToReturn; // The velocity of the object before the game was paused
                                      // to apply back on the rigidbody when the game resumes

    [Header("Sound Variables")]
    [SerializeField]
    private AudioSource hitSound;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GetComponent<PlayerController>();
        playerManager = GameObject.FindGameObjectWithTag("PlayerManager").GetComponent<PlayerManager>();
        playerHealth = GetComponent<PlayerHealth>();
        isBeingThrown = false;
        hasHitSomething = false;
        originalDamage = damage;
        cameraShake = Camera.main.GetComponent<CameraShake>();
        pauseMenu = GameObject.FindGameObjectWithTag("Pause").GetComponent<PauseMenu>();
    }

    private void OnCollisionEnter(Collision collision) {
        if (isBeingThrown && !hitSpring) {
            hasHitSomething = true;
            StartCoroutine(ReturnControls(0.85f));
        }
        StartCoroutine(CancelIsBeingThrown(0.25f));
    }

    private IEnumerator ReturnControls(float timeDelay) {
        damage = originalDamage;
        if (pauseMenu.IsTheGamePaused()) {
            yield return null;
        }
        // Check that this object has a player script, and re-enable it
        if (player != null) {
            yield return new WaitForSeconds(timeDelay);
            // If the player is picked up, cancel the coroutine
            if (player.playerState == PlayerController.playerMode.beingHeld) {
                yield break;
            }
            player.playerState = PlayerController.playerMode.move;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Catcher") {
            if (playerManager.currentLevel == PlayerManager.level.Lobby) {
                transform.position = new Vector3(0, 10, 0);
                rb.velocity = Vector3.zero;
                transform.rotation = Quaternion.Euler(Vector3.zero);
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                return;
            } else {
                if (playerHealth != null && !playerHealth.isDead) {
                    hitSound.Play();
                    playerHealth.DisablePlayer();
                }
                return;
            }
        }

        // If the player hits something while being thrown
        if (isBeingThrown || (player != null && player.playerState ==PlayerController.playerMode.beingThrown) && hasHitSomething == false) {
            if (other.tag == "Enemy") {
                hitSound.Play();
                rb.velocity *= 0.5f;
                float cameraShakeMagnitude = (damage * (100 / originalDamage)) / 100;
                cameraShake.ShakeCamera(cameraShakeMagnitude);
                EnemyHealth hitEnemy = other.GetComponent<EnemyHealth>();
                hitEnemy.SetObjectHitBy(this.gameObject);
                hitEnemy.KnockOverEnemy(this, true);
                StartCoroutine(CancelIsBeingThrown(0.01f));
                //If the thrown object is a player, set them invincible so they can escape after being thrown
                if (player != null) {
                    player.SetInvincibilityTimer(1.1f);
                }
            }
            else if (other.tag == "Spring") {
                 if (pauseMenu.IsTheGamePaused()) {
                    return;
                 }
                Spring spring = other.GetComponent<Spring>();
                if (spring != null && !hitSpring) {
                    spring.LaunchPlayer(this.gameObject);
                    cameraShake.ShakeCamera(0.5f);
                }
                hitSpring = true;
            }
            if (!hitSpring) {
                StartCoroutine(ReturnControls(0.85f));
            }
        }

        hasHitSomething = true;
    }

    private IEnumerator CancelIsBeingThrown(float delay) {
        yield return new WaitForSeconds(delay);
        isBeingThrown = false;
        thrownByBoss = false;
    }

    private void OnTriggerExit(Collider other) {
        if (other.tag == "Spring") {
            hitSpring = false;
            if (player != null && player.playerState == PlayerController.playerMode.beingThrown && !isBeingThrown) {
                ReturnControls(0.4f);
            }
        }
    }

    public void PausePhysics() {
        velocityToReturn = rb.velocity;
        rb.isKinematic = true;
    }

    public void ResumePhysics() {
        rb.isKinematic = false;
        rb.velocity = velocityToReturn;
    }
}
