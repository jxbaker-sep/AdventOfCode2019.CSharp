using AdventOfCode2019.CSharp;
using FluentAssertions;
using Parser;
using P = Parser.ParserBuiltins;


public class IntCodeComputerTests
{
  [Fact]
  public void RelativeBaseTests()
  {
    var icc = new IntCodeComputer([109,19,99]);
    icc.Run();
    icc.RB.Should().Be(19);
  }

  [Fact]
  public void CopyOfItself()
  {
    List<long> x = [109,1,204,-1,1001,100,1,100,1008,100,16,101,1006,101,0,99];
    var icc = new IntCodeComputer(x);
    List<long> output = [];
    icc.Run(() => throw new ApplicationException(), i => output.Add(i));
    output.Should().BeEquivalentTo(x);
  }

  [Fact]
  public void SixteenDigitOutput()
  {
    var icc = new IntCodeComputer([1102,34915192,34915192,7,4,7,99,0]);
    long output = 0;
    icc.Run(() => throw new ApplicationException(), i => output = i);
    output.Should().Be(1219070632396864L);
  }

  [Fact]
  public void SixteenDigitOutput2()
  {
    var icc = new IntCodeComputer([104,1125899906842624,99]);
    long output = 0;
    icc.Run(() => throw new ApplicationException(), i => output = i);
    output.Should().Be(1125899906842624);
  }

  [Theory]
  [InlineData("3,12,6,12,15,1,13,14,13,4,13,99,-1,0,1,9", 0, 0)]
  [InlineData("3,12,6,12,15,1,13,14,13,4,13,99,-1,0,1,9", 1, 1)]
  [InlineData("3,12,6,12,15,1,13,14,13,4,13,99,-1,0,1,9", -01, 1)]
  [InlineData("3,3,1105,-1,9,1101,0,0,12,4,12,99,1", 0, 0)]
  [InlineData("3,3,1105,-1,9,1101,0,0,12,4,12,99,1", 123, 1)]
  [InlineData("3,3,1105,-1,9,1101,0,0,12,4,12,99,1", -345, 1)]
  [InlineData("3,21,1008,21,8,20,1005,20,22,107,8,21,20,1006,20,31,1106,0,36,98,0,0,1002,21,125,20,4,20,1105,1,46,104,999,1105,1,46,1101,1000,1,20,4,20,1105,1,46,98,99", 
    7, 999)]
  [InlineData("3,21,1008,21,8,20,1005,20,22,107,8,21,20,1006,20,31,1106,0,36,98,0,0,1002,21,125,20,4,20,1105,1,46,104,999,1105,1,46,1101,1000,1,20,4,20,1105,1,46,98,99", 
    8, 1000)]
  [InlineData("3,21,1008,21,8,20,1005,20,22,107,8,21,20,1006,20,31,1106,0,36,98,0,0,1002,21,125,20,4,20,1105,1,46,104,999,1105,1,46,1101,1000,1,20,4,20,1105,1,46,98,99", 
    9, 1001)]
  public void Sanity(string data, long input, long expected)
  {
    var icc = new IntCodeComputer(Convert(data));

    long actual = -1;
    icc.Run(() => input, v => {actual = v;});
    actual.Should().Be(expected);
  }

  [Theory]
  [InlineData("1,0,0,0,99", "2,0,0,0,99")]
  [InlineData("2,3,0,3,99", "2,3,0,6,99")]
  [InlineData("2,4,4,5,99,0", "2,4,4,5,99,9801")]
  [InlineData("1,1,1,4,99,5,6,0,99", "30,1,1,4,2,5,6,0,99")]
  public void Sanity1(string input, string expected)
  {
    var icc = new IntCodeComputer(Convert(input));
    icc.Run();
    icc.Program.Should().BeEquivalentTo(Convert(expected));
  }

  private static List<long> Convert(string data) => P.Long.Plus(",").Parse(data);
}