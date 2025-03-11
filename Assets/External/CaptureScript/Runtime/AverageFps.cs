using System;
using UnityEngine;


public class AverageFps
{
    public float averageFps;
    public float averageOnePercentFps;
    public float averageZeroOnePercentFps;

    private float[] _samples;
    private float[] _samplesSorted;
    private short _sampleCount;
    private short _currentSampleCount;
    private short _onePercentSampleCount;
    private short _zeroOnePercentSampleCount;
    private short _index;

    public void Initialize()
    {
        Initialize(1000);
    }

    public void Initialize(short sampleCount)
    {
        _sampleCount = sampleCount;
        _currentSampleCount = 0;

        _onePercentSampleCount = (short)Mathf.Max(1, sampleCount / 100);
        _zeroOnePercentSampleCount = (short)Mathf.Max(1, sampleCount / 1000);

        _samples = new float[sampleCount];
        _samplesSorted = new float[sampleCount];

        _index = 0;
    }

    public void Update(float fps)
    {
        _samples[_index] = fps;

        if (_index < _sampleCount - 1)
            _index++;
        else
            _index = 0;

        if (_currentSampleCount < _sampleCount)
            _currentSampleCount++;

        averageFps = GetAverageFps();

        Array.Copy(_samples, _samplesSorted, _sampleCount);
        Array.Sort(_samplesSorted, (a, b) => a.CompareTo(b));

        var startIndex = _sampleCount - _currentSampleCount;

        averageOnePercentFps = GetAverageOnePercentFps(startIndex);
        averageZeroOnePercentFps = GetAverageZeroOnePercentFps(startIndex);
    }

    private float GetAverageFps()
    {
        var value = 0f;

        for (var i = 0; i < _currentSampleCount; i++)
            value += _samples[i];

        value /= _currentSampleCount;

        return value;
    }

    private float GetAverageOnePercentFps(int startIndex)
    {
        var value = 0f;

        var count = Mathf.Min(_currentSampleCount, _onePercentSampleCount);

        for (var i = startIndex; i < startIndex + count; i++)
            value += _samplesSorted[i];

        value /= count;

        return value;
    }

    private float GetAverageZeroOnePercentFps(int startIndex)
    {
        var value = 0f;

        var count = Mathf.Min(_currentSampleCount, _zeroOnePercentSampleCount);

        for (var i = startIndex; i < startIndex + count; i++)
            value += _samplesSorted[i];

        value /= count;

        return value;
    }
}