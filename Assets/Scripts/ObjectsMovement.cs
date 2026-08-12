using UnityEngine;

public class ObjectsMovement : MonoBehaviour
{
    [SerializeField] private float speed = 50f; // Aumentado para velocidade tipo bala
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private bool rotation;

    private Vector3 moveDirection;

    void Start()
    {
        // Como o Shooter rotacionou o prefab para a mira, transform.forward aponta diretamente para o alvo
        moveDirection = transform.forward;
    }

    void Update()
    {
        // Move o projétil em linha reta para a mira
        transform.position -= moveDirection * speed * Time.deltaTime;

        // Rotação meramente estática/visual (ex: meteoro ou projétil a girar)
        if (rotation)
        {
            transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);
        }
    }
}