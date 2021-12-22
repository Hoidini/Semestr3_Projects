using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1
    {
        public abstract class Active_Interface
        {

            //public abstract void resize();
        }
        class Main_Interface : Active_Interface, IDisposable //главное меню
        {
            public Label Name_Of_Game;
            readonly Controls_Deleter deleter;
            readonly Send_Active_Interface FormControl;
            public Button Start_Game;
            readonly Add_Controls AddControls;
            ListBox BoxOfLevelChoise;
            Dictionary<string, Func<RectanglesField, Shooting_Mode >> keyValuePairsOfLevelModes;
            public Main_Interface(Point Size_Of_Window, Send_Active_Interface FormControl_Event,
               Controls_Deleter controls_Deleter, Add_Controls AddControls)
            {
                //принимаем управление формой
                this.AddControls = AddControls;
                this.FormControl = FormControl_Event;
                this.deleter = controls_Deleter;
                //инициализируем элементы взаимодействия с формой и задаем их свойства
                this.Name_Of_Game = new Label
                {
                    Text = "МОРСКОЙ БОЙ",
                    Font = new Font(DefaultFont.FontFamily, 25),
                    Size = new Size(300, 100),
                    BackColor = Color.White
                };
                this.Name_Of_Game.Location = new Point(
                   (int)((Size_Of_Window.X * 0.5) - (this.Name_Of_Game.Size.Width / 2)),
                    (int)(Size_Of_Window.Y * 0.05));
                this.Start_Game = new Button();
                Loc_Res.Choose_Loc_And_Size(Start_Game,
                   0.4, 0.2, 0.4, 0.2, Size_Of_Window);
                this.Start_Game.Text = "Начать игру";
                Start_Game.MouseClick += new MouseEventHandler(this.Start_Game_button_Click);
                this.BoxOfLevelChoise = new ListBox();
                this.keyValuePairsOfLevelModes = new Dictionary<string, Func<RectanglesField, Shooting_Mode>>();
                Func<RectanglesField, Shooting_Mode> Easy = s => new Random_Shoot(s);
                Func<RectanglesField, Shooting_Mode> Medium = s => new Density_Shoot(s);
                Func<RectanglesField, Shooting_Mode> Hard = s => new Probabilistic_Shoot(s);
                Loc_Res.Choose_Loc_And_Size(this.BoxOfLevelChoise,
                   0.43, 0.15, 0.65, 0.08, Size_Of_Window);
                this.keyValuePairsOfLevelModes.Add("Легкий уровень", Easy);
                this.keyValuePairsOfLevelModes.Add("Средний уровень", Medium);
                this.keyValuePairsOfLevelModes.Add("Сложный уровень", Hard);
                this.BoxOfLevelChoise.Items.Add("Легкий уровень");
                this.BoxOfLevelChoise.Items.Add("Средний уровень");
                this.BoxOfLevelChoise.Items.Add("Сложный уровень");
            }
            /* public override void resize()
             {
             }*/
            public void Start_Game_button_Click(object sender, EventArgs e)
            {
                if(this.BoxOfLevelChoise.SelectedItem == null)
                {
                    return;
                }
                Func<RectanglesField, Shooting_Mode> curLevel;
                this.keyValuePairsOfLevelModes.TryGetValue((string)this.BoxOfLevelChoise.SelectedItem,
                    out curLevel);
                Game_Interface New_Act_Int = new Game_Interface(Size_Of_Window_Global, this.FormControl, this.deleter,
                    this.AddControls, curLevel);
                this.Dispose();
                New_Act_Int.GetControlOfForm();
            }
            public void GetControlOfForm()
            {
                this.FormControl(this, this.BoxOfLevelChoise, this.Start_Game, this.Name_Of_Game);
            }

            public void Dispose()
            {
                this.BoxOfLevelChoise.Dispose();
                this.Name_Of_Game.Dispose();
                this.Start_Game.Dispose();
            }
        }
        //интерфейс окна во время игры
        public class Game_Interface : Active_Interface
        {
            readonly Bot bot;
            readonly Label textPlayerField;
            readonly Label textBotField;
            readonly Label Instruction;
            readonly Button BackButton;
            int counter;
            bool IsGameStarted;
            readonly Spread_Bot_Random bot_place_controller;
            public Timer timer;
            event Func<bool> IsGameGo;
            readonly Controls_Deleter deleter;
            public Temp_Menu placement_menu;
            public RectanglesField PlayerField;
            public RectanglesField BotField;
            readonly Add_Controls AddControls;
            readonly Point Size_Of_Window;
            readonly Send_Active_Interface FormControl;
            public Game_Interface(Point Size_Of_Window, Send_Active_Interface FormControl_Event,
               Controls_Deleter deleter, Add_Controls AddControls, 
               Func<RectanglesField, Shooting_Mode> LevelOfHard)
            {
                //принимаем управление формой
                this.FormControl = FormControl_Event;
                this.deleter = deleter;
                this.AddControls = AddControls;
                //инициализируем элементы взаимодействия с формой и задаем их свойства
                this.timer = new Timer
                {
                    Interval = 30
                };
                this.textBotField = new Label
                {
                    Text = "Поле бота",
                    BackColor = Color.White,
                    Font = new Font(DefaultFont.FontFamily, 13),
                    Size = new Size(200, 20),
                    Location = new Point((int)(Size_Of_Window.X * 0.65), (int)(Size_Of_Window.X * 0.05)),
                };
                this.textPlayerField = new Label
                {
                    Text = "Поле игрока",
                    Font = new Font(DefaultFont.FontFamily, 13),
                    Location = new Point((int)(Size_Of_Window.X * 0.15),
                    (int)(Size_Of_Window.X * 0.05)),
                    Size = new Size(200, 20),
                    BackColor = Color.White
                };
                this.Instruction = new Label
                {
                    Text = "Поворачивать корабль при расстановке на A D\n" +
                    "Зеленый цвет - живая ячейка вашего корабля, желтый цвет - выстрел мимо " +
                    "красный цвет - ранил, черный - убил. Синий цвет - просто поле",
                    Location = new Point((int)(Size_Of_Window.X * 0.37), (int)(Size_Of_Window.X * 0.1)),
                    Font = new Font(DefaultFont.FontFamily, 9),
                    Size = new Size(200, 100),
                    BackColor = Color.White
                };
                string path = Directory.GetCurrentDirectory() + @"\Pictures\Krest.png";
                this.BackButton = new Button
                {
                    Image = (Bitmap)Image.FromFile(path),
                    Location = new Point((int)(Size_Of_Window.X * 0.03), (int)(Size_Of_Window.X * 0.02)),
                    Size = new Size(30, 30)
                };
                this.BackButton.MouseClick += delegate
                {
                    Main_Interface New_Act_Int = new Main_Interface(Size_Of_Window_Global, this.FormControl, this.deleter,
                    this.AddControls);
                    New_Act_Int.GetControlOfForm();
                };
                this.counter = 0;
                this.IsGameStarted = false;
                this.PlayerField = new RectanglesField(Size_Of_Window,
                    new Point((int)(Size_Of_Window.X * 0.1), (int)(Size_Of_Window.X * 0.1)));
                this.BotField = new RectanglesField(Size_Of_Window,
                    new Point((int)(Size_Of_Window.X * 0.6), (int)(Size_Of_Window.X * 0.1)));
                //изменяем состояние формы
                List<Ship> list_of_ships = new List<Ship>();
                for (int i = 1; i <= 4; i++)
                {
                    for (int j = 1; j <= 5 - i; j++)
                    {
                        list_of_ships.Add(new Ship(i));
                    }
                }
                this.IsGameGo += delegate
                {
                    if (this.PlayerField.COUNT_OF_SHIPS != 0 &&
                    this.BotField.COUNT_OF_SHIPS != 0 && this.IsGameStarted)
                    {
                        return true;
                    }
                    return false;
                };
                this.bot = new Bot(LevelOfHard(this.PlayerField), this.PlayerField,
                    this.IsGameGo);
                this.Size_Of_Window = Size_Of_Window;
                this.bot_place_controller = new Spread_Bot_Random(BotField, list_of_ships);
                this.bot_place_controller.complete_placement();
                this.placement_menu = new Temp_Menu(this.PlayerField);
                this.placement_menu.Placement_Draw += delegate ((Point?, Point?) points,
                    bool IsOk) // рисование
                //при расстановке
                {
                    this.PlayerField.TEMP_PLACEMENT_POINTS = (points.Item1, points.Item2,
                    IsOk);
                };
                this.placement_menu.ships_box.KeyDown += this.TurnOffListBoxIneractionWithKeyPad;
                void MouseMoveTemp(object sender,
                     MouseEventArgs e)
                {
                    Point Size = this.PlayerField.Size_of_field;
                    Point start_position_to_draw = new Point(e.Location.X / Size.X,
                    e.Location.Y / Size.Y);
                    if (start_position_to_draw.X == 10)
                    {
                        start_position_to_draw.X--;
                    }
                    if (start_position_to_draw.Y == 10)
                    {
                        start_position_to_draw.Y--;
                    }
                    this.placement_menu.Cur_Position = start_position_to_draw;
                    this.PlayerField.GetPictureBox.Focus();
                }
                this.PlayerField.GetPictureBox.PreviewKeyDown += this.KeyClickToTurnShip;
                this.PlayerField.GetPictureBox.MouseMove += MouseMoveTemp;
                void MouseLeaveTemp(object sender, EventArgs e)
                {
                    this.PlayerField.TEMP_PLACEMENT_POINTS = (null, null, false);
                }
                this.PlayerField.GetPictureBox.MouseLeave += MouseLeaveTemp;
                void MouseClickTemp(object sender,
                   MouseEventArgs e)
                {
                    (Point?, Point?, Point?) points = this.placement_menu.GetPointsToDraw();
                    if (this.placement_menu.current != null &&
                    this.placement_menu.control.Check_Location(points.Item1, points.Item2))
                    {
                        this.placement_menu.current.Position = (points.Item1, points.Item2);
                        Point_Vector iteration_vect = new Point_Vector(points.Item1.Value,
                            points.Item2.Value);
                        foreach (Point cur in iteration_vect)
                        {
                            this.PlayerField[cur].Color = Color.Green;
                            this.PlayerField[cur].Field_Condition = Game_Rectangle.field_Condition_fact.live;
                            this.PlayerField[cur].ship = this.placement_menu.current;
                        }
                        this.PlayerField.TEMP_PLACEMENT_POINTS = (null, null, false);
                        this.placement_menu.current = null;
                        //this.PlayerField.COUNT_OF_SHIPS++;
                    }
                }
                this.PlayerField.GetPictureBox.MouseClick += MouseClickTemp;
                this.placement_menu.OK_Button.MouseClick += delegate (object sender,
                    MouseEventArgs e)
                {
                    if (this.placement_menu.ships_box.Items.Count == 0 &&
                    this.placement_menu.current == null)
                    {
                        PictureBox picture = this.PlayerField.GetPictureBox;
                        picture.MouseClick -= MouseClickTemp;
                        picture.MouseMove -= MouseMoveTemp;
                        picture.MouseLeave -= MouseLeaveTemp;
                        this.placement_menu.Dispose();
                        this.deleter(this.placement_menu.OK_Button,
                            this.placement_menu.ships_box);
                        this.placement_menu = null;
                        this.BotField.GetPictureBox.MouseClick += delegate (object sender2,
                            MouseEventArgs e2)
                        {
                            if (this.IsGameGo() && !this.bot.IsBotTurn)
                            {
                                int X = (int)(e2.X / this.BotField.Size_of_field.X);
                                int Y = (int)(e2.Y / this.BotField.Size_of_field.Y);
                                X = (X == 10 ? 9 : X);
                                Y = (Y == 10 ? 9 : Y);
                                if (!this.BotField[X, Y].Is_Checked)
                                {
                                    (Point, bool)? result_of_shoot =
                                    Shooting_Mode.Shoot(X, Y, this.BotField);
                                    if (!result_of_shoot.HasValue)
                                    {
                                        this.bot.Repeat();
                                    }
                                }
                                this.TryGameToEnd();
                            }
                        };
                        this.IsGameStarted = true;
                    }
                };
                this.timer.Tick += delegate (object sender, EventArgs e)
                {
                    if ((this.counter++) % 10 == 1)
                    {
                        this.bot.DoStep();
                    }
                    this.BotField.Fill_Color();
                    this.PlayerField.Fill_Color();
                    this.BotField.Render();
                    this.PlayerField.Render();
                    this.TryGameToEnd();
                };
                this.timer.Enabled = true;
            }
            void KeyClickToTurnShip(object sender, PreviewKeyDownEventArgs e)
            {
                if(this.placement_menu.current != null)
                {
                    int dir;
                    if (e.KeyCode == Keys.A)
                    {
                        dir = (((int)this.placement_menu.cur_dir) + 1) % 4;
                        this.placement_menu.cur_dir = (Temp_Menu.Vect)dir;
                    }
                    else if (e.KeyCode == Keys.D)
                    {
                        dir = ((int)this.placement_menu.cur_dir) - 1;
                        dir = (dir < 0 ? 3 : dir);
                        this.placement_menu.cur_dir = (Temp_Menu.Vect)dir;
                    }
                    this.placement_menu.UpdatePosition();
                }
            }
            void TurnOffListBoxIneractionWithKeyPad(object sender, KeyEventArgs e)
            {
                e.Handled = true;
            }
            public void GetControlOfForm()
            {
                this.FormControl(this,
                this.BotField.GetPictureBox, this.PlayerField.GetPictureBox,
                this.placement_menu.OK_Button, this.placement_menu.ships_box,
                this.textBotField, this.textPlayerField, this.BackButton, this.Instruction);
            }
            public void TryGameToEnd()
            {

                if (!this.IsGameGo() && this.IsGameStarted)
                {
                    this.timer.Enabled = false;
                    this.BotField.Dispose();
                    this.PlayerField.Dispose();
                    this.BackButton.Dispose();
                    this.textBotField.Dispose();
                    this.textPlayerField.Dispose();
                    this.Instruction.Dispose();
                    this.deleter(this.PlayerField.GetPictureBox, this.textBotField, this.Instruction,
                          this.BotField.GetPictureBox, this.textPlayerField, this.BackButton);
                    Label text_end_game = new Label();
                    if (this.BotField.COUNT_OF_SHIPS == 0)
                    {
                        text_end_game.Text = "Вы победили!";
                    }
                    else
                    {
                        text_end_game.Text = "Вы проиграли...";
                    }
                    text_end_game.Location = new Point((int)(Size_Of_Window.X * 0.5),
                        (int)(Size_Of_Window.Y * 0.3));
                    text_end_game.BackColor = Color.White;
                    Button EndGameButton = new Button
                    {
                        Location = new Point((int)(Size_Of_Window.X * 0.5),
                        (int)(Size_Of_Window.Y * 0.5)),
                        Text = "ОК",
                    };
                    this.AddControls(EndGameButton, text_end_game);
                    EndGameButton.MouseClick += delegate (object sender3,
                        MouseEventArgs e3)
                    {
                        Main_Interface main_interface = new Main_Interface(Size_Of_Window_Global, this.FormControl,
                            this.deleter, this.AddControls);
                        this.deleter(EndGameButton, text_end_game);
                        EndGameButton.Dispose();
                        text_end_game.Dispose();
                        main_interface.GetControlOfForm();
                    };
                    
                }
            }
            // внутренний класс, содержащий в себе элементы управления, позволяющие
            //игроку расставить свои корабли
            public class Temp_Menu : IDisposable
            {
                bool ev_is_active; // переменная, указывающие, что событие листбокса активно
                public enum Vect { up, left, down, right };
                public Spread_Player_Control control;
                Point cur_position;
                RectanglesField gamefield;
                public Button OK_Button;
                public ListBox ships_box;
                public Ship current;
                public Vect cur_dir;
                public event Action<(Point?, Point?), bool> Placement_Draw;
                public Point Cur_Position
                {
                    get => this.cur_position;
                    set
                    {
                        if (this.current != null)
                        {
                            this.cur_position = value;
                            this.UpdatePosition();
                        }

                    }
                }
                //обновляет расстановку корабля. 
                public void UpdatePosition()
                {
                    (Point?, Point?, Point?) temp_point_cont = this.GetPointsToDraw();
                    this.Placement_Draw((temp_point_cont.Item1, temp_point_cont.Item3),
                        this.control.Check_Location(temp_point_cont.Item1,
                        temp_point_cont.Item2));
                }
                //возвращает первую точку, вторую точку без учета границ и вторую
                //точку с учетом границ - 3 параметр
                public (Point?, Point?, Point?) GetPointsToDraw()
                {
                    Point? third = null;
                    Point? second = null;
                    if (this.cur_position != null && this.current != null)
                    {
                        int temp_point_charact;
                        switch (cur_dir)
                        {
                            case Temp_Menu.Vect.up:
                                temp_point_charact = cur_position.Y - current.Lenght + 1;
                                second = new Point(cur_position.X, temp_point_charact);
                                third = new Point(cur_position.X,
                                    (temp_point_charact >= 0 ? temp_point_charact : 0));
                                break;
                            case Temp_Menu.Vect.down:
                                temp_point_charact = cur_position.Y + current.Lenght - 1;
                                second = new Point(cur_position.X, temp_point_charact);
                                third = new Point(cur_position.X,
                                    (temp_point_charact < 10 ? temp_point_charact : 9));
                                break;
                            case Temp_Menu.Vect.left:
                                temp_point_charact = cur_position.X - current.Lenght + 1;
                                second = new Point(temp_point_charact, cur_position.Y);
                                third = new Point((temp_point_charact >= 0 ?
                                    temp_point_charact : 0), cur_position.Y);
                                break;
                            case Temp_Menu.Vect.right:
                                temp_point_charact = cur_position.X + current.Lenght - 1;
                                second = new Point(temp_point_charact, cur_position.Y);
                                third = new Point((temp_point_charact < 10 ?
                                    temp_point_charact : 9), cur_position.Y);
                                break;
                        }
                    }
                    return (this.cur_position, second, third);
                }

                public void Dispose()
                {
                    this.OK_Button.Dispose();
                    this.ships_box.Dispose();
                }

                public Temp_Menu(RectanglesField field)
                {
                    this.ev_is_active = false;
                    this.cur_dir = Vect.up;
                    this.gamefield = field;
                    this.control = new Spread_Player_Control(this.gamefield);
                    this.OK_Button = new Button();
                    this.ships_box = new ListBox();
                    this.OK_Button.Location = new Point(525, 475);
                    this.OK_Button.Size = new Size(100, 50);
                    this.ships_box.Location = new Point(475, 300);
                    this.ships_box.Size = new Size(200, 150);
                    this.OK_Button.Text = "Расставить";
                    for (int i = 1; i <= 4; i++)
                    {
                        for (int j = 1; j <= 5 - i; j++)
                        {
                            field.COUNT_OF_SHIPS++;
                            this.ships_box.Items.Add(new Ship(i));
                        }
                    }
                    this.ships_box.SelectedIndexChanged += delegate (object sender, EventArgs e)
                    {
                        if (this.ships_box.SelectedItem is Ship s)
                        {
                            if (s != null)
                            {
                                if (this.current != null && !this.ev_is_active)
                                {
                                    this.ships_box.Items.Add(this.current.Clone());
                                    this.current = null;
                                }
                                this.ev_is_active = true;
                                this.current = (Ship)(this.ships_box.SelectedItem as Ship).Clone();
                                this.ships_box.Items.Remove(this.ships_box.SelectedItem);
                                this.ships_box.SelectedItem = null;
                                this.control.CURRENT_SHIP = current;
                                this.ev_is_active = false;
                            }
                        }
                    };
                   
                }
            }

        }

    }
}
