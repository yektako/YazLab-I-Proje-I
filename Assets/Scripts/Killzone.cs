using NPC;
using Player;
using UnityEngine;

public class Killzone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            player.UseCheckpoint();
        }
        else if (other.CompareTag("NPC"))
        {
            NPCController npc = other.GetComponent<NPCController>();
            npc.UseCheckpoint();
        }
    }
}
