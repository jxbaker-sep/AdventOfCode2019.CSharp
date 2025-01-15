namespace AdventOfCode2019.CSharp;

class IntCodeComputer(List<long> Program)
{
  private int PC = 0;

  public List<long> Program { get; } = Program;

  public void Run()
  {
    while (true)
    {
      switch (Program[PC])
      {
        case 99:
          return;
        case 1:
          Program[(int)Program[PC + 3]] = Program[(int)Program[PC + 1]] + Program[(int)Program[PC + 2]];
          PC += 4;
          break;
        case 2:
          Program[(int)Program[PC + 3]] = Program[(int)Program[PC + 1]] * Program[(int)Program[PC + 2]];
          PC += 4;
          break;
        default:
          throw new ApplicationException();
      }
    }
  }
}