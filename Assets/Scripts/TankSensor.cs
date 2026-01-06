using UnityEngine;
using Unity.MLAgents.Sensors;



public class TankSensor : SensorComponent {

    [Header("References")]
    [SerializeField] private Tank _tank;
    [SerializeField] private TankFireActuator _tankFireActuator;

    [field: Header("Settings")]
    [field: SerializeField] public string SensorName { get; private set; } = nameof(TankSensor);
    [Min(1)]
    [SerializeField] private int _observationStacks = 5;
    
    private const int ObservationSize = 1; // Should match the number of observations in OnCollectObservations()
    private VectorSensor _sensor;
    


    private void Awake() {
        _tank.OnCollectObservations += OnCollectObservations;
    }



    public override ISensor[] CreateSensors() {
        _sensor = new VectorSensor(ObservationSize, SensorName, ObservationType.Default);
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



    private void OnCollectObservations() {
        _sensor.AddObservation(_tankFireActuator.RelativeReloadTime);
    }



    private void OnDestroy() {
        _tank.OnCollectObservations -= OnCollectObservations;
    }



}