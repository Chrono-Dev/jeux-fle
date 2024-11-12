using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIFadeInOut : MonoBehaviour
{
    public Image uiImage; // Refer�ncia para a imagem
    public TextMeshProUGUI uiText; // Refer�ncia para o texto
    public float fadeDuration = 2.0f; // Tempo que demora para desaparecer

    private Color originalImageColor;
    private Color originalTextColor;

    void Awake()
    {

        if (uiImage == null || uiText == null)
        {
            Debug.LogError("Os componentes Image ou TextMeshProUGUI n�o est�o atribu�dos corretamente.");
        }

        // Armazena as cores originais
        originalImageColor = uiImage.color;
        originalTextColor = uiText.color;

        // Inicialmente, esconder a imagem e o texto
        HideUI();
    }

    // Fun��o para definir o texto e come�ar a exibir a imagem e o texto
    public void ShowUI(string message)
    {
        uiText.text = message;

        // Torna a UI vis�vel antes do fade-in
        uiImage.gameObject.SetActive(true);
        uiText.gameObject.SetActive(true);

        // Inicia o processo de fade-in/fade-out
        StartCoroutine(FadeInAndOut());
    }

    // Esconder a UI (tornar completamente invis�vel)
    private void HideUI()
    {
        // Esconde a imagem e o texto, tornando o alpha 0
        uiImage.color = new Color(originalImageColor.r, originalImageColor.g, originalImageColor.b, 0);
        uiText.color = new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, 0);

        // Desativa os game objects
        uiImage.gameObject.SetActive(false);
        uiText.gameObject.SetActive(false);
    }

    private IEnumerator FadeInAndOut()
    {
        float elapsedTime = 0f;

        // Fade-in: aumenta o alpha para 1 (vis�vel)
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            // Calcula a transpar�ncia baseada no tempo
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);

            // Atualiza a cor da imagem e do texto
            uiImage.color = new Color(originalImageColor.r, originalImageColor.g, originalImageColor.b, alpha);
            uiText.color = new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, alpha);

            yield return null;
        }

        // Tempo extra com a UI vis�vel antes do fade-out
        yield return new WaitForSeconds(2f);

        elapsedTime = 0f;

        // Fade-out: diminui o alpha para 0 (invis�vel)
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            float alpha = Mathf.Clamp01(1 - (elapsedTime / fadeDuration));

            // Atualiza a cor da imagem e do texto
            uiImage.color = new Color(originalImageColor.r, originalImageColor.g, originalImageColor.b, alpha);
            uiText.color = new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, alpha);

            yield return null;
        }

        // Esconde a UI ap�s o fade-out
        HideUI();
    }
}
