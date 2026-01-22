using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class UnityPhysicsCar : MonoBehaviour
{

    InputAction jumpAction;

    [SerializeField] float forwardAcceleration = 40;
    [SerializeField] float turnSpeedMultiplier = 2;
    [SerializeField] float velocityToTurnSpeedMultiplier = 0.0001f;
    [SerializeField] AnimationCurve TurningCurve;
    [SerializeField] GameObject[] frontWheels = new GameObject[2];
    [SerializeField] float tireGripMultplier = 1; 


    Rigidbody carBody;

    public Rigidbody CarBody => carBody;
    int turnDirection = 0;
    float accelerationDirection;
    float turningAngle = 0;
    Quaternion turningQuaternion = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        jumpAction = InputSystem.actions.FindAction("Jump");
        carBody = GetComponent<Rigidbody>();
    }

    public void Accelerate(float Direction)
    {
        accelerationDirection = Direction;
    }

    public void Turn(int Direction)
    {
        if (Vector3.Dot(carBody.transform.forward, carBody.linearVelocity) < -0.05)
        {
            Direction *= -1;
        }

        turningAngle += turnSpeedMultiplier * Direction;
        turnDirection = Direction;
    }

    public float getSpeed()
    {
        return carBody.linearVelocity.magnitude;
    }
    private void FixedUpdate()
    {
        if (turningAngle != 0 && turnDirection == 0)
        {
            turningAngle -= turningAngle / Mathf.Abs(turningAngle) * turnSpeedMultiplier;
        }

        turningAngle = Mathf.Clamp(turningAngle, -30, 30);
        turningQuaternion = Quaternion.AngleAxis(turningAngle, Vector3.up);

        // Vissualy turn front wheels
        foreach (GameObject wheel in frontWheels)
        {
            if (Vector3.Dot(carBody.transform.forward, carBody.linearVelocity) >= -0.05)
            {
                wheel.transform.localRotation = Quaternion.AngleAxis(turningAngle, wheel.transform.up);
            }
            else
            {
                wheel.transform.localRotation = Quaternion.AngleAxis(-turningAngle, wheel.transform.up);
            }
        }

        //Execure acceleration
        if (Physics.Raycast(carBody.transform.position, -carBody.transform.up, 2))
        {
            carBody.AddForce(turningQuaternion * carBody.transform.forward * forwardAcceleration * accelerationDirection);

            if (carBody.linearVelocity != Vector3.zero)
            {
                Quaternion StartQuaternion = carBody.transform.rotation;
                Quaternion TargetQuaternion = Quaternion.LookRotation(turningQuaternion * carBody.transform.forward, carBody.transform.up);
                float turningRate = Mathf.Clamp(Time.deltaTime * carBody.linearVelocity.magnitude * velocityToTurnSpeedMultiplier, 0, 0.3f);

                carBody.transform.rotation = Quaternion.Slerp(StartQuaternion, TargetQuaternion, TurningCurve.Evaluate(carBody.linearVelocity.magnitude) * Time.fixedDeltaTime);
            }
        }

        /*//Turn velocity toward facing, represents tires ability to not slip
        if (Vector3.Dot(carBody.transform.forward, carBody.linearVelocity) >= -0.05)
        {
            CarBody.linearVelocity = (CarBody.linearVelocity.normalized + carBody.transform.forward.normalized * tireGripMultplier).normalized * CarBody.linearVelocity.magnitude;
        }
        else
        {
            CarBody.linearVelocity = (CarBody.linearVelocity.normalized - carBody.transform.forward.normalized * tireGripMultplier).normalized * CarBody.linearVelocity.magnitude;
        }*/


        //Turn velocity toward facing, represents tires ability to not slip
        if (Vector3.Dot(carBody.transform.forward, carBody.linearVelocity) >= -0.05)
        {
            CarBody.linearVelocity = (CarBody.linearVelocity.normalized + ((carBody.transform.forward + turningQuaternion * carBody.transform.forward) / 2).normalized * tireGripMultplier).normalized * CarBody.linearVelocity.magnitude;
        }
        else
        {
            CarBody.linearVelocity = (CarBody.linearVelocity.normalized - ((carBody.transform.forward + turningQuaternion * carBody.transform.forward) / 2).normalized * tireGripMultplier).normalized * CarBody.linearVelocity.magnitude;
            //CarBody.linearVelocity = (CarBody.linearVelocity.normalized - carBody.transform.forward.normalized * tireGripMultplier).normalized * CarBody.linearVelocity.magnitude;
        }
    }

}
