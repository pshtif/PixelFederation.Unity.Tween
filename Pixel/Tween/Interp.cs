/*
 *  Tween library for Unity
 * 
 *	Copyright 2011-2017 Peter @sHTiF Stefcek. All rights reserved.
 *
 *	Ported from Genome2D framework (https://github.com/pshtif/Genome2D/)
 *	
 *	Interpolators
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Pixel.Geom;

namespace Pixel.Tween
{
    public class Interp
    {
        virtual public void Update(float p_progress) { }
    }

    public class Interp<T> : Interp where T : struct
    {
        protected GameObject _target;
        public GameObject Target { get { return _target; } }

        protected T _start;
        protected T _to;

        protected T _current;
        public T Current { get { return _current; } }

        private readonly Func<T, T, float, T> _interpFunc;

        public Interp(Func<T, T, float, T> p_interpFunc)
        {
            _interpFunc = p_interpFunc;
        }

        virtual public void Start(GameObject p_target, T p_start, T p_to)
        {
            _target = p_target;
            _start = p_start;
            _to = p_to;
        }

        override public void Update(float p_progress)
        {
            _current = _interpFunc(_start, _to, p_progress);
        }
    }

    public class FloatInterp : Interp<float>
    {
        private static float InterpFloat(float p_start, float p_end, float p_progress) { return Mathf.Lerp(p_start, p_end, p_progress); }

        public FloatInterp() : base(InterpFloat) { }
    }

    public class Vector3Interp : Interp<Vector3>
    {
        private static Vector3 InterpVector3(Vector3 p_start, Vector3 p_end, float p_progress) { return Vector3.Lerp(p_start, p_end, p_progress); }

        public Vector3Interp() : base(InterpVector3) { }
    }

    public class QuaternionInterp : Interp<Quaternion>
    {
        private static Quaternion InterpQuaternion(Quaternion p_start, Quaternion p_end, float p_progress) { return Quaternion.Lerp(p_start, p_end, p_progress); }

        public QuaternionInterp() : base(InterpQuaternion) { }
    }

    public class AlphaInterp : FloatInterp
    {
        override public void Update(float p_progress)
        {
            base.Update(p_progress);
            if (Target.GetComponent<SpriteRenderer>() != null)
            {
                Color c = Target.GetComponent<SpriteRenderer>().color;
                c.a = Current;
                Target.GetComponent<SpriteRenderer>().color = c;
            }
        }
    }

    public class RotationInterp : QuaternionInterp
    {
        override public void Update(float p_progress)
        {
            base.Update(p_progress);
            Target.transform.rotation = Current;
        }
    }

    public class ScaleInterp : Vector3Interp
    {
        override public void Update(float p_progress)
        {
            base.Update(p_progress);
            Target.transform.localScale = Current;
        }
    }

    public class PositionInterp : Vector3Interp
    {
        override public void Update(float p_progress)
        {
            base.Update(p_progress);
            Target.transform.position = Current;
        }
    }

    public class PositionCurve1Interp : Interp<Vector3>
    {
        private Curve1 _curveX;
        private Curve1 _curveY;
        private Curve1 _curveZ;

        public PositionCurve1Interp(Curve1 p_curveX, Curve1 p_curveY, Curve1 p_curveZ) : base(null) {
            _curveX = p_curveX;
            _curveY = p_curveY;
            _curveZ = p_curveZ;
        }

        override public void Update(float p_progress)
        {
            float cx = _start.x + (_curveX != null ? _curveX.Calculate(p_progress) : 0);
            float cy = _start.y + (_curveY != null ? _curveY.Calculate(p_progress) : 0);
            float cz = _start.z + (_curveZ != null ? _curveZ.Calculate(p_progress) : 0);

            Target.transform.position = new Vector3(cx, cy, cz);
        }
    }

    public class PositionCurve3Interp : Interp<Vector3>
    {
        private Curve3 _curve;

        public PositionCurve3Interp(Curve3 p_curve) : base(null) {
            _curve = p_curve;
        }

        override public void Update(float p_progress)
        {
            Vector3 cx = _start + _curve.Calculate(p_progress);

            Target.transform.position = cx;
        }
    }

    public class ScaleCurve3Interp : Interp<Vector3>
    {
        private Curve3 _curve;

        public ScaleCurve3Interp(Curve3 p_curve) : base(null) {
            _curve = p_curve;
        }

        override public void Update(float p_progress)
        {
            Vector3 cx = _start + _curve.Calculate(p_progress);

            Target.transform.localScale = cx;
        }
    }
}
