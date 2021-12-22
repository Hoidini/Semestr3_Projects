using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    partial class Probabilistic_Shoot
    {
        class TreeOfFireLogic
        {
            readonly Random rand;
            readonly ANode Root;
            readonly int[] CountOfLifeShipsEveryType;
            int cur_index;
            public TreeOfFireLogic()
            {
                rand = new Random((int)(DateTime.Now.Ticks % 10000000));
                this.Root = new Node((0, 9), (0, 9));
                this.TreeOfFireLogicRecurs((0,9), (0, 9), ref this.Root);
                this.Root.CalculateMass();
                this.CountOfLifeShipsEveryType = new int[4] { 4, 3, 2, 1 };
                this.cur_index = 3;
            }
            public void Decrement_Count_Of_Ship(int len)
            {
                if (len < 1 || len > 4)
                {
                    throw new ArgumentException("Шо с длиной, ковбой?");
                }
                this.CountOfLifeShipsEveryType[len - 1]--;
                if (this.CountOfLifeShipsEveryType[cur_index] == 0)
                {
                    while (this.cur_index >= 0)
                    {
                        cur_index--;
                        if (cur_index < 0 || this.CountOfLifeShipsEveryType[cur_index] != 0)
                        {
                            break;
                        }
                    }
                }
            }
            public Point ChooseShip() 
            {
                ANode current = this.Root;
                int mass;
                int numbind;
                while (current is Node cur)
                {
                    mass = numbind = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        if (cur.NEXTANODES[i] != null)
                        {
                            if (cur.NEXTANODES[i].MASS[this.cur_index] > mass)
                            {
                                mass = cur.NEXTANODES[i].MASS[this.cur_index];
                                numbind = i;
                            }
                            else if (cur.NEXTANODES[i].MASS[this.cur_index] == mass)
                            {
                                if (this.rand.Next() % 2 == 0)
                                {
                                    numbind = i;
                                }
                            }
                        }
                    }
                    current = cur.NEXTANODES[numbind];
                }
                Leaf leaf = current as Leaf;
                Point result = leaf.coords;
                leaf.parent = null;
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < i + 1; j++)
                    {
                        for (int k = 0; k < 2; k++)
                        {
                            leaf.ExistenseVector[i][j, k] = false;
                        }
                    }
                }
                this.ChangeSur(leaf);
                return result;
            }
            public void ChangeSur(Point p)
            {
                Leaf leaf = this.GetLeaf(p);
                for(int i = 0; i< 4; i++)
                {
                    for(int j = 0; j < i + 1; j++)
                    {
                        leaf.ExistenseVector[i][j, 0] = false;
                        leaf.ExistenseVector[i][j, 1] = false;
                    }
                }
                this.ImpactSurroundings(leaf, true, true);
                this.ImpactSurroundings(leaf, true, false);
                this.ImpactSurroundings(leaf, false, true);
                this.ImpactSurroundings(leaf, false, false);
                this.Root.CalculateMass();
            }
            void ChangeSur(Leaf leaf)
            {
                this.ImpactSurroundings(leaf, true, true);
                this.ImpactSurroundings(leaf, true, false);
                this.ImpactSurroundings(leaf, false, true);
                this.ImpactSurroundings(leaf, false, false);
                this.Root.CalculateMass();
            }
            Leaf GetLeaf(Point p)
            {
                ANode current = this.Root;
                while (current is Node cur)
                {
                    int x = p.X; int y = p.Y;
                    int halfX = (cur.XCoords.Item2 - cur.XCoords.Item1) / 2 + cur.XCoords.Item1;
                    int halfY = (cur.YCoords.Item2 - cur.YCoords.Item1) / 2 + cur.YCoords.Item1;
                    if (x <= halfX)
                    {
                        if (y <= halfY)
                        {
                            current = cur.NEXTANODES[0];
                        }
                        else
                        {
                            current = cur.NEXTANODES[1];
                        }
                    }
                    else
                    {
                        if (y <= halfY)
                        {
                            current = cur.NEXTANODES[2];
                        }
                        else
                        {
                            current = cur.NEXTANODES[3];
                        }
                    }
                }
                return current as Leaf;
            }
            void ImpactSurroundings(Leaf curLeaf, bool IsHorizontal, bool IsPositive)
            {
                Point newP;
                Point curp = curLeaf.coords;
                Leaf nextleaf;
                bool GoNextIteration;
                bool[][,] NewBoolVect = new bool[4][,];
                for (int i = 0; i < 4; i++)
                {
                    NewBoolVect[i] = new bool[i + 1, 2];
                    for (int j = 0; j < i + 1; j++)
                    {
                        NewBoolVect[i][j, 0] = true;
                        NewBoolVect[i][j, 1] = true;
                    }
                }
                if (IsHorizontal)
                {
                    if (IsPositive)
                    {
                        newP = new Point(curp.X, curp.Y + 1);
                        if(newP.X < 0 || newP.X > 9 || newP.Y < 0|| newP.Y > 9)
                        {
                            return;
                        }
                        GoNextIteration = CountSur(1, 1);
                    }
                    else
                    {
                        newP = new Point(curp.X, curp.Y - 1);
                        if (newP.X < 0 || newP.X > 9 || newP.Y < 0 || newP.Y > 9)
                        {
                            return;
                        }
                        GoNextIteration = CountSur(1, -1);

                    }
                }
                else
                {
                    if (IsPositive)
                    {
                        newP = new Point(curp.X + 1, curp.Y);
                        if (newP.X < 0 || newP.X > 9 || newP.Y < 0 || newP.Y > 9)
                        {
                            return;
                        }
                        GoNextIteration = CountSur(0, 1);

                    }
                    else
                    {
                        newP = new Point(curp.X - 1, curp.Y);
                        if (newP.X < 0 || newP.X > 9 || newP.Y < 0 || newP.Y > 9)
                        {
                            return;
                        }
                        GoNextIteration = CountSur(0, -1);
                    }
                }
                nextleaf = this.GetLeaf(newP);
                for(int i = 0; i < 4; i++)
                {
                    for(int j = 0; j < i + 1; j++)
                    {
                        for(int k = 0; k < 2; k++)
                        {
                            nextleaf.ExistenseVector[i][j, k] =
                                (nextleaf.ExistenseVector[i][j, k] && NewBoolVect[i][j, k]);
                        }
                    }
                }
                if (GoNextIteration)
                {
                    this.ImpactSurroundings(nextleaf, IsHorizontal, IsPositive);
                }
                bool CountSur(int IndexOfOrt, int du) // результат - было ли что-то изменено
                {
                    bool result = false;
                    for (int i = 1; i < 4; i++)
                    {
                        int j = (du > 0 ? 1 : i - 1);
                        Func<bool> check = delegate { return (du > 0 ? (j <= i) : (j >= 0)); };
                        for (; check(); j += du)
                        {
                            if(!result && NewBoolVect[i][j - du, IndexOfOrt] != curLeaf.ExistenseVector[i][j, IndexOfOrt])
                            {
                                result = true;
                            }
                            NewBoolVect[i][j - du, IndexOfOrt] = curLeaf.ExistenseVector[i][j, IndexOfOrt];
                        }
                    }
                    return result;
                }
            }
            void TreeOfFireLogicRecurs((int, int) XCoords, (int, int) YCoords, ref ANode anode)
            {
                Node node = anode as Node;
                int limx = (XCoords.Item2 - XCoords.Item1) / 2 + XCoords.Item1;
                int limy = (YCoords.Item2 - YCoords.Item1) / 2 + YCoords.Item1;
                node.NEXTANODES[0] = this.GetNewANode((XCoords.Item1, limx), (YCoords.Item1, limy), ref node);
                node.NEXTANODES[1] = this.GetNewANode((XCoords.Item1, limx), (limy + 1, YCoords.Item2), ref node);
                node.NEXTANODES[2] = this.GetNewANode((limx + 1, XCoords.Item2), (YCoords.Item1, limy), ref node);
                node.NEXTANODES[3] = this.GetNewANode((limx + 1, XCoords.Item2), (limy + 1, YCoords.Item2), ref node);

            }
            ANode GetNewANode((int, int) XCoords, (int, int) YCoords, ref Node parent) //примен только в конструкторе
            {
                ANode result = null;
                if (XCoords.Item2 - XCoords.Item1 + 1 > 0 && YCoords.Item2 - YCoords.Item1 + 1 > 0)
                {
                    if (XCoords.Item2 - XCoords.Item1 > 0 || YCoords.Item2 - YCoords.Item1 > 0)
                    {
                        result = new Node(XCoords, YCoords, parent);
                        this.TreeOfFireLogicRecurs(XCoords, YCoords, ref result);
                    }
                    else
                    {
                        bool[][,] vect = this.GetExVect(XCoords.Item1, YCoords.Item1);
                        result = new Leaf(new Point(XCoords.Item1, YCoords.Item1), parent, vect);
                    }
                }
                return result;
            }
            abstract class ANode
            {
                protected int[] mass;
                public Node parent;
                public int[] MASS
                {
                    get => this.mass;
                }
                protected void InitMass()
                {
                    this.mass = new int[4];
                }
                public abstract int[] CalculateMass();
            }
            bool[][,] GetExVect(int x, int y)
            {
                bool[][,] result = new bool[4][,];
                for (int i = 0; i < 4; i++)
                {
                    result[i] = new bool[i + 1, 2];
                    for (int j = 0; j < i + 1; j++)
                    {
                        int x1 = x - (i - j), x2 = x + j;
                        int y1 = y - (i - j), y2 = y + j;
                        bool xres, yres;
                        if (x1 >= 0 && x1 < 10 && x2 >= 0 && x2 < 10)
                        {
                            xres = true;
                        }
                        else
                        {
                            xres = false;
                        }
                        if (y1 >= 0 && y1 < 10 && y2 >= 0 && y2 < 10)
                        {
                            yres = true;
                        }
                        else
                        {
                            yres = false;
                        }
                        result[i][j, 0] = xres;
                        result[i][j, 1] = yres;
                    }
                }
                return result;
            }
            class Leaf : ANode
            {
                public readonly Point coords;
                public readonly bool[][,] ExistenseVector; //первый индекс отвечает за количество палуб, 
                //второй индекс отвечает за количество возможных размещений корабля по соотв. координате
                // 0 - x; 1 - y(x или y задает вторая часть индекса);
                // при максимальном индексе 1 точка корабля стоит на изначальной, вторая
                //максимально сдвинута по положительному направлению.

                public Leaf(Point coords, Node parent, bool[][,] ExistenseVector)
                {
                    base.parent = parent;
                    base.InitMass();
                    this.coords = coords;
                    this.ExistenseVector = ExistenseVector;
                }

                public override int[] CalculateMass()
                {
                    int[] result = new int[4] { 0, 0, 0, 0 };
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < i + 1; j++)
                        {
                            for (int k = 0; k < 2; k++)
                            {
                                if (this.ExistenseVector[i][j, k] == true)
                                {
                                    result[i] += 1;
                                }
                            }
                        }
                    }
                    this.mass = result;
                    return result;
                }
            }
            class Node : ANode
            {
                public (int, int) XCoords, YCoords;
                readonly ANode[] NextANodes; // LXLY, LXMY, MXLY, MXMY
                public ANode[] NEXTANODES
                {
                    get => this.NextANodes;
                }
                public Node((int, int) XCoords, (int, int) YCoords, Node parent)
                {
                    base.InitMass();
                    this.XCoords = XCoords;
                    this.YCoords = YCoords;
                    this.parent = parent;
                    this.NextANodes = new ANode[4];
                    for (int i = 0; i < 4; i++)
                    {
                        this.NextANodes[i] = null;
                    }
                }
                public Node((int, int) XCoords, (int, int) YCoords) : this(XCoords, YCoords, null) { }

                public override int[] CalculateMass()
                {
                    int[] result = new int[4] { 0, 0, 0, 0 };
                    for (int i = 0; i < 4; i++)
                    {
                        if (this.NextANodes[i] != null)
                        {
                            int[] temp = this.NextANodes[i].CalculateMass();
                            for (int j = 0; j < 4; j++)
                            {
                                result[j] += temp[j];
                            }
                        }
                    }
                    base.mass = result;
                    return result;
                }
            }
        }
    }
}
