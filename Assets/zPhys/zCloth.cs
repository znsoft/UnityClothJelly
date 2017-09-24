﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using zPhys;

public class zCloth : MonoBehaviour {
    zUnit[] zList;
    Vector3[] vertices;
    Mesh mesh,colliderMesh;

    void Start () {
        mesh = GetComponent<MeshFilter>().mesh;
        colliderMesh = GetComponent<MeshCollider>().GetComponent<MeshFilter>().mesh;

        vertices = mesh.vertices;
        zList = new zUnit[vertices.Length];
        zUnit old = null;
        int i = 0;
        while (i < vertices.Length)
        {

            zUnit z = new zUnit(vertices[i].x, vertices[i].z,  i==0||i==10);
            zList[i]=z;
            //if (i % 11 > 0);
                zConstraint.ConnectUnits(z, old, false);
            if(i >= 11)zConstraint.ConnectUnits(z, zList[i-11], false);
            if (i >= 10) zConstraint.ConnectUnits(z, zList[i - 10], false);
            if (i >= 12) zConstraint.ConnectUnits(z, zList[i - 12], false);
            old = z;
            i++;
        }

    }

	
	void Update () {
        foreach (var c in zConstraint.AllConstraints) {
            c.resolve();
            Debug.DrawLine(c.unit1.pos, c.unit2.pos);
        }

        
        int i = 0;
        while (i < zList.Length)
        {
            zUnit z = zList[i];
            z.update();
            Vector3 v = new Vector3(z.pos.x, vertices[i].y, z.pos.y);//,vertices[i].z+0.1f);
            vertices[i] = v;
            i++;
        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        colliderMesh.vertices = vertices;
        colliderMesh.RecalculateBounds();
    }
}
