using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GauthMaker : MonoBehaviour {
    //目的の変換関数
    public Func<float, float, Vector3> f = ToSphere;

    private static Func<float, float> Sin = Mathf.Sin;
    private static Func<float, float> Cos = Mathf.Cos;

    
    public static Vector3 ToMebius(float u, float v) {
        float a = 2f;
        return new Vector3(
            (a + u * Sin(v/2))* Cos(v),
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
    public float MoveTime = 2f;
    public float UResolution = 0.1f,VResolution = 0.1f;//分解能
    public bool MakeRoot = true;
    public bool DrawDebugLine = false;

    public GameObject DefaultQuad;
    private MoveMesh[,] Meshes;


    void Awake() {
        CreateDefaultMesh();
    }

    void CreateDefaultMesh() {
        DefaultQuad.transform.localScale = new Vector3(UResolution, 1, VResolution);
        int UNum = (int)((UMax - UMin) / UResolution);
        int VNum = (int)((VMax - VMin) / VResolution);
        Meshes = new MoveMesh[UNum + 1,VNum + 1];

        for (int k = 0; k < (MakeRoot ? 2 : 1); k++) {
            for (int i = 0; i < UNum; i++) {
                for (int j = 0; j < VNum; j++) {
                    Meshes[i, j] = ((GameObject)(Instantiate(DefaultQuad, new Vector3(UMin + UResolution * (i + 0.5f), 0, VMin + VResolution * (j + 0.5f)), new Quaternion())
                                    )).GetComponent<MoveMesh>();
                    Meshes[i, j].GM = this;
                    Meshes[i, j].MyColor =  ColorHSV.FromHsv((int)(360f * (float)i / (float)UNum), (float)j / (float)VNum, 0.9f);
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

    void Start() {
        foreach (var m in Meshes) {
            if (m != null) {
                m.Init(f, UResolution, VResolution);
            }
        }
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            foreach (var m in Meshes) {
                if (m != null) {
                    m.StartMoving(f, m.GetNormalVector(f,UResolution,VResolution));
                }
            }
        }else if (Input.GetKeyDown(KeyCode.Backspace)) {
            foreach (var m in Meshes) {
                if (m != null) {
                    m.SwapMoving();
                }
            }
        } else if (Input.GetKeyDown(KeyCode.L)) {
            DrawDebugLine = !DrawDebugLine;
        
        }
    }



}
