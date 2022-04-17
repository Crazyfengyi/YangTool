// Version 1.6.2
// ©2016 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System.Collections.Generic;
using UnityEngine;

namespace Exploder
{
    class PostprocessExplode : Postprocess
    {
        public PostprocessExplode(Core Core) : base(Core)
        {
        }

        public override TaskType Type { get { return TaskType.PostprocessExplode; } }

        public override void Init()
        {
            base.Init();

            if (!core.splitMeshIslands)
            {
                core.postList = new List<MeshObject>(core.meshSet);
            }

            var fragmentsNum = core.postList.Count;

            if (fragmentsNum == 0)
            {
                return;
            }

            FragmentPool.Instance.Allocate(fragmentsNum, core.parameters.MeshColliders, core.parameters.Use2DCollision, core.parameters.FragmentPrefab);
            FragmentPool.Instance.SetDeactivateOptions(core.parameters.DeactivateOptions, core.parameters.FadeoutOptions, core.parameters.DeactivateTimeout);
            FragmentPool.Instance.SetExplodableFragments(core.parameters.ExplodeFragments, core.parameters.DontUseTag);
            FragmentPool.Instance.SetFragmentPhysicsOptions(core.parameters.FragmentOptions, core.parameters.Use2DCollision);
            FragmentPool.Instance.SetSFXOptions(core.parameters.SFXOptions);

            core.pool = FragmentPool.Instance.GetAvailableFragments(fragmentsNum);

            if (core.parameters.Callback != null)
            {
                core.parameters.Callback(Watch.ElapsedMilliseconds, ExploderObject.ExplosionState.ExplosionStarted);
            }
        }

        public override bool Run(float frameBudget)
        {
            var count = core.pool.Count;

            while (core.poolIdx < count)
            {
                var fragment = core.pool[core.poolIdx];
                var mesh = core.postList[core.poolIdx];

                core.poolIdx++;

                if (!mesh.original)
                {
                    continue;
                }

                var unityMesh = mesh.mesh.ToUnityMesh();

                fragment.meshFilter.sharedMesh = unityMesh;

                // choose proper material

                if (mesh.option && mesh.option.FragmentMaterial)
                {
                    fragment.meshRenderer.sharedMaterial = mesh.option.FragmentMaterial;
                }
                else
                {
                    if (core.parameters.FragmentOptions.FragmentMaterial != null)
                    {
                        fragment.meshRenderer.sharedMaterial = core.parameters.FragmentOptions.FragmentMaterial;
                    }
                    else
                    {
                        fragment.meshRenderer.sharedMaterial = mesh.material;
                    }
                }

                unityMesh.RecalculateBounds();

                var oldParent = fragment.transform.parent;
                fragment.transform.parent = mesh.parent;
                fragment.transform.position = mesh.position;
                fragment.transform.rotation = mesh.rotation;
                fragment.transform.localScale = mesh.localScale;
                fragment.transform.parent = null;
                fragment.transform.parent = oldParent;

                if (core.parameters.PartialExplosion)
                {
                    
                }
                else
                {
                    if (mesh.original != core.parameters.ExploderGameObject)
                    {
                        ExploderUtils.SetActiveRecursively(mesh.original, false);
                    }
                    else
                    {
                        ExploderUtils.EnableCollider(mesh.original, false);
                        ExploderUtils.SetVisible(mesh.original, false);
                    }

                    if (mesh.skinnedOriginal && mesh.skinnedOriginal != core.parameters.ExploderGameObject)
                    {
                        ExploderUtils.SetActiveRecursively(mesh.skinnedOriginal, false);
                    }
                    else
                    {
                        ExploderUtils.EnableCollider(mesh.skinnedOriginal, false);
                        ExploderUtils.SetVisible(mesh.skinnedOriginal, false);
                    }

                    if (mesh.skinnedOriginal && mesh.bakeObject)
                    {
                        GameObject.DestroyObject(mesh.bakeObject, 1);
                    }
                }

                var plane = mesh.option && mesh.option.Plane2D;

                var use2d = core.parameters.Use2DCollision;

                if (!core.parameters.FragmentOptions.DisableColliders)
                {
                    if (core.parameters.MeshColliders && !use2d)
                    {
                        // dont use mesh colliders for 2d plane
                        if (!plane)
                        {
                            fragment.meshCollider.sharedMesh = unityMesh;
                        }
                    }
                    else
                    {

                        if (core.parameters.Use2DCollision)
                        {
                            MeshUtils.GeneratePolygonCollider(fragment.polygonCollider2D, unityMesh);
                        }
                        else
                        {
                            fragment.boxCollider.center = unityMesh.bounds.center;
                            fragment.boxCollider.size = unityMesh.bounds.extents;
                        }
                    }
                }

                if (mesh.option)
                {
                    mesh.option.DuplicateSettings(fragment.options);
                }

                fragment.Explode();

                var force = core.parameters.Force;
                if (mesh.option && mesh.option.UseLocalForce)
                {
                    force = mesh.option.Force;
                }

                // apply force to rigid body
                fragment.ApplyExplosion(mesh.transform, mesh.mesh.centroid, core.parameters.Position, core.parameters.FragmentOptions, core.parameters.UseForceVector,
                                        core.parameters.ForceVector, force, mesh.original, core.parameters.TargetFragments);

#if SHOW_DEBUG_LINES
            UnityEngine.Debug.DrawLine(settings.Position, forceVector * settings.Force, Color.yellow, 3);
#endif

                if (Watch.ElapsedMilliseconds > frameBudget)
                {
                    return false;
                }
            }

            if (core.parameters.DestroyOriginalObject)
            {
                foreach (var mesh in core.postList)
                {
                    if (mesh.original && !mesh.original.GetComponent<Fragment>())
                    {
                        Object.Destroy(mesh.original);
                    }

                    if (mesh.skinnedOriginal)
                    {
                        Object.Destroy(mesh.skinnedOriginal);
                    }
                }
            }

            if (core.parameters.ExplodeSelf)
            {
                if (!core.parameters.DestroyOriginalObject)
                {
                    ExploderUtils.SetActiveRecursively(core.parameters.ExploderGameObject, false);
                }
            }

            if (core.parameters.HideSelf)
            {
                ExploderUtils.SetActiveRecursively(core.parameters.ExploderGameObject, false);
            }

#if DBG
        ExploderUtils.Log("Explosion finished! " + postList.Count + postList[0].original.transform.gameObject.name);
#endif
//            core.exploder.OnExplosionFinished(true);

            Watch.Stop();

            return true;
        }
    }
}
