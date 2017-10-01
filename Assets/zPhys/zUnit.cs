using UnityEngine;

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
        public bool useStop = true;
        public bool isEdge = false;

        float friction = 0.65299f;
        float delta = 0.216f;
        
        
        
        public Dictionary<zUnit, zConstraint> connectedTo  = new Dictionary<zUnit, zConstraint>();

        public zUnit(Vector3 v, bool isPin, float friction)
        {
            isPinned = isPin;
            pos = new Vector2(v.x, v.y);
            force = new Vector2(0.0f, 0.0f);
            prevpos = new Vector2(v.x, v.y);
            startPos = new Vector2(v.x, v.y);
            this.friction = friction;
        }


        public zUnit(float x, float y, bool isPin, float friction)
        {
            isPinned = isPin;
            pos = new Vector2(x, y);
            force = new Vector2(0.0f, 0.0f);
            prevpos = new Vector2(x, y);
            startPos = new Vector2(x, y);
            this.friction = friction;
        }


        public zUnit AddForce(Vector2 vec)
        {
            force+=vec;
            return this;
        }

        public zUnit update(float deltaTime)
        {
            if (isPinned) return this;
            Vector2 npos = (pos - prevpos) * friction + force * delta * deltaTime;
            float dist = npos.magnitude;
            npos += pos;

            prevpos = pos;
           // if (dist < zConstraint.TEARDIST)
            pos = npos;
            force = new Vector2(0.0f, 0.0f);
            return this;
        }

        public zUnit stop() {
            if(useStop) pos = new Vector2(prevpos.x, prevpos.y);
            return this;

        }

            internal float getDistanceTo(zUnit unit2)
        {
            return (pos - unit2.pos).magnitude;
        }
    }
}