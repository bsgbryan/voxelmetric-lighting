using System.Collections.Generic;

using UnityEngine;

[ExecuteInEditMode]
public class LightVolumeDebugCube : MonoBehaviour {

  public LayerMask Mask;
  
  public bool Invert = true;

  [Range(1, 10)]
  public int Resolution = 1;

  public Vector3 Dimensions = new Vector3(10, 10, 10);

  public List<GameObject> ShadowVoxels = new List<GameObject>();

  private GameObject Container;

  void Update() => CastShadows();

  public void CastShadows() {
    LayerMask mask = Invert ? ~Mask.value : Mask.value;

    Vector3 direction = transform.rotation * Vector3.back;
    
    foreach (var shadowVoxel in ShadowVoxels) {
      Vector3 worldPosition = Container.transform.position + shadowVoxel.transform.position;
      
      bool hitSomething = Physics.Raycast(worldPosition, direction, 10f, mask);
      
      shadowVoxel.
        gameObject.
        GetComponentInChildren<MeshRenderer>().
        enabled = hitSomething;
    }
  }

  public  void CreateDebugVoxels() {
    Container = GameObject.CreatePrimitive(PrimitiveType.Cube);

    Container.name  = "Voxel Light Debugger";
    Container.layer = LayerMask.NameToLayer("Ignore Raycast");

    Container.transform.position = Vector3.zero;

    Container.
      GetComponentInChildren<MeshRenderer>().
      enabled = false;

    float scale = 1f / (float) Resolution;

    float endX = Dimensions.x * .5f;
    float endY = Dimensions.y * .5f;
    float endZ = Dimensions.z * .5f;
    
    float startX = -endX;
    float startY = -endY;
    float startZ = -endZ;

    float offset = scale * 0.5f;

    for (float y = startY; y < endY; y += scale) {
      for (float x = startX; x < endX; x += scale) {
        for (float z = startZ; z < endZ; z += scale) {
          GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

          cube.transform.parent = Container.transform;
          
          cube.name               = $"{x}:{y}:{z}";
          cube.transform.position = new Vector3(x, y, z);
          cube.transform.localScale = new Vector3(scale, scale, scale);
          
          cube.
            GetComponentInChildren<MeshRenderer>().
            enabled = false;
          
          cube.layer = LayerMask.NameToLayer("Ignore Raycast");

          ShadowVoxels.Add(cube);
        }
      }
    }
  }

  public void DestroyDebugVoxels() {
    GameObject.DestroyImmediate(Container);

    ShadowVoxels.Clear();
  }
}
