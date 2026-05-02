using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class MainMenu : MonoBehaviour
{
    private VisualElement _root;
    
    private VisualElement _selectModePanel;
    private VisualElement _selectTopicPanel;
    
    private Button _toggleMode;

    private void OnEnable()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        if (_root == null)
        {
            Debug.LogError("Missing UIDocument");
            return;
        }

        _selectModePanel = _root.Q<VisualElement>("select-mode");
        _selectTopicPanel = _root.Q<VisualElement>("select-topic");

        _toggleMode = _root.Q<Button>("toggle-mode");
        _toggleMode.RegisterCallback<ClickEvent>(_ => OnModeToggled());

        foreach (VisualElement topicCard in _root.Q<VisualElement>("select-topic").Children())
        {
            topicCard.Q<Button>().RegisterCallback<ClickEvent>(OnTopicSelected);
        }
    }

    private void OnModeToggled()
    {
        switch (GameContext.Mode)
        {
            case GameContext.GameMode.Poetry:
            {
                GameContext.Mode = GameContext.GameMode.Lyrics;
                _toggleMode.text = "Режим: Писатель";
                break;
            }

            case GameContext.GameMode.Lyrics:
            {
                GameContext.Mode = GameContext.GameMode.Poetry;
                _toggleMode.text = "Режим: Поэт";
                break;
            }
        }
    }

    private static void OnTopicSelected(ClickEvent @event)
    {
        VisualElement card = (@event.target as Button)?.parent;
        if (card == null) return;

        GameContext.TopicName = card.name;
        SceneManager.LoadScene("Exercises");
    }
}