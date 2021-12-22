using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowersHan
{
    class Tower : GameObject
    {
        public const int radius = 30;
        public const int high = 500;
        Stack<Disk> disks;
        public Point[] PointsInfo
        {
            get => base.points;
        }
        public Point LOCATION
        {
            get => base.points[0];
        }
        public int DisksCount
        {
            get => this.disks.Count;
        }
        public Tower(Point location, Action<GameObject> AddToPhys) : base(AddToPhys)
        {
            base.points[0] = location;
            this.InitPoints();
            this.disks = new Stack<Disk>();
        }
        void InitPoints()
        {
            Point location = base.points[0];
            base.points[1] = new Point(location.X, location.Y + high);
            base.points[2] = new Point(location.X + 2 * radius, location.Y);
            base.points[3] = new Point(location.X + 2 * radius, location.Y + high);
            base.points[4] = new Point(location.X + radius, location.Y + (high / 2));
        }
        bool CanAddDisk(Disk disk)
        {
            bool result = true;
            if (this.disks.Count != 0 && disk.radius > this.disks.Peek().radius)
            {
                result = false;
            }
            return result;
        }
        public bool Contains(Disk disk)
        {
            bool result = false;
            if (this.disks.Count != 0 && this.disks.Contains(disk))
            {
                result = true;
            }
            return result;
        }
        public bool TryAddDisk(Disk disk)
        {
            bool result = this.CanAddDisk(disk);
            if (result)
            {
                this.disks.Push(disk);
            }
            return result;
        }
        public bool TryDelDisk()
        {
            bool result = this.disks.Count != 0;
            if (result)
            {
                this.disks.Pop();
            }
            return result;
        }
        public Disk peek()
        {
            return this.disks.Peek();
        }
    }
}
