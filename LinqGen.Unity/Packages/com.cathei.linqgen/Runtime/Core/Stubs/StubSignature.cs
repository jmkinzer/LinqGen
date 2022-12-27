﻿// LinqGen, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;

namespace Cathei.LinqGen.Hidden
{
    // Operation is type hint for LinqGen.
    // For example, for code like this:
    // Enumerable.Range(10).Generate().Where(x => x % 2 == 0).Select(x => x * 0.5)
    // The signature is like this:
    // Select<Where<Specialized<IEnumerable<int>>>, double>
    public interface IStubSignature { }

    public abstract class Compiled : IStubSignature { }

    public abstract class Range : IStubSignature { }

    public abstract class Empty : IStubSignature { }

    public abstract class Repeat : IStubSignature { }

    public abstract class Specialize<TEnumerable> : IStubSignature { }

    public abstract class SpecializeList<T> : IStubSignature { }

    public abstract class SpecializeStruct<T, TEnumerator> : IStubSignature { }

    public abstract class SpecializeSpan<T> : IStubSignature { }

    public abstract class Cast<TUp> : IStubSignature { }

    public abstract class OfType<TUp> : IStubSignature { }

    public abstract class Select<TUp, TOut> : IStubSignature { }

    public abstract class SelectAt<TUp, TOut> : IStubSignature { }

    public abstract class SelectStruct<TUp, TOut> : IStubSignature { }

    public abstract class SelectAtStruct<TUp, TOut> : IStubSignature { }

    public abstract class Where<TUp> : IStubSignature { }

    public abstract class WhereAt<TUp> : IStubSignature { }

    public abstract class WhereStruct<TUp> : IStubSignature { }

    public abstract class WhereAtStruct<TUp> : IStubSignature { }

    public abstract class Skip<TUp> : IStubSignature { }

    public abstract class Take<TUp> : IStubSignature { }

    public abstract class Distinct<TUp> : IStubSignature { }

    public abstract class DistinctComparer<TUp> : IStubSignature { }

    public abstract class DistinctStruct<TUp> : IStubSignature { }

    public abstract class Concat<TUp1, TUp2> : IStubSignature { }
}