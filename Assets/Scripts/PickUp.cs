using UnityEngine;

public class PickUp : MonoBehaviour {
    public enum PickUpOptions {
        health,
        helmet,
        bomb,
        chicken,
    }
    private enum PickUpTypes {
        health,
        helmet,
        bomb,
        chicken,
    }

    [SerializeField]
    private PickUpTypes PickUpType;

    public Mesh[] powerUpMeshes;
    public Material[] powerUpMateirals;
    public GameObject[] powerUpObjects;

    private float pickUpDelay;

    private void Start() {
        pickUpDelay = 0.5f;
    }

    private void Update() {
        pickUpDelay -= Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Player" && pickUpDelay < 0) {
            PowerUp playerPowerUp = other.GetComponent<PowerUp>();
            switch (PickUpType) {
                case (PickUpTypes.health):
                    playerPowerUp.ActivatePowerUp(PowerUp.PowerUpTypes.health, this.gameObject);
                    break;
                case (PickUpTypes.helmet):
                    playerPowerUp.ActivatePowerUp(PowerUp.PowerUpTypes.helmet, this.gameObject);
                    break;
                case (PickUpTypes.bomb):
                    playerPowerUp.ActivatePowerUp(PowerUp.PowerUpTypes.bomb, this.gameObject);
                    break;
                case (PickUpTypes.chicken):
                    playerPowerUp.ActivatePowerUp(PowerUp.PowerUpTypes.chicken, this.gameObject);
                    break;
            }
        }
    }

    public void SetPickUpType(PickUpOptions type) {
        GameObject powerUpMesh = null;
        switch (type) {
            case (PickUpOptions.health):
                PickUpType = PickUpTypes.health;
                powerUpMesh = Instantiate(powerUpObjects[0], transform.position, Quaternion.identity);
                powerUpMesh.transform.parent = transform;
                break;
            case (PickUpOptions.helmet):
                PickUpType = PickUpTypes.helmet;
                powerUpMesh = Instantiate(powerUpObjects[1], transform.position, Quaternion.identity);
                powerUpMesh.transform.parent = transform;
                break;
            case (PickUpOptions.bomb):
                PickUpType = PickUpTypes.bomb;
                powerUpMesh = Instantiate(powerUpObjects[2], transform.position, Quaternion.identity);
                powerUpMesh.transform.parent = transform;
                break;
            case (PickUpOptions.chicken):
                PickUpType = PickUpTypes.chicken;
                powerUpMesh = Instantiate(powerUpObjects[3], transform.position, Quaternion.identity);
                powerUpMesh.transform.parent = transform;
                break;
        }
    }
}
