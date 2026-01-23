using UnityEngine;

public class LineTrigger : MonoBehaviour
{
    public enum LineTriggerType
    {
        CheckeredLine,
        HalfPointLine
    }

    [SerializeField] private LineTriggerType triggerType;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Vehicle"))
        {
            if (other.GetComponentInParent<CarController>() != null)
            {
                switch (triggerType)
                {

                    case LineTriggerType.CheckeredLine:
                        RaceManager.Instance.CompletedLap(other.GetComponentInParent<CarController>());
                        break;
                    case LineTriggerType.HalfPointLine:
                        RaceManager.Instance.CarPassedHalfway(other.GetComponentInParent<CarController>());
                        break;

                }
            }
        }
    }

}
