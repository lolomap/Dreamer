using System;
using UnityEngine;
using UnityEngine.UIElements;

public class Topics : MonoBehaviour
{
    private VisualElement _root;
    
    private void OnEnable()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        
        foreach (VisualElement topicCard in _root.Q<VisualElement>("select-topic").Children())
        {
            topicCard.Q<Button>().RegisterCallback<ClickEvent>(OnTopicSelected);
        }
    }

    private static void OnTopicSelected(ClickEvent @event)
    {
        VisualElement card = (@event.target as Button)?.parent;
        if (card == null) return;

        GameContext.TopicName = card.name;
    }
}