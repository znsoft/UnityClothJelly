using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using zPhys;

public class NewBehaviourScript : MonoBehaviour {
    zUnit[] zList;
    Vector3[] vertices;
    Mesh mesh,colliderMesh;

    void Start () {
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        zList = new zUnit[vertices.Length];
        zUnit old = null;
        int i = 0;
        while (i < vertices.Length)
        {
            zUnit z = new zUnit(vertices[i].x, vertices[i].z,  old==null);
            zList[i]=z;
            //if (i % 11 == 0) old = null;
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
           // Debug.DrawLine(c.unit1.pos, c.unit2.pos);
        }

        
        int i = 0;
        while (i < zList.Length)
        {
            zUnit z = zList[i];
            z.update(new Vector2(10,10));
            Vector3 v = new Vector3(z.pos.x, vertices[i].y, z.pos.y);//,vertices[i].z+0.1f);
            vertices[i] = v;
            i++;
        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
       

    }
}
