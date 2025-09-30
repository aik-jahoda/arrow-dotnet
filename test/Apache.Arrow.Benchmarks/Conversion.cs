// Licensed to the Apache Software Foundation (ASF) under one or more
// contributor license agreements. See the NOTICE file distributed with
// this work for additional information regarding copyright ownership.
// The ASF licenses this file to You under the Apache License, Version 2.0
// (the "License"); you may not use this file except in compliance with
// the License.  You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Buffers.Binary;
using System.Data.SqlTypes;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;

//using BenchmarkDotNet.Diagnostics.dotMemory;

namespace Apache.Arrow.Benchmarks
{
    //[EtwProfiler] - needs elevated privileges
    [MemoryDiagnoser]
    public class Conversion
    {
        [Benchmark]
        public void SqlDecimalConvert()
        {
            GetBytes128SqlDecimal(1m, 28, 14, stackalloc byte[16]);
        }

        [Benchmark]
        public void Int128Convert()
        {
            GetBytes128Int128(1m, 28, 14, stackalloc byte[16]);
        }


        private static void GetBytes128Int128(decimal value, int precision, int scale, Span<byte> bytes)
        {
            Span<int> decimalBits = stackalloc int[4];
            decimal.GetBits(value, decimalBits);

            int decScale = (decimalBits[3] >> 16) & 0x7F;

            UInt128 unscaled = ((UInt128)(uint)decimalBits[2] << 64) |
                               ((UInt128)(uint)decimalBits[1] << 32) |
                               (uint)decimalBits[0];

            bool isNegative = (decimalBits[3] & unchecked((int)0x80000000)) != 0;

            Int128 bigInt = isNegative ? -(Int128)unscaled : (Int128)unscaled;

            // validate precision and scale
            if (decScale > scale)
                throw new OverflowException($"Decimal scale cannot be greater than that in the Arrow vector: {decScale} != {scale}");

            if (bigInt >= Pow10Table[precision])
                throw new OverflowException($"Decimal precision cannot be greater than that in the Arrow vector: {value} has precision > {precision}");

            if (decScale < scale) // pad with trailing zeros
            {
                bigInt *= Pow10Table[scale - decScale];
            }

            // extract bytes from BigInteger
            if (bytes.Length != 16)
            {
                throw new OverflowException($"ValueBuffer size not equal to {16} byte width: {bytes.Length}");
            }

            if (bytes.Length < 16)
                throw new OverflowException("Could not extract bytes from integer value " + bigInt);

            BinaryPrimitives.WriteInt128LittleEndian(bytes, bigInt);
        }

        private static readonly Int128[] Pow10Table = BuildPow10Table();

        private static Int128[] BuildPow10Table()
        {
            var table = new Int128[29];
            Int128 value = 1;
            for (int i = 0; i <= 28; i++)
            {
                table[i] = value;
                value *= 10;
            }
            return table;
        }

        internal static unsafe void GetBytes128SqlDecimal(decimal value, int precision, int scale, Span<byte> bytes)
        {
            SqlDecimal.ConvertToPrecScale(value, precision, scale);

            Span<uint> buffer = MemoryMarshal.Cast<byte, uint>(bytes);
            uint* ptr = (uint*)&value;
            ptr[0] = ptr[decimalOffsetLow];
            ptr[1] = ptr[decimalOffsetMid];
            ptr[2] = ptr[decimalOffsetHigh];
            ptr[3] = ptr[decimalOffsetHighHigh];
        }


        const int marker = 0x12345678;
        static readonly int decimalOffsetLow = ComputeSqlDecimalOffset(new SqlDecimal(38, 0, true, marker, 0, 0, 0));
        static readonly int decimalOffsetMid = ComputeSqlDecimalOffset(new SqlDecimal(38, 0, true, 0, marker, 0, 0));
        static readonly int decimalOffsetHigh = ComputeSqlDecimalOffset(new SqlDecimal(38, 0, true, 0, 0, marker, 0));
        static readonly int decimalOffsetHighHigh = ComputeSqlDecimalOffset(new SqlDecimal(38, 0, true, 0, 0, 0, marker));

        static unsafe int ComputeSqlDecimalOffset(SqlDecimal d)
        {
            int* ptr = (int*)&d;
            for (int i = 0; i < 7; i++)
            {
                if (ptr[i] == marker) { return i; }
            }
            throw new InvalidOperationException("Unable to find index of offset for SqlDecimal");
        }
    }
}
