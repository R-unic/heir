﻿using Heir.Syntax;
using Heir.AST;
using Heir.BoundAST;
using Heir.CodeGeneration;

namespace Heir
{
    public sealed class BytecodeGenerator(DiagnosticBag diagnostics, Binder binder) : Statement.Visitor<List<Instruction>>, Expression.Visitor<List<Instruction>>
    {
        private readonly DiagnosticBag _diagnostics = diagnostics;
        private readonly Binder _binder = binder;
        private readonly SyntaxTree _syntaxTree = binder.SyntaxTree;

        public Bytecode GenerateBytecode() => new Bytecode(GenerateBytecode(_syntaxTree), _diagnostics);

        public List<Instruction> VisitSyntaxTree(SyntaxTree syntaxTree) =>
            GenerateStatementsBytecode(syntaxTree.Statements)
            .Append(new Instruction(syntaxTree, OpCode.EXIT))
            .ToList();

        // TODO: create scope
        public List<Instruction> VisitBlock(Block block) =>
            GenerateStatementsBytecode(block.Statements)
            .Prepend(new Instruction(block, OpCode.BEGINSCOPE))
            .Append(new Instruction(block, OpCode.ENDSCOPE))
            .ToList();

        public List<Instruction> VisitVariableDeclaration(VariableDeclaration variableDeclaration) =>
            new List<Instruction>([new Instruction(variableDeclaration.Name, OpCode.PUSH, variableDeclaration.Name.Token.Text)])
            .Concat(variableDeclaration.Initializer != null ? GenerateBytecode(variableDeclaration.Initializer) : [])
            .Append(new Instruction(variableDeclaration, OpCode.STORE, false))
            .ToList();

        public List<Instruction> VisitExpressionStatement(ExpressionStatement expressionStatement) => GenerateBytecode(expressionStatement.Expression);

        public List<Instruction> VisitNoOp(NoOp noOp) => NoOp(noOp);
        public List<Instruction> VisitNoOp(NoOpStatement noOp) => NoOp(noOp);
        public List<Instruction> VisitNoOp(NoOpType noOp) => NoOp(noOp);
        public List<Instruction> VisitSingularTypeRef(SingularType singularType) => NoOp(singularType);
        public List<Instruction> VisitUnionTypeRef(UnionType unionType) => NoOp(unionType);

        public List<Instruction> VisitAssignmentOpExpression(AssignmentOp assignmentOp) => VisitBinaryOpExpression(assignmentOp);
        public List<Instruction> VisitBinaryOpExpression(BinaryOp binaryOp)
        {
            var boundBinaryOp = _binder.GetBoundNode(binaryOp) as BoundBinaryOp;
            if (boundBinaryOp == null)
                return NoOp(binaryOp);

            var leftInstructions = GenerateBytecode(binaryOp.Left);
            var rightInstructions = GenerateBytecode(binaryOp.Right);
            var combined = leftInstructions.Concat(rightInstructions);
            var boundOperatorType = boundBinaryOp.Operator.Type;

            if (boundOperatorType == BoundBinaryOperatorType.Assignment)
                return PushName((Name)binaryOp.Left)
                       .Concat(rightInstructions)
                       .Append(new Instruction(binaryOp, OpCode.STORE))
                       .ToList();

            if (BoundBinaryOperator.InvertedOperations.TryGetValue(boundOperatorType, out var invertedOpCode))
                return combined
                       .Append(new Instruction(binaryOp, invertedOpCode))
                       .Append(new Instruction(binaryOp, OpCode.NOT))
                       .ToList();

            if (BoundBinaryOperator.OpCodeMap.TryGetValue(boundOperatorType, out var opCode))
            {
                return (!SyntaxFacts.BinaryCompoundAssignmentOperators.Contains(binaryOp.Operator.Kind) ?
                    combined.Append(new Instruction(binaryOp, opCode))
                    : PushName((Name)binaryOp.Left)
                        .Concat(leftInstructions)
                        .Concat(rightInstructions)
                        .Append(new Instruction(binaryOp, opCode))
                        .Append(new Instruction(binaryOp, OpCode.STORE))
                ).ToList();
            }

            _diagnostics.Error(DiagnosticCode.H008, $"Unsupported binary operator kind: {binaryOp.Operator.Kind}", binaryOp.Operator);
            return NoOp(binaryOp);
        }

