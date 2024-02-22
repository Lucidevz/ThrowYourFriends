using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth;
    private int health;

    private PlayerManager playerManager;
    private PlayerController controller;
    // The area the player is sent to after they die, to keep them in the level before spawning them back in
    private GameObject playerWaitingZone;
    public bool isDead;

    private bool isProtected; // If the player has a helmet power-up to stop them recieving damage

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        playerManager = FindObjectOfType<PlayerManager>();
        controller = GetComponent<PlayerController>();
        playerWaitingZone = GameObject.FindGameObjectWithTag("Respawn");
    }

    private void Update() {
        if(health <= 0) {
            DisablePlayer();
            transform.position = playerWaitingZone.transform.position;
        }
    }

    public void SubtractHealth(int amountToTake) {
        if (!isProtected) {
            health -= amountToTake;
        }
    }

    public void IncreaseHealth(int amountToGive) {
        health += amountToGive;
        if(health >= maxHealth) {
            health = maxHealth;
        }
    }

    public void SetHealthToMax() {
        health = maxHealth;
    }

    public int GetHealth() {
        return health;
    }

    public void SetIsProtected(bool value) {
        isProtected = value;
    }

    public void DisablePlayer() {
        transform.rotation = Quaternion.Inverse(transform.rotation);
        if (controller != null && controller.isHoldingItem) {
            controller.PutObjectDown();
        }
        controller.DisableControls();
        transform.position = playerWaitingZone.transform.position;
        isDead = true;
        playerManager.ChangeNumberOfActivePlayers(-1);
        SetHealthToMax();
    }
}
