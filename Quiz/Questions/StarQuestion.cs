using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace Quiztellation.Quiz.Questions;

public class StarQuestion : IQuestion
{
    private readonly string _answer;
    private readonly string _question;

    public StarQuestion(string question, string answer, int point)
    {
        _question = question;
        _answer = answer;
        Point = point;
    }

    public int Point { get; private set; }

    public async Task<bool> ProcessQuestion(DiscordClient client, DiscordChannel channel, DiscordUser user)
    {
        var questionEmbed = new DiscordEmbedBuilder
        {
            Title = _question,
            Color = DiscordColor.Azure
        };
        questionEmbed.AddField("Points", $"{Point}");
        questionEmbed.WithFooter("type \"exit\" to end the quiz");
        await channel.SendMessageAsync(questionEmbed);

        var interactivity = client.GetInteractivity();

        while (true)
        {
            var message =
                await interactivity.WaitForMessageAsync(x => x.ChannelId == channel.Id && x.Author.Id == user.Id);

            if (message.TimedOut)
            {
                var timedOutEmbed = new DiscordEmbedBuilder
                {
                    Title = "No interactivity",
                    Color = DiscordColor.Red
                };
                await channel.SendMessageAsync(timedOutEmbed);
                return true;
            }
            
            var ansCap = _answer.Split(", ").Select(x => x.ToUpper()).ToList();

            if (string.Equals(message.Result.Content, "exit", StringComparison.CurrentCultureIgnoreCase)) return true;

            if (ansCap.Contains(message.Result.Content.ToUpper()))
            {
                var correctEmbed = new DiscordEmbedBuilder
                {
                    Title = "Correct!",
                    Color = DiscordColor.Green
                };
                correctEmbed.AddField("Answer", $"{_answer}");
                await message.Result.RespondAsync(correctEmbed);
                return false;
            }

            var incorrectEmbed = new DiscordEmbedBuilder
            {
                Title = "Incorrect!",
                Color = DiscordColor.Red
            };
            incorrectEmbed.AddField("Answer", $"{_answer}");
            await message.Result.RespondAsync(incorrectEmbed);
            Point = 0;
            return false;
        }
    }
}