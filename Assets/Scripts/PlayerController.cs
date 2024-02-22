using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

    // Input variables
    private PlayerInput playerInput;
    private Vector2 moveInput;

    // Spawning Players Variables
    private PlayerManager playerManager;
    [SerializeField]
    private MeshRenderer mesh;

    public enum playerMode {
        stationary, // Player is unable to do anything 
        move, // Player can move around and control as normal
        beingHeld, // Player is being held by another player and can no longer use controls
        beingThrown, // Player is being thrown and cannot use controls until they've landed
        onLadder, // When the player is climbing a ladder
    }

    public playerMode playerState;
    public playerMode stateBeforePaused;

    [SerializeField]
    private Transform thisTransform;

    [Header("Movement Variables")]
    public bool isBeingHeld;
    [SerializeField]
    private float acceleration;
    [SerializeField]
    private float maxSpeed;
    [SerializeField]
    private float slowedSpeed;
    private float originalMaxSpeed;
    [SerializeField]
    private float rotateSpeed;
    [SerializeField]
    private Rigidbody rb;
    private PhysicMaterial physicMaterial;
    private bool isGrounded;
    public Transform groundCheck;
    public LayerMask groundLayers;

    [Header("Dash Variables")]
    [SerializeField]
    private float dashForce;
    [Tooltip("The duration of time in seconds that the dash will last")]
    [SerializeField]
    private float dashLength;
    [Tooltip("The duration of time the player has to wait before performing another dash")]
    [SerializeField]
    private float dashCoolDown;
    private bool isDashing;
    private bool canDash;
    [SerializeField]
    private TrailRenderer trailRenderer;
    public bool hasDashed;

    [Header("Interaction Variables")]
    public LayerMask interactableLayer; // The objects on these layers can be interacted with
    public bool isHoldingItem;
    [Tooltip("The minimum and maximim amount of time in seconds to hold the button down to throw an object")]
    [SerializeField]
    private Vector2 minMaxHoldTime;
    private bool isButtonDown;
    public float interactInputTime; // How long the interact button has been held down

    private GameObject heldItem; // The item currently being held by the player
    public float maxThrowForce;
    public bool isInRangeOfObject;

    [Header("Blocking Variables")]
    public GameObject shield;
    private bool isBlocking;

    private bool isStrafing;

    [Header("Ladder Climbing Variables")]
    [SerializeField]
    private float climbSpeed;
    private bool isOnLadder;
    private bool isInRangeOfLadder;
    private GameObject theLadder;
    private Transform attatchPoint;

    [Header("KnockBack Variables")] // Used to apply knock back when hit by an enemy
    [Tooltip("The length at which a knock back affect will last when the enemy has been hit")]
    [SerializeField]
    private float knockBackLength;
    [SerializeField]
    private float knockBackMultiplyer;
    private float originalKnockBackMultiplyer;
    private float knockBackTimer;
    private Vector3 knockBackForce;

    [Header("Invincibility Varaibles")]
    public float invincibilityLength;
    private float invincibilityTimer;

    [Header("Explosion Power Up Variables")]
    [SerializeField]
    private PowerUp powerUp;
    public bool explodeOnLand;
    public float explosionRadius;
    public float explosionForce;
    public LayerMask enemyLayer;
    public ParticleSystem[] ExplosionParticles;
    public GameObject explosionLight;

    [Header("Extra Script Variables")]
    [SerializeField]
    private Throwable throwable;
    [SerializeField]
    private PlayerHealth health;
    private CameraShake cameraShake;
    private PauseMenu pauseMenu;
    private Vector3 velocityToReturn; // Velocity to set player when game is unpaused

    private Image damageEffect;

    [Header("Sound Effects")]
    [SerializeField]
    private AudioSource pickUpSound;
    [SerializeField]
    private AudioSource throwSound, dashDound, shieldSound, hitSound;

    private SaveData optionsData;

    private void Awake() {
        playerState = playerMode.move;
        //thisTransform = GetComponent<Transform>();
        playerManager = FindObjectOfType<PlayerManager>();
        //mesh = GetComponent<MeshRenderer>();
        isBeingHeld = false;
        playerInput = new PlayerInput();
        //rb = GetComponent<Rigidbody>();
        canDash = true;
        //trailRenderer = GetComponent<TrailRenderer>();
        shield.SetActive(false);
        originalMaxSpeed = maxSpeed;
        originalKnockBackMultiplyer = knockBackMultiplyer;
        //throwable = GetComponent<Throwable>();
        //health = GetComponent<PlayerHealth>();
        rb.maxAngularVelocity = 15;
        physicMaterial = GetComponent<CapsuleCollider>().material;
        cameraShake = Camera.main.GetComponent<CameraShake>();
        pauseMenu = GameObject.FindGameObjectWithTag("Pause").GetComponent<PauseMenu>();
        damageEffect = GameObject.FindGameObjectWithTag("DamageEffect").GetComponent<Image>();
        optionsData = GameObject.FindGameObjectWithTag("OptionsData").GetComponent<SaveData>();

    }

    private void Start() {
        if(playerManager != null) {
            playerManager.numberOfPlayers++;
            name = "Player" + playerManager.numberOfPlayers;
            playerManager.players.Add(this.gameObject);
            mesh.material.color = playerManager.playerColors[playerManager.numberOfPlayers - 1];
            trailRenderer.material.color = playerManager.playerColors[playerManager.numberOfPlayers - 1];
            SetPlayerPosition(playerManager.playerPositionsLobby);
            if(playerManager.currentLevel != PlayerManager.level.Lobby) {
                SpawnPlayerInactive();
            }
        }
        optionsData.AddSoundEffects(GetComponents<AudioSource>());
    }

    public void ResetPlayerVariables() {
        canDash = true;
        health.SetHealthToMax();
        PlaceObjectOnFloor();
        isBeingHeld = false;
        playerState = playerMode.move;
        powerUp.SetPowerUpActive(false);
        powerUp.SetTimer(0);
        EnableControls();
    }

    void SetPlayerPosition(Vector3[] playerPositions) {
        thisTransform.position = playerPositions[playerManager.numberOfPlayers - 1];
    }

    void SpawnPlayerInactive() {
        health.isDead = true;
        playerManager.ChangeNumberOfActivePlayers(-1);
    }

    void OnEnable() {
        EnableControls();
        playerManager.ChangeNumberOfActivePlayers(+1);
    }

    public void EnableControls() {
        playerInput.Controller.Enable();
        playerInput.Controller.Move.Enable();
        playerInput.Controller.Interact.Enable();
        playerInput.Controller.Dash.Enable();
        playerInput.Controller.Block.Enable();
    }

    public void DisableControls() {
        playerInput.Controller.Disable();
        playerInput.Controller.Move.Disable();
        playerInput.Controller.Interact.Disable();
        playerInput.Controller.Dash.Disable();
        playerInput.Controller.Block.Disable();
    }

    void OnDisable() {
        DisableControls();
        playerManager.ChangeNumberOfActivePlayers(-1);
    }

    private void FixedUpdate() {
        if (!pauseMenu.IsTheGamePaused()) {

            PlayerControls();

            if (isDashing) {
                float dashTimer = 0;
                if (dashTimer < dashLength) {
                    rb.AddForce(thisTransform.forward * (dashForce * 10));
                    dashTimer += Time.deltaTime;
                }
            }

            knockBackTimer -= Time.deltaTime;
            if (knockBackTimer > 0) {
                ApplyKnockBack();
            } else {
                knockBackMultiplyer = originalKnockBackMultiplyer;
            }

            // Rumble the controllers when holding the button down
            if (interactInputTime >= 0.95 && !health.isDead) {
                Gamepad.current.SetMotorSpeeds(0.015f, 0.015f);
            } else {
                Gamepad.current.SetMotorSpeeds(0f, 0f);
            }
        }
    }

    void Update() {
        // If the interact button is down, increase a timer (used to determine how to drop an item when the button has been released: tap = drop, hold = throw)
        if (isButtonDown == true && interactInputTime < 1) {
            interactInputTime += Time.deltaTime;

        }
        if (interactInputTime > minMaxHoldTime.y) {
            interactInputTime = 1;
        }

        if (canDash) {
            playerInput.Controller.Dash.Enable();
        } else {
            playerInput.Controller.Dash.Disable();
        }

        if(isBlocking || isStrafing) {
            maxSpeed = slowedSpeed;
        }

        invincibilityTimer -= Time.deltaTime;

        if(playerState == playerMode.beingThrown) {
            physicMaterial.dynamicFriction = 0.5f;
        } else {
            physicMaterial.dynamicFriction = 0.1f;
        }

        isGrounded = Physics.CheckSphere(groundCheck.position, 0.1f, groundLayers);

        float damageEffectOpacity = damageEffect.color.a;
        damageEffectOpacity -= (Time.deltaTime);
        damageEffect.color = new Color(damageEffect.color.r, damageEffect.color.g, damageEffect.color.b, damageEffectOpacity);

        if (isHoldingItem && heldItem != null) {
            Vector3 pickUpPos = new Vector3(thisTransform.position.x, (thisTransform.position.y + thisTransform.localScale.y) + (heldItem.transform.localScale.y + 0.25f), thisTransform.position.z);
            heldItem.transform.position = pickUpPos;
        }
    }


    public void OnMove(InputAction.CallbackContext context) {
        if (pauseMenu.IsTheGamePaused()) {
            return;
        }
        if (context.performed) {
            moveInput = context.ReadValue<Vector2>();

            if (throwable.hasHitSomething && playerState == playerMode.move) {
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                rb.velocity = Vector3.zero;
                throwable.hasHitSomething = false;
            }

        } else if (context.canceled) {
            moveInput = Vector2.zero;
        }
    }

    public void StartDash(InputAction.CallbackContext context) {
        if (pauseMenu.IsTheGamePaused()) {
             return;
        }
        if (context.performed && !isDashing)
            StartCoroutine(Dash());
    }

    public void Interact(InputAction.CallbackContext context) {
        if (pauseMenu.IsTheGamePaused()) {
            return;
        }
        if (context.performed) {
            PickUp();
        } else if (context.canceled) {
            PutObjectDown();
        }
    }

    public void Block(InputAction.CallbackContext context) {
        if (pauseMenu.IsTheGamePaused()) {
            return;
        }
        if (context.performed) {
            if (!isHoldingItem) {
                RaiseShield();
            }
        } else if (context.canceled) {
            LowerShield();
        }
    }

    public void Strafe (InputAction.CallbackContext context) {
        if (pauseMenu.IsTheGamePaused()) {
            return;
        }
        if (context.performed) {
            isStrafing = true;
        } else if (context.canceled) {
            isStrafing = false;
            maxSpeed = originalMaxSpeed;
        }
    }

    public void PauseGame(InputAction.CallbackContext context) {
        if (playerManager.loadingLevel) {
            return;
        }
        if (!pauseMenu.IsTheGamePaused()) {
            if (context.performed) {
                pauseMenu.PauseTheGame();

            }
        }
    }
    public void MoveUpPauseMenu(InputAction.CallbackContext context) {
        if (pauseMenu.IsTheGamePaused()) {
            if (context.performed) {
                pauseMenu.MoveUp();
            }
        }
    }
    public void MoveDownPauseMenu(InputAction.CallbackContext context) {
        if (pauseMenu.IsTheGamePaused()) {
            if (context.performed) {
                pauseMenu.MoveDown();
            }
        }
    }

    public void MoveLeftPauseMenu(InputAction.CallbackContext context) {
        if (pauseMenu.IsTheGamePaused()) {
            if (context.performed) {
                pauseMenu.MoveLeft();
            }
        }
    }

    public void MoveRightPauseMenu(InputAction.CallbackContext context) {
        if (pauseMenu.IsTheGamePaused()) {
            if (context.performed) {
                pauseMenu.moveRight();
            }
        }
    }

    public void PauseSelect(InputAction.CallbackContext context) {
        if (pauseMenu.IsTheGamePaused()) {
            if (context.performed) {
                pauseMenu.SelectButton();
            }
        }
    }

    public void DismountFromPlayer(InputAction.CallbackContext context) {
        if (pauseMenu.IsTheGamePaused()) {
            return;
        }
        if (context.performed) {
            if(playerState == playerMode.beingHeld) {
                PlayerController parentPlayer = thisTransform.parent.GetComponent<PlayerController>();
                if(parentPlayer != null) {
                    parentPlayer.PlaceObjectOnFloor();
                }
            }
        }
    }

    public void AnyButtonPressed(InputAction.CallbackContext context) {
        if (playerManager.GetIsGameOver()) {
            if (context.performed) {
                playerManager.FadeInBlackScreen();
                StartCoroutine(LoadLevel(0, 1f));
            }
        }
    }

    private void PlayerControls() {
        if (pauseMenu.IsTheGamePaused()) {
            return;
        }
        if (playerState == playerMode.move) {
            // Get a variable to be the difference between our current velocity and the max speed
            // When the player makes a greater change in direction, this number will be larger, appying more velocity to the rigidbody, creating more responsive movement
            float speedDifX = maxSpeed - rb.velocity.x;
            float speedDifZ = maxSpeed - rb.velocity.z;
            // Return speed difference variable if it's less than the max speed, else just use max speed
            float xVel = Mathf.Min(speedDifX * acceleration, maxSpeed);
            float zVel = Mathf.Min(speedDifZ * acceleration, maxSpeed);

            Vector3 move = new Vector3(moveInput.x * xVel * Time.deltaTime, rb.velocity.y, moveInput.y * zVel * Time.deltaTime);

            if (!isDashing) {
                rb.velocity = move;

                if (!isBlocking && !isStrafing) {
                    if (move.x != 0 && move.z != 0) {
                        Vector3 LookDirection = new Vector3(moveInput.x * xVel, 0, moveInput.y * zVel) * Time.deltaTime;
                        Quaternion targetRotation = Quaternion.LookRotation(LookDirection, Vector3.up);

                        thisTransform.rotation = Quaternion.RotateTowards(thisTransform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
                    }
                }
            }
        }
        if (playerState == playerMode.onLadder) {
            // Booleans to check if another player is below or above us on the ladder
            bool isOtherPlayerAbove = false;
            bool isOtherPlayerBelow = false;
            // Fire 2 raycasts up and down of the player to check that there is a player above or below them on the ladder
            RaycastHit hit;
            Vector3 rayFirePos = new Vector3(thisTransform.position.x, thisTransform.position.y, thisTransform.position.z);
            Ray upperCheck = new Ray(rayFirePos, thisTransform.up);
            Ray LowerCheck = new Ray(rayFirePos, -thisTransform.up);

            // Stop the player from being able to move up the ladder if there is another player above them
            if (Physics.Raycast(upperCheck, out hit, 1.15f)) {
                if(hit.collider.tag == "Player") {
                    isOtherPlayerAbove = true;
                } else {
                    isOtherPlayerAbove = false;
                }
            }
            // Stop the player from being able to move down the ladder if there is another player beneath them
            if (Physics.Raycast(LowerCheck, out hit, 1.15f)) {
                if (hit.collider.tag == "Player") {
                    isOtherPlayerBelow = true;
                } else {
                    isOtherPlayerBelow = false;
                }
            }

            // Check the upwards/downwards direction of the joystick on the game pad
            if (moveInput.y > 0.125 && !isOtherPlayerAbove) {
                rb.velocity = thisTransform.up * climbSpeed;
            } else if (moveInput.y < -0.125 && !isOtherPlayerBelow) {
                rb.velocity = thisTransform.up * -climbSpeed;
            }
            // Stops player from moving if joystick is close to the middle
            else {
                rb.velocity = Vector3.zero;
            }
            
        }
    }

    private IEnumerator Dash() {
        if (pauseMenu.IsTheGamePaused()) {
            yield break;
        }
        hasDashed = true;
        if (playerState != playerMode.move || !canDash || isStrafing) {
            yield break;
        }
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);

        // Don't dash if the player is standing still or blocking with their shield
        if (moveInput.x != 0 && moveInput.y != 0 && !isBlocking) {
            dashDound.Play();
            isDashing = true;
            canDash = false;
            trailRenderer.emitting = true;
            
            // Wait for the 'dashLength' time and stop the dashing affect
            yield return new WaitForSeconds(dashLength);
            isDashing = false;
            trailRenderer.emitting = false;
            
            // After the cool down, allow the player to dash again
            float dashWaitTime = dashCoolDown - dashLength; // Subtract the dash length from the cool down length because the 'WaitForSeconds' above means we've already waited for a small period of time
            yield return new WaitForSeconds(dashWaitTime);
            canDash = true;
        }
    }

    public void CancelDash() {
        isDashing = false;
        trailRenderer.emitting = false;
    }

    public void PickUp() {
        if(isDashing || isBlocking) {
            return;
        }
        interactInputTime = 0;
        // If the player is currently not holding an item, pick one up if there's one in range.
        if (!isHoldingItem && (playerState == playerMode.move || playerState == playerMode.beingHeld)) {
            Vector3 pickUpPosition = thisTransform.position + (thisTransform.forward * 2.5f);
            Collider[] objectsInRange = Physics.OverlapSphere(pickUpPosition, 2.5f, interactableLayer);
            if (objectsInRange.Length > 0) {
                foreach(Collider objectFound in objectsInRange) {
                    if (objectFound.name != this.name) {
                        PlayerController foundPlayer = objectFound.GetComponent<PlayerController>();
                        EnemyHealth foundEnemy = objectFound.GetComponent<EnemyHealth>();
                        Throwable foundThrowable = objectFound.GetComponent<Throwable>();
                        if(transform.parent != null && objectFound.name == transform.parent.name) {
                            return;
                        }
                        if (foundThrowable != null && foundThrowable.isBeingHeld && !foundThrowable.isBeingThrown) {
                            return;
                        }
                        if(foundEnemy != null) {
                            if (foundEnemy.isDead) {
                                PickUpObject(objectFound.gameObject);
                                return;
                            }
                        } else if (foundPlayer != null) {
                            if((foundPlayer.isBeingHeld ||foundPlayer.playerState != playerMode.beingHeld)) {
                                if (foundPlayer.playerState != playerMode.onLadder) {
                                    PickUpObject(objectFound.gameObject);
                                    return;
                                }
                            }
                        } 
                        else {
                            PickUpObject(objectFound.gameObject);
                            return;
                        }
                    }
                }
            }      
        } 
        else {
            isButtonDown = true;
        }
    }

    void PickUpObject(GameObject objectToPickup) {
        pickUpSound.Play();
        heldItem = objectToPickup;
        PlayerController otherPlayer = heldItem.GetComponent<PlayerController>();
        if (otherPlayer != null) {
            otherPlayer.playerState = PlayerController.playerMode.beingHeld;
            otherPlayer.knockBackTimer = 0;
        }
        EnemyHealth pickedUpEnemy = heldItem.GetComponent<EnemyHealth>();
        if (pickedUpEnemy != null) {
            if (!pickedUpEnemy.isDead) {
                return;
            } else {
                pickedUpEnemy.CancelDeath();
            }
        }
        // Reset Rigidbody values when picking item up
        ReturnRigidbodyValues(heldItem.GetComponent<Rigidbody>(), true, false);

        // Reset the hit ground variable in picked up objects throwable script
        Throwable otherThrowable = heldItem.GetComponent<Throwable>();
        otherThrowable.hasHitSomething = false;
        otherThrowable.isBeingHeld = true;
        otherThrowable.isBeingThrown = false;

        // Set the position of the picked up object above the player
        Vector3 pickUpPos = new Vector3(thisTransform.position.x, (thisTransform.position.y + thisTransform.localScale.y) + (heldItem.transform.localScale.y + 0.25f), thisTransform.position.z);
        heldItem.transform.position = pickUpPos;
        // Only set rotation if this object isn't an enemy
        if (pickedUpEnemy == null) {
            heldItem.transform.rotation = thisTransform.rotation;
        } else {
            Vector3 newRotation = thisTransform.eulerAngles + (Vector3.forward * 90);
            heldItem.transform.rotation = Quaternion.Euler(newRotation);
        }
        heldItem.transform.parent = transform;

        StartCoroutine(Wait(0.2f));
        isHoldingItem = true;
    }

    public void PutObjectDown() {
        if (isHoldingItem && isButtonDown) {
            // The script of the player being held
            PlayerController otherPlayer = heldItem.GetComponent<PlayerController>();
            Throwable thrownObject = heldItem.GetComponent<Throwable>();
            thrownObject.isBeingHeld = false;

            // If the button is tapped
            if (interactInputTime < minMaxHoldTime.x) {
                PlaceObjectOnFloor();
            } 

            // THROW THE OBJECT 
            // If the button has been held down rather than tapped 
            else if (interactInputTime >= minMaxHoldTime.x) {
                if (otherPlayer != null) {
                    // If the player being held is also holding an object, don't throw them
                    if (otherPlayer.isHoldingItem) {
                        // See if the player held by the current player is holding another player
                        // This would be the third players in the stack of players
                        PlayerController thirdStackObject = otherPlayer.heldItem.GetComponent<PlayerController>();
                        if (thirdStackObject != null && thirdStackObject.isHoldingItem) {
                            // See if the third player is holding another player
                            // This would be the fourth players in the stack of players
                            PlayerController fourthStackObject = thirdStackObject.heldItem.GetComponent<PlayerController>();
                            if (fourthStackObject != null && fourthStackObject.isHoldingItem) {
                                // As there can't be more than 4 players in the game, the fourth player
                                // can't be holding another player, but may be holding another object
                                // Make the fourth player throw their object
                                fourthStackObject.ThrowObject(fourthStackObject.heldItem.GetComponent<Throwable>(), fourthStackObject.heldItem.GetComponent<Rigidbody>(), null, fourthStackObject.gameObject, true);
                            }
                            // Make the third player throw their object
                            thirdStackObject.ThrowObject(thirdStackObject.heldItem.GetComponent<Throwable>(), thirdStackObject.heldItem.GetComponent<Rigidbody>(), fourthStackObject, this.gameObject, true);
                        }
                        // Make the second player throw their object
                        otherPlayer.ThrowObject(otherPlayer.heldItem.GetComponent<Throwable>(), otherPlayer.heldItem.GetComponent<Rigidbody>(), thirdStackObject, this.gameObject, true);
                    }
                }
                // Make the first (bottom) player throw their object
                ThrowObject(thrownObject, heldItem.GetComponent<Rigidbody>(), otherPlayer, this.gameObject, true);

            }
            // Reset the held item variables
            //heldItem = null;
            // Check if the player has a parent object, if so, see if it has a player controller script attatched to it
            PlayerController parentController = null;
            if (thisTransform.parent != null) {
                parentController = thisTransform.parent.GetComponent<PlayerController>();
            }
            // If parent object has a player controller, then we are being held by another player, so set player's state to being held
            if (parentController != null) {
                playerState = playerMode.beingHeld;
            }
            // If the player isn't being held, then set the current state to move
            else {
                playerState = playerMode.move;
            }
            isHoldingItem = false;
        }
        // Reset the input variables
        isButtonDown = false;
        interactInputTime = 0;
    }

    public void PlaceObjectOnFloor() {
        if (!isHoldingItem) {
            return;
        }
        pickUpSound.Play();
        // Unparent heldItem from the player and fire a raycast downwards from in front the player.
        heldItem.transform.parent = null;
        Vector3 putDownPos = heldItem.transform.position + thisTransform.forward * 1.5f + thisTransform.up * 1.1f;
        heldItem.transform.position = putDownPos;
        RaycastHit groundCheck;

        // If the raycast hits something, compare the distance between the object and the ground, and move the object down that distance, placing it correctly on the surface
        Ray ray = new Ray(heldItem.transform.position, Vector3.down);
        if (Physics.Raycast(ray, out groundCheck)) {
            float yOffset = heldItem.transform.localScale.y / 2;
            float putDownPosition = groundCheck.distance - yOffset;
            Vector3 groundPos = new Vector3(heldItem.transform.position.x, heldItem.transform.position.y - putDownPosition, heldItem.transform.position.z);
            heldItem.transform.position = groundPos;
            // Return the object Rigidbody values and player script when putting it down
            ReturnRigidbodyValues(heldItem.GetComponent<Rigidbody>(), false, true);
        }
        Throwable heldThrowable = heldItem.GetComponent<Throwable>();
        if (heldThrowable != null) {
            heldThrowable.isBeingHeld = false;
            heldThrowable.isBeingThrown = false;
        }
        PlayerController otherPlayer = heldItem.GetComponent<PlayerController>();
        if(otherPlayer != null) {
            otherPlayer.playerState = PlayerController.playerMode.move;
        }
        EnemyHealth heldEnemy = heldItem.GetComponent<EnemyHealth>();
        if (heldEnemy != null) {
            heldEnemy.StartDeathCountdown();
        }
        interactInputTime = 0;
        heldItem = null;
        isHoldingItem = false;
    }

    void RaiseShield() {
        shieldSound.Play();
        // Reduce the players move speed, and activate their shield object
        maxSpeed = slowedSpeed;
        shield.SetActive(true);
        isBlocking = true;
    }

    public void LowerShield() {
        // Return the players move speed, and de-activate their shield object
        maxSpeed = originalMaxSpeed;
        shield.SetActive(false);
        isBlocking = false;
    }

    public void CancelStrafe() {
        isStrafing = false;
    }

    // Apply knockback force to the players rigid body, and decrease it over time to smooth it out
    public void ApplyKnockBack() {
        rb.velocity = knockBackForce;
        knockBackMultiplyer *= 0.1f;
    }

    // Simple wait function that waits for the specified time in seconds
    private IEnumerator Wait(float waitTime) {
        yield return new WaitForSeconds(waitTime);
    }

    public void ReturnRigidbodyValues(Rigidbody rb, bool pickUp, bool putDown) {
        // When an item is being picked up
        if (pickUp) {
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.rotation = Quaternion.Euler(0, 0, 0);
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.isKinematic = true;
            //playerState = stateToGivePlayer;
        }
        // When an item is being put down
        else if (putDown) {
            rb.useGravity = true;
            rb.isKinematic = false;
            //playerState = stateToGivePlayer;
        } else {
            return;
        }
    }

    public void ThrowObject(Throwable thrownObject, Rigidbody thrownObjectRB, PlayerController thrownPlayer, GameObject playerThatsHolding, bool spinOnThrow) {
        throwSound.Play();
        // Unparent the held item object, and adjust variables in the damage script on it so it can be registered as thrown and harm enemies
        heldItem.transform.parent = null;
        // Adjust the rigidbody variables of the held object when it's thrown
        ReturnRigidbodyValues(thrownObjectRB, false, true);
        if (thrownObject != null) {
            thrownObject.isBeingThrown = true;
            thrownObject.damage *= interactInputTime;
            thrownObject.thrownDirection = thisTransform.forward;
        }
        if (thrownPlayer != null) {
            thrownPlayer.playerState = PlayerController.playerMode.beingThrown;
            thrownPlayer.isBeingHeld = false;
        }
        float throwSustain = 0;
        if(interactInputTime < 0.5f) {
            throwSustain = 0.5f;
        } else {
            throwSustain = interactInputTime;
        }
        // Apply a forwards and downWards force on the object based on how long the button has been held down
        thrownObjectRB.constraints = RigidbodyConstraints.None;
        thrownObjectRB.velocity = (playerThatsHolding.transform.forward * maxThrowForce * throwSustain) + (-thisTransform.up * (maxThrowForce / 4) * interactInputTime);

        // Option to add torque and spin the object when thrown
        if (spinOnThrow) {
            thrownObjectRB.AddTorque(Random.Range(0, 12), Random.Range(1, 2), Random.Range(-12, 12), ForceMode.Impulse);
        }

        EnemyHealth heldEnemy = heldItem.GetComponent<EnemyHealth>();
        if (heldEnemy != null) {
            heldEnemy.StartDeathCountdown();
        }

        // Reset the held item variables
        heldItem = null;
        isHoldingItem = false;
        isButtonDown = false;
        interactInputTime = 0;
    }

    public void TriggerExplosion() {
        explodeOnLand = false;
        SpawnExplosionParticles();
        cameraShake.ShakeCamera(1f);
        Vector3 explosionPos = thisTransform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, explosionRadius, enemyLayer);
        foreach (Collider hit in colliders) {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (!enemy.isDead) {
                enemy.KnockOverEnemy(throwable, true);
            }
        }
        powerUp.SetPowerUpActive(false);
        powerUp.SetTimer(0);
    }

    void SpawnExplosionParticles() {
        foreach(ParticleSystem particle in ExplosionParticles) {
            ParticleSystem newExplosion = Instantiate(particle, thisTransform.position, Quaternion.identity); 
            pauseMenu.AddItemToPause(newExplosion);
        }
        GameObject explosionFlash = Instantiate(explosionLight, thisTransform.position, Quaternion.identity);
    }

    private void OnCollisionEnter(Collision collision) {
        if (playerState == playerMode.beingThrown && explodeOnLand) {
            TriggerExplosion();
        }
        if (collision.gameObject.tag == "Ladder" && isInRangeOfLadder && isGrounded) {
            if (playerState == playerMode.move && !isDashing && !isHoldingItem && !isBlocking) {
                isInRangeOfLadder = false;
                rb.useGravity = false;
                rb.velocity = Vector3.zero;
                knockBackTimer = 0;
                playerState = playerMode.onLadder;
                theLadder = collision.gameObject;
                thisTransform.position = attatchPoint.position;
                thisTransform.rotation = theLadder.transform.rotation;
            }
        } else if (collision.gameObject.tag == "Player") {
            Throwable playerHitBy = collision.gameObject.GetComponent<Throwable>();
            if (playerHitBy.thrownByBoss) {
                if (invincibilityTimer < 0) {
                    if (playerState == playerMode.beingHeld) {
                        PlayerController parentPlayer = thisTransform.parent.GetComponent<PlayerController>();
                        if (parentPlayer != null) {
                            parentPlayer.PlaceObjectOnFloor();
                        }
                    }
                    RecieveDamage(collision.gameObject, (int)playerHitBy.damage);
                    if (playerState == playerMode.onLadder) {
                        thisTransform.position += -thisTransform.forward * 0.2f;

                        playerState = playerMode.move;
                        rb.useGravity = true;
                        thisTransform.rotation = Quaternion.Euler(0, thisTransform.rotation.y, thisTransform.rotation.z);
                    }
                }
            }
        } else if (collision.gameObject.tag == "Boss") {
            if (playerState != playerMode.beingThrown && invincibilityTimer < 0 && !throwable.thrownByBoss) {
                BossPatrol boss = collision.gameObject.GetComponent<BossPatrol>();
                hitSound.Play();
                if (playerState == playerMode.beingHeld) {
                    PlayerController parentPlayer = thisTransform.parent.GetComponent<PlayerController>();
                    if (parentPlayer != null) {
                        parentPlayer.PlaceObjectOnFloor();
                    }
                }
                RecieveDamage(collision.gameObject, (int)boss.damageToGive);
                if (playerState == playerMode.onLadder) {
                    thisTransform.position += -thisTransform.forward * 0.2f;

                    playerState = playerMode.move;
                    rb.useGravity = true;
                    thisTransform.rotation = Quaternion.Euler(0, thisTransform.rotation.y, thisTransform.rotation.z);
                }
            }

        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "LevelStart") {
            playerManager.playersReadyToStart++;
        } else if (other.tag == "Button") {
            other.GetComponent<PuzzleButton>().PressButton();
        } else if (other.tag == "LadderTrigger") {
            if (playerState != playerMode.onLadder) {
                // Set the attatch point to the entry transform on the ladder so the players position can be set correctly
                attatchPoint = other.transform.GetChild(0);
                isInRangeOfLadder = true;
            } else {
                playerState = playerMode.move;
                rb.useGravity = true;
                // Reset the players rotation based on the direction of the ladder
                if (thisTransform.rotation.x != 0) {
                    thisTransform.rotation = Quaternion.Euler(0, thisTransform.rotation.y, thisTransform.rotation.z);
                } else if (thisTransform.rotation.z != 0) {
                    thisTransform.rotation = Quaternion.Euler(thisTransform.rotation.x, thisTransform.rotation.y, 0);
                }
                // Set the players position to the exit point of the ladder
                Transform exitPoint = other.transform.GetChild(1);
                thisTransform.position = exitPoint.position;
            }
        }

        else if (other.tag == "Enemy") {
            if (playerState == playerMode.beingThrown && explodeOnLand) {
                TriggerExplosion();
            }

            if ((playerState != playerMode.beingThrown || !throwable.isBeingThrown) && invincibilityTimer < 0) {
                hitSound.Play();
                EnemyHealth enemyTouched = other.GetComponent<EnemyHealth>();
                EnemyPatrol enemyPatrol = other.GetComponent<EnemyPatrol>();
                if (enemyTouched.isDead) {
                    return;
                }
                RecieveDamage(other.gameObject, enemyPatrol.damageToGive);

                if (playerState == playerMode.onLadder) {
                    // If the player is on a ladder, change the knockback angle
                    //knockBackAngle = thisTransform.up * 1.125f;
                    thisTransform.position += -thisTransform.forward * 0.2f;

                    playerState = playerMode.move;
                    rb.useGravity = true;
                    thisTransform.rotation = Quaternion.Euler(0, thisTransform.rotation.y, thisTransform.rotation.z);
                }
            }
        }
    }
    public void SetInvincibilityTimer(float length) {
        invincibilityTimer = length;
    }

    private void OnTriggerExit(Collider other) {
        if (other.tag == "LevelStart") {
            playerManager.playersReadyToStart--;
        }
        if (other.tag == "LadderTrigger") {
            isInRangeOfLadder = false;
        }
    }

    public void RecieveDamage(GameObject objectHitBy, int damageTook) {
        // If the player collides with an enemy, generate a knock back force
        rb.isKinematic = false;
        Vector3 knockBackAngle = new Vector3(thisTransform.position.x, 0f, thisTransform.position.z) - new Vector3(objectHitBy.transform.position.x, 0f, objectHitBy.transform.position.z);
        thisTransform.rotation = Quaternion.Inverse(thisTransform.rotation);
        PlaceObjectOnFloor();
        cameraShake.ShakeCamera(0.6f);
        damageEffect.color = new Color(mesh.material.color.r, mesh.material.color.g, mesh.material.color.b, 0.75f);
        isButtonDown = false;
        interactInputTime = 0;
        knockBackForce = knockBackAngle * knockBackMultiplyer;

        // Subtract health from the player health Script
        if (invincibilityTimer < 0) {
            health.SubtractHealth(damageTook);
        }

        // Set timer for knock back length and invincibility frames
        if (playerState != playerMode.beingHeld) {
            knockBackTimer = knockBackLength;
        }
        SetInvincibilityTimer(invincibilityLength);
    }

    public void StopKnockBack() {
        knockBackTimer = 0;
        knockBackForce = Vector3.zero;
    }

    public void PausePhysics() {
        velocityToReturn = rb.velocity;
        rb.isKinematic = true;
    }

    public void ResumePhysics() {
        rb.isKinematic = false;
        rb.velocity = velocityToReturn;
    }

    IEnumerator LoadLevel(int sceneIndex, float delay) {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneIndex);
    }
}
