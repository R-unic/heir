namespace Heir.AST.Abstract;

public abstract class Statement : SyntaxNode
{
    public abstract R Accept<R>(Visitor<R> visitor);

    public interface Visitor<out R>
    {
        public R VisitSyntaxTree(SyntaxTree syntaxTree);
        public R VisitVariableDeclaration(VariableDeclaration variableDeclaration);
        public R VisitBlock(Block block);
        public R VisitExpressionStatement(ExpressionStatement expressionStatement);
        public R VisitNoOp(NoOpStatement noOp);
    }
}