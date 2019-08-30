using System.Collections.Generic;

using UnityEngine;

[ExecuteInEditMode]
public class LightVolumeDebugCube : MonoBehaviour {

  [Header("Details")]
  public GameObject ShadowCaster;
  [Range(.01f, 10f)] public float Resolution = 2f;
  [Range(10f, 100f)] public float ShadowLength = 10f;
  [Range(1f, 1.5f)] public float RayAugment = 1.1f;
  [Range(2, 100)] public int MaxPasses = 10;

  [Header("Layer Mask")]
  public LayerMask Name;
  public bool Ignore = true;
  
  [Header("Debug")]
  public bool DrawInitialRay = true;
  public Color InitialRayColor = Color.white;
  public bool DrawHitRays = true;
  [Range(.5f, 1f)] public float HitAlpha = .75f;
  public bool DrawMissRays = false;
  [Range(0f, .5f)] public float MissAlpha = .25f;
  public bool DrawBoundingRays = true;
  public Color BoundingRayColor = Color.black;
  [Range(.25f, 1f)] public float Red = .5f;
  [Range(.25f, 1f)] public float Green = .5f;
  [Range(.25f, 1f)] public float Blue = .5f;

  private void Update() => CastShadows();

  public void CastShadows() {
    var ShadowPoints  = new Vector3[MaxPasses * 2 * 4];
    var ShadowLengths = new float[MaxPasses * 2 * 4];

    float scale = 1f / (float) Resolution;

    LayerMask mask = Ignore ? ~Name.value : Name.value;

    Vector3 shadowDirection = transform.TransformDirection(Vector3.forward);
    Vector3 drawPoint       = shadowDirection * ShadowLength * RayAugment;

    if (DrawInitialRay)
      Debug.DrawRay(ShadowCaster.transform.position, drawPoint, InitialRayColor);

    var ray = new Ray(ShadowCaster.transform.position, drawPoint);
    var p   = ray.GetPoint(ShadowLength);

    bool hasShadow = true;
    int  passes    = 0;

    var rot = transform.rotation;

    while (hasShadow && passes++ < MaxPasses) {
      hasShadow = false;
      
      for (int side = 0; side < 4; side++) {
        bool processingXAxis =  side % 2 == 0;
        bool processingYAxis = !processingXAxis;
        
        bool processingPositive = false;

        float r = 0;
        float g = 0;
        float b = 0;

        if (processingXAxis) {
          r = Red;

          processingPositive = side == 0;
        }
        
        if (processingYAxis) {
          b = Blue;

          processingPositive = side == 1;
        }

        if (processingPositive)
          g = Green;
        
        for (int step = -passes; step < passes; step++) {
          int x = processingPositive ? passes : -passes;
          int y = processingPositive ? step   : -step;

          if (processingYAxis) {
            y = processingPositive ?  passes : -passes;
            x = processingPositive ? -step   :  step;
          }

          Vector3 position  = p + transform.rotation * new Vector3(x * scale, y * scale, 0f);
          Vector3 direction = transform.TransformDirection(Vector3.back);

          RaycastHit hitInfo;

          bool hitSomething = Physics.Raycast(position, direction, out hitInfo, ShadowLength * RayAugment, mask);

          if (hitSomething) {
            hasShadow = true;

            int index = (MaxPasses * 2) * side + MaxPasses + step;

            ShadowPoints[index] = position;
            ShadowLengths[index]  = hitInfo.distance;

            if (DrawHitRays)
              Debug.DrawRay(position, direction * ShadowLength * RayAugment, new Color(r, g, b, HitAlpha));
          } else if (DrawMissRays)
            Debug.DrawRay(position, direction * ShadowLength * RayAugment, new Color(r, g, b, MissAlpha));
        }
      }
    }

    // if (ShadowVolumeCapture) {
    //   if (passes <= MaxPasses)
    //     Debug.Log($"Full shadow volume captured in {passes} passes");
    //   else
    //     Debug.LogWarning($"{MaxPasses} passes did not capture full shadow volume");
    // }

    if (DrawBoundingRays)
      for(int i = 0; i < ShadowPoints.Length; i++)
        if (ShadowPoints[i] != Vector3.zero)
          Debug.DrawRay(ShadowPoints[i], transform.TransformDirection(Vector3.back) * ShadowLengths[i], BoundingRayColor);
  }
}
