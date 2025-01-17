﻿using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public class NoOp : Expression
{
    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitNoOp(this);
    public override List<Token> GetTokens() => [];
    public override void Display(int indent = 0) => Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))}NoOp");
}