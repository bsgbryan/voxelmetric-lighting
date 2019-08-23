using UnityEngine;

using UnityEditor;

[CustomEditor(typeof(LightVolumeDebugCube))]
public class LightVolumeDebugCubeEditor : Editor {
  public override void OnInspectorGUI() {
    DrawDefaultInspector();

    LightVolumeDebugCube debugCube = (LightVolumeDebugCube) target;

    if(GUILayout.Button("Cast Shadows"))
      debugCube.CastShadows();

    if(GUILayout.Button("Refresh Debug Voxels"))
      debugCube.RefreshDebugVoxels();
  }
}