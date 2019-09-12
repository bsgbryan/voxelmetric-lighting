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
    public bool DrawPenumbraOutMesh = false;
    public bool DrawShadowMesh = true;
    public bool DrawPenumbraInMesh = false;

    #if UNITY_EDITOR
      public bool DrawInitialRay = true;
      public Color InitialRayColor = Color.white;
      public bool DrawHitRays = true;
      [Range(.5f, 1f)] public float HitAlpha = .75f;
      public bool DrawMissRays = false;
      [Range(0f, .5f)] public float MissAlpha = .25f;
      public bool DrawBoundingRays = true;
      public Color BoundingRayColor = Color.black;
      public bool DrawCoreShadowNormals = false;
      public bool DrawPenumbraOutNormals = false;
      public bool DrawPenumbraInNormals = false;
      public Color NormalRayColor = Color.magenta;
      [Range(.25f, 1f)] public float Red = .5f;
      [Range(.25f, 1f)] public float Green = .5f;
      [Range(.25f, 1f)] public float Blue = .5f;
    #endif
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
    [HideInInspector] public Dictionary<int, Vector3> ShadowMeshVertices;

    #if UNITY_EDITOR
      [HideInInspector] public int BoundingRayCount = 0;
      [HideInInspector] public int Hits = 0;
      [HideInInspector] public int Misses = 0;
      [HideInInspector] public int PassesExecuted = 0;
      [HideInInspector] public int UnneededPasses = 0;
    #endif
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

    int passes = DetermineShadowBounds(ShadowCaster, ref shadowPoints, ref shadowLengths, out center);

    LinkBoundPointsToMeshVertexes(ShadowCaster.GetComponent<MeshFilter>().sharedMesh.vertices, shadowPoints, shadowLengths);

    BuildShadowMesh(center);

    RenderCoreShadow();
    RenderPenumraOut();
    RenderPenumbraIn();

    #if UNITY_EDITOR
      var pointKeys = shadowPoints.Keys.ToArray();

      PassesExecuted   = passes;
      UnneededPasses   = MaxPasses - passes;
      BoundingRayCount = pointKeys.Length;

      if (DrawBoundingRays)
        for(int i = 0; i < pointKeys.Length; i++)
          Debug.DrawRay(
            shadowPoints[pointKeys[i]],
            transform.TransformDirection(Vector3.back) * shadowLengths[pointKeys[i]],
            BoundingRayColor
          );
    #endif
  }

  private void AssignTriangle_1(ref int triangleIndex, int vertexIndex) {
    ShadowTriangles[triangleIndex]     = vertexIndex;
    ShadowTriangles[triangleIndex + 1] = vertexIndex + 1;
    ShadowTriangles[triangleIndex + 2] = vertexIndex + 2;

    PenumbraOutTriangles[triangleIndex]     = vertexIndex;
    PenumbraOutTriangles[triangleIndex + 1] = vertexIndex + 1;
    PenumbraOutTriangles[triangleIndex + 2] = vertexIndex + 2;

    PenumbraInTriangles[triangleIndex]     = vertexIndex;
    PenumbraInTriangles[triangleIndex + 1] = vertexIndex + 1;
    PenumbraInTriangles[triangleIndex + 2] = vertexIndex + 2;

    triangleIndex += 3;
  }

  private void AssignTriangle_2(ref int triangleIndex, int vertexIndex) {
    ShadowTriangles[triangleIndex]     = vertexIndex + 1;
    ShadowTriangles[triangleIndex + 1] = vertexIndex + 3;
    ShadowTriangles[triangleIndex + 2] = vertexIndex + 2;

    PenumbraOutTriangles[triangleIndex]     = vertexIndex + 1;
    PenumbraOutTriangles[triangleIndex + 1] = vertexIndex + 3;
    PenumbraOutTriangles[triangleIndex + 2] = vertexIndex + 2;

    PenumbraInTriangles[triangleIndex]     = vertexIndex + 1;
    PenumbraInTriangles[triangleIndex + 1] = vertexIndex + 3;
    PenumbraInTriangles[triangleIndex + 2] = vertexIndex + 2;

    triangleIndex += 3;
  }

  private void AssignTriangle_3(ref int triangleIndex, int vertexIndex) {
    ShadowTriangles[triangleIndex]     = vertexIndex;
    ShadowTriangles[triangleIndex + 1] = vertexIndex + 1;
    ShadowTriangles[triangleIndex + 2] = 0;

    PenumbraOutTriangles[triangleIndex]     = vertexIndex;
    PenumbraOutTriangles[triangleIndex + 1] = vertexIndex + 1;
    PenumbraOutTriangles[triangleIndex + 2] = 0;

    PenumbraInTriangles[triangleIndex]     = vertexIndex;
    PenumbraInTriangles[triangleIndex + 1] = vertexIndex + 1;
    PenumbraInTriangles[triangleIndex + 2] = 0;

    triangleIndex += 3;
  }

  private void AssignTriangle_4(ref int triangleIndex, int vertexIndex) {
    ShadowTriangles[triangleIndex]     = vertexIndex + 1;
    ShadowTriangles[triangleIndex + 1] = 1;
    ShadowTriangles[triangleIndex + 2] = 0;

    PenumbraOutTriangles[triangleIndex]     = vertexIndex + 1;
    PenumbraOutTriangles[triangleIndex + 1] = 1;
    PenumbraOutTriangles[triangleIndex + 2] = 0;

    PenumbraInTriangles[triangleIndex]     = vertexIndex + 1;
    PenumbraInTriangles[triangleIndex + 1] = 1;
    PenumbraInTriangles[triangleIndex + 2] = 0;

    triangleIndex += 3;
  }

  private int DetermineShadowBounds(
    GameObject shadowCaster,
    ref Dictionary<int, Vector3> points,
    ref Dictionary<int, float> lengths,
    out Vector3 p
  ) {
    float scale = 1f / (float) Resolution;

    LayerMask mask = Ignore ? ~Name.value : Name.value;

    Vector3 shadowDirection = transform.TransformDirection(Vector3.forward);
    Vector3 drawPoint       = shadowDirection * ShadowLength * RayAugment;

    #if UNITY_EDITOR
      if (DrawInitialRay)
        Debug.DrawRay(ShadowCaster.transform.position, drawPoint, InitialRayColor);
    #endif

    var ray = new Ray(ShadowCaster.transform.position, drawPoint);
    
    p = ray.GetPoint(ShadowLength);

    bool hasShadow = true;
    int  passes    = 0;

    #if UNITY_EDITOR
      Hits           = 0;
      Misses         = 0;
      PassesExecuted = 0;
    #endif
    
    while (hasShadow && passes++ < MaxPasses) {
      hasShadow = false;
      
      for (int side = 0; side < 4; side++) {
        bool processingXAxis =  side % 2 == 0;
        bool processingYAxis = !processingXAxis;
        
        bool processingPositive = false;

        #if UNITY_EDITOR
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
        #endif
        
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

            #if UNITY_EDITOR
              if (DrawHitRays)
                Debug.DrawRay(position, direction * ShadowLength * RayAugment, new Color(r, g, b, HitAlpha));

              Hits++;
            #endif
          } else {
            #if UNITY_EDITOR
              if (DrawMissRays)
                Debug.DrawRay(position, direction * ShadowLength * RayAugment, new Color(r, g, b, MissAlpha));

              Misses++;
            #endif
          }
        }
      }
    }

    return passes - 1;
  }

  private void BuildShadowMesh(Vector3 center) {
    var keys = ShadowMeshVertices.Keys.ToList();
    
    int p = keys.Count;
    int t = p * 3;
    
    ShadowVertices      = new Vector3[p];
    PenumbraOutVertices = new Vector3[p];
    PenumbraInVertices  = new Vector3[p];
    
    ShadowNormals      = new Vector3[p];
    PenumbraOutNormals = new Vector3[p];
    PenumbraInNormals  = new Vector3[p];

    ShadowTriangles      = new int[t];
    PenumbraOutTriangles = new int[t];
    PenumbraInTriangles  = new int[t];

    int triangleIndex = 0;

    keys.Sort();

    for (int i = 0; i < keys.Count; i += 2) {
      Vector3 point = ShadowMeshVertices[keys[i]];
      
      #region Vertex assignment
        int nextI = i + 1;

        Vector3 back = ShadowMeshVertices[keys[i + 1]];

        ShadowVertices[i]     = point;
        ShadowVertices[nextI] = back;

        ShadowNormals[i]     = point;
        ShadowNormals[nextI] = point;

        Vector3 distanceToCenter = point - center;
        Vector3 penumbraOut      = point + distanceToCenter * PenumbraOutMagnitude;

        PenumbraOutVertices[i]     = penumbraOut;
        PenumbraOutVertices[nextI] = back;

        PenumbraOutNormals[i]     = penumbraOut;
        PenumbraOutNormals[nextI] = penumbraOut;

        Vector3 penumbraIn = point - distanceToCenter * PenumbraInMagnitude;
        
        PenumbraInVertices[i]     = penumbraIn;
        PenumbraInVertices[nextI] = back;

        PenumbraInNormals[i]     = penumbraIn;
        PenumbraInNormals[nextI] = penumbraIn;
      #endregion

      
      if (i < keys.Count - 2) {
        AssignTriangle_1(ref triangleIndex, i);
        AssignTriangle_2(ref triangleIndex, i);
      }
      else {
        AssignTriangle_3(ref triangleIndex, i);
        AssignTriangle_4(ref triangleIndex, i);
      }
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
        vertices  = ShadowVertices.ToArray(),
        triangles = (int[]) ShadowTriangles.Clone()
      };

      ShadowVolume.GetComponent<MeshFilter>().sharedMesh = shadowMesh;
    }
    else if (ShadowVolume.GetComponent<MeshFilter>().sharedMesh != null)
      ShadowVolume.GetComponent<MeshFilter>().sharedMesh = null;
    
    #if UNITY_EDITOR
      if (DrawCoreShadowNormals)
        for (int i = 0; i < ShadowVertices.Length; i++)
          Debug.DrawRay(ShadowVertices[i], ShadowNormals[i], NormalRayColor);
    #endif
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
    else if (PenumbraOutVolume.GetComponent<MeshFilter>().sharedMesh != null)
      PenumbraOutVolume.GetComponent<MeshFilter>().sharedMesh = null;
    
    #if UNITY_EDITOR
      if (DrawPenumbraOutNormals)
        for (int i = 0; i < PenumbraOutNormals.Length; i++)
          Debug.DrawRay(PenumbraOutVertices[i], PenumbraOutNormals[i], NormalRayColor);
    #endif
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

      PenumbraInVolume.GetComponent<MeshFilter>().sharedMesh = penumbraInMesh;
    }
    else if (PenumbraInVolume.GetComponent<MeshFilter>().sharedMesh != null)
      PenumbraInVolume.GetComponent<MeshFilter>().sharedMesh = null;

    #if UNITY_EDITOR
      if (DrawPenumbraInNormals)
        for (int i = 0; i < PenumbraInNormals.Length; i++)
          Debug.DrawRay(PenumbraInVertices[i], PenumbraInNormals[i], NormalRayColor);
    #endif
  }

  private void LinkBoundPointsToMeshVertexes(
    Vector3[] vertexes,
    Dictionary<int, Vector3> points,
    Dictionary<int, float> lengths
  ) {
    var pointKeys = points.Keys.ToList();

    pointKeys.Sort();
    
    ShadowMeshVertices = new Dictionary<int, Vector3>();
    
    for (int s = 0, index = 0; s < pointKeys.Count; s++, index += 2) {
      float shortestDistance = float.PositiveInfinity;

      int     p     = pointKeys[s];
      Vector3 point = points[p];
      Vector3 back  = point + (transform.TransformDirection(Vector3.back) * lengths[p]);

      Vector3 front = Vector3.zero;
      Vector3 rear  = Vector3.zero;

      for (int v = 0; v < vertexes.Length; v++) {
        Matrix4x4 matrix = Matrix4x4.TRS(
          ShadowCaster.transform.position,
          ShadowCaster.transform.rotation,
          ShadowCaster.transform.localScale
        );
        
        Vector3 vertex   = matrix.MultiplyPoint(vertexes[v]);
        float   distance = Vector3.Distance(vertex, back);

        if (distance < shortestDistance) {
          front = point;
          rear  = vertex;
          
          shortestDistance = distance;
        }
      }
      
      ShadowMeshVertices[index]     = front;
      ShadowMeshVertices[index + 1] = rear;
    }
  }
}
