using System;
using Data;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class MainMenu : MonoBehaviour
{
    private VisualElement _root;
    
    private VisualElement _selectModePanel;
    private VisualElement _selectTopicPanel;
    
    private Button _selectPoetryMode;
    private Button _selectLyricsMode;

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
        _selectModePanel.style.display = DisplayStyle.Flex;
        _selectTopicPanel.style.display = DisplayStyle.None;

        _selectPoetryMode = _root.Q("poetry-mode").Q<Button>();
        _selectLyricsMode = _root.Q("lyrics-mode").Q<Button>();

        _selectPoetryMode.RegisterCallback<ClickEvent>(_ => OnPoetry());
        _selectLyricsMode.RegisterCallback<ClickEvent>(_ => OnLyrics());

        foreach (VisualElement topicCard in _root.Q<VisualElement>("select-topic").Children())
        {
            topicCard.Q<Button>().RegisterCallback<ClickEvent>(OnTopicSelected);
        }
    }

    private void OnPoetry()
    {
        GameContext.Mode = GameContext.GameMode.Poetry;

        _selectLyricsMode.style.transformOrigin = new TransformOrigin(Length.Percent(50f), Length.Percent(100f));
        OnModeSelected(_selectPoetryMode, _selectLyricsMode);
    }

    private void OnLyrics()
    {
        GameContext.Mode = GameContext.GameMode.Lyrics;
        
        _selectPoetryMode.style.transformOrigin = new TransformOrigin(Length.Percent(50f), Length.Percent(0f));
        OnModeSelected(_selectLyricsMode, _selectPoetryMode);
    }

    private void OnModeSelected(VisualElement selected, VisualElement notSelected)
    {
        float slideDirection = selected.worldBound.y < notSelected.worldBound.y ? 1f : -1f;
        
        selected.experimental.animation
            .Start(new() {top = slideDirection * selected.resolvedStyle.width / 2f}, 1000)
            .Ease(Easing.InOutCubic);
        selected.experimental.animation
            .Start(Vector3.one, Vector3.one * 1.25f, 1000, (element, value) => { element.style.scale = value; })
            .Ease(Easing.InOutCubic);
        
        notSelected.experimental.animation
            .Start(Vector3.one, Vector3.zero, 1000, (element, value) => { element.style.scale = value; })
            .Ease(Easing.InOutCubic)
            .OnCompleted(ToTopicSelection);
    }
    
    private void ToTopicSelection()
    {
        DOTween.Sequence()
            .AppendInterval(0.75f)
            .AppendCallback(() =>
            {
                _root.experimental.animation
                    .Start(Vector3.one, Vector3.zero, 750, (element, value) => { element.style.scale = value; })
                    .Ease(Easing.InOutCubic)
                    .OnCompleted(() =>
                    {
                        _selectModePanel.style.display = DisplayStyle.None;
                        _selectTopicPanel.style.display = DisplayStyle.Flex;
                
                        _root.experimental.animation
                            .Start(Vector3.zero, Vector3.one, 750, (element, value) => { element.style.scale = value; })
                            .Ease(Easing.InOutCubic);
                    });
            })
            .Play();
    }

    private static void OnTopicSelected(ClickEvent @event)
    {
        VisualElement card = (@event.target as Button)?.parent;
        if (card == null) return;

        GameContext.TopicName = card.name;
    }
}