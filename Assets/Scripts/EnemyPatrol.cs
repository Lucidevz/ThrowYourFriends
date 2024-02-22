using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrol : MonoBehaviour
{
    public GameObject target;
    private PlayerHealth targetHealth;
    public int damageToGive;
    public PlayerController[] players;

    [Tooltip("A minimum and maximum range for generating a random move speed for the enemy")]
    public Vector2 minMaxSpeedRange;
    private NavMeshAgent agent;
    private float originalSpeed;

    private bool isPlayerAvailable = false;

    [SerializeField]
    private PauseMenu pauseMenu;

    // Start is called before the first frame update
    void Start() {
        players = FindObjectsOfType<PlayerController>();

        agent = GetComponent<NavMeshAgent>();
        originalSpeed = Random.Range(minMaxSpeedRange.x, minMaxSpeedRange.y);
        agent.speed = originalSpeed;

        if (players.Length > 0) {
            target = AssignNewTarget();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null && agent.enabled) {
            agent.destination = target.transform.position;
            transform.LookAt(new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z));

            if (targetHealth.isDead) {
                target = AssignNewTarget();
            }
        }
    }

    public GameObject AssignNewTarget() {
        isPlayerAvailable = false;
        for (int i = 0; i < players.Length; i++) {
            // If there is at least one player not dead, set isPlayerAvailable to true
            if (!players[i].GetComponent<PlayerHealth>().isDead) {
                isPlayerAvailable = true;
                // If the player has chicken power up enabled (and isn't dead), set target to them
                if (players[i].GetComponent<PowerUp>().isAChicken) {
                    AssignSpecificTarget(players[i].gameObject);
                    return players[i].gameObject;
                }
            }
        }
        
        if (!isPlayerAvailable) {
            // If all players are dead isPlayerAvailable with be false so let enmies wander around the level
            StartCoroutine(Wander());
            return null;
        } else {
            do {
                int randomPlayer = Random.Range(0, players.Length);
                GameObject newTarget = players[randomPlayer].gameObject;
                if (newTarget.GetComponent<PlayerHealth>().isDead == false) {
                    target = newTarget;
                }
            }
            while (target == null);

            targetHealth = target.GetComponent<PlayerHealth>();
        }

        return target;
    }

    public void AssignSpecificTarget(GameObject newTarget) {
        target = newTarget;
        targetHealth = target.GetComponent<PlayerHealth>();
    }

    IEnumerator Wander() {
        if (pauseMenu.IsTheGamePaused()) {
            yield return null;
        }
        Vector3 randomLocation = new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
        agent.destination = transform.position + randomLocation;
        yield return new WaitForSeconds(Random.Range(3f, 7f));
        StartCoroutine(Wander());
    }

    public void FreezeAgent() {
        agent.speed = 0;
    }

    public void UnFreezeAgent() {
        agent.speed = originalSpeed;
    }
}
