using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class AssignmentOp(Expression left, Token op, Expression right) : BinaryOp(left, op, right)
{
    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitAssignmentOpExpression(this);

    public override void Display(int indent = 0)
    {
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}AssignmentOp(");
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Left ->");
        Left.Display(indent + 2);
        Console.WriteLine(",");
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Operator: {Operator.Text},");
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Right ->");
        Right.Display(indent + 2);
        Console.WriteLine();
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
    }
}