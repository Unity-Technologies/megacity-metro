public class FloatAverage
{
    private int index;
    private int count;
    private float[] values;

    public FloatAverage(int count = 60)
    {
        values = new float[count];
    }

    public void Add(float value)
    {
        values[index] = value;
        index = (index + 1) % values.Length;
        if (count < index)
        {
            count = index;
        }
    }

    public float GetAverage()
    {
        if (count == 0) return 0;

        float result = 0;
        for (int i = 0; i < count; i++)
        {
            result += values[i] / count;
        }

        return result;
    }
}