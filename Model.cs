using Microsoft.EntityFrameworkCore;

namespace SimpleBowling
{
	public class LaneContext : DbContext
	{
		public DbSet<Player> Players { get; set; }
		public DbSet<Chance> Chances { get; set; }
		public DbSet<Game> Games { get; set; }

		public string DbPath { get; }

		public LaneContext()
		{
			var folder = Environment.SpecialFolder.LocalApplicationData;
			var path = Environment.GetFolderPath(folder);
			DbPath = Path.Join(path, "lane.db");
		}

		// The following configures EF to create a Sqlite database file in the
		// special "local" folder for your platform.
		protected override void OnConfiguring(DbContextOptionsBuilder options)
		{
			options.UseSqlite($"Data Source={DbPath}");
		}
	}

	public class Game
	{
		public int Id { get; set; }
		public bool HasStarted { get; set; } = false;
		public bool HasFinished { get; set; } = false;
	}

	public class Player
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int Score { get; }
		public int GameId { get; set; }
		public bool IsActive { get; set; } = false;

		public int? MostRecentFrameNumber => GetChances().LastOrDefault()?.FrameNumber;

		public List<Chance> Chances { get; set; } = new();

		public List<Chance> GetChances(int? frame = null)
		{
			if (frame is null)
			{
				return Chances.OrderBy(c => c.FrameNumber).ThenBy(c => c.Number).ToList();
			}
			else
			{
				return Chances.Where(c => c.FrameNumber == frame).OrderBy(c => c.Number).ToList();
			}
		}

		public int GetScore(int? frame = null)
		{
			var score = 0;

			if (frame is null)
			{
				for (var i = 0; i < MostRecentFrameNumber; i++)
				{
					score += GetScore(i + 1);
				}
			}
			else
			{
				var chances = GetChances(frame);

				foreach (var chance in chances)
				{
					score += chance.Score;

					if (chance.IsStrike)
					{
						return score + (chance.Next?.Score ?? 0) + (chance.Next?.Next?.Score ?? 0);
					}
					else if (chance.IsSpare)
					{
						return score + chance.Next?.Score ?? 0;
					}
				}
			}

			return score;
		}

		public int GetRunningScore(int frame = 1)
		{
			var score = 0;
			for (var i = 0; i < frame; i++)
			{
				score += GetScore(i + 1);
			}

			return score;
		}
	}

	public class Chance
	{
		public int Id { get; set; }
		public int Number { get; set; } = 1;
		public bool IsUsed { get; set; } = false;
		public int Score { get; set; } = 0;
		public bool IsActive { get; set; } = true;

		public int FrameNumber { get; set; }
		public int PlayerId { get; set; }

		public Player Player { get; set; }

		public bool IsFirst => Number == 1;
		public bool IsSecond => Number == 2;
		public bool IsFinalFrame => FrameNumber == 10;
		public int PinsLeft => 10 - Score;
		public bool CanGoAgain => Number < NumberOfChancesAllowed || !IsUsed;

		public string FormattedScore
		{
			get
			{
				if (IsSpare)
				{
					return "/";
				}
				else if (Score == 10)
				{
					return "X";
				}

				return Score.ToString();
			}
		}

		public int NumberOfChancesAllowed
		{
			get
			{
				if (Player is null) return 0;

				var chances = Player.GetChances(FrameNumber);
				if (IsFinalFrame && chances.Any(c => c.IsStrike || c.IsSpare))
				{
					return 3;
				}
				else if (IsStrike)
				{
					return 1;
				}

				return 2;
			}
		}

		public bool IsStrike => IsFirst && Score == 10;

		public bool IsSpare
		{
			get
			{
				if (IsSecond)
				{
					var first = Player.GetChances(FrameNumber).First();

					return (first.Score + Score) == 10;
				}

				return false;
			}
		}

		public bool CanStrike => IsFirst || (IsFinalFrame && Previous?.PinsLeft == 0);
		public bool CanSpare => IsSecond && !(Previous?.IsStrike ?? true);

		public Chance? Previous
		{
			get
			{
				if (Player is null) return null;

				var chances = Player.GetChances();
				var index = chances.IndexOf(this);
				return chances.ElementAtOrDefault(index - 1);
			}
		}

		public Chance? Next
		{
			get
			{
				if (Player is null) return null;

				var chances = Player.GetChances();
				var index = chances.IndexOf(this);
				return chances.ElementAtOrDefault(index + 1);
			}
		}
	}
}