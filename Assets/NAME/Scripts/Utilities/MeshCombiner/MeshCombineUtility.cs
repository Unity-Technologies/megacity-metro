using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Rendering;

namespace Unity.NAME.Utilities
{
    public static class MeshCombineUtility
    {
        public class RenderBatchData
        {
            public class MeshAndTrs
            {
                public Mesh Mesh;
                public Matrix4x4 Trs;

                public MeshAndTrs(Mesh m, Matrix4x4 t)
                {
                    Mesh = m;
                    Trs = t;
                }
            }

            public Material Material;
            public int SubmeshIndex = 0;
            public ShadowCastingMode ShadowMode;
            public bool ReceiveShadows;
            public MotionVectorGenerationMode MotionVectors;
            public List<MeshAndTrs> MeshesWithTrs = new List<MeshAndTrs>();
        }

        public enum RendererDisposeMethod
        {
            DestroyGameObject,
            DestroyRendererAndFilter,
            DisableGameObject,
            DisableRenderer,
        }

        public static void Combine(List<MeshRenderer> renderers, RendererDisposeMethod disposeMethod, string newObjectName)
        {
            int renderersCount = renderers.Count;

            List<RenderBatchData> renderBatches = new List<RenderBatchData>();

            // Build render batches for all unique material + submeshIndex combinations
            for (int i = 0; i < renderersCount; i++)
            {
                MeshRenderer meshRenderer = renderers[i];

                if (meshRenderer == null)
                    continue;

                MeshFilter meshFilter = meshRenderer.GetComponent<MeshFilter>();

                if (meshFilter == null)
                    continue;

                Mesh mesh = meshFilter.sharedMesh;

                if (mesh == null)
                    continue;

                Transform t = meshRenderer.GetComponent<Transform>();
                Material[] materials = meshRenderer.sharedMaterials;

                for (int s = 0; s < mesh.subMeshCount; s++)
                {
                    if (materials[s] == null)
                        continue;

                    int batchIndex = GetExistingRenderBatch(renderBatches, materials[s], meshRenderer, s);
                    if (batchIndex >= 0)
                    {
                        renderBatches[batchIndex].MeshesWithTrs.Add(new RenderBatchData.MeshAndTrs(mesh, Matrix4x4.TRS(t.position, t.rotation, t.lossyScale)));
                    }
                    else
                    {
                        RenderBatchData newBatchData = new RenderBatchData();
                        newBatchData.Material = materials[s];
                        newBatchData.SubmeshIndex = s;
                        newBatchData.ShadowMode = meshRenderer.shadowCastingMode;
                        newBatchData.ReceiveShadows = meshRenderer.receiveShadows;
                        newBatchData.MeshesWithTrs.Add(new RenderBatchData.MeshAndTrs(mesh, Matrix4x4.TRS(t.position, t.rotation, t.lossyScale)));

                        renderBatches.Add(newBatchData);
                    }
                }

                // Destroy probuilder component if present
                ProBuilderMesh pbm = meshRenderer.GetComponent<ProBuilderMesh>();
                if (pbm)
                {
                    GameObject.Destroy(pbm);
                }

                switch (disposeMethod)
                {
                    case RendererDisposeMethod.DestroyGameObject:
                        if (Application.isPlaying)
                        {
                            GameObject.Destroy(meshRenderer.gameObject);
                        }
                        else
                        {
                            GameObject.DestroyImmediate(meshRenderer.gameObject);
                        }
                        break;
                    case RendererDisposeMethod.DestroyRendererAndFilter:
                        if (Application.isPlaying)
                        {
                            GameObject.Destroy(meshRenderer);
                            GameObject.Destroy(meshFilter);
                        }
                        else
                        {
                            GameObject.DestroyImmediate(meshRenderer);
                            GameObject.DestroyImmediate(meshFilter);
                        }
                        break;
                    case RendererDisposeMethod.DisableGameObject:
                        meshRenderer.gameObject.SetActive(false);
                        break;
                    case RendererDisposeMethod.DisableRenderer:
                        meshRenderer.enabled = false;
                        break;
                }
            }

            // Combine each unique render batch
            for (int i = 0; i < renderBatches.Count; i++)
            {
                RenderBatchData rbd = renderBatches[i];

                Mesh newMesh = new Mesh();
                newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                CombineInstance[] combineInstances = new CombineInstance[rbd.MeshesWithTrs.Count];

                for (int j = 0; j < rbd.MeshesWithTrs.Count; j++)
                {
                    combineInstances[j].subMeshIndex = rbd.SubmeshIndex;
                    combineInstances[j].mesh = rbd.MeshesWithTrs[j].Mesh;
                    combineInstances[j].transform = rbd.MeshesWithTrs[j].Trs;
                }

                // Create mesh
                newMesh.CombineMeshes(combineInstances);
                newMesh.RecalculateBounds();

                // Create the gameObject
                GameObject combinedObject = new GameObject(newObjectName);
                MeshFilter mf = combinedObject.AddComponent<MeshFilter>();
                mf.sharedMesh = newMesh;
                MeshRenderer mr = combinedObject.AddComponent<MeshRenderer>();
                mr.sharedMaterial = rbd.Material;
                mr.shadowCastingMode = rbd.ShadowMode;
            }
        }

        private static int GetExistingRenderBatch(List<RenderBatchData> renderBatches, Material mat, MeshRenderer ren, int submeshIndex)
        {
            for (int i = 0; i < renderBatches.Count; i++)
            {
                RenderBatchData data = renderBatches[i];
                if (data.Material == mat &&
                    data.SubmeshIndex == submeshIndex &&
                    data.ShadowMode == ren.shadowCastingMode &&
                    data.ReceiveShadows == ren.receiveShadows)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
