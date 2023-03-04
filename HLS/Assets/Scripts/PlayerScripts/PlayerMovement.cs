using NetworkScripts;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace PlayerScripts
{
    public class PlayerMovement : NetworkBehaviour
    {
        void Start()
        {
            if (!IsOwner) Destroy(this);
        }
        void Update()
        {
            Vector3 moveDir = new Vector3(0, 0, 0);

            if (Input.GetKey(KeyCode.W)) moveDir.z = +1f;
            if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
            if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
            if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;

            float moveSpeed = 3f;
            transform.position += moveDir * (moveSpeed * Time.deltaTime);
        }
    }
}
