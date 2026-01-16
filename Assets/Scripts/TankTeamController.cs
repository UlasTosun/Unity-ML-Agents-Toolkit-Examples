using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;



public class TankTeamController : MonoBehaviour {

    [Header("References")]
    [SerializeField] private TankTeam[] _teams;

    [Header("Settings")]
    [Min(0)]
    [SerializeField] private int _maxStep;
    [SerializeField] private SpawnPosition[] _spawnPositions = { };
    
    public event UnityAction<TankTeam> OnGameEnded;
    public event UnityAction OnGameInterrupted;

    private int _stepCount;
    private SpawnPosition[] _spawnPositionsShuffled;



    private void Awake() {
        if (_teams.Length != _spawnPositions.Length)
            Debug.LogError("The number of spawn positions must match the number of teams.");
        
        ShuffleSpawnPositions();
        
        foreach (TankTeam team in _teams) {
            team.Initialize(this);
            team.OnTankInTeamDead += OnTankInTeamDead;
        }
    }



    private void FixedUpdate() {
        _stepCount++;
        if (_maxStep <= 0 || _stepCount < _maxStep)
            return;
        
        _stepCount = 0;
        Debug.Log("Max step reached, interrupting game.");
        ShuffleSpawnPositions();
        OnGameInterrupted?.Invoke();
    }



    private void OnTankInTeamDead() {
        CheckForGameEnd();
    }



    private void CheckForGameEnd() {
        // if all teams are dead, end the game, no team wins
        if (_teams.All(team => team.TankCount <= 0)) {
            ShuffleSpawnPositions();
            OnGameEnded?.Invoke(null);
            return;
        }

        // if only one team is alive, that team wins
        TankTeam[] aliveTeams = _teams.Where(team => team.TankCount > 0).ToArray();
        if (aliveTeams.Length == 1) {
            ShuffleSpawnPositions();
            OnGameEnded?.Invoke(aliveTeams[0]); 
        }
    }



    private void ShuffleSpawnPositions() {
        _spawnPositionsShuffled = _spawnPositions.OrderBy(spawnPosition => UnityEngine.Random.value).ToArray();
    }
    
    
    
    public SpawnPosition GetRandomSpawnPosition(TankTeam team) {
        int index = Array.IndexOf(_teams, team);
        return _spawnPositionsShuffled[index];
    }



    private void OnDrawGizmosSelected() {
        foreach (SpawnPosition spawnPosition in _spawnPositions) {
            Color color = Gizmos.color;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(spawnPosition.Center, spawnPosition.Radius);
            Gizmos.color = color;
        }
    }
    
    
    
    private void OnDestroy() {
        foreach (TankTeam team in _teams) {
            team.OnTankInTeamDead -= OnTankInTeamDead;
        }
    }


    
}