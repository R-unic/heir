﻿using Heir.BoundAST;
using Heir.Types;

namespace Heir
{
    public class TypeChecker(BoundSyntaxTree syntaxTree) : BoundStatement.Visitor<object?>, BoundExpression.Visitor<object?>
    {
        private readonly BoundSyntaxTree _syntaxTree = syntaxTree;
        private readonly DiagnosticBag _diagnostics = syntaxTree.Diagnostics;

        public void Check() => Check(_syntaxTree);

        public object? VisitBoundSyntaxTree(BoundSyntaxTree syntaxTree) => VisitBoundBlock(syntaxTree);

        public object? VisitBoundAssignmentOpExpression(BoundAssignmentOp assignmentOp)
        {
            Check(assignmentOp.Right);
            Assert(assignmentOp.Right, assignmentOp.Operator.RightType);
            return null;
        }

        public object? VisitBoundBinaryOpExpression(BoundBinaryOp binaryOp)
        {
            Check(binaryOp.Left);
            Check(binaryOp.Right);
            Assert(binaryOp.Left, binaryOp.Operator.LeftType);
            Assert(binaryOp.Right, binaryOp.Operator.RightType);
            return null;
        }

        public object? VisitBoundBlock(BoundBlock block) => CheckStatements(block.Statements);

        public object? VisitBoundExpressionStatement(BoundExpressionStatement expressionStatement) => Check(expressionStatement.Expression);

        public object? VisitBoundIdentifierNameExpression(BoundIdentifierName identifierName) => null;
        public object? VisitBoundLiteralExpression(BoundLiteral literal) => null;
        public object? VisitBoundNoOp(BoundNoOp noOp) => null;
        public object? VisitBoundNoOp(BoundNoOpStatement noOp) => null;

        public object? VisitBoundParenthesizedExpression(BoundParenthesized parenthesized) => Check(parenthesized.Expression);

        public object? VisitBoundUnaryOpExpression(BoundUnaryOp unaryOp)
        {
            Check(unaryOp.Operand);
            Assert(unaryOp.Operand, unaryOp.Operator.OperandType);
            return null;
        }

        public object? VisitBoundVariableDeclaration(BoundVariableDeclaration variableDeclaration)
        {
            if (variableDeclaration.Initializer == null) return null;
            Check(variableDeclaration.Initializer);

            // TODO: forgo this assertion if the initializer is an ArrayType
            Assert(variableDeclaration.Initializer, variableDeclaration.Symbol.Type);
            return null;
        }

        private object? CheckStatements(List<BoundStatement> statements)
        {
            statements.ForEach(statement => Check(statement));
            return null;
        }

        private object? Check(BoundExpression expression) => expression.Accept(this);
        private object? Check(BoundStatement statement) => statement.Accept(this);
        private object? Check(BoundSyntaxNode node)
        {
            if (node is BoundExpression expression)
                Check(expression);
            else if (node is BoundStatement statement)
                Check(statement);

            return null;
        }

        private void Assert(BoundExpression node, BaseType type, string? message = null)
        {
            if (node.Type.IsAssignableTo(type)) return;
            _diagnostics.Error(DiagnosticCode.H007, message ?? $"Type {node.Type.ToString()} is not assignable to type {type.ToString()}", node.GetFirstToken());
        }
    }
}
