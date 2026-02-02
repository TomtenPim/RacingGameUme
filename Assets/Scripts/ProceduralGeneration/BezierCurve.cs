using NUnit.Framework;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


[ExecuteInEditMode]
public class BezierCurve : MonoBehaviour
{
    private float totalDistance;
    public float TotalDistance => totalDistance;

    [System.Serializable]
    public class ControlPoint
    {
        public Vector3 Position;
        [HideInInspector] public Vector3 ScaledPosition;
        public Vector3 Tangent;
        [HideInInspector] public Vector3 ScaledTangent;
        public float Distance;
    }

    [SerializeField]
    public List<ControlPoint> AllPoints = new List<ControlPoint>();
    public bool IsEmpty => AllPoints.Count == 0;
    public ControlPoint FirstPoint => !IsEmpty ? AllPoints[0] : null;
    public ControlPoint LastPoint => !IsEmpty ? AllPoints[AllPoints.Count - 1] : null;
    public int TotalPoints => AllPoints.Count;

    public ControlPoint closedPoint = new ControlPoint();

    public Vector3 DebugPosition;

    public bool IsClosedLoop = true;

    public float Scale = 1.0f;

    public int PointSplitAmount = 2;

    public Vector2 RandomTangentXRange = new Vector2(-4.0f, 4.0f);

    public Vector2 RandomTangentZRange = new Vector2(-4.0f, 4.0f);

    public Vector2 RandomRangePointPosisionScale = new Vector2(0.9f, 1.1f);
    
    public float GetWidth => GetComponent<TrackMeshGeneration>().TrackWidth;

    public static BezierCurve Instance;
    public ControlPoint ControlPointOnTrack(int inPointIndex)
    {
        if (inPointIndex < 0 || inPointIndex > TotalPoints - 1)
        {
            return null;
        }

        return AllPoints[inPointIndex];
    }

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

        ValidatePoints();
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
        ValidatePoints();
    }

    public void ValidatePoints()
    {
        foreach (var point in AllPoints)
        {
            point.ScaledPosition = point.Position * Scale;
            point.ScaledTangent = point.Tangent * Scale;
        }
        UpdateDistances();
    }

    public void MakeRandomTrack()
    {
        MakeRandomPointsInTriangleShape();
        ValidatePoints();
        GetComponent<TrackMeshGeneration>().UpdateMesh();
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
            for (float f = 0.0f; f <= 1.0f; f += 0.1f)
            {
                Vector3 vCurr = GetPosition(A, B, f);
                if (Vector3.Distance(inLocation, vCurr) <= Vector3.Distance(inLocation, closestPoint))
                {
                    closestPoint = vCurr;
                    outBlendValue = Mathf.Lerp(A.Distance, B.Distance, f);
                    closestPose.position = vCurr;
                    closestPose.rotation = Quaternion.LookRotation(GetForward(A, B, f));
                }

                vLast = vCurr;
            }
        }

        if (IsClosedLoop && Vector3.Distance(inLocation, LastPoint.Position) >= 0)
        {
            ControlPoint A = LastPoint;
            ControlPoint B = closedPoint;

            Vector3 vLast = A.ScaledPosition;
            for (float f = 0.0f; f <= 1.0f; f += 0.1f)
            {
                Vector3 vCurr = GetPosition(A, B, f);
                if (Vector3.Distance(inLocation, vCurr) < Vector3.Distance(inLocation, closestPoint))
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

        if (IsClosedLoop && inDistanceAlongCurve <= totalDistance)
        {
            float blend = Mathf.InverseLerp(LastPoint.Distance, totalDistance, inDistanceAlongCurve);

            return new Pose
            {
                position = GetPosition(LastPoint, closedPoint, blend),
                rotation = Quaternion.LookRotation(GetForward(LastPoint, closedPoint, blend))
            };
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
        totalDistance = 0.0f;
        if (IsEmpty) return;

        AllPoints[0].Distance = 0.0f;

        for (int i = 1; i < AllPoints.Count; i++)
        {
            ControlPoint A = AllPoints[i - 1];
            ControlPoint B = AllPoints[i];
            B.Distance = A.Distance + CalculateDistance(A, B);
        }
        totalDistance += LastPoint.Distance;

        if (IsClosedLoop)
        {
            closedPoint.Position = FirstPoint.Position;
            closedPoint.Tangent = FirstPoint.Tangent;
            totalDistance += CalculateDistance(LastPoint, closedPoint);
            closedPoint.Distance = totalDistance;
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

    public void MakeRandomPointsInTriangleShape()
    {
        AllPoints.Clear();
        List<ControlPoint> SourcePoints = new List<ControlPoint>();
        // Define the triangle control points
        ControlPoint p0 = new ControlPoint { Position = new Vector3(0, 0, 0), Tangent = new Vector3(10, 0, -30) };
        ControlPoint p1 = new ControlPoint { Position = new Vector3(200, 0, 100), Tangent = new Vector3(-5, 0, 5) };
        ControlPoint p2 = new ControlPoint { Position = new Vector3(0, 0, 200), Tangent = new Vector3(0, 0, -20) };
        ControlPoint p3 = new ControlPoint { Position = new Vector3(0, 0, 10), Tangent = new Vector3(2, 0, 0) };
        ControlPoint EndPoint = new ControlPoint { Position = new Vector3(0, 0, 0), Tangent = new Vector3(0, 0, -1) };
        SourcePoints.AddRange(new ControlPoint[] { p0, p1, p2, p3 });

        // Get the blend amount based on the number of splits
        float splitBlendAmount = 1.0f;
        splitBlendAmount = splitBlendAmount / PointSplitAmount;

        List<ControlPoint> NewPoints = new List<ControlPoint>();

        for (int i = 0; i < SourcePoints.Count - 1; i++)
        {
            float currentBlendValue = splitBlendAmount;
            AllPoints.Add(SourcePoints[i]);

            for (int j = 0; j < PointSplitAmount - 1; j++)
            {
                // Calculate new position by blending between the two source points and adding randomness
                Vector3 newPosition = Vector3.Lerp(SourcePoints[i].Position, SourcePoints[i + 1].Position, currentBlendValue);
                newPosition = newPosition * Random.Range(RandomRangePointPosisionScale.x, RandomRangePointPosisionScale.y);

                // Calculate new tangent with randomness
                Vector3 newTangent = new Vector3(
                    SourcePoints[i].Tangent.x + Random.Range(RandomTangentXRange.x, RandomTangentXRange.y),
                    0,
                    SourcePoints[i].Tangent.z + Random.Range(RandomTangentXRange.x, RandomTangentXRange.y));

                ControlPoint newPoint = new ControlPoint { Position = newPosition, Tangent = newTangent };
                NewPoints.Add(newPoint);

                currentBlendValue += splitBlendAmount;
            }

            AllPoints.AddRange(NewPoints);
            NewPoints.Clear();
        }

        AllPoints[AllPoints.Count - 1].Tangent = new Vector3(0, 0, -2);
        AllPoints.Add(EndPoint);
    }
}
