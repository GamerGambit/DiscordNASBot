using System;
using System.Collections.Generic;

namespace Bot
{
	public enum CoverType
	{
		Banner,
		FanArt,
		Poster
	}

	public struct Image
	{
		public CoverType CoverType;
		public string URL;

		public static RadarrSharp.Models.Image ToRadarrImage(Image img)
		{
			return new RadarrSharp.Models.Image()
			{
				Url = img.URL,
				CoverType = img.CoverType switch
				{
					CoverType.Banner => RadarrSharp.Enums.CoverType.Banner,
					CoverType.FanArt => RadarrSharp.Enums.CoverType.FanArt,
					CoverType.Poster => RadarrSharp.Enums.CoverType.Poster,
					_ => throw new NotImplementedException()
				}
			};
		}

		public static SonarrSharp.Models.Image ToSonarrImage(Image img)
		{
			return new SonarrSharp.Models.Image()
			{
				Url = img.URL,
				CoverType = img.CoverType switch
				{
					CoverType.Banner => SonarrSharp.Enum.CoverType.Banner,
					CoverType.FanArt => SonarrSharp.Enum.CoverType.FanArt,
					CoverType.Poster => SonarrSharp.Enum.CoverType.Poster,
					_ => throw new NotImplementedException()
				}
			};
		}

		public static Image FromRadarrImage(RadarrSharp.Models.Image img)
		{
			return new Image()
			{
				URL = img.Url,
				CoverType = img.CoverType switch
				{
					RadarrSharp.Enums.CoverType.Banner => CoverType.Banner,
					RadarrSharp.Enums.CoverType.Poster => CoverType.Poster,
					RadarrSharp.Enums.CoverType.FanArt => CoverType.FanArt,
					_ => throw new NotImplementedException()
				}
			};
		}

		public static Image FromSonarrImage(SonarrSharp.Models.Image img)
		{
			return new Image()
			{
				URL = img.Url,
				CoverType = img.CoverType switch
				{
					SonarrSharp.Enum.CoverType.Banner => CoverType.Banner,
					SonarrSharp.Enum.CoverType.FanArt => CoverType.FanArt,
					SonarrSharp.Enum.CoverType.Poster => CoverType.Poster,
					_ => throw new NotImplementedException()
				}
			};
		}
	}

	public enum MediaType
	{
		Movie,
		Show
	}

	public struct MediaItem
	{
		public MediaType MediaType;
		public string Title;
		public int Year;
		public long TmdbId;
		public IEnumerable<Image> Images;
		public string Overview;
		public IEnumerable<SonarrSharp.Models.Season> Seasons;

		public bool Exists
		{
			get
			{
				return MediaType switch
				{
					MediaType.Movie => Radarr.HasMovie(TmdbId),
					MediaType.Show => Sonarr.HasShow(TmdbId),
					_ => throw new NotImplementedException(),
				};
			}
		}
	}
}
