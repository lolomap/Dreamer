using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using DG.Tweening;
using Events;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Zenject;

public class Score : MonoBehaviour
{
    public bool IsDebug;
    public int DebugScore;
    
    private VisualElement _root;
    private Button _backBtn;
    private VisualElement _aiStars;
    private VisualElement _userStars;
    
    // Star rating state
    private List<Button> _userStarButtons;
    private int? _selectedStars = null;   // null means no rating chosen yet
    private int _hoveredStars = 0;        // 0 = nothing hovered

    [Inject] private EventBus _eventBus;

    private void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _aiStars = _root.Q("ai-stars");
        _userStars = _root.Q("user-stars");

        // Gather all Button elements inside the user-stars row (the stars)
        _userStarButtons = _userStars.Query<Button>().ToList();

        // Set up hover events on the whole star row to avoid flicker
        _userStars.RegisterCallback<PointerEnterEvent>(OnPointerEnterStarRow);
        _userStars.RegisterCallback<PointerLeaveEvent>(OnPointerLeaveStarRow);

        // Set up click event on each individual star button
        for (int i = 0; i < _userStarButtons.Count; i++)
        {
            int index = i; // capture for closure
            _userStarButtons[i].clicked += () => OnStarClicked(index);
        }

        // Back button – obtain the inner Button of the "back" template instance
        var backInstance = _root.Q<TemplateContainer>("back");
        if (backInstance != null)
        {
            _backBtn = backInstance.Q<Button>("btn");
            _backBtn.clicked += OnReady;
        }

        // Set initial star states (all empty)
        _selectedStars = null;
        _hoveredStars = 0;
        UpdateStarVisuals();

        int j = 0;
        if (IsDebug) GameContext.Score = DebugScore;
        foreach (VisualElement element in _aiStars.Children())
        {
            if (j >= GameContext.Score)
                return;
            element.RemoveFromClassList("star-no");
            element.AddToClassList("star");
            j++;
        }
    }

    private void OnDestroy()
    {
        if (_userStars != null)
        {
            _userStars.UnregisterCallback<PointerEnterEvent>(OnPointerEnterStarRow);
            _userStars.UnregisterCallback<PointerLeaveEvent>(OnPointerLeaveStarRow);
        }
    }

    private void Start()
    {
        _eventBus.BubbleShow.RaiseEvent(
            "Отличная работа, вот твой результат. Предложи родителям тоже поставить тебе свою оценку.");
    }

    private void OnReady()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // --- Star rating callbacks ---

    private void OnPointerEnterStarRow(PointerEnterEvent evt)
    {
        // Determine which star was entered (the target may be the star Button itself)
        if (evt.target is Button targetButton)
        {
            int idx = _userStarButtons.IndexOf(targetButton);
            if (idx >= 0)
            {
                // Set hover count = index + 1 (1‑based number of stars)
                _hoveredStars = idx + 1;
                UpdateStarVisuals();
            }
        }
    }

    private void OnPointerLeaveStarRow(PointerLeaveEvent evt)
    {
        // Reset hover only if no rating is selected
        if (_selectedStars == null)
        {
            _hoveredStars = 0;
            UpdateStarVisuals();
        }
    }

    private void OnStarClicked(int index)
    {
        // Select this star: 1‑based count
        _selectedStars = index + 1;
        // After a selection, hover effect is no longer shown
        _hoveredStars = 0;
        UpdateStarVisuals();
    }

    /// <summary>
    /// Updates the class of each star button based on the current hover/selection state.
    /// Stars up to the highlighted count get the "star" class, the rest get "star-no".
    /// </summary>
    private void UpdateStarVisuals()
    {
        int highlightCount = _selectedStars ?? _hoveredStars;

        for (int i = 0; i < _userStarButtons.Count; i++)
        {
            Button star = _userStarButtons[i];
            // Remove both possible state classes
            star.RemoveFromClassList("star");
            star.RemoveFromClassList("star-no");

            // Apply the correct class (1‑based index)
            if (i < highlightCount)
                star.AddToClassList("star");
            else
                star.AddToClassList("star-no");
        }
    }
}