using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMining.Graphs
{

    public class AdaptiveBetweennessCentrality
    {
        public static double eps = 1e-8;
        //typedef std::vector<std::vector<int> > g_t;
        //typedef std::vector<int> he_t;
        //typedef std::vector<he_t> hg_t;
        //typedef std::vector<std::pair<int, double> > whe_t;
        //typedef std::vector<whe_t> whg_t;

        /*********/
        /*  BFS  */
        /*********/

        static void bfs(LightWeightGraph G, int s, int[] dists, int[] nums)
        {
            int V = G.Nodes.Count();
            int qh = 0, qt = 0;
            int[] q = new int[V];

            //dists = new int[V];  // initialize with -1
            for (int i = 0; i < V; i++)
            {
                dists[i] = -1;
            }
            //nums = new int[V];
            for (int i = 0; i < V; i++)
            {
                nums[i] = 0;
            }
            q[qt++] = s;
            dists[s] = 0;
            nums[s] = 1;
            while (qh != qt)
            {
                int u = q[qh++];
                for (int i = 0; i < G.Nodes[u].Edge.Count(); i++)
                {
                    int v = G.Nodes[u].Edge[i];

                    if (dists[v] == -1 || dists[v] == dists[u] + 1)
                    {
                        nums[v] += nums[u];
                    }
                    if (dists[v] == -1)
                    {
                        dists[v] = dists[u] + 1;
                        q[qt++] = v;
                    }
                } //for loop ends here
            }
        } // end bfs

        static void bfs(LightWeightGraph G, int s, Dictionary<int, int> dists, Dictionary<int, int> nums, HashSet<int> alive)
        {
            int qh = 0;
            List<int> q = new List<int>();

            // dists = new Dictionary<int, int>();
            // nums = new Dictionary<int, int>();
            dists.Clear();
            nums.Clear();

            if (!alive.Contains(s)) return;

            q.Add(s);
            dists[s] = 0;
            nums[s] = 1;
            int myValue;
            while (qh != (int)q.Count())
            {
                int u = q[qh++];
                for (int i = 0; i < G.Nodes[u].Edge.Count(); i++)
                {
                    int v = G.Nodes[u].Edge[i];
                    if (!alive.Contains(v)) continue;

                    if (!dists.TryGetValue(v, out myValue) || dists[v] == dists[u] + 1)
                    {
                        //nums[v] += nums[u];
                        nums.TryGetValue(u, out myValue);
                        try { nums.Add(v, myValue);
                        }
                        catch(System.ArgumentException)
                        {
                            int myValue2;
                            nums.TryGetValue(v, out myValue2);
                            nums[v] = myValue + myValue2;
                        }
                        
                    }
                    if (!dists.TryGetValue(v, out myValue))
                    {
                        dists[v] = dists[u] + 1;
                        q.Add(v);
                    }
                }
            }
        }


        static void restricted_bfs(LightWeightGraph G, int s, Dictionary<int, int> baseline_dists, Dictionary<int, int> nums, bool[] is_seed, HashSet<int> domain)
        {
            int qh = 0;
            List<int> q = new List<int>();
            //baseline_dists = new Dictionary<int, int>();
            Dictionary<int, int> dists = new Dictionary<int, int>();
            //nums = new Dictionary<int, int>();
            int myValue;

            if (baseline_dists.TryGetValue(s, out myValue) && baseline_dists[s] != 0) return;
            if (!domain.Contains(s)) return;

            q.Add(s);
            dists[s] = 0;
            nums[s] = 1;
            while (qh != (int)q.Count())
            {
                int u = q[qh++];
                for (int i = 0; i < G.Nodes[u].Edge.Count(); i++)
                {
                    int v = G.Nodes[u].Edge[i];
                    if (!domain.Contains(v) || is_seed[v]) continue;
                    if (baseline_dists[v] != baseline_dists[u] + 1) continue;

                    if (!dists.TryGetValue(v, out myValue) || dists[v] == dists[u] + 1)
                    {
                        //nums[v] += nums[u];
                        nums.TryGetValue(u, out myValue);
                        try
                        {
                            nums.Add(v, myValue);
                        }
                        catch (System.ArgumentException)
                        {
                            int myValue2;
                            nums.TryGetValue(v, out myValue2);
                            nums[v] = myValue + myValue2;
                        }
                    }
                    if (!dists.TryGetValue(v, out myValue))
                    {
                        dists[v] = dists[u] + 1;
                        q.Add(v);
                    }
                }
            }
        }


        static void restricted_bfs(LightWeightGraph G, int s, int[] baseline_dists, int[] nums, bool[] is_seed)
        {
            int V = G.Nodes.Count();
            int qh = 0, qt = 0;
            List<int> q = new List<int>(V);
            for (int i = 0; i< V; i++) {
                q.Add(0);
            }


            int[] dists = new int[V];
            for (int i = 0; i < dists.Length; i++)
            {
                dists[i] = -1;
            }
            //nums = new int[V];

            if (baseline_dists[s] != 0) return;

            q[qt++] = s;
            dists[s] = 0;
            nums[s] = 1;
            while (qh != qt)
            {
                int u = q[qh++];
                for (int i = 0; i < G.Nodes[u].Edge.Count(); i++)
                {
                    int v = G.Nodes[u].Edge[i];
                    if (is_seed[v]) continue;
                    if (baseline_dists[v] != baseline_dists[u] + 1) continue;

                    if (dists[v] == -1 || dists[v] == dists[u] + 1)
                    {
                        nums[v] += nums[u];
                    }
                    if (dists[v] == -1)
                    {
                        dists[v] = dists[u] + 1;
                        q[qt++] = v;
                    }
                }
            }
        }

        /**************************/
        /*  Building Hypergraphs  */
        /**************************/


        static void build_betweenness_hypergraph(LightWeightGraph G, List<List<Tuple<int, double>>> H, int M, List<int> seeds, List<Tuple<int, int>> pairs = null)
        {
            Random r = new Random();
            int V = G.Nodes.Count();
            bool[] is_seed = new bool[V];
            for (int i = 0; i < seeds.Count; i++) is_seed[seeds[i]] = true;

            for (int j = 0; j < M; j++)
            {
                int s = r.Next(0, V), t = r.Next(0, V);
                //if (pairs) pairs->push_back(std::make_pair(s, t));
                if (pairs != null)
                {
                    pairs.Add(new Tuple<int, int>(s, t));
                }
                int[] dists = new int[V];
                int[] nums = new int[V];
                int[] nums_with_seeds = new int[V];
                bfs(G, s, dists, nums);

                restricted_bfs(G, s, dists, nums_with_seeds, is_seed);

                int qh = 0, qt = 0;
                int[] q = new int[V]; // std::vector<int> q(V);
                bool[] added = new bool[V]; //  std::vector<bool> added(V);
                double[] btws = new double[V]; // std::vector<double> btws(V);
                List<Tuple<int, double>> whe = new List<Tuple<int, double>>();
                //std::vector<std::pair<int, double>> whe;
                //whe_t whe;
                q[qt++] = t;
                added[t] = true;

                while (qh != qt)
                {
                    int u = q[qh++];
                    if (u == s) continue;

                    for (int i = 0; i < G.Nodes[u].Edge.Count(); i++)
                    {  //rep(i, G[u].size()) {
                        int v = G.Nodes[u].Edge[i];  //int v = G[u][i];
                        if (dists[v] == dists[u] - 1)
                        {
                            if (added[v] == false)
                            {
                                q[qt++] = v;
                                added[v] = true;
                            }
                        }
                        else if (dists[v] == dists[u] + 1)
                        {
                            double k = 0;
                            k += (double)nums_with_seeds[u] / nums[v];
                            if (nums_with_seeds[v] != 0)
                            {
                                if (!is_seed[v])
                                {
                                    k += btws[v] / nums_with_seeds[v] * nums_with_seeds[u];
                                }
                            }
                            btws[u] += k;
                        }
                    }
                    whe.Add(new Tuple<int, double>(u, btws[u]));
                    //whe.push_back(std::make_pair(u, btws[u]));
                }
                H.Add(whe); // H.push_back(whe);
            }
        }

        /*************************/
        /*  Betweenness  */
        /*************************/


        public static void exact_betweenness(LightWeightGraph G, List<int> seeds, List<double> wbtws)
        {
            int V = G.Nodes.Count(); //int V = G.size();
            for (int i = 0; i < V; i++) { wbtws.Add(0); }
            bool[] is_seed = new bool[V];
            
            for (int i = 0; i < seeds.Count; i++) is_seed[seeds[i]] = true; //.Length
            //rep(i, seeds.size()) is_seed[seeds[i]] = true;

            //wbtws = new double[V];

            for (int s = 0; s < V; s++)
            {
                //rep(s, V) {
                double[] btws = new double[V];

                int[] dists = new int[V];
                int[] nums = new int[V];
                int[] nums_with_seeds = new int[V];
                bfs(G, s, dists, nums);

                restricted_bfs(G, s, dists, nums_with_seeds, is_seed);

                int[] out_degree = new int[V];
                for (int u = 0; u < V; u++)
                {
                    //rep(u, V) {
                    for (int i = 0; i < G.Nodes[u].Edge.Count(); i++)
                    {
                        //rep(i, G[u].size()) {
                        int v = G.Nodes[u].Edge[i]; //int v = G[u][i];
                        if (dists[v] == dists[u] + 1) out_degree[u]++;
                    }
                }

                int qh = 0, qt = 0;
                int[] q = new int[V];
                for (int u = 0; u < V; u++)
                {
                    //rep(u, V)
                    if (out_degree[u] == 0)
                        q[qt++] = u;
                }

                while (qh != qt)
                {
                    int u = q[qh++];
                    if (u == s) continue;

                    //rep(i, G[u].size()) {
                    for (int i = 0; i < G.Nodes[u].Edge.Count(); i++)
                    {
                        int v = G.Nodes[u].Edge[i]; //int v = G[u][i];
                        if (dists[v] == dists[u] - 1)
                        {
                            if (--out_degree[v] == 0) q[qt++] = v;
                        }
                        else if (dists[v] == dists[u] + 1)
                        {
                            double k = 0;
                            k += (double)nums_with_seeds[u] / nums[v];
                            if (nums_with_seeds[v] != 0)
                            {
                                if (!is_seed[v])
                                {
                                    k += btws[v] / nums_with_seeds[v] * nums_with_seeds[u];
                                }
                            }
                            btws[u] += k;
                        }
                    }
                }
                for (int u = 0; u < V; u++)
                {
                    //rep(u, V) {
                    if (is_seed[u]) continue;
                    wbtws[u] += btws[u];
                }
            }
        }

        public static void approximate_betweenness(LightWeightGraph G, int M, List<int> seeds, List<double> btws)
        {
            
            int V = G.Nodes.Count(); //int V = G.size();
            for (int i = 0; i < V; i++) { btws.Add(0); }
                List<List<Tuple<int, double>>> H = new List<List<Tuple<int, double>>>(); // whg_t H;
            build_betweenness_hypergraph(G, H, M, seeds);

            //btws = new double[V];
            for (int i = 0; i < H.Count; i++)
            {
                //rep(i, H.size()) {
                List<Tuple<int, double>> whe = H[i]; // whe_t & whe = H[i];
                for (int j = 0; j < whe.Count; j++)
                {
                    //rep(j, whe.size()) {
                    int v = whe[j].Item1;
                    btws[v] += whe[j].Item2;
                }
            }
        }

        public static List<Tuple<int, double>> make_hyperedge(LightWeightGraph G, int s, int t, bool[] is_seed, HashSet<int> vertices_in_whe)
        {
            List<Tuple<int, double>> whe = new List<Tuple<int, double>>(); //whe_t whe;
            Dictionary<int, double> btws = new Dictionary<int, double>();
            Dictionary<int, int> dists = new Dictionary<int, int>();
            Dictionary<int, int> nums = new Dictionary<int, int>();
            Dictionary<int, int> nums_with_seeds = new Dictionary<int, int>();
            bfs(G, s, dists, nums, vertices_in_whe);

            restricted_bfs(G, s, dists, nums_with_seeds, is_seed, vertices_in_whe);

            HashSet<int> added = new HashSet<int>();
            int qh = 0;
            List<int> q = new List<int>();
            q.Add(t);
            while (qh != (int)q.Count())
            {
                int u = q[qh++];
                if (u == s) continue;

                for (int i = 0; i < G.Nodes[u].Edge.Count(); i++)
                {
                    //rep(i, G[u].size()) {
                    int v = G.Nodes[u].Edge[i];
                    if (!vertices_in_whe.Contains(v)) continue; // if (!vertices_in_whe.count(v)) continue;
                    if (!dists.ContainsKey(v)) continue; //if (!dists.count(v)) continue;

                    if (dists[v] == dists[u] - 1)
                    {
                        if (!added.Contains(v)) //if (!added.count(v))
                        {
                            q.Add(v);
                            added.Add(v);
                        }
                    }
                    else if (dists[v] == dists[u] + 1)
                    {
                        double k = 0;
                        int myValue;
                        bool exists = nums_with_seeds.TryGetValue(u, out myValue);
                        if (exists)
                        {
                            k += (double)nums_with_seeds[u] / nums[v];
                        }
                        else
                        {
                            nums_with_seeds.Add(u, 0);
                            k += (double)nums_with_seeds[u] / nums[v];
                        }
                        if (!nums_with_seeds.TryGetValue(v, out myValue))
                        {
                            nums_with_seeds.Add(v, 0);
                        }
                        if (nums_with_seeds[v] != 0)
                        {
                            if (!is_seed[v])
                            {
                                double myValue3;
                                if (!btws.TryGetValue(v, out myValue3))
                                {
                                    btws.Add(v, 0);
                                }
                                k += btws[v] / nums_with_seeds[v] * nums_with_seeds[u];
                            }
                        }
                        //btws[u] += k;
                        double myValue2;
                        btws.TryGetValue(u, out myValue2);
                        try
                        {
                            btws.Add(u, myValue2);
                        }
                        catch (System.ArgumentException)
                        {
                            
                            btws[u] = myValue2 + k;
                        }
                    }
                }
            }
            foreach (int itm in added) 
            //for (auto it = added.begin(); it != added.end(); ++it)
            {
                int u = itm;
                //whe.push_back(std::make_pair(u, btws[*it]));
                double myValue;
                btws.TryGetValue(u, out myValue);
                //whe.Add(new Tuple<int, double>(u, btws[u]));
                whe.Add(new Tuple<int, double>(u, myValue));
            }
            return whe;
        }


        public static void adaptive_approximate_betweenness(LightWeightGraph G, int M, int k, List<int> seeds, List<double> btwss)
        {
            int V = G.Nodes.Count(); //int V = G.size();
            bool[] is_seed = new bool[V];
            seeds.Clear();
            btwss.Clear();

            List<List<Tuple<int, double>>> H = new List<List<Tuple<int, double>>>(); //whg_t H;
            List<Tuple<int, int>> pairs = new List<Tuple<int, int>>();
            build_betweenness_hypergraph(G, H, M, seeds, pairs);

            // preprocess using H
            double[] degrees = new double[V]; // std::vector<double> degrees(V);
            //std::vector<std::vector<int>> vertex_to_heids(V);
            List<List<int>> vertex_to_heids = new List<List<int>>(V);
            for (int i = 0; i< V; i++)
            {
                vertex_to_heids.Add(new List<int>());
            }
            //std::vector<std::unordered_set<int>> vertices_in_whes(H.size());
            List<HashSet<int>> vertices_in_whes = new List<HashSet<int>>(H.Count); 
            for (int i = 0; i < H.Count; i++)
            {
                vertices_in_whes.Add(new HashSet<int>());
            }
            for (int i = 0; i < H.Count; i++) {
                //rep(i, H.size()) {
                List<Tuple<int, double>> whe = H[i]; // whe_t & whe = H[i];whe_t & whe = H[i];
                for ( int j =0; j<whe.Count; j++) { 
                //rep(j, whe.size()) {
                    int v = whe[j].Item1;
                    degrees[v] += whe[j].Item2;
                    vertex_to_heids[v].Add(i);
                    vertices_in_whes[i].Add(v);
                }
                vertices_in_whes[i].Add(pairs[i].Item1);
                vertices_in_whes[i].Add(pairs[i].Item2);
            }

            PriorityQueue<Tuple<double, int>> pq = new PriorityQueue<Tuple<double, int>>();
            for (int u = 0; u < V; u++) pq.push(new Tuple<double, int>(degrees[u], u));
            //rep(u, V) pq.push(std::make_pair(degrees[u], u));
            double[] current_degrees = new double[degrees.Length];
            for (int i =0; i < current_degrees.Length; i++)
            {
                current_degrees[i] = degrees[i];
            }
            //std::vector<double> current_degrees = degrees;
            //std::vector<bool> vertex_done(V);
            bool[] vertex_done = new bool[V];

            //while (!pq.empty() && (int)seeds.size() < k)
            while (pq.Count > 0 && (int) seeds.Count < k)
            {
                Console.WriteLine("seeds.cont = " + seeds.Count);
                double weight = pq.top().Item1;
                int u = pq.top().Item2;
                pq.pop();
                if (vertex_done[u]) continue;
                if (Math.Abs(weight - current_degrees[u]) > eps) continue;
                vertex_done[u] = true;

                seeds.Add(u);
                is_seed[u] = true;
                btwss.Add(weight);

                for (int i = 0; i < vertex_to_heids[u].Count; i++) { 
                //rep(i, vertex_to_heids[u].size()) {
                    int heid = vertex_to_heids[u][i];
                    List<Tuple<int, double>> whe = H[heid]; // whe_t & whe = H[heid];
                    //whe_t new_whe = make_hyperedge(G, pairs[heid].first, pairs[heid].second, is_seed, vertices_in_whes[heid]);
                    List<Tuple<int, double>> new_whe = make_hyperedge(G, pairs[heid].Item1, pairs[heid].Item2, is_seed, vertices_in_whes[heid]);
                    SortedDictionary<int, int> make_sure = new SortedDictionary<int, int>();
                    for (int j=0; j< new_whe.Count; j++) make_sure[new_whe[j].Item1] = j;
                    //rep(j, new_whe.size()) make_sure[new_whe[j].first] = j;

                    for (int j = 0; j < whe.Count; j++) { 
                    //rep(j, whe.size()) {
                        int v = whe[j].Item1;
                        if (vertex_done[v]) continue;
                        int myValue;
                        if (!make_sure.TryGetValue(v, out myValue))
                        {
                            make_sure.Add(v, 0);
                        }
                        if (whe[j].Item2 != new_whe[make_sure[v]].Item2)
                        {
                            current_degrees[v] -= whe[j].Item2;
                            current_degrees[v] += new_whe[make_sure[v]].Item2;
                            whe[j] = new Tuple<int, double>(v, new_whe[make_sure[v]].Item2);
                            //whe[j].Item2 = new_whe[make_sure[v]].Item2;
                        }
                        pq.push(new Tuple<double, int>(current_degrees[v], v));
                    }
                }
            }
        }


    } // end class AdaptiveBetweennessCentrality
} // end namespace
