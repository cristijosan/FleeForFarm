using System;
using DG.Tweening;
using Game;
using UnityEngine;

namespace Player
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance;
    
        [SerializeField] private Material radialFill;
        [SerializeField] private GameObject vacuum;
        private readonly int arc1 = Shader.PropertyToID("_Arc1");
        private int fill;
        private Movement.Movement movement;
        
        [Header("Component")]
        public Transform collectPoint;
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            movement = GetComponent<Movement.Movement>();
            
            fill = (int) radialFill.GetFloat(arc1);
        }

        public void ResetWeight()
        {
            radialFill.SetFloat(arc1, 360);
        }
        
        public void IncreaseWeight(float mass, int price)
        {
            var endValue = 0;
            if (fill - mass > 0)
                endValue = (int)(fill - mass);
            else
            {
                radialFill.SetFloat(arc1,0);
                GameManager.Instance.ShowNotification("Your inventory is full !");
                return;
            }
            
            DOTween.To(() => fill, x => fill = x, endValue, 1f)
                .OnUpdate(() =>
                {
                    radialFill.SetFloat(arc1,fill);
                });
            
            PlayerPrefs.SetInt(GameKeys.TempCoins, PlayerPrefs.GetInt(GameKeys.TempCoins, 0) + price);
        }

        private bool isChanged;
        private void LateUpdate()
        {
            if (transform.position.x > 127)
            {
                if (isChanged) return;
                
                CameraFollow.Instance.ChangeGameOffset();
                movement.ChangeIdleState(true);
                vacuum.SetActive(true);
                isChanged = true;
            }
            else
            {
                if (!isChanged) return;
                
                CameraFollow.Instance.ChangeMenuOffset();
                movement.ChangeIdleState(false);
                vacuum.SetActive(false);
                isChanged = false;
            }
        }
    }
}
