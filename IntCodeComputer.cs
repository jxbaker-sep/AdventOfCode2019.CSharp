namespace AdventOfCode2019.CSharp;

class IntCodeComputer(List<long> Program)
{
  private int PC = 0;

  public List<long> Program { get; } = Program;

  public void Run() => Run(() => throw new ApplicationException("No input handler"), _ => {});

  public void Run(Func<long> input, Action<long> output)
  {
    long Read(long mode, int x) => mode switch {
      0 => Program[(int)Program[x]],
      1 => Program[x],
      _ => throw new ApplicationException("Unsupported mode")
    };
    while (true)
    {
      var code = Program[PC];
      var opcode = code % 100;
      var mode1 = (code / 100) % 10;
      var mode2 = (code / 1000) % 10;
      var mode3 = (code / 10000) % 10;
      switch (opcode)
      {
        case 99:
          return;
        case 1:
          Program[(int)Program[PC + 3]] = Read(mode1, PC+1) + Read(mode2, PC + 2);
          PC += 4;
          break;
        case 2:
          Program[(int)Program[PC + 3]] = Read(mode1, PC+1) * Read(mode2, PC + 2);
          PC += 4;
          break;
        case 3:
          Program[(int)Program[PC+1]] = input();
          PC += 2;
          break;
        case 4:
          output(Read(mode1, PC+1));
          PC += 2;
          break;
        default:
          throw new ApplicationException();
      }
    }
  }
}