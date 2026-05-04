using System.Collections.Generic;
using UnityEngine;
namespace Data
{
    [CreateAssetMenu(fileName = "TopicsDB", menuName = "Tasks/TopicsDB", order = -1000)]
    public class TopicsDB : ScriptableObject
    {
        public List<TopicSO> Topics;
    }
}
