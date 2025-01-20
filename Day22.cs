using AdventOfCode2019.CSharp.Utils;
using FluentAssertions;
using Parser;
using Utils;
using P = Parser.ParserBuiltins;


namespace AdventOfCode2019.CSharp;
public class Day22
{
  [Theory]
  [InlineData("Day22", 2306)]
  public void Part1(string path, int expected)
  {
    var program = Convert(AoCLoader.LoadLines(path));
    Shuffle(10007, program).IndexOf(2019).Should().Be(expected);
  }

  [Theory]
  [InlineData("deal into new stack", "9 8 7 6 5 4 3 2 1 0")]
  [InlineData("cut 3", "3 4 5 6 7 8 9 0 1 2")]
  [InlineData("cut -4", "6 7 8 9 0 1 2 3 4 5")]
  [InlineData("deal with increment 3", "0 7 4 1 8 5 2 9 6 3")]
  [InlineData(@"deal with increment 7
deal into new stack
deal into new stack", "0 3 6 9 2 5 8 1 4 7")]
  [InlineData(@"cut 6
deal with increment 7
deal into new stack", "3 0 7 4 1 8 5 2 9 6")]
  [InlineData(@"deal with increment 7
deal with increment 9
cut -2", "6 3 0 7 4 1 8 5 2 9")]
  [InlineData(@"deal into new stack
cut -2
deal with increment 7
cut 8
cut -4
deal with increment 7
cut 3
deal with increment 9
deal with increment 3
cut -1", "9 2 5 8 1 4 7 0 3 6")]
  public void Sanity1(string input, string expected)
  {
    Shuffle(10, Convert([.. input.Split("\n")])).Join(" ").Should().Be(expected);
  }

  static List<int> Shuffle(int deckSize, List<Instruction> instructions)
  {
    var deck = Enumerable.Range(0, deckSize).ToList();
    foreach(var instruction in instructions)
    {
      switch (instruction.Technique)
      {
        case Technique.DealIntoNewStack: {
          deck.Reverse();
          break;
        }
        case Technique.Cut: {
          if (instruction.Order >= 0) deck = [..deck[instruction.Order..], ..deck[..instruction.Order]];
          else {
            var order = Math.Abs(instruction.Order);
            deck = [..deck[^order..], ..deck[..^order]];
          }
          break;
        }
        case Technique.DealWithIncrement: {
          var next = Enumerable.Repeat(-1, deck.Count).ToList();
          for(var n = 0; n < deck.Count; n++)
          {
            var x = (n * instruction.Order) % deck.Count;
            next[x].Should().Be(-1);
            next[x] = deck[n];
          }
          deck = next;
          break;
        }
      }
    }
    return deck;
  }

  public enum Technique { DealWithIncrement, Cut, DealIntoNewStack }

  public record Instruction(Technique Technique, int Order);

  private static List<Instruction> Convert(List<string> data) => 
    (P.String("deal into new stack").Select(_ => new Instruction(Technique.DealIntoNewStack, 0))
    | P.Format("deal with increment {}", P.Long).Select(it => new Instruction(Technique.DealWithIncrement, (int)it))
    | P.Format("cut {}", P.Long).Select(it => new Instruction(Technique.Cut, (int)it)))
    .ParseMany(data);

}

// #####.###########
// #####.##.########