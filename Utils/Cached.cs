namespace AdventOfCode2019.CSharp.Utils;

public static class Cached
{
  public static Func<TIn1, TIn2, TOut> Create<TIn1, TIn2, TOut>(Func<TIn1, TIn2, TOut> func)
  {
    Dictionary<(TIn1, TIn2), TOut> cache = [];
    return (i1, i2) => {
      var k = (i1, i2);
      if (cache.TryGetValue(k, out var cached)) return cached;

      var value = func(i1, i2);
      cache[k] = value;
      return value;
    };
  }
}