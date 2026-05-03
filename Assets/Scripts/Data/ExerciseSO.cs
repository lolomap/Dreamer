using System.Collections.Generic;
using UnityEngine;
namespace Data
{
    [CreateAssetMenu(fileName = "Exercise", menuName = "Tasks/Exercise", order = -1000)]
    public class ExerciseSO : ScriptableObject
    {
        public string Text;
        public bool HasAnswers; 
        public List<string> Answers;
        public string CorrectAnswer;
    }
}
