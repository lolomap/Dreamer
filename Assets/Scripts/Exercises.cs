using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class Exercises : MonoBehaviour
{
    public ExercisesDB ExercisesDB;
    
    private ExerciseSO _exercise;
    private VisualElement _root;
    private VisualElement _answersList;
    private TextField _answerInput;
    
    private void Awake()
    {
        ExercisesDB.ExercisesByTopic.TryGetValue(GameContext.TopicName ?? ExercisesDB.DefaultTopic, out List<ExerciseSO> topicExercises);
        if (topicExercises == null)
        {
            Debug.LogError("No topic is selected");
            return;
        }
        if (topicExercises.Count < 1)
        {
            Debug.LogError("Topic has no exercises");
            return;
        }
        
        _exercise = topicExercises.Random();

        _root = GetComponent<UIDocument>().rootVisualElement;
        _root.Bind(new(_exercise));
        _answersList = _root.Q("exercise-answers-list");
        _answerInput = _root.Q<TextField>("exercise-answer-input");

        if (_exercise.HasAnswers)
        {
            _answersList.RemoveFromClassList("hidden");
            _answerInput.AddToClassList("hidden");
            LoadAnswers();
        }
        else
        {
            _answerInput.RemoveFromClassList("hidden");
            _answersList.AddToClassList("hidden");
        }

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
            Button answerBtn in _exercise.Answers.Select(answer =>
                new Button(() => {OnAnswer(answer);}) { text = answer }
            )
        )
        {
            _answersList.Add(answerBtn);
        }
    }

    private void OnAnswer(string answer)
    {
        Debug.Log(answer);
    }
}