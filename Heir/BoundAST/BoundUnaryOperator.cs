using Heir.Syntax;
using Heir.Types;
using Heir.CodeGeneration;

namespace Heir.BoundAST
{
    public enum BoundUnaryOperatorType
    {
        Negate,
        Increment,
        Decrement,
        LogicalNot,
        BitwiseNot
    }

    public sealed class BoundUnaryOperator
    {
        public static readonly Dictionary<BoundUnaryOperatorType, OpCode> OpCodeMap = new()
        {
            { BoundUnaryOperatorType.Negate,     OpCode.UNM },
            { BoundUnaryOperatorType.Increment,  OpCode.ADD },
            { BoundUnaryOperatorType.Decrement,  OpCode.SUB },
            { BoundUnaryOperatorType.LogicalNot, OpCode.NOT },
            { BoundUnaryOperatorType.BitwiseNot, OpCode.BNOT }
        };

        public SyntaxKind SyntaxKind { get; }
        public BoundUnaryOperatorType Type { get; }
        public BaseType OperandType { get; }
        public BaseType ResultType { get; }

        private BoundUnaryOperator(SyntaxKind syntaxKind, BoundUnaryOperatorType type, BaseType operandType)
            : this(syntaxKind, type, operandType, operandType)
        {
        }

        private BoundUnaryOperator(SyntaxKind syntaxKind, BoundUnaryOperatorType type, BaseType operandType, BaseType resultType)
        {
            SyntaxKind = syntaxKind;
            Type = type;
            OperandType = operandType;
            ResultType = resultType;
        }

        private static readonly BoundUnaryOperator[] _operators =
        [
            new(SyntaxKind.Minus, BoundUnaryOperatorType.Negate, IntrinsicTypes.Number),
            new(SyntaxKind.PlusPlus, BoundUnaryOperatorType.Increment, IntrinsicTypes.Number),
            new(SyntaxKind.MinusMinus, BoundUnaryOperatorType.Decrement, IntrinsicTypes.Number),
            new(SyntaxKind.Bang, BoundUnaryOperatorType.LogicalNot, PrimitiveType.Bool),
            new(SyntaxKind.Tilde, BoundUnaryOperatorType.BitwiseNot, PrimitiveType.Int)
        ];

        public static BoundUnaryOperator? Bind(Token token, BaseType operandType)
        {
            if (operandType is LiteralType literalType)
                operandType = literalType.AsPrimitive();
            
            foreach (var op in _operators)
            {
                if (!token.IsKind(op.SyntaxKind)) continue;
                if (!op.OperandType.IsAssignableTo(operandType)) continue;

                return op;
            }

            return null;
        }
    }
}
