using UnityEngine;



public class Projectile : MonoBehaviour {

    [Header("Settings")]
    [SerializeField] private float _speed = 60f;
    [SerializeField] private float _range = 60f;
    [field: SerializeField] public int Damage { get; private set; } = 40;

    [Header("References")]
    [SerializeField] private ParticleSystem _hitEffect;
    [field: SerializeField] public Material Material { get; private set; }

    private Rigidbody _rigidbody;
    
    public Tank Tank { get; private set; }
    


    private void Start() {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.linearVelocity = transform.forward * _speed;

        Invoke(nameof(TimeOut), _range / _speed);
    }



    public void Initialize(Tank tank) {
        Tank = tank;
        Tank.OnEpisodeStarted += OnEpisodeStarted;
    }



    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject == Tank.gameObject)
            return;

        if (!collision.gameObject.TryGetComponent(out Tank otherTank))
            Tank.SetShotResult(ShotResult.Miss);
        
        PlayParticle(collision);
        SelfDestroy();
    }



    private void TimeOut() {
        Tank.SetShotResult(ShotResult.Miss);
        SelfDestroy();
    }
    


    private void PlayParticle(Collision collision) {
        ParticleSystem particle = Instantiate(_hitEffect, collision.contacts[0].point, Quaternion.identity);
        particle.transform.forward = collision.contacts[0].normal;
        ParticleSystem.MainModule hitEffectMain = particle.main;
        hitEffectMain.startColor = Material.color;
    }



    private void OnEpisodeStarted() {
        SelfDestroy();
    }



    private void SelfDestroy() {
        Tank.OnEpisodeStarted -= OnEpisodeStarted;
        gameObject.SetActive(false);
        Destroy(gameObject);
    }



}



public enum ShotResult {

    Miss,
    FriendlyFire,
    HitWithoutDestroyingTank,
    HitAndDestroyTank

}
