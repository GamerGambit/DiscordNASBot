
using Discord;
using Discord.Commands;
using Discord.Interactive;
using Discord.WebSocket;

using RadarrSharp.Enums;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bot.Modules
{
	public class AddModule : InteractiveBase<SocketCommandContext>
	{
		[Command("Add")]
		[Summary("Adds a movie or TV show")]
		public async Task AddAsync([Remainder][Summary("Name of the movie or TV show to add")] string term)
		{
			try
			{
				var radarrResults = await Radarr.Client.Movie.SearchForMovie(term);
				var sonarrResults = await Sonarr.Client.SeriesLookup.SearchForSeries(term);

				var results = radarrResults.Select(m => new MediaItem
				{
					MediaType = MediaType.Movie,
					Title = m.Title,
					Year = m.Year,
					TmdbId = m.TmdbId,
					Images = m.Images.Select(i => Image.FromRadarrImage(i)),
					Overview = m.Overview,
					Seasons = null
				}).Concat(sonarrResults.Select(s => new MediaItem
				{
					MediaType = MediaType.Show,
					Title = s.Title,
					Year = s.Year,
					TmdbId = s.TvdbId,
					Overview = s.Overview,
					Images = s.Images.Select(i => Image.FromSonarrImage(i)),
					Seasons = s.Seasons.Select(s => { s.Monitored = true; return s; })
				}));

				if (!results.Any())
				{
					await Context.Channel.SendMessageAsync($"No results found for \"{term}\".");
					return;
				}

				var resultsCount = results.Count();
				var pg = new PaginatedMessage().AddPages(
					results.Select((result, i) => new EmbedBuilder()
							.WithColor(result.Exists ? Color.Green : Color.LightGrey)
							.WithTitle(string.Format("{0} ({1})", result.Title, result.Year))
							.WithDescription(result.Overview)
							.WithThumbnailUrl(result.Images.FirstOrDefault().URL)
							.AddField("Exists in library", result.Exists ? "Yes" : "No")
							.WithFooter(string.Format("Result {0}/{1}", i + 1, resultsCount)))
				);

				var paginatedMessage = await SendPaginatedMessage(Context, pg, content: "Please select a page number, or multiple comma separated page numbers, to add.");

				bool hasReactions = true;
				async Task reactionHandle(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel)
				{
					var msg = await cache.DownloadAsync();
					if (msg == paginatedMessage)
					{
						hasReactions = false;
					}
				};

				Context.Client.ReactionsCleared += reactionHandle;

				var reply = await NextMessageAsync(new NextMessageCriteria(Context), Interactive.DefaultPaginatorTimeout);
				if (reply != null && hasReactions)
				{
					try
					{
						await paginatedMessage.RemoveAllReactionsAsync();

						var replyText = reply.Content;
						var pageIndicesToAdd = replyText.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(str => int.Parse(str));
						var mediaToAdd = pageIndicesToAdd
							.Select(i => results.ElementAt(i - 1))
							.Where(mi => !mi.Exists);

						if (!mediaToAdd.Any())
						{
							await ReplyAsync("Nothing was added, since it already exists in the library.");
							return;
						}

						// Process them all in parallel to be quick
						await Task.WhenAll(mediaToAdd // skip movies we already have just in case the API doesnt do it for us
							.Select<MediaItem, Task>(mi =>
							{
								return mi.MediaType switch
								{
									MediaType.Movie => Radarr.Client.Movie.AddMovie(mi.Title, mi.Year, 4, mi.TmdbId.ToString(), mi.Images.Select(i => Image.ToRadarrImage(i)).ToList(), (int)mi.TmdbId, "/media/movies", MinimumAvailability.Announced, true, new RadarrSharp.Endpoints.Movie.AddOptions() { SearchForMovie = true }),
									MediaType.Show => Sonarr.Client.Series.AddSeries((int)mi.TmdbId, mi.Title, 6, mi.TmdbId.ToString(), mi.Images.Select(i => Image.ToSonarrImage(i)).ToArray(), mi.Seasons.ToArray(), "/media/shows", monitored: true, addOptions: new SonarrSharp.Endpoints.Series.AddOptions { SearchForMissingEpisodes = true }),
									_ => throw new NotImplementedException()
								};
							}));

						await ReplyAsync("Added " + string.Join(", ", mediaToAdd.Select(mi => string.Format("{0} ({1})", mi.Title, mi.Year))));
					}
					catch (Exception e) when (e is ArgumentOutOfRangeException || e is FormatException || e is OverflowException)
					{
						await paginatedMessage.RemoveAllReactionsAsync();
					}
				}

				Context.Client.ReactionsCleared += reactionHandle;
			}
			catch (Exception e)
			{
				await ReplyAsync(e.Message);
			}
		}
	}
}
