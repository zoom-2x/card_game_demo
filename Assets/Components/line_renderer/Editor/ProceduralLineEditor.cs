using UnityEngine;
using UnityEditor;

using gc_components;

[CanEditMultipleObjects]
[CustomEditor(typeof(ProceduralLine))]
public class ProceduralLineEditor : Editor
{
    Transform gameobject;
    ProceduralLine line;

    SerializedProperty config_show_handles;
    SerializedProperty config_closed;
    SerializedProperty config_reversed;
    SerializedProperty config_tile_size;
    SerializedProperty config_thickness;
    SerializedProperty config_circle_radius;
    SerializedProperty config_join_segments;
    SerializedProperty config_tilt_multiplier;
    SerializedProperty config_tilt_value;
    SerializedProperty config_tilt_reversed;

    void OnEnable()
    {
        line = (ProceduralLine) target;
        gameobject = line.transform;

        config_show_handles = serializedObject.FindProperty("config_show_handles");
        config_closed = serializedObject.FindProperty("config_closed");
        config_reversed = serializedObject.FindProperty("config_reversed");
        config_tile_size = serializedObject.FindProperty("config_tile_size");
        config_thickness = serializedObject.FindProperty("config_thickness");
        config_circle_radius = serializedObject.FindProperty("config_circle_radius");
        config_join_segments = serializedObject.FindProperty("config_join_segments");
        config_tilt_multiplier = serializedObject.FindProperty("config_tilt_multiplier");
        config_tilt_value = serializedObject.FindProperty("config_tilt_value");
        config_tilt_reversed = serializedObject.FindProperty("config_tilt_reversed");
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Add node")) {
            line.add_node();
        }

        serializedObject.Update();

        EditorGUILayout.PropertyField(config_show_handles, new GUIContent("Show handles:"));
        EditorGUILayout.PropertyField(config_closed, new GUIContent("Closed path:"));
        EditorGUILayout.PropertyField(config_reversed, new GUIContent("Reversed path:"));
        EditorGUILayout.PropertyField(config_tile_size, new GUIContent("Tile size:"));
        EditorGUILayout.PropertyField(config_thickness, new GUIContent("Thickness:"));
        EditorGUILayout.PropertyField(config_circle_radius, new GUIContent("Join radius:"));
        EditorGUILayout.PropertyField(config_join_segments, new GUIContent("Join segments:"));
        EditorGUILayout.PropertyField(config_tilt_multiplier, new GUIContent("Tilt multiplier:"));
        EditorGUILayout.PropertyField(config_tilt_value, new GUIContent("Maximum tilt:"));
        EditorGUILayout.PropertyField(config_tilt_reversed, new GUIContent("Reverse tilt:"));

        serializedObject.ApplyModifiedProperties();
    }
}
