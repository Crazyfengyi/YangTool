// Version 1.6.2
// ©2016 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Exploder
{
    internal class PartialSeparator : ExploderTask
    {
        private readonly MeshCutter cutter;

        public PartialSeparator(Core Core) : base(Core)
        {
            // init cutter
            cutter = new MeshCutter();
            cutter.Init(512, 512);
        }

        public override TaskType Type
        {
            get { return TaskType.PartialSeparator; }
        }

        public override void Init()
        {
            base.Init();
        }

        public override bool Run(float frameBudget)
        {
            Separate();

            Watch.Stop();

            return true;
        }

        private void Separate()
        {
            Debug.Assert(core.meshSet.Count == 1);

            Debug.DrawLine(core.parameters.HitPosition, core.parameters.HitPosition + core.parameters.ShotDir*1000, Color.red, 10000);

            var mesh = core.meshSet.ElementAt(0);

            var plane = new Exploder.Plane(-core.parameters.ShotDir, core.parameters.HitPosition + core.parameters.ShotDir*0.2f);

            List<ExploderMesh> meshes = null;

            var triangulateHoles = true;
            var crossSectionVertexColour = Color.white;
            var crossSectionUV = new Vector4(0, 0, 1, 1);
            
            if (mesh.option)
            {
                triangulateHoles = !mesh.option.Plane2D;
                crossSectionVertexColour = mesh.option.CrossSectionVertexColor;
                crossSectionUV = mesh.option.CrossSectionUV;
                core.splitMeshIslands |= mesh.option.SplitMeshIslands;
            }

            if (core.parameters.Use2DCollision)
            {
                triangulateHoles = false;
            }

            cutter.Cut(mesh.mesh, mesh.transform, plane, triangulateHoles, core.parameters.DisableTriangulation, ref meshes, crossSectionVertexColour, crossSectionUV);

            core.meshSet.Clear();

            if (meshes != null)
            {
                core.meshSet.Add(new MeshObject
                {
                    mesh = meshes[0],

                    material = mesh.material,
                    transform = mesh.transform,
                    id = mesh.id,
                    original = mesh.original,
                    skinnedOriginal = mesh.skinnedOriginal,

                    parent = mesh.transform.parent,
                    position = mesh.transform.position,
                    rotation = mesh.transform.rotation,
                    localScale = mesh.transform.localScale,

                    option = mesh.option,
                });

                var unityMesh = meshes[1].ToUnityMesh();
                var meshFilter = mesh.original.GetComponent<MeshFilter>();
                meshFilter.sharedMesh = unityMesh;
            }
        }
    }
}
