using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "ExercisesDB", menuName = "Tasks/ExercisesDB", order = -1000)]
    public class ExercisesDB : ScriptableObject
    {
        public string DefaultTopic;
        public SerializedDictionary<string, List<ExerciseSO>> ExercisesByTopic;
    }
}
