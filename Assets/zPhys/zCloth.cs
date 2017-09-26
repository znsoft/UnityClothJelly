using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using zPhys;

public class zCloth : MonoBehaviour {
    zUnit[] zList;
    Vector3[] vertices;
    Vector2[] cVertices;
    Mesh mesh;
    EdgeCollider2D colliderMesh;
    public int xSections = 10;
    List<zUnit> edges; 

    void Start ()
    {

        GenerateClothFromCurrentFigure();

    }

    private void GenerateClothFromCurrentFigure()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        colliderMesh = GetComponent<EdgeCollider2D>();
        int xDiv = xSections + 1;
        vertices = mesh.vertices;
        zList = new zUnit[vertices.Length];
        zUnit old = null;
        zUnit lastEdge = null;
        int i = 0;
        while (i < vertices.Length)
        {

            zUnit z = new zUnit(vertices[i].x, vertices[i].z, false);// i == 0 || i == xSections);
            zList[i] = z;

            if (i % xDiv > 0) { zConstraint.ConnectUnits(z, old, false); } else z.isEdge = true;
            if (i >= xDiv) { zConstraint.ConnectUnits(z, zList[i - xDiv], false); } else z.isEdge = true;
            if (i >= xSections && i % xDiv < xSections) { zConstraint.ConnectUnits(z, zList[i - xSections], false); } else z.isEdge = true;
            if (i >= xDiv + 1 && i % xDiv > 0) { zConstraint.ConnectUnits(z, zList[i - xDiv - 1], false); } else z.isEdge = true;
            if (vertices.Length < i + 10) z.isEdge = true;
            old = z;
            if (z.isEdge) lastEdge = z;
            i++;
        }

        edges = new List<zUnit>();

        FindEdgesRecursive(lastEdge);

        cVertices = new Vector2[edges.Count];
    }


    void FindEdgesRecursive(zUnit z) {
        foreach (var o in z.connectedTo)
        {
            if (!o.Key.isEdge) continue;
            o.Value.isEdge = true;
            if (edges.Contains(o.Key))continue;
            edges.Add(o.Key);
            FindEdgesRecursive(o.Key);
            //break;
  
        }

    }


        void Update () {
        float delta = Time.deltaTime;
        foreach (var c in zConstraint.AllConstraints) {
            c.resolve(delta);
            if(c.isEdge)Debug.DrawLine(c.unit1.pos, c.unit2.pos);
        }




        int i = 0;
        while (i < zList.Length)
        {
            zUnit z = zList[i];
            z.update(delta);
            Vector3 v = new Vector3(z.pos.x, z.pos.y, 0);//,vertices[i].z+0.1f);
            vertices[i] = v;
            i++;
        }

        i = 0;
        foreach (var c in edges)
            cVertices[i++] = c.pos;

        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        colliderMesh.points = cVertices;
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        // Debug.Log(coll.gameObject.name);
        RenderCollisions(coll);
    }

    private void RenderCollisions(Collision2D coll)
    {
        foreach (var c in coll.contacts)
        {
            zUnit z = FindZUnit(edges, c.point);
            Vector2 f = c.relativeVelocity * 0.001f * c.rigidbody.mass + new Vector2(0,-0.001f * c.rigidbody.mass);
            z.AddForce(f);

        }
    }

    void OnCollisionStay2D(Collision2D coll)
    {
        RenderCollisions(coll);
    }

    private zUnit FindZUnit(List<zUnit> edges, Vector2 point)
    {
        zUnit result = null;
        float minDist = float.MaxValue;
        foreach (var unit in edges) {
            float near = (unit.pos - point).magnitude;
            if (near < minDist) { minDist = near; result = unit; }
        }
        return result;
    }
}
