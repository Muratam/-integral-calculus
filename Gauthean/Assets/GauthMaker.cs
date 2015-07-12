using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GauthMaker : MonoBehaviour {
    //目的の変換関数
    public static Func<float, float, Vector3> UserDef
       = (u, v) => new Vector3(u * 2, 1, v * 2);

    public enum FunctionType {
        Sphere,Column,Double,Mebius,UserDef
    }
    public FunctionType functionType = FunctionType.Sphere;
    private Func<float, float, Vector3> f {
        get {
            switch (functionType) {
                default:
                case FunctionType.UserDef: return UserDef;
                case FunctionType.Sphere: return ToSphere;
                case FunctionType.Column: return ToColumn;
                case FunctionType.Double: return ToDouble;
                case FunctionType.Mebius: return ToMebius;
            }
        }
    }

    private static Func<float, float> Sin = Mathf.Sin;
    private static Func<float, float> Cos = Mathf.Cos;

    
    public static Vector3 ToMebius(float u, float v) {
        float a = 2f;
        return new Vector3(
            (a + u * Cos(v/2))* Cos(v),
            u * Sin(v / 2),
            (a + u * Cos(v/2))* Sin(v)
            );
    }

    public static Func<float, float, Vector3> ToDouble
       = (u, v) => new Vector3(u * 2 , 1, v * 2);


    public static Func<float, float, Vector3> ToColumn
        = (u, v) => new Vector3(Sin(u) + 3.14f, Cos(u), v);

    public static Func<float, float, Vector3> ToSphere
        = (u, v) => new Vector3(Sin(u) * Cos(v), Cos(u), Sin(u) * Sin(v));





    public float UMin = -1, UMax = 1, VMin = -1, VMax = 1;
    int UNum { get { return (int)((UMax - UMin) / UResolution); } }
    int VNum { get { return (int)((VMax - VMin) / VResolution); } }
    
    public float MoveTime = 2f;
    public float UResolution = 0.1f,VResolution = 0.1f;//分解能
    public bool MakeRoot = true;
    public bool DrawDebugLine = false;
    private string MeshName = "Mesh";


    void Awake() {
        //CreateDefaultQuads();
        CreateMesh();
    }



    bool StartSwitch = false;
    bool FinishedSwitch = false;
    float StartTime;

    void SWAP<T> (ref T A,ref T B){T C = A; A = B; B = C;}

    void Update() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            StartSwitch = true;
            StartTime = Time.time;
        }else if (Input.GetKeyDown(KeyCode.Backspace)) {
            foreach (var m in MeshQuads) {
                if (m != null) {
                    SWAP(ref m.ToPosition,ref m.FromPosition);
                }
            }
            StartSwitch = true;
            StartTime = Time.time;
            FinishedSwitch = false;
        }

        if (DrawDebugLine) {
            foreach (var m in MeshQuads) {
                if (m != null) {
                    Debug.DrawLine(m.ToPosition, m.FromPosition, ColorHSV.FromHsv(360f * (float)(m.I) / (float)UNum, 0.3f - 0.3f * (float)(m.J) / (float)VNum, 0.9f));
                }
            }
        }
        

        if(StartSwitch && !FinishedSwitch) {

            float NormalizedTime = (Time.time - StartTime)/MoveTime;
            if (NormalizedTime < 1) {
                var Vertices = mf.mesh.vertices; 
                foreach (var m in MeshQuads) {
                    if (m != null) {
                        Vertices[m.Index] = m.FromPosition + NormalizedTime * (m.ToPosition - m.FromPosition);
                    }
                }
                mf.mesh.vertices = Vertices;
                mf.mesh.RecalculateBounds(); 
            } else {
                FinishedSwitch = true;
                var Vertices = mf.mesh.vertices; 
                foreach (var m in MeshQuads) if (m != null) mf.mesh.vertices[m.Index] = m.ToPosition;
                mf.mesh.vertices = Vertices;
                mf.mesh.RecalculateBounds(); 
            }
        
        }
    }

    public Material Mat;
    
    private class MeshQuad {

        public void SetToPosition(Func<float, float, Vector3> f) {
            ToPosition = f(FromPosition.x, FromPosition.z);
        }

        public void SetdUdV(Func<float, float, Vector3> f) {
        
            float u = FromPosition.x;
            float v = FromPosition.z;
            dU = f(u + UResolution, v) - f(u, v);
            dV = f(u, v + VResolution) - f(u, v);
        }
        public void SetToUp(Func<float, float, Vector3> f) {
            ToUp = Vector3.Cross(dU, dV);
        }
        

        public void SetToScale(Func<float, float, Vector3> f) {
            var UV = new Vector2(dU.magnitude / UResolution, dV.magnitude / VResolution);
            ToScale = new Vector3(FromScale.x * UV.x, 1,FromScale.z * UV.y);
        }
        
        public Vector3 FromPosition, ToPosition, FromUp, ToUp, FromScale, ToScale;
        public Vector3 dU,dV;
        public readonly float UResolution, VResolution;
        public readonly int Index;
        public int I, J;

        public MeshQuad(Vector3 _FromPosition, float _UResolution, float _VResolution, Func<float, float, Vector3> f,int _Index,int _I,int _J) {
            I = _I; J = _J;
            UResolution = _UResolution; VResolution = _VResolution;
            Index = _Index;
            FromPosition = _FromPosition;
            FromUp = new Vector3(0, 1, 0);
            FromScale = new Vector3(UResolution, 1, VResolution);
            SetdUdV(f);
            SetToPosition(f);
            SetToScale(f);
            SetToUp(f);
        }

        public Vector3 getQuadVertex(int index,bool isFrom){
            Vector3 U, V,Base;
            U = isFrom ? new Vector3(UResolution,0,0) : dU;
            V = isFrom ? new Vector3(0, 0, VResolution) : dV;
            Base = isFrom ? FromPosition : ToPosition;
            switch (index) {
                default:
                case 0:return Base;
                case 1: return Base + U;
                case 2: return Base + U + V;
                case 3: return Base + V;
            }
        }

    }
    MeshQuad[,] MeshQuads;
    MeshFilter mf;


    void CreateMesh() {

        MeshQuads = new MeshQuad[UNum + 1, VNum + 1];
        List<Vector3> CourseVertices = new List<Vector3>();
        List<int> CourseTriangles = new List<int>();
        List<Vector2> CourseUV = new List<Vector2>();

        for (int k = 0; k < (MakeRoot ? 2 : 1); k++) {
            Debug.Log("UNum : " + UNum + "    : Vnum : " + VNum);
            for (int i = 0; i < UNum; i++) {
                for (int j = 0; j < VNum; j++) {
                    int Index = CourseVertices.Count;
                    MeshQuads[i, j] = new MeshQuad(
                        new Vector3(UMin + UResolution * i, 0, VMin + VResolution * j), UResolution, VResolution, f, Index,i,j);
                    CourseVertices.Add(MeshQuads[i, j].FromPosition);
                    CourseUV.Add(new Vector2((float)i / (float)UNum, (float)j / (float)VNum));
                    if (i > 0 && j > 0) MakeTetra(Index - VNum - 1, Index - VNum, Index, Index - 1, ref CourseTriangles);

                }
            }
            GameObject Course = new GameObject();
            Course.AddComponent<MeshFilter>();
            Course.AddComponent<MeshRenderer>();

            mf = Course.GetComponent<MeshFilter>();
            MeshRenderer mr = Course.GetComponent<MeshRenderer>();
            var CourseMesh = new Mesh();


            CourseMesh.vertices = CourseVertices.ToArray();
            CourseMesh.triangles = CourseTriangles.ToArray();
            CourseMesh.RecalculateNormals();
            CourseMesh.RecalculateBounds();

            mf.mesh = CourseMesh;
            mf.mesh.uv = CourseUV.ToArray();
            mr.material = Mat;

            Course.name = MeshName;
        }



    }

    void MakeTetra(int t1, int t2, int t3, int t4, ref List<int> Tr,bool isBack = false) {
        if (!isBack) {
            Tr.Add(t1); Tr.Add(t2); Tr.Add(t3);
            Tr.Add(t1); Tr.Add(t3); Tr.Add(t4);
        } else {
            Tr.Add(t1); Tr.Add(t3); Tr.Add(t2);
            Tr.Add(t1); Tr.Add(t4); Tr.Add(t3);
        }
    }

}

