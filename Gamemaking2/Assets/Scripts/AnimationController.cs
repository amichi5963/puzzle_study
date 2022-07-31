using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController
{
    const float DELTA_TIME_MAX = 1.0f;
    int _time = 0;
    float _inv_time_max = 1.0f;

    public void Set(int max_time)
    {
        Debug.Assert(max_time > 0.0f);

        _time = max_time;
        _inv_time_max = 1.0f / (float)max_time;
    }

    //アニメーション中ならtrueを返す
    public bool Update()
    {
        _time = Mathf.Max(--_time, 0);
        return (0 < _time);
    }

    public float GetNormalized()
    {
        return (float)_time * _inv_time_max;
    }
}