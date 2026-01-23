using UnityEngine;

public class AIControllerV3 : CarController
{
    public float averageSpeed = 50f; // speed that affects how far the car should look ahead
    public float minLookAhead = 5f;
    public float maxLookAhead = 20f;
    public float stuckSpeedThreshold = 2f;
    public float stuckTime = 1f;
    public float recoveryExitDot = 0.7f;
    public int laneIndex = 0;
    public float laneWidth = 3f;

    private Pose toDriveTo;
    private float stuckTimer;
    private bool isRecovering;


    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        HandleAI();

    }

    private void HandleAI()
    {
        // looks ahead on track to account for incomming turns
        float distanceOnTrack = 0f;
        Pose currentPose = BezierCurve.Instance.GetClosestPoseFromLocation(Car.transform.position, ref distanceOnTrack);

        float currentSpeed = Car.CarBody.linearVelocity.magnitude;
        float speed = Mathf.Clamp01(currentSpeed / averageSpeed);

        float lookAheadDistance = Mathf.Lerp(minLookAhead, maxLookAhead, speed);
        float targetDisctance = distanceOnTrack + lookAheadDistance;

        Pose basePose = BezierCurve.Instance.GetPose(targetDisctance);
        Vector3 offset = basePose.right * laneIndex * laneWidth;

        toDriveTo = new Pose(basePose.position + offset, basePose.rotation);

        Vector3 toTarget = (toDriveTo.position - Car.transform.position).normalized;

        HandleStuck(currentSpeed);

        if (!isRecovering)
        {
            HandleSteering(toTarget);
            HandleThrottle(toTarget);

        }
    }


    private void HandleSteering(Vector3 toTarget)
    {
        float steer = Vector3.Dot(Car.transform.right, toTarget);
        Car.Turn(Mathf.Clamp(steer, -1f, 1f));
    }

    private void HandleThrottle(Vector3 toTarget)
    {
        float forwardDot = Vector3.Dot(Car.transform.forward, toTarget);

        float throttle = Mathf.Clamp01(forwardDot);
        Car.Accelerate(throttle);
    }

    private void HandleStuck(float currentSpeed)
    {
        Vector3 toTarget = (toDriveTo.position - Car.transform.position).normalized;
        float forwardDot = Vector3.Dot(Car.transform.forward, toTarget);

        if (!isRecovering)
        {
            if (currentSpeed < stuckSpeedThreshold)
                stuckTimer += Time.deltaTime;
            else
                stuckTimer = 0f;

            if (stuckTimer > stuckTime)
            {
                isRecovering = true;
                stuckTimer = 0f;
            }
        }

        if (isRecovering)
        {
            Car.Accelerate(-1f);
            // rotates and reversing to recover and get pointed in right direction
            float steer = Vector3.Dot(Car.transform.right, toTarget);
            Car.Turn(Mathf.Clamp(-steer, -1f, 1f));

            if (forwardDot > recoveryExitDot)
            {
                isRecovering = false;
            }
        }
    }


    private void OnDrawGizmos()
    {

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(toDriveTo.position, 0.5f);
        Gizmos.DrawLine(Car.transform.position, toDriveTo.position);
    }
}

