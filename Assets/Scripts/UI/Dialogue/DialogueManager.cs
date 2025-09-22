using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public GameObject dialoguePanel;

    public float typingSpeed = 0.05f;

    private Queue<string> sentences;
    private bool isTyping = false;
    private string currentFullSentence; // Сохраняем полное предложение
    private string currentDisplayedText; // Текущий отображаемый текст

    void Start()
    {
        sentences = new Queue<string>();
        dialoguePanel.SetActive(false);
    }

    public void StartDialogue(string[] lines)
    {
        sentences.Clear();
        dialoguePanel.SetActive(true);
        Time.timeScale = 0;
        foreach (string line in lines)
        {
            sentences.Enqueue(line);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (isTyping)
        {
            // Если текст ещё печатается, досрочно завершить и показать весь текст
            StopAllCoroutines();
            dialogueText.text = currentFullSentence;
            isTyping = false;
            return;
        }

        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentFullSentence = sentences.Dequeue();
        StartCoroutine(TypeSentence(currentFullSentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";
        currentDisplayedText = "";
        
        foreach (char letter in sentence.ToCharArray())
        {
            currentDisplayedText += letter;
            dialogueText.text = currentDisplayedText;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }
        isTyping = false;
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        Time.timeScale = 1;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            DisplayNextSentence();
        }
    }
}