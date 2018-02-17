﻿using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Dogmeat.Utilities;
using Microsoft.EntityFrameworkCore;
using System;

namespace Dogmeat.Database.Servers
{
    public class TimeSensitiveHandler
    {
        public static async Task RunConstantChecks()
        {
            while (Vars.KeepAlive)
            {
                using (DatabaseHandler Context = new DatabaseHandler())
                {
                    await Context.Database.EnsureCreatedAsync();

                    var TempBans = Context.TempBans.Where(t => t.UnbanTime <= Vars.Now());
                    if (await TempBans.AnyAsync())
                    {
                        foreach (TempBan Ban in TempBans)
                        {
                            SocketGuild Guild = Vars.Client.GetGuild(Ban.ServerID);
                            SocketUser User = Vars.Client.GetUser(Ban.ID);

                            if (User == null || Guild == null)
                                continue;

                            try { await Guild.RemoveBanAsync(Ban.ID); }
                            catch (Discord.Net.HttpException e) { }

                            (await User.GetOrCreateDMChannelAsync()).SendMessageAsync(
                                $"You have been unbanned from {Guild.Name}");

                            Context.TempBans.Remove(Ban); //TODO: Why does this hang thread?!?!

                            Logger.Log($"Unbanned {User.Username}");
                        }
                    }

                    var Reminders = Context.Reminders.Where(r => r.RemindDate <= Vars.Now());
                    if (await Reminders.AnyAsync())
                    {
                        foreach (Reminder Reminder in Reminders)
                        {
                            SocketUser User = Vars.Client.GetUser(Reminder.ID);
                            (await User.GetOrCreateDMChannelAsync()).SendMessageAsync($"Reminder: {Reminder.Content}");

                            SocketGuild Guild = Vars.Client.GetGuild(Reminder.ServerID);
                            SocketTextChannel Channel = Guild.GetTextChannel(Reminder.ChannelID);
                            Channel.SendMessageAsync($"Reminder for {User.Mention}: {Reminder.Content}");

                            Context.Reminders.Remove(Reminder);
                        }
                    }

                    await Context.SaveChangesAsync();

                }
                Logger.Log("Time Sensitives Checked");
                Thread.Sleep(60000);
            }
            Task.Delay(-1);
        }
    }
}