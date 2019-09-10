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
    [Range(.1f, 10f)] public float PenumbraOutMagnitude = 1f;
    [Range(.1f, 10f)] public float PenumbraInMagnitude = 1f;
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
    public bool DrawPenumbraOutMesh = false;
    public bool DrawCoreShadowNormals = false;
    public bool DrawPenumbraOutNormals = false;
    public bool DrawPenumbraInNormals = false;
    public Color NormalRayColor = Color.magenta;
    public bool DrawPenumbraInMesh = false;
    [Range(.25f, 1f)] public float Red = .5f;
    [Range(.25f, 1f)] public float Green = .5f;
    [Range(.25f, 1f)] public float Blue = .5f;
  #endregion

  #region Hidden properties
    [HideInInspector] public Vector3[] ShadowVertices;
    [HideInInspector] public Vector3[] PenumbraOutVertices;
    [HideInInspector] public Vector3[] PenumbraInVertices;
    [HideInInspector] public int[] ShadowTriangles;
    [HideInInspector] public int[] PenumbraOutTriangles;
    [HideInInspector] public int[] PenumbraInTriangles;
    [HideInInspector] public Vector3[] ShadowNormals;
    [HideInInspector] public Vector3[] PenumbraOutNormals;
    [HideInInspector] public Vector3[] PenumbraInNormals;
    [HideInInspector] public int BoundingRayCount = 0;
    [HideInInspector] public int Hits = 0;
    [HideInInspector] public int Misses = 0;
    [HideInInspector] public int PassesExecuted = 0;
    [HideInInspector] public int UnneededPasses = 0;
  #endregion

  #region Private properties
    private GameObject ShadowVolume;
    private GameObject PenumbraOutVolume;
    private GameObject PenumbraInVolume;
  #endregion

  private void Update() => CastShadows();

  public void CastShadows() {
    var shadowPoints  = new Dictionary<int, Vector3>();
    var shadowLengths = new Dictionary<int, float>();
    
    Vector3 center = Vector3.zero;

    int passes = FindShadowBounds(ShadowCaster, ref shadowPoints, ref shadowLengths, out center);

    BuildShadowMesh(center, shadowPoints, shadowLengths);

    RenderCoreShadow();
    RenderPenumraOut();
    RenderPenumbraIn();

    var pointKeys = shadowPoints.Keys.ToArray();

    PassesExecuted   = passes;
    UnneededPasses   = MaxPasses - passes;
    BoundingRayCount = pointKeys.Length;

    if (DrawBoundingRays)
      for(int i = 0; i < pointKeys.Length; i++)
        Debug.DrawRay(shadowPoints[pointKeys[i]], transform.TransformDirection(Vector3.back) * shadowLengths[pointKeys[i]], BoundingRayColor);
  }

  private void AssignTriangle_1(ref int triangleIndex, int vertexIndex) {
    ShadowTriangles[triangleIndex]     = vertexIndex - 3;
    ShadowTriangles[triangleIndex + 1] = vertexIndex - 1;
    ShadowTriangles[triangleIndex + 2] = vertexIndex - 2;

    PenumbraOutTriangles[triangleIndex]     = vertexIndex - 3;
    PenumbraOutTriangles[triangleIndex + 1] = vertexIndex - 1;
    PenumbraOutTriangles[triangleIndex + 2] = vertexIndex - 2;

    PenumbraInTriangles[triangleIndex]     = vertexIndex - 3;
    PenumbraInTriangles[triangleIndex + 1] = vertexIndex - 1;
    PenumbraInTriangles[triangleIndex + 2] = vertexIndex - 2;

    triangleIndex += 3;
  }

  private void AssignTriangle_2(ref int triangleIndex, int vertexIndex) {
    ShadowTriangles[triangleIndex]     = vertexIndex - 2;
    ShadowTriangles[triangleIndex + 1] = vertexIndex - 1;
    ShadowTriangles[triangleIndex + 2] = vertexIndex;

    PenumbraOutTriangles[triangleIndex]     = vertexIndex - 2;
    PenumbraOutTriangles[triangleIndex + 1] = vertexIndex - 1;
    PenumbraOutTriangles[triangleIndex + 2] = vertexIndex;

    PenumbraInTriangles[triangleIndex]     = vertexIndex - 2;
    PenumbraInTriangles[triangleIndex + 1] = vertexIndex - 1;
    PenumbraInTriangles[triangleIndex + 2] = vertexIndex;

    triangleIndex += 3;
  }

  private void AssignTriangle_3(ref int triangleIndex, int vertexIndex) {
    ShadowTriangles[triangleIndex]     = vertexIndex - 2;
    ShadowTriangles[triangleIndex + 1] = vertexIndex - 1;
    ShadowTriangles[triangleIndex + 2] = 0;

    PenumbraOutTriangles[triangleIndex]     = vertexIndex - 2;
    PenumbraOutTriangles[triangleIndex + 1] = vertexIndex - 1;
    PenumbraOutTriangles[triangleIndex + 2] = 0;

    PenumbraInTriangles[triangleIndex]     = vertexIndex - 2;
    PenumbraInTriangles[triangleIndex + 1] = vertexIndex - 1;
    PenumbraInTriangles[triangleIndex + 2] = 0;

    triangleIndex += 3;
  }

  private void AssignTriangle_4(ref int triangleIndex, int vertexIndex) {
    ShadowTriangles[triangleIndex]     = vertexIndex - 1;
    ShadowTriangles[triangleIndex + 1] = 1;
    ShadowTriangles[triangleIndex + 2] = 0;

    PenumbraOutTriangles[triangleIndex]     = vertexIndex - 1;
    PenumbraOutTriangles[triangleIndex + 1] = 1;
    PenumbraOutTriangles[triangleIndex + 2] = 0;

    PenumbraInTriangles[triangleIndex]     = vertexIndex - 1;
    PenumbraInTriangles[triangleIndex + 1] = 1;
    PenumbraInTriangles[triangleIndex + 2] = 0;

    triangleIndex += 3;
  }

  private int FindShadowBounds(GameObject shadowCaster, ref Dictionary<int, Vector3> points, ref Dictionary<int, float> lengths, out Vector3 p) {
    float scale = 1f / (float) Resolution;

    LayerMask mask = Ignore ? ~Name.value : Name.value;

    Vector3 shadowDirection = transform.TransformDirection(Vector3.forward);
    Vector3 drawPoint       = shadowDirection * ShadowLength * RayAugment;

    if (DrawInitialRay)
      Debug.DrawRay(ShadowCaster.transform.position, drawPoint, InitialRayColor);

    var ray = new Ray(ShadowCaster.transform.position, drawPoint);
    
    p = ray.GetPoint(ShadowLength);

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

            points[index]  = position;
            lengths[index] = hitInfo.distance;

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

    return passes - 1;
  }

  private void BuildShadowMesh(Vector3 center, Dictionary<int, Vector3> points, Dictionary<int, float> lengths) {
    var pointKeys = points.Keys.ToList();

    pointKeys.Sort();

    int p = pointKeys.Count * 2;
    int t = pointKeys.Count * 6;
    
    ShadowVertices      = new Vector3[p];
    PenumbraOutVertices = new Vector3[p];
    PenumbraInVertices  = new Vector3[p];
    
    ShadowNormals      = new Vector3[p];
    PenumbraOutNormals = new Vector3[p];
    PenumbraInNormals  = new Vector3[p];

    ShadowTriangles      = new int[t];
    PenumbraOutTriangles = new int[t];
    PenumbraInTriangles  = new int[t];

    int vertexIndex   = 0;
    int triangleIndex = 0;

    for (int i = 0; i < pointKeys.Count; i++) {
      bool even = i % 2 == 0;
      int  key  = pointKeys[i];

      Vector3 point  = points[key];
      float   length = lengths[key];

      #region Smoothing logic
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

          Vector3 prevPrev = points[prevPrevKey];
          Vector3 previous = points[previousKey];
          Vector3 next     = points[nextKey];
          Vector3 nextNext = points[nextNextKey];
          Vector3 averaged = (prevPrev + previous + points[key] + next + nextNext) / 5;

          float prevPrevLength = lengths[prevPrevKey];
          float previousLength = lengths[previousKey];
          float nextLength     = lengths[nextKey];
          float nextNextLength = lengths[nextNextKey];
          float averagedLength = (prevPrevLength + previousLength + lengths[key] + nextLength + nextNextLength) / 5f;

          point  = averaged;
          length = averagedLength;
        }
      #endregion
      
      #region Vertex assignment
        int nextVertexIndex = vertexIndex + 1;

        Vector3 back = point + (transform.TransformDirection(Vector3.back) * length);

        ShadowVertices[vertexIndex]     = point;
        ShadowVertices[nextVertexIndex] = back;

        ShadowNormals[vertexIndex]     = point;
        ShadowNormals[nextVertexIndex] = point;

        Vector3 distanceToCenter = point - center;
        Vector3 penumbraOut      = point + distanceToCenter * PenumbraOutMagnitude;

        PenumbraOutVertices[vertexIndex]     = penumbraOut;
        PenumbraOutVertices[nextVertexIndex] = back;

        PenumbraOutNormals[vertexIndex]     = penumbraOut;
        PenumbraOutNormals[nextVertexIndex] = penumbraOut;

        Vector3 penumbraIn = point - distanceToCenter * PenumbraInMagnitude;
        
        PenumbraInVertices[vertexIndex]     = penumbraIn;
        PenumbraInVertices[nextVertexIndex] = back;

        PenumbraInNormals[vertexIndex]     = penumbraIn;
        PenumbraInNormals[nextVertexIndex] = penumbraIn;

        vertexIndex += 2;
      #endregion

      #region Assign vertexes to triangle indexes
        if (even) {  
          if (i >= 2)
            AssignTriangle_1(ref triangleIndex, vertexIndex);

          AssignTriangle_2(ref triangleIndex, vertexIndex);
        } else {
          AssignTriangle_1(ref triangleIndex, vertexIndex);

          if (i < pointKeys.Count - 1)
            AssignTriangle_2(ref triangleIndex, vertexIndex);
          else if (i == pointKeys.Count - 1) {
            AssignTriangle_3(ref triangleIndex, vertexIndex);
            AssignTriangle_4(ref triangleIndex, vertexIndex);
          }
        }
      #endregion
    }
  }

  private void RenderCoreShadow() {
    if (ShadowVolume == null) {
      GameObject found = GameObject.Find("Core Shadow Volume");
      
      ShadowVolume = found == null ? GameObject.CreatePrimitive(PrimitiveType.Cube) : found;

      ShadowVolume.name = "Core Shadow Volume";

      ShadowVolume.GetComponent<BoxCollider>().enabled = false;
      ShadowVolume.GetComponent<MeshRenderer>().sharedMaterial = MeshMaterial;
    }

    if (DrawShadowMesh) {
      var shadowMesh = new Mesh {
        vertices  = (Vector3[]) ShadowVertices.Clone(),
        triangles = (int[]) ShadowTriangles.Clone()
      };

      shadowMesh.RecalculateNormals();

      ShadowVolume.GetComponent<MeshFilter>().sharedMesh = shadowMesh;
    }
    else
      ShadowVolume.GetComponent<MeshFilter>().sharedMesh = null;
    
    if (DrawCoreShadowNormals)
      for (int i = 0; i < ShadowVertices.Length; i++)
        Debug.DrawRay(ShadowVertices[i], ShadowNormals[i], NormalRayColor);
  }

  private void RenderPenumraOut() {
    if (PenumbraOutVolume == null) {
      GameObject found = GameObject.Find("Penumbra Out Volume");

      PenumbraOutVolume = found == null ? GameObject.CreatePrimitive(PrimitiveType.Cube) : found;

      PenumbraOutVolume.name = "Penumbra Out Volume";

      PenumbraOutVolume.GetComponent<BoxCollider>().enabled = false;
      PenumbraOutVolume.GetComponent<MeshRenderer>().sharedMaterial = MeshMaterial;
    }

    if (DrawPenumbraOutMesh) {
      var penumbraOutMesh = new Mesh {
        vertices  = (Vector3[]) PenumbraOutVertices.Clone(),
        triangles = (int[]) PenumbraOutTriangles.Clone(),
        normals   = (Vector3[]) PenumbraOutNormals.Clone()
      };

      PenumbraOutVolume.GetComponent<MeshFilter>().sharedMesh = penumbraOutMesh;
    }
    else
      PenumbraOutVolume.GetComponent<MeshFilter>().sharedMesh = null;
    
    if (DrawPenumbraOutNormals)
      for (int i = 0; i < PenumbraOutNormals.Length; i++)
        Debug.DrawRay(PenumbraOutVertices[i], PenumbraOutNormals[i], NormalRayColor);
  }

  private void RenderPenumbraIn() {
    if (PenumbraInVolume == null) {
      GameObject found = GameObject.Find("Penumbra In Volume");

      PenumbraInVolume = found == null ? GameObject.CreatePrimitive(PrimitiveType.Cube) : found;

      PenumbraInVolume.name = "Penumbra In Volume";

      PenumbraInVolume.GetComponent<BoxCollider>().enabled = false;
      PenumbraInVolume.GetComponent<MeshRenderer>().sharedMaterial = MeshMaterial;
    }

    if (DrawPenumbraInMesh) {
      var penumbraInMesh = new Mesh {
        vertices  = (Vector3[]) PenumbraInVertices.Clone(),
        triangles = (int[]) PenumbraInTriangles.Clone()
      };

      penumbraInMesh.RecalculateNormals();

      PenumbraInVolume.GetComponent<MeshFilter>().sharedMesh = penumbraInMesh;
    }
    else
      PenumbraInVolume.GetComponent<MeshFilter>().sharedMesh = null;

    if (DrawPenumbraInNormals)
      for (int i = 0; i < PenumbraInNormals.Length; i++)
        Debug.DrawRay(PenumbraInVertices[i], PenumbraInNormals[i], NormalRayColor);
  }
}
