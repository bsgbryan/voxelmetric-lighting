using UnityEngine;

using UnityEditor;

[CustomEditor(typeof(LightVolumeDebugCube))]
[CanEditMultipleObjects]
public class LightVolumeDebugCubeEditor : Editor {
  public override void OnInspectorGUI() {
    DrawDefaultInspector();

    LightVolumeDebugCube debugCube = (LightVolumeDebugCube) target;

    EditorGUILayout.LabelField($"Bounding ray count: {debugCube.BoundingRayCount}");
    EditorGUILayout.LabelField($"Vertexes: {debugCube.ShadowVertices.Length}");
    EditorGUILayout.LabelField($"Triangle Indexes: {debugCube.ShadowTriangles.Length}");
    EditorGUILayout.LabelField($"Hits: {debugCube.Hits}");
    EditorGUILayout.LabelField($"Misses: {debugCube.Misses}");
    EditorGUILayout.LabelField($"Passes: {debugCube.PassesExecuted}");
    EditorGUILayout.LabelField($"Unneeded Passes: {debugCube.UnneededPasses}");
  }
}