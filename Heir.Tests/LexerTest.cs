using Heir.Syntax;

namespace Heir.Tests;

public class LexerTest
{
    [Theory]
    [InlineData("1.2.3", DiagnosticCode.H003)]
    [InlineData("'c", DiagnosticCode.H002B)]
    [InlineData("\"ab", DiagnosticCode.H002)]
    [InlineData(@"\\", DiagnosticCode.H001)]
    public void ThrowsWith(string input, DiagnosticCode expectedErrorCode)
    {
        var tokenStream = Tokenize(input);
        Assert.True(tokenStream.Diagnostics.HasErrors);
        Assert.Contains(tokenStream.Diagnostics, diagnostic => diagnostic.Code == expectedErrorCode);
    }

    [Theory]
    [InlineData("\"abc\"", SyntaxKind.StringLiteral, "abc")]
    [InlineData("'a'", SyntaxKind.CharLiteral, 'a')]
    [InlineData("123", SyntaxKind.IntLiteral, 123L)]
    [InlineData("69", SyntaxKind.IntLiteral, 69L)]
    [InlineData("0b1101", SyntaxKind.IntLiteral, 13L)]
    [InlineData("0o420", SyntaxKind.IntLiteral, 272L)]
    [InlineData("0x03E", SyntaxKind.IntLiteral, 62L)]
    [InlineData("123.456", SyntaxKind.FloatLiteral, 123.456)]
    [InlineData("69.420", SyntaxKind.FloatLiteral, 69.420)]
    [InlineData("true", SyntaxKind.BoolLiteral, true)]
    [InlineData("false", SyntaxKind.BoolLiteral, false)]
    [InlineData("none", SyntaxKind.NoneKeyword, null)]
    public void Tokenizes_Literals(string input, SyntaxKind expectedKind, object expectedValue)
    {
        var tokenStream = Tokenize(input);
        var literalToken = tokenStream.First();

        Assert.Equal(expectedKind, literalToken.Kind);
        Assert.Equal(input, literalToken.Text);
        Assert.Equal(expectedValue, literalToken.Value);
    }

    [Fact]
    public void Tokenizes_Operators()
    {
        foreach (var (input, kind) in SyntaxFacts.OperatorMap.Forward)
        {
            var tokenStream = Tokenize(input);
            var token = tokenStream.First();

            Assert.Equal(kind, token.Kind);
            Assert.Equal(input, token.Text);
            Assert.Null(token.Value);
        }
    }

    [Theory]
    [InlineData(".", SyntaxKind.Dot)]
    [InlineData(":", SyntaxKind.Colon)]
    [InlineData("::", SyntaxKind.ColonColon)]
    [InlineData(",", SyntaxKind.Comma)]
    public void Tokenizes_MiscSymbols(string input, SyntaxKind expectedKind)
    {
        var tokenStream = Tokenize(input);
        var token = tokenStream.First();

        Assert.Equal(expectedKind, token.Kind);
        Assert.Equal(input, token.Text);
        Assert.Null(token.Value);
    }

    [Theory]
    [InlineData("let", SyntaxKind.LetKeyword)]
    [InlineData("mut", SyntaxKind.MutKeyword)]
    [InlineData("int", SyntaxKind.IntKeyword)]
    [InlineData("float", SyntaxKind.FloatKeyword)]
    [InlineData("string", SyntaxKind.StringKeyword)]
    [InlineData("char", SyntaxKind.CharKeyword)]
    [InlineData("bool", SyntaxKind.BoolKeyword)]
    [InlineData("none", SyntaxKind.NoneKeyword)]
    public void Tokenizes_Keywords(string input, SyntaxKind expectedKind)
    {
        var tokenStream = Tokenize(input);
        var token = tokenStream.First();

        Assert.Equal(expectedKind, token.Kind);
        Assert.Equal(input, token.Text);
        Assert.Null(token.Value);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("abc123")]
    [InlineData("abc_123")]
    public void Tokenizes_Identifiers(string input)
    {
        var tokenStream = Tokenize(input);
        var token = tokenStream.First();

        Assert.Equal(SyntaxKind.Identifier, token.Kind);
        Assert.Equal(input, token.Text);
        Assert.Null(token.Value);
    }

    private TokenStream Tokenize(string input) => Common.Tokenize(input).WithoutTrivia();
}
