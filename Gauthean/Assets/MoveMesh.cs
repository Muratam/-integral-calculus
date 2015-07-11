using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
public class MoveMesh : MonoBehaviour {

    public GauthMaker GM;
    public float MoveTime {
        get{return GM.MoveTime;}
    }
    public bool DrawDebugLine {
        get { return GM.DrawDebugLine; }
    }
    private bool startSwitch = false;
    private bool MoveFinish = false;
    private float StartTime;

    private Vector3 fromPosition;
    //最初は(0,1,0)の向きのQuadと仮定
    private Vector3 fromUp;
    private Vector3 fromScale;

    private Vector3 toPosition;
    private Vector3 toUp;
    private Vector3 ToUp {//法線ベクトルで指示
        get { return toUp; }
        set {
            toUp = value;
            toUp.Normalize();

        }
    }
    private Vector3 toScale;

    public Color MyColor;

    //法線ベクトルを取得
    public Vector3 GetNormalVector(Func<float, float, Vector3> f,float uResolution,float vResolution) {
        float u = fromPosition.x;
        float v = fromPosition.z;
        Vector3 dU = f(u + uResolution, v) - f(u, v);
        Vector3 dV = f(u, v + vResolution) - f(u, v);
        return Vector3.Cross(dU, dV);
    }

    public Vector2 GetRelativeScale(Func<float, float, Vector3> f, float uResolution, float vResolution) {
        float u = fromPosition.x;
        float v = fromPosition.z;
        Vector3 dU = f(u + uResolution, v) - f(u, v);
        Vector3 dV = f(u, v + vResolution) - f(u, v);
        return new Vector2(dU.magnitude / uResolution ,dV.magnitude/vResolution  );
    }



    static float AxisAngleOnAxisPlane( Vector3 origin, Vector3 fromDirection, Vector3 toDirection, Vector3 axis ) {
	    fromDirection.Normalize();
	    axis.Normalize();
	    Vector3 toDirectionProjected = toDirection - axis * Vector3.Dot(axis,toDirection);
	    toDirectionProjected.Normalize();
	    return Mathf.Acos(Mathf.Clamp(Vector3.Dot(fromDirection,toDirectionProjected),-1f,1f)) *
		    (Vector3.Dot(Vector3.Cross(axis,fromDirection), toDirectionProjected) < 0f ? -Mathf.Rad2Deg : Mathf.Rad2Deg);
    }



    
    public void Init(Func<float, float, Vector3> f,float uResolution,float vResolution) {
        fromPosition = this.transform.position;
        fromScale = this.transform.localScale;
        fromUp = this.transform.up;
        toPosition = f(fromPosition.x, fromPosition.z);
        toUp = GetNormalVector(f,uResolution,vResolution);
        Vector2 RelativeScale = GetRelativeScale(f,uResolution,vResolution);
        toScale = new Vector3(fromScale.x * RelativeScale.x, 1,fromScale.z * RelativeScale.y);
    }

    public void StartMoving(Func<float,float,Vector3> f,Vector3 _ToUp) {
        startSwitch = true;
        StartTime = Time.time;
        MoveFinish = false;
    }

    public void SwapMoving() {
        if (MoveFinish) {
            Swap(ref toPosition, ref fromPosition);
            Swap(ref toUp, ref fromUp);
            Swap(ref toScale,ref fromScale);
            startSwitch = true;
            MoveFinish = false;
            StartTime = Time.time;
        }
    }
    static void Swap<T>(ref T V1,ref T V2) {
        var Temp = V1;V1 = V2; V2 = Temp;
    }


	void Update () {
        if (startSwitch && !MoveFinish) {
            float NormalizedPassedTime = (Time.time - StartTime)/MoveTime;
            transform.position = fromPosition + NormalizedPassedTime * (toPosition - fromPosition);
            transform.up = fromUp + NormalizedPassedTime * (ToUp - fromUp);
            transform.localScale = fromScale + NormalizedPassedTime * (toScale - fromScale);
            if (NormalizedPassedTime > 1) {
                transform.position = toPosition;
                transform.up = ToUp;
                transform.localScale = toScale;
                MoveFinish = true;
            }
        }

        if(DrawDebugLine)Debug.DrawLine(fromPosition, toPosition,MyColor);
	}
}
