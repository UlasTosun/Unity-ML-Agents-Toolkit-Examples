using UnityEngine;
using Unity.MLAgents;



public class Obstacle : MonoBehaviour {

    [Header("Settings")]
    [SerializeField] private bool _randomizeActivation = true;

    private const float Threshold = 0.25f; // at the beginning of the training, the threshold was 0.5f



    public void RandomActivation() {
        if (!_randomizeActivation)
            return;

        // < 0f -> no randomization (do not change current state)
        // [0f, Threshold] -> disable obstacle
        // (Threshold, 1f] -> enable obstacle
        float randomValue = Academy.Instance.EnvironmentParameters.GetWithDefault("randomize_obstacle_activation", 1f);
        if (randomValue < 0f)
            return;
        
        gameObject.SetActive(randomValue > Threshold);
    }



}