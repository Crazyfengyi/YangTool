using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Exploder
{
    class CrackedObject
    {
        public List<Fragment> pool;

        private readonly Stopwatch watch;
        private readonly Quaternion initRot;
        private readonly Vector3 initPos;
        private readonly GameObject originalObject;
        private readonly ExploderParams parameters;

        public CrackedObject(GameObject originalObject, ExploderParams parameters)
        {
            this.originalObject = originalObject;
            this.parameters = parameters;

            this.initPos = originalObject.transform.position;
            this.initRot = originalObject.transform.rotation;

            watch = new Stopwatch();
        }

        public long Explode()
        {
            var count = pool.Count;
            var poolIdx = 0;

            if (count == 0)
                return 0;

            watch.Start();

            if (parameters.Callback != null)
            {
                parameters.Callback(0, ExploderObject.ExplosionState.ExplosionStarted);
            }

            var diffPos = Vector3.zero;
            var diffRot = Quaternion.identity;

            if (originalObject)
            {
                diffPos = originalObject.transform.position - initPos;
                diffRot = originalObject.transform.rotation * Quaternion.Inverse(initRot);
            }

            while (poolIdx < count)
            {
                var fragment = pool[poolIdx];

                poolIdx++;

                if (originalObject != parameters.ExploderGameObject)
                {
                    ExploderUtils.SetActiveRecursively(originalObject, false);
                }
                else
                {
                    ExploderUtils.EnableCollider(originalObject, false);
                    ExploderUtils.SetVisible(originalObject, false);
                }

                fragment.transform.position += diffPos;
                fragment.transform.rotation *= diffRot;

                fragment.Explode();
            }

            if (parameters.DestroyOriginalObject)
            {
                if (originalObject && !originalObject.GetComponent<Fragment>())
                {
                    GameObject.Destroy(originalObject);
                }
            }

            if (parameters.ExplodeSelf)
            {
                if (!parameters.DestroyOriginalObject)
                {
                    ExploderUtils.SetActiveRecursively(parameters.ExploderGameObject, false);
                }
            }

            if (parameters.HideSelf)
            {
                ExploderUtils.SetActiveRecursively(parameters.ExploderGameObject, false);
            }

            watch.Stop();
            return watch.ElapsedMilliseconds;
        }
    }
}
