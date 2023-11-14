using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{

    private PlayerInput playerInput;
    private PlayerInput.OnGroundActions onGround;
    // Start is called before the first frame update
    void Awake()
    {
        playerInput = new PlayerInput();
        onGround = playerInput.OnGround;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        onGround.Enable();
    }
    private void OnDisable()
    {
        onGround.Disable();
    }
}
