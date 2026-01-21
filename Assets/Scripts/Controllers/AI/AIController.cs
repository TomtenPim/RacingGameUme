using UnityEngine;

public class AIController : CarController
{
    enum AIDriveState
    {
        Idle,
        Accelerate,
        BackUp,
        Brake,
    }

    enum AITurnState
    {
        Idle,
        LeftTurn,
        RightTurn
    }

    private AIDriveState driveState = AIDriveState.Idle;
    private AITurnState turnState = AITurnState.Idle;
    private Pose toDriveTo;

    public float CarMoveDirection => Vector3.Dot(Car.CarBody.transform.forward, Car.CarBody.linearVelocity);

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        AccelerationHandler();
        TurningHandler();

        float blendValue = 0;
        toDriveTo = BezierCurve.Instance.GetClosestPoseFromLocation(BezierCurve.Instance.DebugPosition, ref blendValue);

        BezierCurve.Instance.DebugPosition = Car.transform.position + Car.transform.forward * 10;

        ChangeDriveState(AIDriveState.Accelerate);
        if (Car.transform.forward != toDriveTo.forward)
        {
            Vector3 vector = toDriveTo.position - Car.transform.position;
            float dotProduct = Vector3.Dot(Car.transform.right, vector);
            Debug.Log(dotProduct);
            if (dotProduct >= 0f && dotProduct <= 0.2f)
            {
                ChangeTurnState(AITurnState.Idle);
            }
            else if (dotProduct > 0.2f)
            {
                ChangeTurnState(AITurnState.RightTurn);
            }
            else if (dotProduct < 0f)
            {
                ChangeTurnState(AITurnState.LeftTurn);
            }

            Debug.Log(turnState);
        }
    }

    private void ChangeDriveState(AIDriveState inDriveState)
    {
        if (inDriveState == driveState)
        {
            return;
        }
        driveState = inDriveState;
    }
    private void ChangeTurnState(AITurnState inTurnState)
    {
        if (inTurnState == turnState)
        {
            return;
        }
        turnState = inTurnState;
    }

    private void AccelerationHandler()
    {
        switch (driveState)
        {
            case AIDriveState.Idle:
                break;

            case AIDriveState.Accelerate:
                Car.Accelerate(1);
                break;

            case AIDriveState.BackUp:
                Car.Accelerate(-1);
                break;

            case AIDriveState.Brake:
                if (CarMoveDirection > 0.1f)
                {
                    Car.Accelerate(-1);
                }
                else if (CarMoveDirection < -0.1f)
                {
                    Car.Accelerate(1);
                }
                break;
        }

    }

    private void TurningHandler()
    {
        switch (turnState)
        {
            case AITurnState.LeftTurn:
                Car.Turn(-1);
                break;

            case AITurnState.RightTurn:
                Car.Turn(1);
                break;

            case AITurnState.Idle:
                Car.Turn(0);
                break;
        }

    }

}
