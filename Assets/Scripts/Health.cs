using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField]
    private float Maxhealth = 100f;

    private float health;

    [SerializeField] private HealthCanvas healthCanvas;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = Maxhealth;
    }

    // Update is called once per frame
    public void TakeHealth(float take)
    {
        health -= take;
        Verify();
        UpdateUi();
    }
    public float GetHealth() { return health; }

    private void UpdateUi()
    {
        healthCanvas.UpdateHealth();
    }

    private void Verify()
    {
        if (health < 0)
        {
            Destroy(gameObject);
        }
    }
}
