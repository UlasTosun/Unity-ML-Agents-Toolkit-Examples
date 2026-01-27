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
        return //(2 * _environmentRaySensor.RaysPerDirection + 1) * 2 + // teamIDs from environment rays
               (2 * _environmentRaySensor.RaysPerDirection + 1) + // relativeHealths from environment rays
               (2 * _environmentRaySensor.RaysPerDirection + 1) + // distances from environment rays
               (2 * _environmentRaySensor.RaysPerDirection + 1) * _environmentRaySensor.DetectableTags.Count + // tagIndexes from environment rays
               //(2 * _aimingRaySensor.RaysPerDirection + 1) * 2 + // teamIDs from aiming rays
               (2 * _aimingRaySensor.RaysPerDirection + 1) + // relativeHealths from aiming rays
               (2 * _aimingRaySensor.RaysPerDirection + 1) + // distances from aiming rays
               (2 * _aimingRaySensor.RaysPerDirection + 1) * _aimingRaySensor.DetectableTags.Count + // tagIndexes from aiming rays
               8; // internal observations
    }



    private void OnCollectObservations() {
        // Internal observations about the tank itself
        _sensor.AddObservation(_tankFireActuator.RelativeReloadTime); // 1 float
        _sensor.AddObservation(_tank.GetComponent<BehaviorParameters>().TeamId); // 1 float
        _sensor.AddObservation(_tank.RelativeHealth); // 1 float
        _sensor.AddObservation(_tank.transform.localPosition); // 3 floats
        _sensor.AddObservation(_tank.GetComponent<TankMoveActuator>().RelativeSpeed); // 1 float
        _sensor.AddObservation(_tank.GetComponent<TankMoveActuator>().RelativeAngularSpeed); // 1 float
        
        // Observations from rays about the environment sensor
        //_sensor.AddObservation(GetTeamIDs(_environmentRaySensor.RaySensor.RayPerceptionOutput.RayOutputs));
        _sensor.AddObservation(GetRelativeHealths(_environmentRaySensor.RaySensor.RayPerceptionOutput.RayOutputs));
        _sensor.AddObservation(GetRelativeDistances(_environmentRaySensor.RaySensor.RayPerceptionOutput.RayOutputs));
        _sensor.AddObservation(GetTagIndexes(_environmentRaySensor.RaySensor.RayPerceptionOutput.RayOutputs,
                                                       _environmentRaySensor.DetectableTags.Count));
        
        // Observations from rays about the aiming sensor
        //_sensor.AddObservation(GetTeamIDs(_aimingRaySensor.RaySensor.RayPerceptionOutput.RayOutputs));
        _sensor.AddObservation(GetRelativeHealths(_aimingRaySensor.RaySensor.RayPerceptionOutput.RayOutputs));
        _sensor.AddObservation(GetRelativeDistances(_aimingRaySensor.RaySensor.RayPerceptionOutput.RayOutputs));
        _sensor.AddObservation(GetTagIndexes(_aimingRaySensor.RaySensor.RayPerceptionOutput.RayOutputs,
                                                       _aimingRaySensor.DetectableTags.Count));
    }



    private static float[] GetTeamIDs(RayPerceptionOutput.RayOutput[] rayOutputs) {
        float[] teamIDs = new float[rayOutputs.Length];
        for (int i = 0; i < rayOutputs.Length; i++) {
            int index = rayOutputs[i].HitGameObject?.GetComponent<BehaviorParameters>()?.TeamId ?? 2;
            float[] oneHotTeamID = OneHotEncode(index, 3);
            for (int j = 0; j < 3; j++) {
                teamIDs[i * 3 + j] = oneHotTeamID[j];
            }
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



    private static float[] GetRelativeDistances(RayPerceptionOutput.RayOutput[] rayOutputs) {
        float[] relativeDistances = new float[rayOutputs.Length];
        for (int i = 0; i < rayOutputs.Length; i++) {
            relativeDistances[i] = rayOutputs[i].HitFraction;
        }

        return relativeDistances;
    }



    private static float[] GetTagIndexes(RayPerceptionOutput.RayOutput[] rayOutputs, int tagCount) {
        tagCount = Mathf.Max(tagCount, 1);
        
        float[] tagIndexes = new float[rayOutputs.Length * tagCount];
        for (int i = 0; i < rayOutputs.Length; i++) {
            float[] oneHotTagIndex = OneHotEncode(rayOutputs[i].HitTagIndex, tagCount);
            for (int j = 0; j < tagCount; j++) {
                tagIndexes[i * tagCount + j] = oneHotTagIndex[j];
            }
        }

        return tagIndexes;
    }



    private static float[] OneHotEncode(int value, int length) {
        float[] oneHot = new float[length];
        if (value >= 0 && value < length) {
            oneHot[value] = 1f;
        }
        return oneHot;
    }



    private void OnDestroy() {
        _tank.OnCollectObservations -= OnCollectObservations;
    }



}