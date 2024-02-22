using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class WonGame : MonoBehaviour
{
    [SerializeField]
    private Animator blackOverlay;

    public void ReturnToMenu(InputAction.CallbackContext context) {
        if (context.performed) {
            blackOverlay.SetTrigger("FadeIn");
            StartCoroutine(LoadSceneWithDelay(1, 0));
        }
    }

    IEnumerator LoadSceneWithDelay(float delay, int sceneIndex) {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneIndex);
    }
}
