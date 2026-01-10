using System.Collections.Generic;
using UnityEngine;



public class TankTeam : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Tank _tankPrefab;

    [Header("Settings")]
    [Min(1)]
    [SerializeField] private int _tankCount = 1;

    private List<Tank> _tanks = new();



    private void Awake() {
        SpawnTanks();
    }



    private void SpawnTanks() {
        for (int i = 0; i < _tankCount; i++) {
            _tanks.Add(Instantiate(_tankPrefab, transform));
        }
    }



}