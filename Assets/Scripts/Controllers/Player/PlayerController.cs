using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : CarController
{

    InputAction moveAction;
    InputAction driftAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        driftAction = InputSystem.actions.FindAction("Drift");
    }

    // Update is called once per frame
    void Update()
    {


        if(Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Debug.Log("Pause");
        }

        Debug.Log(moveAction.ReadValue<Vector2>() + " drift: " + driftAction.IsPressed());

    }
}
