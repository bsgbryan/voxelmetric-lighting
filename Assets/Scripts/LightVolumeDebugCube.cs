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

  // void Update() => CastShadows();

  public void CastShadows() {
    DestroyDebugVoxels();

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

    LayerMask mask = Invert ? ~Mask.value : Mask.value;

    for (float y = startY; y < endY; y += scale)
      for (float x = startX; x < endX; x += scale)
        for (float z = startZ; z < endZ; z += scale) {
          Vector3 localPosition = new Vector3(x + offset, y + offset, z + offset);
          Vector3 worldPosition = Container.transform.position + localPosition;
          Vector3 direction     = transform.rotation * Vector3.back;

          bool hitSomething = Physics.Raycast(worldPosition, direction, 10f, mask);

          if (hitSomething) {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            cube.name  = $"{x}:{y}:{z}";
            cube.layer = LayerMask.NameToLayer("Ignore Raycast");

            cube.transform.parent     = Container.transform;
            cube.transform.position   = localPosition;
            cube.transform.localScale = new Vector3(scale, scale, scale);

            ShadowVoxels.Add(cube);
          }
        }
  }

  public void DestroyDebugVoxels() {
    GameObject.DestroyImmediate(Container);

    ShadowVoxels.Clear();
  }
}
