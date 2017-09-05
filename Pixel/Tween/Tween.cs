/*
 *  Tween library for Unity
 * 
 *	Copyright 2011-2017 Peter @sHTiF Stefcek. All rights reserved.
 *
 *	Ported from Genome2D framework (https://github.com/pshtif/Genome2D/)
 *	
 *	NOTE: The API differs from original Haxe Genome2D framework due to language differences and Unity's approach to structures
 *	
 */
 #define PIXEL_GEOM

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
#if PIXEL_GEOM
using Pixel.Geom;
#endif

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
            Debug.Log("Initialize Pixel Tween Core");            
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
        public string stepId;

        private bool _empty;
        private float _time;
        private float _duration;
        private List<Interp> _interps;
        private Interp _lastInterp;
        private string _gotoStepId = "";
        private int _gotoRepeatCount = 0;
        private int _gotoCurrentCount = 0;

        private Action<TweenStep> _onUpdate;
        private Action<TweenStep> _onComplete;

        public TweenStep()
        {
            _interps = new List<Interp>();
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
            Reset();
            if (sequence != null) {
                if (_gotoCurrentCount < _gotoRepeatCount)
                {
                    _gotoCurrentCount++;
                    sequence.GoTo(sequence.GetStepById(_gotoStepId));
                }
                else
                {
                    _gotoCurrentCount = 0;
                    if (_onComplete != null) _onComplete.Invoke(this);
                    sequence.NextStep();
                }
            }
        }

        private void Reset()
        {
            _time = 0;
            if (_interps != null) foreach (Interp interp in _interps) interp.Reset();
        }

        private void Dispose()
        {
            _interps = new List<Interp>();
            _empty = true;
            _time = _duration = 0;

            // Clean references
            sequence = null;
            previous = null;
            next = null;

            // Put back to pool
            poolNext = poolFirst;
            poolFirst = this;
        }

        public TweenStep id(string p_id)
        {
            stepId = p_id;
            return this;
        }

        public TweenStep Extend()
        {
            TweenStep step = sequence.AddStep(GetPoolInstance());
            step.target = target;
            return step;
        }

        public TweenStep Delay(float p_duration)
        {
            TweenStep step = _empty ? this : sequence.AddStep(GetPoolInstance());
            step._duration = p_duration;
            step._empty = false;
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

        public TweenStep Ease(Func<float, float> p_ease, bool p_allInterps = false)
        {
            if (p_allInterps)
            {
                if (_interps != null)
                {
                    foreach (Interp interp in _interps)
                    {
                        interp.ease = p_ease;
                    }
                }
            }
            else
            {
                if (_lastInterp != null) _lastInterp.ease = p_ease;
            }
            return this;
        }

        public TweenStep GoTo(string p_stepId, int p_repeatCount)
        {
            _gotoStepId = p_stepId;
            _gotoRepeatCount = p_repeatCount;

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

            if (_interps != null)
            {
                foreach (Interp interp in _interps)
                {
                    if (_time <= interp.duration) interp.Update(interp.ease(_time / interp.duration));
                }
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

        private void AddInterp(Interp p_interp)
        {
            _duration = Mathf.Max(_duration, p_interp.duration);
            _interps.Add(p_interp);
            _lastInterp = p_interp;
            _empty = false;
        }

        /****************************************************************************************************
         *  Interpolators
         ****************************************************************************************************/

        public TweenStep Custom<T>(Interp<T> p_interp, T p_start, T p_target, float p_duration) where T : struct
        {
            p_interp.duration = p_duration;
            p_interp.Init(target, p_target, false);
            AddInterp(p_interp);
            return this;
        }

        public TweenStep Alpha(float p_to, float p_duration, bool p_relative = false)
        {
            FloatInterp interp = new AlphaInterp();
            interp.duration = p_duration;
            interp.Init(target, p_to, p_relative);
            AddInterp(interp);
            return this;
        }

        public TweenStep Alpha(float p_from, float p_to, float p_duration, bool p_relative = false)
        {
            FloatInterp interp = new AlphaInterp();
            interp.duration = p_duration;
            interp.Init(target, p_to, p_from, p_relative);
            AddInterp(interp);
            return this;
        }

        public TweenStep Scale(Vector3 p_to, float p_duration, bool p_relative = false)
        {
            ScaleInterp interp = new ScaleInterp();
            interp.duration = p_duration;
            interp.Init(target, p_to, p_relative);
            AddInterp(interp);
            return this;
        }

        public TweenStep Scale(Vector3 p_from, Vector3 p_to, float p_duration, bool p_relative = false)
        {
            ScaleInterp interp = new ScaleInterp();
            interp.duration = p_duration;
            interp.Init(target, p_to, p_from, p_relative);
            AddInterp(interp);
            return this;
        }

#if PIXEL_GEOM
        public TweenStep CurveScale(Curve3 p_curve, float p_duration)
        {
            ScaleCurve3Interp interp = new ScaleCurve3Interp(p_curve);
            interp.duration = p_duration;
            interp.Init(target, Vector3.one, false);
            AddInterp(interp);
            return this;
        }

        public TweenStep CurveScale(Vector3 p_from, Curve3 p_curve, float p_duration)
        {
            ScaleCurve3Interp interp = new ScaleCurve3Interp(p_curve);
            interp.duration = p_duration;
            interp.Init(target, Vector3.one, p_from, false);
            AddInterp(interp);
            return this;
        }

        public TweenStep CurvePosition(Curve3 p_curve, float p_duration)
        {
            PositionCurve3Interp interp = new PositionCurve3Interp(p_curve);
            interp.duration = p_duration;
            interp.Init(target, Vector3.one, false);
            AddInterp(interp);
            return this;
        }

        public TweenStep CurvePosition(Vector3 p_from, Curve3 p_curve, float p_duration)
        {
            PositionCurve3Interp interp = new PositionCurve3Interp(p_curve);
            interp.duration = p_duration;
            interp.Init(target, Vector3.one, p_from, false);
            AddInterp(interp);
            return this;
        }

        public TweenStep CurvePosition(Curve1 p_curveX, Curve1 p_curveY, Curve1 p_curveZ, float p_duration)
        {
            PositionCurve1Interp interp = new PositionCurve1Interp(p_curveX, p_curveY, p_curveZ);
            interp.duration = p_duration;
            interp.Init(target, Vector3.one, false);
            AddInterp(interp);
            return this;
        }

        public TweenStep CurvePosition(Vector3 p_from, Curve1 p_curveX, Curve1 p_curveY, Curve1 p_curveZ, float p_duration)
        {
            PositionCurve1Interp interp = new PositionCurve1Interp(p_curveX, p_curveY, p_curveZ);
            interp.duration = p_duration;
            interp.Init(target, Vector3.one, p_from, false);
            AddInterp(interp);
            return this;
        }
#endif

        public TweenStep Rotation(Quaternion p_to, float p_duration, bool p_relative = false)
        {
            RotationInterp interp = new RotationInterp();
            interp.duration = p_duration;
            interp.Init(target, p_to, p_relative);
            AddInterp(interp);
            return this;
        }

        public TweenStep Rotation(Quaternion p_from, Quaternion p_to, float p_duration, bool p_relative = false)
        {
            RotationInterp interp = new RotationInterp();
            interp.duration = p_duration;
            interp.Init(target, p_to, p_from, p_relative);
            AddInterp(interp);
            return this;
        }

        public TweenStep Position(Vector3 p_to, float p_duration, bool p_relative = false)
        {
            PositionInterp interp = new PositionInterp();
            interp.duration = p_duration;
            interp.Init(target, p_to, p_relative);
            AddInterp(interp);
            return this;
        }

        public TweenStep Position(Vector3 p_from, Vector3 p_to, float p_duration, bool p_relative =  false)
        {
            PositionInterp interp = new PositionInterp();
            interp.duration = p_duration;
            interp.Init(target, p_to, p_from, p_relative);
            AddInterp(interp);
            return this;
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

        public TweenStep GetStepById(string p_stepId)
        {
            TweenStep step = _firstStep;

            if (p_stepId != "") {
                while (step != null) {
                    if (step.stepId == p_stepId) break;
                    else step = step.next;
                }
            }

            return step;
        }

        public void GoTo(TweenStep p_step)
        {
            _currentStep = p_step;
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
