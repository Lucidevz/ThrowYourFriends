using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossPatrol : MonoBehaviour
{
    public GameObject target;
    private PlayerHealth targetHealth;
    public int damageToGive;

    [SerializeField]
    private float speed;
    private NavMeshAgent agent;

    [SerializeField]
    BossController controller;
    [SerializeField]
    private PauseMenu pauseMenu;

    void Start() {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
    }

    void Update() {
        if (pauseMenu.IsTheGamePaused()) {
            agent.speed = 0;
        }

        if (target != null && agent.enabled) {
            agent.destination = target.transform.position;
            transform.LookAt(new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z));
        }

        if (targetHealth != null && targetHealth.isDead) {
            target = AssignNewTarget(controller.GetAllPlayers());
        }
    }

    public GameObject AssignNewTarget(List<GameObject> targets) {
        bool isPlayerAvailable = false;
        GameObject newTarget = null;
        float distanceToPlayer = float.MaxValue;
        foreach (GameObject player in targets) {
            if (!player.GetComponent<PlayerHealth>().isDead) {
                float tempDistanceToplayer = Vector3.Distance(transform.position, player.transform.position);
                if (tempDistanceToplayer < distanceToPlayer) {
                    distanceToPlayer = tempDistanceToplayer;
                    newTarget = player.gameObject;
                    isPlayerAvailable = true;
                }
            }
        }
        if (!isPlayerAvailable) {
            // If all players are dead isPlayerAvailable with be false so stop boss from moving
            FreezeAgent();
            return null;
        }
        targetHealth = newTarget.GetComponent<PlayerHealth>();
        return newTarget;
    }

    public void SetTarget(GameObject newTarget) {
        target = newTarget;
    }

    public void FreezeAgent() {
        agent.speed = 0;
    }

    public void UnFreezeAgent() {
        agent.speed = speed;
    }
}
