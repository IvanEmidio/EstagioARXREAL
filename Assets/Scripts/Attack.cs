using UnityEngine;

public class Attack : MonoBehaviour
{
    [SerializeField] private LayerMask mask;
    [SerializeField] private float Damage;

    private void OnTriggerEnter(Collider other)
    {
        // Converte a camada do objeto em bitmask e verifica se ela está presente no LayerMask
        if (((1 << other.gameObject.layer) & mask) != 0)
        {
            other.GetComponent<Health>().TakeHealth(Damage);
        }
    }
}