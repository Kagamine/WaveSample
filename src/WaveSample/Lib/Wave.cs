using System;
using System.Numerics;

namespace WaveSample.Lib
{
    public static class Wave
    {
        public static Complex[] FFT(Complex[] sourceData, int countN)
        {
            int r = Convert.ToInt32(Math.Log(countN, 2));
            Complex[] interVar1 = new Complex[countN];
            Complex[] interVar2 = new Complex[countN];
            interVar1 = (Complex[])sourceData.Clone();
            Complex[] w = new Complex[countN / 2];
            for (int i = 0; i < countN / 2; i++)
            {
                double angle = -i * Math.PI * 2 / countN;
                w[i] = new Complex(Math.Cos(angle), Math.Sin(angle));
            }
            
            for (int i = 0; i < r; i++)
            {
                int interval = 1 << i;
                int halfN = 1 << (r - i);
                for (int j = 0; j < interval; j++)
                {
                    int gap = j * halfN;
                    for (int k = 0; k < halfN / 2; k++)
                    {
                        interVar2[k + gap] = interVar1[k + gap] + interVar1[k + gap + halfN / 2];
                        interVar2[k + halfN / 2 + gap] = (interVar1[k + gap] - interVar1[k + gap + halfN / 2]) * w[k * interval];
                    }
                }
                interVar1 = (Complex[])interVar2.Clone();
            }
            for (uint j = 0; j < countN; j++)
            {
                uint rev = 0;
                uint num = j;
                for (int i = 0; i < r; i++)
                {
                    rev <<= 1;
                    rev |= num & 1;
                    num >>= 1;
                }
                interVar2[rev] = interVar1[j];
            }
            return interVar2;
        }
    }
}
