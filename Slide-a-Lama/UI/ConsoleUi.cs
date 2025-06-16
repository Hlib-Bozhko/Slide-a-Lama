
using System;
using System.Threading;
using Slide_a_Lama.Core;
using Datas.Entity;
using Datas.Entity;
using Datas.Service;
using Datas.Service.Scores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Slide_a_Lama.UI
{
    internal class ConsoleUi
    {
        private static FieldAdapter _field;
        private static Menu _menu;
        public readonly IScoreService ScoreService;


        public ConsoleUi()
        {
            var options = new DbContextOptionsBuilder<SlideALamaDbContext>()
                .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=SlideALamaDb;Trusted_Connection=true")
                .Options;
            
            var context = new SlideALamaDbContext(options);
            // using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            //  _logger = loggerFactory.CreateLogger<Menu>();
            // var commentLogger = loggerFactory.CreateLogger<CommentServiceEF>();
            // var ratingLogger = loggerFactory.CreateLogger<RatingServiceEF>();

            //_commentService = new CommentServiceEF(context, commentLogger);
            // _ratingService = new RatingServiceEF(context, ratingLogger);
            _menu = new Menu(this);
            ScoreService = new ScoreServiceEF(context, null);
        }

        public void Init()
        {

            _menu.UpdateMenu("~Slide_a_Lama~", new[] { "Play", "Scores", "rating", "comments", "Exit" });
        }

        private void WriteField()
        {

            SetColors(ConsoleColor.Black, ConsoleColor.Red);
            Console.Write("////--" + _field.CurrentPlayer.Team + " Player--////\n");

            SetColors(ConsoleColor.White, ConsoleColor.Black);
            for (int row = 0; row < _field.RowCount; row++)
            {
                for (int column = 0; column < _field.ColumnCount; column++)
                {
                    Console.BackgroundColor = FindCubeColor(_field.GetCube(row, column).Value);
                    Console.Write(_field.GetCube(row, column).Value + " ");
                    Console.BackgroundColor = ConsoleColor.White;
                }

                Console.Write("\n");
            }
            Console.ForegroundColor = ConsoleColor.Black;//scores
            Console.WriteLine("");
            for (int player = 0; player < _field.PlayersCount; player++)
            {
                Console.WriteLine("Player" + _field.Players[player].Team + ": " + _field.Players[player].Score);
            }


        }

        private ConsoleColor FindCubeColor(int value)
        {
            switch (value)
            {
                case 1:
                    return ConsoleColor.DarkGray;
                case 2:
                    return ConsoleColor.DarkYellow;
                case 3:
                    return ConsoleColor.Yellow;
                case 4:
                    return ConsoleColor.DarkCyan;
                case 5:
                    return ConsoleColor.Cyan;
                case 6:
                    return ConsoleColor.DarkGreen;
                case 7:
                    return ConsoleColor.Green;
            }

            return ConsoleColor.White;
        }

        private void Control(Thread blinker)
        {
            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.RightArrow:
                    _field.MoveRight(_field.CurrentCube[0]);
                    break;

                case ConsoleKey.LeftArrow:
                    _field.MoveLeft(_field.CurrentCube[0]);
                    break;

                case ConsoleKey.UpArrow:
                    _field.MoveUp(_field.CurrentCube[1]);
                    break;

                case ConsoleKey.DownArrow:
                    _field.MoveDown(_field.CurrentCube[1]);
                    break;

                case ConsoleKey.Enter:
                    _field.PutCube();
                    break;

                case ConsoleKey.R:
                    Console.ResetColor();
                    blinker.Interrupt();
                    this.Init();
                    break;
            }
        }

        private void WriteFin()
        {
            Console.Clear();

            ScoreService.AddScore(new Score { Player = Environment.UserName, Points = _field.GetScore() });

            string wText = "Player " + _field.CurrentPlayer.Team + " win!";
            string wText2 = "Score is " + _field.GetScore();
            _menu.CreateFrame("~Congratulate~");
            _menu.FindCenter(wText, 0);
            SetColors(ConsoleColor.White, ConsoleColor.Black);
            Console.Write(wText);
            _menu.FindCenter(wText2, 1);
            Console.Write(wText2);

            wText = "(Press any key to continue)";
            _menu.FindCenter(wText, 2);
            Console.ResetColor();
            Console.Write(wText);
            Console.ReadKey();

            this.Init();
        }

        public void Play()
        {
            _field = FieldAdapterFactory.CreateField(8, 8, _menu.PlayersNumber, _menu.WinScore);
            
            Console.Clear();
            while (_field.GameState == GameState.PLAYING)
            {
                if (_field.CurrentCube[0] != 0 && (_field.CurrentCube[1] != _field.ColumnCount - 1 && _field.CurrentCube[1] != 0))
                {
                    SetColors(ConsoleColor.White, ConsoleColor.Black);
                    _field.AddCube();
                }
                _field.UpdateField();

                _field.UpdateField();

                WriteField();

                var blinker = new Thread(Blink);
                blinker.Start();

                while (_field.UpdateField())
                {
                    _field.UpdateField();
                }
                
                Control(blinker);

                blinker.Interrupt();

                _field.UpdateField();
                Console.ResetColor();
                Console.Clear();
                _field.UpdateField();
            }
            WriteFin();
        }

        public void SetColors(ConsoleColor background, ConsoleColor foreground)
        {
            Console.BackgroundColor = background;
            Console.ForegroundColor = foreground;
        }

        private void Blink()
        {
            try
            {
                while (true)
                {
                    SetColors(ConsoleColor.White, ConsoleColor.Black);
                    Console.SetCursorPosition(_field.CurrentCube[1] * 2, _field.CurrentCube[0] + 1);
                    Console.Write(_field.GetCube(_field.CurrentCube[0], _field.CurrentCube[1]).Value + " ");
                    SetColors(FindCubeColor(_field.GetCube(_field.CurrentCube[0], _field.CurrentCube[1]).Value), ConsoleColor.Black);
                    Thread.Sleep(200);
                    Console.SetCursorPosition(_field.CurrentCube[1] * 2, _field.CurrentCube[0] + 1);
                    Console.Write(_field.GetCube(_field.CurrentCube[0], _field.CurrentCube[1]).Value + " ");
                    Thread.Sleep(200);


                }
            }
            catch (ThreadInterruptedException)
            {
                Thread.Sleep(Timeout.Infinite);
            }
            finally
            {
                Thread.EndThreadAffinity();
            }

        }
    }
}


