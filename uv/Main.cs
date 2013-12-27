using System;
using System.Collections.Generic;

namespace uv
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			uv t = new uv(	new double[,] {{0,0,0,3},{2,0,4,1},{2,3,0,0}},
							new double[,] {{2,2,2,1},{10,8,5,4},{7,6,6,8}});
			t.solve();
		}
	}
	class cell
	{
		public double value {set;get;}
		public double cost {set;get;}
		public int x {set;get;}
		public int y {set;get;}

		public cell(){}
		public cell(int x, int y, double value, double cost)
		{
			this.x = x ;
			this.y = y ;
			this.value = value;
			this.cost = cost;

		}
	}
	class uv 
	{
		public uv(double[,] val, double[,] cost)
		{
			this.startingSolution = new List<cell>();
			for (int i = 0; i < val.GetLength(0); i++) {
				for (int j = 0; j < val.GetLength(1); j++) {
					cell t = new cell(i,j,val[i,j],cost[i,j]);
					startingSolution.Add(t);
				}
			}
			this.mainVars = new List<cell>();
			this.secondaryVars = new List<cell>();
			SortVars();
			this.BaseVar = val;
			this.u = new double[val.GetLength(0)];
			this.v = new double[val.GetLength(1)];
			this.ring = new List<cell>();
			ring.AddRange(mainVars);
			this.tempRing = new List<cell>();
		}
		List<cell> startingSolution;
		public List<cell> mainVars;
		public List<cell> secondaryVars;
		List<cell> ring;
		List<cell> tempRing;
		double[,] BaseVar; 
		double[] u ;
		double[] v ;
		int MinChangeIndex;
		bool firstVisit = true;

		void SortVars()
		{
			foreach (var item in startingSolution) {
				if (item.value != 0) {
					mainVars.Add(item);
				}
				else
					secondaryVars.Add(item);
			}
		}

		public void solve()
		{
			Find_uv();
			DoRelativeChanges();
			FindRing(secondaryVars[MinChangeIndex].x,secondaryVars[MinChangeIndex].y,true);
			FindTheta();
			while(!this.check())
			{
				Find_uv();
				DoRelativeChanges();
				FindRing(secondaryVars[MinChangeIndex].x,secondaryVars[MinChangeIndex].y,true);
				FindTheta();
			}
		}

		void Find_uv()
		{
			foreach (var item in mainVars) {
				if(v[item.y] == 0 )
					v[item.y] = item.cost - u[item.x];
				else
					u[item.x] = item.cost - v[item.y];
			}
		}

		void DoRelativeChanges()
		{
			double min = secondaryVars[0].value;
			foreach (var item in secondaryVars) {
				item.value = item.cost - (u[item.x] + v[item.y]);
				if (item.value < min) {
					min = item.value;
					MinChangeIndex = secondaryVars.IndexOf(item);
				}
			}
		}

		void FindRing(int x, int y, bool isRow)
		{
			if (tempRing.Count + 1 <= ring.Count)
            {
                if (isRow)
                {
                    for (int j = 0; j < BaseVar.GetLength(1); j++)
                        if (secondaryVars[MinChangeIndex].x == x && secondaryVars[MinChangeIndex].y == j && !firstVisit)
                        {
                            ring.Clear();
                            ring.AddRange(tempRing);
                        }
                        else 
                        {
                            firstVisit = false;
                            if (BaseVar[x, j] != 0 && !tempRing.Exists(f => f.x == x && f.y == j))
                            {
                                tempRing.Add(mainVars.Find(f => f.x == x && f.y == j));
                                FindRing(x, j, !isRow);
                            }
                        }
                }
                else 
                {
                    for (int i = 0; i < BaseVar.GetLength(0); i++)
                        if (secondaryVars[MinChangeIndex].x == i && secondaryVars[MinChangeIndex].y == y )
                        {
                            ring.Clear();
                            ring.AddRange(tempRing);
                        }
                        else
                        {
                            if (BaseVar[i, y] != 0 && !tempRing.Exists(f => f.x == i && f.y == y))
                            {
                                tempRing.Add(mainVars.Find(f => f.x == i && f.y == y));
                                FindRing(i, y, !isRow);
                            }
                        }
                }
            }
            else 
            {
                tempRing.RemoveAt(tempRing.Count -1 );                
            }
        }

		void FindTheta()
		{
			double min = ring[0].value;
			cell newSecVar = new cell();
			for (int i = 0; i < ring.Count; i++) 
			{
				if (((i%2) == 0)&&(ring[i].value < min))
				{
					min = ring[i].value;
					int tmp = mainVars.IndexOf(ring[i]);
					newSecVar = mainVars[tmp];
				}
			}
			for (int i = 0; i < ring.Count; i++) 
			{
				if ((i%2) == 0)
				{
					mainVars[mainVars.IndexOf(ring[i])].value -= min;
				}
				else
					mainVars[mainVars.IndexOf(ring[i])].value += min;
			}
			cell newMainVar = secondaryVars[MinChangeIndex];
			newMainVar.value = min;
			mainVars.Add(newMainVar);
			mainVars.Remove(newSecVar);
			secondaryVars.Remove(newMainVar);
			secondaryVars.Add(newSecVar);
			RebuildBase();
		}

		bool check()
		{
			foreach (var item in secondaryVars) {
				if (item.value < 0) {
					return false;
				}
			}
			return true;
		}

		void RebuildBase()
		{
			foreach (var item in mainVars) {
				BaseVar[item.x,item.y] = item.value;
			}
			foreach (var item in secondaryVars) {
				BaseVar[item.x,item.y] = item.value;
			}
		}

	}
}