# ScriptKid

ScriptKid can run C# script and avoid memory leaks in Linux environments.

1. ScriptKid will format the C# script.
2. Then compute the Hash of the formatted C# Script through SHA256 algorithm.
3. Compile the formatted C# script into the assembly.
4. Cache the Assembly with the Hash primary key.
5. Finally using collectible AssemblyLoadContext load the assembly and run it.
6. Unload collectible AssemblyLoadContext. 

## Usage

```
services.AddScriptKid();
```
