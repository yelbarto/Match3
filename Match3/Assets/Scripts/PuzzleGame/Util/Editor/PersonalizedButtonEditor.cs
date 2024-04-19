using PuzzleGame.Util.UI;
using UnityEditor;
using UnityEditor.UI;

namespace PuzzleGame.Util.Editor
{
    [CustomEditor(typeof(PersonalizedButton))]
    public class PersonalizedButtonEditor : ButtonEditor
    {
        private SerializedProperty _scaleTransform;
        private SerializedProperty _scaleValueMultiplier;
        private SerializedProperty _scaleAccelerationMultiplier;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            _scaleTransform = serializedObject.FindProperty("scaleTransform");
            _scaleValueMultiplier = serializedObject.FindProperty("scaleValueMultiplier");
            _scaleAccelerationMultiplier = serializedObject.FindProperty("scaleAccelerationMultiplier");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_scaleTransform);
            EditorGUILayout.PropertyField(_scaleValueMultiplier);
            EditorGUILayout.PropertyField(_scaleAccelerationMultiplier);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                EditorUtility.SetDirty(target);
            }

            base.OnInspectorGUI();
        }
    }
}