using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    class Bot//класс, ответственный за принятие решений о размещении кораблей и стрельбе
             //со стороны бота
    {
        Func<bool> IsGameGo;
        bool repeat;
        Shooting_Mode main_mode;
        Finishing_Shoot finishing;
        bool IsItMainMode;
        public Bot(Shooting_Mode mode, Form1.RectanglesField field, Func<bool> IsGameGo)
        {
            this.IsGameGo = IsGameGo;
            this.main_mode = mode;
            this.finishing = new Finishing_Shoot(field);
            this.repeat = true;
            this.IsItMainMode = true;
        }
        public void Repeat()
        {
            this.repeat = true;
        }
        public bool IsBotTurn
        {
            get => this.repeat;
        }
        public void DoStep()
        {

            if (this.IsGameGo() && this.repeat)
            {
                if (!this.IsItMainMode)
                {
                    bool IsFinishMode;
                    (this.repeat, IsFinishMode) = this.finishing.choose_location();
                    this.IsItMainMode = !IsFinishMode;
                    if (this.IsItMainMode)
                    {
                        this.main_mode.DoAdditionalActionsAfterFinalShoot(this.finishing.EXPORTVECTOR);
                    }
                }
                else 
                {
                    (bool, bool) result_of_shoot = this.main_mode.choose_location();
                    this.repeat = result_of_shoot.Item1;
                    if (result_of_shoot.Item1)
                    {
                        if (result_of_shoot.Item2)
                        {
                            this.IsItMainMode = false;
                            this.finishing.FirstPoint = this.main_mode.FirstPoint;
                            this.finishing.SecondPoint = this.main_mode.SecondPoint;
                        }
                        else
                        {
                            this.IsItMainMode = true;
                            this.main_mode.DoAdditionalActionsAfterFinalShoot(
                                new Point_Vector(this.main_mode.FirstPoint.Value,
                                this.main_mode.FirstPoint.Value));
                        }
                    }
                    //Thread.Sleep(200);
                }
            }
        }
    }
    public abstract class Shooting_Mode
    { //режим огня
        protected Form1.RectanglesField current_field;
        protected Random rand;
        public Point? FirstPoint, SecondPoint; //точки, отвечающие за текущие известные края корабля
        public abstract (bool, bool) choose_location(); //возвращаемый результат - 
        //первый параметр сообщает, было ли попадание, второй - выжил ли корабль
        public abstract void DoAdditionalActionsAfterFinalShoot(Point_Vector vect);

        /*осуществляет выстрел по заданным координатам, регистрируя точку в случае попадания,
         второй параметр показывает, жив ли корабль после выстрела, а также перекрашивает
        поле в цвет, соответствующий результату выстрела*/
        public static (Point, bool)? Shoot(int x_Pos, int y_Pos, Form1.RectanglesField current_field)
        {
            Point Fired_Position = new Point(x_Pos, y_Pos);
            Game_Rectangle cur_field = current_field[Fired_Position];
            cur_field.Is_Checked = true;
            Game_Rectangle.field_Condition_fact f_cond
                = cur_field.Field_Condition;
            string path = Directory.GetCurrentDirectory() + @"\sprites";
            cur_field.Load_Animatiom(path);
            if (f_cond == Game_Rectangle.field_Condition_fact.live)
            {
                cur_field.ship.Reduce_Count();

                if (cur_field.ship.Is_live())
                {
                    cur_field.Field_Condition = Game_Rectangle.field_Condition_fact.broken;
                    cur_field.Color = Color.Red;
                    return (new Point(x_Pos, y_Pos), true);
                }
                else
                {
                    (Point?, Point?) coord = cur_field.ship.Position;
                    if (!coord.Item1.HasValue || !coord.Item2.HasValue)
                    {
                        throw new ArgumentNullException("отсутствуют координаты корабля");
                    }
                    Point_Vector vect = new Point_Vector(coord.Item1.Value, coord.Item2.Value);
                    foreach (Point cur_p in vect)
                    {
                        current_field[cur_p].Color = Color.Black;
                    }
                    current_field.COUNT_OF_SHIPS--;
                    return (Fired_Position, false);
                }
            }
            else
            {
                cur_field.Color = Color.Yellow;
                return null;
            }
        }
        public (bool, bool) Process_Shoot(int x_Pos, int y_Pos)
        {
            (Point, bool)? info_package = Shoot(x_Pos, y_Pos, this.current_field);
            if (info_package.HasValue)
            {
                this.FirstPoint = this.SecondPoint = info_package.Value.Item1;
                if (info_package.Value.Item2)
                {

                    return (true, true);
                }
                else
                {
                    return (true, false);
                }
            }
            else
            {
                return (false, false);
            }
        }
        public Form1.RectanglesField Current_Field
        {
            get
            {
                return current_field;
            }
            set
            {
                current_field = value;
            }
        }
    }
    //класс, содержащий в себе логику случайного огня
    class Random_Shoot : Shooting_Mode
    {
        public Random_Shoot(Form1.RectanglesField field)
        {
            base.rand = new Random((int)(DateTime.Now.Ticks & 100000));
            base.Current_Field = field;
        }
        public override (bool, bool) choose_location()
        {
            int x_Pos, y_Pos;
            while (true)
            {
                x_Pos = rand.Next() % 10;
                y_Pos = rand.Next() % 10;
                if (!base.Current_Field[x_Pos, y_Pos].Is_Checked)
                {
                    return base.Process_Shoot(x_Pos, y_Pos);
                }
            }
        }

        public override void DoAdditionalActionsAfterFinalShoot(Point_Vector vect)
        {
            PointVectorAndVicinity Vic = new PointVectorAndVicinity(vect);
            Point_Vector[] vectors = Vic.GetPointVectors;
            for (int i = 0; i < 3; i++)
            {
                if (vectors[i] != null)
                {
                    foreach (Point p in vectors[i])
                    {
                        this.current_field[p].Is_Checked = true;
                    }
                }
            }
        }
    }

    //класс, хранящий в себе логику плотностного огня
    // class Density_Shoot : Shooting_Mode { }
    partial class Density_Shoot : Shooting_Mode
    {
        TreeOfAvailableToFirePoints LogicTree;
        int counter;
        public Density_Shoot(Form1.RectanglesField field)
        {
            this.counter = 0;
            this.LogicTree = new TreeOfAvailableToFirePoints();
            base.rand = new Random((int)(DateTime.Now.Ticks & 100000));
            base.Current_Field = field;
        }
        public override (bool, bool) choose_location()
        {
            Point coords;
            if ((++this.counter) % 3 == 1)
            {
                int x, y;
                do
                {
                    x = base.rand.Next() % 10; y = base.rand.Next() % 10;
                } while (this.current_field[x, y].Is_Checked || !this.LogicTree.Contains(x, y));
                this.LogicTree.Decrement_Mass(x, y);
                coords = new Point(x, y);
            }
            else
            {
                coords = this.LogicTree.Choose_Point();
            }
            return base.Process_Shoot(coords.X, coords.Y);
        }

        public override void DoAdditionalActionsAfterFinalShoot(Point_Vector vect)
        {
            PointVectorAndVicinity Vic = new PointVectorAndVicinity(vect);
            Point_Vector[] vectors = Vic.GetPointVectors;
            for (int i = 0; i < 3; i++)
            {
                if (vectors[i] != null)
                {
                    this.LogicTree.Decrement_Mass(vectors[i]);
                    foreach (Point p in vectors[i])
                    {
                        this.current_field[p].Is_Checked = true;
                    }
                }
            }
        }
    }
    //класс, хранящий в себе логику вероятностного огня
    //class Probabilistic_Shoot : Shooting_Mode { }
    partial class Probabilistic_Shoot : Shooting_Mode
    {
        TreeOfFireLogic Tree;
        public Probabilistic_Shoot(Form1.RectanglesField field)
        {
            this.Tree = new TreeOfFireLogic();
            base.rand = new Random((int)(DateTime.Now.Ticks & 100000));
            base.Current_Field = field;
        }
        public override (bool, bool) choose_location()
        {
            Point Loc = Tree.ChooseShip();
            return base.Process_Shoot(Loc.X, Loc.Y);
        }

        public override void DoAdditionalActionsAfterFinalShoot(Point_Vector vect)
        {
            PointVectorAndVicinity Vic = new PointVectorAndVicinity(vect);
            Point_Vector[] vectors = Vic.GetPointVectors;
            this.Tree.Decrement_Count_Of_Ship(vect.Length);
            for(int i = 0; i < 3; i++)
            {
                if(vectors[i] != null)
                {
                    foreach(var p in vectors[i])
                    {
                        this.Tree.ChangeSur(p);
                    }
                }
            }
        }
    }
    //класс, хранящий в себе логику финального выстрела
    class Finishing_Shoot
    {
        public Point? FirstPoint, SecondPoint;
        Point_Vector ExportVect;
        int fire_direction;
        Form1.RectanglesField current_field;
        public Finishing_Shoot(Form1.RectanglesField field)
        {
            this.current_field = field;
            fire_direction = 0;
        }
        //выбирает направление стрельбы
        void ChooseDirectionForFire(out bool repeat, out bool ship_is_live)
        {
            repeat = ship_is_live = true; // ненужное присваивание, которое будет 
            //изменено, но без него компилятор бузит
            switch (this.fire_direction)
            {
                case 0:
                    this.Search_and_fire(-1, 0, out repeat, out ship_is_live);
                    break;
                case 1:
                    this.Search_and_fire(0, -1, out repeat, out ship_is_live);
                    break;
                case 2:
                    this.Search_and_fire(1, 0, out repeat, out ship_is_live);
                    break;
                case 3:
                    this.Search_and_fire(0, 1, out repeat, out ship_is_live);
                    break;
            }
        }
        void fire_direction_logic()
        {
            if (this.FirstPoint.Value != this.SecondPoint.Value)
            {
                Point help = this.FirstPoint.Value;
                this.FirstPoint = this.SecondPoint.Value;
                this.SecondPoint = help;
            }
            switch (fire_direction)
            {
                case (0):
                    this.fire_direction = 2;
                    break;
                case (1):
                    this.fire_direction = 3;
                    break;
                case (2):
                    this.fire_direction = 1;
                    break;
                case (3):
                    this.fire_direction = 0;
                    break;
            }
        }
        void Search_and_fire(int horisont, int vertical, out bool repeat, out bool IsLive)
        {
            int x = SecondPoint.Value.X + horisont;
            int y = SecondPoint.Value.Y + vertical;
            IsLive = true;
            repeat = false;
            (Point, bool)? package;
            if ((x >= 0 && x < 10) && (y >= 0 && y < 10) && !this.current_field[x, y].Is_Checked)
            {
                package = Shooting_Mode.Shoot(x, y, this.current_field);
                if (package.HasValue)
                {
                    repeat = true;

                    this.SecondPoint = new Point(x, y);

                    if (!package.Value.Item2)
                    {
                        IsLive = false;
                        this.fire_direction = 0; //обнуляем схему выбора точки для огня,
                        //иначе в дальнейшем может произойти неоптимальный выбор при
                        //переходе со стрельбы наверх на стрельбу влево
                        this.ExportVect = new Point_Vector(this.FirstPoint.Value,
                            this.SecondPoint.Value);
                        this.FirstPoint = this.SecondPoint = null;
                    }
                }
                else
                {
                    this.fire_direction_logic();
                }
            }
            else
            {
                this.fire_direction_logic();
                this.ChooseDirectionForFire(out repeat, out IsLive);
            }
        }
        public Point_Vector EXPORTVECTOR
        {
            get => this.ExportVect;
        }


        public (bool, bool) choose_location()
        {
            this.ExportVect = null;
            this.ChooseDirectionForFire(out bool repeat, out bool ship_is_live);
            return (repeat, ship_is_live); // если корабль убит, то true
        }
    }
}
