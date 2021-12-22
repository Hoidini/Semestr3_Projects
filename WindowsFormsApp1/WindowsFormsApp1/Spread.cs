using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class Spread_Controller
    {
        Form1.RectanglesField field_to_control;
        public Spread_Controller(Form1.RectanglesField field)
        {
            this.field_to_control = field;
        }
        //метод, отвечающий за проверку полей на возможность установки корабля
        public bool Control_Location(Point p1, Point p2)
        {
            int dx = (p2.X - p1.X) != 0 ? ((p2.X - p1.X) > 0 ? 1 : -1) : 0;
            int dy = (p2.Y - p1.Y) != 0 ? ((p2.Y - p1.Y) > 0 ? 1 : -1) : 0;
            if (p2.X < 0 || p2.X > 9 || p2.Y < 0 || p2.Y > 9)
            {
                return false;
            }
            Point cur = p1;
            while (true)
            {
                int i = cur.X - 1;
                while (true)
                {
                    if (i >= 0 && i < 10)
                    {
                        int j = cur.Y - 1;
                        while (true)
                        {
                            if (j >= 0 && j < 10)
                            {
                                if (this.field_to_control[i, j].Field_Condition !=
                                    Game_Rectangle.field_Condition_fact.empty)
                                {
                                    return false;
                                }
                            }
                            if (j != cur.Y + 1)
                            {
                                j += 1;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    if (i != cur.X + 1)
                    {
                        i += 1;
                    }
                    else
                    {
                        break;
                    }
                }
                if (cur != p2)
                {
                    cur.X += dx;
                    cur.Y += dy;
                }
                else
                {
                    return true;
                }
            }
        }
    }
    //родительский класс расстановщика кораблей
    public abstract class Spread_Mode
    {
        protected Form1.RectanglesField field_to_control;
        protected Spread_Controller controller;
    }
    //класс отвечающий за рандомную расстановку кораблей ботом
    class Spread_Bot_Random : Spread_Mode
    {
        Random randomiser;
        List<Ship> ships;
        public Spread_Bot_Random(Form1.RectanglesField rectanglesField,
            List<Ship> ships)
        {
            this.ships = ships;
            this.randomiser = new Random((int)DateTime.Now.Ticks);
            base.field_to_control = rectanglesField;
            base.controller = new Spread_Controller(this.field_to_control);
        }
        public void complete_placement()
        {
            if (this.ships == null)
            {
                throw new ArgumentNullException("А кораблей то нет!!!!");
            }
            while (this.ships.Count != 0)
            {
                Ship cur_s = this.ships.First<Ship>();
                int len = cur_s.Lenght - 1;
                this.ships.RemoveAt(0);
                bool repeat = true;
                while (repeat)
                {
                    repeat = false;
                    int x1 = this.randomiser.Next() % 10;
                    int y1 = this.randomiser.Next() % 10;
                    int x2, y2;
                    x2 = x1;
                    y2 = y1;
                    int direction = this.randomiser.Next() % 4;
                    switch (direction)
                    {
                        case 0:
                            x2 = x1 + len;
                            break;
                        case 1:
                            x2 = x1 - len;
                            break;
                        case 2:
                            y2 = y1 - len;
                            break;
                        case 3:
                            y2 = y1 + len;
                            break;
                    }
                    Point p1, p2;
                    p1 = new Point(x1, y1);
                    p2 = new Point(x2, y2);
                    if ((x1 < 0 || x1 > 9) || (x2 < 0 || x2 > 9) ||
                        (y1 < 0 || y1 > 9) || (y2 < 0 || y2 > 9)
                        || !base.controller.Control_Location(p1, p2))
                    {
                        repeat = true;
                        continue;
                    }
                    Point_Vector vector_p = new Point_Vector(p1, p2);
                    cur_s.Position = (p1, p2);
                    this.field_to_control.COUNT_OF_SHIPS++;
                    foreach (Point cur_p in vector_p)
                    {
                        base.field_to_control[cur_p].Field_Condition =
                            Game_Rectangle.field_Condition_fact.live;
                        base.field_to_control[cur_p].ship = cur_s;
                    }
                }
            }
        }
    }
    // класс, отвечающий за взаимодействие пользователя со своим полем во время
    // расстановки кораблей
    public class Spread_Player_Control : Spread_Mode
    {
        Ship current_ship;
        public bool Check_Location(Point? p1, Point? p2)
        {
            if (!p1.HasValue || !p2.HasValue)
            {
                throw new ArgumentNullException("А что проверять то?");
            }
            return base.controller.Control_Location(p1.Value, p2.Value);
        }
        public Spread_Player_Control(Form1.RectanglesField field)
        {
            base.field_to_control = field;
            base.controller = new Spread_Controller(this.field_to_control);
        }
        public Ship CURRENT_SHIP
        {
            get => this.current_ship;
            set => this.current_ship = value;
        }
    }
}
