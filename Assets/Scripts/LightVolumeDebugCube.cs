using UnityEngine;

[ExecuteInEditMode]
public class LightVolumeDebugCube : MonoBehaviour {

  void Start() {
    GameObject container = GameObject.CreatePrimitive(PrimitiveType.Cube);

    container.name = "Voxel Light Debugger";

    container.transform.position   = new Vector3( 0, 0.5f, 0 );
    container.transform.localScale = new Vector3(10, 1,    10);

    for (int x = -5; x < 5; x++)
      for (int z = -5; z < 5; z++) {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        
        cube.name               = $"{x}:{z}";
        cube.transform.position = new Vector3(((float) x) + 0.5f, 0.5f, ((float) z) + 0.5f);
        
        cube.
          GetComponent<MeshRenderer>().
          material = new Material(Shader.Find("Voxelmetric Lighting/Debug"));
        
        cube.gameObject.SetActive(false);

        cube.transform.parent = container.transform;
      }
  }

  void Update() {
      
  }
}
