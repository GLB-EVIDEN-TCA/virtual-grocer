using System.Text;

namespace Eviden.VirtualGrocer.Shared;

public class LoremIpsumGeneration
{
    private static readonly string[] Sentences =
    {
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
        "Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
        "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris.",
        "Duis aute irure dolor in reprehenderit in voluptate velit.",
        "Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
        "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium."
    };

    public static string GenerateLoremIpsumSentence()
    {
        var random = new Random();
        return Sentences[random.Next(Sentences.Length)];
    }

    public static string GenerateLoremIpsumText()
    {
        var random = new Random();
        var result = new StringBuilder();

        for (int i = 0, numSentences = random.Next(1, 3); i < numSentences; i++)
        {
            var sentence = GenerateLoremIpsumSentence();
            result.Append($"{sentence} ");
        }

        return result.ToString().Trim();
    }
}