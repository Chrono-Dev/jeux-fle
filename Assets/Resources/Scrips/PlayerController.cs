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
        // Parar o agente ao alcançar a distância de interação
        agent.SetDestination(transform.position);

        // Se já estamos interagindo ou se o jogador está ocupado com outra coisa, sair
        if (playerBusy) return;

        // Verificar o tipo de interação
        switch (target.InteractionType)
        {
            case InteractableType.NPC:
                Interactable npcInteraction = target.GetComponent<Interactable>();

                if (npcInteraction != null && !isUIOpen) // Garantir que a UI não esteja aberta
                {
                    playerBusy = true;
                    npcInteraction.Interact(this); // Chama a função de interação do NPC
                    ToggleUI(true); // Abrir UI do NPC e desativar movimentação
                }
                break;

            case InteractableType.Item:
                // Lógica para interação com item
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
        // Verificar se o jogador está se movendo manualmente
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
            // Após fechar a UI, liberar a interação e limpar o alvo
            playerBusy = false;
            target = null; // Limpar o alvo após interação para evitar reabertura
        }
    }

    // Função para mover o jogador para uma posição especificada
    public void MoveToPosition(Vector3 targetPosition, float stopDistance, System.Action onArrival = null)
    {
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent não encontrado!");
            return;
        }

        // Definir a posição de destino do agente
        agent.SetDestination(targetPosition);

        // Iniciar a verificação da chegada ao destino
        StartCoroutine(CheckArrival(targetPosition, stopDistance, onArrival));
    }

    // Coroutine que checa se o personagem chegou à posição desejada
    private System.Collections.IEnumerator CheckArrival(Vector3 targetPosition, float stopDistance, System.Action onArrival)
    {
        // Esperar até que o jogador esteja a uma distância menor que stopDistance da posição alvo
        while (Vector3.Distance(agent.transform.position, targetPosition) > stopDistance)
        {
            yield return null; // Esperar o próximo frame
        }

        // Chegou ao destino, chama o callback, se fornecido
        if (onArrival != null)
        {
            onArrival();
        }
    }

    // Função para mover o jogador manualmente para uma posição com NavMeshAgent desativado
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
            // Atualiza a direção
            Vector3 direction = (targetPosition - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

            // Rotaciona suavemente para a nova direção
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookRotationSpeed);

            // Move o personagem suavemente
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            SetAnimations();
            yield return null; // Esperar o próximo frame
        }

        // Em vez de "jogar" para a posição final, defina a posição suavemente
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f) // Distância final muito pequena
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null; // Esperar o próximo frame
        }

        // Garante que o personagem está na posição final e em estado IDLE
        transform.position = targetPosition;
        animator.Play(IDLE);
        agent.enabled = true;
        isMovingManually = false; // Marcar como não em movimento manual
        onArrival?.Invoke();
    }

    // Método para rotacionar o personagem em direção ao destino
    private IEnumerator RotateToTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

        // Rotaciona suavemente para a nova direção
        while (Quaternion.Angle(transform.rotation, lookRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookRotationSpeed);
            yield return null; // Esperar o próximo frame
        }
    }
}
