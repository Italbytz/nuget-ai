using System.Collections.Generic;

namespace Italbytz.AI.Robotics;

/// <summary>A 2-D occupancy map used for ray-casting.</summary>
public interface IMap<TPose>
{
    bool IsPoseValid(TPose pose);
    /// <summary>Returns expected sensor range by casting a ray from pose at the given angle.</summary>
    double RayCast(TPose pose, double angle);
}
