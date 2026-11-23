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

    /// <summary>
    /// The value below which a standard normal variable falls with probability <paramref name="p"/>, for example 1.96 for
    /// 0.975. A rational approximation (after Acklam), within 1e-4 of the true value, which is plenty for the critical
    /// values of confidence intervals.
    /// </summary>
    public static double NormalQuantile(double p)
    {
        if (p <= 0 || p >= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(p), "p must be between 0 and 1.");
        }

        const double low = 0.02425;
        double[] a = [-39.69683028665376, 220.9460984245205, -275.9285104469687, 138.3577518672690, -30.66479806614716, 2.506628277459239];
        double[] b = [-54.47609879822406, 161.5858368580409, -155.6989798598866, 66.80131188771972, -13.28068155288572];
        double[] c = [-0.007784894002430293, -0.3223964580411365, -2.400758277161838, -2.549671010422195, 4.374664141464968, 2.938163982698783];
        double[] d = [0.007784695709041462, 0.3224671290700398, 2.445134137142996, 3.754408661907416];

        if (p < low || p > 1 - low)
        {
            var q = Math.Sqrt(-2 * Math.Log(p < low ? p : 1 - p));
            var x = (((((c[0] * q + c[1]) * q + c[2]) * q + c[3]) * q + c[4]) * q + c[5]) / ((((d[0] * q + d[1]) * q + d[2]) * q + d[3]) * q + 1);
            return p < low ? x : -x;
        }

        var r = (p - 0.5) * (p - 0.5);
        return (((((a[0] * r + a[1]) * r + a[2]) * r + a[3]) * r + a[4]) * r + a[5]) * (p - 0.5)
            / (((((b[0] * r + b[1]) * r + b[2]) * r + b[3]) * r + b[4]) * r + 1);
    }

    public static double Divide(double numerator, double denominator)
    {
        return denominator == 0 ? 0 : numerator / denominator;
    }
}
