using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

namespace zPhys { 
public class zUnit 
{
        public Vector2 startPos;
        public Vector2 pos;
        public Vector2 prevpos;
        public Vector2 force;
        public bool isPinned = false;

        float friction = 0.299f;
        float delta = 17.216f;
        float gravity = -0.00333f;
        float bounce = 0.5f;

        public zUnit(Vector3 v, bool isPin)
        {
            isPinned = isPin;
            pos = new Vector2(v.x, v.y);
            force = new Vector2(0.0f, 0.0f);
            prevpos = new Vector2(v.x, v.y);
            startPos = new Vector2(v.x, v.y);
        }


        public zUnit(float x, float y, bool isPin)
        {
            isPinned = isPin;
            pos = new Vector2(x, y);
            force = new Vector2(0.0f, 0.0f);
            prevpos = new Vector2(x, y);
            startPos = new Vector2(x, y);
        }


        public zUnit AddForce(Vector2 vec)
        {
            force+=vec;
            return this;
        }

        public zUnit update()
        {
            if (isPinned) return this;
            AddForce(new Vector2(0, gravity));
            Vector2 npos = (pos - prevpos) * friction + force * delta;
            float dist = npos.magnitude;
            npos += pos;

            prevpos = pos;
            if (dist < zConstraint.TEARDIST)
            pos = npos;
            force = new Vector2(0.0f, 0.0f);
            return this;
        }

        public zUnit stop() {
            pos = new Vector2(prevpos.x, prevpos.y);
            return this;

        }

            internal float getDistanceTo(zUnit unit2)
        {
            return (pos - unit2.pos).magnitude;
        }
    }
}