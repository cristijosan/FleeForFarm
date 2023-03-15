    using UnityEngine;
using System;
    using Game;
    using Unity.VisualScripting;
    using UnityEngine.Serialization;

    namespace Movement
{
    public class Movement : MonoBehaviour
    {
        public static Action<float, float> ChangeVector;
        public static Action OnStop;
        public static Action RefreshSpeed;
        private CharacterController characterController;
        private float turnSmoothVelocity;
        private readonly int speedId = Animator.StringToHash("Speed");
        private readonly int inGameId = Animator.StringToHash("InGame");
        
        
        [Header("Movement: ")]
        public float speed;
        public float turnSmoothTime;
        [Header("UI: ")]
        [SerializeField] private Transform indicator;
        [SerializeField] private Animator animator;

        private int plusSpeed;
        
        private void Awake()
        {
            ChangeVector += Move;
            OnStop += OnStopMove;
            characterController = GetComponent<CharacterController>();
            RefreshSpeed += () => { plusSpeed = PlayerPrefs.GetInt(CharacterKeys.Speed, 1) / 4; };
        }

        private void OnDestroy()
        {
            ChangeVector -= Move;
            OnStop -= OnStopMove;
            RefreshSpeed = null;
        }

        private void Start()
        {
            plusSpeed = PlayerPrefs.GetInt(CharacterKeys.Speed, 1) / 4;
            indicator.eulerAngles = new Vector3(-90, 0, 0);
        }

        private void Move(float vertical, float horizontal)
        {
            var direction = new Vector3(-vertical, 0, horizontal).normalized;
            if (!(direction.magnitude >= .1f))
                return;
            var targetAngle = Mathf.Atan2(direction.z, -direction.x) * Mathf.Rad2Deg;
            var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0, angle, 0);
            characterController.Move(direction * (speed + plusSpeed) * Time.deltaTime);
            animator.SetFloat(speedId, characterController.velocity.magnitude);
            indicator.eulerAngles = new Vector3(-90, 0, 0);
        }

        public void ChangeIdleState(bool state)
        {
            animator.SetBool(inGameId, state);
        }

        private void OnStopMove()
        {
            animator.SetFloat(speedId, 0);
        }
    }
}
