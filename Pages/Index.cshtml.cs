using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Channels;

namespace SimpleBowling.Pages
{
	[BindProperties]
	public class IndexModel : PageModel
	{
		private readonly LaneContext _context;
		private readonly ILogger<IndexModel> _logger;

		public bool HasStarted => Game?.HasStarted ?? false;
		public bool HasFinished => Game?.HasFinished ?? false;
		public bool IsFinalFrame => Chance?.IsFinalFrame ?? false;
		public int Frame => Chance?.FrameNumber ?? 1;
		public Player? Player => Players.SingleOrDefault(p => p.IsActive);

		public Chance? Chance
		{
			get
			{
				var active = _context.Chances.Include(c => c.Player).SingleOrDefault(c => c.IsActive);
				return active ?? _context.Chances.OrderBy(c => c.Id).LastOrDefault();
			}
		}

		public List<Player> Players => _context.Players.Include(p => p.Chances).ToList();

		private Game? Game => _context.Games.SingleOrDefault();

		public IndexModel(LaneContext context, ILogger<IndexModel> logger)
		{
			_context = context;
			_logger = logger;
		}

		public async Task OnGetAsync([FromQuery] bool @new = false)
		{
			if (@new || Game is null)
			{
				_context.RemoveRange(_context.Chances);
				_context.RemoveRange(_context.Players);
				_context.RemoveRange(_context.Games);

				_context.Add(new Game());

				await _context.SaveChangesAsync();
				RedirectToPage();
			}
		}

		public async Task OnPostStartAsync()
		{
			ValidateHasNotStarted();

			Game.HasStarted = true;

			var startingPlayer = Players.OrderBy(p => p.Id).First();
			startingPlayer.IsActive = true;
			startingPlayer.Chances.Add(new Chance { FrameNumber = 1 });

			await _context.SaveChangesAsync();
			RedirectToPage();
		}

		public async Task OnPostPlayerAsync([FromForm] string name)
		{
			ValidateHasNotStarted();

			await _context.AddAsync(new Player { Name = name, GameId = Game.Id });
			await _context.SaveChangesAsync();
			RedirectToPage();
		}

		public async Task OnPostScoreAsync([FromForm] int score)
		{
			ValidateHasStarted();
			ValidateHasNotFinished();
			ValidatePinsLeft(score);

			var chance = Score(score);
			NextChance(chance);
			await _context.SaveChangesAsync();
			RedirectToPage();
		}

		public async Task OnPostStrikeAsync()
		{
			ValidateHasStarted();
			ValidateHasNotFinished();
			ValidateCanStrike();

			var chance = Score(10);
			NextChance(chance);
			await _context.SaveChangesAsync();
			RedirectToPage();
		}

		public async Task OnPostSpareAsync()
		{
			ValidateHasStarted();
			ValidateHasNotFinished();
			ValidateCanSpare();

			var previous = Chance?.Previous;
			var chance = Score(10 - previous.Score);

			NextChance(chance);
			await _context.SaveChangesAsync();
			RedirectToPage();
		}

		private Chance Score(int score)
		{
			var chance = Chance;
			chance.Score = score;
			chance.IsUsed = true;
			chance.IsActive = false;

			_context.Update(chance);

			return chance;
		}

		private void NextChance(Chance currentChance)
		{
			var currentFrame = currentChance.FrameNumber;

			if (currentChance.CanGoAgain)
			{
				Player.Chances.Add(new Chance { FrameNumber = currentFrame, Number = currentChance.Number + 1 });
			}
			else
			{
				var frame = NextPlayer(currentFrame);
				if (!Game.HasFinished)
				{
					Player.Chances.Add(new Chance { FrameNumber = frame });
				}
			}

			if (Player is not null)
			{
				_context.Update(Player);
			}
		}

		private int NextPlayer(int currentFrame)
		{
			var player = Player;
			var nextIndex = 1 + Players.IndexOf(player);

			player.IsActive = false;
			_context.Update(player);

			if (nextIndex < Players.Count)
			{
				Players[nextIndex].IsActive = true;
				return currentFrame;
			}
			else if (currentFrame < 10)
			{
				Players[0].IsActive = true;
				return currentFrame + 1;
			}
			else
			{
				Game.HasFinished = true;
				return currentFrame;
			}
		}

		private void ValidateHasNotStarted()
		{
			if (Game.HasStarted)
			{
				throw new BadHttpRequestException("The game has already started.");
			}
		}

		private void ValidateHasStarted()
		{
			if (!Game.HasStarted)
			{
				throw new BadHttpRequestException("The game has not started.");
			}
		}

		private void ValidateHasNotFinished()
		{
			if (Game.HasFinished)
			{
				throw new BadHttpRequestException("The game has finished.");
			}
		}

		private void ValidatePinsLeft(int score)
		{
			if (score > Chance.PinsLeft)
			{
				throw new BadHttpRequestException($"There are only {Chance.PinsLeft} pins left.");
			}
		}

		private void ValidateCanStrike()
		{
			if (!Chance?.CanStrike ?? false)
			{
				throw new BadHttpRequestException($"A strike is not possible for this ball.");
			}
		}

		private void ValidateCanSpare()
		{
			if (!Chance?.CanSpare ?? false)
			{
				throw new BadHttpRequestException($"A spare is not possible for this ball.");
			}
		}
	}
}