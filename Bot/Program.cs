using Discord;
using Discord.Commands;
using Discord.Interactive;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Bot
{
	class Program
	{
		private DiscordSocketClient _client;
		private IServiceProvider _services;
		private CommandService _commands;

		static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

		public async Task MainAsync()
		{
			_services = ConfigureServices();

			_client = _services.GetRequiredService<DiscordSocketClient>();
			_client.Log += Log;

			_commands = _services.GetRequiredService<CommandService>();
			var ret = await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

			await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_TOKEN"));
			await _client.StartAsync();

			_client.MessageReceived += HandleCommandAsync;

			await Task.Delay(-1);
		}

		private Task Log(LogMessage message)
		{
			Console.WriteLine(message.ToString());
			return Task.CompletedTask;
		}

		private static IServiceProvider ConfigureServices()
		{
			var client = new DiscordSocketClient(new DiscordSocketConfig
			{
				LogLevel = LogSeverity.Debug,
				MessageCacheSize = 100
			});

			return new ServiceCollection()
				.AddSingleton(client)
				.AddSingleton(new CommandService(new CommandServiceConfig { CaseSensitiveCommands = false, IgnoreExtraArgs = true, DefaultRunMode = RunMode.Async }))
				.AddSingleton<InteractiveService>()
				.BuildServiceProvider();
		}

		private async Task HandleCommandAsync(SocketMessage message)
		{
			var umessage = message as SocketUserMessage;

			if (umessage == null)
				return;

			int argPos = 0;

			if (!umessage.HasMentionPrefix(_client.CurrentUser, ref argPos) || umessage.Author.IsBot)
				return;

			var context = new SocketCommandContext(_client, umessage);
			await _commands.ExecuteAsync(context, argPos, _services);
		}
	}
}
