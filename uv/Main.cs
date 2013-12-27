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
		
		public cell FindFInRow(List<cell> l)
		{
			cell res = null;
			int distance = 0;
			foreach (var item in l) {
				if (item.x == this.x) {
					if (Math.Abs(this.y - item.y) > distance) {
						distance = Math.Abs(this.y - item.y);
						res = item;
					}
				}
			}
			if (res != null)
			{
				return res;
			}

			return null ;
		}

		public cell FindFinCol(List<cell> l)
		{
			cell res = null;
			int distance = 0 ;
			foreach (var item in l) {
				if (item.y == this.y) {
					if (Math.Abs(this.x - item.x) > distance) {
						distance = Math.Abs(this.x - item.x);
						res = item;
					}
				}
			}
			if (res != null)
			{
				return res;
			}

			return null ;
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
			this.u = new double[val.GetLength(0)];
			this.v = new double[val.GetLength(1)];
			this.ring = new List<cell>();
			this.tested = new List<cell>();
		}
		List<cell> startingSolution;
		public List<cell> mainVars;
		public List<cell> secondaryVars;
		List<cell> ring;
		List<cell> tested;
		double[] u ;
		double[] v ;
		int MinChangeIndex;

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
			FindRing(secondaryVars[MinChangeIndex]);
			FindTheta();
			while(!this.check())
			{
				Find_uv();
				DoRelativeChanges();
				FindRing(secondaryVars[MinChangeIndex]);
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

		void FindRing(cell sp)
		{
			Predicate<cell> inRow = (cell c) => {return c.x == sp.x;};
			List<cell> inSRow = mainVars.FindAll(inRow);
			cell fInRow = sp.FindFInRow(inSRow);
			if(!tested.Contains(fInRow))
			{
				tested.Add(fInRow);
				if (fInRow != null) 
				{
					ring.Add(fInRow);
					if(fInRow.y != secondaryVars[MinChangeIndex].y)
					{
						Predicate<cell> inCol = (cell c) => {return c.y == fInRow.y;};
						List<cell> inSCol = mainVars.FindAll(inCol);
						cell fInCol = fInRow.FindFinCol(inSCol);
						if (!tested.Contains(fInCol)) {
							tested.Add(fInCol);
							if(fInCol != null)
							{
								ring.Add(fInCol);
								FindRing(fInCol);
							}
							else
							{
								inSCol.Remove(fInCol);
								fInCol = fInRow.FindFinCol(inSCol);
								ring.Add(fInCol);
								FindRing(fInCol);
							}
						}
					}
				}
			}
			else
			{
				inSRow.Remove(fInRow);
				fInRow = sp.FindFInRow(inSRow);
				ring.Add(fInRow);
				if(fInRow.y != secondaryVars[MinChangeIndex].y)
				{
					Predicate<cell> inCol = (cell c) => {return c.y == fInRow.y;};
					List<cell> inSCol = mainVars.FindAll(inCol);
					cell fInCol = fInRow.FindFinCol(inSCol);
					if (!tested.Contains(fInCol)) {
						tested.Add(fInCol);
						if(fInCol != null)
						{
							ring.Add(fInCol);
							FindRing(fInCol);
						}
						else
						{
							inSCol.Remove(fInCol);
							fInCol = fInRow.FindFinCol(inSCol);
							ring.Add(fInCol);
							FindRing(fInCol);
						}
					}
				}
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
			ring.Clear();
			tested.Clear();
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

	}
}