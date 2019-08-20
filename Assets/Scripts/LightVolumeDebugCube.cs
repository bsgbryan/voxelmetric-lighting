using System.Collections.Generic;

using UnityEngine;

using UnityEditor;

[ExecuteInEditMode]
public class LightVolumeDebugCube : MonoBehaviour {

  public Vector3 Dimensions = new Vector3(10, 1, 10);

  public List<GameObject> ShadowVoxels = new List<GameObject>();

  private GameObject Container;
  
  void Start() {

  }

  void Update() {
      
  }

  public  void CreateDebugVoxels() {
    Container = GameObject.CreatePrimitive(PrimitiveType.Cube);

    Container.name = "Voxel Light Debugger";

    Container.transform.position   = new Vector3(0, 0.5f, 0);
    Container.transform.localScale = new Vector3(Dimensions.x, Dimensions.y, Dimensions.z);

    int endX   = (int) (Dimensions.x * 0.5f);
    int endZ   = (int) (Dimensions.z * 0.5f);
    
    int startX = -endX;
    int startZ = -endZ;

    for (int x = startX; x < endX; x++)
      for (int z = startZ; z < endZ; z++) {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        
        cube.name               = $"{x}:{z}";
        cube.transform.position = new Vector3(((float) x) + 0.5f, 0.5f, ((float) z) + 0.5f);
        
        cube.
          GetComponent<MeshRenderer>().
          material = new Material(Shader.Find("Voxelmetric Lighting/Debug"));
        
        cube.gameObject.SetActive(false);

        cube.transform.parent = Container.transform;

        ShadowVoxels.Add(cube);
      }
  }

  public void DestroyDebugVoxels() {
    GameObject.DestroyImmediate(Container);

    ShadowVoxels.Clear();
  }
}
