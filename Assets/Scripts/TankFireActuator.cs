using UnityEngine;
using Unity.MLAgents.Actuators;



public class TankFireActuator : ActuatorComponent, IActuator {

    [Header("References")]
    [SerializeField] private Projectile _projectilePrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private ParticleSystem _fireEffect;
    
    [field: Header("Settings")]
    [field: SerializeField] public string Name { get; private set; } = nameof(TankFireActuator);
    [SerializeField] private float _reloadTime = 1f;

    private Tank _tank;
    private InputSystemActions _inputActions;
    private float _timeSinceLastShot;

    public float RelativeReloadTime => Mathf.Clamp01(_timeSinceLastShot / _reloadTime);
    public Projectile ActiveProjectile { get; private set;}
    
    public override ActionSpec ActionSpec { get; } = ActionSpec.MakeDiscrete(2);



    private void Awake() {
        _inputActions = new InputSystemActions();
        _inputActions.Tank.Enable();

        _tank = GetComponent<Tank>();
        _tank.OnEpisodeStarted += OnEpisodeStarted;

        ParticleSystem.MainModule fireEffectMain = _fireEffect.main;
        fireEffectMain.startColor = _projectilePrefab.Material.color;
    }



    public override IActuator[] CreateActuators() {
        return new IActuator[] { this };
    }



    private void OnEpisodeStarted() {
        _timeSinceLastShot = 0f;
    }



    private void Update() {
        _timeSinceLastShot += Time.deltaTime;
    }



    public void OnActionReceived(ActionBuffers actionBuffers) {
        Fire(actionBuffers.DiscreteActions[0]);
    }



    private void Fire(int action) {
        // 0: do not fire, 1: fire
        if (!_projectilePrefab || !_firePoint || action == 0 || _timeSinceLastShot < _reloadTime || ActiveProjectile)
            return;

        ActiveProjectile = Instantiate(_projectilePrefab, _firePoint.position, _firePoint.rotation);
        ActiveProjectile.Initialize(_tank);

        _timeSinceLastShot = 0f;
        _fireEffect?.Play();
    }



    public void Heuristic(in ActionBuffers actionBuffersOut) {
        ActionSegment<int> discreteActions = actionBuffersOut.DiscreteActions;
        discreteActions[0] = _inputActions.Tank.Fire.IsPressed() ? 1 : 0;
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