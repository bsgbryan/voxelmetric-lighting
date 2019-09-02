using System.Linq;
using System.Collections.Generic;

using UnityEngine;

[ExecuteInEditMode]
public class LightVolumeDebugCube : MonoBehaviour {

  #region Editor properties
    [Header("Details")]
    public GameObject ShadowCaster;
    [Range(.01f, 10f)] public float Resolution = 2f;
    [Range(.1f, 10f)] public float Density = 1f;
    public bool Smooth = false;
    [Range(10f, 100f)] public float ShadowLength = 10f;
    [Range(1f, 1.5f)] public float RayAugment = 1.1f;
    [Range(2, 100)] public int MaxPasses = 10;
    public Material MeshMaterial;

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
    public bool DrawShadowMesh = true;
    [Range(.25f, 1f)] public float Red = .5f;
    [Range(.25f, 1f)] public float Green = .5f;
    [Range(.25f, 1f)] public float Blue = .5f;
  #endregion

  #region Hidden properties
    [HideInInspector] public Vector3[] ShadowVertices;
    [HideInInspector] public int[] ShadowTriangles;
    [HideInInspector] public int BoundingRayCount = 0;
    [HideInInspector] public int Hits = 0;
    [HideInInspector] public int Misses = 0;
    [HideInInspector] public int PassesExecuted = 0;
    [HideInInspector] public int UnneededPasses = 0;
  #endregion

  #region Private properties
    private GameObject ShadowVolume;
  #endregion

  private void Update() => CastShadows();

