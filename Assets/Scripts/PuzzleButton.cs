using UnityEngine;

public class PuzzleButton : MonoBehaviour
{
    public bool canBePressed;
    public bool hasBeenPressed;
    [SerializeField]
    private GameObject levelTrigger;
    [SerializeField]
    private Vector3 positionToSetTrigger;
    [SerializeField]
    private MeshRenderer mesh;

    public void PressButton() {
        if (canBePressed) {
            hasBeenPressed = true;
            levelTrigger.transform.localPosition = positionToSetTrigger;
            mesh.enabled = false;
        }
    }
}
