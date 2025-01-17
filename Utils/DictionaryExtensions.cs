namespace Utils;
#pragma warning disable CS8714

public static class DictionaryExtensions
{
  public static Dictionary<T1, T2> Clone<T1, T2>(this Dictionary<T1, T2> self) where T1 : notnull => self.ToDictionary(it => it.Key, it => it.Value);

  public static Dictionary<T2, List<T3>> GroupToDictionary<T1, T2, T3>(this IEnumerable<T1> self, Func<T1, T2> key, Func<T1, T3> value)
  {
    return self.GroupBy(it => key(it), it => value(it)).ToDictionary(it => it.Key, it => it.ToList());
  }
}
#pragma warning restore CS8714