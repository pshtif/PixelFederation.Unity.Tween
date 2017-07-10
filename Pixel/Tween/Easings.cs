/*
 *  Tween library for Unity
 * 
 *	Copyright 2011-2017 Peter @sHTiF Stefcek. All rights reserved.
 *
 *	Ported from Genome2D framework (https://github.com/pshtif/Genome2D/)
 *	
 *	Easings
 */

using UnityEngine;
using System.Collections;
using System;

namespace Pixel.Tween
{
    public class Bounce
    {
        static public float EaseIn(float p_t)
        {
            throw new System.InvalidOperationException("Not implemented yet.");
        }

        static public float EaseOut(float p_t)
        {
            if (p_t < (1 / 2.75))
            {
                return (7.5625f * p_t * p_t);
            }
            else if (p_t < (2 / 2.75))
            {
                return (7.5625f * (p_t -= (1.5f / 2.75f)) * p_t + .75f);
            }
            else if (p_t < (2.5 / 2.75))
            {
                return (7.5625f * (p_t -= (2.25f / 2.75f)) * p_t + .9375f);
            }
            else
            {
                return (7.5625f * (p_t -= (2.625f / 2.75f)) * p_t + .984375f);
            }
        }

        static public float EaseInOut(float p_t)
        {
            if (p_t < .5)
            {
                return EaseIn(p_t * 2) * .5f;
            }
            else
            {
                return EaseOut(p_t * 2 - 1) * .5f + .5f;
            }
        }
    }

    public class Cubic
    {
        static public float EaseIn(float p_t)
        {
		    return p_t * p_t * p_t;
	    }

        static public float EaseOut(float p_t)
        {
		    return ((p_t -= 1) * p_t * p_t + 1);
	    }

        static public float EaseInOut(float p_t)
        {
		    if ((p_t *= 2) < 1) {
			    return .5f * p_t * p_t * p_t;
		    }else {
			    return .5f * ((p_t -= 2) * p_t * p_t + 2);
		    }
	    }
    }

    public class Back
    {
        static public float DRIVE = 1.70158f;

	    static public float EaseIn(float p_t)
        {
		    return p_t* p_t * ((DRIVE + 1) * p_t - DRIVE);
	    }

        static public float EaseOut(float p_t)
        {
		    return ((p_t -= 1) * p_t * ((DRIVE + 1) * p_t + DRIVE) + 1);
	    }

        static public float EaseInOut(float p_t)
        {
		    float s = DRIVE * 1.525f;
		    if ((p_t*=2) < 1) return 0.5f * (p_t* p_t * (((s) + 1) * p_t - s));
		    return .5f * ((p_t -= 2) * p_t * (((s) + 1) * p_t + s) + 2);
	    }	
    }

    public class Expo
    {
        static public float EaseIn(float p_t)
        {
		    return p_t == 0 ? 0 : Mathf.Pow(2, 10 * (p_t - 1));
	    }

        static public float EaseOut(float p_t)
        {
		    return p_t == 1 ? 1 : (1 - Mathf.Pow(2, -10 * p_t));
	    }

        static public float EaseInOut(float p_t)
        {
		    if (p_t == 0 || p_t == 1) return p_t;

		    if ((p_t *= 2.0f) < 1.0) {
			    return 0.5f * Mathf.Pow(2, 10 * (p_t - 1));
		    }
		    return .5f * (2 - Mathf.Pow(2, -10 * --p_t));
	    }	
    }

    public class Quad
    {
        static public float EaseIn(float p_t)
        {
		    return p_t * p_t;
        }

        static public float EaseOut(float p_t)
        {
		    return -p_t * (p_t - 2);
	    }

        static public float EaseInOut(float p_t)
        {
		    p_t *= 2;
		    if (p_t< 1) {
			    return .5f * p_t * p_t;
		    }
		    return -.5f * ((p_t - 1) * (p_t - 3) - 1);
	    }
    }

    public class Quart
    {
        static public float EaseIn(float p_t)
        {
		    return p_t * p_t * p_t * p_t;
        }

        static public float EaseOut(float p_t)
        {
		    return -((p_t -= 1) * p_t * p_t * p_t - 1);
	    }
    
        static public float EaseInOut(float p_t)
        {
		    p_t *= 2;
		    if (p_t< 1) {
			    return .5f * p_t * p_t * p_t * p_t;
		    }
		    return -.5f * ((p_t -= 2) * p_t * p_t * p_t - 2);
	    }
    }

    public class Quint
    {

        static public float EaseIn(float p_t)
        {
		    return p_t * p_t * p_t * p_t * p_t;
	    }

        static public float EaseOut(float p_t)
        {
		    return ((p_t -= 1) * p_t * p_t * p_t * p_t + 1);
	    }

        static public float EaseInOut(float p_t)
        {
		    p_t *= 2;
		    if (p_t< 1) {
			    return .5f * p_t * p_t * p_t * p_t * p_t;
		    }
		    return .5f * ((p_t -= 2) * p_t * p_t * p_t * p_t + 2);
	    }	
    }

    public class Sine
    {
        static public float EaseIn(float p_t)
        {
		    return -Mathf.Cos(p_t* (Mathf.PI / 2));
	    }

        static public float EaseOut(float p_t)
        {
		    return Mathf.Sin(p_t* (Mathf.PI / 2));
	    }

        static public float EaseInOut(float p_t)
        {
		    return -0.5f * (Mathf.Cos(Mathf.PI* p_t) - 1);
	    }
    }

    public class Linear
    {
        static public float None(float p_t)
        {
            return p_t;
        }
    }
}
