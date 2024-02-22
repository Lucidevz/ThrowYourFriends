using UnityEngine;

public class ReduceLightIntensity : MonoBehaviour
{
    private Light thisLight;
    private float originalIntensity;
    [Range(0.1f, 10f)]
    public float timeToFadeOut;

    // Start is called before the first frame update
    void Start()
    {
        thisLight = GetComponent<Light>();
        originalIntensity = thisLight.intensity;
    }

    // Update is called once per frame
    void Update()
    {
        thisLight.intensity -= Time.deltaTime * originalIntensity / timeToFadeOut;

        if(thisLight.intensity <= 0) {
            Destroy(gameObject);
        }
    }
}
