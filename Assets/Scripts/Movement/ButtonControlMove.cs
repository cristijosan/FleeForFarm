using Game;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Movement
{
    public class ButtonControlMove : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private FloatingJoystick floatingJoystick;
        private bool isPress;
        
        private void Start() => floatingJoystick = gameObject.GetComponent<FloatingJoystick>();

        public void OnPointerDown(PointerEventData eventData)
        {
            isPress = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPress = false;
            Movement.OnStop?.Invoke();
            TriggeredArea.Instance.CheckShop();
        }
        
        private void FixedUpdate()
        {
            if (isPress)
                Movement.ChangeVector?.Invoke(floatingJoystick.Vertical, floatingJoystick.Horizontal);
        }
    }
}