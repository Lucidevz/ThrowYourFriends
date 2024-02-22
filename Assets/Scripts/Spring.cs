using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring : MonoBehaviour
{
    public enum directions {
        [Tooltip("The direction the player is facing")]
        playerForwardsDirection,
        [Tooltip("The direction the spring is facing (in the Z axis)")]
        springForwardsDirection,

    }

    public directions forceDirections;
    public float force;
    public float upwardsForce;

    public void LaunchPlayer(GameObject thrownObject) {
        Rigidbody rB = thrownObject.GetComponent<Rigidbody>();
        Throwable thrown = thrownObject.GetComponent<Throwable>();
        if(rB != null ) {
            if (forceDirections == directions.playerForwardsDirection) {
                rB.velocity = thrown.thrownDirection * force;
                rB.AddForce(transform.up * upwardsForce, ForceMode.Impulse);
            }
            else if (forceDirections == directions.springForwardsDirection) {
                rB.velocity = transform.forward * force;
                rB.AddForce(transform.up * upwardsForce, ForceMode.Impulse);
            }
        }
    }
}
