using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TowersHan
{
    class GameController
    {
        Physics physics;
        Player player;
        public readonly Bot bot;
        int count;
        List<Tower> towers;
        List<Disk> disks;
        public GameController(Player player, Bot bot, int count)
        {
            this.player = player;
            this.bot = bot;
            this.player.TryCatchDisk += this.TryCatch;
            this.count = count;
            this.ReSpawn();
            this.bot.ReSpawn += this.ReSpawn;
            this.bot.GetSteps(this.towers[0].DisksCount, 0, 1);

        }
        public bool TryCatch(Point ClickPoint)
        {
            bool result = false;
            foreach (Disk disk in this.disks)
            {
                if (disk.TryCatch(ClickPoint))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
        public bool DoStep()
        {
            if (this.bot.CapturedDisk != null)
            {
                this.physics.IsBotWorking = true;
                this.physics.DoStep(this.bot.DoStep());
            }
            else
            {

                this.physics.DoStep(this.player.CurrentMousePosition);
            }
            return this.TryGameToEnd();
        }
        void ReSpawn()
        {
            this.physics = new Physics();
            this.physics.CountOfDisks = this.count;
            this.disks = new List<Disk>();
            this.towers = new List<Tower>();
            int add = 50 / this.count;
            for (int i = 0; i < 3; i++)
            {
                Point loc = new Point(200 + i * 200, 600 - Tower.high);
                this.towers.Add(new Tower(loc, this.physics.AddToListOfObjects));
            }
            Point location = new Point(200, 600);
            for (int i = 1; i <= count; i++)
            {
                int j = this.count - i;
                int high = Disk.MinH + j * add;
                int radius = Disk.MinR + j * add / 2;
                location = new Point(200 - radius, location.Y - high);
                Disk newDisk = new Disk(this.physics.AddToListOfObjects,
                    radius, location, high);
                newDisk.CurrentTower = this.towers[0];
                this.towers[0].TryAddDisk(newDisk);
                this.disks.Add(newDisk);
            }
            this.bot.Towers = this.towers;
        }
        bool TryGameToEnd()
        {
            bool result = false;
            if ((this.towers[1].DisksCount == this.count || this.towers[2].DisksCount == count)
                && this.disks[this.disks.Count - 1].LOCATION.Y + this.disks[this.disks.Count - 1].high ==
                this.disks[this.disks.Count - 2].LOCATION.Y)
            {
                result = true;
            }
            return result;
        }
        public List<Tower> GetTowers
        {
            get => this.towers;
        }
        public List<Disk> GetDisks
        {
            get => this.disks;
        }
    }
}
