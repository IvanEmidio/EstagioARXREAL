using Fusion;
using UnityEngine;

public class ObjectsMovement : NetworkBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private bool rotation;

    private Rigidbody rb;

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody>();

        if (Object.HasStateAuthority)
        {
            if (rb != null)
            {
                // Damos uma velocidade constante ao Rigidbody na direção para onde o meteoro está a olhar (-transform.forward)
                rb.linearVelocity = -transform.forward * speed;
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Se quisermos rotação contínua
        if (rotation && Object.HasStateAuthority)
        {
            transform.rotation *= Quaternion.Euler(Vector3.right * rotationSpeed * Runner.DeltaTime);
        }
    }
}