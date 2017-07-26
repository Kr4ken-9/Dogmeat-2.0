﻿using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace DogMeat
{
    class Program
    {
        static void Main(string[] args) => new Program().RunAsync().GetAwaiter().GetResult();

        public async Task RunAsync()
        {
            Vars.Client = new DiscordSocketClient();
            Vars.CService = new CommandService();

            await Vars.Client.LoginAsync(TokenType.Bot, Vars.Token);
            await Vars.Client.StartAsync();

            Vars.ISProvider = new ServiceCollection().BuildServiceProvider();

            Vars.Client.Ready += OnStart;

            await Task.Delay(-1);
        }

        private async Task OnStart()
        {
            Vars.PointBlank = Vars.Client.GetGuild(332435336921612299);
            Vars.Main = Vars.Client.GetGuild(281249097770598402);
            Vars.Commands = await Vars.Main.GetChannelAsync(297587358063394816);
            Vars.Logging = await Vars.Main.GetChannelAsync(297587378804097025);

            MessageHandler.InitializeCommandHandler();

            MessageHandler.InitializeGeneralHandler();

            MessageHandler.InitializeOwnerCommandsHandler();
            
            #region Continous Tasks
            
            CancellationTokenSource Token = new CancellationTokenSource();
            
            new Task(() => Utilities.MaintainConnection(), Token.Token, TaskCreationOptions.LongRunning).Start();

            new Task(() => Utilities.UpdateVars(), Token.Token, TaskCreationOptions.LongRunning).Start();
            
            #endregion
        }

    }
}