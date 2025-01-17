﻿using Heir.AST;
using Heir.AST.Abstract;
using Heir.Types;

namespace Heir.BoundAST
{
    public abstract class BoundStatement : BoundSyntaxNode
    {
        public abstract BaseType? Type { get; }

        public abstract R Accept<R>(Visitor<R> visitor);

        public interface Visitor<out R>
        {
            public R VisitBoundSyntaxTree(BoundSyntaxTree tree);
            public R VisitBoundBlock(BoundBlock boundBlock);
            public R VisitBoundVariableDeclaration(BoundVariableDeclaration variableDeclaration);
            public R VisitBoundExpressionStatement(BoundExpressionStatement expressionStatement);
            public R VisitBoundNoOp(BoundNoOpStatement noOp);
            public R VisitBoundReturnStatement(BoundReturn @return);
            public R VisitBoundFunctionDeclaration(BoundFunctionDeclaration declaration);
            public R VisitBoundIfStatement(BoundIf @if);
        }
    }

    public abstract class BoundExpression : BoundSyntaxNode
    {
        public abstract BaseType Type { get; }

        public abstract R Accept<R>(Visitor<R> visitor);

        public interface Visitor<out R>
        {
            public R VisitBoundIdentifierNameExpression(BoundIdentifierName identifierName);
            public R VisitBoundAssignmentOpExpression(BoundAssignmentOp assignmentOp);
            public R VisitBoundUnaryOpExpression(BoundUnaryOp unaryOp);
            public R VisitBoundBinaryOpExpression(BoundBinaryOp binaryOp);
            public R VisitBoundParenthesizedExpression(BoundParenthesized parenthesized);
            public R VisitBoundLiteralExpression(BoundLiteral literal);
            public R VisitBoundObjectLiteralExpression(BoundObjectLiteral objectLiteral);
            public R VisitBoundNoOp(BoundNoOp noOp);
            public R VisitBoundParameter(BoundParameter boundParameter);
            public R VisitBoundInvocationExpression(BoundInvocation invocation);
            public R VisitBoundElementAccessExpression(BoundElementAccess elementAccess);
            public R VisitBoundMemberAccessExpression(BoundMemberAccess memberAccess);
        }
    }

    public abstract class BoundSyntaxNode : SyntaxNode;
}
