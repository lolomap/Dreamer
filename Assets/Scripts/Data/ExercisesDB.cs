using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data
{
    [CreateAssetMenu(fileName = "ExercisesDB", menuName = "Tasks/ExercisesDB", order = -1000)]
    public class ExercisesDB : ScriptableObject
    {
        public List<ExerciseSO> Exercises;
        public ExerciseBubblesSO Bubbles;
    }
}
