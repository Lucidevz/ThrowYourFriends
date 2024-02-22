using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPickUp : MonoBehaviour
{
    [SerializeField]
    private Transform bossTransform;
    private GameObject heldPlayer;
    private bool isHoldingPlayer;
    private GameObject playerToThrowAt;
    private Vector3 throwDirection;
    [SerializeField]
    private SphereCollider pickUpPlayerCollider;

    [SerializeField]
    BossController controller;
    [SerializeField]
    private BossPatrol bossPatrol;

    [Header("Sound varaibles")]
    [SerializeField]
    private AudioSource pickUpSound;
    [SerializeField]
    private AudioSource throwSound;

    private void Update() {
        if (isHoldingPlayer && !controller.isDead) {
            if(playerToThrowAt != null) {
                transform.LookAt(new Vector3(playerToThrowAt.transform.position.x, bossTransform.position.y, playerToThrowAt.transform.position.z));
                // Get the direction to thow the player (adds on 1 forward unit vector)
                throwDirection = (playerToThrowAt.transform.position
                    - heldPlayer.transform.position);
            } else {
                transform.LookAt(bossTransform.position + bossTransform.forward * 10);
                // Get the direction to thow the player (adds on 1 forward unit vector)
                throwDirection = (bossTransform.position + bossTransform.forward * 10)
                    - heldPlayer.transform.position;
            }
        }
    }

    public void PickUpPlayer(GameObject playerToPickUp) {
        if (controller.isDead) {
            return;
        }
        if (!isHoldingPlayer) {
            heldPlayer = playerToPickUp;
            PlayerController otherPlayer = heldPlayer.GetComponent<PlayerController>();
            if (otherPlayer != null) {
                if(otherPlayer.playerState != PlayerController.playerMode.move) {
                    return;
                }
                otherPlayer.PlaceObjectOnFloor();
                otherPlayer.StopKnockBack();
                otherPlayer.ReturnRigidbodyValues(otherPlayer.GetComponent<Rigidbody>(), true, false);
                otherPlayer.playerState = PlayerController.playerMode.beingHeld;
            }
            pickUpSound.Play();
            // Reset the hit ground variable in picked up objects throwable script
            Throwable otherThrowable = heldPlayer.GetComponent<Throwable>();
            otherThrowable.hasHitSomething = false;

            // Set the position of the picked up object above the player
            Vector3 pickUpPos = new Vector3(bossTransform.position.x, (bossTransform.position.y + bossTransform.localScale.y) + (heldPlayer.transform.localScale.y + 0.25f), bossTransform.position.z);
            heldPlayer.transform.position = pickUpPos;
            heldPlayer.transform.rotation = bossTransform.transform.rotation;
            heldPlayer.transform.parent = transform;

            bossPatrol.FreezeAgent();
            playerToThrowAt = GetPlayerToThrowAt(controller.GetAllPlayers());
            isHoldingPlayer = true;
            TogglePickUpCollision(false);
            StartCoroutine(PrepareToThrow());
        }
    }

    public GameObject GetPlayerToThrowAt(List<GameObject> targets) {
        bool isPlayerAvailable = false;
        GameObject newTarget = null;
        float distanceToPlayer = float.MaxValue;
        foreach (GameObject player in targets) {
            if (player.name != heldPlayer.name && !player.GetComponent<PlayerHealth>().isDead) {
                float tempDistanceToplayer = Vector3.Distance(transform.position, player.transform.position);
                if (tempDistanceToplayer < distanceToPlayer) {
                    distanceToPlayer = tempDistanceToplayer;
                    newTarget = player.gameObject;
                    isPlayerAvailable = true;
                }
            }
        }
        if (!isPlayerAvailable) {
            return null;
        }
        return newTarget;
    }

    IEnumerator PrepareToThrow() {
        yield return new WaitForSeconds(2f);
        ThrowPlayer();
        yield return new WaitForSeconds(3f);
        TogglePickUpCollision(true);
    }

    public void ThrowPlayer() {
        if (controller.isDead) {
            return;
        }
        throwSound.Play();
        Rigidbody playerRB = heldPlayer.GetComponent<Rigidbody>();
        // Set the held players rigidbody variables
        playerRB.constraints = RigidbodyConstraints.None;
        playerRB.useGravity = true;
        playerRB.isKinematic = false;
        heldPlayer.transform.parent = null;
        // Apply force and torque the player being held
        heldPlayer.transform.position += bossTransform.forward;
        playerRB.AddForce(throwDirection * 10, ForceMode.Impulse);
        playerRB.AddTorque(Random.Range(0, 12), Random.Range(1, 2), Random.Range(-12, 12), ForceMode.Impulse);
        // Set varaibles in the player controller and throwable script to account for the player being thrown
        heldPlayer.GetComponent<PlayerController>().playerState = PlayerController.playerMode.beingThrown;
        Throwable heldPalyerThrowable = heldPlayer.GetComponent<Throwable>();
        heldPalyerThrowable.isBeingThrown = true;
        heldPalyerThrowable.thrownByBoss = true;

        // Reset the held player variables
        heldPlayer = null;
        isHoldingPlayer = false;
        bossPatrol.UnFreezeAgent();
    }

    public void TogglePickUpCollision(bool value) {
        pickUpPlayerCollider.enabled = value;
    }
}
