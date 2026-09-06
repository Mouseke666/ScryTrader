using System.Text.RegularExpressions;
using ScryTrader.Models;

namespace ScryTrader.Services;

public class MoxfieldDeckParser
{
    private static readonly Regex CardLineRegex = new(
        @"^(?<quantity>\d+)\s+(?<name>.+?)\s+\((?<set>[A-Z0-9]+)\)\s+(?<collector>[^\s]+)(?:\s+(?<finish>\*[FE]\*))?$",
        RegexOptions.Compiled);

    public Deck Parse(string fileName)
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, fileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Moxfield decklist not found.", filePath);
        }

        var lines = File.ReadAllLines(filePath);

        var deck = new Deck();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (line.StartsWith("SIDEBOARD:", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var match = CardLineRegex.Match(line);

            if (!match.Success)
            {
                throw new FormatException($"Could not process Moxfield line: '{line}'");
            }

            var card = new DeckCard
            {
                Quantity = int.Parse(match.Groups["quantity"].Value),
                Printing = new CardPrinting
                {
                    Name = match.Groups["name"].Value,
                    SetCode = match.Groups["set"].Value,
                    CollectorNumber = match.Groups["collector"].Value,
                    Finish = ParseFinish(match.Groups["finish"].Value)
                }
            };

            deck.Cards.Add(card);
        }

        return deck;
    }

    private static CardFinish ParseFinish(string finish)
    {
        return finish switch
        {
            "*F*" => CardFinish.Foil,
            "*E*" => CardFinish.Etched,
            _ => CardFinish.NonFoil
        };
    }
}