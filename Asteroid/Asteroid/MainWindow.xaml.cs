//  Braden Boettcher - 2017
//
// Extra Credit Considerations: I put a LOT of time and effort into this.
//      - I was able to implement multiplayer co-op mode
//      - I was able to get a good difficulty setting mode working
//        where easy/medium/hard directly relate to firing speed/enemy
//        firing speed/points earned/etc.
//
//  Strengths?:
//      - Readable code (I think it's readable)
//      - Easily Extendable: I plan on adding battle mode, a campaign mode, new ships, etc. over winter break
//
//  Shortcomings a.k.a. "Planned Fixes": 
//      - Lots of "magic numbers" I did get rid of a lot, but there are still too many.
//      - Collision algorithm is not optimized well (needs a grid-based approach)
//        but I won't have that updated by the time it's due.
//      - THIS Monster Class - I never really figured out the correct way to do multiple forms in WPF
//        and that resulted in this huge XAML / .cs file
//
//        ****USE CODE FOLDING PLEASE FOR READABILITY!****
//                    ESPECIALLY IN THE XAML
//
//      - Visibility: I made everything "quick & dirty" while trying to get things working
//        and ran out of time for cleaning that up.
//      - Duplicate Code: A lot of duplicate code with minor differences.
//      - Looks and runs great on my machine - but I don't think it would work well with smaller displays
//        I don't have very good scaling on menus and stuff - and for some reason I couldn't get scrolling
//        menus to work.
//

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
using Asteroid.Objects;
using Asteroid.Ordnances;
using System.Windows.Threading;
using Asteroid.Effects;
using System.Data.SQLite;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows.Media.Effects;

