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

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        AccelerationHandler();
        TurningHandler();

    }

    private void ChangeDriveState(AIDriveState inDriveState)
    {
        if (inDriveState != driveState)
        {
            return;
        }
        driveState = inDriveState;
    }
    private void ChangeTurnState(AITurnState inTurnState)
    {
        if (inTurnState != turnState)
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
                Car.getSpeed();
                break;
        }

    }

    private void TurningHandler()
    {

    }

}
