using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP
{
	class PartialState
	{
		private double[,] matrix;
		private double lowerBound;
        private List<Tuple<int,int>> edges;

        public PartialState(double[,] matrix, double lowerBound, List<Tuple<int, int>> edges)
        {
            this.matrix = matrix;
            this.lowerBound = lowerBound;
            this.edges = edges;
        }

        public double[,] Matrix 
		{ 
			get { return matrix;	}
			set { this.matrix = value; }
		}

		public double LowerBound
		{
			get { return lowerBound; }
			set { this.lowerBound = value; }
		}

        public List<Tuple<int, int>> Edges
        {
            get { return edges; }
            set { this.edges = value; }
        }
    }
}