  public void CastShadows() {
    var shadowPoints  = new Dictionary<int, Vector3>();
    var shadowLengths = new Dictionary<int, float>();

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

    Hits           = 0;
    Misses         = 0;
    PassesExecuted = 0;

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
          float x = (float) (processingPositive ? passes : -passes);
          float y = (float) (processingPositive ? step   : -step);

          if (processingYAxis) {
            y = (float) (processingPositive ?  passes : -passes);
            x = (float) (processingPositive ? -step   :  step);
          }

          x *= 1f / Density;
          y *= 1f / Density;

          Vector3 position  = p + transform.rotation * new Vector3(x * scale, y * scale , 0f);
          Vector3 direction = transform.TransformDirection(Vector3.back);

          RaycastHit hitInfo;

          bool hitSomething = Physics.Raycast(position, direction, out hitInfo, ShadowLength * RayAugment, mask);

          if (hitSomething) {
            hasShadow = true;

            int index = (MaxPasses * 2) * side + MaxPasses + step;

            shadowPoints[index]  = position;
            shadowLengths[index] = hitInfo.distance;

            if (DrawHitRays)
              Debug.DrawRay(position, direction * ShadowLength * RayAugment, new Color(r, g, b, HitAlpha));

            Hits++;
          } else {
            if (DrawMissRays)
              Debug.DrawRay(position, direction * ShadowLength * RayAugment, new Color(r, g, b, MissAlpha));

            Misses++;
          }
        }
      }
    }

    var pointKeys = shadowPoints.Keys.ToList();

    pointKeys.Sort();

    BoundingRayCount = pointKeys.Count;
    PassesExecuted   = passes;

    ShadowVertices  = new Vector3[pointKeys.Count * 2];
    ShadowTriangles = new int[pointKeys.Count * 6];

    int vertexIndex   = 0;
    int triangleIndex = 0;

    UnneededPasses = MaxPasses - passes;

    for (int i = 0; i < pointKeys.Count; i++) {
      bool even = i % 2 == 0;
      int  key  = pointKeys[i];

      if (Smooth == true) {
        int prevPrevKey = 0;
        int previousKey = 0;
        int nextKey     = 0;
        int nextNextKey = 0;

        if (i == 0) {
          prevPrevKey = pointKeys[pointKeys.Count - 2];
          previousKey = pointKeys.Last();
          nextKey     = pointKeys[1];
          nextNextKey = pointKeys[2];
        } else if (i == 1) {
          prevPrevKey = pointKeys.Last();
          previousKey = pointKeys.First();
          nextKey     = pointKeys[2];
          nextNextKey = pointKeys[3];
        } else if (i == pointKeys.Count - 1) {
          prevPrevKey = pointKeys[i - 2];
          previousKey = pointKeys[i - 1];
          nextKey     = pointKeys.First();
          nextNextKey = pointKeys[1];
        } else if (i == pointKeys.Count - 2) {
          prevPrevKey = pointKeys[pointKeys.Count - 3];
          previousKey = pointKeys[pointKeys.Count - 2];
          nextKey     = pointKeys.Last();
          nextNextKey = pointKeys.First();
        } else {
          prevPrevKey = pointKeys[i - 2];
          previousKey = pointKeys[i - 1];
          nextKey     = pointKeys[i + 1];
          nextNextKey = pointKeys[i + 2];
        }

        Vector3 prevPrev = shadowPoints[prevPrevKey];
        Vector3 previous = shadowPoints[previousKey];
        Vector3 next     = shadowPoints[nextKey];
        Vector3 nextNext = shadowPoints[nextNextKey];
        Vector3 averaged = (prevPrev + previous + shadowPoints[key] + next + nextNext) / 5;

        ShadowVertices[vertexIndex++] = averaged;
        ShadowVertices[vertexIndex++] = averaged + (transform.TransformDirection(Vector3.back) * shadowLengths[key]);
      } else {
        ShadowVertices[vertexIndex++] = shadowPoints[key];
        ShadowVertices[vertexIndex++] = shadowPoints[key] + (transform.TransformDirection(Vector3.back) * shadowLengths[key]);
      }

      if (even) {
        if (i >= 2) {
          ShadowTriangles[triangleIndex++] = vertexIndex - 3;
          ShadowTriangles[triangleIndex++] = vertexIndex - 1;
          ShadowTriangles[triangleIndex++] = vertexIndex - 2;
        }

        ShadowTriangles[triangleIndex++] = vertexIndex - 2;
        ShadowTriangles[triangleIndex++] = vertexIndex - 1;
        ShadowTriangles[triangleIndex++] = vertexIndex;
      } else {
        ShadowTriangles[triangleIndex++] = vertexIndex - 3;
        ShadowTriangles[triangleIndex++] = vertexIndex - 1;
        ShadowTriangles[triangleIndex++] = vertexIndex - 2;

        if (i < pointKeys.Count - 1) {
          ShadowTriangles[triangleIndex++] = vertexIndex - 2;
          ShadowTriangles[triangleIndex++] = vertexIndex - 1;
          ShadowTriangles[triangleIndex++] = vertexIndex;
        } else if (i == pointKeys.Count - 1) {
          ShadowTriangles[triangleIndex++] = vertexIndex - 2;
          ShadowTriangles[triangleIndex++] = vertexIndex - 1;
          ShadowTriangles[triangleIndex++] = 0;
          ShadowTriangles[triangleIndex++] = vertexIndex - 1;
          ShadowTriangles[triangleIndex++] = 1;
          ShadowTriangles[triangleIndex++] = 0;
        }
      }
    }

    if (ShadowVolume == null) {
      ShadowVolume = GameObject.CreatePrimitive(PrimitiveType.Cube);

      ShadowVolume.name = "Shadow Volume";

      ShadowVolume.GetComponent<BoxCollider>().enabled = false;
    }

    if (DrawShadowMesh) {
      var shadowMesh = new Mesh {
        vertices  = (Vector3[]) ShadowVertices.Clone(),
        triangles = (int[]) ShadowTriangles.Clone()
      };

      shadowMesh.RecalculateNormals();

      ShadowVolume.GetComponent<MeshRenderer>().sharedMaterial = MeshMaterial;
      ShadowVolume.GetComponent<MeshFilter>().sharedMesh       = shadowMesh;
    } else
      ShadowVolume.GetComponent<MeshFilter>().sharedMesh = null;

    if (DrawBoundingRays)
      for(int i = 0; i < pointKeys.Count; i++)
        Debug.DrawRay(shadowPoints[pointKeys[i]], transform.TransformDirection(Vector3.back) * shadowLengths[pointKeys[i]], BoundingRayColor);
  }
}
