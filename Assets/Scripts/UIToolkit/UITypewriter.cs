using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class UITypewriter : MonoBehaviour
{
    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private float _delay = 0.05f;

    // Для работы через события EventBus замените вызов на публичный метод,
    // который принимает строку и запускает корутину.
    public void ShowText(string fullText, Label label)
    {
        StartCoroutine(WriteText(fullText, label));
    }

    private IEnumerator WriteText(string fullText, Label label)
    {
        for (int i = 0; i < fullText.Length; i++)
        {
            string visiblePart = fullText.Substring(0, i + 1);
            string invisiblePart = fullText.Substring(i + 1);
            label.text = visiblePart + "<alpha=#00>" + invisiblePart;
            yield return new WaitForSeconds(_delay);
        }
        // Убираем возможный сбой в конце
        label.text = fullText;
    }
}
