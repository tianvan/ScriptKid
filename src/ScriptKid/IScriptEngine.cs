using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

public interface IScriptEngine
{
    Task<TResult> RunAsync<TResult>(string script, object? globals = default);

    MethodInfo GetEntryPoint(Assembly assembly);
}