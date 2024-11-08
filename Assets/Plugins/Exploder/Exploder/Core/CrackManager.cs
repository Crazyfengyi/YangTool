﻿using System.Collections.Generic;
using UnityEngine;

namespace Exploder
{
    class CrackManager
    {
        private readonly Dictionary<GameObject, CrackedObject> crackedObjects;

        public CrackManager()
        {
            crackedObjects = new Dictionary<GameObject, CrackedObject>();
        }

        public CrackedObject Create(GameObject originalObject, ExploderParams parameters)
        {
            Debug.Assert(!crackedObjects.ContainsKey(originalObject), "GameObject already cracked!");

            var crackedObject = new CrackedObject(originalObject, parameters);
            crackedObjects[originalObject] = crackedObject;

            return crackedObject;
        }

        public long Explode(GameObject gameObject)
        {
            if (crackedObjects.ContainsKey(gameObject))
            {
                long ellapsedMS = 0;

                CrackedObject obj;
                if (crackedObjects.TryGetValue(gameObject, out obj))
                {
                    ellapsedMS = obj.Explode();
                    crackedObjects.Remove(gameObject);
                }

                return ellapsedMS;
            }

            UnityEngine.Debug.LogErrorFormat("GameObject {0} not cracked, Call CrackObject first!", gameObject.name);
            return 0;
        }

        public long ExplodeAll()
        {
            long ellapsedMS = 0;

            foreach (var crackedObject in crackedObjects.Values)
            {
                ellapsedMS += crackedObject.Explode();
            }

            crackedObjects.Clear();

            return ellapsedMS;
        }
    }
}
