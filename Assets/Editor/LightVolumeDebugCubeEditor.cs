using UnityEngine;

using UnityEditor;

[CustomEditor(typeof(LightVolumeDebugCube))]
public class LightVolumeDebugCubeEditor : Editor {
  public override void OnInspectorGUI() {
    DrawDefaultInspector();

    LightVolumeDebugCube debugCube = (LightVolumeDebugCube) target;
    
    if(GUILayout.Button("Create Debug Voxels") && debugCube.ShadowVoxels.Count == 0)
      debugCube.CreateDebugVoxels();

    if(GUILayout.Button("Destroy Debug Voxels"))
      debugCube.DestroyDebugVoxels();

    if(GUILayout.Button("Refresh Debug Voxels")) {
      debugCube.DestroyDebugVoxels();
      debugCube.CreateDebugVoxels();
    }
  }
}