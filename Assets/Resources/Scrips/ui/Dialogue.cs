using System.Collections.Generic;

[System.Serializable]
public class Dialogue
{
    public string npcName;
    public List<DialogueLine> dialogueTextOptions; // Lista de diálogos com texto e áudio
    public DialogueLine interactedText; // Texto e áudio da interação final
    public List<Choice> choices; // Lista de escolhas
}

[System.Serializable]
public class DialogueLine
{
    public int id; // Identificador do diálogo
    public string text; // Texto do diálogo
    public string audioPath; // Caminho para o arquivo de áudio
}

[System.Serializable]
public class Choice
{
    public List<string> choices; // Lista de opções de escolha
}