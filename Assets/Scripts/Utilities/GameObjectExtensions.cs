using UnityEngine;

namespace Utilities
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Gets, or adds if doesn't contain the component yet
        /// </summary>
        /// <param name="gameObject">GameObject instance</param>
        /// <typeparam name="T">Component type</typeparam>
        /// <returns></returns>
        public static T GetOrAdd<T>(this GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if(!component) component = gameObject.AddComponent<T>();
            return component;
        }
        
        /// <summary>
        /// Checks if game object contains component
        /// </summary>
        /// <param name="gameObject">GameObject instance</param>
        /// <typeparam name="T">Component type</typeparam>
        /// <returns>If component exists</returns>
        public static bool Has<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.GetComponent<T>() != null;
        }
    }
}