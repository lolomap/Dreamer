using System.Linq;
using Data;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

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

        /*float score = Poetry.Instance.ScoreRhyme("Попросил подвезти меня дружок", "Дал я гари, что покраснел движок");
        Debug.LogWarning(score);
        
        score = Poetry.Instance.ScoreRhyme("Всё погнулось колесо", "Метров пять я носом пропахал песок");
        Debug.LogWarning(score);
        
        score = Poetry.Instance.ScoreRhyme("велит", "инвалид");
        Debug.LogWarning(score);*/
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
            
            // 
            
            if (_correctAnswers >= 3 || _totalAnswers >= 5)
            {
                OnFinishExercises();
            }
            else
            {
                RestartExercises();
            }
        }
        else
        {
            _failed = true;
            target.AddToClassList("bad-color");
        }
    }

    private void OnFinishExercises()
    {
        Debug.Log("Finish ex");
    }

    private void RestartExercises()
    {
        _answerPlaceholder.text = "_______";
        _failed = false;
        _exercise = ExercisesDB.Exercises.Random();
        ExercisesDB.Exercises.Remove(_exercise);
        _root.Bind(new(_exercise));
        LoadAnswers();
    }
}