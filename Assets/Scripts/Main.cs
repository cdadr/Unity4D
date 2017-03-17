using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour {

    public Material FaceMaterial;
    public Material WireframeMaterial;
    public Boolean FourDim = false;

    private GameObject proceduralObject;
    private static AbstractMesh meshObject;


    void Start() {
        if (FourDim) {
            meshObject = Mesh4D.GenerateHypercube(45.0f);
        } else {
            meshObject = Mesh3D.GenerateCube();
        }

        proceduralObject = new GameObject(meshObject.name);
        proceduralObject.transform.SetParent(transform);
        proceduralObject.transform.localPosition = Vector3.zero;
        proceduralObject.transform.localRotation = Quaternion.identity;

        proceduralObject.AddComponent<MeshFilter>();
        MeshRenderer meshrenderer = proceduralObject.AddComponent<MeshRenderer>();
        meshrenderer.materials = new Material[] { WireframeMaterial, FaceMaterial };
    }


    void Update () {
        //proceduralObject.transform.Rotate(Vector3.up, 0.05f);

        Mesh mesh = proceduralObject.GetComponent<MeshFilter>().mesh;
        meshObject.UpdateMesh(mesh);
    }
}
