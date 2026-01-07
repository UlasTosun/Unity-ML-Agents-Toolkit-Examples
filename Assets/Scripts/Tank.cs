using UnityEngine;
using UnityEngine.Events;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;



public class Tank : Agent {

    [Header("Settings")]
    [SerializeField] private int _maxHealth = 100;

    public float RelativeHealth => Mathf.Clamp01((float) Health / _maxHealth);
    public event UnityAction OnHealthChanged;
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



    public void HitOnTarget(bool isEnemyDestroyed) {
        Debug.Log("Hit target" + (isEnemyDestroyed ? " and destroyed it." : "."));
        if (isEnemyDestroyed) {
            SetReward(1f);
            EndEpisode();
            Debug.Log("Enemy tank destroyed, ending episode.");
        } else {
            AddReward(0.1f);
        }
    }



    public void MissTarget() {
        Debug.Log("Missed target");
        AddReward(-0.05f);
    }



    public void TakeDamage(int damage) {
        Debug.Log($"Tank took {damage} damage");
        Health -= damage;

        if (Health <= 0)
            Die();
    }



    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == _groundCheckLayer)
            TakeDamage(_maxHealth);
    }



    private void Die() {
        SetReward(-1f);
        EndEpisode();
        Debug.Log("Tank destroyed, ending episode.");
    }



}