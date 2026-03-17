using ShitSystem.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShitSystem.Registerers
{
    public static class BehRegisterer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Fire()
        {
            SceneManager.sceneLoaded += Init;
        }

        private static void Init(Scene arg0, LoadSceneMode arg1)
        {
            foreach (var comp in Object.FindObjectsOfType<RegBehaviour>(true))
            {
                if (comp.gameObject.activeInHierarchy)
                    comp.OnInitialize();
                else
                {
                    if (comp.GetType().GetCustomAttributes(typeof(InitOnDisabled), false).Length > 0)
                        comp.OnInitialize();
                }
            }
        }
    }
}