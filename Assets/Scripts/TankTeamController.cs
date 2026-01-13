using System.Linq;
using UnityEngine;
using UnityEngine.Events;



public class TankTeamController : MonoBehaviour {

    [Header("References")]
    [SerializeField] private TankTeam[] _teams;
    
    public event UnityAction<TankTeam> OnGameEnded;
    public event UnityAction OnGameInterrupted;



    private void Awake() {
        foreach (TankTeam team in _teams) {
            team.Initialize(this);
            team.OnTankInTeamDead += OnTankInTeamDead;
        }
    }



    private void OnTankInTeamDead() {
        CheckForGameEnd();
    }



    private void CheckForGameEnd() {
        // if all teams are dead, end the game, no team wins
        if (_teams.All(team => team.TankCount <= 0)) {
            OnGameEnded?.Invoke(null);
            return;
        }

        // if only one team is alive, that team wins
        TankTeam[] aliveTeams = _teams.Where(team => team.TankCount > 0).ToArray();
        if (aliveTeams.Length == 1) {
            OnGameEnded?.Invoke(aliveTeams[0]); 
        }
    }
    
    
    
    private void OnDestroy() {
        foreach (TankTeam team in _teams) {
            team.OnTankInTeamDead -= OnTankInTeamDead;
        }
    }


    
}