using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PowerUp : MonoBehaviour {
    public enum PowerUpTypes {
        health,
        helmet,
        bomb,
        chicken,
    }

    public PowerUpTypes ActivatedPowerUp;

    [SerializeField]
    private float powerUpLength;
    private float timer;

    private bool powerUpActive;

    [SerializeField]
    private PlayerController playerController;
    private PlayerManager playerManager;
    [SerializeField]
    private PlayerHealth health;
    private EnemySpawner enemySpawner;
    private PauseMenu pauseMenu;

    public bool isAChicken;

    private void Start() {
        //playerController = GetComponent<PlayerController>();
        playerManager = GameObject.FindGameObjectWithTag("PlayerManager").GetComponent<PlayerManager>();
        //health = GetComponent<PlayerHealth>();
        enemySpawner = GameObject.FindGameObjectWithTag("Enemy Spawner").GetComponent<EnemySpawner>();
        pauseMenu = GameObject.FindGameObjectWithTag("Pause").GetComponent<PauseMenu>();
    }

    // Update is called once per frame
    void Update() {
        if (powerUpActive && !pauseMenu.IsTheGamePaused()) {
            timer -= Time.deltaTime;
            if (timer <= 0) {
                DeActivatePowerUp();
            }
        }
    }

    public void ActivatePowerUp(PowerUpTypes type, GameObject powerUpObject) {
        if (powerUpActive) {
            return;
        }

        Destroy(powerUpObject);

        ActivatedPowerUp = type;
        
        if(ActivatedPowerUp == PowerUpTypes.health) {
            health.IncreaseHealth(25);
            return;
        }

        switch (ActivatedPowerUp) {
            case (PowerUpTypes.helmet):
                playerManager.helmetPowerUpInLevel = true;
                health.SetIsProtected(true);
                break;
            case (PowerUpTypes.bomb):
                playerManager.bombPowerUpInLevel = true;
                playerController.explodeOnLand = true;
                break;
            case (PowerUpTypes.chicken):
                foreach(GameObject enemy in enemySpawner.allEnemies){
                    enemy.GetComponent<EnemyPatrol>().AssignSpecificTarget(this.gameObject);
                }
                isAChicken = true;
                playerManager.chickenPowerUpInLevel = true;
                break;
        }

        timer = powerUpLength;
        powerUpActive = true;

    }

    void DeActivatePowerUp() {
        timer = powerUpLength;
        powerUpActive = false;
        switch (ActivatedPowerUp) {
            case (PowerUpTypes.helmet):
                health.SetIsProtected(false);
                playerManager.helmetPowerUpInLevel = false;
                break;
            case (PowerUpTypes.bomb):
                playerController.explodeOnLand = false;
                playerManager.bombPowerUpInLevel = false;
                break;
            case (PowerUpTypes.chicken):
                playerManager.chickenPowerUpInLevel = false;
                isAChicken = false;
                // Set the enemies to lock onto a new random target
                foreach (GameObject enemy in enemySpawner.allEnemies) {
                    EnemyPatrol enemyAgent = enemy.GetComponent<EnemyPatrol>();
                    enemyAgent.target = null;
                    enemyAgent.target = enemyAgent.AssignNewTarget();
                }
                break;
        }
    }

    public void SetPowerUpActive(bool value) {
        powerUpActive = value;
    }

    public float GetPowerUpLength() {
        return powerUpLength;
    }

    public float GetTimer() {
        return timer;
    }

    public void SetTimer(float value) {
        timer = value;
    }

}
