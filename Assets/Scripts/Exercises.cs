using System.Linq;
using Data;
using DG.Tweening;
using Events;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Zenject;

public class Exercises : MonoBehaviour
{
    public VisualTreeAsset ButtonPrefab;
    public ExercisesDB ExercisesDB;
    
    private ExerciseSO _exercise;
    private VisualElement _root;
    private VisualElement _answersList;
    private TextField _answerInput;
    private Label _answerPlaceholder;

    private int _correctAnswers;
    private int _totalAnswers;
    private bool _failed;

    [Inject] private EventBus _eventBus;
    
    private void Awake()
    {
        if (ExercisesDB.Exercises == null)
        {
            Debug.LogError("No topic is selected");
            return;
        }
        if (ExercisesDB.Exercises.Count < 1)
        {
            Debug.LogError("Topic has no exercises");
            return;
        }

        _root = GetComponent<UIDocument>().rootVisualElement;
        _answersList = _root.Q("exercise-answers-list");
        _answerPlaceholder = _root.Q<Label>("exercise-answer-placeholder");
        
        RestartExercises();
    }

    private void LoadAnswers()
    {
        _answersList.Clear();

        foreach (
            VisualElement answerBtn in _exercise.Answers.Select(answer =>
                {
                    VisualElement element = ButtonPrefab.Instantiate();
                    Button button = element.Q<Button>();
                    button.clicked += () => { OnAnswer(element, answer); };
                    button.text = answer;
                    return element;
                }
            )
        )
        {
            _answersList.Add(answerBtn);
        }
    }

    private void OnAnswer(VisualElement target, string answer)
    {
        if (answer == _exercise.CorrectAnswer)
        {
            target.AddToClassList("good-color");
            _answerPlaceholder.text = answer;
            
            if (!_failed)
                _correctAnswers++;
            _totalAnswers++;

            DOTween.Sequence()
                .AppendCallback(() => { _eventBus.BubbleShow.RaiseEvent(ExercisesDB.Bubbles.Success.Random()); })
                .AppendInterval(2f)
                .AppendCallback(() =>
                {
                    if (_correctAnswers >= 3 || _totalAnswers >= 5)
                    {
                        OnFinishExercises();
                    }
                    else
                    {
                        RestartExercises();
                    }
                })
                .Play();
        }
        else
        {
            _failed = true;
            _eventBus.BubbleShow.RaiseEvent(ExercisesDB.Bubbles.Failed.Random());
            target.AddToClassList("bad-color");
        }
    }

    private void OnFinishExercises()
    {
        SceneManager.LoadScene("Topics");
    }

    private void RestartExercises()
    {
        _answerPlaceholder.text = "_______";
        _failed = false;
        _exercise = ExercisesDB.Exercises.Random();
        ExercisesDB.Exercises.Remove(_exercise);
        //_root.Bind(new(_exercise));
        _root.Q<Label>("exercise-text").text = _exercise.Text;
        LoadAnswers();
    }
}