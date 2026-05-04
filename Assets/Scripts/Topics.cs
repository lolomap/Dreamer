using System;
using Events;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Zenject;

public class Topics : MonoBehaviour
{
    private VisualElement _root;

    [Inject] private EventBus _eventBus;
    
    private void OnEnable()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        
        foreach (VisualElement topicCard in _root.Q<VisualElement>("select-topic").Children())
        {
            topicCard.Q<Button>().RegisterCallback<ClickEvent>(OnTopicSelected);
        }
    }

    private void Start()
    {
        _eventBus.BubbleShow.RaiseEvent("Теперь выбери тему для творческого задания!");
    }

    private static void OnTopicSelected(ClickEvent @event)
    {
        VisualElement card = (@event.target as Button)?.parent.parent;
        if (card == null) return;

        GameContext.TopicName = card.name;
        SceneManager.LoadScene("Poetry");
    }
}