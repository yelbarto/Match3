using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.Util.UI
{
    [AddComponentMenu("UI/PersonalizedButton", -90)]
    public class PersonalizedButton : Button
    {
        [SerializeField] private Transform scaleTransform;
        [SerializeField] private float scaleValueMultiplier = 0.95f;
        [SerializeField] private float scaleAccelerationMultiplier = 200f;
        private SelectionState _previousSelectionState;
        private bool _scaledToTargetAlready;
        private bool _scaledToOriginalAlready;
        private readonly Vector3 _originalLocalScale = Vector3.one;

        private void Update()
        {
#if UNITY_EDITOR
            
            if (!Application.isPlaying) return;
            
#endif
            
            switch (currentSelectionState)
            {
                case SelectionState.Pressed:
                    ScaleOnButtonClick();
                    break;

                default:
                    ResetScale();
                    break;
            }
        }
        
        private void ResetScale()
        {
            if(_scaledToOriginalAlready) return;

            _scaledToTargetAlready = false;
            var localScale = scaleTransform.localScale;
            var targetScale = _originalLocalScale;
            var targetMagnitude = targetScale.magnitude;
            
            if(localScale.magnitude > targetMagnitude * 0.999f && localScale.magnitude < targetMagnitude * 1.001f)
            {
                scaleTransform.localScale = targetScale;
                _scaledToOriginalAlready = true;
                return;
            }
            
            localScale = Vector3.Lerp(localScale, targetScale, scaleAccelerationMultiplier * Time.deltaTime);
            scaleTransform.localScale = localScale;
        }
        
        private void ScaleOnButtonClick()
        {
            if(_scaledToTargetAlready) return;

            _scaledToOriginalAlready = false;
            var localScale = scaleTransform.localScale;
            var targetScale = _originalLocalScale * scaleValueMultiplier;
            var targetMagnitude = targetScale.magnitude;
            
            if(localScale.magnitude > targetMagnitude * 0.999f && localScale.magnitude < targetMagnitude * 1.001f)
            {
                scaleTransform.localScale = targetScale;
                _scaledToTargetAlready = true;
                return;
            }
            
            localScale = Vector3.Lerp(localScale, targetScale, scaleAccelerationMultiplier * Time.deltaTime);
            scaleTransform.localScale = localScale;
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            if (!Application.isPlaying) return;
            if (_scaledToOriginalAlready) return;
            if(scaleTransform.localScale.magnitude > 0f)
            {
                scaleTransform.localScale = _originalLocalScale;
            }
            _scaledToOriginalAlready = true;
        }
    }
}