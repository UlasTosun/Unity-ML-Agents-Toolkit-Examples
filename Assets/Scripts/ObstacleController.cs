using UnityEngine;



public class ObstacleController : MonoBehaviour {

    [Header("References")]
    [SerializeField] private TankTeamController _tankTeamController;
    [SerializeField] private Obstacle[] _obstacles;



    private void Awake() {
        RandomizeObstacles();
        _tankTeamController.OnGameEnded += OnGameEnded;
        _tankTeamController.OnGameInterrupted += OnGameInterrupted;
    }



    private void OnGameEnded(TankTeam winner) {
        RandomizeObstacles();
    }
    
    
    
    private void OnGameInterrupted() {
        RandomizeObstacles();
    }



    private void RandomizeObstacles() {
        if (_obstacles == null)
            return;
        
        foreach (Obstacle obstacle in _obstacles) {
            obstacle.RandomActivation();
        }
    }
    
    
    
    private void OnDestroy() {
        _tankTeamController.OnGameEnded -= OnGameEnded;
        _tankTeamController.OnGameInterrupted -= OnGameInterrupted;
    }


}