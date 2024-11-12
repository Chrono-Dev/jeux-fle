using System.Collections.Generic;

[System.Serializable]
public class Dialogue
{
    public string npcName;
    public List<DialogueLine> dialogueTextOptions; // Lista de di�logos com texto e �udio
    public DialogueLine interactedText; // Texto e �udio da intera��o final
    public List<Choice> choices; // Lista de escolhas
}

[System.Serializable]
public class DialogueLine
{
    public int id; // Identificador do di�logo
    public string text; // Texto do di�logo
    public string audioPath; // Caminho para o arquivo de �udio
}

[System.Serializable]
public class Choice
{
    public List<string> choices; // Lista de op��es de escolha
}