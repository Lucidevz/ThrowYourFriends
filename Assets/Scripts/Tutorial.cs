using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Tutorial : MonoBehaviour {
    public Vector3[] cameraPositions, room1PlayerPositions, room2PlayerPositions, room3PlayerPositions, room4PlayerPositions, room5PlayerPositions, pressurePadPositions;

    [SerializeField]
    [Tooltip("The amount of time a box has to be placed on the pad for the next room to start")]
    private float timeToOpenDoor;
    private float timer;

    private int currentRoom = 1;

    [SerializeField]
    private Camera cam;
    [SerializeField]
    private PlayerManager playerManager;

    public Image blackOverlay;

    // Start is called before the first frame update
    void Start() {
        timer = timeToOpenDoor;
        //FadeOutBlackScreen();
    }

    // Update is called once per frame
    void Update() {
        if (timer < 0) {
            MoveToNextRoom(currentRoom);
        }
    }

    private void OnTriggerStay(Collider other) {
        if(other.tag == "PickUp") {
            timer -= Time.deltaTime;
        }
    }

    private void OnTriggerExit(Collider other) {
        if(other.tag == "PickUp") {
            timer = timeToOpenDoor;
        }
    }

    void MoveToNextRoom(int roomNumber) {
        switch (roomNumber) {
            case (0):
                StartCoroutine(MoveToNextRoom(room1PlayerPositions, roomNumber));
                break;
            case (1):
                StartCoroutine(MoveToNextRoom(room2PlayerPositions, roomNumber));
                break;
            case (2):
                StartCoroutine(MoveToNextRoom(room3PlayerPositions, roomNumber));
                break;
            case (3):
                StartCoroutine(MoveToNextRoom(room4PlayerPositions, roomNumber));
                break;
            case (4):
                FadeInBlackScreen();
                StartCoroutine(LoadScene());
                break;
            case (5):
                //SceneManager.LoadScene(0);
                break;
        }
        timer = timeToOpenDoor;

    }

    private IEnumerator MoveToNextRoom(Vector3[] newPositions, int nextRoom) {
        FadeInBlackScreen();
        yield return new WaitForSeconds(1f);
        for(int i = 0; i < playerManager.players.Count; i++) {
            playerManager.players[i].GetComponent<PlayerController>().ResetPlayerVariables();
            playerManager.players[i].transform.position = newPositions[i];
        }
        cam.transform.position = cameraPositions[nextRoom];
        transform.position = pressurePadPositions[nextRoom];
        currentRoom++;
        FadeOutBlackScreen();
    }

    private IEnumerator LoadScene() {
       yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(0);
    }

    public void FadeInBlackScreen() {
        blackOverlay.CrossFadeColor(Color.blue, 1f, false, true);
    }

    public void FadeOutBlackScreen() {
        blackOverlay.CrossFadeColor(Color.clear, 1f, false, true);
    }
}