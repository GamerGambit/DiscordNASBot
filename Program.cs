using Discord;
using Discord.WebSocket;

using System;
using System.Threading.Tasks;

namespace DiscordNASBot
{
	class Program
	{
		static void Main(string[] args)
		{
			new Program().MainAsync().GetAwaiter().GetResult();
		}

		private DiscordSocketClient client;
		public async Task MainAsync()
		{
			client = new DiscordSocketClient();
			client.Log += Log;

			var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");

			await client.LoginAsync(TokenType.Bot, token);
			await client.StartAsync();

			await Task.Delay(-1);
		}

		private Task Log(LogMessage message)
		{
			Console.WriteLine(message.ToString());
			return Task.CompletedTask;
		}
	}
}
