// LinqGen.Generator, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Cathei.LinqGen.Generator
{
    using static SyntaxFactory;
    using static CodeGenUtils;

    public sealed class MinMaxEvaluation : VisitorEvaluation
    {
        private bool IsMin { get; }
        private bool WithComparerParameter { get; }
        private bool IsElementComparable { get; }
        private TypeSyntax ComparerType { get; }

        public MinMaxEvaluation(in LinqGenExpression expression, int id, bool isMin) : base(expression, id)
        {
            IsMin = isMin;

            // Min and Max with parameter
            WithComparerParameter = expression.MethodSymbol.Parameters.Length == 1;

            if (WithComparerParameter)
            {
                ComparerType = TypeName("Comparer");
            }
            else if (TryGetComparableSelfInterface(InputElementSymbol, out _))
            {
                IsElementComparable = true;
                ComparerType = GenericName(
                    Identifier("CompareToComparer"), TypeArgumentList(InputElementType));
            }
            else
            {
                ComparerType = ComparerInterfaceType;
            }
        }

        protected override TypeSyntax ReturnType => InputElementType;

        private TypeSyntax ComparerInterfaceType =>
            GenericName(Identifier("IComparer"), TypeArgumentList(InputElementType));

        protected override IEnumerable<TypeParameterInfo> GetTypeParameterInfos()
        {
            if (WithComparerParameter)
                yield return new(TypeName("Comparer"), ComparerInterfaceType);
        }

        protected override IEnumerable<ParameterInfo> GetParameterInfos()
        {
            if (WithComparerParameter)
                yield return new(ComparerType, IdentifierName("comparer"));
        }

        protected override IEnumerable<MemberDeclarationSyntax> RenderVisitorFields()
        {
            if (!WithComparerParameter)
                yield return FieldDeclaration(PrivateTokenList, ComparerType, Identifier("comparer"));

            yield return FieldDeclaration(PrivateTokenList, BoolType, VarName("isSet").Identifier);
            yield return FieldDeclaration(PrivateTokenList, ReturnType, VarName("result").Identifier);
        }

        protected override IEnumerable<StatementSyntax> RenderInitialization()
        {
            if (!WithComparerParameter)
            {
                yield return ExpressionStatement(SimpleAssignmentExpression(
                    IdentifierName("comparer"), IsElementComparable
                        ? ObjectCreationExpression(ComparerType, EmptyArgumentList, null)
                        : ComparerDefault(Upstream.OutputElementType)));
            }
        }

        protected override IEnumerable<StatementSyntax> RenderAccumulation()
        {
            var expressionKind = IsMin ? SyntaxKind.LessThanExpression : SyntaxKind.GreaterThanExpression;

            var comparison = BinaryExpression(expressionKind,
                LiteralExpression(0),
                InvocationExpression(MemberAccessExpression(IdentifierName("comparer"), CompareMethod),
                    ArgumentList(VarName("result"), ElementVar)));

            yield return IfStatement(
                LogicalOrExpression(LogicalNotExpression(VarName("isSet")), comparison), Block(
                    ExpressionStatement(SimpleAssignmentExpression(VarName("isSet"), TrueExpression())),
                    ExpressionStatement(SimpleAssignmentExpression(VarName("result"), ElementVar))));

            yield return ReturnStatement(TrueExpression());
        }

        protected override IEnumerable<StatementSyntax> RenderReturn()
        {
            yield return IfStatement(LogicalNotExpression(VarName("isSet")), ThrowInvalidOperationStatement());
            yield return ReturnStatement(VarName("result"));
        }
    }
}