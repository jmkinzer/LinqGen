// LinqGen.Generator, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Cathei.LinqGen.Generator
{
    using static SyntaxFactory;
    using static CodeGenUtils;

    public sealed class SumEvaluation : Evaluation
    {
        private TypeSyntax? FunctionType { get; }
        private bool WithStruct { get; }

        public SumEvaluation(in LinqGenExpression expression) : base(expression)
        {
            ReturnType = ParseTypeName(expression.MethodSymbol.ReturnType);

            if (MethodSymbol.Parameters.Length == 2)
            {
                // bit hard coding but Sum with two parameter uses selector
                var parameterType = MethodSymbol.Parameters[0].Type;

                FunctionType = ParseTypeName(parameterType);
                WithStruct = IsStructFunction(parameterType);
            }
            else
            {
                // and single parameter only has default value
                FunctionType = null;
                WithStruct = false;
            }
        }

        public override TypeSyntax ReturnType { get; }

        public override IEnumerable<ParameterSyntax> GetParameters()
        {
            foreach (var info in base.GetParameters())
                yield return info;

            if (FunctionType != null)
            {
                yield return Parameter(default, default,
                    WithStruct ? IdentifierName($"{TypeParameterPrefix}1") : FunctionType, SelectorName.Identifier, default);
            }

            yield return Parameter(default, default,
                ReturnType, InitialValueName.Identifier, EqualsValueClause(DefaultLiteral));
        }

        protected override IEnumerable<TypeParameterInfo> GetTypeParameterInfos()
        {
            if (WithStruct)
                yield return new TypeParameterInfo(IdentifierName($"{TypeParameterPrefix}1"), FunctionType!);
        }

        public override BlockSyntax RenderMethodBody()
        {
            return Block(UsingLocalDeclarationStatement(
                    IteratorName.Identifier, InvocationExpression(SourceName, GetEnumeratorName)),
                WhileStatement(InvocationExpression(IteratorName, MoveNextName),
                    ExpressionStatement(
                        AssignmentExpression(SyntaxKind.AddAssignmentExpression, InitialValueName,
                            FunctionType == null
                                ? MemberAccessExpression(IteratorName, CurrentName)
                                : InvocationExpression(MemberAccessExpression(SelectorName, InvokeName),
                                    ArgumentList(MemberAccessExpression(IteratorName, CurrentName)))))),
                ReturnStatement(InitialValueName));
        }
    }
}
