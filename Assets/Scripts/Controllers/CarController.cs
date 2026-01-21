using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    protected UnityPhysicsCar Car;

    protected virtual void Start()
    {
        Car = GetComponentInChildren<UnityPhysicsCar>();
    }

    protected virtual void Update()
    {
        
    }
}
