using UnityEngine;
using Unity.MLAgents.Actuators;



public class TankMoveActuator : ActuatorComponent, IActuator {
    
    [Header("References")]
    [SerializeField] private Tank _tank;
    [SerializeField] private Rigidbody _rigidbody;

    [field: Header("Settings")]
    [field: SerializeField] public string Name { get; private set; } = nameof(TankMoveActuator);
    [SerializeField] private float _maxSpeed = 10f;
    [SerializeField] private float _acceleration = 10f;
    [SerializeField] private float _maxAngularSpeed = 180f;
    [SerializeField] private float _angularAcceleration = 180f;

    private InputSystemActions _inputActions;
    private float _speed;
    private float _angularSpeed;

    public override ActionSpec ActionSpec { get; } = ActionSpec.MakeDiscrete(3, 3);



    private void Awake() {
        _inputActions = new InputSystemActions();
        _inputActions.Tank.Enable();

        _tank.OnEpisodeStarted += OnEpisodeStarted;
    }



    public override IActuator[] CreateActuators() {
        return new IActuator[] { this };
    }



    private void OnEpisodeStarted() {
        _speed = 0f;
        _angularSpeed = 0f;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }
    


    public void OnActionReceived(ActionBuffers actionBuffers) {
        Move(actionBuffers.DiscreteActions[0]);
        Rotate(actionBuffers.DiscreteActions[1]);
    }



    // 0: stop, 1: forward, -1: backward
    private void Move(int action) {
        float speedChange = _acceleration * Time.fixedDeltaTime;
        if (action == 0) { // stop
            _speed = _speed > 0f
                ? Mathf.Clamp(_speed - speedChange, 0f, _maxSpeed)
                : Mathf.Clamp(_speed + speedChange, -_maxSpeed, 0f);
        } else { // accelerate
            _speed = Mathf.Clamp(_speed + action * speedChange, -_maxSpeed, _maxSpeed);
        }

        Vector3 moveDirection = _speed * Time.fixedDeltaTime * transform.forward;
        _rigidbody.MovePosition(_rigidbody.position + moveDirection);
    }



    // 0: no turn, 1: right turn, -1: left turn
    private void Rotate(int action) {
        float angularSpeedChange = _angularAcceleration * Time.fixedDeltaTime;
        if (action == 0) { // stop
            _angularSpeed = _angularSpeed > 0f
                ? Mathf.Clamp(_angularSpeed - angularSpeedChange, 0f, _maxAngularSpeed)
                : Mathf.Clamp(_angularSpeed + angularSpeedChange, -_maxAngularSpeed, 0f);
        } else { // accelerate
            _angularSpeed = Mathf.Clamp(_angularSpeed + action * angularSpeedChange, -_maxAngularSpeed, _maxAngularSpeed);
        }
        
        Quaternion turnRotation = Quaternion.Euler(0f, _angularSpeed * Time.fixedDeltaTime, 0f);
        _rigidbody.MoveRotation(_rigidbody.rotation * turnRotation);
    }



    public void Heuristic(in ActionBuffers actionBuffersOut) {
        ActionSegment<int> discreteActions = actionBuffersOut.DiscreteActions;
        Vector2 moveInput = _inputActions.Tank.Move.ReadValue<Vector2>();

        // Move
        discreteActions[0] = moveInput.y switch {
                                 > 0 => 1,
                                 < 0 => -1,
                                 _ => 0
                             };

        // Turn
        discreteActions[1] = moveInput.x switch {
                                 > 0 => 1,
                                 < 0 => -1,
                                 _ => 0
                             };
    }



    public void WriteDiscreteActionMask(IDiscreteActionMask actionMask) {

    }



    public void ResetData() {

    }



    private void OnDestroy() {
        _tank.OnEpisodeStarted -= OnEpisodeStarted;

        _inputActions.Tank.Disable();
        _inputActions.Dispose();
    }

}