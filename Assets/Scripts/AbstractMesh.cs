using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractMesh {

    protected string _name;
    public string name {
        get {
            return _name;
        }
    }
    

    public AbstractMesh(string name) {
        _name = name;
    }

    public abstract void UpdateMesh(Mesh mesh);


    protected static Vector3[] CalculateNormals(Vector3[] vertices, int[] quads) {
        Vector3[] normals = new Vector3[vertices.Length];
        for (int i = 0; i < quads.Length; i += 4) {
            int i0 = quads[i];
            int i1 = quads[i + 1];
            int i2 = quads[i + 2];
            int i3 = quads[i + 3];
            Vector3 normal = CalculateNormal(vertices[i0], vertices[i1], vertices[i2]);
            //Debug.Log("normal quad[" + (i / 4) + "] = " + normal);

            // flat map: all vertices have the same normal
            normals[i0] = normal;
            normals[i1] = normal;
            normals[i2] = normal;
            normals[i3] = normal;
        }
        return normals;
    }


    private static Vector3 CalculateNormal(Vector3 v0, Vector3 v1, Vector3 v2) {
        Vector3 d0 = v0 - v1;
        Vector3 d1 = v1 - v2;
        return Vector3.Cross(d0, d1).normalized;
    }


    protected static void generateUniqueVertices<V>(
        V[] originalVertices, int[] originalQuads, int[] originalLines, 
        out V[] newVertices, out int[] newQuads, out int[] newLines
        ) 
    {
        int nVertices = originalQuads.Length;
        newVertices = new V[nVertices];
        newQuads = new int[nVertices];
        int[] vertexMap = new int[originalVertices.Length];
        for (int i = 0; i < originalVertices.Length; i++) {
            vertexMap[i] = -1;
        }

        for (int i = 0; i < nVertices; i += 4) {
            int i0 = originalQuads[i + 0];
            int i1 = originalQuads[i + 1];
            int i2 = originalQuads[i + 2];
            int i3 = originalQuads[i + 3];

            vertexMap[i0] = i + 0;
            vertexMap[i1] = i + 1;
            vertexMap[i2] = i + 2;
            vertexMap[i3] = i + 3;

            newVertices[i + 0] = originalVertices[i0];
            newVertices[i + 1] = originalVertices[i1];
            newVertices[i + 2] = originalVertices[i2];
            newVertices[i + 3] = originalVertices[i3];

            newQuads[i + 0] = i + 0;
            newQuads[i + 1] = i + 1;
            newQuads[i + 2] = i + 2;
            newQuads[i + 3] = i + 3;
        }

        newLines = new int[originalLines.Length];
        for (int i = 0; i < originalLines.Length; i++) {
            newLines[i] = vertexMap[originalLines[i]];
        }
    }
}


public class Mesh3D : AbstractMesh {
    private int[] lines;
    private int[] quads;
    private Vector3[] vertices;

    public Mesh3D(string name) : base(name) {
    }

    public override void UpdateMesh(Mesh mesh) {
        float angle = Time.time % 360.0f;
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);

        Vector3[] transformedVertices = new Vector3[vertices.Length];
        for (int i = 0; i < transformedVertices.Length; i++) {
            Vector3 orig = vertices[i];
            //float scale = Mathf.Pow(0.8f, Time.time);
            //transformedVertices[i] = new Vector3(orig.x, orig.y * 2.0f * scale, orig.z);

            float newX = (cos * orig.x) + (sin * orig.z);
            float newZ = (cos * orig.z) - (sin * orig.x);
            transformedVertices[i] = new Vector3(newX, orig.y, newZ);
        }

        mesh.Clear();

        mesh.vertices = transformedVertices;
        mesh.normals = CalculateNormals(transformedVertices, quads);

        mesh.subMeshCount = 2;
        mesh.SetIndices(lines, MeshTopology.Lines, 0);
        mesh.SetIndices(quads, MeshTopology.Quads, 1);
    }

    private void generateUniqueVertices(Vector3[] originalVertices, int[] originalQuads, int[] originalLines) {
        generateUniqueVertices(originalVertices, originalQuads, originalLines, out vertices, out quads, out lines);
    }

    public static Mesh3D GenerateCube() {
        Vector3[] cubeVertices = new Vector3[] {
            new Vector3(-1.0f, -1.0f, -1.0f),   // 0-3 bottom
            new Vector3( 1.0f, -1.0f, -1.0f),
            new Vector3( 1.0f, -1.0f,  1.0f),
            new Vector3(-1.0f, -1.0f,  1.0f),

            new Vector3(-1.0f,  1.0f,  1.0f),   // 4-7 top
            new Vector3( 1.0f,  1.0f,  1.0f),
            new Vector3( 1.0f,  1.0f, -1.0f),
            new Vector3(-1.0f,  1.0f, -1.0f),
        };
        int[] cubeQuads = new int[] {
            0, 1, 2, 3,     // bottom
            4, 5, 6, 7,     // top
            7, 6, 1, 0,     // front
            6, 5, 2, 1,     // right
            5, 4, 3, 2,     // back
            4, 7, 0, 3,     // left
        };
        int[] cubeLines = new int[] {
             0,  1,  1,  2,  2,  3,  3,  0,     // bottom
             4,  5,  5,  6,  6,  7,  7,  4,     // top
             0,  7,  1,  6,  2,  5,  3,  4,     // sides
        };

        Mesh3D cube = new Mesh3D("Cube");
        cube.generateUniqueVertices(cubeVertices, cubeQuads, cubeLines);
        return cube;
    }
}


