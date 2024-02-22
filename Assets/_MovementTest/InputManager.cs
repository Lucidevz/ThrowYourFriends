using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public Gamepad[] controllers;
    public List<GameObject> players = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        //var p1 = PlayerInput.Instantiate(playerPrefab, controlScheme: "Gamepad", pairWithDevice: Gamepad.all[0]);

        //controllers = Gamepad.all;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
