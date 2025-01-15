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
        case 5: {
          var param1 = Read(mode1, PC+1);
          if (param1 != 0) PC = (int)Read(mode2, PC+2);
          else PC += 3;
          break;
        }
        case 6: {
          var param1 = Read(mode1, PC+1);
          if (param1 == 0) PC = (int)Read(mode2, PC+2);
          else PC += 3;
          break;
        }
        case 7: {
          var param1 = Read(mode1, PC+1);
          var param2 = Read(mode2, PC+2);
          Program[(int)Program[PC + 3]] = param1 < param2 ? 1 : 0;
          PC += 4;
          break;
        }
        case 8: {
          var param1 = Read(mode1, PC+1);
          var param2 = Read(mode2, PC+2);
          Program[(int)Program[PC + 3]] = param1 == param2 ? 1 : 0;
          PC += 4;
          break;
        }
        default:
          throw new ApplicationException();
      }
    }
  }
}