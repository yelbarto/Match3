using UnityEditor;
using UnityEngine;

namespace PuzzleGame.Editor
{
    public static class RectTransformEditorUtility
    {
        private static Rect GetWorldRect(RectTransform rectTransform)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            // Get the bottom left corner.
            Vector3 position = corners[0];

            var lossyScale = rectTransform.lossyScale;
            var rect = rectTransform.rect;
            Vector2 size = new Vector2(
                lossyScale.x * rect.size.x,
                lossyScale.y * rect.size.y);
 
            return new Rect(position, size);
        }

        [MenuItem("CONTEXT/RectTransform/Convert to anchored")]
        public static void ConvertToAnchored(MenuCommand command)
        {
            RectTransform rt = (RectTransform) command.context;
            if (rt == null) return;
            Undo.RecordObject(rt,"Convert to Anchored");
            var rect = GetWorldRect(rt);
            var parentRect = GetWorldRect(rt.parent.GetComponent<RectTransform>());
            
            rt.anchorMin = new Vector2(
                parentRect.width!=0?(rect.xMin-parentRect.xMin)/parentRect.width:0,
                parentRect.width!=0?(rect.yMin-parentRect.yMin)/parentRect.height:0
            );
            rt.anchorMax = new Vector2(
                parentRect.width!=0?(rect.xMax-parentRect.xMin)/parentRect.width:0,
                parentRect.width!=0?(rect.yMax-parentRect.yMin)/parentRect.height:0
            );
            
            rt.pivot = Vector2.one / 2f;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.offsetMin = Vector2.zero;
            PrefabUtility.RecordPrefabInstancePropertyModifications(rt);
        }
    }
}