using UnityEngine;

public class ShaderSwitcher : MonoBehaviour
{
    public Material newMaterial; // O novo shader que será aplicado
    public GameObject[] objectsToChange; // Lista de objetos para trocar o shader

    public Interactable NPC;

    // Função para trocar o shader
    public void ChangeMaterials()
    {
        if (newMaterial == null)
        {
            Debug.LogError("Novo material não atribuído.");
            return;
        }

        foreach (GameObject obj in objectsToChange)
        {
            if (obj != null)
            {
                // Obter todos os materiais do objeto
                Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    // Trocar o material de cada renderer
                    Material[] materials = renderer.materials;
                    for (int i = 0; i < materials.Length; i++)
                    {
                        materials[i] = newMaterial; // Altera o material
                    }
                    renderer.materials = materials; // Atribui os novos materiais
                }
            }
        }
    }

    private void Update()
    {
        if (NPC.interacted == true) // Por exemplo, ao pressionar a barra de espaço
        {
            ChangeMaterials();
        }
    }
}