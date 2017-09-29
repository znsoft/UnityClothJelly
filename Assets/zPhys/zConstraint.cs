using System;
using System.Collections.Generic;
using UnityEngine;

namespace zPhys
{
    public class zConstraint
    {
        public float ELASTICITY = 0.010f;
        public float FREEZE = 0.060f;
        public float CONSTRAINTFRICTION = 0.57f;

        public zUnit unit1;
        public zUnit unit2;
        public bool isEdge = false;

        public float minlength;

        public float length;
        public float dist;
        public float tearDist;// длина обрыва связи

        public float SPACING = 0.8f;//0.9f; // длина спокойной (не сжатой и не растянутой) связи
        public float SPRINGSTOP = 0.4f; // длина сжатой связи
        public static float TEARDIST = 15.5f;// длина обрыва связи
        public bool isHide = false;


        public zConstraint(zUnit unit1, zUnit unit2, bool ishide)
        {
            this.unit1 = unit1;
            this.unit2 = unit2;
            length = SPACING * unit1.getDistanceTo(unit2);
            
            tearDist = length * TEARDIST;
            isHide = ishide;
            minlength = length * SPRINGSTOP;
            unit1.connectedTo.Add(unit2, this);
            unit2.connectedTo.Add(unit1, this);
        }


        public zConstraint resolve(float deltaTime)
        {
            var dpos = unit1.pos - unit2.pos;
            dist = dpos.magnitude;

            if (dist==0.0f||dist == this.length) return null;

            float diff = (length - dist) / dist;
            float mul = diff * CONSTRAINTFRICTION * (1 - this.length / dist);
            var delta = dpos * mul;
            
            if (dist <= this.length)
                delta=delta * -FREEZE; else  delta = delta * ELASTICITY;
            moveUnits(delta);
            return null;
        }

        private void moveUnits(Vector2 delta)
        {
            unit1.AddForce(delta);
            unit2.AddForce(-delta);
            if (dist > tearDist||dist< minlength)
            {
                unit1.stop();
                unit2.stop();
            }
        }




    }
}