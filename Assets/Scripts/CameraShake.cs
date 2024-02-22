using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField]
    private Transform camTransform;
    [SerializeField]
    private float maxShakeAngle;
    private float shake;
    private float shakeForce;

    private Quaternion originalRotation;

    // Start is called before the first frame update
    void Start()
    {
        originalRotation = camTransform.rotation;
    }

    public void ShakeCamera(float shakeAmount) {
        shake = shakeAmount;
        StartCoroutine(Shake());
    }

    Vector3 shakeAngle;

    IEnumerator Shake() {
        while (shake > 0) {
            shakeForce = Mathf.Pow(shake, 3f);
            Vector3 rotaion = originalRotation.eulerAngles;
            float randomNumber = Random.Range(-1f, 1f);
            shakeAngle = new Vector3(
                (maxShakeAngle * shakeForce * Mathf.PerlinNoise(Time.time, Time.time) * randomNumber),
                (maxShakeAngle * shakeForce * Mathf.PerlinNoise(Time.time, Time.time) * randomNumber),
                (maxShakeAngle * shakeForce * Mathf.PerlinNoise(Time.time, Time.time) * randomNumber));

            rotaion = new Vector3(rotaion.x + shakeAngle.x, rotaion.y + shakeAngle.y, rotaion.z + shakeAngle.z);
            camTransform.rotation = Quaternion.Euler(rotaion);

            //camTransform.localRotation = Quaternion.EulerAngles(shakeAngle);
            //camTransform.Rotate(shakeAngle);
            shake -= Time.deltaTime;
            yield return null;
        }  
        shake = 0;
        shakeForce = 0;
        camTransform.rotation = originalRotation;
        
    }
}
