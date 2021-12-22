using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1
    {
        public class RectanglesField : IDisposable//игровое поле
        {
            (Point?, Point?, Point_Vector, bool) temp_placement_points;
            readonly PictureBox Picture_box;
            readonly Graphics graphics;
            readonly BufferedGraphicsContext currentContext;
            readonly BufferedGraphics myBuffer;
            readonly Game_Rectangle[,] rectangles;
            int count_of_ships;
            Point size_of_field;
            public Point Size_of_field
            {
                get => this.size_of_field;
            }
            Point position;
            public int COUNT_OF_SHIPS
            {
                get => this.count_of_ships;
                set => this.count_of_ships = value;
            }
            public RectanglesField(Point Size_OF_Window, Point pos)
            {
                this.size_of_field = new Point((int)(Size_OF_Window.X * 0.02),
                   (int)(Size_OF_Window.X * 0.02));
                this.count_of_ships = 0;
                this.Picture_box = new PictureBox
                {
                    Size = new Size(this.size_of_field.X * 10,
                    this.size_of_field.Y * 10)
                };
                //this.Picture_box.BackColor = Color.Aquamarine;
                Picture_box.Location = pos;
                currentContext = BufferedGraphicsManager.Current;
                myBuffer = currentContext.Allocate(this.Picture_box.CreateGraphics(),
                       this.Picture_box.DisplayRectangle);
                graphics = myBuffer.Graphics;
                rectangles = new Game_Rectangle[10, 10];
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        this.rectangles[i, j] = new Game_Rectangle();
                    }
                }
                Position = pos;
                this.Picture_box.Show();
            }
            public (Point?, Point?, bool) TEMP_PLACEMENT_POINTS
            {
                set
                {
                    (this.temp_placement_points.Item1, this.temp_placement_points.Item2)
                        = (value.Item1, value.Item2);
                    this.temp_placement_points.Item4 = value.Item3;
                    if (this.temp_placement_points.Item1.HasValue)
                    {
                        this.temp_placement_points.Item3 = new Point_Vector(
                            this.temp_placement_points.Item1.Value, this.temp_placement_points.Item2.Value);
                    }
                    else
                    {
                        this.temp_placement_points.Item3 = null;
                    }
                }
                get => (this.temp_placement_points.Item1, this.temp_placement_points.Item2,
                    this.temp_placement_points.Item4);
            }
            // данный метод проверяет, подлежит ли поле покраске в фиолетовый из-за размещения
            public bool Check_color_to_draw(Point pos)
            {
                bool result = false;
                if (this.temp_placement_points.Item3 != null)
                {
                    result = this.temp_placement_points.Item3.Check(pos);
                }
                return result;
            }
            //метод, отвечающий за раскраску клеток
            public void Fill_Color()
            {
                Point Size = this.Size_of_field;
                Point Loc;
                SolidBrush colorBrush;
                Pen colorPen = new Pen(Color.Black);
                Rectangle help_rect;
                this.graphics.Clear(Color.Blue);
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        Bitmap picture = null;
                        Loc = new Point(i * Size.X,
                             j * Size.Y);
                        help_rect = new Rectangle(Loc, (Size)Size);
                        if (this.Check_color_to_draw(new Point(i, j)))
                        {
                            if (this.temp_placement_points.Item4)
                            {
                                colorBrush = new SolidBrush(Color.Purple);
                            }
                            else
                            {
                                colorBrush = new SolidBrush(Color.Red);
                            }
                        }
                        else
                        {
                            colorBrush = new SolidBrush(this.rectangles[i, j].Color);
                            picture = this[i, j].Animation.IteratePictures();

                        }
                        this.graphics.FillRectangle(colorBrush, help_rect);
                        this.graphics.DrawRectangle(colorPen, help_rect);
                        if (picture != null)
                        {
                            this.graphics.DrawImage(picture, Loc.X, Loc.Y, Size.X,
                                Size.Y);
                        }
                    }
                }
            }
            public void Render()
            {
                this.myBuffer.Render();
            }

            public void Dispose()
            {
                this.currentContext.Dispose();
                this.graphics.Dispose();
                this.myBuffer.Dispose();
                this.Picture_box.Dispose();
            }

            public Game_Rectangle[,] Rectangles
            {
                get => this.rectangles;
            }
            public PictureBox GetPictureBox
            {
                get => this.Picture_box;
            }
            public Game_Rectangle this[int x_Pos, int y_Pos]
            {
                get
                {
                    if (x_Pos < 0 || x_Pos > 9 || y_Pos < 0 || y_Pos > 9)
                    {
                        throw new ArgumentException("Цифры, Мэйсон! Что они значат?");
                    }
                    return rectangles[x_Pos, y_Pos];
                }
                set
                {
                    if (x_Pos < 0 || x_Pos > 9 || y_Pos < 0 || y_Pos > 9)
                    {
                        throw new ArgumentException("Цифры, Мэйсон! Что они значат?");
                    }
                    this.rectangles[x_Pos, y_Pos] = value;
                }
            }
            public Game_Rectangle this[Point point]
            {
                get
                {
                    try
                    {
                        return this[point.X, point.Y];
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }

                set
                {
                    try
                    {
                        this[point.X, point.Y] = value;
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
            public Point Position
            {
                get
                {
                    return position;
                }
                set
                {
                    position = value;
                }
            }
        }
    }
    public class Game_Rectangle
    {
        Color color;
        public Ship ship;
        public bool Is_Checked;
        Animation animation;
        public enum field_Condition_fact { empty, live, broken, dead }
        field_Condition_fact Cond;
        public Animation Animation
        {
            get => this.animation;
        }
        public Game_Rectangle()
        {
            this.animation = new Animation();
            this.color = Color.Blue;
            this.Is_Checked = false;
            this.Cond = field_Condition_fact.empty;
            this.ship = null;
        }
        public void Load_Animatiom(string path)
        {
            this.animation.LoadSprites(path);
        }
        public Color Color
        {
            get => this.color;
            set => this.color = value;
        }
        public field_Condition_fact Field_Condition
        {
            get
            {
                return Cond;
            }
            set
            {
                Cond = value;
            }
        }
    }
    /*Класс корабля, отвечает за определение его состояния*/
    public class Ship : ICloneable
    {
        public readonly string text_data;
        readonly int lenght;
        int live_sections_count;
        Point? StartPoint, EndPoint;
        public Ship(int lenght, Point? StartPoint, Point? EndPoint)
        {
            this.lenght = lenght;
            this.live_sections_count = this.lenght;
            this.StartPoint = StartPoint;
            this.EndPoint = EndPoint;
            this.text_data = "Корабль на " + (this.lenght) + " ячеек";
        }
        public Ship(int lenght) : this(lenght, null, null) { }
        public override string ToString()
        {
            return this.text_data;
        }
        public object Clone()
        {
            return new Ship(lenght, StartPoint, EndPoint);
        }
        public (Point?, Point?) Position
        {
            get => (this.StartPoint, this.EndPoint);
            set
            {
                if (value.Item1.HasValue)
                {
                    this.StartPoint = value.Item1.Value;
                }
                else
                {
                    this.StartPoint = null;
                }
                if (value.Item2.HasValue)
                {
                    this.EndPoint = value.Item2.Value;
                }
                else
                {
                    this.EndPoint = null;
                }
            }

        }
        public int Count
        {
            get
            {
                return live_sections_count;
            }
        }
        public void Reduce_Count()
        {
            if (live_sections_count > 0)
            {
                live_sections_count--;
            }
        }
        public bool Is_live()
        {
            return (this.live_sections_count != 0);
        }
        public int Lenght
        {
            get
            {
                return lenght;
            }
        }
    }
    //вспомогательный класс, осуществляющий проход по точкам, лежащим между двумя заданными
    //(данные точки должны находится на одной прямой) (помогает работать с клетками поля)
    public class Point_Vector : IEnumerable<Point>
    {
        Point p1;
        Point p2;
        readonly int dx;
        readonly int dy;
        public (Point, Point) Deconstructor
        {
            get => (this.p1, this.p2);
        }
        public Point_Vector(Point p1, Point p2)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.dx = (p2.X - p1.X) != 0 ? ((p2.X - p1.X) > 0 ? 1 : -1) : 0;
            this.dy = (p2.Y - p1.Y) != 0 ? ((p2.Y - p1.Y) > 0 ? 1 : -1) : 0;
            if (dx != 0 && dy != 0)
            {
                throw new ArgumentException("Точки должны находится на одной прямой!");
            }
        }
        public int Length
        {
            get
            {
                
                if(this.dx != 0)
                {
                    if(dx > 0)
                    {
                        return this.p2.X - this.p1.X + 1;
                    }
                    else
                    {
                        return this.p1.X - this.p2.X + 1;
                    }
                }
                else if(this.dy != 0)
                {
                    if(dy > 0)
                    {
                        return this.p2.Y - this.p1.Y + 1;
                    }
                    else
                    {
                        return this.p1.Y - this.p2.Y + 1;
                    }
                }
                else
                {
                    return 1;
                }
            }
        }
        public IEnumerator<Point> GetEnumerator()
        {
            Point cur_point = this.p1;
            Point result_point;
            bool repeat = true;
            while (repeat)
            {
                result_point = cur_point;
                if (cur_point == this.p2)
                {
                    repeat = false;
                }
                else
                {
                    cur_point.X += dx;
                    cur_point.Y += dy;
                }
                yield return result_point;
            }
        }
        public bool Check(Point p) //метод, осуществляющий проверку на принадлежность точки вектору
        {
            bool result;
            if (this.dx != 0)
            {
                if (this.dx > 0)
                {
                    result = (p.X >= this.p1.X && p.X <= this.p2.X && p.Y == this.p1.Y);
                }
                else
                {
                    result = (p.X <= this.p1.X && p.X >= this.p2.X && p.Y == this.p1.Y);
                }
            }
            else if (this.dy != 0)
            {
                if (this.dy > 0)
                {
                    result = (p.Y >= this.p1.Y && p.Y <= this.p2.Y && p.X == this.p1.X);
                }
                else
                {
                    result = (p.Y <= this.p1.Y && p.Y >= this.p2.Y && p.X == this.p1.X);
                }
            }
            else
            {
                result = this.p1 == p;
            }
            return result;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    //класс содержащий в себе не только вектор, но и окрестные точки
    class PointVectorAndVicinity
    {
        readonly Point_Vector[] vectors;
        public Point_Vector[] GetPointVectors
        {
            get => this.vectors;
        }
        public PointVectorAndVicinity(Point_Vector vect) : this(vect.Deconstructor.Item1,
            vect.Deconstructor.Item2)
        { }
        public PointVectorAndVicinity(Point p1, Point p2)
        {

            this.vectors = new Point_Vector[3];
            Point P1 = p1;
            Point P2 = p2;
            int dx = (p2.X - p1.X) != 0 ? ((p2.X - p1.X) > 0 ? 1 : -1) : 0;
            int dy = (p2.Y - p1.Y) != 0 ? ((p2.Y - p1.Y) > 0 ? 1 : -1) : 0;
            if (dx != 0 && dy != 0)
            {
                throw new ArgumentException("Точки должны находится на одной прямой!");
            }
            if (dx == 0 && dy == 0)
            {
                dx = 1;
            }
            if (dx != 0)
            {
                this.vectors[0] = this.InitXVect(dx, ref P1, ref P2);
                int NewY1 = P1.Y - 1, NewY2 = P1.Y + 1;
                this.vectors[1] = this.InitVicinityX(P1, P2, NewY1);
                this.vectors[2] = this.InitVicinityX(P1, P2, NewY2);
            }
            else
            {
                this.vectors[0] = this.InitYVect(dy, ref P1, ref P2);
                int NewX1 = P1.X - 1, NewX2 = P1.X + 1;
                this.vectors[1] = this.InitVicinityY(P1, P2, NewX1);
                this.vectors[2] = this.InitVicinityY(P1, P2, NewX2);
            }
        }
        Point_Vector InitVicinityX(Point P1, Point P2, int NewY)
        {
            if (NewY >= 0 && NewY < 10)
            {
                Point P1New = new Point(P1.X, NewY);
                Point P2New = new Point(P2.X, NewY);
                return new Point_Vector(P1New, P2New);
            }
            else
            {
                return null;
            }
        }
        Point_Vector InitXVect(int dx, ref Point P1, ref Point P2)
        {
            if (dx != 1 && dx != -1)
            {
                throw new ArgumentException("В данном методе dx должен быть равен 1 или -1");
            }
            int NewX2 = P2.X + dx;
            int NewX1 = P1.X - dx;
            if (NewX2 >= 0 && NewX2 < 10)
            {
                P2 = new Point(NewX2, P2.Y);
            }
            if (NewX1 >= 0 && NewX1 < 10)
            {
                P1 = new Point(NewX1, P1.Y);
            }
            return new Point_Vector(P1, P2);
        }
        Point_Vector InitVicinityY(Point P1, Point P2, int NewX)
        {
            if (NewX >= 0 && NewX < 10)
            {
                Point P1New = new Point(NewX, P1.Y);
                Point P2New = new Point(NewX, P2.Y);
                return new Point_Vector(P1New, P2New);
            }
            else
            {
                return null;
            }
        }
        Point_Vector InitYVect(int dy, ref Point P1, ref Point P2)
        {
            if (dy != 1 && dy != -1)
            {
                throw new ArgumentException("В данном методе dy должен быть равен 1 или -1");
            }
            int NewY2 = P2.Y + dy;
            int NewY1 = P1.Y - dy;
            if (NewY2 >= 0 && NewY2 < 10)
            {
                P2 = new Point(P2.X, NewY2);
            }
            if (NewY1 >= 0 && NewY1 < 10)
            {
                P1 = new Point(P1.X, NewY1);
            }
            return new Point_Vector(P1, P2);
        }
    }
}
