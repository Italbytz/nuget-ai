using System;
using System.Collections.Generic;
using System.Linq;

namespace Italbytz.AI.Probability.Demo;

/// <summary>
/// The 4×3 stochastic grid world from AIMA3e Fig. 17.1 and 17.3.
/// States are "row,col" strings (1-indexed). Cell (2,2) is the obstacle/wall.
/// Terminal states: (1,4) reward +1, (2,4) reward -1.
/// All other states: reward -0.04. Discount factor γ = 0.95.
/// Actions: Up/Down/Left/Right.
/// Stochastic movement: 0.8 intended direction, 0.1 each perpendicular direction.
/// </summary>
public class GridWorldMdp : IMarkovDecisionProcess<string, string>
{
    private const int Rows = 3;
    private const int Cols = 4;
    private static readonly string Wall = "wall";
    private static readonly string GoalPos = "1,4";
    private static readonly string PitPos = "2,4";

    private readonly List<string> _states;
    private static readonly string[] AllActions = { "Up", "Down", "Left", "Right" };

    public IReadOnlyList<string> States => _states;
    public double Discount => 0.95;

    public GridWorldMdp()
    {
        _states = new List<string>();
        for (int r = 1; r <= Rows; r++)
            for (int c = 1; c <= Cols; c++)
                if (!(r == 2 && c == 2))  // skip wall
                    _states.Add($"{r},{c}");
    }

    public IReadOnlyList<string> Actions(string state)
    {
        if (state == GoalPos || state == PitPos) return Array.Empty<string>();
        return AllActions.ToList();
    }

    public double Reward(string state) => state switch
    {
        "1,4" =>  1.0,
        "2,4" => -1.0,
        _     => -0.04
    };

    public double Transition(string s, string a, string sPrime)
    {
        if (s == GoalPos || s == PitPos) return 0;

        var (r, c) = Parse(s);
        // (dr,dc) for Up/Down/Left/Right
        var intended = GetDelta(a);
        var perp = GetPerpendicular(a);

        double prob = 0;
        prob += 0.8 * (Destination(r, c, intended) == sPrime ? 1 : 0);
        prob += 0.1 * (Destination(r, c, perp.left) == sPrime ? 1 : 0);
        prob += 0.1 * (Destination(r, c, perp.right) == sPrime ? 1 : 0);
        return prob;
    }

    private string Destination(int r, int c, (int dr, int dc) delta)
    {
        int nr = r + delta.dr;
        int nc = c + delta.dc;
        if (nr < 1 || nr > Rows || nc < 1 || nc > Cols || (nr == 2 && nc == 2))
            return $"{r},{c}";  // bump back
        return $"{nr},{nc}";
    }

    private static (int dr, int dc) GetDelta(string action) => action switch
    {
        "Up"    => (-1,  0),
        "Down"  => ( 1,  0),
        "Left"  => ( 0, -1),
        "Right" => ( 0,  1),
        _       => ( 0,  0)
    };

    private static ((int dr, int dc) left, (int dr, int dc) right) GetPerpendicular(string action) =>
        action switch
        {
            "Up"    => (( 0, -1), ( 0,  1)),
            "Down"  => (( 0,  1), ( 0, -1)),
            "Left"  => ((-1,  0), ( 1,  0)),
            "Right" => (( 1,  0), (-1,  0)),
            _       => (( 0,  0), ( 0,  0))
        };

    private static (int r, int c) Parse(string state)
    {
        var parts = state.Split(',');
        return (int.Parse(parts[0]), int.Parse(parts[1]));
    }
}
