using Fusion;
using UnityEngine;

public class ObjectsMovement : NetworkBehaviour // Mudado para NetworkBehaviour
{
    [SerializeField] private float speed = 50f;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private bool rotation;

    private Vector3 moveDirection;

    public override void Spawned()
    {
        moveDirection = transform.forward;
    }

    public override void FixedUpdateNetwork() // Mudado de Update para FixedUpdateNetwork do Fusion
    {
        // Move o projétil sincronizado pela rede
        transform.position -= moveDirection * speed * Runner.DeltaTime;

        if (rotation)
        {
            transform.rotation *= Quaternion.Euler(Vector3.right * rotationSpeed * Runner.DeltaTime);
        }
    }
}