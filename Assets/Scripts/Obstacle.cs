using UnityEngine;
using Unity.MLAgents;



public class Obstacle : MonoBehaviour {

    [Header("Settings")]
    [SerializeField] private bool _randomizeActivation = true;



    public void RandomActivation() {
        if (!_randomizeActivation)
            return;

        // < 0f -> no randomization (do not change current state)
        // [0f, 0.5f] -> disable obstacle
        // (0.5f, 1f] -> enable obstacle
        float randomValue = Academy.Instance.EnvironmentParameters.GetWithDefault("randomize_obstacle_activation", 1f);
        if (randomValue < 0f)
            return;
        
        gameObject.SetActive(randomValue > 0.5f);
    }



}