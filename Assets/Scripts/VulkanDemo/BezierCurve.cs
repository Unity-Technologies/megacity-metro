using UnityEngine;

public struct BezierCurve
{
    public Vector3 a, b, c, d;

    public BezierCurve(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        this.a = a;
        this.b = b;
        this.c = c;
        this.d = d;
    }

    public Vector3 Evaluate(float t)
    {
        t = Mathf.Clamp01(t);
        float invT = 1f - t;
        return invT * invT * invT * a +
            3f * invT * invT * t * b +
            3f * invT * t * t * c +
            t * t * t * d;
    }

    public float GetApproximateLength()
    {
        float firstToLastDistance = Vector3.Distance(a, d);
        float perSegmentDistance = Vector3.Distance(a, b) + Vector3.Distance(b, c) + Vector3.Distance(c, d);
        return (firstToLastDistance + perSegmentDistance) * 0.5f;
    }

    public void GetApproximateLength(out Vector2 lengths)
    {
        SplitCurve(out var firstHalf, out var secondHalf);
        lengths.x = firstHalf.GetApproximateLength();
        lengths.y = secondHalf.GetApproximateLength();
    }

    public void GetApproximateLength(out Vector4 lengths)
    {
        SplitCurve(out var firstHalf, out var secondHalf);
        firstHalf.GetApproximateLength(out Vector2 quarterLengths);
        lengths.x = quarterLengths.x;
        lengths.y = quarterLengths.y;
        secondHalf.GetApproximateLength(out quarterLengths);
        lengths.z = quarterLengths.x;
        lengths.w = quarterLengths.y;
    }

    public void GetApproximateLength(float[] lengths)
    {
        int segmentCount = lengths.Length;
        if (!Mathf.IsPowerOfTwo(segmentCount))
            throw new System.ArgumentException("Bezier subdivision must be power of two");
        FillLengthArrayRecursive(lengths, 0, segmentCount, this);
    }

    void FillLengthArrayRecursive(float[] array, int startingIndex, int fillCount, BezierCurve curve)
    {
        if (fillCount > 1)
        {
            int halfFillCount = fillCount / 2;
            curve.SplitCurve(out var firstHalf, out var secondHalf);
            FillLengthArrayRecursive(array, startingIndex, halfFillCount, firstHalf);
            FillLengthArrayRecursive(array, startingIndex + halfFillCount, halfFillCount, secondHalf);
            return;
        }
        array[startingIndex] = curve.GetApproximateLength();
    }

    public void SplitCurve(out BezierCurve firstHalf, out BezierCurve secondHalf)
    {
        Vector3 e = (a + b) * 0.5f;
        Vector3 f = (b + c) * 0.5f;
        Vector3 g = (c + d) * 0.5f;
        Vector3 h = (e + f) * 0.5f;
        Vector3 j = (f + g) * 0.5f;
        Vector3 k = (h + j) * 0.5f;

        firstHalf = new BezierCurve(a, e, h, k);
        secondHalf = new BezierCurve(k, j, g, d);
    }
}

[System.Serializable]
public class CurveSampler
{
    [SerializeField] int curveIndex = 0;
    [SerializeField] int subdivisionIndex = 0;
    [SerializeField] float subdivisionPercent = 0f;

    BezierCurve[] curves;
    float[][] subdivisionLengths;
    float currentSubdivisionLength;
    System.Action<int> onNewSegment;

    public CurveSampler(System.Action<int> onNewSegment)
    {
        this.onNewSegment = onNewSegment;
    }

    public void MapToSet(BezierCurve[] curves, float[][] subdivisionLengths)
    {
        this.curves = curves;
        this.subdivisionLengths = subdivisionLengths;
        currentSubdivisionLength = subdivisionLengths[0][0];
        curveIndex = 0;
        subdivisionIndex = 0;
        subdivisionPercent = 0f;
    }

    public void Advance(float stepSize)
    {
        if (curveIndex >= curves.Length) return;
        float distanceLeftInCurrentQuarter = currentSubdivisionLength * (1f - subdivisionPercent);
        if (distanceLeftInCurrentQuarter <= stepSize)
        {
            stepSize -= distanceLeftInCurrentQuarter;
            AdvanceToNextSubdivision();
            Advance(stepSize);
            return;
        }
        subdivisionPercent += stepSize / currentSubdivisionLength;
    }

    public Vector3 SampleCurrentPosition()
    {
        if (curveIndex >= curves.Length)
        {
            return curves[curves.Length - 1].Evaluate(1f);
        }

        float currentNodePercent = (subdivisionPercent + subdivisionIndex) / TrackFlier.BEZIER_SUBDIVISION_COUNT;
        currentNodePercent = Mathf.Clamp01(currentNodePercent);
        return curves[curveIndex].Evaluate(currentNodePercent);
    }

    void AdvanceToNextSubdivision()
    {
        subdivisionIndex++;
        if (subdivisionIndex >= TrackFlier.BEZIER_SUBDIVISION_COUNT)
            AdvanceToNextSegment();
        else
        {
            currentSubdivisionLength = subdivisionLengths[curveIndex][subdivisionIndex];
            subdivisionPercent = 0;
        }
    }

    void AdvanceToNextSegment()
    {
        curveIndex += 1;
        onNewSegment?.Invoke(curveIndex);
        if (curveIndex >= curves.Length) return;

        subdivisionPercent = 0;
        subdivisionIndex = 0;
        currentSubdivisionLength = subdivisionLengths[curveIndex][0];
    }
}