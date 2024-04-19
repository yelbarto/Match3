using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PuzzleGame.Util
{
    [RequireComponent(typeof(BoxCollider))]
    public class GameObjectInputComponent : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private BoxCollider boxCollider;
        
        private bool _touched;
        public event Action OnObjectClicked;

        public void SetInteractable(bool isEnabled)
        {
            boxCollider.enabled = isEnabled;
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            _touched = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_touched) return;
            Debug.Log("touched object " + gameObject.name);
            _touched = false;
            OnObjectClicked?.Invoke();
        }
    }
}