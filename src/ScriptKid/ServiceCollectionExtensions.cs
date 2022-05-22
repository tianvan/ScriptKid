using ScriptKid;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddScriptKid(this IServiceCollection services)
    {
        services.AddOptions<ScriptEngineOptions>();
        services.AddSingleton<IScriptFormatter, ScriptFormatter>();
        services.AddSingleton<IScriptDigestComputer, ScriptDigestComputer>();
        services.AddSingleton<ICompilationInfoStorage, CompilationInfoStorage>();
        services.AddSingleton<IScriptEngine, ScriptEngine>();

        return services;
    }
}
