// Version 1.6.2
// ©2016 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using UnityEngine;

namespace Exploder.Utils
{
    /// <summary>
    /// utility class for easy accessing single exploder object in the scene
    /// assign this class to exploder game object
    /// </summary>
    public class ExploderSingleton : MonoBehaviour
    {
        /// <summary>
        /// instance of the exploder object
        /// </summary>
        public static ExploderObject ExploderInstance;

        void Awake()
        {
            ExploderInstance = gameObject.GetComponent<ExploderObject>();
        }
    }
}
