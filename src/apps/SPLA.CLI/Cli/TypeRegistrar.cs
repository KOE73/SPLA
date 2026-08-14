using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace SPLA.CLI;

/// <summary>Standard Spectre.Console.Cli DI bridge: lets command classes take <see cref="ILoggerFactory"/>
/// and <see cref="Domain.Settings.ResolvedSettings"/> as constructor parameters instead of re-resolving
/// them by hand in every <c>Execute</c>.</summary>
internal sealed class TypeRegistrar(IServiceCollection services) : ITypeRegistrar
{
    public ITypeResolver Build() => new TypeResolver(services.BuildServiceProvider());
    public void Register(Type service, Type implementation) => services.AddSingleton(service, implementation);
    public void RegisterInstance(Type service, object implementation) => services.AddSingleton(service, implementation);
    public void RegisterLazy(Type service, Func<object> factory) => services.AddSingleton(service, _ => factory());
}

internal sealed class TypeResolver(IServiceProvider provider) : ITypeResolver, IDisposable
{
    public object? Resolve(Type? type) => type == null ? null : provider.GetService(type);
    public void Dispose() { if (provider is IDisposable d) d.Dispose(); }
}
