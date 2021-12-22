using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowersHan
{
    class Physics
    {
        bool isbotworking;
        public bool IsBotWorking
        {
            get => this.isbotworking;
            set => this.isbotworking = value;
        }

        int countofdisks;
        public int CountOfDisks
        {
            get => this.countofdisks;
            set => this.countofdisks = value;
        }
        public const int g = 1;
        readonly List<Disk> disks;
        readonly List<Tower> towers;
        readonly public Action<GameObject> AddToListOfObjects;
        public Physics()
        {
            this.IsBotWorking = false;
            this.disks = new List<Disk>();
            this.towers = new List<Tower>();
            this.AddToListOfObjects = (s) =>
            {
                if (s is Tower t)
                {
                    this.towers.Add(t);
                }
                else if (s is Disk d)
                {
                    this.disks.Add(d);
                }
                else
                {
                    throw new ArgumentException("Не существует такого игрового объекта");
                }
            };
        }
        public void DoStep(Point? LocOfCatchedDisk)
        {
            int curcount = 0;
            for(int i = 0; i < 3; i++)
            {
                curcount += this.towers[i].DisksCount;
            }
            //List<Point> NewPoints = new List<Point>();
            foreach (Disk disk in disks)
            {
                if (disk.ISCATCHED)
                {
                    if (!LocOfCatchedDisk.HasValue || (curcount != this.countofdisks && disk.CurrentTower != null && !this.IsBotWorking))
                    {
                        disk.ISCATCHED = false;
                        disk.SPEED = new Point(0, 0);
                    }
                    else
                    {
                        disk.LOCATION = LocOfCatchedDisk.Value;
                        disk.SPEED = new Point(LocOfCatchedDisk.Value.X - disk.PreviousLocation.X,
                            LocOfCatchedDisk.Value.Y - disk.PreviousLocation.Y);
                        if(disk.PreviousLocation.X + disk.radius * 2 + disk.SPEED.X > 800)
                        {
                            disk.SPEED = new Point(0, LocOfCatchedDisk.Value.Y - disk.PreviousLocation.Y);
                        }
                        disk.InitPoints();
                    }
                }
                if (!disk.ISCATCHED)
                {
                    disk.ChangeSpeed(new Point(0, g));
                    disk.ChangeLocationUseSpeed();
                }
            }
            double[] ParamTime = new double[this.disks.Count];
            for (int i = 0; i < ParamTime.Length; i++)
            {
                ParamTime[i] = double.MaxValue;
            }
            for (int i = 0; i < this.disks.Count; i++)
            {
                Disk disk1 = this.disks[i];
                for (int j = i + 1; j < this.disks.Count; j++)
                {

                    Disk disk2 = this.disks[j];
                    double? CollideParams = Physics.CollideParams(disk1, disk2);
                    if (CollideParams.HasValue)
                    {
                        InitNewCoord(ParamTime, disk1, i, CollideParams.Value);
                        InitNewCoord(ParamTime, disk2, j, CollideParams.Value);
                    }
                }
                for (int j = 0; j < this.towers.Count; j++)
                {
                    Tower tower = this.towers[j];
                    if (tower.Contains(disk1))
                    {
                        disk1.SPEED = new Point(0, disk1.SPEED.Y);
                        disk1.LOCATION = new Point(tower.LOCATION.X + Tower.radius - disk1.radius, disk1.LOCATION.Y);
                        disk1.InitPoints();
                        if(disk1.LOCATION.Y + disk1.high < tower.LOCATION.Y)
                        {
                            disk1.CurrentTower = null;
                            disk1.LOCATION = new Point(disk1.LOCATION.X, disk1.LOCATION.Y - 10);
                            tower.TryDelDisk();
                        }

                    }
                    else if(disk1.CurrentTower == null)
                    {
                        double? CollideParams = Physics.CollideParams(disk1, tower);
                        if (CollideParams.HasValue)
                        {
                            Point speed = disk1.SPEED;
                            int locY = disk1.LOCATION.Y;
                            InitNewCoord(ParamTime, disk1, i, CollideParams.Value);
                            if (disk1.PreviousLocation.Y + disk1.high >= tower.LOCATION.Y &&
                                disk1.LOCATION.Y + disk1.high <= tower.LOCATION.Y && 
                                disk1.PointsInfo[4].X >= tower.LOCATION.X + Tower.radius/2 && disk1.PointsInfo[4].X <= tower.LOCATION.X + Tower.radius * 3 /2
                                && tower.TryAddDisk(disk1))
                            {
                                disk1.CurrentTower = tower;
                                disk1.SPEED = new Point(disk1.SPEED.X, speed.Y);
                                disk1.LOCATION = new Point(tower.LOCATION.X + Tower.radius - disk1.radius,
                                    locY);
                                disk1.InitPoints();
                            }
                        }
                    }
                }
                disk1.PreviousLocation = disk1.LOCATION;
            }

            void InitNewCoord(double[] paramTime, Disk disk1, int index1, double param)
            {
                //check отвечает за то, что объекты вообще могут столкнуться при дальнейшем движении
                if (paramTime[index1] > param)
                {
                    paramTime[index1] = param;
                    disk1.LOCATION = new Point(disk1.PreviousLocation.X + (int)(disk1.SPEED.X * param),
                        disk1.PreviousLocation.Y + (int)(disk1.SPEED.Y * param));
                    if (param == 0)
                    {
                        disk1.SPEED = new Point(0, 0);
                    }
                    disk1.InitPoints();
                }
            }
        }

        // первый индекс параметра отвечает за координату поверхности, которые столкнуться первыми, 
        // вторая за коллизийные поверхности.
        static double? FindCoordinateParams(int x11, int x12, int x21, int x22, int u1, int u2)
        {
            double? result = null;
            if (Physics.CheckOverlap(x11, x12, x21, x22))
            {
                result = 0;
            }
            else if (u2 - u1 != 0)
            {
                result = (double)(x11 - x21) / (u2 - u1);
                result = (result >= 0 && result <= 1) ? result : null;
            }

            return result;
        }
        // аргументы с одинаковым первым индексом отвечают за две соответствующие
        // краевые коориданты одного объекта
        static double? CollideParams(Disk disk1, Disk disk2)
        {
            double? xParam = null, yParam = null;
            double? result;
            
            if (disk2.PreviousLocation.X >= disk1.PreviousLocation.X + disk1.radius * 2) // диск 2 справа от диска 1
            {
                if (disk2.SPEED.X - disk1.SPEED.X <= 0)
                {
                    xParam = Physics.FindCoordinateParams(disk2.PreviousLocation.X,
                        disk2.PreviousLocation.X + disk2.radius * 2, disk1.PreviousLocation.X + disk1.radius * 2,
                        disk1.PreviousLocation.X, disk2.SPEED.X, disk1.SPEED.X);
                }
            }
            else if (disk1.PreviousLocation.X >= disk2.PreviousLocation.X + disk2.radius * 2) // диск 1 справа
            {
                if (disk2.SPEED.X - disk1.SPEED.X >= 0)
                {
                    xParam = Physics.FindCoordinateParams(disk1.PreviousLocation.X,
                        disk1.PreviousLocation.X + disk1.radius * 2, disk2.PreviousLocation.X + disk2.radius * 2,
                        disk2.PreviousLocation.X, disk1.SPEED.X, disk2.SPEED.X);
                }
            }
            else if (Physics.CheckOverlap(disk1.PreviousLocation.X,
                        disk1.PreviousLocation.X + disk1.radius * 2, disk2.PreviousLocation.X + disk2.radius * 2,
                        disk2.PreviousLocation.X))
            {
                xParam = 0;
            }
            if (disk2.PreviousLocation.Y >= disk1.PreviousLocation.Y + disk1.high) // диск 2 снизу
            {
                if (disk2.SPEED.Y - disk1.SPEED.Y <= 0)
                {
                    yParam = Physics.FindCoordinateParams(disk2.PreviousLocation.Y,
                        disk2.PreviousLocation.Y + disk2.high, disk1.PreviousLocation.Y + disk1.high,
                        disk1.PreviousLocation.Y, disk2.SPEED.Y, disk1.SPEED.Y);
                }
            }
            else if (disk1.PreviousLocation.Y >= disk2.PreviousLocation.Y + disk2.high) // диск 1 снизу
            {
                if (disk2.SPEED.Y - disk1.SPEED.Y >= 0)
                {
                    yParam = Physics.FindCoordinateParams(disk1.PreviousLocation.Y,
                        disk1.PreviousLocation.Y + disk1.high, disk2.PreviousLocation.Y + disk2.high,
                        disk2.PreviousLocation.Y, disk1.SPEED.Y, disk2.SPEED.Y);
                }
            }
            else if (Physics.CheckOverlap(disk1.PreviousLocation.Y,
                        disk1.PreviousLocation.Y + disk1.high, disk2.PreviousLocation.Y + disk2.high,
                        disk2.PreviousLocation.Y))
            {
                yParam = 0;
            }
            if (yParam == null && Physics.CheckOverlap(disk1.LOCATION.Y, disk1.LOCATION.Y + disk1.high,
                disk2.LOCATION.Y, disk2.LOCATION.Y + disk2.high))
            {
                yParam = 0;
            }
            if (xParam == null && Physics.CheckOverlap(disk1.LOCATION.X, disk1.LOCATION.X + disk1.radius * 2,
                disk2.LOCATION.X, disk2.LOCATION.X + disk2.radius * 2))
            {
                xParam = 0;
            }
            if (xParam == null || yParam == null)
            {
                result = null;
            }
            else if (xParam > yParam)
            {
                result = xParam;
                int add1 = (int)(disk1.SPEED.Y * result);
                int add2 = (int)(disk2.SPEED.Y * result);
                if (!Physics.CheckOverlap(disk1.PreviousLocation.Y + add1,
                        disk1.PreviousLocation.Y + disk1.high + add1, disk2.PreviousLocation.Y + disk2.high + add2,
                        disk2.PreviousLocation.Y + add2))
                {
                    result = null;
                }
            }
            else
            {
                result = yParam;
                int add1 = (int)(disk1.SPEED.X * result);
                int add2 = (int)(disk2.SPEED.X * result);
                if (!Physics.CheckOverlap(disk1.PreviousLocation.X + add1,
                        disk1.PreviousLocation.X + add1 + disk1.radius * 2, disk2.PreviousLocation.X + add2 + disk2.radius * 2,
                        disk2.PreviousLocation.X + add2))
                {
                    result = null;
                }
            }
            return result;
        }
        static double? CollideParams(Disk disk1, Tower tower)
        {
            double? xParam = null, yParam = null;
            double? result;

            if (tower.LOCATION.X>= disk1.PreviousLocation.X + disk1.radius * 2) // башня справа от диска
            {
                if (disk1.SPEED.X >= 0)
                {
                    xParam = Physics.FindCoordinateParams(tower.LOCATION.X,
                        tower.LOCATION.X + Tower.radius * 2, disk1.PreviousLocation.X + disk1.radius * 2,
                        disk1.PreviousLocation.X, 0, disk1.SPEED.X);
                }
            }
            else if (disk1.PreviousLocation.X >= tower.LOCATION.X + Tower.radius * 2) // диск 1 справа
            {
                if (disk1.SPEED.X <= 0)
                {
                    xParam = Physics.FindCoordinateParams(disk1.PreviousLocation.X,
                        disk1.PreviousLocation.X + disk1.radius * 2, tower.LOCATION.X + Tower.radius * 2,
                        tower.LOCATION.X, disk1.SPEED.X, 0);
                }
            }
            else if (Physics.CheckOverlap(disk1.PreviousLocation.X,
                        disk1.PreviousLocation.X + disk1.radius * 2, tower.LOCATION.X + Tower.radius * 2,
                        tower.LOCATION.X))
            {
                xParam = 0;
            }
            if (tower.LOCATION.Y >= disk1.PreviousLocation.Y + disk1.high) // диск сверху
            {
                if (disk1.SPEED.Y >= 0)
                {
                    yParam = Physics.FindCoordinateParams(tower.LOCATION.Y,
                        tower.LOCATION.Y + Tower.high, disk1.PreviousLocation.Y + disk1.high,
                        disk1.PreviousLocation.Y, 0, disk1.SPEED.Y);
                }
            }
            else if (disk1.PreviousLocation.Y >= tower.LOCATION.Y + Tower.high) // диск 1 снизу
            {
                if (disk1.SPEED.Y <= 0)
                {
                    yParam = Physics.FindCoordinateParams(disk1.PreviousLocation.Y,
                        disk1.PreviousLocation.Y + disk1.high, tower.LOCATION.Y + Tower.high,
                        tower.LOCATION.Y, disk1.SPEED.Y, 0);
                }
            }
            else if (Physics.CheckOverlap(disk1.PreviousLocation.Y,
                        disk1.PreviousLocation.Y + disk1.high, tower.LOCATION.Y + Tower.high,
                        tower.LOCATION.Y))
            {
                yParam = 0;
            }
            if (yParam == null && Physics.CheckOverlap(disk1.LOCATION.Y, disk1.LOCATION.Y + disk1.high,
                tower.LOCATION.Y + Tower.high,
                        tower.LOCATION.Y))
            {
                yParam = 0;
            }
            if (xParam == null && Physics.CheckOverlap(disk1.LOCATION.X, disk1.LOCATION.X + disk1.radius * 2,
                tower.LOCATION.X + Tower.radius * 2,
                        tower.LOCATION.X))
            {
                xParam = 0;
            }
            if (xParam == null || yParam == null)
            {
                result = null;
            }
            else if (xParam > yParam)
            {
                result = xParam;
                int add1 = (int)(disk1.SPEED.Y * result);
                if (!Physics.CheckOverlap(disk1.PreviousLocation.Y + add1,
                        disk1.PreviousLocation.Y + disk1.high + add1, tower.LOCATION.Y + Tower.high,
                        tower.LOCATION.Y))
                {
                    result = null;
                }
            }
            else
            {
                result = yParam;
                int add1 = (int)(disk1.SPEED.X * result);
                if (!Physics.CheckOverlap(disk1.PreviousLocation.X + add1,
                        disk1.PreviousLocation.X + add1 + disk1.radius * 2, tower.LOCATION.X + Tower.radius * 2,
                        tower.LOCATION.X))
                {
                    result = null;
                }
            }
            return result;
        }
        // x1, y1 - две точки первого отрезка, x2, y2 - две точки второго отрезка
        public static bool CheckOverlap(int x1, int y1, int x2, int y2)
        {
            Physics.CheckAndSwap(ref x1, ref y1);
            Physics.CheckAndSwap(ref x2, ref y2);
            bool result = true;
            if ((y1 < x2) || (x1 > y2))
            {
                result = false;
            }
            return result;
        }
        static void CheckAndSwap(ref int a, ref int b)
        {
            if (a > b)
            {
                Physics.Swap(ref a, ref b);
            }
        }
        static void Swap(ref int a, ref int b)
        {
            int temp = a;
            a = b;
            b = temp;
        }
    }
}
