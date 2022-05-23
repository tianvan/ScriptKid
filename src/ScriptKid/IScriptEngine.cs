using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

public interface IScriptEngine
{
    Task<TResult?> RunAsync<TResult>(string script, object? globals = default, CancellationToken cancellationToken = default);

    MethodInfo GetEntryPoint(Assembly assembly);
}