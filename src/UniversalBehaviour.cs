using UnityEngine;

namespace UniverseLib
{
    /// <summary>
    /// Used for receiving Update events and starting Coroutines.
    /// </summary>
    internal class UniversalBehaviour : UnityEngine.MonoBehaviour
    {
        internal static UniversalBehaviour Instance { get; private set; }

        internal static void Setup()
        {

            GameObject obj = new("UniverseLibBehaviour");
            GameObject.DontDestroyOnLoad(obj);
            obj.hideFlags |= HideFlags.HideAndDontSave;
            Instance = obj.AddComponent<UniversalBehaviour>();
        }

        internal void Update()
        {
            Universe.Update();
        }
    }
}
