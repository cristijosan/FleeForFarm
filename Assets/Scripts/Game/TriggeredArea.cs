using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Game
{
    [RequireComponent(typeof(Collider))]
    public class TriggeredArea : MonoBehaviour
    {
        public static TriggeredArea Instance;
        public UnityEvent onEnter;
        private bool isEntered;

        private void Awake()
        {
            Instance = this;
        }

        public void CheckShop()
        {
            if (isEntered)
                onEnter?.Invoke();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            isEntered = true;
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            isEntered = false;
        }
    }
}
