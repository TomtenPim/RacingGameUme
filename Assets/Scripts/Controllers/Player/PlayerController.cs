using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : CarController
{
    InputAction moveAction;
    InputAction driftAction;
    InputAction pauseAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();

        moveAction = InputSystem.actions.FindAction("Move");
        driftAction = InputSystem.actions.FindAction("Drift");
        pauseAction = InputSystem.actions.FindAction("Pause");

        pauseAction.performed += pauseGame;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (Time.timeScale == 0) return;

        Vector2 moveDirection = moveAction.ReadValue<Vector2>();

        if(moveAction.IsPressed())
        {
            Car.Turn((int)moveDirection.x);

            Car.Accelerate(Mathf.Clamp(moveDirection.y, -0.5f, 1));
        }
        else if (moveAction.WasReleasedThisFrame())
        {
            Car.Turn(0);
        }

        float carSpeed = Car.getSpeed();
        float maxSpeed = 240f;
        UIManager.Instance.updateSpeedometer(carSpeed/maxSpeed);

    }



    private void pauseGame(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            UIManager.Instance.Pause();
        }
    }

}
