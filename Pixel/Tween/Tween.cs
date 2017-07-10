/*
 *  Tween library for Unity
 * 
 *	Copyright 2011-2017 Peter @sHTiF Stefcek. All rights reserved.
 *
 *	Ported from Genome2D framework (https://github.com/pshtif/Genome2D/)
 *	
 */

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Pixel.Geom;

namespace Pixel.Tween
{
    public class TweenCore : MonoBehaviour
    {
        static private bool _initialized = false;
        static private GameObject _core;
        static private TweenTimeline _currentTimeline;
        static private List<TweenTimeline> _timelines;

        static public TweenStep Create(GameObject p_target)
        {
            if (!_initialized) Initialize();
            TweenSequence sequence = TweenSequence.GetPoolInstance();

            if (_currentTimeline == null) AddTimeline(new TweenTimeline(), true);
            _currentTimeline.AddSequence(sequence);

            TweenStep step = sequence.AddStep(TweenStep.GetPoolInstance());
            step.target = p_target;

            sequence.Run();

            return step;
        }

        static private void AddTimeline(TweenTimeline p_timeline, bool p_setCurrent = false)
        {
            if (p_setCurrent) _currentTimeline = p_timeline;
            if (_timelines == null) _timelines = new List<TweenTimeline>();
            _timelines.Add(p_timeline);
        }

        static public void Initialize()
        {
            Debug.Log("Initialize TweenCore");            
            _initialized = true;
            _core = new GameObject();
            _core.name = "PixelTweenCore";
            _core.hideFlags = HideFlags.HideAndDontSave;
            _core.AddComponent<TweenCore>();
            GameObject.DontDestroyOnLoad(_core);
        }

        // Update is called once per frame
        void Update()
        {
            if (_timelines != null)
            {
                for (int i=0; i<_timelines.Count; ++i)
                {
                    _timelines[i].Update(Time.deltaTime);
                }
            }
        }
    }

    public class TweenStep
    {
        public TweenStep poolNext;
        static public TweenStep poolFirst;
        static public TweenStep GetPoolInstance()
        {
            TweenStep step = null;
            if (poolFirst == null)
            {
                step = new TweenStep();
            }
            else
            {
                step = poolFirst;
                poolFirst = poolFirst.poolNext;
                step.poolNext = null;
            }

            return step;
        }

        public TweenSequence sequence;
        public TweenStep previous;
        public TweenStep next;
        public GameObject target;
        public Func<float, float> ease;
        private bool _empty;
        private float _time;
        private float _duration;
        private Interp _interp;

        private Action<TweenStep> _onUpdate;
        private Action<TweenStep> _onComplete;

        public TweenStep()
        {
            ease = Linear.None;
            _time = _duration = 0;
            _empty = true;
        }

        public TweenStep OnComplete(Action<TweenStep> p_callback)
        {
            _onComplete = p_callback;
            return this;
        }

        public TweenStep OnUpdate(Action<TweenStep> p_callback)
        {
            _onUpdate = p_callback;
            return this;
        }

        public void Skip()
        {
            //for (interp in g2d_interps) interp.setValue(interp.getFinalValue());
            Finish();
        }

        private void Finish()
        {
            if (_onComplete != null) _onComplete.Invoke(this);
            sequence.NextStep();
            //if (g2d_interps != null) for (interp in g2d_interps) interp.reset();
        }

        private void Dispose()
        {
            sequence = null;
            previous = null;
            next = null;
            _time = _duration = 0;

            // Put back to pool
            poolNext = poolFirst;
            poolFirst = this;
        }

        public TweenStep Delay(float p_duration)
        {
            TweenStep step = _empty ? this : sequence.AddStep(GetPoolInstance());
            step._duration = p_duration;
            _empty = false;
            step = sequence.AddStep(GetPoolInstance());
            step.target = target;
            return step;
        }

        public TweenStep Create(GameObject p_target)
        {
            TweenStep step = sequence.AddStep(GetPoolInstance());
            step.target = p_target;

            return step;
        }

        public TweenStep Custom<T>(Interp<T> p_interp, T p_start, T p_target, float p_duration) where T : struct
        {
            TweenStep step = _empty ? this : sequence.AddStep(GetPoolInstance());
            step._duration = p_duration;
            p_interp.Start(target, p_start, p_target);
            step._interp = p_interp;
            _empty = false;
            return step;
        }

        public TweenStep Alpha(float p_alpha, float p_duration)
        {
            TweenStep step = _empty ? this : sequence.AddStep(GetPoolInstance());
            step._duration = p_duration;
            FloatInterp floatInterp = new AlphaInterp();
            floatInterp.Start(target, 1, p_alpha);
            step._interp = floatInterp;
            _empty = false;
            return step;
        }

        public TweenStep Scale(Vector3 p_position, float p_duration)
        {
            TweenStep step = _empty ? this : sequence.AddStep(GetPoolInstance());
            step._duration = p_duration;
            ScaleInterp vector3Interp = new ScaleInterp();
            vector3Interp.Start(target, target.transform.position, p_position);
            step._interp = vector3Interp;
            _empty = false;
            return step;
        }

        public TweenStep CurveScale(Curve3 p_curve, float p_duration)
        {
            TweenStep step = _empty ? this : sequence.AddStep(GetPoolInstance());
            step._duration = p_duration;
            ScaleCurve3Interp curveInterp = new ScaleCurve3Interp(p_curve);
            curveInterp.Start(target, target.transform.localScale, Vector3.one);
            step._interp = curveInterp;
            _empty = false;
            return step;
        }

