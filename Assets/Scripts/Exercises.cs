using System.Collections.Generic;
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
        
        _exercise = ExercisesDB.Exercises.Random();

        _root = GetComponent<UIDocument>().rootVisualElement;
        _root.Bind(new(_exercise));
        _answersList = _root.Q("exercise-answers-list");
        
        LoadAnswers();

        float score = Poetry.Instance.ScoreRhyme("Попросил подвезти меня дружок", "Дал я гари, что покраснел движок");
        Debug.LogWarning(score);
        
        score = Poetry.Instance.ScoreRhyme("Всё погнулось колесо", "Метров пять я носом пропахал песок");
        Debug.LogWarning(score);
        
        score = Poetry.Instance.ScoreRhyme("велит", "инвалид");
        Debug.LogWarning(score);
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
            Debug.Log("Correct");       
            target.AddToClassList("good-color");
        }
        else
        {
            Debug.Log("Wrong!");
            target.AddToClassList("bad-color");
        }
    }
}