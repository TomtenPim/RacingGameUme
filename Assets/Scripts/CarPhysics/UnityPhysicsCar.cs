using UnityEngine;
using UnityEngine.InputSystem;

public class UnityPhysicsCar : MonoBehaviour
{

    InputAction jumpAction;

    [SerializeField] float forwardAcceleration = 40;
    [SerializeField] float turnSpeedMultiplier = 2;
    [SerializeField] float velocityToTurnSpeedMultiplier = 0.0001f;
    [SerializeField] AnimationCurve TurningCurve;

    Rigidbody carBody;

    public Rigidbody CarBody => carBody;

    int turnDirection = 0;

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
        carBody.AddForce(turningQuaternion * carBody.transform.forward * forwardAcceleration * Direction);
    }

    public void Turn(int Direction)
    {
        if (Vector3.Dot(carBody.transform.forward, carBody.linearVelocity) < 0)
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

    // Update is called once per frame
    void Update()
    {

        if (turningAngle != 0 && turnDirection == 0)
        {
            turningAngle -= turningAngle / Mathf.Abs(turningAngle) * turnSpeedMultiplier;
        }

        turningAngle = Mathf.Clamp(turningAngle, -30, 30);
        turningQuaternion = Quaternion.AngleAxis(turningAngle, Vector3.up);


        Debug.DrawLine(carBody.position, (carBody.position + carBody.transform.forward * 15), Color.red);
        Debug.DrawLine(carBody.position, (carBody.position + turningQuaternion * carBody.transform.forward * 10), Color.green);




        if (carBody.linearVelocity != Vector3.zero)
        {
            Quaternion StartQuaternion = carBody.transform.rotation;
            Quaternion TargetQuaternion = Quaternion.LookRotation(turningQuaternion * carBody.transform.forward, carBody.transform.up);
            float turningRate = Mathf.Clamp(Time.deltaTime * carBody.linearVelocity.magnitude * velocityToTurnSpeedMultiplier, 0, 0.3f);

            carBody.transform.rotation = Quaternion.Slerp(StartQuaternion, TargetQuaternion, TurningCurve.Evaluate(carBody.linearVelocity.magnitude) * Time.deltaTime);
        }
    }
}
