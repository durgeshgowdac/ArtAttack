using System.Collections;
using TMPro;
using UnityEngine;

public class GameOverTextAnimator : MonoBehaviour
{
    [TextArea(2, 5)]
    public string[] gameOverMessages;

    public TextMeshProUGUI messageText;
    public float typingSpeed = 0.05f;

    private Coroutine typingCoroutine;

    void OnEnable()
    {
        if (gameOverMessages.Length == 0 || messageText == null)
        {
            Debug.LogWarning("Missing messages or TextMeshPro reference.");
            return;
        }

        string chosenMessage = gameOverMessages[Random.Range(0, gameOverMessages.Length)];

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeMessage(chosenMessage));
    }

    IEnumerator TypeMessage(string fullText)
    {
        messageText.text = "";
        foreach (char c in fullText)
        {
            messageText.text += c;
            if (c != ' ' && c != '\n')
                yield return new WaitForSeconds(typingSpeed);
            else
                yield return null;
        }
    }
}