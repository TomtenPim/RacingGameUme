using Unity.VisualScripting;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public GameObject Car;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {


    }

    private void LateUpdate()
    {
        Vector3 Target = Car.transform.position - Car.transform.forward * 5 + Vector3.up * 2;

        //transform.position = Target;
        //transform.position = Vector3.MoveTowards(transform.position, Target, 0.1f);

        int MaxDistance = 3;
        float Distance = Vector3.Distance(transform.position, Target);

        if (Distance > MaxDistance)
        {
            Vector3 Direction = (transform.position - Target).normalized;
            transform.position = Target + Direction * MaxDistance;
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, Target, Distance * Time.deltaTime);
        }

        transform.LookAt(Car.transform.position);
    }
}
