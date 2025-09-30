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

using System.Linq;
using Apache.Arrow.Types;
using BenchmarkDotNet.Attributes;
//using BenchmarkDotNet.Diagnostics.dotMemory;

namespace Apache.Arrow.Benchmarks
{
    //[EtwProfiler] - needs elevated privileges
    [MemoryDiagnoser]
    public class ArrayBuildersBenchmark
    {

        string[] stringData = Enumerable.Range(0, 16).Select(i => "Some string " + i).ToArray();
        decimal[] decimalData = Enumerable.Range(-7, 16).Select(i => (decimal)i).ToArray();


        [Benchmark]
        public Int32Array.Builder Int32ArrayBuilder_Append()
        {
            Int32Array.Builder builder = new();
            builder.Reserve(16);
            for (int i = -7; i < 8; i++)
            {
                builder.Append(i);
            }

            return builder;
        }

        [Benchmark]
        public void Int64ArrayBuilder()
        {
            Int64Array.Builder builder = new();
            builder.Reserve(16);
            for (int i = -7; i < 8; i++)
            {
                builder.Append(i);
            }
        }

        [Benchmark]
        public void StringArrayBuilder()
        {
            StringArray.Builder builder = new();
            builder.Reserve(16);
            for (int i = 0; i < 16; i++)
            {
                builder.Append(stringData[i]);
            }
        }

        [Benchmark]
        public void Decimal32ArrayBuilder()
        {
            Decimal32Array.Builder builder = new(new Decimal32Type(9, 2));
            builder.Reserve(16);
            for (int i = 0; i < 16; i++)
            {
                builder.Append(decimalData[i]);
            }
        }

        [Benchmark]
        public void Decimal64ArrayBuilder()
        {
            Decimal64Array.Builder builder = new(new Decimal64Type(19, 9));
            builder.Reserve(16);
            for (int i = 0; i < 16; i++)
            {
                builder.Append(decimalData[i]);
            }
        }

        [Benchmark]
        public void Decimal128ArrayBuilder()
        {
            Decimal128Array.Builder builder = new(new Decimal128Type(38, 19));
            builder.Reserve(16);
            for (int i = 0; i < 16; i++)
            {
                builder.Append(decimalData[i]);
            };
        }

        [Benchmark]
        public void Decimal256ArrayBuilder()
        {
            Decimal256Array.Builder builder = new(new Decimal256Type(77, 38));
            builder.Reserve(16);
            for (int i = 0; i < 16; i++)
            {
                builder.Append(decimalData[i]);
            }
        }
    }
}
