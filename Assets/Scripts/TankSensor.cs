using UnityEngine;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;



public class TankSensor : SensorComponent {

    [Header("References")]
    [SerializeField] private Tank _tank;
    [SerializeField] private TankFireActuator _tankFireActuator;
    [SerializeField] private RayPerceptionSensorComponent3D _environmentRaySensor;
    [SerializeField] private RayPerceptionSensorComponent3D _aimingRaySensor;

    [field: Header("Settings")]
    [field: SerializeField] public string SensorName { get; private set; } = nameof(TankSensor);
    [Min(1)]
    [SerializeField] private int _observationStacks = 5;
    
    private VectorSensor _sensor;
    


    private void Awake() {
        _tank.OnCollectObservations += OnCollectObservations;
    }



    public override ISensor[] CreateSensors() {
        _sensor = new VectorSensor(GetObservationSize(), SensorName, ObservationType.Default);
        switch (_observationStacks) {
            case 1:
                return new ISensor[] { _sensor };

            case > 1:
                return new ISensor[] { _sensor, new StackingSensor(_sensor, _observationStacks) };

            default:
                Debug.LogError($"Observation stacks must be greater than 0, but was {_observationStacks}.");
                return null;
        }

    }



    // Should return the number of observations in OnCollectObservations()
    private int GetObservationSize() {
        return (2 * _environmentRaySensor.RaysPerDirection + 1) + // teamIDs from environment rays
               (2 * _environmentRaySensor.RaysPerDirection + 1) + // relativeHealths from environment rays
               (2 * _aimingRaySensor.RaysPerDirection + 1) + // teamIDs from aiming rays
               (2 * _aimingRaySensor.RaysPerDirection + 1) + // relativeHealths from aiming rays
               6; // internal observations
    }



    private void OnCollectObservations() {
        // Internal observations about the tank itself
        _sensor.AddObservation(_tankFireActuator.RelativeReloadTime); // 1 float
        _sensor.AddObservation(_tank.GetComponent<BehaviorParameters>().TeamId); // 1 float
        _sensor.AddObservation(_tank.RelativeHealth); // 1 float
        _sensor.AddObservation(_tank.transform.localPosition); // 3 floats
        
        // Observations from rays about the environment
        _sensor.AddObservation(GetTeamIDs(_environmentRaySensor.RaySensor.RayPerceptionOutput.RayOutputs));
        _sensor.AddObservation(GetRelativeHealths(_environmentRaySensor.RaySensor.RayPerceptionOutput.RayOutputs));
        _sensor.AddObservation(GetTeamIDs(_aimingRaySensor.RaySensor.RayPerceptionOutput.RayOutputs));
        _sensor.AddObservation(GetRelativeHealths(_aimingRaySensor.RaySensor.RayPerceptionOutput.RayOutputs));
    }



    private static float[] GetTeamIDs(RayPerceptionOutput.RayOutput[] rayOutputs) {
        // Team IDs are not normalized, they may exceed 1. Consider normalizing them!
        float[] teamIDs = new float[rayOutputs.Length];
        for (int i = 0; i < rayOutputs.Length; i++) {
            teamIDs[i] = rayOutputs[i].HitGameObject?.GetComponent<BehaviorParameters>()?.TeamId ?? -1f;
        }

        return teamIDs;
    }



    private static float[] GetRelativeHealths(RayPerceptionOutput.RayOutput[] rayOutputs) {
        float[] relativeHealths = new float[rayOutputs.Length];
        for (int i = 0; i < rayOutputs.Length; i++) {
            relativeHealths[i] = rayOutputs[i].HitGameObject?.GetComponent<Tank>()?.RelativeHealth ?? -1f;
        }

        return relativeHealths;
    }
    


    private void OnDestroy() {
        _tank.OnCollectObservations -= OnCollectObservations;
    }



}