using System.Collections;
using TMPro;
using UnityEngine;

public class TypewriterEffect : MonoBehaviour
{
    [TextArea(3, 10)]
    public string fullText;

    public float typingSpeed = 0.05f;

    private TextMeshProUGUI textComponent;
    private Coroutine typingCoroutine;

    void Start()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        StartTyping();
    }

    public void StartTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(RevealText());
    }

    IEnumerator RevealText()
    {
        textComponent.text = "";
        foreach (char c in fullText)
        {
            textComponent.text += c;
            if (c != ' ' && c != '\n')
                yield return new WaitForSeconds(typingSpeed);
            else
                yield return null;
        }
    }
}