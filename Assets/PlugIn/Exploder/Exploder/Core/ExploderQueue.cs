// Version 1.6.2
// ©2016 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System.Collections.Generic;
using UnityEngine;

namespace Exploder
{
    class ExploderParams
    {
        public Vector3 Position;
        public Vector3 ForceVector;
        public Vector3 CubeRadius;
        public Vector3 HitPosition;
        public Vector3 ShotDir;

        public float Force;
        public float FrameBudget;
        public float Radius;
        public float DeactivateTimeout;
        public float BulletSize;

        public int id;
        public int TargetFragments;
        public int FragmentPoolSize;

        public DeactivateOptions DeactivateOptions;
        public ExploderObject.ThreadOptions ThreadOptions;
        public FadeoutOptions FadeoutOptions;

        public ExploderObject.OnExplosion Callback;
        public ExploderObject.FragmentOption FragmentOptions;
        public ExploderObject.SFXOption SFXOptions;

        public GameObject Target;
        public GameObject ExploderGameObject;
        public GameObject FragmentPrefab;

        public bool UseCubeRadius;
        public bool DontUseTag;
        public bool UseForceVector;
        public bool MeshColliders;
        public bool ExplodeSelf;
        public bool HideSelf;
        public bool DestroyOriginalObject;
        public bool ExplodeFragments;
        public bool SplitMeshIslands;
        public bool Use2DCollision;
        public bool DisableRadiusScan;
        public bool UniformFragmentDistribution;
        public bool DisableTriangulation;
        public bool PartialExplosion;
        public bool Crack;
        public bool processing;

        public ExploderParams(ExploderObject exploder)
        {
            Position = ExploderUtils.GetCentroid(exploder.gameObject);
            DontUseTag = exploder.DontUseTag;
            Radius = exploder.Radius;
            UseCubeRadius = exploder.UseCubeRadius;
            CubeRadius = exploder.CubeRadius;
            ForceVector = exploder.ForceVector;
            UseForceVector = exploder.UseForceVector;
            Force = exploder.Force;
            FrameBudget = exploder.FrameBudget;
            TargetFragments = exploder.TargetFragments;
            DeactivateOptions = exploder.DeactivateOptions;
            DeactivateTimeout = exploder.DeactivateTimeout;
            MeshColliders = exploder.MeshColliders;
            ExplodeSelf = exploder.ExplodeSelf;
            HideSelf = exploder.HideSelf;
            ThreadOptions = exploder.ThreadOption;
            DestroyOriginalObject = exploder.DestroyOriginalObject;
            ExplodeFragments = exploder.ExplodeFragments;
            SplitMeshIslands = exploder.SplitMeshIslands;
            FragmentOptions = exploder.FragmentOptions.Clone();
            SFXOptions = exploder.SFXOptions.Clone();
            Use2DCollision = exploder.Use2DCollision;
            FragmentPoolSize = exploder.FragmentPoolSize;
            FragmentPrefab = exploder.FragmentPrefab;
            FadeoutOptions = exploder.FadeoutOptions;
            DisableRadiusScan = exploder.DisableRadiusScan;
            UniformFragmentDistribution = exploder.UniformFragmentDistribution;
            DisableTriangulation = exploder.DisableTriangulation;
            ExploderGameObject = exploder.gameObject;
        }
    }

    class ExploderQueue
    {
        private readonly Queue<ExploderParams> queue;
        private readonly Core core;

        public ExploderQueue(Core core)
        {
            this.core = core;
            queue = new Queue<ExploderParams>();
        }

        public void Enqueue(ExploderObject exploderObject, ExploderObject.OnExplosion callback, GameObject target, bool crack)
        {
            var settings = new ExploderParams(exploderObject)
            {
                Callback = callback,
                Target = target,
                Crack = crack,
                processing = false
            };

            queue.Enqueue(settings);
            ProcessQueue();
        }

        public void EnqueuePartialExplosion(ExploderObject exploderObject, ExploderObject.OnExplosion callback,
                                            GameObject target, Vector3 shotDir, Vector3 hitPosition, float bulletSize)
        {
            var settings = new ExploderParams(exploderObject)
            {
                Callback = callback,
                Target = target,
                HitPosition = hitPosition,
                BulletSize = bulletSize,
                PartialExplosion = true,
                ShotDir = shotDir,
                processing = false,
            };

            queue.Enqueue(settings);
            ProcessQueue();
        }

        void ProcessQueue()
        {
            if (queue.Count > 0)
            {
                var peek = queue.Peek();

                if (!peek.processing)
                {
                    peek.id = Random.Range(int.MinValue, int.MaxValue);
                    peek.processing = true;
                    core.StartExplosionFromQueue(peek);
                }
            }
        }

        public void OnExplosionFinished(int id, long ellapsedMS)
        {
            var explosion = queue.Dequeue();
            ExploderUtils.Assert(explosion.id == id, "Explosion id mismatch!");

            if (explosion.Callback != null)
            {
                explosion.Callback(ellapsedMS, explosion.Crack ? ExploderObject.ExplosionState.ObjectCracked : ExploderObject.ExplosionState.ExplosionFinished);
            }

            ProcessQueue();
        }
    }
}
