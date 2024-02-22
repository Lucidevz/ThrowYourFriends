using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGravity : MonoBehaviour {

    private CustomGravityManager manager;
    private Rigidbody rb;
    private float customGravityScale;
    private float originalGravity;

    public bool accelWhenFalling;
    private float accelRate;

    private void Start() {
        
    }

/*    private void FixedUpdate() {
        if (rb != null && !rb.isKinematic *//*&& rb.velocity.y != 0*//* ) {
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y + (customGravityScale / 100), rb.velocity.z);
        }
    }

    private void Update() {
        if (accelWhenFalling) {
            if (rb != null && rb.velocity.y < 0) {
                customGravityScale -= accelRate * Time.deltaTime;
            } else if (rb != null && rb.velocity.y >= 0) {
                customGravityScale = originalGravity;
            }
        }
    }*/
}
