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

    public sealed class RangeGeneration : PredefinedGeneration
    {
        public RangeGeneration(in LinqGenExpression expression, int id) : base(expression, id)
        {
        }

        public override TypeSyntax OutputElementType => IntType;

        protected override IEnumerable<MemberInfo> GetMemberInfos(bool isLocal)
        {
            yield return new MemberInfo(MemberKind.Both, IntType, VarName("start"));
            yield return new MemberInfo(MemberKind.Both, IntType, VarName("count"));

            yield return new MemberInfo(MemberKind.Enumerator, IntType, VarName("index"));
        }

        public override IEnumerable<StatementSyntax> RenderInitialization(RenderOption option)
        {
            yield return ExpressionStatement(SimpleAssignmentExpression(VarName("index"), LiteralExpression(-1)));
        }

        public override BlockSyntax RenderIteration(RenderOption option, SyntaxList<StatementSyntax> statements)
        {
            var currentName = VarName("current");
            var currentRewriter = new CurrentRewriter(currentName);

            // replace current variables of downstream
            statements = currentRewriter.VisitStatementSyntaxList(statements);

            statements = statements.Insert(0, LocalDeclarationStatement(
                currentName.Identifier, AddExpression(VarName("start"), VarName("index"))));

            return Block(WhileStatement(
                LessThanExpression(PreIncrementExpression(VarName("index")), VarName("count")),
                Block(statements)));
        }
    }
}