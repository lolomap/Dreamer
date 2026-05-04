using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data;
using DG.Tweening;
using Events;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Zenject;

public class Poetry : MonoBehaviour
{
    public TopicsDB TopicsDB;
    public VisualTreeAsset TextFieldPrefab;
    public VisualTreeAsset KeywordPrefab;

    private VisualElement _root;
    private VisualElement _keywordsPreview;
    private VisualElement _keywordsHintPanel;
    private VisualElement _taskWritePanel;
    private VisualElement _inputsList;
    private VisualElement _loadingPanel;
    private Image _loadingSpinner;
    private Button _readyBtn;

    private string _poem;
    private TopicSO _topic;
    private KeywordChecker _keywordChecker;

    [Inject] private EventBus _eventBus;

    private void Awake()
    {
        _keywordChecker = GameContext.KeywordChecker;
        _root = GetComponent<UIDocument>().rootVisualElement;
        _inputsList = _root.Q("task-write");
        _readyBtn = _root.Q("task-ready").Q<Button>();
        _keywordsPreview = _root.Q("task-keywords-preview");
        _keywordsHintPanel = _root.Q("keywords-panel");
        _taskWritePanel = _root.Q("task-write");
        _loadingPanel = _root.Q("loading");
        _loadingSpinner = _loadingPanel.Q<Image>();

        _readyBtn.text = "Написал и прочитал";
        _readyBtn.clicked += OnOffscreen;
        _keywordsHintPanel.AddToClassList("hidden");
        _keywordsPreview.RemoveFromClassList("hidden");
        _taskWritePanel.AddToClassList("hidden");
        _loadingPanel.AddToClassList("hidden");

        var scheduledAnimation = _loadingSpinner.schedule.Execute(UpdateRotation).Every(10);
        
        LoadKeywords();
        AddTextField();
    }

    private void UpdateRotation()
    {
        if (_loadingSpinner == null) return;

        // Вычисляем новый угол
        float newAngle = (_loadingSpinner.resolvedStyle.rotate.angle.ToDegrees() + 200 * 0.01f) % 360f;
        // Применяем поворот
        _loadingSpinner.style.rotate = new Rotate(new Angle(newAngle, AngleUnit.Degree));
    }

    private void Start()
    {
        _eventBus.BubbleShow.RaiseEvent("Возьми тетрадку и ручку и напиши стихотворение, используя слова из списка!\nКогда закончишь, прочитай-ка своё творение родителям.");
    }

    private void LoadKeywords()
    {
        GameContext.TopicName ??= TopicsDB.DefaultTopic;

        _topic = TopicsDB.Topics.Find(topic => topic.Name == GameContext.TopicName);
        if (_topic == null)
        {
            Debug.LogError("No topic with selected name found");
            return;
        }

        foreach (string keyword in _topic.Keywords)
        {
            VisualElement element = KeywordPrefab.Instantiate();
            VisualElement element2 = KeywordPrefab.Instantiate();
            element.Q<Label>().text = keyword;
            element2.Q<Label>().text = keyword;
            _keywordsPreview.Add(element);
            _keywordsHintPanel.Add(element2);
        }
    }

    private void OnOffscreen()
    {
        _eventBus.BubbleShow.RaiseEvent("Ну что же, фантазёр, сейчас посмотрим что ты там сочинил! Введи свое стихотворение сюда или попроси родителей.");

        _keywordsPreview.AddToClassList("hidden");
        _taskWritePanel.RemoveFromClassList("hidden");
        _keywordsHintPanel.RemoveFromClassList("hidden");
        
        _readyBtn.text = "Готово";
        _readyBtn.clicked -= OnOffscreen;
        _readyBtn.clicked += OnReady;
    }
    
    private async void OnReady()
    {
        // Отключаем кнопку, чтобы избежать повторных нажатий
        _readyBtn.SetEnabled(false);

        _poem = "";
        foreach (VisualElement visualElement in _inputsList.Children().Where(element => element.GetClasses().Contains("text-field")))
        {
            TextField field = visualElement.Q<TextField>();
            _poem += field.text + "\n";
        }

        bool hasTopic = _keywordChecker.ContainsKeywords(_poem, _topic.Keywords, 3);
        if (!hasTopic)
        {
            _eventBus.BubbleShow.RaiseEvent("Попробуй добавить больше слов по теме.");
            _readyBtn.SetEnabled(true);
            return;
        }

        // Показываем загрузку
        _keywordsPreview.AddToClassList("hidden");
        _taskWritePanel.AddToClassList("hidden");
        _loadingPanel.RemoveFromClassList("hidden");

        try
        {
            // Асинхронная оценка стихотворения
            GameContext.Score = await Task.Run(() => PoetryLogic.Instance.ScorePoem(_poem));

            // Загружаем сцену с результатами (замените "ResultScene" на актуальное имя)
            SceneManager.LoadScene("ResultScene");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Ошибка при оценке стихотворения: {ex.Message}");
        }
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