using System;
using UnityEngine;
using UnityEngine.InputSystem; 

public class TouchManager : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction pressAction;


    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        pressAction = playerInput.actions["Touch"]; 
    }

    private void OnEnable()
    {
        pressAction.performed += PressPerformed;
    }

    private void OnDisable()
    {
        pressAction.performed -= PressPerformed;
    }

    private void PressPerformed(InputAction.CallbackContext context)
    {
        
    }
}
