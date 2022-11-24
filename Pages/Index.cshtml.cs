using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace SimpleBowling.Pages
{
	[BindProperties]
	public class IndexModel : PageModel
	{
		private readonly LaneContext _context;
		private readonly ILogger<IndexModel> _logger;

		public bool HasStarted => Game?.HasStarted ?? false;
		public int Frame => Chance?.FrameNumber ?? 0;
		public Player? Player => Players.SingleOrDefault(p => p.IsActive);
		public Chance? Chance => _context.Chances.Include(c => c.Player).Single(c => c.IsActive);
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
			Game.HasStarted = true;

			var startingPlayer = Players.OrderBy(p => p.Id).First();
			startingPlayer.IsActive = true;
			startingPlayer.Chances.Add(new Chance { FrameNumber = 1 });

			await _context.SaveChangesAsync();

			RedirectToPage();
		}

		public async Task OnPostPlayerAsync([FromForm] string name)
		{
			await _context.AddAsync(new Player { Name = name, GameId = Game.Id });
			await _context.SaveChangesAsync();

			RedirectToPage();
		}

		public async Task OnPostScoreAsync([FromForm] int score)
		{
			var chance = Chance;
			chance.Score = score;
			chance.IsUsed = true;

			var currentFrame = chance.FrameNumber;
			chance.IsActive = false;

			_context.Update(chance);

			if (currentFrame < 10)
			{
				if (chance.Number < chance.NumberOfChancesAllowed)
				{
					Player.Chances.Add(new Chance { FrameNumber = currentFrame, Number = chance.Number + 1 });
				}
				else
				{
					var frame = NextPlayer(currentFrame);
					Player.Chances.Add(new Chance { FrameNumber = frame });
				}

				_context.Update(Player);
			}

			await _context.SaveChangesAsync();
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
			else
			{
				Players[0].IsActive = true;
				return currentFrame + 1;
			}
		}
	}
}