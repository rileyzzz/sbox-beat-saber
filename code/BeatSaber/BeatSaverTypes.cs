using Sandbox;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BeatSaber
{
	public class MapDetail
	{
		[JsonPropertyName( "automapper" )]
		bool Automapper { get; set; }

		[JsonPropertyName( "createdAt" )]
		string CreatedAt { get; set; }

		[JsonPropertyName( "description" )]
		public string Description { get; set; }

		[JsonPropertyName( "id" )]
		public string ID { get; set; }

		[JsonPropertyName( "lastPublishedAt" )]
		string LastPublishedAt { get; set; }

		[JsonPropertyName( "metadata" )]
		public MapDetailMetadata Metadata { get; set; }

		[JsonPropertyName( "name" )]
		public string Name { get; set; }

		[JsonPropertyName( "qualified" )]
		public bool Qualified { get; set; }

		[JsonPropertyName( "ranked" )]
		public bool Ranked { get; set; }

		[JsonPropertyName( "stats" )]
		public MapStats Stats { get; set; }

		[JsonPropertyName( "updatedAt" )]
		string UpdatedAt { get; set; }

		[JsonPropertyName( "uploaded" )]
		string Uploaded { get; set; }

		[JsonPropertyName( "uploader" )]
		public UserDetail Uploader { get; set; }

		[JsonPropertyName( "versions" )]
		public MapVersion[] Versions { get; set; }

		[JsonIgnore]
		public string DownloadDirectory => "download/" + ID;

		[JsonIgnore]
		public bool Installed => FileSystem.Data.DirectoryExists( DownloadDirectory );
	}

	public struct MapDetailMetadata
	{
		[JsonPropertyName( "bpm" )]
		public float BPM { get; set; }

		[JsonPropertyName( "duration" )]
		public int Duration { get; set; }

		[JsonPropertyName( "levelAuthorName" )]
		public string LevelAuthor { get; set; }

		[JsonPropertyName( "songAuthorName" )]
		public string SongAuthor { get; set; }

		[JsonPropertyName( "songName" )]
		public string SongName { get; set; }

		[JsonPropertyName( "songSubName" )]
		public string SongSubName { get; set; }
	}

	public struct MapStats
	{
		[JsonPropertyName( "downloads" )]
		public int Downloads { get; set; }

		[JsonPropertyName( "downvotes" )]
		public int Downvotes { get; set; }

		[JsonPropertyName( "plays" )]
		public int Plays { get; set; }

		[JsonPropertyName( "score" )]
		public float Score { get; set; }

		[JsonPropertyName( "scoreOneDP" )]
		public float ScoreOneDP { get; set; }

		[JsonPropertyName( "upvotes" )]
		public int Upvotes { get; set; }
	}

	public struct MapVersion
	{
		[JsonPropertyName( "coverURL" )]
		public string CoverURL { get; set; }

		[JsonPropertyName( "createdAt" )]
		public string CreatedAt { get; set; }

		[JsonPropertyName( "diffs" )]
		public MapDifficulty[] Difficulties { get; set; }

		[JsonPropertyName( "downloadURL" )]
		public string DownloadURL { get; set; }

		[JsonPropertyName( "feedback" )]
		public string Feedback { get; set; }

		[JsonPropertyName( "hash" )]
		public string Hash { get; set; }

		[JsonPropertyName( "key" )]
		public string Key { get; set; }

		[JsonPropertyName( "previewURL" )]
		public string PreviewURL { get; set; }

		[JsonPropertyName( "sageScore" )]
		public short SageScore { get; set; }

		[JsonPropertyName( "scheduledAt" )]
		string ScheduledAt { get; set; }

		[JsonPropertyName( "state" )]
		string strState { get; set; }

		[JsonIgnore]
		public MapState State => Enum.Parse<MapState>(strState);

		[JsonPropertyName( "testplayAt" )]
		string TestplayAt { get; set; }

		[JsonPropertyName( "testplays" )]
		public MapTestplay[] Testplays { get; set; }
	}

	public struct MapDifficulty
	{
		[JsonPropertyName( "bombs" )]
		public int Bombs { get; set; }

		[JsonPropertyName( "characteristic" )]
		public string Characteristic { get; set; }

		[JsonPropertyName( "chroma" )]
		public bool Chroma { get; set; }

		[JsonPropertyName( "cinema" )]
		public bool Cinema { get; set; }

		[JsonPropertyName( "difficulty" )]
		string strDifficulty { get; set; }

		[JsonIgnore]
		public LevelDifficulty Difficulty => Enum.Parse<LevelDifficulty>( strDifficulty );

		[JsonPropertyName( "events" )]
		public int Events { get; set; }

		[JsonPropertyName( "length" )]
		public double Length { get; set; }

		[JsonPropertyName( "me" )]
		bool Me { get; set; }

		[JsonPropertyName( "ne" )]
		bool Ne { get; set; }

		[JsonPropertyName( "njs" )]
		float NJS { get; set; }

		[JsonPropertyName( "notes" )]
		public int Notes { get; set; }

		[JsonPropertyName( "nps" )]
		double NPS { get; set; }

		[JsonPropertyName( "obstacles" )]
		public int Obstacles { get; set; }

		[JsonPropertyName( "offset" )]
		public float Offset { get; set; }

		[JsonPropertyName( "paritySummary" )]
		public MapParitySummary ParitySummary { get; set; }

		[JsonPropertyName( "seconds" )]
		public double Seconds { get; set; }

		[JsonPropertyName( "stars" )]
		public float Stars { get; set; }
	}

	public struct MapParitySummary
	{
		[JsonPropertyName( "errors" )]
		public int Errors { get; set; }

		[JsonPropertyName( "resets" )]
		public int Resets { get; set; }

		[JsonPropertyName( "warns" )]
		public int Warns { get; set; }
	}

	public enum MapState
	{
		Uploaded,
		Testplay,
		Published,
		Feedback,
		Scheduled
	}

	public struct MapTestplay
	{
		[JsonPropertyName( "createdAt" )]
		string CreatedAt { get; set; }

		[JsonPropertyName( "feedback" )]
		public string Feedback { get; set; }

		[JsonPropertyName( "feedbackAt" )]
		string FeedbackAt { get; set; }

		[JsonPropertyName( "user" )]
		public UserDetail User { get; set; }

		[JsonPropertyName( "video" )]
		public string Video { get; set; }
	}

	public enum UserType
	{
		DISCORD,
		SIMPLE,
		DUAL
	}

	public class UserDetail
	{
		[JsonPropertyName( "avatar" )]
		public string Avatar { get; set; }

		[JsonPropertyName( "email" )]
		public string Email { get; set; }

		[JsonPropertyName( "hash" )]
		public string Hash { get; set; }

		[JsonPropertyName( "id" )]
		public int ID { get; set; }

		[JsonPropertyName( "name" )]
		public string Name { get; set; }

		[JsonPropertyName( "stats" )]
		public UserStats Stats { get; set; }

		[JsonPropertyName( "testplay" )]
		public bool TestPlay { get; set; }

		[JsonPropertyName( "type" )]
		string strType { get; set; }

		[JsonIgnore]
		public UserType Type => Enum.Parse<UserType>(strType);

		[JsonPropertyName( "uniqueSet" )]
		public bool UniqueSet { get; set; }

		[JsonPropertyName( "uploadLimit" )]
		public int UploadLimit { get; set; }
	}

	public struct UserStats
	{
		[JsonPropertyName( "avgBpm" )]
		public float AvgBPM { get; set; }

		[JsonPropertyName( "avgDuration" )]
		public float AvgDuration { get; set; }

		[JsonPropertyName( "avgScore" )]
		public float AvgScore { get; set; }

		[JsonPropertyName( "diffStats" )]
		public UserDiffStats DiffStats { get; set; }

		[JsonPropertyName( "firstUpload" )]
		public string FirstUpload { get; set; }

		[JsonPropertyName( "lastUpload" )]
		public string LastUpload { get; set; }

		[JsonPropertyName( "rankedMaps" )]
		public int RankedMaps { get; set; }

		[JsonPropertyName( "totalDownvotes" )]
		public int TotalDownvotes { get; set; }

		[JsonPropertyName( "totalMaps" )]
		public int TotalMaps { get; set; }

		[JsonPropertyName( "totalUpvotes" )]
		public int TotalUpvotes { get; set; }
	}

	public struct UserDiffStats
	{
		[JsonPropertyName( "easy" )]
		public int Easy { get; set; }

		[JsonPropertyName( "normal" )]
		public int Normal { get; set; }

		[JsonPropertyName( "hard" )]
		public int Hard { get; set; }

		[JsonPropertyName( "expert" )]
		public int Expert { get; set; }

		[JsonPropertyName( "expertPlus" )]
		public int ExpertPlus { get; set; }

		[JsonPropertyName( "total" )]
		public int Total { get; set; }
	}
}
