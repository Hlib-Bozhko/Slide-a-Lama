
using System;
using Datas.Entity;
using Datas.Service.Comments;
using Datas.Service.Rates;
using Datas.Entity;
using Datas.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Slide_a_Lama.UI
{
    internal class Menu
    {
        public int CurrentMItem { get; private set; }
        public int WinScore { get; private set; }
        public int PlayersNumber { get; private set; }

        private readonly ICommentService _commentService;
        private readonly IRatingService _ratingService;


        private string[] _menuItems;
        private readonly ConsoleUi _consoleUi;

        public Menu(ConsoleUi consoleUi)
        {
            _consoleUi = consoleUi;
            _menuItems = new string[3];
            CurrentMItem = 0;
            WinScore = 0;

            
            var options = new DbContextOptionsBuilder<SlideALamaDbContext>()
                .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=SlideALamaDb;Trusted_Connection=true")
                .Options;
            
            var context = new SlideALamaDbContext(options);
            
            //using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            // _logger = loggerFactory.CreateLogger<Menu>();
            // var commentLogger = loggerFactory.CreateLogger<CommentServiceEF>();
            // var ratingLogger = loggerFactory.CreateLogger<RatingServiceEF>();

            _commentService = new CommentServiceEF(context, null);
            _ratingService = new RatingServiceEF(context, null);
        }


        public void FindCenter(String str, int offset)
        {
            int centerX = (Console.WindowWidth / 2) - (str.Length / 2);
            int centerY = (Console.WindowHeight / 2) - 1 + offset;
            Console.SetCursorPosition(centerX, centerY);
        }

        public void WriteInCenter(string str, int offset)
        {
            FindCenter(str, offset);
            Console.Write(str);
        }

        private void WriteFrameSection(ref int left, int top, string sign, int start, int end)
        {
            for (; start < end; start++)
            {
                Console.SetCursorPosition(left, top);
                Console.Write(sign);
                left -= 1;
            }
        }

        public void CreateFrame(String section)
        {

            int b = (Console.WindowWidth / 2) + 24;
            int c = (Console.WindowWidth / 2) + 24;

            Console.ForegroundColor = ConsoleColor.Green;
            WriteFrameSection(ref b, (Console.WindowHeight / 2) + 4, "╝", 0, 1);
            WriteFrameSection(ref b, (Console.WindowHeight / 2) + 4, "═", 0, 47);

            b += 1;
            WriteFrameSection(ref b, (Console.WindowHeight / 2) + 4, "╚", 0, 1);

            for (int a = (Console.WindowHeight / 2) - 1 - 6; a < (Console.WindowHeight / 2) - 1 + 4; a++) // left right
            {
                Console.SetCursorPosition((Console.WindowWidth / 2) - 23, a + 1);
                Console.WriteLine("║");
                Console.SetCursorPosition((Console.WindowWidth / 2) + 24, a + 1);
                Console.WriteLine("║");
            }

            WriteFrameSection(ref c, (Console.WindowHeight / 2) - 6, "╗", 0, 1);
            WriteFrameSection(ref c, (Console.WindowHeight / 2) - 6, "═", 0, 47);

            c += 1;
            WriteFrameSection(ref c, (Console.WindowHeight / 2) - 6, "╔", 0, 1);

            Console.SetCursorPosition((Console.WindowWidth / 2) - (section.Length / 2), (Console.WindowHeight / 2) - 6);
            Console.Write(section);

            _consoleUi.SetColors(ConsoleColor.Black, ConsoleColor.White);
        }

        public void UpdateMenu(String menuSection, String[] menuItems)
        {
            Console.Clear();
            _menuItems = menuItems;
            CreateFrame(menuSection);

            for (int item = 0; item < menuItems.Length; item++)
            {
                if (item == CurrentMItem)
                {
                    _consoleUi.SetColors(ConsoleColor.White, ConsoleColor.Black);

                    WriteInCenter(menuItems[item], item - menuItems.Length / 2);
                    
                    _consoleUi.SetColors(ConsoleColor.Black, ConsoleColor.White);
                }

                else
                {
                    WriteInCenter(menuItems[item], item - menuItems.Length / 2);
                }
            }

            SelectItem(menuSection);
        }

        private void SelectItem(String menuSection)
        {
            bool movable = true;
            while (movable)
            {
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.UpArrow:
                        CurrentMItem--;
                        if (CurrentMItem < 0)
                        {
                            CurrentMItem = _menuItems.Length - 1;
                        }
                        UpdateMenu(menuSection, _menuItems);
                        break;
                    case ConsoleKey.DownArrow:
                        CurrentMItem++;
                        if (CurrentMItem >= _menuItems.Length)
                        {
                            CurrentMItem = 0;
                        }
                        UpdateMenu(menuSection, _menuItems);
                        break;
                    case ConsoleKey.Enter:
                        Console.Clear();
                        switch (CurrentMItem)
                        {
                            case 0:
                                movable = false;
                                SetOptions();
                                _consoleUi.Play();
                                break;
                            case 1:
                                WriteScores();
                                break;
                            case 2:
                                WriteRates();
                                break;
                            case 3:
                                WriteComments();
                                break;
                            case 4:
                                movable = false;
                                Environment.Exit(0);
                                break;
                        }
                        break;
                }
            }
        }

        private void DisplayOptionsInput(string text, bool choise, int offset)
        {
            WriteInCenter(text,offset);
            FindCenter("xyz", offset+1);
            string check = Console.ReadLine();
            if (Int32.TryParse(check, out int res))
            {
                if (choise)
                {
                    WinScore = res;
                }
                else
                {
                    PlayersNumber = res;
                }
            }
        }
        private void SetOptions()
        {
            CreateFrame("Options");
            while (WinScore <= 0)
            {
                DisplayOptionsInput("Set the number of points needed to win", true, -2);
            }

            while (PlayersNumber <= 0)
            {
                DisplayOptionsInput("Set the number of players", false, 1);
            }
        }

        private void WriteComments()
        {
            CreateFrame("Comments");
            WriteInCenter("New comment(N) / show comments(s)", 0);

            while (true)
            {
                string check = Console.ReadLine();
                switch (check)
                {
                    case "s":
                        Console.Clear();
                        CreateFrame("Comments");
                        int i = -3;
                        foreach (var comment in _commentService.GetComments())
                        {
                            WriteInCenter(comment.Name, i);
                            i++;
                            Console.SetCursorPosition((Console.WindowWidth / 2) - 22, (Console.WindowHeight / 2) - 1 + i);
                            Console.WriteLine(comment.Text);
                            i++;
                        }

                        break;
                    case "n":
                    case "N":
                        Console.Clear();
                        CreateFrame("Comments");

                        WriteInCenter("Enter your name:", -4);
                        string name = Console.ReadLine();
                        WriteInCenter("Enter your comment:\n", -2);
                        Console.SetCursorPosition((Console.WindowWidth / 2) - 22, (Console.WindowHeight / 2) + 3);
                        string comm = Console.ReadLine();
                        _commentService.AddComment(new Comment {Name = name, Text = comm});
                        break;
                    default:
                        continue;

                }
                break;
            }

            WriteInCenter("Press ENTER to return to the menu", 3);
            WriteInCenter("or R to reset the comments", 4);
            if (Console.ReadKey().Key == ConsoleKey.Enter)
            {
                UpdateMenu("~Slide_a_Lama~", new[] { "Play", "Scores", "rating", "comments", "Exit" });
            }
            else if (Console.ReadKey().Key == ConsoleKey.R)
            {
                _commentService.ResetComments();
            }

        }

        private void WriteRates()
        {
            CreateFrame("Rating");
            WriteInCenter("New rate(N) / show rates(s)", 0);

            while (true)
            {
                string check = Console.ReadLine();
                switch (check)
                {
                    case "s":
                        Console.Clear();
                        CreateFrame("Rating");
                        int i = -3;
                        foreach (var rate in _ratingService.GetRates())
                        {
                            WriteInCenter(rate.Name + ": " + rate.mark,i);
                            i++;
                        }

                        break;
                    case "n":
                    case "N":
                        Console.Clear();
                        CreateFrame("Rating");
                        WriteInCenter("Enter your name:\n", -4);
                        FindCenter("qwerty", -3);
                        string name = Console.ReadLine();

                        WriteInCenter("Enter your mark (0-100):\n", -2);
                        FindCenter("qwerty", -1);
                        string mark = Console.ReadLine();
                       if (Int32.TryParse(mark, out int res) && res is > 0 and < 100 )
                       {
                            _ratingService.AddRate(new Rating { Name = name, mark = res });
                        }
                        break;
                    default:
                        continue;

                }
                break;
            }

            WriteInCenter("Press ENTER to return to the menu", 3);
            WriteInCenter("or R to reset Rating", 4);

            if (Console.ReadKey().Key == ConsoleKey.Enter)
            {
                UpdateMenu("~Slide_a_Lama~", new[] { "Play", "Scores", "rating", "comments", "Exit" });
            }
            else if (Console.ReadKey().Key == ConsoleKey.R)
            {
                _ratingService.ResetRates();
            }

        }

        private void WriteScores()
        {
            CreateFrame("Scores");
            int i = -3;
            foreach (var score in _consoleUi.ScoreService.GetTopScores())
            {
                FindCenter(score.Player, i);
                Console.WriteLine("{0}. {1} {2}", i + 4, score.Player, score.Points);
                i++;
            }

            WriteInCenter("Press ENTER to return to the menu", i + 1);
            WriteInCenter("or R to reset the scores", i + 2);
            if (Console.ReadKey().Key == ConsoleKey.Enter)
            {
                UpdateMenu("~Slide_a_Lama~", new[] { "Play", "Scores", "rating", "comments", "Exit" });
            }
            else if (Console.ReadKey().Key == ConsoleKey.R)
            {
                _consoleUi.ScoreService.ResetScore();
            }
        }
    }
}

