using System.Runtime.Loader;

namespace ScriptKid;
internal class CollectibleAssemblyLoadContext : AssemblyLoadContext
{
    public CollectibleAssemblyLoadContext() : base(true)
    {
    }
}
