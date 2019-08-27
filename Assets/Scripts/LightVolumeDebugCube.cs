using System.Collections.Generic;

using UnityEngine;

[ExecuteInEditMode]
public class LightVolumeDebugCube : MonoBehaviour {

  public float RayLifespan = 3f;
  public float ShadowLength = 10f;

  public LayerMask Mask;
  
  public bool Invert = true;

  [Range(1, 10)]
  public int Resolution = 1;

  public GameObject ShadowCaster;

  private void Update() => CastShadows();

  public void CastShadows() {
    float scale = 1f / (float) Resolution;

    LayerMask mask = Invert ? ~Mask.value : Mask.value;

    Vector3 shadowDirection = transform.TransformDirection(Vector3.forward);
    Vector3 drawPoint       = shadowDirection * ShadowLength;

    Debug.DrawRay(ShadowCaster.transform.position, drawPoint, Color.white, RayLifespan);

    var ray = new Ray(ShadowCaster.transform.position, drawPoint);
    var p   = ray.GetPoint(ShadowLength);

    bool hasShadow = true;
    int  passes    = 0;

    var rot = transform.rotation;

    while (hasShadow) {
      hasShadow = false;

      passes++;
      
      for (int side = 0; side < 4; side++) {
        bool processingXAxis =  side % 2 == 0;
        bool processingYAxis = !processingXAxis;
        
        bool processingPositive = false;
        bool processingNegative = false;

        if (processingXAxis) {
          processingPositive =  side == 0;
          processingNegative = !processingPositive;
        }
        
        if (processingYAxis) {
          processingPositive =  side == 1;
          processingNegative = !processingPositive;
        }

        int totalStepsThisPass = passes;

        int start = -totalStepsThisPass;
        
        for (int step = start; step < totalStepsThisPass; step++) {
          int x = 0;
          int y = 0;

          if (processingXAxis) {
            x = processingNegative ? -passes : passes;
            y = step;
          } else {
            y = processingNegative ? -passes : passes;
            x = step;
          }

          Vector3 position  = p + transform.rotation * new Vector3(x * scale, y * scale, 0f);
          Vector3 direction = transform.TransformDirection(Vector3.back);

          bool hitSomething = Physics.Raycast(position, direction, ShadowLength * 1.25f, mask);

          Debug.DrawRay(position, direction * ShadowLength * 1.25f, hitSomething ? Color.red : Color.blue, RayLifespan);

          if (hitSomething && !hasShadow)
            hasShadow = true;
        }
      }
    }
  }
}
