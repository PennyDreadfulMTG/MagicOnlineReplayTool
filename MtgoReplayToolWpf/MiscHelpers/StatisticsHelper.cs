using System;

namespace MtgoReplayToolWpf.MiscHelpers
{
    public static class StatisticsHelper
    {
        public static Double TwoProportionZTest(Int32 positivesA, Int32 sampleA, Int32 positivesB, Int32 sampleB)
        {
            Double sampleProportionA = (Double)positivesA / sampleA;
            Double sampleProportionB = (Double)positivesB / sampleB;
            Double sample = sampleA + sampleB;
            Double pooledSampleProportion = (positivesA + positivesB) / sample;
            Double standardError = Math.Sqrt(pooledSampleProportion * (1 - pooledSampleProportion) * (1.0/sampleA + 1.0/sampleB));
            Double zScore = (sampleProportionA - sampleProportionB) / standardError;
            Double p = MathNet.Numerics.Distributions.Normal.CDF(0, 1, zScore);
            return p;
        }
    }
}
