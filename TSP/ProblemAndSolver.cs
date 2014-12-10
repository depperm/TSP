using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Timers;

namespace TSP
{

    class ProblemAndSolver
    {

        private class TSPSolution
        {
            /// <summary>
            /// we use the representation [cityB,cityA,cityC] 
            /// to mean that cityB is the first city in the solution, cityA is the second, cityC is the third 
            /// and the edge from cityC to cityB is the final edge in the path.  
            /// You are, of course, free to use a different representation if it would be more convenient or efficient 
            /// for your node data structure and search algorithm. 
            /// </summary>
            public ArrayList
                Route;

            public TSPSolution(ArrayList iroute)
            {
                Route = new ArrayList(iroute);
            }


            /// <summary>
            /// Compute the cost of the current route.  
            /// Note: This does not check that the route is complete.
            /// It assumes that the route passes from the last city back to the first city. 
            /// </summary>
            /// <returns></returns>
            public double costOfRoute()
            {
                // go through each edge in the route and add up the cost. 
                int x;
                City here;
                double cost = 0D;

                for (x = 0; x < Route.Count - 1; x++)
                {
                    here = Route[x] as City;
                    cost += here.costToGetTo(Route[x + 1] as City);
                }

                // go from the last city to the first. 
                here = Route[Route.Count - 1] as City;
                cost += here.costToGetTo(Route[0] as City);
                return cost;
            }
        }

        #region Private members 

        /// <summary>
        /// Default number of cities (unused -- to set defaults, change the values in the GUI form)
        /// </summary>
        // (This is no longer used -- to set default values, edit the form directly.  Open Form1.cs,
        // click on the Problem Size text box, go to the Properties window (lower right corner), 
        // and change the "Text" value.)
        private const int DEFAULT_SIZE = 25;

        private const int CITY_ICON_SIZE = 5;

        // For normal and hard modes:
        // hard mode only
        private const double FRACTION_OF_PATHS_TO_REMOVE = 0.20;

        /// <summary>
        /// the cities in the current problem.
        /// </summary>
        private City[] Cities;
        /// <summary>
        /// a route through the current problem, useful as a temporary variable. 
        /// </summary>
        private ArrayList Route;
        /// <summary>
        /// best solution so far. 
        /// </summary>
        private TSPSolution bssf; 

        /// <summary>
        /// how to color various things. 
        /// </summary>
        private Brush cityBrushStartStyle;
        private Brush cityBrushStyle;
        private Pen routePenStyle;


        /// <summary>
        /// keep track of the seed value so that the same sequence of problems can be 
        /// regenerated next time the generator is run. 
        /// </summary>
        private int _seed;
        /// <summary>
        /// number of cities to include in a problem. 
        /// </summary>
        private int _size;

        /// <summary>
        /// Difficulty level
        /// </summary>
        private HardMode.Modes _mode;

        /// <summary>
        /// random number generator. 
        /// </summary>
        private Random rnd;
        #endregion

        #region Public members
        public int Size
        {
            get { return _size; }
        }

        public int Seed
        {
            get { return _seed; }
        }
        #endregion

        #region Constructors
        public ProblemAndSolver()
        {
            this._seed = 1; 
            rnd = new Random(1);
            this._size = DEFAULT_SIZE;

            this.resetData();
        }

        public ProblemAndSolver(int seed)
        {
            this._seed = seed;
            rnd = new Random(seed);
            this._size = DEFAULT_SIZE;

            this.resetData();
        }

        public ProblemAndSolver(int seed, int size)
        {
            this._seed = seed;
            this._size = size;
            rnd = new Random(seed); 
            this.resetData();
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Finds the distance between two points
        /// </summary>
        private double getDistance(PointF from, PointF to)
        {
            return Math.Sqrt(Math.Pow(to.X - from.X, 2) + Math.Pow(to.Y - from.Y, 2));
        }

        /// <summary>
        /// Finds the angle(theta) between two points
        /// </summary>
        private double getAngle(PointF origin, PointF to)
        {
            float xDiff = origin.X - to.X;
            float yDiff = origin.Y - to.Y;
            return ((double)Math.Atan2(yDiff, xDiff) * (double)(180 / Math.PI)) + 180;
        }

        /// <summary>
        /// Reset the problem instance.
        /// </summary>
        private void resetData()
        {

            Cities = new City[_size];
            Route = new ArrayList(_size);
            bssf = null;

            if (_mode == HardMode.Modes.Easy)
            {
                for (int i = 0; i < _size; i++)
                {
                    Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble());
                    Cities[i].number = i;
                }
            }
            else // Medium and hard
            {
                for (int i = 0; i < _size; i++)
                {
                    Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble() * City.MAX_ELEVATION);
                    Cities[i].number = i;
                }
            }

            HardMode mm = new HardMode(this._mode, this.rnd, Cities);
            if (_mode == HardMode.Modes.Hard)
            {
                int edgesToRemove = (int)(_size * FRACTION_OF_PATHS_TO_REMOVE);
                mm.removePaths(edgesToRemove);
            }
            City.setModeManager(mm);

            cityBrushStyle = new SolidBrush(Color.Black);
            cityBrushStartStyle = new SolidBrush(Color.Red);
            routePenStyle = new Pen(Color.Blue,1);
            routePenStyle.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// make a new problem with the given size.
        /// </summary>
        /// <param name="size">number of cities</param>
        //public void GenerateProblem(int size) // unused
        //{
        //   this.GenerateProblem(size, Modes.Normal);
        //}

        /// <summary>
        /// make a new problem with the given size.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size, HardMode.Modes mode)
        {
            this._size = size;
            this._mode = mode;
            resetData();
        }

        /// <summary>
        /// return a copy of the cities in this problem. 
        /// </summary>
        /// <returns>array of cities</returns>
        public City[] GetCities()
        {
            City[] retCities = new City[Cities.Length];
            Array.Copy(Cities, retCities, Cities.Length);
            return retCities;
        }

        /// <summary>
        /// draw the cities in the problem.  if the bssf member is defined, then
        /// draw that too. 
        /// </summary>
        /// <param name="g">where to draw the stuff</param>
        public void Draw(Graphics g)
        {
            float width  = g.VisibleClipBounds.Width-45F;
            float height = g.VisibleClipBounds.Height-45F;
            Font labelFont = new Font("Arial", 10);

            // Draw lines
            if (bssf != null)
            {
                // make a list of points. 
                Point[] ps = new Point[bssf.Route.Count];
                int index = 0;
                foreach (City c in bssf.Route)
                {
                    if (index < bssf.Route.Count -1)
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[index+1]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    else 
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[0]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    ps[index++] = new Point((int)(c.X * width) + CITY_ICON_SIZE / 2, (int)(c.Y * height) + CITY_ICON_SIZE / 2);
                }

                if (ps.Length > 0)
                {
                    g.DrawLines(routePenStyle, ps);
                    g.FillEllipse(cityBrushStartStyle, (float)Cities[0].X * width - 1, (float)Cities[0].Y * height - 1, CITY_ICON_SIZE + 2, CITY_ICON_SIZE + 2);
                }

                // draw the last line. 
                g.DrawLine(routePenStyle, ps[0], ps[ps.Length - 1]);
            }

            // Draw city dots
            foreach (City c in Cities)
            {
                g.FillEllipse(cityBrushStyle, (float)c.X * width, (float)c.Y * height, CITY_ICON_SIZE, CITY_ICON_SIZE);
            }

        }

