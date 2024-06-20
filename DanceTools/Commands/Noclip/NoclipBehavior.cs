using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DanceTools
{
    public class NoclipBehaviour : MonoBehaviour
    {
        private Vector3 position = Vector3.zero;
        private bool isCrouching = false;
        void Awake()
        {
            var controller = this.gameObject.GetComponent<PlayerControllerB>();

            position = this.transform.position;
            isCrouching = controller.isCrouching;
        }
        void Update()
        {
            if (!DanceTools.playerNoclipping)
            {
                Destroy(this.GetComponent<NoclipBehaviour>());
            }
            else
            {
                var controller = this.gameObject.GetComponent<PlayerControllerB>();
                var camera = controller.gameplayCamera;

                var input = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Move").ReadValue<Vector2>();
                var crouch = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Crouch").ReadValue<float>() > 0.5;

                isCrouching = crouch;

                var inputY = 0;

                if (Keyboard.current.eKey.isPressed)
                {
                    inputY += 1;
                }

                if (Keyboard.current.qKey.isPressed)
                {
                    inputY -= 1;
                }

                var moveVector = Vector3.zero;

                moveVector += (camera.transform.right * input.x) + (camera.transform.forward * input.y) + (camera.transform.up * inputY);
                position += moveVector * (Time.deltaTime * ((controller.isSpeedCheating && !isCrouching) ? controller.isSprinting ? 200f : 65f : controller.isSprinting ? 30f : isCrouching ? 5f : 15f));

                controller.TeleportPlayer(position);
                controller.ResetFallGravity();
                if (isCrouching != controller.isCrouching)
                {
                    controller.Crouch(isCrouching);
                }
            }
        }
    }
}