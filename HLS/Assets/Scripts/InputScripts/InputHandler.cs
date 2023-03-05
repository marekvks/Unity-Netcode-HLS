using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    // Player Input Actions
    private PlayerInputActions _playerInputActions;
    private PlayerInputActions.PlayerActions _playerActions;

    // Public variables
    [HideInInspector] public Vector2 MovementInput;

    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
        _playerActions = _playerInputActions.Player;
        SetupMovement();
    }

    private void Update()
    {
        MovementInput = _playerActions.Movement.ReadValue<Vector2>();
    }

    private void SetupMovement()
    {
        _playerActions.Enable();
    }

    public void SubscribeToJump(Action<UnityEngine.InputSystem.InputAction.CallbackContext> jumpFunction) => _playerActions.Jump.performed += jumpFunction;

    public void SubscribeToShoot(Action<UnityEngine.InputSystem.InputAction.CallbackContext> shootFunction) => _playerActions.Shoot.performed += shootFunction;

    public void SubscribeToPause(Action<UnityEngine.InputSystem.InputAction.CallbackContext> pauseFunction) => _playerActions.Pause.performed += pauseFunction;
}