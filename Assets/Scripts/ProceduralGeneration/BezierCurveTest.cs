using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class BezierCurveTest : MonoBehaviour
{
    [System.Serializable]
    public class ControlPoint
    {
        public Vector3 Position;
        public Vector3 Tangent;
    }

    [SerializeField]
    public List<ControlPoint> AllPoints = new List<ControlPoint>();

    public bool IsEmpty => AllPoints.Count == 0;
    public ControlPoint FirstPoint => !IsEmpty ? AllPoints[0] : null;
    public ControlPoint LastPoint => !IsEmpty ? AllPoints[AllPoints.Count - 1] : null;

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
}
