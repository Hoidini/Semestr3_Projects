using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    
   public class Animation : IEnumerable
    {
        class SortDictAnim : IComparer<HitSteps>
        {
            public int Compare(HitSteps x, HitSteps y)
            {
                return ((int)x - (int)y);
            }
        }
        enum HitSteps {step1, step2, step3 };
        int counter;
        readonly SortedDictionary<HitSteps, Bitmap> SpritesList;
        readonly Dictionary<string, HitSteps> keyValuePairs;
        public Animation()
        {
            this.counter = 0;
            this.SpritesList = new SortedDictionary<HitSteps, Bitmap>(new SortDictAnim());
            this.keyValuePairs = new Dictionary<string, HitSteps>
            {
                { "step1.png", HitSteps.step1 },
                { "step2.png", HitSteps.step2 },
                { "step3.png", HitSteps.step3 }
            };
        }
        public void LoadSprites(string path)
        {
            this.SpritesList.Clear();
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            foreach(var file in directoryInfo.GetFiles())
            {
                string name = file.Name;
                if (this.keyValuePairs.ContainsKey(name))
                {
                    this.SpritesList.Add(this.keyValuePairs[name], (Bitmap)Image.FromFile(file.FullName));
                }
            }
        }
        public Bitmap IteratePictures()
        {
            if(this.SpritesList.Count == 0 || this.counter > 2)
            {
                this.counter = 0;
                this.SpritesList.Clear();
                return null;
            }
            else
            {
                Bitmap image = this.SpritesList[(HitSteps)(this.counter)];
                counter++;
                return image;
            }
        }
        public IEnumerator GetEnumerator()
        {
            foreach(var package in this.SpritesList)
            {
                Image image = package.Value;
                yield return image;
            }
        }
    }
}
