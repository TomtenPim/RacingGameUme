using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    protected UnityPhysicsCar Car;

    public Transform CarTransform
    {
        get
        {
            if (Car == null)
            {
                Car = GetComponentInChildren<UnityPhysicsCar>();
            }
            return Car.CarBody.transform;
        }

    }

    protected virtual void Start()
    {
        Car = GetComponentInChildren<UnityPhysicsCar>();
    }

    protected virtual void Update()
    {

    }
}
