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

    public GameObject uiPanel; // Refer�ncia ao painel da UI
    public Button continueButton;
    public TextMeshProUGUI uiText;
    public TextMeshProUGUI uiTitle;
    public List<List<string>> choices = new List<List<string>>();
    public TextAsset dialogueJSON;

    public bool interacted;

    private bool isUIVisible = false;

    public PlayerController playerController;

    private int indexDialogue = 0;

    public GameObject choicePanel; // Painel para mostrar as op��es de resposta
    public Button[] choiceButtons; // Bot�es para mostrar as escolhas

    private Dialogue dialogueData;

    public Catraca catracaScript;

    private AudioSource audioSource; // Para reproduzir os �udios de di�logo

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
            isUIVisible = false; // Inicialmente, a UI n�o est� vis�vel
          
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
                Debug.LogError("Arquivo JSON n�o atribu�do no Inspector.");
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
            // Se o di�logo terminou, ocultar a UI
            uiPanel.SetActive(false);
            choicePanel.SetActive(false);
            playerController.ToggleUI(false);
            isUIVisible = false;
            interacted = true;

            // Rodar anima��es nos objetos
            PlayAnimations();
        }
    }

    public void Interact(PlayerController pc)
    {
        if (isUIVisible) return; // Evita reabrir a UI se j� estiver vis�vel

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

        // Mostrar texto do di�logo
        DialogueLine currentDialogue = dialogueData.dialogueTextOptions[indexDialogue];
        uiText.text = currentDialogue.text;
        PlayAudio(currentDialogue.audioPath);

        if (indexDialogue == 0 || indexDialogue > choices.Count)
        {
            choicePanel.SetActive(false); // Oculta as op��es se n�o houver escolhas para essa fala
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
            Debug.Log("�udio n�o encontrado em " + audioPath);
        }
    }

    void ShowChoices(List<string> currentChoices)
    {
        // Ativar o painel de escolhas e garantir que h� bot�es suficientes
        choicePanel.SetActive(true);

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < currentChoices.Count)
            {
                choiceButtons[i].gameObject.SetActive(true);
                choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = currentChoices[i];
                int choiceIndex = i; // Capturar �ndice local para o listener
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(choiceIndex));
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false); // Esconder bot�es que n�o s�o necess�rios
            }
        }
    }

    void OnChoiceSelected(int choiceIndex)
    {
        // Aqui voc� pode lidar com a escolha feita pelo jogador
        Debug.Log("Escolha selecionada: " + choices[indexDialogue - 2][choiceIndex]);

        // Continuar para o pr�ximo di�logo ap�s a escolha
        ShowNextDialogue();
    }

    private void PlayAnimations()
    {
        foreach (GameObject obj in objectsToAnimate)
        {
            Animator animator = obj.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("PlayAnimation"); // Altere "PlayAnimation" para o trigger que voc� configurou
            }
            else
            {
                Debug.LogWarning("O objeto " + obj.name + " n�o possui um Animator.");
            }
        }
    }

}
