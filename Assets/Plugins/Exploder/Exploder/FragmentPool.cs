﻿// Version 1.6
// ©2016 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System.Collections.Generic;
using UnityEngine;

namespace Exploder
{
    /// <summary>
    /// fragment pool is a manager for fragments (create/recycle/...)
    /// </summary>
    public class FragmentPool : MonoBehaviour
    {
        /// <summary>
        /// instance
        /// </summary>
        public static FragmentPool Instance
        {
            get
            {
                if (instance == null)
                {
                    var fragmentRoot = new GameObject("FragmentRoot");
                    instance = fragmentRoot.AddComponent<FragmentPool>();
                }

                return instance;
            }
        }

        private static FragmentPool instance;
        private Fragment[] pool;
        private bool meshColliders;
        private float fragmentSoundTimeout;

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }

        private void OnDestroy()
        {
            DestroyFragments();
            instance = null;
        }

        /// <summary>
        /// gets the size of the pool
        /// </summary>
        public int PoolSize { get { return pool.Length; } }

        /// <summary>
        /// returns all pool
        /// </summary>
        public Fragment[] Pool { get { return pool; } }

        /// <summary>
        /// timeout between impact sound effects
        /// </summary>
        public float HitSoundTimeout = 1.0f;

        /// <summary>
        /// maximal number of particle emitters
        /// </summary>
        public int MaxEmitters = 1000;

        /// <summary>
        /// returns list of fragments with requested size
        /// this method pick fragments hidden from camera or sleeping rather then visible
        /// </summary>
        /// <param name="size">number of requested fragments</param>
        /// <returns>list of fragments</returns>
        public List<Fragment> GetAvailableFragments(int size)
        {
            if (size > pool.Length)
            {
                Debug.LogError("Requesting pool size higher than allocated! Please call Allocate first! " + size);
                return null;
            }

            if (size == pool.Length)
            {
                return new List<Fragment>(pool);
            }

            var fragments = new List<Fragment>();

            int counter = 0;

            // get deactivated fragments first
            foreach (var fragment in pool)
            {
                // get invisible fragments
                if (!fragment.activeObj && !fragment.cracked)
                {
                    fragments.Add(fragment);
                    counter++;
                }

                if (counter == size)
                {
                    return fragments;
                }
            }

            foreach (var fragment in pool)
            {
                // get invisible fragments
                if (!fragment.visible && !fragment.cracked)
                {
                    fragments.Add(fragment);
                    counter++;
                }

                if (counter == size)
                {
                    return fragments;
                }
            }

            // there are still live fragments ... get sleeping ones
            if (counter < size)
            {
                foreach (var fragment in pool)
                {
                    if (fragment.IsSleeping() && fragment.visible && !fragment.cracked)
                    {
                        ExploderUtils.Assert(!fragments.Contains(fragment), "!!!");
                        fragments.Add(fragment);
                        counter++;
                    }

                    if (counter == size)
                    {
                        return fragments;
                    }
                }
            }

            // there are still live fragments...
            if (counter < size)
            {
                foreach (var fragment in pool)
                {
                    if (!fragment.IsSleeping() && fragment.visible && !fragment.cracked)
                    {
                        ExploderUtils.Assert(!fragments.Contains(fragment), "!!!");
                        fragments.Add(fragment);
                        counter++;
                    }

                    if (counter == size)
                    {
                        return fragments;
                    }
                }
            }

            Debug.LogWarning("Not enough fragments in the pool, increase pool size!");
            return fragments;
        }

        /// <summary>
        /// returns number of available fragments that can be used for cracking the object
        /// </summary>
        public int GetAvailableCrackFragmentsCount()
        {
            int counter = 0;

            // get deactivated fragments first
            foreach (var fragment in pool)
            {
                // get invisible fragments
                if (!fragment.cracked)
                {
                    counter++;
                }
            }

            return counter;
        }

        /// <summary>
        /// create pool (array) of fragment game objects with all necessary components
        /// </summary>
        /// <param name="poolSize">number of fragments</param>
        /// <param name="useMeshColliders">use mesh colliders</param>
        /// <param name="use2dCollision">enable Unity 2D collision system</param>
        /// <param name="fragmentPrefab">custom fragment prefab object</param>
        public void Allocate(int poolSize, bool useMeshColliders, bool use2dCollision, GameObject fragmentPrefab)
        {
            ExploderUtils.Assert(poolSize > 0, "");

            if (pool == null || pool.Length < poolSize || useMeshColliders != this.meshColliders)
            {
                DestroyFragments();

                pool = new Fragment[poolSize];

                this.meshColliders = useMeshColliders;

                for (int i = 0; i < poolSize; i++)
                {
                    GameObject fragment = null;

                    if (fragmentPrefab)
                    {
                        fragment = Instantiate(fragmentPrefab) as GameObject;
                        fragment.name = "fragment_" + i;
                    }
                    else
                    {
                        fragment = new GameObject("fragment_" + i);
                    }

                    fragment.AddComponent<MeshFilter>();
                    fragment.AddComponent<MeshRenderer>();

                    if (use2dCollision)
                    {
                        fragment.AddComponent<PolygonCollider2D>();
                        fragment.AddComponent<Rigidbody2D>();
                    }
                    else
                    {
                        if (useMeshColliders)
                        {
                            var meshCollider = fragment.AddComponent<MeshCollider>();
                            meshCollider.convex = true;
                        }
                        else
                        {
                            fragment.AddComponent<BoxCollider>();
                        }

                        fragment.AddComponent<Rigidbody>();
                    }

                    fragment.AddComponent<ExploderOption>();

                    var fragmentComponent = fragment.AddComponent<Fragment>();

                    fragment.transform.parent = gameObject.transform;

                    pool[i] = fragmentComponent;

                    ExploderUtils.SetActiveRecursively(fragment.gameObject, false);

                    fragmentComponent.RefreshComponentsCache();

                    fragmentComponent.Sleep();
                }
            }
        }

