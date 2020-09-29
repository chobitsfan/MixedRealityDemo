using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SparseDesign
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]

    /// <summary>
    /// Creates a tube with with. Bottom and top radiuses can be different.
    /// </summary>
    public class CreateTube : MonoBehaviour
    {
        [SerializeField] public float height = 1f;
        [SerializeField] public int nbSides = 24;

        [SerializeField] public float bottomRadius1 = .5f;
        [SerializeField] public float topRadius1 = .5f;
        [SerializeField] public bool generated = false;

        private Mesh mesh;

        void Awake()
        {
            mesh = this.gameObject.GetComponent<MeshFilter>().mesh;
        }

        // Start is called before the first frame update
        void Start()
        {
            if (!generated)
            {
                Generate();
            }
        }

        void OnValidate()
        {
            if (mesh)
            {
                Generate();
            }
        }

        void Generate()
        {
            mesh.Clear();

            int nbVerticesSides = nbSides;
            #region Vertices

            Vector3[] vertices = new Vector3[nbVerticesSides * 2];
            Vector3[] normales = new Vector3[vertices.Length];
            Vector4[] tangents = new Vector4[vertices.Length];
            int vert = 0;
            float _2pi = Mathf.PI * 2f;
            int sideCounter;

            // Sides (out)
            sideCounter = 0;
            //while (vert < nbVerticesCap * 2 + nbVerticesSides)
            while (vert < nbVerticesSides * 2)
            {
                sideCounter = (sideCounter == nbSides) ? 0 : sideCounter;
                //Debug.Log(sideCounter);

                float r1 = (float)(sideCounter++) / nbSides * _2pi;
                float cos = Mathf.Cos(r1);
                float sin = Mathf.Sin(r1);

                vertices[vert] = new Vector3(cos * bottomRadius1, 0, sin * bottomRadius1);
                vertices[vert + 1] = new Vector3(cos * topRadius1, height, sin * topRadius1);

                normales[vert] = normales[vert + 1] = new Vector3(cos, 10f, sin).normalized;
                tangents[vert] = tangents[vert + 1] = new Vector4(sin, 0f, cos, -1);
                vert += 2;
            }

            #endregion

            #region UVs
            Vector2[] uvs = new Vector2[vertices.Length];

            vert = 0;

            // Sides (out)
            sideCounter = 0;
            while (vert < (vertices.Length - 1))
            {
                float t = (float)(sideCounter++) / nbSides;
                //Debug.Log("t: " + t);
                uvs[vert++] = new Vector2(t, 0f);
                uvs[vert++] = new Vector2(t, 1f);
            }

            #endregion

            #region Triangles
            //int nbFace = nbSides * 4;
            int nbTriangles = nbSides * 2;
            //int nbIndexes = nbTriangles * 3;
            int[] triangles = new int[nbTriangles * 3];

            // Bottom cap
            int i = 0;
            sideCounter = 0;


            int ld = (vertices.Length - 1) - 1;
            int lu = (vertices.Length - 1);
            int rd = 0;
            int ru = 1;
            triangles[i++] = lu;
            triangles[i++] = rd;
            triangles[i++] = ld;

            triangles[i++] = lu;
            triangles[i++] = ru;
            triangles[i++] = rd;

            while (sideCounter < vertices.Length - 2)
            {
                ld = sideCounter;
                lu = ld + 1;
                rd = ld + 2;
                ru = ld + 3;

                triangles[i++] = lu;
                triangles[i++] = rd;
                triangles[i++] = ld;

                triangles[i++] = lu;
                triangles[i++] = ru;
                triangles[i++] = rd;

                sideCounter += 2;
            }

            #endregion

            mesh.vertices = vertices;
            mesh.normals = normales;
            mesh.tangents = tangents;
            mesh.uv = uvs;
            mesh.triangles = triangles;

            //mesh.RecalculateBounds();
            //mesh.Optimize();
            generated = true;
            this.gameObject.GetComponent<MeshRenderer>().enabled = true;
        }
    }
}