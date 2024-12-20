﻿using Heir.Syntax;

namespace Heir.AST
{
    public class NoOpStatement : Statement
    {
        public override R Accept<R>(Visitor<R> visitor) => visitor.VisitNoOp(this);
        public override List<Token> GetTokens() => [];
        public override void Display(int indent) => Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))}NoOpStatement");
    }
}
