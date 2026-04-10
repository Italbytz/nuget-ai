using System.Runtime.CompilerServices;
using Microsoft.ML;

namespace Italbytz.AI.ML.Core;

public static class ThreadSafeMLContext
{
    private static int? _seed;

    [ThreadStatic]
    private static MLContext? _localContext;

    public static int? Seed
    {
        get => _seed;
        set
        {
            _seed = value;
            _localContext = null;
        }
    }

    public static MLContext LocalMLContext => _localContext ?? Create();

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static MLContext Create()
    {
        return _localContext = _seed.HasValue ? new MLContext(_seed.Value) : new MLContext();
    }
}