        /// <summary>
        ///  return the cost of the best solution so far. 
        /// </summary>
        /// <returns></returns>
        public double costOfBssf ()
        {
            if (bssf != null)
                return (bssf.costOfRoute());
            else
                return -1D; 
        }

        /// <summary>
        ///  solve the problem.  This is the entry point for the solver when the run button is clicked
        /// right now it just picks a simple solution. 
        /// </summary>
        public void solveProblem()
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            int x;
            Route = new ArrayList(); 
            // this is the trivial solution. 
            for (x = 0; x < Cities.Length; x++)
            {
                Route.Add( Cities[Cities.Length - x -1]);
            }
            // call this the best solution so far.  bssf is the route that will be drawn by the Draw method. 
            bssf = new TSPSolution(Route);
            timer.Stop();
            // update the cost of the tour. 
            Program.MainForm.tbCostOfTour.Text = " " + bssf.costOfRoute();
            //update the time
            Program.MainForm.tbElapsedTime.Text = "" + timer.Elapsed;
            // do a refresh. 
            Program.MainForm.Invalidate();

        }

        ///random
        public void solveRandomProblem()
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            Route = new ArrayList();

            List<int> spots = new List<int>();
            for (int i = 0; i < Cities.Length; i++)
            {
                spots.Add(i);
            }
            Random rand = new Random();
            int r;
            while (spots.Count > 0)
            {
                r = rand.Next(0, spots.Count);
                Route.Add(Cities[spots[r]]);
                spots.RemoveAt(r);
            }
            // call this the best solution so far.  bssf is the route that will be drawn by the Draw method. 
            bssf = new TSPSolution(Route);
            timer.Stop();
            // update the cost of the tour. 
            Program.MainForm.tbCostOfTour.Text = " " + bssf.costOfRoute();
            //update the time
            Program.MainForm.tbElapsedTime.Text = "" + timer.Elapsed;
            // do a refresh. 
            Program.MainForm.Invalidate();

        }

        // greedy solver
        public void greedySolve()
        {
            int initial = 0;
            bool pathNotFound = true;
            HashSet<int> visited = new HashSet<int>();

            // loop through initial starting nodes for greedy path
            // until one of them finishes visiting all cities 
            // without hitting a dead end
            while (pathNotFound)
            {
                pathNotFound = false;
                Route = new ArrayList();
                City cursor = Cities[initial];
                Route.Add(Cities[initial]);
                visited.Add(initial);

                // starting with city at index `initial`
                // go to the nearest city and repeat
                // untill all cities have been visited
                while (visited.Count != Cities.Length)
                {
                    double dist = Double.PositiveInfinity;
                    int bestNeighbor = -1;

                    for (int i = 0; i < Cities.Length; i++)
                    {
                        if (!visited.Contains(i))
                        {
                            if (cursor.costToGetTo(Cities[i]) < dist)
                            {
                                dist = cursor.costToGetTo(Cities[i]);
                                bestNeighbor = i;
                            }
                        }
                    }

                    if (bestNeighbor == -1)
                    {
                        // no path available
                        // break and try a different start node
                        // (class spec says no backtracking)
                        pathNotFound = true;
                        break;
                    }
                    else
                    {
                        // found closest neighbor, go there
                        visited.Add(bestNeighbor);
                        Route.Add(Cities[bestNeighbor]);
                        cursor = Cities[bestNeighbor];
                    }

                }

                // if the last path failed to find all cities...
                // ...start wiht a different initial city
                initial++;
            }

            // assign as best solution so far
            bssf = new TSPSolution(Route);



            //double costToBeat = bssf.costOfRoute();
            //Console.WriteLine("initial length: {0}", costToBeat);
            //bool changesMade = true;
            //while (changesMade)
            //{
            //    changesMade = false;

            //    for (int i = 1; i < (Route.Count - 2); i++)
            //    {
            //        for (int j = i + 1; j < (Route.Count - 1); j++)
            //        {
            //            // find the cost of original sequence versus cost of reversed
            //            double currSeqCost = 0.0D;
            //            for (int k = i - 1; k <= j; k++)
            //            {
            //                currSeqCost += (Route[k] as City).costToGetTo((Route[k + 1] as City));
            //            }

            //            double revSeqCost = (Route[i - 1] as City).costToGetTo((Route[j] as City));
            //            for (int k = i; k < j; k++)
            //            {
            //                double dist = (Route[k + 1] as City).costToGetTo((Route[k] as City));
            //                if (dist == 0D || Double.IsInfinity(dist) || Double.IsNaN(dist))
            //                {
            //                    revSeqCost = Double.PositiveInfinity;
            //                    break;
            //                }

            //                revSeqCost += dist;
            //            }
            //            if (!Double.IsInfinity(revSeqCost))
            //            {
            //                double dist = (Route[i] as City).costToGetTo((Route[j + 1] as City));
            //                if (dist != 0.0D && !Double.IsInfinity(dist) && !Double.IsNaN(dist))
            //                {
            //                    revSeqCost += dist;
            //                }
            //                else
            //                {
            //                    revSeqCost = Double.PositiveInfinity;
            //                }
            //            }

            //            if (revSeqCost < currSeqCost && revSeqCost != Double.PositiveInfinity)
            //            {
            //                Route.Reverse(i, j - i + 1);
            //                bssf = new TSPSolution(Route);
            //                costToBeat = bssf.costOfRoute();
            //                changesMade = true;
            //            }
            //        }
            //    }

            //}


            // update the cost of the tour. 
            Program.MainForm.tbCostOfTour.Text = " " + bssf.costOfRoute();
            // do a refresh. 
            Program.MainForm.Invalidate();
        }
        #endregion

        #region Branch & Bound
        /// I didn't give B&B it's own region to be narcisisstic, it just 
        /// kind of got big on me


        /// <summary>
        /// Row reduce the given matrix in the partial state
        /// </summary>
        private PartialState reduce(PartialState stateIn)
        {
            double lowerBound = stateIn.LowerBound;
            double[,] matrix = stateIn.Matrix;
            int size = matrix.GetLength(0);
            for (int i = 0; i < size; i++)
            {
                /// If we have already left this edge, then we don't need to reduce
                /// the row because it has already been set to infinity
                if (stateIn.Edges.Exists(x => x.Item1 == i))
                    continue;

                /// Get the lowest value in the row
                double lowestRowValue = Double.PositiveInfinity;
                for (int j = 0; j < size; j++)
                {
                    if (matrix[i, j] < lowestRowValue)
                        lowestRowValue = matrix[i, j];
                }

                /// If that value is not zero and is not infinite, then we need to reduce the row
                if (lowestRowValue != 0 && !Double.IsInfinity(lowestRowValue))
                {
                    for (int j = 0; j < size; j++)
                        matrix[i, j] = matrix[i, j] - lowestRowValue;
                }

                /// Add the lowest row value to the lower bound before going to
                /// the next row
                lowerBound += lowestRowValue;
            }

            /// Now we just need to reduce the columns and we will have a
            /// reduced cost martrix
            for (int j = 0; j < size; j++)
            {
                /// If we have already entered this edge, then
                if (stateIn.Edges.Exists(x => x.Item2 == j))
                    continue;

                /// Find the lowest value
                double lowestColValue = Double.PositiveInfinity;
                for (int i = 0; i < size; i++)
                {
                    if (matrix[i, j] < lowestColValue)
                        lowestColValue = matrix[i, j];
                }

                /// If that value is not zero and is not infinite, then we need to reduce the column
                if (lowestColValue != 0 && !Double.IsInfinity(lowestColValue))
                {
                    for (int i = 0; i < size; i++)
                        matrix[i, j] = matrix[i, j] - lowestColValue;
                }

                /// Add that value to the lower bound of this state
                lowerBound += lowestColValue;
            }
            return new PartialState(matrix, lowerBound, stateIn.Edges);
        }

        private PartialState includeEdgeState(PartialState state, Tuple<int, int> edgeToInclude)
        {
            /// We need to make a copy of the partial state.
            PartialState includeState = new PartialState(
                (double[,])state.Matrix.Clone(),
                state.LowerBound,
                new List<Tuple<int, int>>(state.Edges));

            /// Set the inverse position to infinity to not allow backtracking there.
            includeState.Matrix[edgeToInclude.Item2, edgeToInclude.Item1] = Double.PositiveInfinity;

            /// Set the values for the entire column of the destination to infinity so there are no cycles
            int size = includeState.Matrix.GetLength(0);
            for (int i = 0; i < size; i++)
                includeState.Matrix[i, edgeToInclude.Item2] = Double.PositiveInfinity;

            /// Set the values for the entire row of the origin to infinity so there are no cycles
            for (int j = 0; j < size; j++)
                includeState.Matrix[edgeToInclude.Item1, j] = Double.PositiveInfinity;

            /// Add the included edge to the Parital State Route
            includeState.Edges.Add(edgeToInclude);

            /// Delete edges that would give us a premature cycle
            if (includeState.Edges.Count < size - 1)
            {
                int startCity = edgeToInclude.Item1;
                while (includeState.Edges.Find(x => x.Item2 == startCity) != null)
                    startCity = includeState.Edges.Find(x => x.Item2 == startCity).Item1;

                int endCity = edgeToInclude.Item2;
                while (includeState.Edges.Find(x => x.Item1 == endCity) != null)
                    endCity = includeState.Edges.Find(x => x.Item1 == endCity).Item2;

                while (startCity != endCity)
                {
                    includeState.Matrix[endCity, startCity] = Double.PositiveInfinity;
                    includeState.Matrix[edgeToInclude.Item2, startCity] = Double.PositiveInfinity;
                    startCity = includeState.Edges.Find(x => x.Item1 == startCity).Item2;
                }
            }

            return reduce(includeState);
        }

        private PartialState excludeEdgeState(PartialState state, Tuple<int, int> edgeToExclude)
        {
            /// We need to make a copy of the partial state.
            PartialState excludeState = new PartialState(
                (double[,])state.Matrix.Clone(),
                state.LowerBound,
                new List<Tuple<int, int>>(state.Edges));

            /// Exclude the given edge, then reduce and return the matrix
            excludeState.Matrix[edgeToExclude.Item1, edgeToExclude.Item2] = Double.PositiveInfinity;
            return reduce(excludeState);
        }

        private double excludeCost(PartialState state, Tuple<int, int> edgeToExclude)
        {
            int size = state.Matrix.GetLength(0);

            /// Get the smallest row value, aside from the edgeToExclude
            double rowVal = Double.PositiveInfinity;
            for (int i = 0; i < size; i++)
            {
                if (i != edgeToExclude.Item1 && state.Matrix[i, edgeToExclude.Item2] < rowVal)
                    rowVal = state.Matrix[i, edgeToExclude.Item2];
            }

            /// Get the smallest col value, aside from the edgeToExclude
            double colVal = Double.PositiveInfinity;
            for (int j = 0; j < size; j++)
            {
                if (j != edgeToExclude.Item2 && state.Matrix[edgeToExclude.Item1, j] < colVal)
                    colVal = state.Matrix[edgeToExclude.Item1, j];
            }

            return colVal + rowVal;
        }

        private double IncludeExcludeDiff(PartialState state, Tuple<int, int> position)
        {
            PartialState includeState = includeEdgeState(state, position);
            return excludeCost(state, position) - includeState.LowerBound;
        }

        /// <summary>
        ///  Small helper function to help with finding some intial routes for bssf
        /// </summary>
        /// <param name="list">list to shuffle</param>
        /// <returns>a shuffled list</returns>
        private List<City> shuffle(List<City> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                City value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }


        /// <summary>
        /// solve the problem.  This is the entry point for the solver when the run button is clicked
        /// right now it just picks a simple solution. 
        /// </summary>
        public void solveBBProblem()
        {
         
            /// Get our timer and stopwatch ready
            bool intervalExceeded = false;
            Timer timer = new Timer(10 * 60 * 1000); //Minutes * 60 Seconds * 1000 Milliseconds
            ElapsedEventHandler handler = (sender, e) => { intervalExceeded = true; };
            timer.Elapsed += handler;
            Stopwatch stopwatch = new Stopwatch();

            timer.Start();
            stopwatch.Start();

            /// Generate five random, greedy solutions and select the best one as our initial value
            TSPSolution bestSoFar = null;
            List<City> randomCities = new List<City>(GetCities());
            for (int i = 0; i < 5; i++)
            {
                bool validRoute = true;
                HashSet<int> cities = new HashSet<int>();
                Route = new ArrayList();
                randomCities = shuffle(randomCities);
                Route.Add(randomCities[0]);
                for (int x = 1; x < randomCities.Count; x++)
                    cities.Add(x);
                int nextCity = -1;
                double dist = Double.PositiveInfinity;
                while (Route.Count < randomCities.Count && validRoute)
                {
                    foreach (int x in cities)
                    {
                        City temp = (City)Route[0];
                        if (temp.costToGetTo(randomCities[x]) < dist)
                        {
                            nextCity = x;
                            dist = temp.costToGetTo(randomCities[x]);
                        }
                    }

                    if (nextCity != -1)
                    {
                        Route.Insert(0, randomCities[nextCity]);
                        cities.Remove(nextCity);
                        nextCity = -1;
                        dist = double.PositiveInfinity;
                    }
                    else
                        validRoute = false;
                }

                if (Double.IsInfinity(((City)Route[0]).costToGetTo((City)Route[Route.Count - 1])))
                    validRoute = false;

                if (!validRoute)
                {
                    i--;
                    continue;
                }


                if (bestSoFar == null)
                {
                    Route.Reverse();
                    bestSoFar = new TSPSolution(Route);
                }
                else
                {
                    TSPSolution tempSolution = new TSPSolution(Route);
                    if (tempSolution.costOfRoute() < bestSoFar.costOfRoute())
                        bestSoFar = tempSolution;
                }
            }

            // call this the best solution so far.
            bssf = bestSoFar;

            City[] myCities = GetCities();

            /// Create and then fill the cost matrix
            double[,] matrix = new double[myCities.Length, myCities.Length];
            for (int i = 0; i < myCities.Length; i++)
            {
                /// Get initial values for the row
                for (int j = 0; j < myCities.Length; j++)
                {
                    if (i == j)
                        matrix[i, j] = Double.PositiveInfinity;
                    else
                        matrix[i, j] = myCities[i].costToGetTo(myCities[j]);
                }
            }

            /// Reduce the cost matrix to get the initial lower bound
            /// and the initial state for this problem
            PartialState initialState
                = reduce(new PartialState(matrix, 0, new List<Tuple<int, int>>()));

            /// Use these variables to keep track of various statistics for the 
            /// performance of the method
            int maxNumStatesStored = 0;
            int numBssfUpdates = 0;
            int numStatesCreated = 0;
            int numStatesPruned = 0;

            /// Insert the initial state into the priority queue, and
            /// we're off to the races
            PriorityQueue pq = new PriorityQueue();
            pq.insert(initialState);

            /// While we still have states in the queue...
            while (!pq.isEmpty() && !intervalExceeded)
            {
                /// Check to see how many states are being stored by the queue, update as needed
                if (pq.size() > maxNumStatesStored) maxNumStatesStored = pq.size();

                /// Get the Partial State with the lowest lower bound
                PartialState myState = pq.deleteMin();

                /// If this partial state has a lower bound that is 
                /// worst than bssf, then we can prune it
                if (bssf.costOfRoute() <= myState.LowerBound)
                {
                    numStatesPruned++;
                    continue;
                }

                /// If the route in this state has included all cities
                if (myState.Edges.Count == myCities.GetLength(0) - 1)
                {
                    Route = new ArrayList();
                    Tuple<int, int> firstEdge = myState.Edges[0];
                    Route.Add(myCities[firstEdge.Item1]);
                    Route.Add(myCities[firstEdge.Item2]);

                    int lastCity = firstEdge.Item2;
                    int firstCity = firstEdge.Item1;
                    for (int i = 2; i < myCities.GetLength(0); i++)
                    {
                        Tuple<int, int> currentEdge
                            = myState.Edges.Find(x => x.Item1 == lastCity);
                        if (currentEdge == null)
                        {
                            Tuple<int, int> tempEdge
                                = myState.Edges.Find(x => x.Item2 == firstCity);
                            if (currentEdge == null)
                                Route.Insert(0, myCities[tempEdge.Item1]);
                            firstCity = tempEdge.Item1;
                        }
                        else
                        {
                            Route.Add(myCities[currentEdge.Item2]);
                            lastCity = currentEdge.Item2;
                        }
                    }

                    TSPSolution tempSolution = new TSPSolution(Route);
                    if (bssf.costOfRoute() > tempSolution.costOfRoute())
                    {
                        bssf = tempSolution;
                        numBssfUpdates++;
                    }
                }
                else
                {
                    /// Find the spots in the partial state that have distances of zero
                    int size = myState.Matrix.GetLength(0);
                    List<Tuple<int, int>> zeroSpots = new List<Tuple<int, int>>();
                    for (int i = 0; i < size; i++)
                    {
                        /// Only check those cities that have not yet been added to our route
                        if (!myState.Edges.Exists(x => x.Item1 == i))
                        {
                            for (int j = 0; j < size; j++)
                            {
                                /// For every city not checking itself, and has a distance of 
                                /// zero after the lower bound, add it
                                if (i != j && myState.Matrix[i, j] == 0)
                                    zeroSpots.Add(new Tuple<int, int>(i, j));
                            }
                        }
                    }

                    /// Pick the zero spot that has the greatest difference between
                    /// including and excluding that particular edge.
                    Tuple<Tuple<int, int>, double> lowestSpot
                        = new Tuple<Tuple<int, int>, double>(zeroSpots[0], IncludeExcludeDiff(myState, zeroSpots[0]));
                    for (int i = 1; i < zeroSpots.Count; i++)
                    {
                        if (IncludeExcludeDiff(myState, zeroSpots[i]) > lowestSpot.Item2)
                            lowestSpot = new Tuple<Tuple<int, int>, double>(zeroSpots[i],
                                                                            IncludeExcludeDiff(myState, zeroSpots[i]));
                    }

                    PartialState includeState = includeEdgeState(myState, lowestSpot.Item1);
                    PartialState excludeState = excludeEdgeState(myState, lowestSpot.Item1);

                    if (includeState.LowerBound < bssf.costOfRoute())
                    {
                        pq.insert(includeState);
                        numStatesCreated++;
                    }
                    else
                        numStatesPruned++;

                    if (excludeState.LowerBound < bssf.costOfRoute())
                    {
                        pq.insert(excludeState);
                        numStatesCreated++;
                    }
                    else
                        numStatesPruned++;
                }
            }

            stopwatch.Stop();

            // update the cost of the tour. 
            Program.MainForm.tbCostOfTour.Text = " " + bssf.costOfRoute();
            Program.MainForm.tbElapsedTime.Text = stopwatch.Elapsed.TotalSeconds.ToString();
            //Program.MainForm.tbStatesSaved.Text = maxNumStatesStored.ToString();
            //Program.MainForm.tbBSSFUpdates.Text = numBssfUpdates.ToString();
            //Program.MainForm.tbStatesCreated.Text = numStatesCreated.ToString();
            //Program.MainForm.tbStatesPruned.Text = numStatesPruned.ToString();

            // do a refresh. 
            Program.MainForm.Invalidate();

            Console.Out.WriteLine("Max # of States Stored: " + maxNumStatesStored.ToString());
            Console.Out.WriteLine("# of BSSF Updates: " + numBssfUpdates.ToString());
            Console.Out.WriteLine("Total # of States Created: " + numStatesCreated.ToString());
            Console.Out.WriteLine("Total # of States Pruned: " + numStatesPruned.ToString());
        }

        #endregion

        #region Popularity -> 2-opt
        // not narcississtic to me, makes it pretty and organized :)

        // integer greedy measure
        private double lengthOfGreedy()
        {
            int initial = 0;
            bool pathNotFound = true;
            HashSet<int> visited = new HashSet<int>();

            // loop through initial starting nodes for greedy path
            // until one of them finishes visiting all cities 
            // without hitting a dead end
            while (pathNotFound)
            {
                pathNotFound = false;
                Route = new ArrayList();
                City cursor = Cities[initial];
                Route.Add(Cities[initial]);
                visited.Add(initial);

                // starting with city at index `initial`
                // go to the nearest city and repeat
                // untill all cities have been visited
                while (visited.Count != Cities.Length)
                {
                    double dist = Double.PositiveInfinity;
                    int bestNeighbor = -1;

                    for (int i = 0; i < Cities.Length; i++)
                    {
                        if (!visited.Contains(i))
                        {
                            if (cursor.costToGetTo(Cities[i]) < dist)
                            {
                                dist = cursor.costToGetTo(Cities[i]);
                                bestNeighbor = i;
                            }
                        }
                    }

                    if (bestNeighbor == -1)
                    {
                        // no path available
                        // break and try a different start node
                        // (class spec says no backtracking)
                        pathNotFound = true;
                        break;
                    }
                    else
                    {
                        // found closest neighbor, go there
                        visited.Add(bestNeighbor);
                        Route.Add(Cities[bestNeighbor]);
                        cursor = Cities[bestNeighbor];
                    }

                }

                // if the last path failed to find all cities...
                // ...start wiht a different initial city
                initial++;
            }

            // assign as best solution so far
            bssf = new TSPSolution(Route);
            return bssf.costOfRoute();
        }

        // storage container for edge info
        private struct EdgeInfo
        {
            public int nodeA;
            public int nodeB;
            public double distance;
            public double score;

            public EdgeInfo(int a, int b, double d, double s)
            {
                nodeA = a;
                nodeB = b;
                distance = d;
                score = s;
            }
        }

        // storage unit for City popularity
        private struct CityInfo
        {
            public double averageOutboundEdgeLength;
            public int cityArrayIndex;
            public double popRanking;

            public CityInfo(int idx, double ave, double pop)
            {
                averageOutboundEdgeLength = ave;
                cityArrayIndex = idx;
                popRanking = pop;
            }
        }

        // sorter for edges by distance
        private List<EdgeInfo> sortEdgeInfoByDist(List<EdgeInfo> edges)
        {
            // base cases
            if (edges.Count <= 1)
            {
                return edges;
            }

            // recursive split and sort
            double pivotPoint = edges[edges.Count / 2].distance;
            List<EdgeInfo> lessThans = new List<EdgeInfo>();
            List<EdgeInfo> greaterThans = new List<EdgeInfo>();
            List<EdgeInfo> equalTos = new List<EdgeInfo>();

            foreach (EdgeInfo point in edges)
            {
                if (point.distance < pivotPoint)
                {
                    lessThans.Add(point);
                }
                else if (point.distance > pivotPoint)
                {
                    greaterThans.Add(point);
                }
                else
                {
                    equalTos.Add(point);
                }
            }

            lessThans = this.sortEdgeInfoByDist(lessThans);
            greaterThans = this.sortEdgeInfoByDist(greaterThans);

            List<EdgeInfo> total = new List<EdgeInfo>();
            total.AddRange(lessThans);
            total.AddRange(equalTos);
            total.AddRange(greaterThans);
            return total;
        }

        // sorter for edges by their popularity score
        private List<EdgeInfo> sortEdgeInfoByScore(List<EdgeInfo> edges)
        {
            // base cases
            if (edges.Count <= 1)
            {
                return edges;
            }

            // recursive split and sort
            double pivotPoint = edges[edges.Count / 2].score;
            List<EdgeInfo> lessThans = new List<EdgeInfo>();
            List<EdgeInfo> greaterThans = new List<EdgeInfo>();
            List<EdgeInfo> equalTos = new List<EdgeInfo>();

            foreach (EdgeInfo point in edges)
            {
                if (point.score < pivotPoint)
                {
                    lessThans.Add(point);
                }
                else if (point.score > pivotPoint)
                {
                    greaterThans.Add(point);
                }
                else
                {
                    equalTos.Add(point);
                }
            }

            lessThans = this.sortEdgeInfoByScore(lessThans);
            greaterThans = this.sortEdgeInfoByScore(greaterThans);

            // reorder from small to large
            List<EdgeInfo> total = new List<EdgeInfo>();
            total.AddRange(lessThans);
            total.AddRange(equalTos);
            total.AddRange(greaterThans);
            return total;
        }

        // sorter for cities by their average outbound edge length
        private List<CityInfo> sortCityInfoByAve(List<CityInfo> cities)
        {
            // base cases
            if (cities.Count <= 1)
            {
                return cities;
            }

            // recursive split and sort
            double pivotPoint = cities[cities.Count / 2].averageOutboundEdgeLength;
            List<CityInfo> lessThans = new List<CityInfo>();
            List<CityInfo> greaterThans = new List<CityInfo>();
            List<CityInfo> equalTos = new List<CityInfo>();

            foreach (CityInfo point in cities)
            {
                if (point.averageOutboundEdgeLength < pivotPoint)
                {
                    lessThans.Add(point);
                }
                else if (point.averageOutboundEdgeLength > pivotPoint)
                {
                    greaterThans.Add(point);
                }
                else
                {
                    equalTos.Add(point);
                }
            }

            lessThans = this.sortCityInfoByAve(lessThans);
            greaterThans = this.sortCityInfoByAve(greaterThans);

            // reorder from greatest to smallest
            List<CityInfo> total = new List<CityInfo>();
            total.AddRange(greaterThans);
            total.AddRange(equalTos);
            total.AddRange(lessThans);
            return total;
        }

        // sorter for cities by their original Cities array index
        private List<CityInfo> sortCityInfoByIndex(List<CityInfo> cities)
        {
            // base cases
            if (cities.Count <= 1)
            {
                return cities;
            }

            // recursive split and sort
            double pivotPoint = cities[cities.Count / 2].cityArrayIndex;
            List<CityInfo> lessThans = new List<CityInfo>();
            List<CityInfo> greaterThans = new List<CityInfo>();
            List<CityInfo> equalTos = new List<CityInfo>();

            foreach (CityInfo point in cities)
            {
                if (point.cityArrayIndex < pivotPoint)
                {
                    lessThans.Add(point);
                }
                else if (point.cityArrayIndex > pivotPoint)
                {
                    greaterThans.Add(point);
                }
                else
                {
                    equalTos.Add(point);
                }
            }

            lessThans = this.sortCityInfoByIndex(lessThans);
            greaterThans = this.sortCityInfoByIndex(greaterThans);

            // reorder from greatest to smallest
            List<CityInfo> total = new List<CityInfo>();
            total.AddRange(greaterThans);
            total.AddRange(equalTos);
            total.AddRange(lessThans);
            return total;
        }


        // Popularity -> 2-opt TSP solver
        public void solveByPopularityAnd2opt()
        {
            double timeLimit = 10 * 60 * 1000;
            //based on a sample of 20,50,100,200,300
            //.5 is a bit larger than necessary to cover varying time(could probably work with .25)
            double timeToMoveOn = 200D;//.000113*(Math.Pow(Cities.Length, 2)) + .0034 * Cities.Length + .5;
           
            Stopwatch clock0 = Stopwatch.StartNew(); //total func time

            // Dark Magick factors for picking the next popularity ratio
            // later, popularity will be determined by two things:
            //     - the actual length of the edge
            //     - the average of the lengths of the lower than average-lengthed edges 
            //       that are LEAVING the destination city of the edge in question
            //
            // Seems complex, but really isn't. Basically, does the city an edge
            // takes you to have a decent chance of having more short edges to
            // choose from? The problem is, the weights of those two numbers differ
            // depending on the spread of the cities. Prime country for a local minimum
            // is roughly (3 + sqrt(lengthOfGreedySolution/1000)):1, so we start there,
            // and re-run this algorithm focusing in on local minimums and jumping further
            // and further away from the other areas. This does cause some spread in total
            // run times, but it does work consistently to find shorter paths than nearest
            // neighbor greedy, and is quite unique :)
            int directionFactor = -1;
            double multFactor = 0.25D;
            double gNNlength = this.lengthOfGreedy();
            double dasDarkMagickFactor = (3D + (Math.Sqrt(gNNlength/1000D)));
            double lastBSSF = 0.0D;
            int failedTriesAtBSSF = 0;
            int forgivenessFactor = 50;

            // calculate popularity of each city (more pop == smaller average outgoing edges)
            List<CityInfo> cityPopularities = new List<CityInfo>();
            for (int i = 0; i < Cities.Length; i++)
            {
                // start by finding average length of outgoing edges
                double averageOutEdgeLen = 0.0D;
                int actualEdgeCount = 0;
                for (int j = 0; j < Cities.Length; j++)
                {
                    if (Cities[i].costToGetTo(Cities[j]) != Double.PositiveInfinity && Cities[i].costToGetTo(Cities[j]) != 0.0D)
                    {
                        averageOutEdgeLen += Cities[i].costToGetTo(Cities[j]);
                        actualEdgeCount++;
                    }
                }
                averageOutEdgeLen /= (double)actualEdgeCount;

                // now, find the average length of outgoing edges that are SHORTER 
                // than the average length of an outgoing edge
                // (basically, how short are the shorter edges this city has?)
                double averageOfSmalls = 0.0D;
                int actualSmalls = 0;
                for (int j = 0; j < Cities.Length; j++)
                {
                    if (Cities[i].costToGetTo(Cities[j]) != Double.PositiveInfinity && Cities[i].costToGetTo(Cities[j]) < averageOutEdgeLen)
                    {
                        averageOfSmalls += Cities[i].costToGetTo(Cities[j]);
                        actualSmalls++;
                    }
                }

                averageOfSmalls /= (double)actualSmalls;
                cityPopularities.Insert(cityPopularities.Count, new CityInfo(i, averageOfSmalls, averageOfSmalls));
            }

            // we're going to find many BSSFs, wanna save the overall
            // best so far here (using provided variables in this.* for
            // the best so far relative to inside this loop)
            //
            // Note that this requires this run until at least one BSSF is found, that will 
            // prevent 2-opt receiving a null object and crashing! (though this has not 
            // happened at least up through 1,000-city testing)
            TSPSolution overallBSSF = null;
            ArrayList overallBestRoute = new ArrayList();
            while (overallBSSF == null || (failedTriesAtBSSF < forgivenessFactor && (clock0.ElapsedMilliseconds / 1000 < timeToMoveOn))) 
            {
                // first, calculate value of all edges (existing or not)
                EdgeInfo[,] edgeScores = new EdgeInfo[Cities.Length, Cities.Length];
                for (int i = 0; i < Cities.Length; i++)
                {
                    for (int j = 0; j < Cities.Length; j++)
                    {
                        if (i == j)
                        {
                            // now that popularity is lower for better paths, this should
                            // probably not default to zero, but shouldn't have an impact
                            // on the actual results, since non-existant edges are pruned
                            // at path finding time. However, performance gains might be had
                            // by defaulting this score to inf instead of zero ???
                            edgeScores[i, j] = new EdgeInfo(i, j, Double.PositiveInfinity, Double.PositiveInfinity);
                        }
                        else
                        {
                            // popularity scored as follows: the lower, the better (meaning yielding a shorter overall path)
                            // use distance plus the average of shorter than average edges leaving destination city
                            // as the total popularity
                            edgeScores[i, j] = new EdgeInfo(i, j, Cities[i].costToGetTo(Cities[j]), Cities[i].costToGetTo(Cities[j]));
                            edgeScores[i, j].score += (cityPopularities[j].popRanking / dasDarkMagickFactor);
                            edgeScores[j, i] = new EdgeInfo(j, i, Cities[j].costToGetTo(Cities[i]), Cities[j].costToGetTo(Cities[i]));
                            edgeScores[j, i].score += (cityPopularities[i].popRanking / dasDarkMagickFactor);
                        }
                    }
                }

                // split up by city 
                // (leftover from my old approach, this could be removed
                // if the ending path choosing didn't rely on it...)
                List<EdgeInfo[]> edgesByNode = new List<EdgeInfo[]>();
                for (int i = 0; i < Cities.Length; i++)
                {
                    EdgeInfo[] nodeEdges = new EdgeInfo[Cities.Length];
                    for (int j = 0; j < Cities.Length; j++)
                    {
                        nodeEdges[j] = edgeScores[i, j];
                    }

                    // sort nodeEdges by their popularity scores
                    List<EdgeInfo> tmpList = new List<EdgeInfo>(nodeEdges);
                    tmpList = this.sortEdgeInfoByScore(tmpList);
                    nodeEdges = tmpList.ToArray();

                    edgesByNode.Insert(i, nodeEdges);
                }

                // select edges by their popularity until enough for path are needed
                List<EdgeInfo> path = new List<EdgeInfo>();
                int firstCitySeed = -1;
                int firstCity = firstCitySeed;
                int finalCity = 0;
                while (path.Count < Cities.Length - 1)
                {
                    // if this loop runs more than once, it was because the
                    // last starting city ended in a dead end somewhere
                    // so we start at a different city each time
                    // (very rare)
                    firstCitySeed++;
                    path = new List<EdgeInfo>();
                    int[] visitCounts = new int[Cities.Length];
                    for (int k = 0; k < visitCounts.Length; k++)
                    {
                        visitCounts[k] = 0;
                    }

                    int currCity = firstCitySeed;
                    int nextCity = 0;
                    firstCity = firstCitySeed;
                    finalCity = 0;
                    visitCounts[0] = 1;

                    while (path.Count < (Cities.Length - 1))
                    {
                        bool deadEndFound = true;
                        List<EdgeInfo> currNodeEdges = new List<EdgeInfo>(edgesByNode[currCity]);
                        for (int k = 0; k < currNodeEdges.Count; k++)
                        {
                            if (currNodeEdges[k].nodeA == currCity)
                            {
                                if (visitCounts[currNodeEdges[k].nodeB] == 0 && edgeScores[currNodeEdges[k].nodeA, currNodeEdges[k].nodeB].distance != Double.PositiveInfinity)
                                {
                                    nextCity = currNodeEdges[k].nodeB;

                                    visitCounts[nextCity] = 1;
                                    path.Add(currNodeEdges[k]);
                                    currCity = nextCity;
                                    finalCity = nextCity;
                                    deadEndFound = false;
                                    break;
                                }
                            }
                            //else //if (currNodeEdges[k].nodeB == currCity), which is already implied
                            //{
                            //    if (visitCounts[currNodeEdges[k].nodeA] == 0 && edgeScores[currNodeEdges[k].nodeB, currNodeEdges[k].nodeA].distance != Double.PositiveInfinity)
                            //    {
                            //        nextCity = currNodeEdges[k].nodeA;

                            //        visitCounts[nextCity] = 1;
                            //        path.Add(currNodeEdges[k]);
                            //        currCity = nextCity;
                            //        finalCity = nextCity;
                            //        deadEndFound = false;
                            //        break;
                            //    }
                            //}
                        }

                        if (deadEndFound)
                        {
                            // this log statement is necessary, otherwise MY compiler at least
                            // was omitting the break statement, which causes the algo to fail!
                            Console.WriteLine("Warning: Dead End Found (message needed for compiler)");
                            break;
                        }
                    }

                    if (path.Count < (Cities.Length - 1))
                    {
                        // this log statement is necessary, otherwise MY compiler at least
                        // was omitting the continue statement, which causes the algo to fail!
                        Console.WriteLine("Warning: Dead End Found (message needed for compiler)");
                        continue;
                    }

                    // This... is super ghetto. Adding the last city in such a way
                    // that we know an edge exists not only from n-1 to n, but also
                    // from n back to 1 (since the path has to be a cycle)
                    // Feel free to explore better ways of doing this.
                    EdgeInfo[] lastNodeEdges = edgesByNode[finalCity];
                    for (int i = 0; i < lastNodeEdges.Length; i++)
                    {
                        if (lastNodeEdges[i].nodeA == firstCity && lastNodeEdges[i].nodeB == finalCity)
                        {
                            path.Add(lastNodeEdges[i]);
                            break;
                        }
                        if (lastNodeEdges[i].nodeB == firstCity && lastNodeEdges[i].nodeA == finalCity)
                        {
                            path.Add(lastNodeEdges[i]);
                            break;
                        }
                    }

                }

                // load Route!
                Route = new ArrayList();
                int cityIdx = firstCitySeed;
                while (Route.Count < Cities.Length)
                {
                    // look for occurrence on i in the nodes
                    for (int pathIdx = 0; pathIdx < path.Count; pathIdx++)
                    {
                        if (path[pathIdx].nodeA == cityIdx)
                        {
                            Route.Add(Cities[cityIdx]);
                            cityIdx = path[pathIdx].nodeB;
                            path.RemoveAt(pathIdx);
                            break;
                        }
                        if (path[pathIdx].nodeB == cityIdx)
                        {
                            Route.Add(Cities[cityIdx]);
                            cityIdx = path[pathIdx].nodeA;
                            path.RemoveAt(pathIdx);
                            break;
                        }
                    }
                }
                bssf = new TSPSolution(Route);

                // one time through the popularity algortihm is complete. Now we compare
                // its performance against the previous run and determine what ratio
                // to use in the next popularity calculation.

                //Console.WriteLine("dark magick factor: {0}, length found: {1}", dasDarkMagickFactor,bssf.costOfRoute());
                if (overallBSSF == null || overallBSSF.costOfRoute() > bssf.costOfRoute())
                {
                    // A new best route was found!
                    // reset failed tries, our hope is renewed!
                    // lower the multiply factor, we shoudl explore around this find
                    // to see if it has a close buddy who's even shorter!
                    overallBSSF = bssf;
                    overallBestRoute = Route;
                    Console.WriteLine("Popularity found a better solution: {0} at: {1}",overallBSSF.costOfRoute(), clock0.Elapsed.TotalSeconds);
                    multFactor = 0.25D;
                    failedTriesAtBSSF = 0;
                }
                else if (lastBSSF == bssf.costOfRoute())
                {
                    // hmmm, we're stuck in a plateau 
                    // (path lengths tend to be the same for long stretches of
                    // a dasDarkMagickFactor value)
                    // up the multiply factor to FFWD out of it
                    // and give up less hope since plateaus are normal
                    multFactor *= 2D;
                    directionFactor *= -1;
                    failedTriesAtBSSF++; // punish less for being stuck in plateau
                    if (Math.Abs(dasDarkMagickFactor) > 500D)
                    {
                        // outside this range rarely finds any paths of value
                        // re-center-ish (coming back to the same spot
                        // each time doesn't allow for good exploration)
                        // turn BACK around to face original direction
                        // and slow back down
                        directionFactor *= -1;
                        multFactor = 2D;
                        dasDarkMagickFactor -= (500D * (Math.Abs(dasDarkMagickFactor)/dasDarkMagickFactor));
                        failedTriesAtBSSF--;
                    }
                }
                else if(lastBSSF < bssf.costOfRoute())
                {
                    // whoa, this once is worse than the last!
                    // let's start FFWDing faster!
                    // jump back the other way!
                    // hope drains quickly!
                    multFactor *= 2D;
                    directionFactor *= -1;
                    failedTriesAtBSSF += 2;
                }
                else
                {
                    // we did better than last time, but so what?
                    // keep plodding along at same pace, maybe
                    // we'll get even better soon
                    // hope drains away though
                    failedTriesAtBSSF += 2;
                    directionFactor *= -1;
                }

                // adjust the ratio more strongly if the gap ebtween our answer and greedy's widens
                // this helps find better solutions when the first one is worse than greedy
                // which is typical.
                // then, direcitons goes left/right on -/+ number scale, and multFactor
                // controls our speed along said number line
                dasDarkMagickFactor -= directionFactor * (multFactor * (bssf.costOfRoute() / gNNlength));
                lastBSSF = bssf.costOfRoute();
            }
            double priorityTime = clock0.ElapsedMilliseconds;
            // we gave up hope, the best we got are now our BSSF and Route
            // which will be put through 2-opt for refining
            bssf = overallBSSF;
            Route = overallBestRoute;
            double costToBeat = bssf.costOfRoute();
            Console.WriteLine("initial length: {0} at time: {1}", costToBeat, clock0.Elapsed.TotalSeconds);

            // keep doing 2-opt over and over until it has no effect!
            bool changesMade = true;
            TSPSolution tempBssf = bssf;
            while (changesMade && clock0.ElapsedMilliseconds < timeLimit)
            {
                changesMade = false;

                // for each valid starting point of a reversible segment...
                for (int i = 0; i < Route.Count; i++)
                {
                    double currSeqCost = 0.0D;
                    double revSeqCost = 0.0D;

                    // for each valid ending point of a reversible segment...
                    for (int j = i + 3; j < Route.Count + i; j++)
                    {
                        // cut off 2-opt if it surpasses the 10min mark
                        // added here since at 500 cities, the outer while loop
                        // takes roughly 80 seconds to run, too much overshot of
                        // the "reasonable time" mark!
                        if (clock0.ElapsedMilliseconds > timeLimit)
                        {
                            i = Route.Count;
                            changesMade = false;
                            break;
                        }

                        // find the cost of original sequence versus cost of reversed
                        // if a effective swap wasn't found last time, then the addition of the edges
                        // were cached, and we can just add to them!
                        if (currSeqCost == 0D)
                        {
                            for (int k = i; k < j; k++)
                            {
                                currSeqCost += (Route[(k % Route.Count)] as City).costToGetTo((Route[(k + 1) % Route.Count] as City));
                            }
                        }
                        else
                        {
                            currSeqCost += (Route[((j - 1) % Route.Count)] as City).costToGetTo((Route[(j % Route.Count)]) as City);
                        }

                        // note that finding cost of reversed sequence is a little harder
                        // C# may provide some safety with addition of PositiveInfinity, 
                        // but I wasn't willing to risk it (obviously, look at this mess!)
                        // Again, results were cached from unsuccessful reversals
                        if (revSeqCost == 0D)
                        {
                            revSeqCost = (Route[i] as City).costToGetTo((Route[(j - 1) % Route.Count] as City));
                            for (int k = j - 1; k > i + 1; k--)
                            {
                                revSeqCost += (Route[k % Route.Count] as City).costToGetTo((Route[(k - 1) % Route.Count] as City));
                                if (revSeqCost == double.PositiveInfinity)
                                    break;
                            }
                            revSeqCost += (Route[(i + 1) % Route.Count] as City).costToGetTo((Route[(j % Route.Count)] as City));
                        }
                        else
                        {
                            // remove the old edges connecting the subsequence to the rest of the route
                            revSeqCost -= (Route[(i + 1) % Route.Count] as City).costToGetTo((Route[((j-1) % Route.Count)] as City)); 
                            revSeqCost -= (Route[i] as City).costToGetTo((Route[(j - 2) % Route.Count] as City)); 

                            // add the new edges replacing ^those, connecting the subsequence to the rest of the route
                            revSeqCost += (Route[(i + 1) % Route.Count] as City).costToGetTo((Route[(j % Route.Count)] as City));
                            revSeqCost += (Route[i] as City).costToGetTo((Route[(j - 1) % Route.Count] as City));

                            // add the new "backward" edge for the new node added to the subsequence
                            revSeqCost += (Route[(j-1) % Route.Count] as City).costToGetTo((Route[(j-2) % Route.Count] as City)); 
                        }

                        // if the cost of the reversal is less than the original subsequence, 
                        // then we'll reverse it and calculate a new BSSF!
                        if (revSeqCost < currSeqCost && revSeqCost != Double.PositiveInfinity && j < Route.Count)
                        {
                            Route.Reverse(i + 1, j - i - 1);
                            tempBssf = new TSPSolution(Route);
                            if (tempBssf.costOfRoute() < bssf.costOfRoute())
                            {
                                bssf = new TSPSolution(Route);
                                costToBeat = bssf.costOfRoute();
                                changesMade = true; // since we improved, 2-opt has to run at least once more
                                currSeqCost = 0D;
                                revSeqCost = 0D;
                                //Console.WriteLine("2-opt improved to: {0}", costToBeat);
                            }
                        }//cycle reverse
                        else if (revSeqCost < currSeqCost && revSeqCost != Double.PositiveInfinity && j > Route.Count)
                        {
                            ArrayList tempList = new ArrayList();
                            if (i != Route.Count - 1)
                            {
                                tempList = new ArrayList(Route.GetRange(i + 1, Route.Count - i - 1));
                                Route.RemoveRange(i + 1, Route.Count - i - 1);
                                for (int k = 0; k < tempList.Count; k++)
                                {
                                    Route.Insert(k, tempList[k]);
                                }
                            }
                            Route.Reverse(0, j - i - 1);
                            tempBssf = new TSPSolution(Route);
                            if (tempBssf.costOfRoute() < bssf.costOfRoute())
                            {
                                bssf = new TSPSolution(Route);
                                costToBeat = bssf.costOfRoute();
                                changesMade = true;
                                currSeqCost = 0D;
                                revSeqCost = 0D;
                                //Console.WriteLine("2-opt improved to: {0}", costToBeat);
                            }
                        }
                        
                    }
                }
            }

            clock0.Stop();
            
            // update the cost of the tour. 
            Program.MainForm.tbCostOfTour.Text = " " + bssf.costOfRoute();
            Program.MainForm.tbElapsedTime.Text = " " + clock0.Elapsed.TotalSeconds + " s";
            // do a refresh. 
            Program.MainForm.Invalidate();            
        }

        #endregion


    }

}
