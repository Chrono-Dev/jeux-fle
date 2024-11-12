using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Catraca : MonoBehaviour
{
    public GameObject avisoUI; // Referência para o UI da mensagem de aviso
    private UIFadeInOut fadeScript;
    private bool interagiuComNPC = false; // Para checar se o NPC foi interagido

    public Transform moveToPosition;  // A posição que o jogador deve se mover se não interagiu
    public Transform postCatracaPosition; // Posição final além da catraca
    public float stopDistance = 1.5f; // Distância para parar antes da catraca

    public PlayerController playerController; // Referência ao PlayerController

    public Interactable NPCInteractable;

    public float manualMoveSpeed = 0.1f;

    public BoxCollider[] colliders;

    private void Start()
    {
        fadeScript = avisoUI.GetComponent<UIFadeInOut>();
    }

    private void Update()
    {
        // Verificar se já interagiu com o NPC
        if (NPCInteractable.interacted)
        {
            interagiuComNPC = true;
            avisoUI.SetActive(false); // Remove a mensagem, caso esteja visível
        }
    }

    // Método que detecta o clique na catraca
    void OnMouseDown()
    {
        if (interagiuComNPC) // Se já interagiu com o NPC
        {
            if (playerController != null && moveToPosition != null)
            {
                playerController.MoveToPosition(moveToPosition.position, stopDistance, () =>
                {
                    playerController.MoveManually(postCatracaPosition.position, manualMoveSpeed, stopDistance, DeactivateCollider);
                });
            }
        }
        else // Se não interagiu com o NPC
        {
            if (playerController != null && moveToPosition != null)
            {   
                // Mover o jogador até a posição antes da catraca
                playerController.MoveToPosition(moveToPosition.position, stopDistance, () =>
                {
                  if (fadeScript != null)
                  {
                   fadeScript.ShowUI("Vá falar com o agente da imigração antes!");
                  }          
                });
            }
        }
    }

    private void DeactivateCollider()
    {
   
        foreach (BoxCollider collider in colliders)
        {
            collider.enabled = false; // Desativar o collider
        }
    }

}
