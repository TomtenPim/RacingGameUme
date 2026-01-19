using UnityEngine;
using UnityEngine.InputSystem;

public class Car : MonoBehaviour
{

    InputAction jumpAction;
    //GameObject[] Wheels;  

    float acceleration = new();
    float velocity  = new();

    float dragAndFrictionMultiplier = 0.95f;

    Vector3 currentDirection;
    Vector3 turningDirection;

    float degreeTurning = 0; 
   

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        jumpAction = InputSystem.actions.FindAction("Jump");
    }

    // Update is called once per frame
    void Update()
    {
        acceleration = new();
        turningDirection = Vector3.forward;
        degreeTurning = 0;

                   
        if (Keyboard.current.spaceKey.IsPressed())
        {
            acceleration = 1;
        }

        if (Keyboard.current.aKey.IsPressed())
        {
            //turningDirection = Vector3.left;
            degreeTurning = -10;
        }

        if (Keyboard.current.dKey.IsPressed()) 
        {
            //turningDirection = Vector3.right;
            degreeTurning = 10;
        }

        velocity += acceleration;
        velocity = velocity * dragAndFrictionMultiplier;

        this.transform.rotation = Quaternion.AngleAxis(transform.rotation.eulerAngles.y + degreeTurning * velocity * Time.deltaTime, Vector3.up);
        this.transform.position += this.transform.rotation* Vector3.forward * velocity * Time.deltaTime;
    }
}
