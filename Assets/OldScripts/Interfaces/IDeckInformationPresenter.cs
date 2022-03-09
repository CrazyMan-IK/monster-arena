using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonsterArena.Models;

namespace MonsterArena.Interfaces
{
    public interface IDeck
    {
        IReadOnlyList<MonsterInformation> Monsters { get; }
    }
}
