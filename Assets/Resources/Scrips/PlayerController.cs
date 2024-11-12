using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using System;
public class PlayerController : MonoBehaviour
{
    const string IDLE = "IDLE";
    const string WALK = "WALK";
    const string INTERACT = "INTERACT";
    const string PICKUP = "PICKUP";

    CustomActions input;
    NavMeshAgent agent;

    Animator animator; 

    [Header("Movement")]
    [SerializeField] ParticleSystem clickEffect;
    [SerializeField] LayerMask clickableLayers;

    float lookRotationSpeed = 8f;

    [Header("Interact")]
    [SerializeField] float interactDistance = 1.5f;
    [SerializeField] ParticleSystem interactEffect;

    bool playerBusy = false;

    Interactable target;

    bool isUIOpen = false;

    private bool isMovingManually = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        input = new CustomActions();
        AssignInputs();
    }

    void AssignInputs()
    {
        input.Main.Move.performed += ctx => ClickToMove(); 
    }

    void ClickToMove()
    {
        if (isUIOpen) return;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100, clickableLayers))
        {
            if (hit.transform.CompareTag("Interactable"))
            {
                target = hit.transform.GetComponent<Interactable>();
                if (clickEffect != null)
                {
                    Instantiate(clickEffect, hit.point += new Vector3(0, 0.1f, 0), clickEffect.transform.rotation);
                }
            }
            else
            {
                target = null;
                agent.destination = hit.point;
                if (clickEffect != null)
                {
                    Instantiate(clickEffect, hit.point += new Vector3(0, 0.1f, 0), clickEffect.transform.rotation);
                }
            }
         
        }
    }

    void OnEnable()
    {
        input.Enable();
    }

    void OnDisable()
    {
        input.Disable();    
    }

     void Update()
    {
        if (isUIOpen) return;

        FaceTarget();
        SetAnimations();
        FollowTarget();
    }

    void FollowTarget()
    {
        if (target == null) { return; }

        if (Vector3.Distance(target.transform.position, transform.position) <= interactDistance)
        {
            ReachDistance();
        }
        else
        {
            agent.SetDestination(target.transform.position);
        }

    }

    void ReachDistance()
    {
        // Parar o agente ao alcan�ar a dist�ncia de intera��o
        agent.SetDestination(transform.position);

        // Se j� estamos interagindo ou se o jogador est� ocupado com outra coisa, sair
        if (playerBusy) return;

        // Verificar o tipo de intera��o
        switch (target.InteractionType)
        {
            case InteractableType.NPC:
                Interactable npcInteraction = target.GetComponent<Interactable>();

                if (npcInteraction != null && !isUIOpen) // Garantir que a UI n�o esteja aberta
                {
                    playerBusy = true;
                    npcInteraction.Interact(this); // Chama a fun��o de intera��o do NPC
                    ToggleUI(true); // Abrir UI do NPC e desativar movimenta��o
                }
                break;

            case InteractableType.Item:
                // L�gica para intera��o com item
                break;
        }
    }
    void FaceTarget()
    {
        if (agent.velocity != Vector3.zero)
        {
            Vector3 direction = (agent.destination - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookRotationSpeed);
        }
    }

    void SetAnimations()
    {
        // Verificar se o jogador est� se movendo manualmente
        if (isMovingManually || (agent.velocity != Vector3.zero))
        {
            animator.Play(WALK);
        }
        else
        {
            animator.Play(IDLE);
        }
    }

    public void ToggleUI(bool state)
    {
        isUIOpen = state;

        if (isUIOpen)
        {
            animator.Play(IDLE);
            playerBusy = true;
        }
        else
        {
            // Ap�s fechar a UI, liberar a intera��o e limpar o alvo
            playerBusy = false;
            target = null; // Limpar o alvo ap�s intera��o para evitar reabertura
        }
    }

    // Fun��o para mover o jogador para uma posi��o especificada
    public void MoveToPosition(Vector3 targetPosition, float stopDistance, System.Action onArrival = null)
    {
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent n�o encontrado!");
            return;
        }

        // Definir a posi��o de destino do agente
        agent.SetDestination(targetPosition);

        // Iniciar a verifica��o da chegada ao destino
        StartCoroutine(CheckArrival(targetPosition, stopDistance, onArrival));
    }

    // Coroutine que checa se o personagem chegou � posi��o desejada
    private System.Collections.IEnumerator CheckArrival(Vector3 targetPosition, float stopDistance, System.Action onArrival)
    {
        // Esperar at� que o jogador esteja a uma dist�ncia menor que stopDistance da posi��o alvo
        while (Vector3.Distance(agent.transform.position, targetPosition) > stopDistance)
        {
            yield return null; // Esperar o pr�ximo frame
        }

        // Chegou ao destino, chama o callback, se fornecido
        if (onArrival != null)
        {
            onArrival();
        }
    }

    // Fun��o para mover o jogador manualmente para uma posi��o com NavMeshAgent desativado
    public void MoveManually(Vector3 targetPosition, float moveSpeed, float stopDistance, System.Action onArrival = null)
    {
        StartCoroutine(MoveToPositionWithoutNavMesh(targetPosition, moveSpeed, stopDistance, onArrival));
    }

    // Coroutine para movimento manual sem NavMeshAgent
    private IEnumerator MoveToPositionWithoutNavMesh(Vector3 targetPosition, float moveSpeed, float stopDistance, Action onArrival)
    {
        agent.enabled = false;
        isMovingManually = true;

        while (Vector3.Distance(transform.position, targetPosition) > stopDistance)
        {
            // Atualiza a dire��o
            Vector3 direction = (targetPosition - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

            // Rotaciona suavemente para a nova dire��o
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookRotationSpeed);

            // Move o personagem suavemente
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            SetAnimations();
            yield return null; // Esperar o pr�ximo frame
        }

        // Em vez de "jogar" para a posi��o final, defina a posi��o suavemente
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f) // Dist�ncia final muito pequena
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null; // Esperar o pr�ximo frame
        }

        // Garante que o personagem est� na posi��o final e em estado IDLE
        transform.position = targetPosition;
        animator.Play(IDLE);
        agent.enabled = true;
        isMovingManually = false; // Marcar como n�o em movimento manual
        onArrival?.Invoke();
    }

    // M�todo para rotacionar o personagem em dire��o ao destino
    private IEnumerator RotateToTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

        // Rotaciona suavemente para a nova dire��o
        while (Quaternion.Angle(transform.rotation, lookRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookRotationSpeed);
            yield return null; // Esperar o pr�ximo frame
        }
    }
}
