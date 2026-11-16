using MathNet.Numerics.LinearAlgebra;   
using System.Globalization;            

//maps each point in the circuit (e.g. "in", "out") to a row/column number in the grid
var nodes = new Dictionary<string, int>();

//every component we read from the file: type (R/V/I), name, the two points it connects, and its value
var parts = new List<(char type, string name, int a, int b, double val)>();

//turns a point name into a grid number, giving new points the next free number
int Node(string s)
{
    //ground is 0 V by definition, so it doesn't get a row in the grid. -1 marks it
    if (s == "0") return -1;

    //first time we've seen this point, so give it the next free number
    if (!nodes.ContainsKey(s)) nodes[s] = nodes.Count;

    return nodes[s];
}

//converts text like "1k" or "4.7u" into a real number (1000 or 0.0000047)
double ParseValue(string s)
{
    s = s.ToLower();   

    //suffixes and their multipliers. "meg" must come before "m",
    //otherwise "1meg" would be read as 1 milli
    var suffixes = new (string, double)[] {
        ("meg", 1e6), ("k", 1e3), ("m", 1e-3), ("u", 1e-6), ("n", 1e-9), ("p", 1e-12) };

    foreach (var (suf, mult) in suffixes)
        if (s.EndsWith(suf))
            //strip the suffix off, parse the number part, then multiply
            return double.Parse(s[..^suf.Length], CultureInfo.InvariantCulture) * mult;

    return double.Parse(s, CultureInfo.InvariantCulture);
}

//STEP 1: read the circuit file and store each component
foreach (var line in File.ReadAllLines("circuit.txt"))
{
    //skip blank lines and comment lines (starting with *)
    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("*")) continue;

    //split e.g. "R1 in out 1k" into ["R1", "in", "out", "1k"]
    var p = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

    //first letter gives the type, p[1] and p[2] are the two points it connects, p[3] is the value
    parts.Add((char.ToUpper(p[0][0]), p[0], Node(p[1]), Node(p[2]), ParseValue(p[3])));
}

//STEP 2: build the empty grid (A) and the answers column (z)
int n = nodes.Count;                         //number of points whose voltage we need to find
int m = parts.Count(x => x.type == 'V');     //number of voltage sources(each adds an unknown current)

//grid needs one row/column per point, plus one per voltage source
var A = Matrix<double>.Build.Dense(n + m, n + m);
var z = Vector<double>.Build.Dense(n + m);

//STEP 3: fill the grid. Each component adds its own numbers
int vIndex = 0;   // counts which voltage source we're on
foreach (var (type, name, a, b, val) in parts)
{
    if (type == 'R')
    {
        //a resistor passes more current the lower its resistance, so we use conductance g = 1/R
        double g = 1 / val;

        //add g on the diagonal for each end of the resistor (skipping ground)
        if (a >= 0) A[a, a] += g;
        if (b >= 0) A[b, b] += g;

        //subtract g in the off-diagonal spots, which links the two ends together
        if (a >= 0 && b >= 0) { A[a, b] -= g; A[b, a] -= g; }
    }
    else if (type == 'I')
    {
        //current source: current leaves point a and enters point b
        //so point a loses current and point b gains it
        if (a >= 0) z[a] -= val;
        if (b >= 0) z[b] += val;
    }
    else if (type == 'V')
    {
        //voltage sources get their own row/column after the points, at position k
        int k = n + vIndex++;

        //these 1s link the source's unknown current to the points it's connected to
        if (a >= 0) { A[a, k] += 1; A[k, a] += 1; }
        if (b >= 0) { A[b, k] -= 1; A[k, b] -= 1; }

        //this row says: voltage at a minus voltage at b equals the source's value
        z[k] = val;
    }
}

//STEP 4: solve the equations and print the results
var x = A.Solve(z);   // x now holds every point's voltage, then every source's current

//print the voltage at each point
foreach (var kv in nodes)
    Console.WriteLine($"V({kv.Key}) = {x[kv.Value]:F4} V");

//print the current through each voltage source (they sit after the voltages in x)
vIndex = 0;
foreach (var p in parts.Where(p => p.type == 'V'))
    Console.WriteLine($"I({p.name}) = {x[n + vIndex++]:F6} A");