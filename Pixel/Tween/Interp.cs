/*
 *  Tween library for Unity
 * 
 *	Copyright 2011-2017 Peter @sHTiF Stefcek. All rights reserved.
 *
 *	Ported from Genome2D framework (https://github.com/pshtif/Genome2D/)
 *	
 *	Interpolators
 */

#define PIXEL_GEOM

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
#if PIXEL_GEOM
using Pixel.Geom;
#endif

namespace Pixel.Tween
{
    public class Interp
    {
        public Func<float, float> ease;
        public float duration;

        protected bool _relative = false;
        protected bool _reflectFrom = false;
        protected bool _initialized = false;

        virtual public void Update(float p_progress) { }

        public void Reset()
        {
            _initialized = false;
        }
    }

    public class Interp<T> : Interp where T : struct
    {
        protected GameObject _target;
        public GameObject Target { get { return _target; } }

        protected T _from;
        protected T _to;

        protected T _start;
        protected T _end;

        protected T _current;
        public T Current { get { return _current; } }

        private readonly Func<T, T, float, T> _interpFunc;

        public Interp(Func<T, T, float, T> p_interpFunc)
        {
            ease = Linear.None;
            _interpFunc = p_interpFunc;
        }

        virtual public void Init(GameObject p_target, T p_to, bool p_relative)
        {
            _target = p_target;
            _to = p_to;
            _relative = p_relative;
            _reflectFrom = true;
        }

        virtual public void Init(GameObject p_target, T p_to, T p_from, bool p_relative)
        {
            _target = p_target;
            _to = p_to;
            _relative = p_relative;
            _reflectFrom = false;
        }

        override public void Update(float p_progress)
        {
            _current = _interpFunc(_start, _end, p_progress);
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
            if (!_initialized)
            {
                _start = _reflectFrom ? Target.GetComponent<SpriteRenderer>().color.a : _from;
                _end = _relative ? _start + _to : _to;
                
                _initialized = true;
            }
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
            if (!_initialized)
            {
                _start = _reflectFrom ? Target.transform.rotation : _from;
                _end = _relative ? _start * _to : _to;
                _initialized = true;
            }
            base.Update(p_progress);
            Target.transform.rotation = Current;
        }
    }

    public class ScaleInterp : Vector3Interp
    {
        override public void Update(float p_progress)
        {
            if (!_initialized)
            {
                _start = _reflectFrom ? Target.transform.localScale : _from;
                _end = _relative ? _start + _to : _to;
                _initialized = true;
            }
            base.Update(p_progress);
            Target.transform.localScale = Current;
        }
    }

    public class PositionInterp : Vector3Interp
    {
        override public void Update(float p_progress)
        {
            if (!_initialized)
            {
                _start = _reflectFrom ? Target.transform.position : _from;
                _end = _relative ? _start + _to : _to;
                _initialized = true;
            }
            base.Update(p_progress);
            Target.transform.position = Current;
        }
    }

    #if PIXEL_GEOM
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
            if (!_initialized)
            {
                _start = _reflectFrom ? Target.transform.position : _from;
                _initialized = true;
            }

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
            if (!_initialized)
            {
                _start = _reflectFrom ? Target.transform.position : _from;
                _initialized = true;
            }

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
            if (!_initialized)
            {
                _start = _reflectFrom ? Target.transform.localScale : _from;
                _initialized = true;
            }

            Vector3 cx = _start + _curve.Calculate(p_progress);

            Target.transform.localScale = cx;
        }
    }
    #endif
}
