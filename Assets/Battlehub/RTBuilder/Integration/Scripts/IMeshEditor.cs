﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace Battlehub.ProBuilderIntegration
{
    public enum MeshEditorSelectionMode
    {
        Add,
        Substract,
        Difference
    }

    public class MeshEditorState
    {
        internal readonly Dictionary<ProBuilderMesh, MeshState> State = new Dictionary<ProBuilderMesh, MeshState>();
    }

    internal class MeshState
    {
        public readonly IList<Vector3> Positions;
        public readonly IList<PBFace> Faces;

        public MeshState(IList<Vector3> positions, IList<Face> faces)
        {
            Positions = positions;
            Faces = faces.Select(f => new PBFace(f)).ToList();
        }
    }

    public class MeshSelection
    {
        internal Dictionary<ProBuilderMesh, IList<Face>> SelectedFaces = new Dictionary<ProBuilderMesh, IList<Face>>();
        internal Dictionary<ProBuilderMesh, IList<Face>> UnselectedFaces = new Dictionary<ProBuilderMesh, IList<Face>>();

        internal Dictionary<ProBuilderMesh, IList<Edge>> SelectedEdges = new Dictionary<ProBuilderMesh, IList<Edge>>();
        internal Dictionary<ProBuilderMesh, IList<Edge>> UnselectedEdges = new Dictionary<ProBuilderMesh, IList<Edge>>();

        internal Dictionary<ProBuilderMesh, IList<int>> SelectedIndices = new Dictionary<ProBuilderMesh, IList<int>>();
        internal Dictionary<ProBuilderMesh, IList<int>> UnselectedIndices = new Dictionary<ProBuilderMesh, IList<int>>();

        public bool HasFaces
        {
            get { return SelectedFaces.Count != 0 || UnselectedFaces.Count != 0; }
        }

        public bool HasEdges
        {
            get { return SelectedEdges.Count != 0 || UnselectedEdges.Count != 0; }
        }

        public bool HasVertices
        {
            get { return SelectedIndices.Count != 0 || UnselectedIndices.Count != 0; }
        }

        public MeshSelection()
        {

        }

        public MeshSelection(MeshSelection selection)
        {
            SelectedFaces = selection.SelectedFaces.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            UnselectedFaces = selection.UnselectedFaces.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            SelectedEdges = selection.SelectedEdges.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            UnselectedEdges = selection.UnselectedEdges.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            SelectedIndices = selection.SelectedIndices.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            UnselectedIndices = selection.UnselectedIndices.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public MeshSelection(params GameObject[] gameObjects)
        {
            AddToSelection(gameObjects, SelectedFaces, SelectedEdges, SelectedIndices);
        }

        public void Invert()
        {
            var temp1 = SelectedFaces;
            SelectedFaces = UnselectedFaces;
            UnselectedFaces = temp1;

            var temp2 = SelectedEdges;
            SelectedEdges = UnselectedEdges;
            UnselectedEdges = temp2;

            var temp3 = SelectedIndices;
            SelectedIndices = UnselectedIndices;
            UnselectedIndices = temp3;
        }

        private static void AddToSelection(GameObject[] gameObjects, Dictionary<ProBuilderMesh, IList<Face>> faces, Dictionary<ProBuilderMesh, IList<Edge>> edges, Dictionary<ProBuilderMesh, IList<int>> indices)
        {
            for (int i = 0; i < gameObjects.Length; ++i)
            {
                GameObject go = gameObjects[i];
                ProBuilderMesh mesh = go.GetComponent<ProBuilderMesh>();
                if (mesh != null)
                {
                    if (!faces.ContainsKey(mesh))
                    {
                        faces.Add(mesh, new List<Face>());
                    }

                    if(!edges.ContainsKey(mesh))
                    {
                        edges.Add(mesh, new List<Edge>());
                    }

                    if (!indices.ContainsKey(mesh))
                    {
                        indices.Add(mesh, new List<int>());
                    }
                }
            }
        }

        public void FacesToVertices(bool invert)
        {
            SelectedIndices.Clear();
            UnselectedIndices.Clear();

            foreach(KeyValuePair<ProBuilderMesh, IList<Face>> kvp in invert ? UnselectedFaces : SelectedFaces)
            {
                ProBuilderMesh mesh;
                List<int> indices;
                GetCoindicentIndices(kvp, out mesh, out indices);

                SelectedIndices.Add(mesh, indices);
            }

            foreach(KeyValuePair<ProBuilderMesh, IList<Face>> kvp in invert ? SelectedFaces : UnselectedFaces)
            {
                ProBuilderMesh mesh;
                List<int> indices;
                GetCoindicentIndices(kvp, out mesh, out indices);

                UnselectedIndices.Add(mesh, indices);
            }
        }

        public void VerticesToFaces(bool invert)
        {
            SelectedFaces.Clear();
            UnselectedFaces.Clear();

            foreach (KeyValuePair<ProBuilderMesh, IList<int>> kvp in invert ? UnselectedIndices : SelectedIndices)
            {
                ProBuilderMesh mesh = kvp.Key;
                HashSet<int> indicesHs = new HashSet<int>(mesh.GetCoincidentVertices(kvp.Value));
                List<Face> faces = GetFaces(mesh, indicesHs);

                if (faces.Count > 0)
                {
                    SelectedFaces.Add(mesh, faces);
                }
            }

            foreach (KeyValuePair<ProBuilderMesh, IList<int>> kvp in invert ? SelectedIndices : UnselectedIndices)
            {
                ProBuilderMesh mesh = kvp.Key;
                HashSet<int> indicesHs = new HashSet<int>(mesh.GetCoincidentVertices(kvp.Value));
                List<Face> faces = GetFaces(mesh, indicesHs);

                if (faces.Count > 0)
                {
                    UnselectedFaces.Add(mesh, faces);
                }
            }
        }

        public void FacesToEdges(bool invert)
        {
            SelectedEdges.Clear();
            UnselectedEdges.Clear();

            foreach (KeyValuePair<ProBuilderMesh, IList<Face>> kvp in invert ? UnselectedFaces : SelectedFaces)
            {
                ProBuilderMesh mesh;
                HashSet<Edge> edgesHs;
                GetEdges(kvp, out mesh, out edgesHs);
                SelectedEdges.Add(mesh, edgesHs.ToArray());
            }

            foreach (KeyValuePair<ProBuilderMesh, IList<Face>> kvp in invert ? SelectedFaces : UnselectedFaces)
            {
                ProBuilderMesh mesh;
                HashSet<Edge> edgesHs;
                GetEdges(kvp, out mesh, out edgesHs);
                UnselectedEdges.Add(mesh, edgesHs.ToArray());
            }
        }

        public void EdgesToFaces(bool invert)
        {
            SelectedFaces.Clear();
            UnselectedFaces.Clear();

            foreach (KeyValuePair<ProBuilderMesh, IList<Edge>> kvp in invert ? UnselectedEdges : SelectedEdges)
            {
                ProBuilderMesh mesh = kvp.Key;
                HashSet<Edge> edgesHs = new HashSet<Edge>(kvp.Value);
                List<Face> faces = GetFaces(mesh, edgesHs);

                if (faces.Count > 0)
                {
                    SelectedFaces.Add(mesh, faces);
                }
            }

            foreach (KeyValuePair<ProBuilderMesh, IList<Edge>> kvp in invert ? SelectedEdges : UnselectedEdges)
            {
                ProBuilderMesh mesh = kvp.Key;
                HashSet<Edge> edgesHs = new HashSet<Edge>(kvp.Value);
                List<Face> faces = GetFaces(mesh, edgesHs);

                if (faces.Count > 0)
                {
                    UnselectedFaces.Add(mesh, faces);
                }
            }
        }

        public void EdgesToVertices(bool invert)
        {
            SelectedIndices.Clear();
            UnselectedIndices.Clear();

            foreach (KeyValuePair<ProBuilderMesh, IList<Edge>> kvp in invert ? UnselectedEdges : SelectedEdges)
            {
                ProBuilderMesh mesh;
                List<int> indices;
                GetCoindicentIndices(kvp, out mesh, out indices);

                SelectedIndices.Add(mesh, indices);
            }

            foreach (KeyValuePair<ProBuilderMesh, IList<Edge>> kvp in invert ? SelectedEdges : UnselectedEdges)
            {
                ProBuilderMesh mesh;
                List<int> indices;
                GetCoindicentIndices(kvp, out mesh, out indices);

                UnselectedIndices.Add(mesh, indices);
            }
        }

        public void VerticesToEdges(bool invert)
        {
            SelectedEdges.Clear();
            UnselectedEdges.Clear();

            foreach (KeyValuePair<ProBuilderMesh, IList<int>> kvp in invert ? UnselectedIndices : SelectedIndices)
            {
                ProBuilderMesh mesh = kvp.Key;
                HashSet<int> indicesHs = new HashSet<int>(mesh.GetCoincidentVertices(kvp.Value));
                List<Edge> edges = GetEdges(mesh, indicesHs);

                if (edges.Count > 0)
                {
                    SelectedEdges.Add(mesh, edges);
                }
            }

            foreach (KeyValuePair<ProBuilderMesh, IList<int>> kvp in invert ? SelectedIndices : UnselectedIndices)
            {
                ProBuilderMesh mesh = kvp.Key;
                HashSet<int> indicesHs = new HashSet<int>(mesh.GetCoincidentVertices(kvp.Value));
                List<Edge> edges = GetEdges(mesh, indicesHs);

                if (edges.Count > 0)
                {
                    UnselectedEdges.Add(mesh, edges);
                }
            }
        }

        private static List<Face> GetFaces(ProBuilderMesh mesh, HashSet<int> indicesHs)
        {
            IList<Face> allFaces = mesh.faces;
            List<Face> faces = new List<Face>();
            for (int i = 0; i < allFaces.Count; ++i)
            {
                Face face = allFaces[i];

                if (face.indexes.All(index => indicesHs.Contains(index)))
                {
                    faces.Add(face);
                }
            }

            return faces;
        }

        private static List<Face> GetFaces(ProBuilderMesh mesh, HashSet<Edge> edgesHs)
        {
            IList<Face> allFaces = mesh.faces;
            List<Face> faces = new List<Face>();
            for (int i = 0; i < allFaces.Count; ++i)
            {
                Face face = allFaces[i];

                if (face.edges.All(index => edgesHs.Contains(index)))
                {
                    faces.Add(face);
                }
            }
            return faces;
        }

        private static List<Edge> GetEdges(ProBuilderMesh mesh, HashSet<int> indicesHs)
        {
            IList<Face> allFaces = mesh.faces;
            HashSet<Edge> edgesHs = new HashSet<Edge>();
            for (int i = 0; i < allFaces.Count; ++i)
            {
                Face face = allFaces[i];
                ReadOnlyCollection<Edge> edges = face.edges;
                for(int e = 0; e < edges.Count; ++e)
                {
                    Edge edge = edges[e];
                    if(!edgesHs.Contains(edge))
                    {
                        if(indicesHs.Contains(edge.a) && indicesHs.Contains(edge.b))
                        {
                            edgesHs.Add(edge);
                        }
                    }
                }
            }

            return edgesHs.ToList();
        }

        private static void GetEdges(KeyValuePair<ProBuilderMesh, IList<Face>> kvp, out ProBuilderMesh mesh, out HashSet<Edge> edgesHs)
        {
            mesh = kvp.Key;
            edgesHs = new HashSet<Edge>();
            IList<Face> faces = kvp.Value;
            for (int i = 0; i < faces.Count; ++i)
            {
                ReadOnlyCollection<Edge> edges = faces[i].edges;
                for (int e = 0; e < edges.Count; ++e)
                {
                    if (!edgesHs.Contains(edges[e]))
                    {
                        edgesHs.Add(edges[e]);
                    }
                }
            }
        }

        private static void GetCoindicentIndices(KeyValuePair<ProBuilderMesh, IList<Face>> kvp, out ProBuilderMesh mesh, out List<int> indices)
        {
            mesh = kvp.Key;
            IList<Face> faces = kvp.Value;
            indices = new List<int>();
            mesh.GetCoincidentVertices(faces, indices);
        }

        private static void GetCoindicentIndices(KeyValuePair<ProBuilderMesh, IList<Edge>> kvp, out ProBuilderMesh mesh, out List<int> indices)
        {
            mesh = kvp.Key;
            IList<Edge> edges = kvp.Value;
            indices = new List<int>();
            mesh.GetCoincidentVertices(edges, indices);
        }
    }

    public interface IMeshEditor
    {
        bool HasSelection
        {
            get;
        }

        bool CenterMode
        {
            get;
            set;
        }

        bool GlobalMode
        {
            get;
            set;
        }

        Vector3 Position
        {
            get;
            set;
        }

        Vector3 Normal
        {
            get;
        }

        GameObject Target
        {
            get;
        }

        void Hover(Camera camera, Vector3 pointer);
        void Extrude(float distance = 0.0f);
        void Delete();
        MeshSelection SelectHoles();
        void FillHoles();

        MeshSelection Select(Camera camera, Vector3 pointer, bool shift);
        MeshSelection Select(Camera camera, Rect rect, GameObject[] gameObjects, MeshEditorSelectionMode mode);
        MeshSelection Select(Material material);
        MeshSelection Unselect(Material material);

        void ApplySelection(MeshSelection selection);
        void RollbackSelection(MeshSelection selection);

        MeshSelection GetSelection();
        MeshSelection ClearSelection();
        
        MeshEditorState GetState();
        void SetState(MeshEditorState state);

        void BeginRotate(Quaternion initialRotation);
        void Rotate(Quaternion rotation);
        void EndRotate();

        void BeginScale();
        void Scale(Vector3 scale, Quaternion rotation);
        void EndScale();
    }

    public static class IMeshEditorExt
    {
        public static MeshSelection Select(Material material)
        {
            MeshSelection selection = new MeshSelection();
            ProBuilderMesh[] meshes = UnityEngine.Object.FindObjectsOfType<ProBuilderMesh>();
            foreach (ProBuilderMesh mesh in meshes)
            {
                Renderer renderer = mesh.GetComponent<Renderer>();
                if (renderer == null)
                {
                    continue;
                }

                Material[] materials = renderer.sharedMaterials;
                int index = Array.IndexOf(materials, material);
                if (index < 0)
                {
                    continue;
                }

                List<Face> selectedFaces = new List<Face>();
                IList<Face> faces = mesh.faces;
                for (int i = 0; i < faces.Count; ++i)
                {
                    Face face = faces[i];
                    if (face.submeshIndex == index)
                    {
                        selectedFaces.Add(face);
                    }
                }

                if (selectedFaces.Count > 0)
                {
                    selection.SelectedFaces.Add(mesh, selectedFaces);
                }
            }
            return selection;
        }

    }
}


