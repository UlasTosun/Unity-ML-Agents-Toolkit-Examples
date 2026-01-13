using UnityEngine;
using UnityEngine.Events;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;



public class Tank : Agent {

    [Header("Settings")]
    [SerializeField] private int _maxHealth = 100;

    public float RelativeHealth => Mathf.Clamp01((float) Health / _maxHealth);
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



    public void HitOnTarget(bool isEnemyDestroyed) {
        //Debug.Log("Hit target" + (isEnemyDestroyed ? " and destroyed it." : "."));
        if (isEnemyDestroyed) {
            SetReward(1f);
            //Debug.Log("Enemy tank destroyed, ending episode.");
        } else {
            AddReward(0.1f);
        }
    }



    public void MissTarget() {
        AddReward(-0.05f);
    }



    public void TakeDamage(int damage) {
        Health -= damage;
        AddReward(-Mathf.Clamp((float)damage / _maxHealth, 0f, 1f));
    }



    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == _groundCheckLayer)
            TakeDamage(_maxHealth);
    }



    private void Die() {
        Debug.Log("Tank destroyed " + gameObject.name);
        SetReward(-1f); // TODO remove this for the team mode
        gameObject.SetActive(false);
        OnTankDead?.Invoke();
    }



}