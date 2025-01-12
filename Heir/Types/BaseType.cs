﻿using Heir.Syntax;
using Heir.AST.Abstract;

namespace Heir.Types;

public abstract class BaseType
{
    public abstract TypeKind Kind { get; }

    public abstract string ToString(bool colors = false);

    public static BaseType FromTypeRef(TypeRef typeRef)
    {
        return typeRef switch
        {
            AST.SingularType singularType =>
                singularType.Token.IsKind(SyntaxKind.Identifier)
                    ? new SingularType(singularType.Token.Text)
                    : SyntaxFacts.PrimitiveTypeMap[singularType.Token.Kind],
            
            AST.ParenthesizedType parenthesizedType =>
                new ParenthesizedType(FromTypeRef(parenthesizedType.Type)),
            
            AST.UnionType unionType =>
                new UnionType(unionType.Types.ConvertAll(FromTypeRef)),
            
            AST.IntersectionType intersectionType =>
                new IntersectionType(intersectionType.Types.ConvertAll(FromTypeRef)),
            
            _ => PrimitiveType.None
        };
    }

    public bool IsAssignableTo(BaseType other)
    {
        if (this is AnyType || other is AnyType)
            return true;
        
        if (this is ParenthesizedType parenthesized)
            return parenthesized.Type.IsAssignableTo(other);
        
        if (other is ParenthesizedType otherParenthesized)
            return otherParenthesized.Type.IsAssignableTo(this);
        
        if (this is UnionType union)
            return union.Types.Any(type => type.IsAssignableTo(other));

        if (other is UnionType otherUnion)
            return otherUnion.Types.Any(IsAssignableTo);
        
        if (other is IntersectionType otherIntersection)
            return otherIntersection.Types.All(IsAssignableTo);
        
        if (this is IntersectionType intersection)
            return intersection.Types.Any(type => type.IsAssignableTo(other));
        
        if (this is LiteralType literal && other is LiteralType otherLiteral)
            return literal.Value == otherLiteral.Value;
        
        if (this is SingularType singular && other is SingularType otherSingular)
            return singular.Name == otherSingular.Name;

        return false;
    }
}