namespace Asteroid
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Random seed;
        public List<GameObject> Objects;
        public List<PlayerShip> Players;
        PlayerShip p1, p2;

        //Visuals
        Brush p1stroke;
        Brush p1fill;
        Color p1glow;
        Brush p2stroke;
        Brush p2fill;
        Color p2glow;
        Brush Astroke = (Brush)new BrushConverter().ConvertFrom("#FF777777");
        Brush Afill = (Brush)new BrushConverter().ConvertFrom("#FF0B0B0B");
        Color Aglow = Colors.Black;
        int p1health;
        int p2health;
        ProgressBar p1HealthBar;
        ProgressBar p2HealthBar;

        //Game Logic
        double difficulty;
        int p1lives, p2lives;
        int gameRound;
        double saucerTimer, motherShipTimer;
        private bool gameStarted;
        private bool gameIs2Player;
        private bool cheatsEnabled;
        DispatcherTimer timer;
        private bool cheatsUsed;

        //Database
        SQLiteConnection conn;
        SQLiteCommand cmd;
        SQLiteDataReader dr;

        public Color P1_Custom_Glow { get; private set; }
        public Color P2_Custom_Glow { get; private set; }
        public bool GameIsBattle { get; private set; }
        public int BattleArena { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            InitializeDatabase();

            //Default Values
            Objects = new List<GameObject>();
            Players = new List<PlayerShip>();
            seed = new Random();
            gameStarted = false;
            gameIs2Player = false;
            cheatsEnabled = false;
            cheatsUsed = false;
            difficulty = 1.0; //normal (easy = .5, hard = 2.0)
            gameRound = 1;
            p1stroke = Brushes.Lime;
            p1fill = Brushes.Transparent;
            p1glow = Colors.Lime;
            p1lives = 3;
            p1health = 1;
            p2stroke = Brushes.Orange;
            p2fill = Brushes.Transparent;
            p2glow = Colors.Orange;
            p2lives = 3;
            p2health = 1;
        }

        private void InitializeDatabase()
        {
            try
            {
                conn = new SQLiteConnection("Data Source=HighScores.db;Version=3;New=True;Compress=True;");
                conn.Open();

                cmd = conn.CreateCommand();
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS SP_HighScores(ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, Player varchar(15), Score INTEGER);";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS MP_HighScores(ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, Players varchar(15), Score INTEGER);";
                cmd.ExecuteNonQuery();

                //If < 10 High Scores -> fill remainder with default values
                int spRecords = 0;
                cmd.CommandText = "SELECT * FROM SP_HighScores;";
                dr = cmd.ExecuteReader();

                while (dr.Read())
                    spRecords++;
                dr.Close();
                for (int i = 0; i < (10 - spRecords); i++)
                {
                    cmd.CommandText = "INSERT INTO SP_HighScores(Player, Score) VALUES('---','0');";
                    cmd.ExecuteNonQuery();
                }

                int mpRecords = 0;
                cmd.CommandText = "SELECT * FROM MP_HighScores;";
                dr = cmd.ExecuteReader();

                while (dr.Read())
                    mpRecords++;
                dr.Close();
                for (int i = 0; i < (10 - mpRecords); i++)
                {
                    cmd.CommandText = "INSERT INTO MP_HighScores(Players, Score) VALUES('--- & ---','0');";
                    cmd.ExecuteNonQuery();
                }

                conn.Close();
                conn = null;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Database Operations Error: " + ex.Message);
                if (conn != null)
                    conn.Close();
                if (!dr.IsClosed)
                    dr.Close();
                if (!dr.IsClosed)
                    dr.Close();

                dr = null;
                dr = null;
                conn = null;
            }
        }

        //MENU NAVIGATION
        #region Menus

        #region BUTTON EFFECTS
        private void TB_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock s = (TextBlock)sender;
            s.Foreground = (Brush)new BrushConverter().ConvertFromString("#FFB163FF");
        }

        private void TB_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBlock s = (TextBlock)sender;
            s.Foreground = Brushes.White;
        }

        private void TB_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBlock s = (TextBlock)sender;
            s.Foreground = Brushes.Black;
        }

        private void RB_MouseLeave(object sender, MouseEventArgs e)
        {
            RadioButton s = (RadioButton)sender;
            s.Foreground = Brushes.Black;
        }

        private void RB_MouseEnter(object sender, MouseEventArgs e)
        {
            RadioButton s = (RadioButton)sender;
            s.Foreground = Brushes.White;
        }

        private void RB_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            RadioButton s = (RadioButton)sender;
            s.Foreground = (Brush)new BrushConverter().ConvertFromString("#FFB163FF");
        }
        #endregion

        //MAIN MENU
        #region MAIN MENU Button Clicks
        //SINGLE PLAYER
        private void TB_SinglePlayer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TB_SinglePlayer.Foreground = Brushes.White;
            Main_Menu.IsEnabled = false;
            Main_Menu.Visibility = Visibility.Hidden;
            Single_Player_Menu.IsEnabled = true;
            Single_Player_Menu.Visibility = Visibility.Visible;
        }

        //MULTIPLAYER
        private void TB_MultiPlayer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TB_MultiPlayer.Foreground = Brushes.White;
            Main_Menu.IsEnabled = false;
            Main_Menu.Visibility = Visibility.Hidden;
            Multi_Player_Menu.IsEnabled = true;
            Multi_Player_Menu.Visibility = Visibility.Visible;
        }

        //CUSTOMIZE
        private void TB_Customize_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TB_Customize.Foreground = Brushes.White;
            Customize.IsEnabled = true;
            Customize.Visibility = Visibility.Visible;
            Main_Menu.IsEnabled = false;
            Main_Menu.Visibility = Visibility.Hidden;
        }

        //HIGH SCORES
        private void TB_HighScores_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TB_HighScores.Foreground = Brushes.White;
            Main_Menu.IsEnabled = false;
            Main_Menu.Visibility = Visibility.Hidden;
            High_Scores_Display.IsEnabled = true;
            High_Scores_Display.Visibility = Visibility.Visible;
        }

        //OPTIONS
        private void TB_Options_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TB_Options.Foreground = Brushes.White;
            Main_Menu.IsEnabled = false;
            Main_Menu.Visibility = Visibility.Hidden;
            Options_Menu.IsEnabled = true;
            Options_Menu.Visibility = Visibility.Visible;
        }

        //QUIT
        private void TB_Quit_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
        #endregion

        //OPTIONS MENU
        #region OPTIONS MENU Button Clicks
        private void TB_OPT_About_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TB_OPT_About.Foreground = Brushes.White;
            Options_Menu.Visibility = Visibility.Hidden;
            Options_Menu.IsEnabled = false;
            About.Visibility = Visibility.Visible;
            About.IsEnabled = true;
        }

        private void TB_OPT_Back_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TB_OPT_Back.Foreground = Brushes.White;
            Options_Menu.IsEnabled = false;
            Options_Menu.Visibility = Visibility.Hidden;
            Main_Menu.IsEnabled = true;
            Main_Menu.Visibility = Visibility.Visible;
        }

        private void About_Back_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            About_Back.Foreground = Brushes.White;
            Options_Menu.Visibility = Visibility.Visible;
            Options_Menu.IsEnabled = true;
            About.Visibility = Visibility.Hidden;
            About.IsEnabled = false;
        }
        #endregion

        //CUSTOMIZE MENU
        #region CUSTOMIZE MENU Button Clicks/Logic
        private void P1_Ship_Color_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(CustomP1StrokeRed != null && CustomP1StrokeGreen != null && CustomP1StrokeBlue != null)
            {
                int R = (int)CustomP1StrokeRed.Value;
                int G = (int)CustomP1StrokeGreen.Value;
                int B = (int)CustomP1StrokeBlue.Value;
                P1_Custom_Glow = Color.FromRgb((byte)R, (byte)G, (byte)B);
                P1_Custom_Ship.Stroke = new SolidColorBrush(Color.FromRgb((byte)R, (byte)G, (byte)B));
                P1_Custom_Ship.Effect = new DropShadowEffect()
                {
                    Color = Color.FromRgb((byte)R, (byte)G, (byte)B),
                    ShadowDepth = 0,
                    BlurRadius = 50
                };
            }
        }

        private void P2_Ship_Color_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CustomP2StrokeRed != null && CustomP2StrokeGreen != null && CustomP2StrokeBlue != null)
            {
                int R = (int)CustomP2StrokeRed.Value;
                int G = (int)CustomP2StrokeGreen.Value;
                int B = (int)CustomP2StrokeBlue.Value;
                P2_Custom_Glow = Color.FromRgb((byte)R, (byte)G, (byte)B);
                P2_Custom_Ship.Stroke = new SolidColorBrush(Color.FromRgb((byte)R, (byte)G, (byte)B));
                P2_Custom_Ship.Effect = new DropShadowEffect()
                {
                    Color = Color.FromRgb((byte)R, (byte)G, (byte)B),
                    ShadowDepth = 0,
                    BlurRadius = 50
                };
            }
        }

        private void TB_CUST_Accept_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            p1stroke = P1_Custom_Ship.Stroke;
            p1fill = Brushes.Transparent;
            p1glow = P1_Custom_Glow;
            p2stroke = P2_Custom_Ship.Stroke;
            p2fill = Brushes.Transparent;
            p2glow = P2_Custom_Glow;

            TB_CUST_Accept.Foreground = Brushes.White;
            Customize.IsEnabled = false;
            Customize.Visibility = Visibility.Hidden;
            Main_Menu.IsEnabled = true;
            Main_Menu.Visibility = Visibility.Visible;
        }

        private void TB_CUST_Cancel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CustomP1StrokeRed.Value = 0;
            CustomP1StrokeGreen.Value = 255;
            CustomP1StrokeBlue.Value = 255;
            CustomP2StrokeRed.Value = 255;
            CustomP2StrokeGreen.Value = 165;
            CustomP2StrokeBlue.Value = 0;
            p1stroke = Brushes.Cyan;
            p1fill = Brushes.Transparent;
            p1glow = Colors.Cyan;
            p2stroke = Brushes.Orange;
            p2fill = Brushes.Transparent;
            p2glow = Colors.Orange;

            TB_CUST_Cancel.Foreground = Brushes.White;
            Customize.IsEnabled = false;
            Customize.Visibility = Visibility.Hidden;
            Main_Menu.IsEnabled = true;
            Main_Menu.Visibility = Visibility.Visible;
        }
        #endregion
        
        //SINGLE PLAYER MENU
        #region SINGLE PLAYER MENU Button Clicks
        private void TB_SP_Arcade_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            canvas.Visibility = Visibility.Visible;
            TB_SP_Arcade.Foreground = Brushes.White;
            Single_Player_Menu.Visibility = Visibility.Hidden;
            Single_Player_Menu.IsEnabled = false;
            this.Cursor = Cursors.None;
            SinglePlayerArcadeStart();
        }

        //private void TB_SP_Campaign_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    TB_SP_Campaign.Foreground = Brushes.White;
        //    MessageBox.Show("Coming in Version 1.0!", "CAMPAIGN MODE COMING SOON", MessageBoxButton.OK, MessageBoxImage.None);
        //}

        private void TB_SP_Controls_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TB_SP_Controls.Foreground = Brushes.White;
            SP_Controls.Visibility = Visibility.Visible;
            SP_Controls.IsEnabled = true;
            Single_Player_Menu.Visibility = Visibility.Hidden;
            Single_Player_Menu.IsEnabled = false;
        }

        private void SP_Controls_Back_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SP_Controls_Back.Foreground = Brushes.White;
            SP_Controls.Visibility = Visibility.Hidden;
            SP_Controls.IsEnabled = false;
            Single_Player_Menu.Visibility = Visibility.Visible;
            Single_Player_Menu.IsEnabled = true;
        }

        private void TB_SP_Back_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TB_SP_Back.Foreground = Brushes.White;
            Single_Player_Menu.IsEnabled = false;
            Single_Player_Menu.Visibility = Visibility.Hidden;
            Main_Menu.IsEnabled = true;
            Main_Menu.Visibility = Visibility.Visible;
        }

        #endregion

        //MULTIPLAYER MENU
        #region MULTIPLAYER MENU Button Clicks
        private void TB_MP_Coop_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TB_MP_Coop.Foreground = Brushes.White;
            canvas.Visibility = Visibility.Visible;
            Multi_Player_Menu.Visibility = Visibility.Hidden;
            Multi_Player_Menu.IsEnabled = false;
            this.Cursor = Cursors.None;
            MultiPlayerArcadeStart();
        }

        private void TB_MP_Battle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TB_MP_Battle.Foreground = Brushes.White;
            canvas.Visibility = Visibility.Visible;
            Multi_Player_Menu.Visibility = Visibility.Hidden;
            Multi_Player_Menu.IsEnabled = false;
            this.Cursor = Cursors.None;
            MultiPlayerBattleStart();
        }

        //private void TB_MP_Versus_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    TB_MP_Versus.Foreground = Brushes.White;
        //}

        //private void TB_MP_Customize_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    TB_MP_Customize.Foreground = Brushes.White;
        //}

        private void TB_MP_Controls_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TB_MP_Controls.Foreground = Brushes.White;           
            MP_Controls.Visibility = Visibility.Visible;
            MP_Controls.IsEnabled = true;
            Multi_Player_Menu.Visibility = Visibility.Hidden;
            Multi_Player_Menu.IsEnabled = false;
        }

        private void MP_Controls_Back_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MP_Controls_Back.Foreground = Brushes.White;
            MP_Controls.Visibility = Visibility.Hidden;
            MP_Controls.IsEnabled = false;
            Multi_Player_Menu.Visibility = Visibility.Visible;
            Multi_Player_Menu.IsEnabled = true;
        }

        private void TB_MP_Back_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TB_MP_Back.Foreground = Brushes.White;
            Multi_Player_Menu.IsEnabled = false;
            Multi_Player_Menu.Visibility = Visibility.Hidden;
            Main_Menu.IsEnabled = true;
            Main_Menu.Visibility = Visibility.Visible;
        }

        #endregion

        //PAUSE MENU
        #region PAUSE MENU Button Clicks
        private void TB_PM_Resume_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Pause_Menu.IsEnabled = false;
            Pause_Menu.Visibility = Visibility.Hidden;
            canvas.Visibility = Visibility.Visible;
            this.Cursor = Cursors.None;
            timer.Start();
        }
        private void TB_PM_Quit_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            gameStarted = false;
            Main_Menu.IsEnabled = true;
            Main_Menu.Visibility = Visibility.Visible;
            canvas.Children.RemoveRange(0, canvas.Children.Count);
            ArcadeHUD.Visibility = Visibility.Hidden;
            Pause_Menu.IsEnabled = false;
            Pause_Menu.Visibility = Visibility.Hidden;
        }
        private void PauseCheatsDisabled_Checked(object sender, RoutedEventArgs e)
        {
            cheatsEnabled = false;
        }
        private void PauseCheatsEnabled_Checked(object sender, RoutedEventArgs e)
        {
            cheatsEnabled = true;
        }
        #endregion

        //HIGH SCORES MENU
        #region HIGH SCORE MENU Button Clicks
        private void HS_Back_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            HS_Back.Foreground = Brushes.White;
            Main_Menu.IsEnabled = true;
            Main_Menu.Visibility = Visibility.Visible;
            High_Scores_Display.IsEnabled = false;
            High_Scores_Display.Visibility = Visibility.Hidden;
        }

        private void HS_MultiPlayerScores_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            HS_MultiPlayerScores.Foreground = Brushes.White;
            High_Scores_Display.Visibility = Visibility.Hidden;
            High_Scores_Display.IsEnabled = false;
            //Fill HIGH SCORES TABLE with sorted MP values
            conn = new SQLiteConnection("Data Source=HighScores.db;Version=3;New=True;Compress=True;");
            conn.Open();
            cmd = conn.CreateCommand();
            List<TextBlock> table = new List<TextBlock>();
            foreach(TextBlock t in High_Scores_Table.Children)
            {
                if (t.Name != "HS_Return")
                    table.Add(t);
            }

            cmd.CommandText = "SELECT * FROM MP_HighScores ORDER BY Score DESC;";
            dr = cmd.ExecuteReader();
            int i = 0;
            while(dr.Read())
            {
                table[i].Text = " "+dr["Players"].ToString();
                i++;
                table[i].Text = dr["Score"].ToString()+" ";
                i++;
            }
            dr.Close();
            conn.Close();
            High_Scores_Table.Visibility = Visibility.Visible;
            High_Scores_Table.IsEnabled = true;
        }

        private void HS_SinglePlayerScores_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            HS_SinglePlayerScores.Foreground = Brushes.White;
            High_Scores_Display.Visibility = Visibility.Hidden;
            High_Scores_Display.IsEnabled = false;
            //Fill HIGH SCORES TABLE with sorted MP values
            conn = new SQLiteConnection("Data Source=HighScores.db;Version=3;New=True;Compress=True;");
            conn.Open();
            cmd = conn.CreateCommand();
            List<TextBlock> table = new List<TextBlock>();
            foreach (TextBlock t in High_Scores_Table.Children)
            {
                if (t.Name != "HS_Return")
                    table.Add(t);
            }

            cmd.CommandText = "SELECT * FROM SP_HighScores ORDER BY Score DESC;";
            dr = cmd.ExecuteReader();
            int i = 0;
            while (dr.Read())
            {
                table[i].Text = " "+ dr["Player"].ToString();
                i++;
                table[i].Text = dr["Score"].ToString()+" ";
                i++;
            }
            dr.Close();
            conn.Close();
            High_Scores_Table.Visibility = Visibility.Visible;
            High_Scores_Table.IsEnabled = true;
        }

        private void HS_Return_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            HS_Return.Foreground = Brushes.White;
            High_Scores_Table.Visibility = Visibility.Hidden;
            High_Scores_Table.IsEnabled = false;
            High_Scores_Display.Visibility = Visibility.Visible;
            High_Scores_Display.IsEnabled = true;
        }
        #endregion
        #region HIGH SCORES Logic
        private void AddHighScore(string name, int score, string gameMode)
        {
            if (!cheatsUsed)
            {
                try
                {
                    conn = new SQLiteConnection("Data Source=HighScores.db;Version=3;New=True;Compress=True;");
                    conn.Open();
                    cmd = conn.CreateCommand();

                    cmd.CommandText = "SELECT MIN(" + gameMode + "_HighScores.Score) FROM " + gameMode + "_HighScores;";
                    int minHighScore = int.Parse(cmd.ExecuteScalar().ToString());
                    //int minHighScore = (int)dr[0];

                    if (score > minHighScore) //method(score, gameMode) 'where GameMode=="gameMode"' returns true: high score is in top 10
                    {
                        //Delete Min Value
                        cmd.CommandText = "DELETE FROM " + gameMode + "_HighScores WHERE ID = (SELECT ID FROM " + gameMode + "_HighScores WHERE Score = (SELECT MIN(" + gameMode + "_HighScores.Score) FROM " + gameMode + "_HighScores) LIMIT 1);";
                        cmd.ExecuteNonQuery();

                        //Insert New Value
                        if (gameIs2Player)
                        {
                            cmd.CommandText = "INSERT INTO MP_HighScores(Players, Score) VALUES('" + name + "','" + score.ToString() + "');";
                            cmd.ExecuteNonQuery();
                        }
                        else
                        {
                            cmd.CommandText = "INSERT INTO SP_HighScores(Player, Score) VALUES('" + name + "','" + score.ToString() + "');";
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Database Operation Error: " + ex.Message);
                }
                finally
                {
                    if (conn != null)
                        conn.Close();
                    conn = null;
                }

            }
            else
                MessageBox.Show("You Scored: " + score + "!\nBUT you used cheats.\nSo this stays out of the High Scores.", "Well Done.", MessageBoxButton.OK, MessageBoxImage.None);

            GoToHighScores(gameIs2Player);

        }

        private void GoToHighScores(bool is2Player)
        {
            if (is2Player)
                HS_MultiPlayerScores_MouseLeftButtonUp(HS_MultiPlayerScores, null);
            else
                HS_SinglePlayerScores_MouseLeftButtonUp(HS_SinglePlayerScores, null);
        }

        private void GetPlayerInitials(bool is2Player)
        {
            SP_Initials.IsEnabled = true;
            SP_Initials.Visibility = Visibility.Visible;
            if (is2Player)
            {
                MP_Initials.IsEnabled = true;
                MP_Initials.Visibility = Visibility.Visible;
            }
            else
            {
                MP_Initials.IsEnabled = false;
                MP_Initials.Visibility = Visibility.Hidden;
            }
        }

        private void SP_Init_Submit_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            bool valid = true;
            string name = "";
            if (I1.Text != "-" && I2.Text != "-" && I3.Text != "-")
            {
                name = I1.Text;
                name += I2.Text;
                name += I3.Text;

                if (gameIs2Player)
                {
                    if (I4.Text != "-" && I5.Text != "-" && I6.Text != "-")
                    {
                        name += " & ";
                        name += I4.Text;
                        name += I5.Text;
                        name += I6.Text;
                    }
                    else
                        valid = false;
                }
            }
            else
                valid = false;
            if (valid)
            {
                SP_Initials.IsEnabled = false;
                SP_Initials.Visibility = Visibility.Hidden;
                MP_Initials.IsEnabled = false;
                MP_Initials.Visibility = Visibility.Hidden;
                AddHighScore(name, int.Parse(ArcadeScore.Text), gameIs2Player ? "MP" : "SP");
            }
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            string initial = ((TextBox)sender).Text;
            if (!Regex.IsMatch(initial, @"^[a-zA-Z]{1}$"))
            {
                ((TextBox)sender).Text = "-";
            }
            else
            {
                ((TextBox)sender).Text = initial.ToUpper();
            }
        }
        #endregion
        
        #endregion

        #region SINGLE PLAYER ARCADE
        private void SinglePlayerArcadeStart()
        {
            //Arcade Initialization
            gameStarted = true;
            gameIs2Player = false;
            GameIsBattle = false;

            Objects = new List<GameObject>();
            Players = new List<PlayerShip>();
            gameRound = 1;

            if ((bool)EasyMode.IsChecked)
            {
                difficulty = 0.5;
                ArcadeDifficulty.Text = "EASY";
            }
            else if ((bool)MediumMode.IsChecked)
            {
                difficulty = 1.0; //normal
                ArcadeDifficulty.Text = "MEDIUM";
            }
            else if ((bool)HardMode.IsChecked)
            {
                difficulty = 2.0; //twice as hard
                ArcadeDifficulty.Text = "HARD";
            }
            else
            {
                difficulty = 1.0;
                ArcadeDifficulty.Text = "MEDIUM";
            }

            Objects.Asteroid a1 = new Objects.Asteroid(this, Astroke, Afill, Aglow, 25, difficulty, 5, new Point(300, 100), "ExtraLarge", seed.Next(0, 3), seed.Next());
            p1 = new PlayerShip(p1stroke, p1fill, p1glow, 3, difficulty, gameRound, p1lives, p1health, new Point(canvas.ActualWidth / 2, canvas.ActualHeight / 2), canvas);

            Objects.Add(p1);
            Players.Add(p1);
            canvas.Children.Add(p1.Geometry);

            Objects.Add(a1);
            canvas.Children.Add(a1.Geometry);

            saucerTimer = 2000 / difficulty;
            motherShipTimer = 4000 / difficulty;

            //Arcade Heads-Up-Display
            double x = canvas.ActualWidth / 2;
            Polyline HUD = new Polyline
            {
                Points = new PointCollection()
                {
                    new Point(x-300,0),
                    new Point(x-310,15),
                    new Point(x-170,15),
                    new Point(x-160,0),
                    new Point(x-180,30),
                    new Point(x-100,30),
                    new Point(x-80,0),
                    new Point(x-120,60),
                    new Point(x+120,60),
                    new Point(x+80,0),
                    new Point(x+100,30),
                    new Point(x+180,30),
                    new Point(x+160,0),
                    new Point(x+170,15),
                    new Point(x+310,15),
                    new Point(x+300,0),
                },
                Stroke = p1stroke,
                Fill = Brushes.Transparent,
                Effect = new FX(p1glow).Glow,
                StrokeThickness = 5
            };

            p1HealthBar = new ProgressBar();
            p1HealthBar.Background = Brushes.Black;
            p1HealthBar.Foreground = p1stroke;
            p1HealthBar.BorderBrush = p1stroke;
            p1HealthBar.BorderThickness = new Thickness(1);
            p1HealthBar.Maximum = p1.MaxHealth;
            p1HealthBar.Minimum = 0;
            p1HealthBar.Height = 10;
            p1HealthBar.Width = 100;
            p1HealthBar.Margin = new Thickness(x - 285, 2, 0, 0);
            canvas.Children.Add(p1HealthBar);

            canvas.Children.Add(HUD);
            ArcadeLives.Text = p1.Lives.ToString();
            ArcadeScore.Text = p1.Score.ToString();
            ArcadeHUD.IsEnabled = true;
            ArcadeHUD.Visibility = Visibility.Visible;

            timer = new DispatcherTimer();
            timer.Tick += SP_Arcade_Move_Elements;
            timer.Interval = new System.TimeSpan(100000);
            timer.Start();
        }
        private void SP_Arcade_Move_Elements(object sender, EventArgs e)
        {
            MoveElements();

            NewCollisionTest();

            ArcadeLives.Text = p1.Lives.ToString();
            ArcadeScore.Text = ((int)(p1.Score)).ToString();
            p1HealthBar.Value = p1.Health;

            CheckForRoundClear();

            SpawnShips();

            CheckEndCondition(p1.Lives <= 0);
        }
        #endregion
        #region SP CAMPAIGN
        #endregion

        #region MULTIPLAYER CO-OP
        private void MultiPlayerArcadeStart()
        {
            //Arcade Initialization
            gameStarted = true;
            gameIs2Player = true;
            GameIsBattle = false;

            Objects = new List<GameObject>();
            Players = new List<PlayerShip>();
            gameRound = 1;

            if ((bool)EasyMode.IsChecked)
            {
                difficulty = 0.5;
                ArcadeDifficulty.Text = "EASY";
            }
            else if ((bool)MediumMode.IsChecked)
            {
                difficulty = 1.0; //normal
                ArcadeDifficulty.Text = "MEDIUM";
            }
            else if ((bool)HardMode.IsChecked)
            {
                difficulty = 2.0; //twice as hard
                ArcadeDifficulty.Text = "HARD";
            }
            else
            {
                difficulty = 1.0;
                ArcadeDifficulty.Text = "MEDIUM";
            }

            p1 = new PlayerShip(p1stroke, p1fill, p1glow, 3, difficulty, gameRound, p1lives, 1, new Point((canvas.ActualWidth / 2) - 50, canvas.ActualHeight / 2), canvas);
            Objects.Add(p1);
            Players.Add(p1);
            canvas.Children.Add(p1.Geometry);

            p2 = new PlayerShip(p2stroke, p2fill, p2glow, 3, difficulty, gameRound, p2lives, 1, new Point((canvas.ActualWidth / 2) + 50, canvas.ActualHeight / 2), canvas);
            Objects.Add(p2);
            Players.Add(p2);
            canvas.Children.Add(p2.Geometry);

            Objects.Asteroid a1 = new Objects.Asteroid(this, Astroke, Afill, Aglow, 25, difficulty, 5, new Point(300, 100), "ExtraLarge", seed.Next(0,3), seed.Next());
            Objects.Add(a1);
            canvas.Children.Add(a1.Geometry);
            saucerTimer = 2000 / difficulty;
            motherShipTimer = 4000 / difficulty;

            //Arcade Heads-Up-Display
            double x = canvas.ActualWidth / 2;
            Polyline HUD = new Polyline
            {
                Points = new PointCollection()
                {
                    new Point(x-210,0),
                    new Point(x-220,15),
                    new Point(x-170,15),
                    new Point(x-160,0),
                    new Point(x-180,30),
                    new Point(x-100,30),
                    new Point(x-80,0),
                    new Point(x-120,60),
                    new Point(x+120,60),
                    new Point(x+80,0),
                    new Point(x+100,30),
                    new Point(x+180,30),
                    new Point(x+160,0),
                    new Point(x+170,15),
                    new Point(x+220,15),
                    new Point(x+210,0),
                },
                Stroke = p1stroke,
                Fill = Brushes.Transparent,
                Effect = new FX(p1glow).Glow,
                StrokeThickness = 5
            };
            canvas.Children.Add(HUD);
            ArcadeHUD.IsEnabled = true;
            ArcadeHUD.Visibility = Visibility.Visible;

            timer = new DispatcherTimer();
            timer.Tick += MP_COOP_Move_Elements;
            timer.Interval = new System.TimeSpan(100000);
            timer.Start();
        }
        private void MP_COOP_Move_Elements(object sender, EventArgs e)
        {
            MoveElements();

            NewCollisionTest();

            int p1Lives = p1.Lives;
            int p2Lives = p2.Lives;
            if (p1Lives <= 0) p1Lives = 0;
            if (p2Lives <= 0) p2Lives = 0;

            ArcadeLives.Text = (p1Lives+p2Lives).ToString();
            ArcadeScore.Text = ((int)((p1.Score+p2.Score))).ToString();

            CheckForRoundClear();

            SpawnShips();

            CheckEndCondition(p1Lives <= 0 && p2Lives <= 0);
        }
        #endregion

        #region MULTIPLAYER VERSUS
        #endregion

        #region MULTIPLAYER BATTLE
        private void MultiPlayerBattleStart()
        {
            //Arcade Initialization
            gameStarted = true;
            gameIs2Player = true;
            GameIsBattle = true;
            BattleArena = 2;
            Objects = new List<GameObject>();
            Players = new List<PlayerShip>();
            gameRound = 1;
            difficulty = .5;
            int p1Health = 20;
            int p2Health = 20;

            p1 = new PlayerShip(p1stroke, p1fill, p1glow, 3, difficulty, gameRound, p1lives, p1Health, new Point((canvas.ActualWidth / 2) - 50, canvas.ActualHeight / 2), canvas);
            Objects.Add(p1);
            Players.Add(p1);
            canvas.Children.Add(p1.Geometry);

            p2 = new PlayerShip(p2stroke, p2fill, p2glow, 3, difficulty, gameRound, p2lives, p2Health, new Point((canvas.ActualWidth / 2) + 50, canvas.ActualHeight / 2), canvas);
            Objects.Add(p2);
            Players.Add(p2);
            canvas.Children.Add(p2.Geometry);

            List<Wall> walls = CreateBattleArena(Players);
            foreach(Wall w in walls)
            {
                Objects.Add(w);
                canvas.Children.Add(w.Geometry);
            }
 
            //Battle Heads-Up-Display
            //ADD HUD & HEALTH BARS

            p1HealthBar = new ProgressBar();
            p1HealthBar.Background = Brushes.Black;
            p1HealthBar.Foreground = p1stroke;
            p1HealthBar.BorderBrush = p1stroke;
            p1HealthBar.BorderThickness = new Thickness(1);
            p1HealthBar.Maximum = p1.MaxHealth;
            p1HealthBar.Minimum = 0;
            p1HealthBar.Height = 10;
            p1HealthBar.Width = 100;
            p1HealthBar.Margin = new Thickness(canvas.ActualWidth/5, 20, 0, 0);
            canvas.Children.Add(p1HealthBar);

            p2HealthBar = new ProgressBar();
            p2HealthBar.Background = Brushes.Black;
            p2HealthBar.Foreground = p2stroke;
            p2HealthBar.BorderBrush = p2stroke;
            p2HealthBar.BorderThickness = new Thickness(1);
            p2HealthBar.Maximum = p2.MaxHealth;
            p2HealthBar.Minimum = 0;
            p2HealthBar.Height = 10;
            p2HealthBar.Width = 100;
            p2HealthBar.Margin = new Thickness(2*canvas.ActualWidth/5, 20, 0, 0);
            canvas.Children.Add(p2HealthBar);

            timer = new DispatcherTimer();
            timer.Tick += MP_BATTLE_Move_Elements;
            timer.Interval = new System.TimeSpan(100000);
            timer.Start();
        }
        private void MP_BATTLE_Move_Elements(object sender, EventArgs e)
        {
            MoveElements();
            p1HealthBar.Value = p1.Health;
            p2HealthBar.Value = p2.Health;

            BattleCollisionTest();

            int p1Lives = p1.Lives;
            int p2Lives = p2.Lives;
            if (p1Lives <= 0) p1Lives = 0;
            if (p2Lives <= 0) p2Lives = 0;

            ArcadeLives.Text = (p1Lives).ToString()+" "+(p2Lives).ToString();
            ArcadeScore.Text = ((int)((p1.Score + p2.Score))).ToString();

            //CheckForRoundClear();

            //SpawnShips();

            CheckEndCondition(p1Lives <= 0 || p2Lives <= 0);
        }
        private void BattleCollisionTest()
        {
            List<GameObject> copy = new List<GameObject>();
            copy.AddRange(Objects);

            foreach (PlayerShip p in Players)
            {
                foreach (GameObject obj in copy)
                {
                    if (!(obj is PlayerShip) || (p != obj as PlayerShip))
                    {
                        bool skipPlayerHitTest = false;
                        foreach (Ordnance pOrd in p.Ordnances)
                        {
                            if (!obj.IsDestroyed)
                            {
                                if (!(obj is PlayerShip) || !(((PlayerShip)obj).IsInvincible))
                                {
                                    if (pOrd.UsesPointCollision())
                                    {
                                        if (obj.Geometry.RenderedGeometry.StrokeContains(new Pen(Brushes.Transparent, 10), pOrd.CollisionPoint()))
                                        {
                                            skipPlayerHitTest = true;
                                            obj.DestructionColor = p.PlayerColor;
                                            obj.DestructionBrush = p.PlayerStroke;
                                            obj.Destroy();
                                            p.AddPoints(obj.GetPointValue());
                                            pOrd.Lifetime = 0;
                                        }
                                    }
                                    else //Not Point Collision
                                    {
                                        IntersectionDetail collides = obj.Geometry.RenderedGeometry.StrokeContainsWithDetail(new Pen(Brushes.Transparent, 3), pOrd.Geometry.RenderedGeometry, 2, ToleranceType.Relative);
                                        if (collides != IntersectionDetail.Empty && collides != IntersectionDetail.NotCalculated)
                                        {
                                            skipPlayerHitTest = true;
                                            obj.DestructionColor = p.PlayerColor;
                                            obj.DestructionBrush = p.PlayerStroke;
                                            obj.Destroy();
                                            p.AddPoints(obj.GetPointValue());
                                            pOrd.Lifetime = 0;
                                        }
                                    }
                                }
                            }
                        }
                        if (!skipPlayerHitTest && !p.IsInvincible && !p.IsDestroyed)
                        {
                            //TEST PLAYER DESTROYED BY ENEMY WEAPONS
                            foreach (Ordnance enemyOrdnance in obj.Ordnances)
                            {
                                if (enemyOrdnance.UsesPointCollision())
                                {
                                    if (p.Geometry.RenderedGeometry.StrokeContains(new Pen(Brushes.Transparent, 4), enemyOrdnance.CollisionPoint()))
                                    {
                                        p.Destroy();
                                        enemyOrdnance.Lifetime = 0;
                                    }
                                }
                                else //Not Point Collision
                                {
                                    IntersectionDetail collides = p.Geometry.RenderedGeometry.StrokeContainsWithDetail(new Pen(Brushes.Transparent, 4), enemyOrdnance.Geometry.RenderedGeometry, 2, ToleranceType.Relative);
                                    if (collides != IntersectionDetail.Empty && collides != IntersectionDetail.NotCalculated)
                                    {
                                        p.Destroy();
                                        enemyOrdnance.Lifetime = 0;
                                    }
                                }
                            }

                            //TEST FOR PLAYER DESTROYED BY OBJECT COLLISON
                            if (!p.IsDestroyed && !obj.IsDestroyed)
                            {
                                IntersectionDetail collides = obj.Geometry.RenderedGeometry.StrokeContainsWithDetail(new Pen(Brushes.Transparent, 4), p.Geometry.RenderedGeometry, 2, ToleranceType.Relative);
                                if (collides != IntersectionDetail.Empty && collides != IntersectionDetail.NotCalculated)
                                {
                                    p.Destroy();
                                    if (obj is PlayerShip && !((PlayerShip)obj).IsInvincible)
                                    {
                                        obj.Destroy();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private List<Wall> CreateBattleArena(List<PlayerShip> players)
        {
            double w = canvas.ActualWidth;
            double h = canvas.ActualHeight;
            int x = 1;
            List<PointCollection> points = new List<PointCollection>();
            List<Wall> walls = new List<Wall>();
            switch (BattleArena)
            {
                case 1:
                    points.Add(new PointCollection()
                    {
                        new Point(10,10), new Point(w-10,10),
                        new Point(w-10, h-10),
                        new Point(10,h-10), new Point(10,10)
                    });
                    walls.Add(new Wall(Brushes.DarkGray, Colors.DarkGray, 15, new Point(0, 0), points[0], 0, difficulty, new List<Ordnance>(), new List<Effects.Effect>()));
                    for (int i = 0; i < players.Count; i++)
                    {
                        players[i].RespawnLocation = new Point((i + x) * w / 8, h / 2);
                        x++;
                    }
                    break;

                case 2:
                    points.Add(new PointCollection()
                    {
                        new Point (10, 10), new Point(w/2, 10),
                        new Point(w/2, h/2), new Point(w/2, 10),
                        new Point(w-10, 10), new Point(w-10, h-10),
                        new Point(3*w/4, h-10), new Point(3*w/4, h/2),
                        new Point(3*w/4, h-10), new Point(w/4, h-10),
                        new Point(w/4, h/2), new Point(w/4, h-10),
                        new Point(10,h-10), new Point(10,10)
                    });
                    walls.Add(new Wall(Brushes.DarkGray, Colors.DarkGray, 15, new Point(0, 0), points[0], 0, difficulty, new List<Ordnance>(), new List<Effects.Effect>()));
                    for (int i = 0; i < players.Count; i++)
                    {
                        players[i].RespawnLocation = new Point((i+x) * w / 8, h / 2);
                        x++;
                    }
                    break;
                case 3: break;
                case 4: break;
                case 5: break;
                case 6: break;
            }
            return walls;
        }
        
        #endregion

        #region GAMEPLAY HELPER METHODS

        private void MoveElements()
        {
            foreach (GameObject o in Objects)
            {
                o.Move();
                LoopBorders(o);
            }
            Objects.RemoveAll(obj => obj.RemoveFromGame == true);
        }

        private void LoopBorders(GameObject o)
        {
            double teleportOffsetX = 0;
            double teleportOffsetY = 0;
            bool loopBorders = false;

            if (o.Center.X > canvas.ActualWidth + 5)
            {
                teleportOffsetX = -(canvas.ActualWidth + 10);
                loopBorders = true;
            }
            else if (o.Center.X < -5)
            {
                teleportOffsetX = canvas.ActualWidth + 10;
                loopBorders = true;

            }
            if (o.Center.Y > canvas.ActualHeight + 5)
            {
                teleportOffsetY = -(canvas.ActualHeight + 10);
                loopBorders = true;

            }
            else if (o.Center.Y < -5)
            {
                teleportOffsetY = canvas.ActualHeight + 10;
                loopBorders = true;

            }
            if (loopBorders)
                o.TeleportPoints(new Point(teleportOffsetX, teleportOffsetY));
        }

        private void SpawnShips()
        {
            //SPAWN SHIPS
            if ((int)saucerTimer > 0)
                saucerTimer--;
            else
            {
                SpawnSmallSaucer();
                saucerTimer = 2000 / gameRound / difficulty;
            }
            if ((int)motherShipTimer > 0)
                motherShipTimer--;
            else
            {
                SpawnMotherShip();
                motherShipTimer = 4000 / gameRound / difficulty;
            }
        }

        private void CheckForRoundClear()
        {
            bool roundCleared = true;
            foreach (var a in Objects)
                if (a is Objects.Asteroid || a is SmallSaucer || a is MotherShip)
                    roundCleared = false;

            //NEW ROUND
            if (roundCleared)
            {
                if (int.Parse(ArcadeScore.Text) < 40000 && difficulty != .5)
                {
                    gameRound++;
                }
                foreach (PlayerShip p in Players)
                {
                    p.RoundModifier++;
                    p.IsInvincible = true;
                    p.InvincibilityTimer = 200;
                }
                for (int i = 0; i < gameRound; i++)
                {
                    Objects.Asteroid a = new Objects.Asteroid(this, Astroke, Afill, Aglow, 25, difficulty, 5, new Point(seed.Next(0, (int)canvas.ActualWidth), seed.Next(0, (int)canvas.ActualHeight)), "ExtraLarge", seed.Next(0, 3), seed.Next());
                    Objects.Add(a);
                    canvas.Children.Add(a.Geometry);
                }
            }
        }

        private void CheckEndCondition(bool endCondition)
        {
            if (endCondition)
                EndGame();
        }

        private void EndGame()
        {
            timer.Stop();
            this.Cursor = Cursors.Cross;
            gameStarted = false;
            canvas.Children.RemoveRange(0, canvas.Children.Count);
            ArcadeHUD.Visibility = Visibility.Hidden;
            if(GameIsBattle)
            {
                if (p1.Lives > 0)
                    MessageBox.Show("PLAYER 1 WINS!");
                else
                    MessageBox.Show("PLAYER 2 WINS!");
                Main_Menu.Visibility = Visibility.Visible;
                Main_Menu.IsEnabled = true;
            }
            else
            {
                GetPlayerInitials(gameIs2Player);
            }
        }

        private void NewCollisionTest()
        {
            List<GameObject> copy = new List<GameObject>();
            copy.AddRange(Objects);
            foreach (GameObject obj in copy)
            {
                if (!(obj is PlayerShip))
                {
                    Rect boundingBox = obj.Geometry.RenderedGeometry.Bounds;
                    boundingBox.Inflate(10, 10);

                    foreach (PlayerShip player in Players)
                    {
                        bool skipPlayerHitTest = false;
                        //TEST OBJECT DESTROYED BY PLAYER
                        foreach (Ordnance playerOrdnance in player.Ordnances)
                        {
                            if (!obj.IsDestroyed)
                            {
                                if (playerOrdnance.UsesPointCollision())
                                {
                                    if (obj.Geometry.RenderedGeometry.FillContains(playerOrdnance.CollisionPoint()))
                                    {
                                        skipPlayerHitTest = true;
                                        obj.DestructionColor = player.PlayerColor;
                                        obj.DestructionBrush = player.PlayerStroke;
                                        obj.Destroy();
                                        player.AddPoints(obj.GetPointValue());
                                        playerOrdnance.Lifetime = 0;
                                    }
                                }
                                else //Not Point Collision
                                {
                                    IntersectionDetail collides = obj.Geometry.RenderedGeometry.FillContainsWithDetail(playerOrdnance.Geometry.RenderedGeometry, 2, ToleranceType.Relative);
                                    if (collides != IntersectionDetail.Empty && collides != IntersectionDetail.NotCalculated)
                                    {
                                        skipPlayerHitTest = true;
                                        obj.DestructionColor = player.PlayerColor;
                                        obj.DestructionBrush = player.PlayerStroke;
                                        obj.Destroy();
                                        player.AddPoints(obj.GetPointValue());
                                        playerOrdnance.Lifetime = 0;
                                    }
                                }
                            }
                        }
                        if (!skipPlayerHitTest && !player.IsInvincible && !player.IsDestroyed)
                        {

                            //TEST PLAYER DESTROYED BY ENEMY WEAPONS
                            foreach (Ordnance enemyOrdnance in obj.Ordnances)
                            {
                                if (enemyOrdnance.UsesPointCollision())
                                {
                                    if (player.Geometry.RenderedGeometry.FillContains(enemyOrdnance.CollisionPoint()))
                                    {
                                        player.Destroy();
                                        enemyOrdnance.Lifetime = 0;
                                    }
                                }
                                else //Not Point Collision
                                {
                                    IntersectionDetail collides = player.Geometry.RenderedGeometry.FillContainsWithDetail(enemyOrdnance.Geometry.RenderedGeometry, 2, ToleranceType.Relative);
                                    if (collides != IntersectionDetail.Empty && collides != IntersectionDetail.NotCalculated)
                                    {
                                        player.Destroy();
                                        enemyOrdnance.Lifetime = 0;
                                    }
                                }
                            }

                            //TEST FOR PLAYER DESTROYED BY OBJECT COLLISON
                            if (!player.IsDestroyed && !obj.IsDestroyed)
                            {
                                Rect playerBoundingBox = player.Geometry.RenderedGeometry.Bounds;
                                if (boundingBox.IntersectsWith(playerBoundingBox))
                                {
                                    IntersectionDetail collides = obj.Geometry.RenderedGeometry.FillContainsWithDetail(player.Geometry.RenderedGeometry, 2, ToleranceType.Relative);
                                    if (collides != IntersectionDetail.Empty && collides != IntersectionDetail.NotCalculated)
                                    {
                                        player.Destroy();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SpawnSmallSaucer()
        {
            Random seed = new Random();
            double Y = new Random(seed.Next()).Next(20, (int)canvas.ActualHeight - 50);
            PlayerShip p = p1;
            if (gameIs2Player)
            {
                if (seed.Next(2) == 1)
                    p = p2;
                if (p1.Lives <= 0)
                    p = p2;
                if (p2.Lives <= 0)
                    p = p1;
            }
            SmallSaucer s1 = new SmallSaucer(canvas, Brushes.MediumPurple, Brushes.Transparent, Colors.MediumPurple, 3, 500, difficulty, new Point(canvas.ActualWidth, Y), 20, p);
            Objects.Add(s1);
            canvas.Children.Add(s1.Geometry);
        }

        private void SpawnMotherShip()
        {
            Random seed = new Random();
            double Y = new Random(seed.Next()).Next(20, (int)canvas.ActualHeight - 100);
            PlayerShip p = p1;
            if (gameIs2Player)
            {
                if (seed.Next(2) == 1)
                    p = p2;
                if (p1.Lives <= 0)
                    p = p2;
                if (p2.Lives <= 0)
                    p = p1;
            }

            MotherShip m1 = new MotherShip(canvas, 750, difficulty, (int)(2 * difficulty), Brushes.DarkViolet, Brushes.Transparent, Colors.DarkViolet, 3, new Point(canvas.ActualWidth + 40, Y), 10, p);
            Objects.Add(m1);
            canvas.Children.Add(m1.Geometry);
        }
        #endregion

        #region KEYBOARD EVENTS
        private void canvas_KeyDown(object sender, KeyEventArgs e)
        {
            if(gameStarted)
            {
                if (e.Key == Key.W)
                    p1.IsApplyingThrust = true;
                if (e.Key == Key.A)
                    p1.IsRotatingCounterClockwise = true;
                if (e.Key == Key.D)
                    p1.IsRotatingClockwise = true;
                if(gameIs2Player)
                {
                    if (e.Key == Key.NumPad8)
                        p2.IsApplyingThrust = true;
                    if (e.Key == Key.NumPad4)
                        p2.IsRotatingCounterClockwise = true;
                    if (e.Key == Key.NumPad6)
                        p2.IsRotatingClockwise = true;
                }

            }
        }

        private void canvas_KeyUp(object sender, KeyEventArgs e)
        {
            if (gameStarted)
            {
                if (e.Key == Key.W)
                {
                    p1.IsApplyingThrust = false;
                    p1.Speed = 0;
                }
                if (e.Key == Key.A)
                    p1.IsRotatingCounterClockwise = false;
                if (e.Key == Key.D)
                    p1.IsRotatingClockwise = false;
                if (e.Key == Key.Space)
                    p1.Fire();
                if (e.Key == Key.S)
                    p1.Warp();
                if(e.Key == Key.Escape)
                {
                    timer.Stop();
                    Pause_Menu.IsEnabled = true;
                    Pause_Menu.Visibility = Visibility.Visible;
                    this.Cursor = Cursors.Cross;
                    canvas.Visibility = Visibility.Hidden;
                }
            }
            if(cheatsEnabled)
            {
                if (e.Key == Key.D1)
                {
                    p1.InvincibilityTimer = 1000; //15 sec
                    p1.IsInvincible = true;
                    if (gameIs2Player)
                    {
                        p2.InvincibilityTimer = 1000;
                        p2.IsInvincible = true;
                    }
                    cheatsUsed = true;
                }
                if(e.Key == Key.D2)
                {
                    p1.Lives++;
                    if (gameIs2Player)
                        p2.Lives++;
                    cheatsUsed = true;
                }
                if(e.Key == Key.D3)
                {
                    Objects.Asteroid a = new Objects.Asteroid(this, Astroke, Afill, Aglow, 25, difficulty, 5, new Point(seed.Next(0, (int)canvas.ActualWidth), seed.Next(0, (int)canvas.ActualHeight)), "ExtraLarge", seed.Next(0, 3), seed.Next());
                    Objects.Add(a);
                    canvas.Children.Add(a.Geometry);
                    cheatsUsed = true;
                }
                if (e.Key == Key.D4)
                {
                    SpawnSmallSaucer();
                    cheatsUsed = true;
                }
                if (e.Key == Key.D5)
                {
                    SpawnMotherShip();
                    cheatsUsed = true;
                }

            }
            if(gameIs2Player)
            {
                if (e.Key == Key.NumPad8)
                {
                    p2.IsApplyingThrust = false;
                    p2.Speed = 0;
                }
                if (e.Key == Key.NumPad4)
                    p2.IsRotatingCounterClockwise = false;
                if (e.Key == Key.NumPad6)
                    p2.IsRotatingClockwise = false;
                if (e.Key == Key.Enter)
                    p2.Fire();
                if (e.Key == Key.NumPad5)
                    p2.Warp();
            }

        
        }
        #endregion



    }
}
