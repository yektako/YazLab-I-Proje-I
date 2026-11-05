using NPC;
using Player;
using UnityEngine;

public class Killzone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController controller = other.GetComponent<PlayerController>();
            controller.UseCheckpoint();
        }
        else if (other.CompareTag("NPC"))
        {
            NPCController controller = other.GetComponent<NPCController>();
            controller.UseCheckpoint();
        }
    }
}
