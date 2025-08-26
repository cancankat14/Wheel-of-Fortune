using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[ExecuteAlways]
public static class UtilityHelper
{
    
    // Play Mode Validation: Ensures fields marked with [MustBeAssigned] are not null

    #region MustBeAssignedProperty
#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    public static void InitializeValidation()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            if (!ValidateMustBeAssignedFields())
            {
                Debug.LogError("Play mode stopped due to unassigned required fields.");
                EditorApplication.isPlaying = false;
            }
        }
    }
    private static bool ValidateMustBeAssignedFields()
    {
        bool isValid = true;

        // Iterate through all active GameObjects in the scene
        foreach (GameObject obj in Object.FindObjectsOfType<GameObject>())
        {
            MonoBehaviour[] components = obj.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour component in components)
            {
                if (component == null) continue;

                // Use reflection to inspect fields
                FieldInfo[] fields = component.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (FieldInfo field in fields)
                {
                    // Check for the MustBeAssignedAttribute
                    if (field.GetCustomAttribute(typeof(MustBeAssignedAttribute)) != null)
                    {
                        object value = field.GetValue(component);
                        if (value == null || value.Equals(null))
                        {
                            Debug.LogError($"[UtilityHelper] {component.GetType().Name} on '{obj.name}' has unassigned field '{field.Name}'.");
                            isValid = false; // Validation failed
                        }
                    }
                }
            }
        }

        return isValid;
    }
    
   
#endif
    #endregion
    
    public static Color Hex(string hex)
    {
        return ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.white;
    }
    public static T FindImmediateChildComponent<T>(Transform root, string childName) where T : Component
    {
        var t = root.Find(childName);        // immediate child or explicit path like "Parent/Child"
        return t ? t.GetComponent<T>() : null;
    }
    public static T FindChildComponentDeep<T>(Transform root, string pathOrName) where T : Component
    {
        if (!root || string.IsNullOrEmpty(pathOrName)) return null;

        // 1) Explicit path (fast path)
        var t = root.Find(pathOrName);
        if (t) return t.GetComponent<T>();

        // 2) Fallback: recursive search by name
        return FindDescendantComponentByName<T>(root, pathOrName);
    }
    public static T FindDescendantComponentByName<T>(Transform root, string name) where T : Component
    {
        for (int i = 0; i < root.childCount; i++)
        {
            var child = root.GetChild(i);

            if (child.name == name)
            {
                var c = child.GetComponent<T>();
                if (c) return c;
            }

            var found = FindDescendantComponentByName<T>(child, name);
            if (found) return found;
        }
        return null;
    }
    
}
// Attribute: Marks fields as required (must be assigned in the Inspector)
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class MustBeAssignedAttribute : PropertyAttribute
{
}
