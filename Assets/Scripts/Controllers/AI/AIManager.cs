using UnityEngine;

public class AIManager : MonoBehaviour
{
    void Start()
    {
        AIControllerV3[] aiCars = FindObjectsOfType<AIControllerV3>();
        int count = aiCars.Length;

        int lane = -count / 2;

        for (int i = 0; i < count; i++)
        {
            int laneIndex = lane + i;
           
            if (count % 2 == 0 && laneIndex >= 0)
            {
                laneIndex += 1;

            }

            aiCars[i].laneIndex = laneIndex;

        }
    }
}
