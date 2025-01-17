﻿using Heir.AST.Abstract;
using Heir.Syntax;

namespace Heir.AST;

public sealed class VariableDeclaration(IdentifierName name, Expression? initializer, TypeRef? type, bool isMutable) : Statement
{
    public IdentifierName Name { get; } = name;
    public Expression? Initializer { get; } = initializer;
    public TypeRef? Type { get; } = type;
    public bool IsMutable { get; } = isMutable;

    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitVariableDeclaration(this);
    public override List<Token> GetTokens() => Name.GetTokens().Concat(Initializer?.GetTokens() ?? []).ToList();

    public override void Display(int indent = 0)
    {
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}VariableDeclaration(");
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Name ->");
        Name.Display(indent + 2);
        Console.WriteLine(",");
        Console.WriteLine();
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Type -> {(Type == null ? "(inferred)" : "\n")}");
        Type?.Display(indent + 2);
        Console.WriteLine(",");
        Console.WriteLine();
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Initializer -> {(Initializer == null ? "none" : "\n")}");
        Initializer?.Display(indent + 2);
        Console.WriteLine(",");
        Console.WriteLine();
        Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Mutable: {IsMutable}");
        Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
    }
}