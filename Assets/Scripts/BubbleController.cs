using System;
using Events;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;
using Zenject;

[RequireComponent(typeof(UITypewriter))]
public class BubbleController : MonoBehaviour
{
    public bool StartClosed = true;
    
    private UITypewriter _typewriter;
    private VisualElement _bubble;
    private Label _bubbleLabel;

    [Inject] private EventBus _eventBus;
    
    private void OnEnable()
    {
        _typewriter = GetComponent<UITypewriter>();
        
        _bubble = GetComponent<UIDocument>().rootVisualElement.Q("character-bubble");
        _bubbleLabel = _bubble.Q<Label>();
        
        if (StartClosed) _bubble.AddToClassList("hidden");
        _bubbleLabel.text = "";

        _eventBus.BubblePopup.EventRaised += PopUp;
        _eventBus.BubbleShow.EventRaised += Show;
    }

    private void OnDisable()
    {
        _eventBus.BubblePopup.EventRaised -= PopUp;
        _eventBus.BubbleShow.EventRaised -= Show;
    }

    private void PopUp()
    {
        _bubble.RemoveFromClassList("hidden");
        _bubble.style.scale = new(0.0f);
        _bubble.experimental.animation.Scale(1.0f, 1000);
    }

    private void Show(string text)
    {
        _typewriter.ShowText(text, _bubbleLabel);
    }
}