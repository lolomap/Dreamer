using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class MainMenu : MonoBehaviour
{
    private VisualElement _root;
    
    private VisualElement _selectModePanel;
    
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

        _selectPoetryMode = _root.Q("poetry-mode").Q<Button>();
        _selectLyricsMode = _root.Q("lyrics-mode").Q<Button>();

        _selectPoetryMode.RegisterCallback<ClickEvent>(_ => OnPoetry());
        _selectLyricsMode.RegisterCallback<ClickEvent>(_ => OnLyrics());
    }

    private void OnPoetry()
    {
        GameContext.Mode = GameContext.GameMode.Poetry;
        SceneManager.LoadScene("Exercises");
    }

    private void OnLyrics()
    {
        GameContext.Mode = GameContext.GameMode.Lyrics;
        
    }

    private void OnModeSelected(VisualElement selected, VisualElement notSelected)
    {
        /*float slideDirection = selected.worldBound.x < notSelected.worldBound.x ? 1f : -1f;
        
        selected.experimental.animation
            .Start(new() {left = slideDirection * selected.resolvedStyle.width / 2f}, 1000)
            .Ease(Easing.InOutCubic);
        selected.experimental.animation
            .Start(Vector3.one, Vector3.one * 1.25f, 1000, (element, value) => { element.style.scale = value; })
            .Ease(Easing.InOutCubic);
        
        notSelected.experimental.animation
            .Start(Vector3.one, Vector3.zero, 1000, (element, value) => { element.style.scale = value; })
            .Ease(Easing.InOutCubic)
            .OnCompleted(ToTopicSelection);*/
    }
    
    private void ToTopicSelection()
    {
        /*DOTween.Sequence()
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
            .Play();*/
    }
}