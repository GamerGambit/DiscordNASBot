using SonarrSharp;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Bot
{
	public static class Sonarr
	{
		private static readonly IEnumerable<long> TmdbIds;

		public static SonarrClient Client { get; }

		static Sonarr()
		{
			Client = new SonarrClient(
				Environment.GetEnvironmentVariable("SONARR_HOST"),
				int.Parse(Environment.GetEnvironmentVariable("SONARR_PORT")),
				Environment.GetEnvironmentVariable("SONARR_APIKEY")
			);

			TmdbIds = Client.Series.GetSeries().GetAwaiter().GetResult().Select(s => (long)s.TvdbId);
		}
		public static bool HasShow(long TmdbId)
		{
			return TmdbIds.Contains(TmdbId);
		}
	}
}
