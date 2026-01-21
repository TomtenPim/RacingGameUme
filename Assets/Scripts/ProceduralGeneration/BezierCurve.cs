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
        public Vector3 Tangent;
        public float Distance;
    }

    [SerializeField]
    public List<ControlPoint> AllPoints = new List<ControlPoint>();

    public bool IsEmpty => AllPoints.Count == 0;
    public ControlPoint FirstPoint => !IsEmpty ? AllPoints[0] : null;
    public ControlPoint LastPoint => !IsEmpty ? AllPoints[AllPoints.Count - 1] : null;

    public float TotalDistance => !IsEmpty ? LastPoint.Distance : 0.0f;

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

    private void OnEnable()
    {
        UpdateDistances();
    }

    public Pose GetPose(float distanceAlongCurve)
    {
        if (IsEmpty)
        {
            throw new System.Exception("Cannot get position on empty Bezier curve.");
        }

        if (distanceAlongCurve < FirstPoint.Distance)
        {
            return new Pose
            {
                position = FirstPoint.Position,
                rotation = Quaternion.LookRotation(FirstPoint.Tangent)
            };
        }

        if (distanceAlongCurve > LastPoint.Distance)
        {
            return new Pose
            {
                position = LastPoint.Position,
                rotation = Quaternion.LookRotation(LastPoint.Tangent)
            };
        }

        for (int i = 1; i < AllPoints.Count; i++)
        {
            ControlPoint A = AllPoints[i - 1];
            ControlPoint B = AllPoints[i];

            if (distanceAlongCurve < B.Distance)
            {
                float blend = Mathf.InverseLerp(A.Distance, B.Distance, distanceAlongCurve);

                return new Pose
                {
                    position = GetPosition(A, B, blend),
                    rotation = Quaternion.LookRotation(GetForward(A, B, blend))
                };
            }
        }

        throw new System.Exception("Should not reach here when getting position on Bezier curve.");
    }

    public static Vector3 GetPosition(ControlPoint A, ControlPoint B, float f)
    {
        Vector3 p0 = A.Position;                 // <-- Start at A
        Vector3 p1 = A.Position + A.Tangent;
        Vector3 p2 = B.Position - B.Tangent;
        Vector3 p3 = B.Position;                 // <-- End at B

        float fOneMinusT = 1.0f - f;

        return p0 * fOneMinusT * fOneMinusT * fOneMinusT +
                p1 * 3 * fOneMinusT * fOneMinusT * f +
                p2 * 3 * fOneMinusT * f * f +
                p3 * f * f * f;
    }

    public static Vector3 GetForward(ControlPoint A, ControlPoint B, float f)
    {
        Vector3 p0 = A.Position;                 // <-- Start at A
        Vector3 p1 = A.Position + A.Tangent;
        Vector3 p2 = B.Position - B.Tangent;
        Vector3 p3 = B.Position;                 // <-- End at B

        f = Mathf.Clamp01(f);
        float fOneMinusT = 1f - f;
        Vector3 dir = 3f * fOneMinusT * fOneMinusT * (p1 - p0) +
                      6f * fOneMinusT * f * (p2 - p1) +
                      3f * f * f * (p3 - p2);

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

    protected static float CalculateDistance(ControlPoint A, ControlPoint B, int numSegments = 20)
    {
        float distance = 0.0f;
        Vector3 last = A.Position;
        for (int i = 1; i <= numSegments; i++)
        {
            float f = i / (float)numSegments;
            Vector3 current = GetPosition(A, B, f);
            distance += Vector3.Distance(last, current);
            last = current;
        }

        return distance;
    }
}
