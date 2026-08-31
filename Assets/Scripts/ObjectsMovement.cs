using Fusion;
using UnityEngine;

public class ObjectsMovement : NetworkBehaviour
{
    [SerializeField] private float speed = 4f;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private bool rotation;

    // Usamos uma variável sincronizada para a direção para garantir que ambos sabem para onde vai
    [Networked] private Vector3 moveDirection { get; set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            moveDirection = transform.forward;
        }
    }

    public override void FixedUpdateNetwork()
    {
        // No Fusion, o movimento de objetos sincronizados por NetworkTransform 
        // deve ser processado por quem tem a State Authority (geralmente o Master Client ou quem gerou).
        // O Network Transform encarrega-se automaticamente de enviar a nova posição para os outros PCs.
        if (Object.HasStateAuthority)
        {
            transform.position -= moveDirection * speed * Runner.DeltaTime;

            if (rotation)
            {
                transform.rotation *= Quaternion.Euler(Vector3.right * rotationSpeed * Runner.DeltaTime);
            }
        }
    }
}