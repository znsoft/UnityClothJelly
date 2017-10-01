using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using zPhys;

public class zCloth : MonoBehaviour
{
    zUnit[] zList;
    Vector3[] vertices;
    Vector2[] cVertices;
    Mesh mesh;
    EdgeCollider2D colliderMesh;
    public int xSections = 10;
    public float friction = 0.5f;
    public float elastic = 0.010f;
    public float freeze = 0.060f;
    zUnit selected;
    List<zUnit> edges;

    void Start()
    {
        GenerateClothFromCurrentFigure();

    }

    List<zConstraint> AllConstraints = new List<zConstraint>();
    void ConnectUnits(zUnit z1, zUnit z2, bool isHide)
    {
        if (z1 == null) return;
        if (z2 == null) return;
        if (z1.connectedTo.ContainsKey(z2)) return;
        if (z2.connectedTo.ContainsKey(z1)) return;
        zConstraint c = new zConstraint(z1, z2, isHide);
        c.ELASTICITY = elastic;
        c.FREEZE = freeze;
        AllConstraints.Add(c);
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

            zUnit z = new zUnit(vertices[i].x, vertices[i].z, false, friction);// i == 0 || i == xSections);
            zList[i] = z;

            if (i % xDiv > 0) { ConnectUnits(z, old, false); } else z.isEdge = true;
            if (i >= xDiv) { ConnectUnits(z, zList[i - xDiv], false); } else z.isEdge = true;
            if (i >= xSections && i % xDiv < xSections) { ConnectUnits(z, zList[i - xSections], false); } else z.isEdge = true;
            if (i >= xDiv + 1 && i % xDiv > 0) { ConnectUnits(z, zList[i - xDiv - 1], false); } else z.isEdge = true;
            if (vertices.Length < i + 10) z.isEdge = true;
            old = z;
            if (z.isEdge) lastEdge = z;
            i++;
        }

        edges = new List<zUnit>();

        FindEdgesRecursive(lastEdge);

        cVertices = new Vector2[edges.Count];
    }


    void FindEdgesRecursive(zUnit z)
    {
        foreach (var o in z.connectedTo)
        {
            if (!o.Key.isEdge) continue;
            o.Value.isEdge = true;
            if (edges.Contains(o.Key)) continue;
            edges.Add(o.Key);
            FindEdgesRecursive(o.Key);
        }

    }


    void Update()
    {
        float delta = Time.deltaTime;
        //delta = 0.1f;
        if (delta > 0) delta = 1 / delta;
        foreach (var c in AllConstraints)
        {
            c.resolve(delta);
            if (c.isEdge) Debug.DrawLine(c.unit1.pos, c.unit2.pos);
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


    public void Hit(Vector2 point)
    {
        zUnit hitUnit;
        Vector2 local = transform.InverseTransformPoint(point);
        if (selected == null)
        {
            List<zUnit> tmp = new List<zUnit>();
            tmp.AddRange(zList);
            hitUnit = FindZUnit(tmp, local);
            hitUnit.isPinned = true;
        }
        else
        {
            hitUnit = selected;
        }
        if (hitUnit != null) { hitUnit.pos = local; selected = hitUnit; }
    }
    public void UnHit()
    {
        if (selected != null) selected.isPinned = false;
        selected = null;
    }


    void OnCollisionEnter2D(Collision2D coll)
    {
        RenderCollisions(coll);
    }

    private void RenderCollisions(Collision2D coll)
    {
        foreach (var c in coll.contacts)
        {
            Vector2 local = transform.InverseTransformPoint(c.point);
            zUnit z = FindZUnit(edges, local);
            Vector2 f = c.relativeVelocity * 0.001f * c.rigidbody.mass + c.normal * 0.001f * c.rigidbody.mass;
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
        foreach (var unit in edges)
        {
            float near = (unit.pos - point).magnitude;
            if (near < minDist) { minDist = near; result = unit; }
        }
        return result;
    }
}
