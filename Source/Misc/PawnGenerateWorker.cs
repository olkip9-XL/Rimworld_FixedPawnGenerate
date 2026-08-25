using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate;

abstract public class PawnGenerateWorker
{
    public FixedPawnDef def;

    abstract public void PostGenerate(Pawn pawn);
}
