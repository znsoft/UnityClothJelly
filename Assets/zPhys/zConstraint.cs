using System;
using System.Collections.Generic;
using UnityEngine;

namespace zPhys
{
    public class zConstraint
    {
        public float ELASTICITY = 0.010f;
        public float FREEZE = 0.060f;
        public static float CONSTRAINTFRICTION = 0.57f;
        public static List<zConstraint> AllConstraints = new List<zConstraint>();
        public zUnit unit1;
        public zUnit unit2;

        public float minlength;

        public float length;
        public float dist;
        public float tearDist;// длина обрыва связи

        public static float SPACING = 0.9f;//0.9f; // длина спокойной (не сжатой и не растянутой) связи
        public static float SPRINGSTOP = 0.8f; // длина сжатой связи
        public static float TEARDIST = 15.5f;// длина обрыва связи
        //public static float GLUEDIST = 30.873f;// приклеивание
        public bool isHide = false;


        public zConstraint(zUnit unit1, zUnit unit2, bool ishide)
        {
            this.unit1 = unit1;
            this.unit2 = unit2;
            length = zConstraint.SPACING * unit1.getDistanceTo(unit2);
            AllConstraints.Add(this);
            tearDist = length * TEARDIST;
            isHide = ishide;
            minlength = length * SPRINGSTOP;
        }


        public zConstraint resolve()
        {
            Vector2 dpos = unit1.pos - unit2.pos;
            dist = dpos.magnitude;

            if (dist == this.length) return null;

            float diff = (length - dist) / dist;
            float mul = diff * CONSTRAINTFRICTION * (1 - this.length / dist);
            Vector2 delta = dpos * mul;
            
            if (dist <= this.length)
            {
                delta=delta*FREEZE;
                unit1.AddForce(-delta);
                unit2.AddForce(delta);
                if (dist <= minlength)
                {
                    unit1.stop();
                    unit2.stop();
                }
                return null;
            }
            if (dist > this.length)
            {
                delta = delta * ELASTICITY;
                unit1.AddForce(delta);
                unit2.AddForce(-delta);
                if (dist > tearDist)
                {
                    unit1.stop();
                    unit2.stop();
                }
            }
            return null;
        }

        internal static void ConnectUnits(zUnit z1, zUnit z2, bool isHide)
        {
            if (z1 == null) return;
            if (z2 == null) return;
            zConstraint c = new zConstraint(z1, z2, isHide);
            AllConstraints.Add(c);
        }



    }
}