namespace Voidwell.Deadlock.Analytics;

public static class Statistics
{
    /// <summary>
    /// The Wilson score interval (95% by default) for a rate of <paramref name="successes"/> in <paramref name="total"/>.
    /// Unlike the plain margin of error it stays inside 0 to 1 and behaves for small samples.
    /// </summary>
    public static (double Low, double High) Wilson(long successes, long total, double z = 1.96)
    {
        if (total <= 0)
        {
            return (0, 0);
        }

        var n = (double)total;
        var p = successes / n;
        var z2 = z * z;
        var denominator = 1 + (z2 / n);
        var center = (p + (z2 / (2 * n))) / denominator;
        var margin = z * Math.Sqrt((p * (1 - p) / n) + (z2 / (4 * n * n))) / denominator;

        return (Math.Max(0, center - margin), Math.Min(1, center + margin));
    }

    /// <summary>
    /// The z statistic of the difference between two rates. Beyond about 1.96 in either direction the difference is
    /// unlikely to be chance.
    /// </summary>
    public static double TwoProportionZ(long successes1, long total1, long successes2, long total2)
    {
        if (total1 <= 0 || total2 <= 0)
        {
            return 0;
        }

        var pooled = (double)(successes1 + successes2) / (total1 + total2);
        var standardError = Math.Sqrt(pooled * (1 - pooled) * ((1.0 / total1) + (1.0 / total2)));

        return standardError == 0 ? 0 : (((double)successes1 / total1) - ((double)successes2 / total2)) / standardError;
    }

    public static double Divide(double numerator, double denominator)
    {
        return denominator == 0 ? 0 : numerator / denominator;
    }
}
