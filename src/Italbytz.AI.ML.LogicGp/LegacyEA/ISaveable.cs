using System.IO;

namespace Italbytz.AI;

internal interface ISaveable
{
    void Save(Stream stream);
}