/*

    public GameObject DefaultQuad;
    private MoveMesh[,] Meshes;
    void CreateDefaultQuads() {
        DefaultQuad.transform.localScale = new Vector3(UResolution, 1, VResolution);
        int UNum = (int)((UMax - UMin) / UResolution);
        int VNum = (int)((VMax - VMin) / VResolution);
        Meshes = new MoveMesh[UNum + 1, VNum + 1];

        for (int k = 0; k < (MakeRoot ? 2 : 1); k++) {
            for (int i = 0; i < UNum; i++) {
                for (int j = 0; j < VNum; j++) {
                    Meshes[i, j] = ((GameObject)(Instantiate(DefaultQuad, new Vector3(UMin + UResolution * (i + 0.5f), 0, VMin + VResolution * (j + 0.5f)), new Quaternion())
                                    )).GetComponent<MoveMesh>();
                    Meshes[i, j].GM = this;
                    Meshes[i, j].MyColor = ColorHSV.FromHsv((int)(360f * (float)i / (float)UNum), (float)j / (float)VNum, 0.9f);
                    var Mats = Meshes[i, j].gameObject.GetComponentsInChildren<MeshRenderer>();
                    foreach (var m in Mats) {
                        switch (m.gameObject.name) {
                            default://case "Quad1":
                                m.material.color = Meshes[i, j].MyColor;
                                break;
                        }
                    }
                }
            }
        }
    }
    void STARTOLD() {
        foreach (var m in Meshes) {
            if (m != null) {
                m.Init(f, UResolution, VResolution);
            }
        }
    }
    void UpdateOld() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            foreach (var m in Meshes) {
                if (m != null) {
                    m.StartMoving(f, m.GetNormalVector(f, UResolution, VResolution));
                }
            }
        } else if (Input.GetKeyDown(KeyCode.Backspace)) {
            foreach (var m in Meshes) {
                if (m != null) {
                    m.SwapMoving();
                }
            }
        } else if (Input.GetKeyDown(KeyCode.L)) {
            DrawDebugLine = !DrawDebugLine;

        }
    }


*/
