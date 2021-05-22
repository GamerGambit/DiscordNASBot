using RadarrSharp;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Bot
{
	public static class Radarr
	{
		private static readonly IEnumerable<long> TmdbIds;

		public static RadarrClient Client { get; }

		static Radarr()
		{
			Client = new RadarrClient(
				Environment.GetEnvironmentVariable("RADARR_HOST"),
				int.Parse(Environment.GetEnvironmentVariable("RADARR_PORT")),
				Environment.GetEnvironmentVariable("RADARR_APIKEY")
			);

			TmdbIds = Client.Movie.GetMovies().GetAwaiter().GetResult().Select(m => m.TmdbId);
		}

		public static bool HasMovie(long TmdbId) => TmdbIds.Contains(TmdbId);
	}
}
