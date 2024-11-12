using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Catraca : MonoBehaviour
{
    public GameObject avisoUI; // Refer�ncia para o UI da mensagem de aviso
    private UIFadeInOut fadeScript;
    private bool interagiuComNPC = false; // Para checar se o NPC foi interagido

    public Transform moveToPosition;  // A posi��o que o jogador deve se mover se n�o interagiu
    public Transform postCatracaPosition; // Posi��o final al�m da catraca
    public float stopDistance = 1.5f; // Dist�ncia para parar antes da catraca

    public PlayerController playerController; // Refer�ncia ao PlayerController

    public Interactable NPCInteractable;

    public float manualMoveSpeed = 0.1f;

    public BoxCollider[] colliders;

    private void Start()
    {
        fadeScript = avisoUI.GetComponent<UIFadeInOut>();
    }

    private void Update()
    {
        // Verificar se j� interagiu com o NPC
        if (NPCInteractable.interacted)
        {
            interagiuComNPC = true;
            avisoUI.SetActive(false); // Remove a mensagem, caso esteja vis�vel
        }
    }

    // M�todo que detecta o clique na catraca
    void OnMouseDown()
    {
        if (interagiuComNPC) // Se j� interagiu com o NPC
        {
            if (playerController != null && moveToPosition != null)
            {
                playerController.MoveToPosition(moveToPosition.position, stopDistance, () =>
                {
                    playerController.MoveManually(postCatracaPosition.position, manualMoveSpeed, stopDistance, DeactivateCollider);
                });
            }
        }
        else // Se n�o interagiu com o NPC
        {
            if (playerController != null && moveToPosition != null)
            {   
                // Mover o jogador at� a posi��o antes da catraca
                playerController.MoveToPosition(moveToPosition.position, stopDistance, () =>
                {
                  if (fadeScript != null)
                  {
                   fadeScript.ShowUI("V� falar com o agente da imigra��o antes!");
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
