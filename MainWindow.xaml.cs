using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace space_invaders
{
    enum Direction : byte { Left, Right, None };

    public partial class MainWindow : Window
    {
        DispatcherTimer timer = new DispatcherTimer();

        Direction currentDirection = Direction.None;

        List<Rectangle> enemies = new List<Rectangle>();
        List<Rectangle> shots = new List<Rectangle>();

        int playerSpeedLeft = -2;
        int playerSpeedRight = 2;
        int shotSpeedUp = -3;
        int shotSpeedDown = 3;

        ulong score = 0;

        public MainWindow()
        {
            InitializeComponent(); 

            MainCanvas.Focus();
            timer.Tick += TimerEvent;
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Start();
        }

        private void TimerEvent(object sender, EventArgs e)
        {
            SpawnEnemy();

            EnemyShoot();

            UpdatePlayerPos();

            UpdateShotPos();

            CheckCollisions();

            ScoreLabel.Content = $"SCORE: {score}";
        }

        private void UpdateShotPos()
        {
            List<Rectangle> shotsToRemove = new List<Rectangle>();

            foreach (Rectangle shot in shots)
            {
                if (Canvas.GetTop(shot) > 0 && (string)shot.Tag == "PlayerShot")
                    Canvas.SetTop(shot, Canvas.GetTop(shot) + shotSpeedUp);
                else if (Canvas.GetTop(shot) + shot.Height < Application.Current.MainWindow.Height && (string)shot.Tag == "EnemyShot")
                    Canvas.SetTop(shot, Canvas.GetTop(shot) + shotSpeedDown);
                else
                {
                    shotsToRemove.Add(shot);

                    MainCanvas.Children.Remove(shot);
                }    
            }

            CleanUpShots(shotsToRemove);
        }

        private void UpdatePlayerPos()
        {
            if (currentDirection == Direction.Left && Canvas.GetLeft(Player) > 20)
                Canvas.SetLeft(Player, Canvas.GetLeft(Player) + playerSpeedLeft);
            if (currentDirection == Direction.Right && Canvas.GetLeft(Player) + Player.Width < Application.Current.MainWindow.Width - Player.Width)
                Canvas.SetLeft(Player, Canvas.GetLeft(Player) + playerSpeedRight);
        }

        private void CheckCollisions()
        {
            Rect player = new Rect(Canvas.GetLeft(Player), Canvas.GetTop(Player), Player.Width, Player.Height);

            List<Rectangle> enemiesToRemove = new List<Rectangle>();
            List<Rectangle> shotsToRemove = new List<Rectangle>();

            foreach (Rectangle enemy in enemies)
            {
                foreach (Rectangle shot in shots)
                {
                    Rect currentShot = new Rect(Canvas.GetLeft(shot), Canvas.GetTop(shot), shot.Width, shot.Height);
                    Rect currentEnemy = new Rect(Canvas.GetLeft(enemy), Canvas.GetTop(enemy), enemy.Width, enemy.Height);

                    if (currentShot.IntersectsWith(currentEnemy) && (string)shot.Tag == "PlayerShot")
                    {
                        score += 10;

                        enemiesToRemove.Add(enemy);
                        shotsToRemove.Add(shot);

                        MainCanvas.Children.Remove(shot);
                        MainCanvas.Children.Remove(enemy);
                    }
                    else if (currentShot.IntersectsWith(player) && (string)shot.Tag == "EnemyShot")
                    {
                        shotsToRemove.Add(shot);
                        MainCanvas.Children.Remove(shot);
                        timer.Stop();

                        Label end = new Label();
                        end.Content = $"Game Over! Total Score: {score}";
                        end.FontFamily = new FontFamily("Segoe UI");
                        end.FontSize = 24;
                        end.Foreground = Brushes.Red;
                        end.Background = Brushes.Black;
                        end.HorizontalAlignment = HorizontalAlignment.Stretch;
                        end.VerticalAlignment = VerticalAlignment.Stretch;
                        end.VerticalContentAlignment = VerticalAlignment.Center;
                        end.HorizontalContentAlignment = HorizontalAlignment.Center;

                        MainGrid.Children.Add(end);
                        Grid.SetRow(end, 0);
                        Grid.SetColumnSpan(end, 2);
                    }
                }
            }

            CleanUpShots(shotsToRemove);
            CleanUpEnemies(enemiesToRemove);
        }

        private void CleanUpShots(List<Rectangle> shotsToRemove)
        {
            foreach (Rectangle shot in shotsToRemove)
            {
                shots.Remove(shot);
            }
        }

        private void CleanUpEnemies(List<Rectangle> enemiesToRemove)
        {
            foreach (Rectangle enemy in enemiesToRemove)
            {
                enemies.Remove(enemy);
            }
        }

        private void SpawnEnemy()
        {
            if (enemies.Count > 0)
                return;

            for (int y = 1; y < 4; y++)
            {
                for (int x = 1; x < 6; x++)
                {
                    Rectangle enemy = new Rectangle();
                    enemy.Width = 40;
                    enemy.Height = 40;
                    enemy.Stretch = Stretch.UniformToFill;
                    ImageBrush brush = new ImageBrush();
                    brush.ImageSource = new BitmapImage(new Uri(@"images\invader.png", UriKind.Relative));
                    enemy.Fill = brush;

                    MainCanvas.Children.Add(enemy);
                    enemies.Add(enemy);
                    Canvas.SetTop(enemy, y * 50);
                    Canvas.SetLeft(enemy, x * 59);
                }
            }

            System.Threading.Thread.Sleep(300);
        }

        private void PlayerShoot()
        {
            Rectangle shot = new Rectangle();
            shot.Width = 5;
            shot.Height = 20;
            shot.Fill = Brushes.White;
            shot.Tag = "PlayerShot";

            MainCanvas.Children.Add(shot);
            shots.Add(shot);
            Canvas.SetTop(shot, Canvas.GetTop(Player) - 25);
            Canvas.SetLeft(shot, Canvas.GetLeft(Player) + (Player.Width / 2) - 3);
        }

        private void EnemyShoot()
        {
            foreach (Rectangle enemy in enemies)
            {
                Rectangle shot = new Rectangle();
                shot.Width = 5;
                shot.Height = 20;
                shot.Fill = Brushes.Red;
                shot.Tag = "EnemyShot";

                Random random = new Random();

                int doIShoot = random.Next(0, 750);

                if (doIShoot == 0)
                {
                    MainCanvas.Children.Add(shot);
                    shots.Add(shot);
                    Canvas.SetTop(shot, Canvas.GetTop(enemy) + 25);
                    Canvas.SetLeft(shot, Canvas.GetLeft(enemy) + (Player.Width / 2) - 3);
                }
            }
        }

        private void KeyDownEvent(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
                currentDirection = Direction.Left;
            if (e.Key == Key.Right)
                currentDirection = Direction.Right;

            if (e.Key == Key.Space)
                PlayerShoot();
        }

        private void KeyUpEvent(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                currentDirection = Direction.None;
            }
            if (e.Key == Key.Right)
            {
                currentDirection = Direction.None;
            }
        }
    }
}
