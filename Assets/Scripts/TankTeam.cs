using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.MLAgents;
using UnityEngine.Events;



public class TankTeam : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Tank _tankPrefab;

    [Header("Settings")]
    [Min(1)]
    [SerializeField] private int _tankCount = 1;

    private readonly List<Tank> _tanks = new();
    private SimpleMultiAgentGroup _agentGroup;
    private TankTeamController _teamController;
    
    public event UnityAction OnTankInTeamDead;
    public int TankCount => _agentGroup.GetRegisteredAgents().Count;



    public void Initialize(TankTeamController teamController) {
        _teamController = teamController;
        _teamController.OnGameEnded += OnGameEnded;
        _teamController.OnGameInterrupted += OnGameInterrupted;
        
        _agentGroup = new SimpleMultiAgentGroup();
        SpawnTanks();
        AddMissingTanksToGroup();
    }



    private void SpawnTanks() {
        for (int i = 0; i < _tankCount; i++) {
            Tank tank = Instantiate(_tankPrefab, transform);
            _tanks.Add(tank);
            tank.OnTankDead += OnTankDead;
        }
    }



    private void AddMissingTanksToGroup() {
        foreach (Tank tank in _tanks.Where(tank => !_agentGroup.GetRegisteredAgents().Contains(tank))) {
            tank.gameObject.SetActive(true);
            _agentGroup.RegisterAgent(tank);
        }
    }



    private void OnTankDead() {
        OnTankInTeamDead?.Invoke();
    }



    private void OnGameInterrupted() {
        _agentGroup.GroupEpisodeInterrupted();
    }



    private void OnGameEnded(TankTeam winningTeam) {
        if (winningTeam == this)
            Win();
        else
            Lose();
        
        ResetTeam();
    }



    private void Win() {
        _agentGroup.SetGroupReward(1f);
        _agentGroup.EndGroupEpisode();
        Debug.Log($"Team {name} won!");
    }



    private void Lose() {
        _agentGroup.SetGroupReward(-1f);
        _agentGroup.EndGroupEpisode();
        Debug.Log($"Team {name} lost!");
    }



    private void ResetTeam() {
        AddMissingTanksToGroup();
    }
    


    private void OnDestroy() {
        _teamController.OnGameEnded -= OnGameEnded;
        _teamController.OnGameInterrupted -= OnGameInterrupted;
        _agentGroup.Dispose();
        
        foreach (Tank tank in _tanks) {
            tank.OnTankDead -= OnTankDead;
        }
    }
    


}