        public void ResetTransform()
        {
            gameObject.transform.position = Vector3.zero;
            gameObject.transform.rotation = Quaternion.identity;
        }

        /// <summary>
        /// wake up physics (just for testing...)
        /// </summary>
        public void WakeUp()
        {
            foreach (var fragment in pool)
            {
                fragment.WakeUp();
            }
        }

        /// <summary>
        /// sleep physics (just for testing...)
        /// </summary>
        public void Sleep()
        {
            foreach (var fragment in pool)
            {
                fragment.Sleep();
            }
        }

        /// <summary>
        /// destroy objects in the pool
        /// </summary>
        public void DestroyFragments()
        {
            if (pool != null)
            {
                foreach (var fragment in pool)
                {
                    if (fragment)
                    {
                        Object.Destroy(fragment.gameObject);
                    }
                }

                pool = null;
            }
        }

        /// <summary>
        /// deactivate all fragments immediately
        /// </summary>
        public void DeactivateFragments()
        {
            if (pool != null)
            {
                foreach (var fragment in pool)
                {
                    if (fragment)
                    {
                        fragment.Deactivate();
                    }
                }
            }
        }

        /// <summary>
        /// set options for deactivations
        /// </summary>
        public void SetDeactivateOptions(DeactivateOptions options, FadeoutOptions fadeoutOptions, float timeout)
        {
            if (pool != null)
            {
                foreach (var fragment in pool)
                {
                    fragment.deactivateOptions = options;
                    fragment.deactivateTimeout = timeout;
                    fragment.fadeoutOptions = fadeoutOptions;
                }
            }
        }

        /// <summary>
        /// set options for explodable fragments, if true fragments can be destroyed again
        /// </summary>
        /// <param name="explodable"></param>
        public void SetExplodableFragments(bool explodable, bool dontUseTag)
        {
            if (pool != null)
            {
                if (dontUseTag)
                {
                    foreach (var fragment in pool)
                    {
                        if (fragment.gameObject)
                        {
                            fragment.gameObject.AddComponent<Explodable>();
                        }
                    }
                }
                else
                {
                    if (explodable)
                    {
                        foreach (var fragment in pool)
                        {
                            fragment.tag = ExploderObject.Tag;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// set options for fragment rigid bodies and layer
        /// </summary>
        public void SetFragmentPhysicsOptions(ExploderObject.FragmentOption options, bool physics2d)
        {
            if (pool != null)
            {
                var constrains = RigidbodyConstraints.None;

                if (options.FreezePositionX)
                    constrains |= RigidbodyConstraints.FreezePositionX;
                if (options.FreezePositionY)
                    constrains |= RigidbodyConstraints.FreezePositionY;
                if (options.FreezePositionZ)
                    constrains |= RigidbodyConstraints.FreezePositionZ;
                if (options.FreezeRotationX)
                    constrains |= RigidbodyConstraints.FreezeRotationX;
                if (options.FreezeRotationY)
                    constrains |= RigidbodyConstraints.FreezeRotationY;
                if (options.FreezeRotationZ)
                    constrains |= RigidbodyConstraints.FreezeRotationZ;

                foreach (var fragment in pool)
                {
                    if (fragment.gameObject)
                    {
                        fragment.gameObject.layer = LayerMask.NameToLayer(options.Layer);
                    }

                    fragment.SetConstraints(constrains);
                    fragment.DisableColliders(options.DisableColliders, meshColliders, physics2d);
                }
            }
        }

        /// <summary>
        /// set options for fragment sound and particle effects
        /// </summary>
        /// <param name="sfx"></param>
        public void SetSFXOptions(ExploderObject.SFXOption sfx)
        {
            if (pool != null)
            {
                HitSoundTimeout = sfx.HitSoundTimeout;
                MaxEmitters = sfx.EmitersMax;

                for (int i = 0; i < pool.Length; i++)
                {
                    pool[i].SetSFX(sfx, i < MaxEmitters);
                }
            }
        }

        /// <summary>
        /// returns list of currently active (visible) fragments
        /// </summary>
        /// <returns></returns>
        public List<Fragment> GetActiveFragments()
        {
            if (pool != null)
            {
                var list = new List<Fragment>(pool.Length);

                foreach (var fragment in pool)
                {
                    if (ExploderUtils.IsActive(fragment.gameObject))
                    {
                        list.Add(fragment);
                    }
                }

                return list;
            }

            return null;
        }

        void Update()
        {
            fragmentSoundTimeout -= Time.deltaTime;
        }

        public void OnFragmentHit()
        {
            fragmentSoundTimeout = HitSoundTimeout;
        }

        public bool CanPlayHitSound()
        {
            return fragmentSoundTimeout <= 0.0f;
        }
    }
}
