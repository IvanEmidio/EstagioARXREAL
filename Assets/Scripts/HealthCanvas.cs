using UnityEngine;
using UnityEngine.UI;

public class HealthCanvas : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private Image healthBarImage; // Renomeado para evitar confusão com o tipo 'Sprite' do Unity

    private Camera mainCamera;
    private void Start()
    {
        mainCamera = Camera.main;
    }
    private void Update()
    {
        transform.rotation = mainCamera.transform.rotation;
    }
    public void UpdateHealth()
    {
        if (health == null || healthBarImage == null) return;

        // Se o seu método GetHealth() já retorna a vida normalizada (entre 0.0 e 1.0):
        healthBarImage.fillAmount = health.GetHealth() / 100;

        // Se o seu GetHealth() retorna a vida atual (ex: 80) e você preciWa dividir pela vida máxima (ex: 100):
        // healthBarImage.fillAmount = health.GetHealth() / health.GetMaxHealth();
    }
}