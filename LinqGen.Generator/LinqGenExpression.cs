﻿// LinqGen.Generator, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cathei.LinqGen.Generator
{
    using static CodeGenUtils;

    public readonly struct LinqGenExpression
    {
        public IMethodSymbol MethodSymbol { get; }
        public INamedTypeSymbol? SignatureSymbol { get; }
        public ITypeSymbol? InputElementSymbol { get; }
        public INamedTypeSymbol? UpstreamSignatureSymbol { get; }

        private LinqGenExpression(IMethodSymbol methodSymbol, INamedTypeSymbol? signatureSymbol,
            ITypeSymbol? inputElementSymbol, INamedTypeSymbol? upstreamSignatureSymbol)
        {
            MethodSymbol = methodSymbol;
            SignatureSymbol = signatureSymbol;
            InputElementSymbol = inputElementSymbol;
            UpstreamSignatureSymbol = upstreamSignatureSymbol;
        }

        public static bool TryParse(SemanticModel semanticModel,
            InvocationExpressionSyntax invocationSyntax, out LinqGenExpression result)
        {
            result = default;

            if (invocationSyntax.Expression is not MemberAccessExpressionSyntax memberAccessSyntax)
            {
                // not a method invocation
                return false;
            }

            var memberSymbolInfo = semanticModel.GetSymbolInfo(memberAccessSyntax.Name);

            if (memberSymbolInfo.Symbol is not IMethodSymbol methodSymbol || !IsStubMethod(methodSymbol))
            {
                // not a stub method
                return false;
            }

            INamedTypeSymbol? signatureSymbol = null;

            // returning stub enumerable, meaning it's compiling generation
            if (methodSymbol.ReturnType is INamedTypeSymbol returnTypeSymbol &&
                IsOutputStubEnumerable(returnTypeSymbol))
            {
                signatureSymbol = returnTypeSymbol.TypeArguments[1] as INamedTypeSymbol;

                if (signatureSymbol == null)
                {
                    // something is wrong
                    // generic signature is not allowed
                    return false;
                }
            }

            ITypeSymbol? inputElementSymbol = null;
            INamedTypeSymbol? upstreamSignatureSymbol = null;

            // this means it takes LinqGen enumerable as input, and upstream type is required
            if (methodSymbol.ReceiverType is INamedTypeSymbol receiverTypeSymbol &&
                IsInputStubEnumerable(receiverTypeSymbol))
            {
                if (!TryParseStubInterface(receiverTypeSymbol, out inputElementSymbol, out upstreamSignatureSymbol))
                {
                    // How did this happen?
                    // TODO: Can we allow generic constrained upstream type?
                    return false;
                }
            }

            if (signatureSymbol == null && inputElementSymbol == null)
            {
                // evaluations must have known input element symbol
                return false;
            }

            if (signatureSymbol != null)
                signatureSymbol = NormalizeSignature(signatureSymbol);

            if (upstreamSignatureSymbol != null)
                upstreamSignatureSymbol = NormalizeSignature(upstreamSignatureSymbol);

            result = new LinqGenExpression(
                methodSymbol, signatureSymbol, inputElementSymbol, upstreamSignatureSymbol);

            return true;
        }

        public static bool TryParse(SemanticModel semanticModel,
            CommonForEachStatementSyntax forEachSyntax, out LinqGenExpression result)
        {
            result = default;

            var expressionTypeSymbol = semanticModel.GetTypeInfo(forEachSyntax.Expression).Type;

            if (expressionTypeSymbol == null)
                return false;

            // Lookup for GetEnumerator Stub extension method
            var methodSymbol = semanticModel
                .LookupSymbols(forEachSyntax.SpanStart, expressionTypeSymbol, "GetEnumerator", true)
                .OfType<IMethodSymbol>()
                .FirstOrDefault();

            if (methodSymbol == null)
                return false;

            ITypeSymbol? inputElementSymbol = null;
            INamedTypeSymbol? upstreamSignatureSymbol = null;

            // this means it takes LinqGen enumerable as input, and upstream type is required
            if (methodSymbol.ReceiverType is INamedTypeSymbol receiverTypeSymbol &&
                IsInputStubEnumerable(receiverTypeSymbol))
            {
                if (!TryParseStubInterface(receiverTypeSymbol, out inputElementSymbol, out upstreamSignatureSymbol))
                {
                    // How did this happen?
                    // TODO: Can we allow generic constrained upstream type?
                    return false;
                }
            }

            if (inputElementSymbol == null)
            {
                // evaluations must have known input element symbol
                return false;
            }

            result = new LinqGenExpression(methodSymbol, null, inputElementSymbol, upstreamSignatureSymbol);
            return true;
        }

        public bool TryGetParameterType(int index, out ITypeSymbol result)
        {
            if (MethodSymbol.Parameters.Length <= index)
            {
                result = default!;
                return false;
            }

            result = MethodSymbol.Parameters[index].Type;
            return true;
        }

        // index 0 is second argument because first argument is treated as caller when it's extension method
        public bool TryGetNamedParameterType(int index, out INamedTypeSymbol result)
        {
            if (!TryGetParameterType(index, out var typeSymbol))
            {
                result = null!;
                return false;
            }

            if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
            {
                result = null!;
                return false;
            }

            result = namedTypeSymbol;
            return true;
        }

        public INamedTypeSymbol GetNamedParameterType(int index)
        {
            if (!TryGetNamedParameterType(index, out var result))
                throw new InvalidOperationException();
            return result;
        }

        public bool IsCompilingGeneration()
        {
            return SignatureSymbol != null;
        }
    }
}