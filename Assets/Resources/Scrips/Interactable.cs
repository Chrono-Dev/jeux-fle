using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum InteractableType { NPC, Item }

public class Interactable : MonoBehaviour
{
    public Actor myActor { get; private set; }
    public InteractableType InteractionType;

    public GameObject uiPanel; // Referência ao painel da UI
    public Button continueButton;
    public TextMeshProUGUI uiText;
    public TextMeshProUGUI uiTitle;
    public List<List<string>> choices = new List<List<string>>();
    public TextAsset dialogueJSON;

    public bool interacted;

    private bool isUIVisible = false;

    public PlayerController playerController;

    private int indexDialogue = 0;

    public GameObject choicePanel; // Painel para mostrar as opções de resposta
    public Button[] choiceButtons; // Botões para mostrar as escolhas

    private Dialogue dialogueData;

    public Catraca catracaScript;

    private AudioSource audioSource; // Para reproduzir os áudios de diálogo

    public GameObject[] objectsToAnimate;

    void Awake()
    {
        if (InteractionType == InteractableType.NPC)
        {
            myActor = GetComponent<Actor>();
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Start()
    {
        if (InteractionType == InteractableType.NPC)
        {
            // Inicialmente, desativa a UI
            uiPanel.SetActive(false);
            choicePanel.SetActive(false);
            isUIVisible = false; // Inicialmente, a UI não está visível
          
            if (dialogueJSON != null)
            {
                string dataAsJson = dialogueJSON.text;
                dialogueData = JsonUtility.FromJson<Dialogue>(dataAsJson);

                if (dialogueData.choices != null)
                {
                    foreach (var choice in dialogueData.choices)
                    {
                        choices.Add(choice.choices);
                    }
                }
                else
                {
                    Debug.LogError("Choices veio como nulo");
                }
            }
            else
            {
                Debug.LogError("Arquivo JSON não atribuído no Inspector.");
            }
            continueButton.onClick.AddListener(OnContinueButtonClick);
        }
    }

    void OnContinueButtonClick()
    {
        if (indexDialogue < dialogueData.dialogueTextOptions.Count && !interacted)
        {
            ShowNextDialogue();
        }
        else
        {
            // Se o diálogo terminou, ocultar a UI
            uiPanel.SetActive(false);
            choicePanel.SetActive(false);
            playerController.ToggleUI(false);
            isUIVisible = false;
            interacted = true;

            // Rodar animações nos objetos
            PlayAnimations();
        }
    }

    public void Interact(PlayerController pc)
    {
        if (isUIVisible) return; // Evita reabrir a UI se já estiver visível

        if (!interacted) {
            playerController = pc;
            uiTitle.text = dialogueData.npcName;
            indexDialogue = 0;
            ShowNextDialogue();
            uiPanel.SetActive(true); // Ativa a UI principal
            isUIVisible = true;
        }
        else
        {
            uiTitle.text = dialogueData.npcName;
            uiText.text = dialogueData.interactedText.text;
            PlayAudio(dialogueData.interactedText.audioPath);
            uiPanel.SetActive(true); // Ativa a UI principal
            isUIVisible = true;
        }
    }

    void ShowNextDialogue()
    {

        // Mostrar texto do diálogo
        DialogueLine currentDialogue = dialogueData.dialogueTextOptions[indexDialogue];
        uiText.text = currentDialogue.text;
        PlayAudio(currentDialogue.audioPath);

        if (indexDialogue == 0 || indexDialogue > choices.Count)
        {
            choicePanel.SetActive(false); // Oculta as opções se não houver escolhas para essa fala
            continueButton.gameObject.SetActive(true);
        }
        else
        {
            ShowChoices(choices[indexDialogue - 1]);
            continueButton.gameObject.SetActive(false);
        }

        indexDialogue++;
    }

    void PlayAudio(string audioPath)
    {
        if (audioSource.isPlaying)
            audioSource.Stop();

        AudioClip clip = Resources.Load<AudioClip>(audioPath);
        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
        else
        {
            Debug.Log("Áudio não encontrado em " + audioPath);
        }
    }

    void ShowChoices(List<string> currentChoices)
    {
        // Ativar o painel de escolhas e garantir que há botões suficientes
        choicePanel.SetActive(true);

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < currentChoices.Count)
            {
                choiceButtons[i].gameObject.SetActive(true);
                choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = currentChoices[i];
                int choiceIndex = i; // Capturar índice local para o listener
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(choiceIndex));
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false); // Esconder botões que não são necessários
            }
        }
    }

    void OnChoiceSelected(int choiceIndex)
    {
        // Aqui você pode lidar com a escolha feita pelo jogador
        Debug.Log("Escolha selecionada: " + choices[indexDialogue - 2][choiceIndex]);

        // Continuar para o próximo diálogo após a escolha
        ShowNextDialogue();
    }

    private void PlayAnimations()
    {
        foreach (GameObject obj in objectsToAnimate)
        {
            Animator animator = obj.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("PlayAnimation"); // Altere "PlayAnimation" para o trigger que você configurou
            }
            else
            {
                Debug.LogWarning("O objeto " + obj.name + " não possui um Animator.");
            }
        }
    }

}
