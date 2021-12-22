using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TowersHan
{
    class Bot : User
    {
        public event Action ReSpawn;
        List<Tower> towers;
        Disk disk;
        int coord_to_move;
        public Disk CapturedDisk
        {
            get => this.disk;
        }
        public List<Tower> Towers
        {
            get => this.towers;
            set => this.towers = value;
        }
        List<(int, int)> steps;
        int animation_step;
        int vector;
        int cur_index;
        bool IsActive = false;
        public Bot() : this(null) { }
        public Bot(List<Tower> towers)
        {
            this.towers = towers;
            this.disk = null;
            this.animation_step = 0;
            this.steps = new List<(int, int)>();
            this.cur_index = 0;
            this.vector = 0;
            coord_to_move = 0;
        }
        public Point? DoStep()
        {
            Point? result = null;
            if (this.IsActive)
            {
                result = this.Animate();
            }
            return result;
        }
        public void GetSteps(int n, int i, int k)
        {
            if (n == 1)
            {
                this.steps.Add((i, k));
            }
            else
            {
                int tmp = 3 - i - k;
                GetSteps(n - 1, i, tmp);
                this.steps.Add((i, k));
                GetSteps(n - 1, tmp, k);
            }
        }
        Point? Animate() // возвращаемое значение - продолжается ли анимация
        {
            if (this.disk != null)
            {
                int first = 10;
                int second = this.towers[this.steps[this.cur_index].Item2].LOCATION.X + Tower.radius - disk.radius;
                int third;
                if (this.towers[this.steps[this.cur_index].Item2].DisksCount != 0)
                {
                    third = this.towers[this.steps[this.cur_index].Item2].peek().LOCATION.Y - disk.high;
                }
                else
                {
                    third = this.towers[this.steps[this.cur_index].Item2].LOCATION.Y + Tower.high - disk.high;
                }
                switch (this.animation_step)
                {
                    case 0:
                        disk.LOCATION = new System.Drawing.Point(disk.LOCATION.X,
                            this.vector / 10 + disk.LOCATION.Y);
                        if (disk.LOCATION.Y <= first)
                        {
                            this.vector = second - disk.LOCATION.X;
                            this.animation_step++;
                        }
                        return disk.LOCATION;
                    case 1:
                        disk.LOCATION = new System.Drawing.Point(this.vector / 10 + disk.LOCATION.X,
                            disk.LOCATION.Y);
                        if (disk.LOCATION.X == second)
                        {
                            this.coord_to_move = third;
                            this.vector = third - disk.LOCATION.Y;
                            this.disk.SPEED = new Point(0, vector / 10);
                            this.disk.ISCATCHED = false;
                            this.animation_step = 0;
                            if (++this.cur_index < this.steps.Count)
                            {
                                int index_to_anim = this.steps[this.cur_index].Item1;
                                this.disk = this.towers[index_to_anim].peek();
                                this.disk.ISCATCHED = true;
                                this.vector = 10 - disk.LOCATION.Y;
                                this.IsActive = true;
                                return disk.LOCATION;
                            }
                            else
                            {
                                this.IsActive = false;
                                return null;
                            }
                        }
                        return disk.LOCATION;
                    case 2:
                        disk.LOCATION = new System.Drawing.Point(disk.LOCATION.X,
                            vector / 10 + disk.LOCATION.Y);
                        if (disk.LOCATION.Y >= this.coord_to_move)
                        {
                            this.animation_step = 0;
                            this.vector = 0;
                            disk.ISCATCHED = false;
                            this.disk = null;
                            this.IsActive = false;
                            if (++this.cur_index < this.steps.Count)
                            {
                                int index_to_anim = this.steps[this.cur_index].Item1;
                                this.disk = this.towers[index_to_anim].peek();
                                this.disk.ISCATCHED = true;
                                this.vector = 50 - disk.LOCATION.Y;
                                this.IsActive = true;
                                return disk.LOCATION;
                            }
                        }
                        else
                        {
                            return disk.LOCATION;
                        }
                        break;
                };
            }
            return null;
        }
        public void StartAnimation()
        {
            if (!this.IsActive)
            {
                this.ReSpawn();
                this.disk = this.towers[0].peek();
                this.vector = 10 - disk.LOCATION.Y;
                this.disk.ISCATCHED = true;
                this.IsActive = true;
            }
        }
    }
}
