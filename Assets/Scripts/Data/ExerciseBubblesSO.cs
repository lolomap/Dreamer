using System.Collections.Generic;
using UnityEngine;
namespace Data
{
    [CreateAssetMenu(fileName = "ExerciseBubbles", menuName = "Tasks/ExerciseBubbles", order = -1000)]
    public class ExerciseBubblesSO : ScriptableObject
    {
        public List<string> Failed;
        public List<string> Success;
    }
}
