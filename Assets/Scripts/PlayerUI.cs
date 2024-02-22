using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour {
    [Header("Health Bar Variables")]
    public Image healthBar;
    private Vector2 healthBarScale;

    [Header("PowerUp Timer Variables")]
    public Image powerUpTimer;
    private Vector2 powerUpTimerScale;

    [Header("Throwing Bar Variables")]
    public GameObject throwingArrow;
    private Vector3 throwingArrowStartPosition;
    [SerializeField]
    private float throwingArrowExtendedDistance; // Max position on Z axis when extending forwards from input
    private MeshRenderer throwingArrowMeshRenderer;

    Color playerColor;
    /*public Image throwingBar;
    public Image throwingBarBackground;
    private float throwingBarWidth;
    private Color throwingBarColor;*/

    private PlayerController playerController;
    private PlayerHealth playerHealth;
    private PlayerManager playerManager;
    private PowerUp powerUp;
    private Camera cam;

    public GameObject directionalArrow;

    // Start is called before the first frame update
    void Start() {

        playerController = GetComponent<PlayerController>();
        playerHealth = GetComponent<PlayerHealth>();
        playerManager = FindObjectOfType<PlayerManager>();
        powerUp = GetComponent<PowerUp>();
        cam = Camera.main;
        throwingArrowStartPosition = throwingArrow.transform.localPosition;
        throwingArrowMeshRenderer = throwingArrow.GetComponent<MeshRenderer>();

        playerColor = GetComponent<MeshRenderer>().material.color;
        directionalArrow.GetComponent<MeshRenderer>().material.color = playerColor;

        switch (transform.name) {
            case ("Player1"):
                healthBar = GameObject.Find("Player1HealthBar").GetComponent<Image>();
                powerUpTimer = GameObject.Find("Player1PowerUpBar").GetComponent<Image>();
                break;
            case ("Player2"):
                healthBar = GameObject.Find("Player2HealthBar").GetComponent<Image>();
                powerUpTimer = GameObject.Find("Player2PowerUpBar").GetComponent<Image>();
                break;
            case ("Player3"):
                healthBar = GameObject.Find("Player3HealthBar").GetComponent<Image>();
                powerUpTimer = GameObject.Find("Player3PowerUpBar").GetComponent<Image>();
                break;
            case ("Player4"):
                healthBar = GameObject.Find("Player4HealthBar").GetComponent<Image>();
                powerUpTimer = GameObject.Find("Player4PowerUpBar").GetComponent<Image>();
                break;
        }

        healthBarScale = healthBar.rectTransform.localScale;
        powerUpTimerScale = powerUpTimer.rectTransform.localScale;
        SetUIBarsVisible(true);
        //throwingBarWidth = throwingBar.rectTransform.localScale.x;
        //throwingBarColor = throwingBar.color;
    }

    // Update is called once per frame
    void Update() {
        if (playerManager.currentLevel == PlayerManager.level.Lobby) {
            //SetUIBarsVisible(false);
        } else {
            SetUIBarsVisible(true);
        }

        SetHealthUIBar();
        SetPowerUpTimer();

        //SetThrowingUIBar();
        
        HandleDirectionArrow();
        ThrowningArrow();

    }

    void SetUIBarsVisible(bool visible) {
        if (visible) {
            healthBar.enabled = true;
            //throwingBar.enabled = true;
            //throwingBarBackground.enabled = true;
        } else {
            healthBar.enabled = false;
            //throwingBar.enabled = false;
            //throwingBarBackground.enabled = false;
        }
    }

    void SetHealthUIBar() {

        if (playerHealth.isDead) {
            healthBar.enabled = false;
        } else {
            healthBar.enabled = true;
        }
        float healthBarScalar = 0;
        float playersHealth = playerHealth.GetHealth();

        healthBarScalar = (playersHealth * (100/playerHealth.maxHealth)) / 100;

        healthBar.rectTransform.localScale = new Vector2 (Mathf.Lerp(healthBar.rectTransform.localScale.x, healthBarScalar, 0.25f), healthBarScale.y);
    }

    void SetPowerUpTimer() {

        if (playerHealth.isDead) {
            powerUpTimer.enabled = false;
        } else {
            powerUpTimer.enabled = true;
        }
        float powerUpTimerScalar = 0;
        float powerUpTime = powerUp.GetTimer();
        float powerUpLength = powerUp.GetPowerUpLength();

        powerUpTimerScalar = powerUpTime * (100 / powerUpLength) / 100;

        if(powerUpTime > 0) {
            powerUpTimer.rectTransform.localScale = new Vector2(powerUpTimerScalar, powerUpTimerScale.y);
        } else {
            powerUpTimer.rectTransform.localScale = new Vector2(1, powerUpTimerScale.y);
        }
    }

    void HandleDirectionArrow() {
        if (playerController.playerState == PlayerController.playerMode.move) {
            // Only Display the players directional arrow when they can move around
            directionalArrow.SetActive(true);
        } else {
            directionalArrow.SetActive(false);
        }
    }

    void ThrowningArrow() {
        if(playerController.interactInputTime > 0.2) {
            float extensionDistance = (playerController.interactInputTime * (throwingArrowExtendedDistance - throwingArrowStartPosition.z));
            throwingArrow.transform.localPosition = new Vector3(throwingArrow.transform.localPosition.x, throwingArrow.transform.localPosition.y, throwingArrowStartPosition.z + extensionDistance);
            if(playerController.interactInputTime >= 0.99) {
                throwingArrowMeshRenderer.material.color = Color.white;
            } else {
                throwingArrowMeshRenderer.material.color = playerColor;
            }
        } else {
            throwingArrow.transform.localPosition = throwingArrowStartPosition;
            throwingArrowMeshRenderer.material.color = playerColor;

        }
    }
}
