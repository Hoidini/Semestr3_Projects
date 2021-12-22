using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowersHan
{
    class Disk : GameObject
    {
        public const int MinR = 50;
        public Point[] PointsInfo
        {
            get => base.points;
        }
        public Point LOCATION 
        {
            get => base.points[0];
            set
            { 
                base.points[0] = value; 
                if(value.X < 0)
                {
                    base.points[0].X = 0;
                    this.SPEED = new Point(0, this.SPEED.Y);
                }
                if(value.Y < 0)
                {
                    base.points[0].Y = 0;
                    this.SPEED = new Point(this.SPEED.X, 0);
                }
                if(value.Y + this.high > 600)
                {
                    base.points[0].Y = 600 - this.high;
                    this.SPEED = new Point(this.SPEED.X, 0);
                }
                if(value.X + this.radius * 2 > 800)
                {
                    base.points[0].X = 800 - this.radius * 2;
                    this.SPEED = new Point(0, this.SPEED.Y);
                }
                this.InitPoints();

            }
        }
        Tower CurTower;
        public Tower CurrentTower
        {
            get => this.CurTower;
            set => this.CurTower = value;
        }
        public Point PreviousLocation
        {
            get => this.PrevLoc;
            set => this.PrevLoc = value;
        }
        Point PrevLoc;
        Point speed;
        public const int MinH = 30;
        public int high;
        bool IsCatched;
        public readonly int radius;
        public bool ISCATCHED
        {
            get => this.IsCatched;
            set => this.IsCatched = value;
        }
        public Point SPEED 
        {
            get => this.speed;
            set => this.speed = value;
        }
        public void ChangeSpeed(Point speed)
        {
            this.SPEED = new Point(this.speed.X + speed.X, this.speed.Y + speed.Y);
            
        }
        public void ChangeLocationUseSpeed()
        {
            this.PrevLoc = this.LOCATION;
            this.LOCATION = new Point(this.LOCATION.X + this.speed.X, this.LOCATION.Y + this.speed.Y);
            this.InitPoints();
        }
        public Disk(Action<GameObject> AddToPhys, int radius, Point location, int high) : base(AddToPhys)
        {
            if(radius < MinR)
            {
                throw new ArgumentException("Радиус не может быть меньше минимального");
            }
            if(high < MinH)
            {
                throw new ArgumentException("Высота не может быть меньше минимальной");
            }
            this.high = high;
            this.radius = radius;
            this.IsCatched = false;
            this.LOCATION = location;
            this.PrevLoc = location;
            this.CurTower = null;
            base.points[1] = new Point(location.X, location.Y + MinH);
            base.points[2] = new Point(location.X + 2 * MinR, location.Y);
            base.points[3] = new Point(location.X + 2 * MinR, location.Y + MinH);
            base.points[4] = new Point(location.X + MinR, location.Y + (MinH / 2));
        }
        public void InitPoints()
        {
            Point location = base.points[0];
            base.points[1] = new Point(location.X, location.Y + this.high);
            base.points[2] = new Point(location.X + 2 * this.radius, location.Y);
            base.points[3] = new Point(location.X + 2 * this.radius, location.Y + this.high);
            base.points[4] = new Point(location.X + this.radius, location.Y + (this.high / 2));
        }
        public bool TryCatch(Point ClickPoint)
        {
            bool result = false;
            bool IsXOverlap = (this.LOCATION.X <= ClickPoint.X && this.points[2].X >= ClickPoint.X);
            bool IsYOverlap = (this.LOCATION.Y <= ClickPoint.Y && this.points[1].Y >= ClickPoint.Y);
            if(IsXOverlap && IsYOverlap)
            {
                this.IsCatched = true;
                result = true;
            }
            return result;
        }
    }
}
