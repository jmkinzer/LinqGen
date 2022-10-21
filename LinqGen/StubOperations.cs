﻿// LinqGen, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;

namespace Cathei.LinqGen.Hidden
{
    // Operation is type hint for LinqGen.
    // For example, for code like this:
    // Enumerable.Range(10).Generate().Where(x => x % 2 == 0).Select(x => x * 0.5)
    // The information is like this:
    // Select<Where<Gen<int>>, double>
    public interface ILinqGenOperation { }

    /// <summary>
    /// NoOp will be used far all actual implementation.
    /// Since StubEnumerable will be used for code generation, we don't need type information.
    /// TODO: we could improve NoOp to hold type information so we can access LinqGen struct from other assembly
    /// </summary>
    public abstract class NoOp : ILinqGenOperation { }

    public abstract class Gen<T> : ILinqGenOperation { }

    public abstract class GenList<T> : ILinqGenOperation { }

    public abstract class Select<TParent, TOut> : ILinqGenOperation { }

    public abstract class SelectAt<TParent, TOut> : ILinqGenOperation { }

    public abstract class SelectStruct<TParent, TOut> : ILinqGenOperation { }

    public abstract class SelectAtStruct<TParent, TOut> : ILinqGenOperation { }

    public abstract class Where<TParent> : ILinqGenOperation { }

    public abstract class WhereAt<TParent> : ILinqGenOperation { }

    public abstract class WhereStruct<TParent> : ILinqGenOperation { }

    public abstract class WhereAtStruct<TParent> : ILinqGenOperation { }

}