using UnityEngine;
using UnityEngine.UI;



public class TankUI : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Tank _tank;
    [SerializeField] private TankFireActuator _tankFireActuator;
    [SerializeField] private Image _reloadBar;
    [SerializeField] private Image _healthBar;

    private Camera _camera;



    private void Awake() {
        _camera = Camera.main;
        _tank.OnHealthChanged += UpdateHealthBar;
        UpdateHealthBar();
    }



    private void Update() {
        if (_camera)
            transform.forward = _camera.transform.forward;
        
        UpdateReloadBar();
    }



    private void UpdateReloadBar() {
        _reloadBar.fillAmount = _tankFireActuator.RelativeReloadTime;
    }



    private void UpdateHealthBar() {
        _healthBar.fillAmount = _tank.RelativeHealth;
    }



    private void OnDestroy() {
        _tank.OnHealthChanged -= UpdateHealthBar;
    }



}