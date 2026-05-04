using System.Collections.Generic;
using System.Linq;
using Events;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

public class Poetry : MonoBehaviour
{
    public VisualTreeAsset TextFieldPrefab;

    private VisualElement _root;
    private VisualElement _inputsList;
    private Button _readyBtn;

    private string _poem;

    [Inject] private EventBus _eventBus;

    private void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _inputsList = _root.Q("task-write");
        _readyBtn = _root.Q("task-ready").Q<Button>();

        _readyBtn.clicked += OnReady;

        AddTextField();
    }

    private void OnReady()
    {
        _poem = "";
        
        foreach (VisualElement visualElement in _inputsList.Children().Where(element => element.GetClasses().Contains("text-field")))
        {
            TextField field = visualElement.Q<TextField>();

            _poem += field.text + "\n";
        }

        PoetryLogic.Instance.ScorePoem(_poem);
    }

    /// <summary>
    /// Создаёт новый экземпляр поля ввода и добавляет его в контейнер.
    /// </summary>
    private VisualElement AddTextField()
    {
        VisualElement element = TextFieldPrefab.Instantiate();
        element.AddToClassList("text-field");

        TextField textField = element.Q<TextField>();
        if (textField == null)
        {
            Debug.LogError("TextField не найден внутри шаблона!");
            return null;
        }

        // Подписываемся на изменение текста, передавая сам element для контекста
        textField.RegisterValueChangedCallback(evt => OnTextFieldValueChanged(evt, element));

        _inputsList.Add(element);
        return element;
    }

    /// <summary>
    /// Обработчик изменения текста в любом поле.
    /// Добавляет новое пустое поле, если в последнем появляется текст.
    /// Удаляет последнее пустое поле, если предпоследнее становится пустым.
    /// </summary>
    private void OnTextFieldValueChanged(ChangeEvent<string> evt, VisualElement fieldElement)
    {
        int index = _inputsList.IndexOf(fieldElement);
        if (index < 0) return; // элемент уже удалён (защита)

        string currentText = evt.newValue;

        // Если это последний элемент и в нём есть текст → добавляем новое пустое поле
        if (index == _inputsList.childCount - 1)
        {
            if (!string.IsNullOrEmpty(currentText))
            {
                AddTextField(); // добавится в конец, и подписка будет у него
            }
        }
        // Если это предпоследний элемент и его текст стал пустым → возможно, нужно удалить последнее поле
        else if (index == _inputsList.childCount - 2)
        {
            if (string.IsNullOrEmpty(currentText))
            {
                // Проверяем, что последний элемент существует и его текст тоже пуст
                var lastElement = _inputsList.ElementAt(_inputsList.childCount - 1);
                var lastTextField = lastElement?.Q<TextField>();
                if (lastTextField != null && string.IsNullOrEmpty(lastTextField.value))
                {
                    // Удаляем последний пустой элемент
                    _inputsList.RemoveAt(_inputsList.childCount - 1);
                    // Отписка от событий не требуется — элемент уничтожится при удалении
                }
            }
        }
    }
}