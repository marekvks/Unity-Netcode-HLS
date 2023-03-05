using NetworkScripts;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace PlayerScripts
{
    [RequireComponent(typeof(CharacterController), typeof(InputHandler))]
    public class PlayerMovement : NetworkBehaviour
    {
        // Scripts
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private CharacterController _controller;

        [Header("Movement Settings")]
        [SerializeField] private float speed = 3f;

        private Vector2 _movementInput;

        void Start()
        {
            if (!IsOwner) Destroy(this);
        }

        void Update()
        {
            _movementInput = _inputHandler.MovementInput;
            if (IsHost)
            {
                //Move();
                Rotate();
                return;
            }

            MoveServerRpc();
        }

        private void Rotate()
        {
            Vector2 mouseScreenPos = Input.mousePosition;
            Vector2 playerScreenPos = Camera.main.WorldToScreenPoint(transform.position);
            Vector2 lookDirection = mouseScreenPos - playerScreenPos;
            float angle = Mathf.Atan2(lookDirection.x, lookDirection.y) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0f, angle, 0f));
        }

        private void Move()
        {
            _controller.Move(transform.forward * speed * Time.deltaTime);
        }

        [ServerRpc]
        private void MoveServerRpc()
        {
            Move();
            Rotate();
        }
    }
}
