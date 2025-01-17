using Heir.Runtime.Intrinsics.Global;

namespace Heir.Runtime.Intrinsics;

public static class Intrinsics
{
    private static readonly List<IntrinsicValue> _intrinsics =
    [
        new PrintFunction(),
        new NowFunction()
    ];
    
    private static readonly List<IntrinsicRegistrar> _registrars =
        _intrinsics.ConvertAll(intrinsic => new IntrinsicRegistrar(intrinsic));

    private static readonly List<IntrinsicRegistrar> _globalRegistrars =
        _registrars.FindAll(registrar => registrar.IsGlobal);

    public static void RegisterGlobalSymbols(Binder binder)
    {
        foreach (var registrar in _globalRegistrars)
            registrar.RegisterSymbol(binder);
    }

    public static void RegisterResolverGlobals(Resolver resolver)
    {
        foreach (var registrar in _globalRegistrars)
            registrar.RegisterInResolver(resolver);
    }
    
    public static void RegisterGlobalValues(Scope scope)
    {
        foreach (var registrar in _globalRegistrars)
            registrar.RegisterValue(scope);
    }
}