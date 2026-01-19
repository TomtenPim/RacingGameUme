using UnityEngine;

public class CheckeredLine : MonoBehaviour
{


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Vehicle"))
        {
            RaceManager.Instance.CompletedLap();
        }
    }

}
