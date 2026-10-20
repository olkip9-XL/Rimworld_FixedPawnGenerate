using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

namespace FixedPawnGenerate;

public abstract class TransformWorker
{
    public SubTransformDef subTransformDef;

    //检查是否匹配当前节点，返回是否匹配
    public virtual bool Match(string label, Pawn pawn)
    {
        //Root是根节点，每帧调用一次，默认每帧update一次。
        return label == "Root";
    }

    //根据elapsed时间更新状态，进行更改，返回是否有更改
    public virtual bool Update(int elapsed, Pawn pawn)
    {
        return false;
    }

    //更新材质参数，返回是否有更改
    public virtual bool UpdateMaterial(ref Material material, int elapsed)
    {
        return false;
    }
}

