using UnityEngine;

public class CheckeredLine : MonoBehaviour
{


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Vehicle"))
        {
            if (other.GetComponent<CarController>() != null)
            {
                RaceManager.Instance.CompletedLap(other.GetComponent<CarController>());
            }
        }
    }

}
