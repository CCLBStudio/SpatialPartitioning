using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpatialGridDrawer))]
public class SpatialGridDrawerEditor : Editor
{
    private SerializedProperty _grid;
    private SerializedProperty _drawGizmosOn;
    private SerializedProperty _gridColor;
    private SerializedProperty _showExistingCells;
    private SerializedProperty _existingCellsColor;
    private SerializedProperty _gizmoGridSize;
    private SerializedProperty _arrowAngle;
    private SerializedProperty _arrowSize;
    
    private void OnEnable()
    {
        _grid = serializedObject.FindProperty(SpatialGridDrawer.GridProperty);
        _drawGizmosOn = serializedObject.FindProperty(SpatialGridDrawer.DrawGizmosOnProperty);
        _gridColor = serializedObject.FindProperty(SpatialGridDrawer.GridColorProperty);
        _showExistingCells = serializedObject.FindProperty(SpatialGridDrawer.ShowExistingCellsProperty);
        _existingCellsColor = serializedObject.FindProperty(SpatialGridDrawer.ExistingCellsColorProperty);
        _gizmoGridSize = serializedObject.FindProperty(SpatialGridDrawer.GizmoGridSizeProperty);
        _arrowAngle = serializedObject.FindProperty(SpatialGridDrawer.ArrowAngleProperty);
        _arrowSize = serializedObject.FindProperty(SpatialGridDrawer.ArrowSizeProperty);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawScriptField();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_grid);
        EditorGUILayout.PropertyField(_drawGizmosOn);

        SpatialGridDrawer.GizmosEvent e = (SpatialGridDrawer.GizmosEvent)_drawGizmosOn.enumValueIndex;
        if (e == SpatialGridDrawer.GizmosEvent.None)
        {
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
            return;
        }

        EditorGUILayout.PropertyField(_gizmoGridSize);
        EditorGUILayout.PropertyField(_arrowAngle);
        EditorGUILayout.PropertyField(_arrowSize);
        
        EditorGUILayout.PropertyField(_gridColor);
        EditorGUILayout.PropertyField(_showExistingCells);
        if (_showExistingCells.boolValue)
        {
            EditorGUILayout.PropertyField(_existingCellsColor);
        }
        
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
    
    private void DrawScriptField()
    {
        GUI.enabled = false;
        if (serializedObject.targetObject is MonoBehaviour behaviour)
        {
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(behaviour), typeof(MonoBehaviour), false);
        }
        else
        {
            EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject((ScriptableObject)serializedObject.targetObject), typeof(ScriptableObject), false);
        }
        GUI.enabled = true;
        GUILayout.Space(5);
    }
}