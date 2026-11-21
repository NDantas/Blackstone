using UnityEngine;
using UnityEngine.InputSystem;

namespace Blackstone.Player
{
    /// <summary>
    /// Controle em primeira pessoa da Build 001:
    /// WASD para andar, Shift para andar mais rápido, mouse para olhar.
    /// O cursor fica preso na tela; clicar na janela prende de novo se ele soltar.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Referências")]
        [Tooltip("Objeto que gira para cima/baixo (pai da câmera).")]
        [SerializeField] private Transform cameraRoot;

        [Header("Movimento")]
        [SerializeField] private float walkSpeed = 2.2f;
        [SerializeField] private float sprintSpeed = 3.6f;
        [SerializeField] private float acceleration = 12f;
        [SerializeField] private float gravity = -20f;

        [Header("Olhar")]
        [Tooltip("Graus de rotação por pixel de movimento do mouse.")]
        [SerializeField] private float mouseSensitivity = 0.1f;
        [SerializeField] private float minPitch = -85f;
        [SerializeField] private float maxPitch = 85f;

        private CharacterController controller;
        private Vector3 horizontalVelocity;
        private float verticalVelocity;
        private float pitch;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            if (cameraRoot == null && Camera.main != null)
            {
                cameraRoot = Camera.main.transform;
            }
        }

        private void OnEnable() => SetCursorLocked(true);
        private void OnDisable() => SetCursorLocked(false);

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus) SetCursorLocked(true);
        }

        private void Update()
        {
            // Se o cursor soltou (Alt+Tab, por exemplo), um clique prende de novo.
            var mouse = Mouse.current;
            if (Cursor.lockState != CursorLockMode.Locked && mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                SetCursorLocked(true);
            }

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Look();
            }

            Move();
        }

        private void Look()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            Vector2 delta = mouse.delta.ReadValue() * mouseSensitivity;

            // Esquerda/direita gira o corpo inteiro.
            transform.Rotate(Vector3.up, delta.x, Space.Self);

            // Cima/baixo gira só a cabeça (cameraRoot), com limite.
            pitch = Mathf.Clamp(pitch - delta.y, minPitch, maxPitch);
            if (cameraRoot != null)
            {
                cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }
        }

        private void Move()
        {
            var keyboard = Keyboard.current;
            Vector2 input = Vector2.zero;
            bool sprinting = false;

            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed) input.y += 1f;
                if (keyboard.sKey.isPressed) input.y -= 1f;
                if (keyboard.dKey.isPressed) input.x += 1f;
                if (keyboard.aKey.isPressed) input.x -= 1f;
                sprinting = keyboard.leftShiftKey.isPressed;
            }

            input = Vector2.ClampMagnitude(input, 1f);

            float speed = sprinting ? sprintSpeed : walkSpeed;
            Vector3 targetVelocity = (transform.right * input.x + transform.forward * input.y) * speed;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, acceleration * Time.deltaTime);

            // Gravidade simples para manter o jogador no chão.
            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }
            verticalVelocity += gravity * Time.deltaTime;

            Vector3 motion = horizontalVelocity + Vector3.up * verticalVelocity;
            controller.Move(motion * Time.deltaTime);
        }

        private static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
