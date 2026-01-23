using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


[ExecuteInEditMode]
public class BezierCurve : MonoBehaviour
{
    [System.Serializable]
    public class ControlPoint
    {
        public Vector3 Position;
        [HideInInspector]public Vector3 ScaledPosition;
        public Vector3 Tangent;
        [HideInInspector] public Vector3 ScaledTangent;
        public float Distance;
    }

    [SerializeField]
    public List<ControlPoint> AllPoints = new List<ControlPoint>();

    public bool IsEmpty => AllPoints.Count == 0;
    public ControlPoint FirstPoint => !IsEmpty ? AllPoints[0] : null;
    public ControlPoint LastPoint => !IsEmpty ? AllPoints[AllPoints.Count - 1] : null;

    public float TotalDistance => !IsEmpty ? LastPoint.Distance : 0.0f;

    public Vector3 DebugPosition;

    public static BezierCurve Instance;

    public float Scale = 1.0f;

    private void Awake()
    {
        if (BezierCurve.Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple BezierCurve instances detected. Only one instance is allowed.");
            Destroy(this);
        }

        foreach(var point in AllPoints)
        {
            point.ScaledPosition = point.Position * Scale;
            point.ScaledTangent = point.Tangent * Scale;
        }

    }


    public IEnumerable<ControlPoint> Points
    {
        get
        {
            foreach (var point in AllPoints)
            {
                yield return point;
            }
        }
    }

    private void OnValidate()
    {
        foreach (var point in AllPoints)
        {
            point.ScaledPosition = point.Position * Scale;
            point.ScaledTangent = point.Tangent * Scale;
        }
        UpdateDistances();
    }

    public Pose GetClosestPoseFromLocation(Vector3 inLocation, ref float outBlendValue)
    {
        if (IsEmpty)
        {
            throw new System.Exception("Cannot get position on empty Bezier curve.");
        }

        Vector3 closestPoint = Vector3.zero;
        Pose closestPose = new Pose();

        for (int i = 1; i < AllPoints.Count; ++i)
        {
            ControlPoint A = AllPoints[i - 1];
            ControlPoint B = AllPoints[i];

            Vector3 vLast = A.ScaledPosition;
            for (float f = 0.0f; f <= 1.0f; f += 0.025f)
            {
                Vector3 vCurr = GetPosition(A, B, f);
                if(Vector3.Distance(inLocation, vCurr) < Vector3.Distance(inLocation, closestPoint))
                {
                    closestPoint = vCurr;
                    outBlendValue = Mathf.Lerp(A.Distance, B.Distance, f);
                    closestPose.position = vCurr;
                    closestPose.rotation = Quaternion.LookRotation(GetForward(A, B, f));
                }

                vLast = vCurr;
            }
        }

        return closestPose;
    }

    public Pose GetPose(float inDistanceAlongCurve)
    {
        if (IsEmpty)
        {
            throw new System.Exception("Cannot get position on empty Bezier curve.");
        }

        if (inDistanceAlongCurve < FirstPoint.Distance)
        {
            return new Pose
            {
                position = FirstPoint.ScaledPosition,
                rotation = Quaternion.LookRotation(FirstPoint.ScaledTangent)
            };
        }

        if (inDistanceAlongCurve > LastPoint.Distance)
        {
            return new Pose
            {
                position = LastPoint.ScaledPosition,
                rotation = Quaternion.LookRotation(LastPoint.ScaledTangent)
            };
        }

        for (int i = 1; i < AllPoints.Count; i++)
        {
            ControlPoint A = AllPoints[i - 1];
            ControlPoint B = AllPoints[i];

            if (inDistanceAlongCurve < B.Distance)
            {
                float blend = Mathf.InverseLerp(A.Distance, B.Distance, inDistanceAlongCurve);

                return new Pose
                {
                    position = GetPosition(A, B, blend),
                    rotation = Quaternion.LookRotation(GetForward(A, B, blend))
                };
            }
        }

        throw new System.Exception("Should not reach here when getting position on Bezier curve.");
    }

    public static Vector3 GetPosition(ControlPoint inA, ControlPoint inB, float inF)
    {
        Vector3 p0 = inA.ScaledPosition;                 // <-- Start at A
        Vector3 p1 = inA.ScaledPosition + inA.ScaledTangent;
        Vector3 p2 = inB.ScaledPosition - inB.ScaledTangent;
        Vector3 p3 = inB.ScaledPosition;                 // <-- End at B

        float fOneMinusT = 1.0f - inF;

        return p0 * fOneMinusT * fOneMinusT * fOneMinusT +
                p1 * 3 * fOneMinusT * fOneMinusT * inF +
                p2 * 3 * fOneMinusT * inF * inF +
                p3 * inF * inF * inF;
    }

    public static Vector3 GetForward(ControlPoint inA, ControlPoint inB, float inF)
    {
        Vector3 p0 = inA.ScaledPosition;                 // <-- Start at A
        Vector3 p1 = inA.ScaledPosition + inA.ScaledTangent;
        Vector3 p2 = inB.ScaledPosition - inB.ScaledTangent;
        Vector3 p3 = inB.ScaledPosition;                 // <-- End at B

        inF = Mathf.Clamp01(inF);
        float fOneMinusT = 1f - inF;
        Vector3 dir = 3f * fOneMinusT * fOneMinusT * (p1 - p0) +
                      6f * fOneMinusT * inF * (p2 - p1) +
                      3f * inF * inF * (p3 - p2);

        return dir;
    }

    public void UpdateDistances()
    {
        if (IsEmpty) return;

        AllPoints[0].Distance = 0.0f;

        for (int i = 1; i < AllPoints.Count; i++)
        {
            ControlPoint A = AllPoints[i - 1];
            ControlPoint B = AllPoints[i];
            B.Distance = A.Distance + CalculateDistance(A, B);
        }
    }

    protected static float CalculateDistance(ControlPoint inA, ControlPoint inB, int inNumSegments = 20)
    {
        float distance = 0.0f;
        Vector3 last = inA.ScaledPosition;
        for (int i = 1; i <= inNumSegments; i++)
        {
            float f = i / (float)inNumSegments;
            Vector3 current = GetPosition(inA, inB, f);
            distance += Vector3.Distance(last, current);
            last = current;
        }

        return distance;
    }
}
