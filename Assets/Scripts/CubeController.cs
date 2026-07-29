using Fusion;
using UnityEngine;

public class CubeController : NetworkBehaviour
{
    [SerializeField] private float speed = 5f;

    // Usamos o FixedUpdateNetwork do Fusion em vez do Update normal para a física de rede
    public override void FixedUpdateNetwork()
    {
        // Apenas o jogador com autoridade (quem tem o controlo) move o cubo
        if (HasStateAuthority)
        {
            float moveX = Input.GetAxis("Horizontal"); // A / D ou Setas
            float moveZ = Input.GetAxis("Vertical");   // W / S ou Setas

            Vector3 move = new Vector3(moveX, 0, moveZ) * speed * Runner.DeltaTime;
            transform.Translate(move, Space.World);
        }
    }
}