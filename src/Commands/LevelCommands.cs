﻿using Discord;
using Discord.Commands;
using Dogmeat.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Dogmeat.Commands
{
    public class LevelCommands : ModuleBase
    {
        [Command("rank"), Summary("Displays rank/exp info"), Alias("profile")]
        public async Task Rank(IUser target = null)
        {
            try
            {
                IUser targetUser = target ?? Context.User;
                UUser User;
                int rank;

                using (DatabaseHandler Context = new DatabaseHandler())
                {
                    await Context.Database.EnsureCreatedAsync();
                    User = await Context.Users.FirstOrDefaultAsync(user => user.ID == targetUser.Id);

                    if (User == null)
                    {
                        ReplyAsync($"{targetUser.Mention} is not in the database.");
                        return;
                    }
                    rank = await Context.Users.CountAsync(user => user.Experience > User.Experience) + 1;
                }

                List<Action<EmbedFieldBuilder>> Fields = new List<Action<EmbedFieldBuilder>>
                {
                    await Utilities.Commands.CreateEmbedFieldAsync("Level", User.Level),
                    await Utilities.Commands.CreateEmbedFieldAsync("Experience",
                    $"{User.Experience} / {ExperienceHandler.CalculateLevelThreshold(User.Level)}"),
                    await Utilities.Commands.CreateEmbedFieldAsync("Rank", $"#{rank}"),
                };

                Embed Embed = await Utilities.Commands.CreateEmbedAsync(
                    targetUser.Username + "'s Profile", User.Description,
                    targetUser.GetAvatarUrl(), Fields, Color.Default);

                ReplyAsync("", embed: Embed);
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
            }
        }
    }
}
