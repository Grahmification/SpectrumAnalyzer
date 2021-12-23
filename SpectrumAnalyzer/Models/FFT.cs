using System;
using System.Collections.Generic;
using System.Threading;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace SpectrumAnalyzer.Models
{
    public class FFT
    {
        public static double MaxFrequency(double[,] inputData)
        {
            return SamplingFrequency(inputData) / 2.0;
        }
        public static double MaxFrequency(double samplingFrequency)
        {
            return samplingFrequency / 2.0; 
        }
        public static double MinFrequency(double[,] inputData)
        {
            return SamplingFrequency(inputData) / inputData.GetLength(0);
        }
        public static double MinFrequency(double samplingFrequency, int datasetLength)
        {
            return samplingFrequency / datasetLength;
        }


        static double[] CalculateFrequencies(double[,] inputData)
        {
            return CalculateFrequencies(inputData.GetLength(0), SamplingFrequency(inputData));
        }
        static double[] CalculateFrequencies(int n, double samplingFrequency)
        {
            int numFreqs = (n / 2) - 1; //number of unique frequencies the fft can compute
            double[] Freqs = new double[numFreqs];

            for (int i = 0; i < numFreqs; i++)
                Freqs[i] = 1.0 * i * samplingFrequency / n; //compute each frequency

            return Freqs;
        }
        static double[,] CalculatePSD(double[] Freqs, Complex32[] FFTData, double freqCutOff = -1)
        {
            int nFreqs = Freqs.Length;

            for (int i = 0; i < nFreqs; i++)
            {
                if (freqCutOff != -1 && Freqs[i] > freqCutOff)
                {
                    nFreqs = i + 1;
                    break;
                }
            }

            var output = new double[2, nFreqs];


            for (int i = 0; i < nFreqs; i++)
            {
                output[0, i] = Freqs[i]; //frequencies
                output[1, i] = FFTData[i].Real * FFTData[i].Real + FFTData[i].Imaginary * FFTData[i].Imaginary; //psd values
            }

            return output;
        }

        public static double[,] computeFFT(double[,] inputData)
        {
            int n = inputData.GetLength(0);

            if (!IsPowerOfTwo(n))
                throw new ArgumentException(string.Format("Input dataset with {0} entries not a valid size for computing an FFT."));

            double samplingFreq = SamplingFrequency(inputData);

            // ---------------------------- Compute Frequencies ----------------------------------

            double[] Freqs = CalculateFrequencies(n, samplingFreq); //occasionally can be infinity if inputdata takes >1ms

            // ---------------------------- Calculate FFT ----------------------------------

            Complex32[] CompArray = PrepareComplexArray(inputData);
            Fourier.Forward(CompArray);

            return CalculatePSD(Freqs, CompArray);
        }
        public static List<SignalComponent> computeFFTComponents(double[,] inputData)
        {
            int n = inputData.GetLength(0);

            if (!IsPowerOfTwo(n))
                throw new ArgumentException(string.Format("Input dataset with {0} entries not a valid size for computing an FFT."));

            // ---------------------------- Compute Frequencies ----------------------------------

            double[] Freqs = CalculateFrequencies(n, SamplingFrequency(inputData)); //occasionally can be infinity if inputdata takes >1ms

            // ---------------------------- Calculate FFT ----------------------------------
            
            Complex32[] CompArray = PrepareComplexArray(inputData);
            Fourier.Forward(CompArray, FourierOptions.NoScaling);

            // ------------------------ Prepare output data -------------------------------------

            var output = new List<SignalComponent>();

            for(int i = 0; i < Freqs.Length; i++)
                output.Add(new SignalComponent(Freqs[i], CompArray[i].Real, CompArray[i].Imaginary, i, n));

            var magnitudeSum = SignalComponent.ComputeTotalMagnitude(output);

            for (int i = 0; i < output.Count; i++)
                output[i].SetContributionFraction(magnitudeSum);

            return output;
        }

        public static bool IsPowerOfTwo(int x)
        {
            return (x > 0) && ((x & (x - 1)) == 0);
        }
        public static double SamplingFrequency(double[,] inputData)
        {
            int n = inputData.GetLength(0) - 1;
            return n / (inputData[n, 0] - inputData[0, 0]); //compute average sampling frequency based on start/end times
        }



        private static Complex32[] PrepareComplexArray(double[,] inputData)
        {
            int n = inputData.GetLength(0);
            Complex32[] CompArray = new Complex32[n];

            for (int i = 0; i < n; i++)
                CompArray[i] = new Complex32((float)inputData[i, 1], 0); //fill array with data in real components

            return CompArray;
        }
        private static double[] generateSignal(int counter)
        {
            double[] output = new double[counter];

            Random rnd = new Random();

            double scale = rnd.Next(1, 20);
            double period = rnd.Next(1, 20);

            double scale2 = rnd.Next(1, 20);
            double period2 = rnd.Next(1, 20);

            for (int i = 0; i < counter; i++)
            {
                output[i] = scale * Math.Sin(period * i / 100) + scale2 * Math.Cos(period2 * i / 100) + i * i / (counter * counter);
                scale2 = rnd.Next(1, 20);
                period2 = rnd.Next(1, 20);

            }
            Thread.Sleep(1);
            return output;

        } //generates a random signal for testing
    }
}
