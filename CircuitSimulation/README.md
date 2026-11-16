A SPICE-style DC circuit simulator written in C#. It reads a netlist, builds the circuit equations using modified nodal analysis, and solves for the voltage at each node and the current through each voltage source.

*Example* 

circuit.txt:
V1 in 0 5
R1 in out 1k
R2 out 0 2k

Output:
V(in) = 5.0000 V
V(out) = 3.3333 V
I(V1) = -0.001667 A

*Supported Components*

Resistors (R)
Independent voltage sources (V)
Independent current sources (I)

*Built Using*

C#, .NET, Math.NET Numerics

*Planned features*

AC Analysis - At the moment the simulator only handles DC circuits. AC analysis will add capacitors and inductors and solve the circuit at a given frequency using complex numbers so each component has an impedance (Z = 1/jωC for a capacitor, Z = jωL for an inductor). A frequency sweep will then show how gain and phase change with frequency which is how circuits like RC filters are normally analysed.

Frequency response output - Results from the AC sweep will be exported or plotted as a Bode plot (gain in dB and phase against frequency) so filter cutoff frequencies can be read off directly. I will check these against hand calculations, for example an RC low-pass filter with f_c = 1/(2πRC).

Possible additions - Diodes and other non-linear components included and the addition of an intuitive, simple interface using winforms





