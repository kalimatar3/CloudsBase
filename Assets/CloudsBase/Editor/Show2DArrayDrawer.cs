using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Show2DArrayAttribute))]
public class Show2DArrayDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var attr = (Show2DArrayAttribute)attribute;
        return EditorGUIUtility.singleLineHeight * (attr.Height + 1);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var attr = (Show2DArrayAttribute)attribute;
        EditorGUI.BeginProperty(position, label, property);

        var values = property.FindPropertyRelative("values");
        if (values == null)
        {
            EditorGUI.LabelField(position, "Invalid Serializable2DArray (no 'values' field found)");
            EditorGUI.EndProperty();
            return;
        }

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float cellWidth = position.width / Mathf.Max(1, attr.Width);

        for (int y = 0; y < Mathf.Min(attr.Height, values.arraySize); y++)
        {
            var row = values.GetArrayElementAtIndex(y);
            var rowValues = row.FindPropertyRelative("values");
            if (rowValues == null) continue;

            for (int x = 0; x < Mathf.Min(attr.Width, rowValues.arraySize); x++)
            {
                Rect cellRect = new Rect(
                    position.x + x * cellWidth,
                    position.y + y * lineHeight,
                    cellWidth - 2,
                    lineHeight
                );
                EditorGUI.PropertyField(cellRect, rowValues.GetArrayElementAtIndex(x), GUIContent.none);
            }
        }

        EditorGUI.EndProperty();
    }
}
