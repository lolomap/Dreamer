using UnityEngine;
using Zenject;

namespace DI
{
    public static class DIGlobal
    {
        public static GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            return Object.FindAnyObjectByType<SceneContext>().Container.InstantiatePrefab(prefab, position, rotation, parent);
        }
        public static GameObject Instantiate(GameObject prefab, Transform parent)
        {
            return Object.FindAnyObjectByType<SceneContext>().Container.InstantiatePrefab(prefab, parent);
        }
    }
}