        public List<Instruction> VisitUnaryOpExpression(UnaryOp unaryOp)
        {
            var value = GenerateBytecode(unaryOp.Operand);
            var bytecode = unaryOp.Operator.Kind switch
            {
                SyntaxKind.Bang => value.Append(new Instruction(unaryOp, OpCode.NOT)),
                SyntaxKind.Tilde => value.Append(new Instruction(unaryOp, OpCode.BNOT)),
                SyntaxKind.Minus => value.Append(new Instruction(unaryOp, OpCode.UNM)),

                SyntaxKind.PlusPlus => PushName((Name)unaryOp.Operand)
                                        .Concat(value.Append(new Instruction(unaryOp, OpCode.PUSH, 1)))
                                        .Concat([
                                            new Instruction(unaryOp, OpCode.ADD),
                                            new Instruction(unaryOp, OpCode.STORE),
                                            new Instruction(unaryOp, OpCode.PUSH, 1),
                                            new Instruction(unaryOp, OpCode.SUB),
                                        ]),

                SyntaxKind.MinusMinus => PushName((Name)unaryOp.Operand)
                                        .Concat(value.Append(new Instruction(unaryOp, OpCode.PUSH, 1)))
                                        .Concat([
                                            new Instruction(unaryOp, OpCode.SUB),
                                            new Instruction(unaryOp, OpCode.STORE),
                                            new Instruction(unaryOp, OpCode.PUSH, 1),
                                            new Instruction(unaryOp, OpCode.ADD),
                                        ]),

                _ => null!
            };

            return bytecode.ToList();
        }

        public List<Instruction> VisitIdentifierNameExpression(IdentifierName identifierName) => [
            new Instruction(identifierName, OpCode.PUSH, identifierName.Token.Text),
            new Instruction(identifierName, OpCode.LOAD)
        ];

        public List<Instruction> VisitParenthesizedExpression(Parenthesized parenthesized) => GenerateBytecode(parenthesized.Expression);
        public List<Instruction> VisitLiteralExpression(Literal literal) =>
            [new Instruction(literal, literal.Token.Value != null ? OpCode.PUSH : OpCode.PUSHNONE, literal.Token.Value)];
        public List<Instruction> VisitObjectLiteralExpression(ObjectLiteral objectLiteral)
        {
            // store objects as dictionaries, for now (this may be permanent tbh)
            var objectValue = new Dictionary<List<Instruction>, List<Instruction>>(
                objectLiteral.Properties
                    .ToList()
                    .ConvertAll<KeyValuePair<List<Instruction>, List<Instruction>>>(property =>
                    {
                        Expression keyExpression = property.Key;
                        if (keyExpression is IdentifierName identifier)
                            keyExpression = new Literal(TokenFactory.StringFromIdentifier(identifier.Token));

                        var key = GenerateBytecode(keyExpression);
                        var value = GenerateBytecode(property.Value);
                        return new(key, value);
                    })
            );

            return [new Instruction(objectLiteral, OpCode.PUSHOBJECT, objectValue)];
        }

        private List<Instruction> PushName(Name name) => [new Instruction(name, OpCode.PUSH, name.ToString())];
        private List<Instruction> NoOp(SyntaxNode node) => [new Instruction(node, OpCode.NOOP)];

        private List<Instruction> GenerateStatementsBytecode(List<Statement> statements) => statements.SelectMany(GenerateBytecode).ToList();
        private List<Instruction> GenerateBytecode(Expression expression) => expression.Accept(this);
        private List<Instruction> GenerateBytecode(Statement statement) => statement.Accept(this);
        private List<Instruction> GenerateBytecode(SyntaxNode node)
        {
            if (node is Expression expression)
                return GenerateBytecode(expression);
            else if (node is Statement statement)
                return GenerateBytecode(statement);

            return null!; // poop
        }
    }
}