public class Mesh4D : AbstractMesh {
    private int[] lines;
    private int[] quads;
    private Vector4[] vertices;

    public Mesh4D(string name) : base(name) {
    }

    public override void UpdateMesh(Mesh mesh) {
        float angle = (Time.time * 0.5f) % 360.0f;
        //float angle = 0.0f;
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);

        Vector3[] transformedVertices = new Vector3[vertices.Length];
        for (int i = 0; i < transformedVertices.Length; i++) {
            Vector4 orig = vertices[i];

            float newZ = (orig.z * cos) + (orig.w * sin);
            float newW = (orig.w * cos) - (orig.z * sin);

            float wProj = 1.25f - (newW * 0.25f);
            transformedVertices[i] = wProj * (new Vector3(orig.x, orig.y, newZ));
        }

        mesh.Clear();

        mesh.vertices = transformedVertices;
        mesh.normals = CalculateNormals(transformedVertices, quads);

        mesh.subMeshCount = 2;
        mesh.SetIndices(lines, MeshTopology.Lines, 0);
        mesh.SetIndices(quads, MeshTopology.Quads, 1);
    }

    private void generateUniqueVertices(Vector4[] originalVertices, int[] originalQuads, int[] originalLines) {
        generateUniqueVertices(originalVertices, originalQuads, originalLines, out vertices, out quads, out lines);
    }

    public static Mesh4D GenerateHypercube(float initialRotation) {
        Vector4[] cubeVertices = new Vector4[] {
            new Vector4(-1.0f, -1.0f, -1.0f,  1.0f),   // 0-3 bottom inner
            new Vector4( 1.0f, -1.0f, -1.0f,  1.0f),
            new Vector4( 1.0f, -1.0f,  1.0f,  1.0f),
            new Vector4(-1.0f, -1.0f,  1.0f,  1.0f),

            new Vector4(-1.0f,  1.0f,  1.0f,  1.0f),   // 4-7 top inner
            new Vector4( 1.0f,  1.0f,  1.0f,  1.0f),
            new Vector4( 1.0f,  1.0f, -1.0f,  1.0f),
            new Vector4(-1.0f,  1.0f, -1.0f,  1.0f),

            new Vector4(-1.0f, -1.0f, -1.0f, -1.0f),   // 8-11 bottom outer
            new Vector4( 1.0f, -1.0f, -1.0f, -1.0f),
            new Vector4( 1.0f, -1.0f,  1.0f, -1.0f),
            new Vector4(-1.0f, -1.0f,  1.0f, -1.0f),

            new Vector4(-1.0f,  1.0f,  1.0f, -1.0f),   // 12-15 top outer
            new Vector4( 1.0f,  1.0f,  1.0f, -1.0f),
            new Vector4( 1.0f,  1.0f, -1.0f, -1.0f),
            new Vector4(-1.0f,  1.0f, -1.0f, -1.0f),
        };
        int[] cubeQuads = new int[] {
             0,  1,  2,  3,     // bottom inner
             4,  5,  6,  7,     // top inner
             7,  6,  1,  0,     // front inner
             6,  5,  2,  1,     // right inner
             5,  4,  3,  2,     // back inner
             4,  7,  0,  3,     // left inner

             8,  9, 10, 11,     // bottom outer
            12, 13, 14, 15,     // top outer
            15, 14,  9,  8,     // front outer
            14, 13, 10,  9,     // right outer
            13, 12, 11, 10,     // back outer
            12, 15,  8, 11,     // left outer

            // TODO: 4x top inner-outer faces (front, right, back, left)
            // TODO: 4x bottom inner-outer faces (front, right, back, left)
        };
        int[] cubeLines = new int[] {
             0,  1,  1,  2,  2,  3,  3,  0,     // bottom inner
             4,  5,  5,  6,  6,  7,  7,  4,     // top inner
             0,  7,  1,  6,  2,  5,  3,  4,     // sides inner

             8,  9,  9, 10, 10, 11, 11,  8,     // bottom outer
            12, 13, 13, 14, 14, 15, 15, 12,     // top outer
             8, 15,  9, 14, 10, 13, 11, 12,     // sides outer

             0,  8,  1,  9,  2, 10,  3, 11,     // bottom w-connections
             4, 12,  5, 13,  6, 14,  7, 15,     // top w-connections
        };

        Mesh4D hypercube = new Mesh4D("Hypercube");
        hypercube.generateUniqueVertices(cubeVertices, cubeQuads, cubeLines);

        // initial rotation
        if (initialRotation != 0.0f) {
            float angle = initialRotation * Mathf.Deg2Rad;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            //foreach (Vector4 v in hypercube.vertices) {
            for (int i = 0; i < hypercube.vertices.Length; i++) {
                Vector4 v = hypercube.vertices[i];
                float newX = (v.x * cos) + (v.w * sin);
                float newW = (v.w * cos) - (v.x * sin);
                hypercube.vertices[i] = new Vector4(newX, v.y, v.z, newW);
            }
        }

        return hypercube;
    }
}
