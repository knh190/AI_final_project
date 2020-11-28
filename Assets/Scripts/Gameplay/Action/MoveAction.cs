using MiniStrategy;

public class MoveAction : ActionBase
{
    struct Context
    {
        public Unit unit;
    }
    Context ctx;

    public static MoveAction Create(Unit unit)
    {
        Context ctx = new Context { unit = unit };
        return new MoveAction { ctx = ctx  };
    }

    public override void Execute()
    {
        ctx.unit.StepMove();
    }

    public override void Undo()
    {
        throw new System.NotImplementedException();
    }
}
