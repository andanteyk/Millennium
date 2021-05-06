using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using UnityEngine;

namespace Millennium.Mathematics
{
    /// <summary>
    /// Seiran128 Random Number Generator
    /// </summary>
    public sealed class Seiran
    {
        private ulong State0;
        private ulong State1;


        [ThreadStatic]
        private static Seiran StaticInstance = null;

        public static Seiran Shared => StaticInstance ??= new Seiran();


        public Seiran()
        {
            var bytes = new byte[16];
            using (var csprng = new RNGCryptoServiceProvider())
            {
                do
                {
                    csprng.GetBytes(bytes);
                } while (bytes.All(b => b == 0));

                State0 = BitConverter.ToUInt64(bytes, 0);
                State1 = BitConverter.ToUInt64(bytes, 8);
            }
        }


        public ulong Next()
        {
            static ulong rotl(ulong x, int k) => x << k | x >> -k;

            ulong s0 = State0, s1 = State1;
            ulong result = rotl((s0 + s1) * 9, 29) + s0;

            State0 = s0 ^ rotl(s1, 29);
            State1 = s0 ^ s1 << 9;

            return result;
        }


        private uint NextUintStrict(uint maxExclusive)
        {
            ulong rand = Next();
            ulong m = (rand & ~0u) * maxExclusive;
            uint mlo = (uint)m;
            uint mhi = (uint)(m >> 32);

            if (mlo < maxExclusive)
            {
                uint lowerBound = 0u - maxExclusive;
                if (lowerBound >= maxExclusive)
                {
                    lowerBound -= maxExclusive;

                    if (lowerBound >= maxExclusive)
                    {
                        lowerBound %= maxExclusive;
                    }
                }

                while (mlo < lowerBound)
                {
                    m = (rand >> 32) * maxExclusive;
                    mlo = (uint)m;
                    mhi = (uint)(m >> 32);

                    if (mlo >= lowerBound)
                        break;

                    rand = Next();
                    m = (rand & ~0u) * maxExclusive;
                    mlo = (uint)m;
                    mhi = (uint)(m >> 32);
                }
            }

            return mhi;
        }

        public int Next(int maxExclusive)
        {
            if (maxExclusive < 0)
                throw new ArgumentOutOfRangeException(nameof(maxExclusive));

            return (int)NextUintStrict((uint)maxExclusive);
        }

        public int Next(int minInclusive, int maxExclusive)
        {
            if (minInclusive > maxExclusive)
                throw new ArgumentOutOfRangeException(nameof(maxExclusive));

            return (int)NextUintStrict((uint)(maxExclusive - minInclusive)) + minInclusive;
        }


        private double NextDouble() => (Next() >> 11) * (1.0 / (1ul << 53));

        public double NextDouble(double minInclusive, double maxExclusive)
        {
            if (minInclusive > maxExclusive)
                throw new ArgumentOutOfRangeException(nameof(maxExclusive));

            double t = NextDouble();
            double ret = minInclusive * (1 - t) + maxExclusive * t;

            if (ret == maxExclusive)
                ret = BitDecrement(ret);

            return ret;
        }

        private float NextSingle() => (Next() >> 40) * (1f / (1u << 24));

        public float NextSingle(float minInclusive, float maxExclusive)
        {
            if (minInclusive > maxExclusive)
                throw new ArgumentOutOfRangeException(nameof(maxExclusive));

            float t = NextSingle();
            float ret = minInclusive * (1 - t) + maxExclusive * t;

            if (ret == maxExclusive)
                ret = BitDecrement(ret);

            return ret;
        }

        public float NextRadian() => NextSingle(-Mathf.PI, Mathf.PI);

        public Vector3 OnUnitCircle()
        {
            var angle = NextSingle(0, Mathf.PI * 2);
            return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        public Vector3 InsideUnitCircle()
        {
            var angle = NextSingle(0, Mathf.PI * 2);
            var radius = Mathf.Sqrt(NextSingle());

            return new Vector3(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));
        }


        // polyfill of Math.BitDecrement 
        [StructLayout(LayoutKind.Explicit)]
        private struct DoubleUlongUnion
        {
            [FieldOffset(0)]
            public double d;
            [FieldOffset(0)]
            public ulong u;
        }
        private static double BitDecrement(double x)
        {
            var union = new DoubleUlongUnion { d = x };
            if ((union.u & 0x7ff00000_00000000) >= 0x7ff00000_00000000)
            {
                return union.u == 0x7ff00000_00000000 ? double.MaxValue : x;
            }

            if (union.u == 0)
                return -double.Epsilon;

            if (union.u < 0)
                union.u++;
            else
                union.u--;

            return union.d;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct FloatUintUnion
        {
            [FieldOffset(0)]
            public float d;
            [FieldOffset(0)]
            public uint u;
        }
        private static float BitDecrement(float x)
        {
            var union = new FloatUintUnion { d = x };
            if ((union.u & 0x7f800000) >= 0x7f800000)
            {
                return union.u == 0x7f800000 ? float.MaxValue : x;
            }

            if (union.u == 0)
                return -float.Epsilon;

            if (union.u < 0)
                union.u++;
            else
                union.u--;

            return union.d;
        }

    }
}