        public TweenStep Rotation(Quaternion p_rotation, float p_duration)
        {
            TweenStep step = _empty ? this : sequence.AddStep(GetPoolInstance());
            step._duration = p_duration;
            RotationInterp interp = new RotationInterp();
            interp.Start(target, target.transform.rotation, p_rotation);
            step._interp = interp;
            _empty = false;
            return step;
        }

        public TweenStep Position(Vector3 p_position, float p_duration)
        {
            TweenStep step = _empty ? this : sequence.AddStep(GetPoolInstance());
            step._duration = p_duration;
            PositionInterp vector3Interp = new PositionInterp();
            vector3Interp.Start(target, target.transform.position, p_position);
            step._interp = vector3Interp;
            _empty = false;
            return step;
        }

        public TweenStep CurvePosition(Curve3 p_curve, float p_duration)
        {
            TweenStep step = _empty ? this : sequence.AddStep(GetPoolInstance());
            step._duration = p_duration;
            PositionCurve3Interp curveInterp = new PositionCurve3Interp(p_curve);
            curveInterp.Start(target, target.transform.position, Vector3.one);
            step._interp = curveInterp;
            _empty = false;
            return step;
        }

        public TweenStep CurvePosition(Curve1 p_curveX, Curve1 p_curveY, Curve1 p_curveZ, float p_duration)
        {
            TweenStep step = _empty ? this : sequence.AddStep(GetPoolInstance());
            step._duration = p_duration;
            PositionCurve1Interp curveInterp = new PositionCurve1Interp(p_curveX, p_curveY, p_curveZ);
            curveInterp.Start(target, target.transform.position, Vector3.one);
            step._interp = curveInterp;
            _empty = false;
            return step;
        }

        public TweenStep Ease(Func<float,float> p_ease)
        {
            ease = p_ease;
            return this;
        }

        public float Update(float p_delta)
        {
            float rest = 0;
            /**/
            _time += p_delta;
            if (_time >= _duration)
            {
                rest = _time - _duration;
                _time = _duration;
            }

            if (_interp != null)
            {
                //Debug.Log(ease(_time / _duration));
                _interp.Update(ease(_time / _duration));
            }

            if (_onUpdate != null)
            {
                _onUpdate.Invoke(this);
            }

            if (_time >= _duration)
            {
                Finish();
            }

            return rest;
        }
    }

    public class TweenSequence
    {
        private TweenSequence _poolNext;
        static private TweenSequence _poolFirst;
        static public TweenSequence GetPoolInstance()
        {
            TweenSequence sequence = null;
            if (_poolFirst == null) {
                sequence = new TweenSequence();
            } else {
                sequence = _poolFirst;
                _poolFirst = _poolFirst._poolNext;
                sequence._poolNext = null;
            }

            return sequence;
        }

        private TweenStep _firstStep;
        private TweenStep _currentStep;
        private TweenStep _lastStep;

        private int _stepCount = 0;
        private bool _running = false;
        public TweenTimeline timeline;
        private bool _complete = false;
        public bool IsComplete()
        {
            return _complete;
        }

        public void dispose()
        {
            _currentStep = null;
            _lastStep = null;
            _stepCount = 0;
            _complete = false;

            _poolNext = _poolFirst;
            _poolFirst = this;
        }

        public float Update(float p_delta)
        {
            if (!_running) return p_delta;

            float rest = p_delta;
            while (rest>0 && _currentStep != null) 
            {
                rest = UpdateCurrentStep(rest);
            }

            return rest;
        }

        private float UpdateCurrentStep(float p_delta)
        {
            float rest = p_delta;
            if (_currentStep == null) {
                Finish();
            } else {
                rest = _currentStep.Update(p_delta);
            }
            return rest;
        }

        private void Finish()
        {
            timeline.dirty = true;
            _complete = true;
        }

        public TweenStep AddStep(TweenStep p_tweenStep)
        {
            p_tweenStep.sequence = this;

            if (_currentStep == null) {
                _firstStep = _lastStep = _currentStep = p_tweenStep;
            } else {
                _lastStep.next = p_tweenStep;
                p_tweenStep.previous = _lastStep;
                _lastStep = p_tweenStep;
            }
            _stepCount++;
            return p_tweenStep;
        }

        public void NextStep()
        {
            _currentStep = _currentStep.next;
        }

        private void RemoveStep(TweenStep p_tweenStep) {
            _stepCount--;
            if (_firstStep == p_tweenStep) _firstStep = _firstStep.next;
            if (_currentStep == p_tweenStep) _currentStep = p_tweenStep.next;
            if (_lastStep == p_tweenStep) _lastStep = p_tweenStep.previous;
            if (p_tweenStep.previous != null) p_tweenStep.previous.next = p_tweenStep.next;
            if (p_tweenStep.next != null) p_tweenStep.next.previous = p_tweenStep.previous;
        }

        public void SkipCurrent()
        {
            if (_currentStep != null) _currentStep.Skip();
        }

        public void Run()
        {
            _running = true;
        }
    }   

    public class TweenTimeline
    {
        public bool dirty = false;
        private List<TweenSequence> _sequences;

        public TweenTimeline()
        {
           _sequences = new List<TweenSequence>();
        }

        public void AddSequence(TweenSequence p_sequence)
        {
            p_sequence.timeline = this;
            _sequences.Add(p_sequence);
        }

        public void Update(float p_delta)
        {
            for (int i = 0; i<_sequences.Count; ++i)
            {
                _sequences[i].Update(p_delta);
            }

            if (dirty)
            {
                int count = _sequences.Count;
                while (count-- > 0)
                {
                    TweenSequence sequence = _sequences[count];
                    if (sequence.IsComplete())
                    {
                        _sequences.Remove(sequence);
                        sequence.dispose();
                    }
                }
            }
        }
    }
}
