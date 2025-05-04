#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(IAgentRL), true)]
public class IAgentRLDrawer : PropertyDrawer
{
    // Cache all concrete types implementing IAgentRL
    private static readonly Type[] _implTypes = AppDomain.CurrentDomain
        .GetAssemblies()
        .SelectMany(a => a.GetTypes())
        .Where(t => typeof(IAgentRL).IsAssignableFrom(t)
                    && !t.IsInterface
                    && !t.IsAbstract)
        .ToArray();

    private static readonly string[] _typeNames = _implTypes
        .Select(t => t.Name)
        .ToArray();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Draw foldout
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, label);

        // Get the current managed reference type full name
        string fullTypename = property.managedReferenceFullTypename; // "AssemblyName TypeName"
        string currentTypeName = fullTypename.Contains(" ") 
            ? fullTypename.Split(' ')[1] 
            : "";

        // Find index in our list
        int currentIndex = Array.FindIndex(_implTypes, t => t.Name == currentTypeName);
        if (currentIndex < 0) currentIndex = 0; // default

        // Draw popup
        int newIndex = EditorGUI.Popup(position, currentIndex, _typeNames);
        if (newIndex != currentIndex)
        {
            // Create a new instance of the selected type
            IAgentRL newObj = (IAgentRL) Activator.CreateInstance(_implTypes[newIndex]);
            property.managedReferenceValue = newObj;
        }

        // If an instance exists, draw its fields
        if (property.managedReferenceValue != null)
        {
            EditorGUI.indentLevel++;
            var nextPos = position;
            nextPos.y += EditorGUIUtility.singleLineHeight + 2;
            EditorGUI.PropertyField(
                nextPos, 
                property, 
                GUIContent.none, 
                true  // draw children
            );
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.managedReferenceValue == null)
            return base.GetPropertyHeight(property, label);

        // height for the dropdown + the managed reference children
        return EditorGUIUtility.singleLineHeight 
               + EditorGUI.GetPropertyHeight(property, label, true);
    }
}
#endif