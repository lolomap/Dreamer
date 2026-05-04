using System.Collections.Generic;
using UnityEngine;
namespace Data
{
    [CreateAssetMenu(fileName = "Topic", menuName = "Tasks/Topic", order = -1000)]
    public class TopicSO : ScriptableObject
    {
        public string Name;
        public List<string> Keywords = new();
    }
}
