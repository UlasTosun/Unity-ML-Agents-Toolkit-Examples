using System;
using UnityEngine;
using UnityEngine.Events;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;



public class Tank : Agent {

    [Header("Settings")]
    [SerializeField] private int _maxHealth = 100;

    public float RelativeHealth => Mathf.Clamp01((float) Health / _maxHealth);
    public int TeamId => GetComponent<BehaviorParameters>().TeamId;
    public event UnityAction OnHealthChanged;
    public event UnityAction OnTankDead;
    public event UnityAction OnEpisodeStarted;
    public event UnityAction OnCollectObservations;

    private LayerMask _groundCheckLayer;
    private int _health;



    public int Health {
        get { return _health; }

        private set {
            if (_health == value)
                return;
            _health = Mathf.Clamp(value, 0, _maxHealth);

            if (_health <= 0)
                Die();

            OnHealthChanged?.Invoke();
        }
    }



    protected override void Awake() {
        _groundCheckLayer = LayerMask.NameToLayer("Ground Check");
        Health = _maxHealth;
    }



    public override void OnEpisodeBegin() {
        Health = _maxHealth;
        OnEpisodeStarted?.Invoke();
    }



    public override void CollectObservations(VectorSensor sensor) {
        OnCollectObservations?.Invoke();
    }



    public void SetShotResult(ShotResult result) {
        switch (result) {
            case ShotResult.Miss:
                AddReward(-0.001f);
                break;
            
            case ShotResult.FriendlyFire:
                AddReward(-0.3f);
                break;

            case ShotResult.HitWithoutDestroyingTank:
                AddReward(0.1f);
                break;

            case ShotResult.HitAndDestroyTank:
                AddReward(0.3f);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
        }
    }



    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == _groundCheckLayer)
            TakeDamage(_maxHealth);
    }



    private void OnCollisionEnter(Collision collision) {
        if (!collision.gameObject.TryGetComponent(out Projectile projectile) || projectile.Tank == this)
            return;

        ShotResult result = projectile.Tank.TeamId == TeamId
            ? ShotResult.FriendlyFire
            : Health <= projectile.Damage
                ? ShotResult.HitAndDestroyTank
                : ShotResult.HitWithoutDestroyingTank;
        
        projectile.Tank.SetShotResult(result); // first, inform the enemy tank about the shot result
        TakeDamage(projectile.Damage); // taking damage may destroy this tank and finish the episode, so we need to call it after the shot result is set to the enemy tank
    }



    private void TakeDamage(int damage) {
        Health -= damage;
        AddReward(-0.1f);
    }



    private void Die() {
        AddReward(-0.3f);
        gameObject.SetActive(false);
        OnTankDead?.Invoke();
    }



}