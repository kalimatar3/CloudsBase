#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
public class SimpleDictionaryDrawer : PropertyDrawer
{
    private float LineHeight = EditorGUIUtility.singleLineHeight;
    private float Spacing = EditorGUIUtility.standardVerticalSpacing;
    private const float RemoveButtonWidth = 20f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty keysProp = property.FindPropertyRelative("keys");
        SerializedProperty valuesProp = property.FindPropertyRelative("values");

        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, label);

        float y = position.y;
        int count = Mathf.Min(keysProp.arraySize, valuesProp.arraySize);

        for (int i = 0; i < count; i++)
        {
            SerializedProperty keyProp = keysProp.GetArrayElementAtIndex(i);
            SerializedProperty valueProp = valuesProp.GetArrayElementAtIndex(i);

            Rect keyRect = new Rect(position.x, y, (position.width - RemoveButtonWidth) * 0.5f, LineHeight);
            Rect valueRect = new Rect(position.x + keyRect.width, y, (position.width - RemoveButtonWidth) * 0.5f, LineHeight);
            Rect removeRect = new Rect(position.x + keyRect.width + valueRect.width, y, RemoveButtonWidth, LineHeight);

            bool isDuplicate = false;
            for (int j = 0; j < count; j++)
            {
                if (j == i) continue;
                if (SerializedProperty.EqualContents(keyProp, keysProp.GetArrayElementAtIndex(j)))
                {
                    isDuplicate = true;
                    break;
                }
            }

            // Draw key with red if duplicated
            Color originalColor = GUI.backgroundColor;
            if (isDuplicate) GUI.backgroundColor = Color.red;

            EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none);
            GUI.backgroundColor = originalColor;

            EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none);

            if (GUI.Button(removeRect, "X"))
            {
                keysProp.DeleteArrayElementAtIndex(i);
                valuesProp.DeleteArrayElementAtIndex(i);
                break;
            }

            y += LineHeight + Spacing;
        }

        // Add button
        Rect addRect = new Rect(position.x, y, position.width, LineHeight);
        if (GUI.Button(addRect, "Add Entry"))
        {
            keysProp.arraySize++;
            valuesProp.arraySize++;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty keysProp = property.FindPropertyRelative("keys");
        return (keysProp.arraySize + 1) * (LineHeight + Spacing);
    }
}
#endif
