using Godot;
using System;
using System.Threading.Tasks;

[GlobalClass]
public partial class 状态机 : Node
{
    public I_状态机 状态机所有者;
    public int 当前状态
    {
        get;
        set
        {
            状态机所有者.切换状态(field, value);
            field = value;
        }
    }

    public override async void _Ready()
    {
        await ToSignal(Owner, Node.SignalName.Ready);
        状态机所有者 = Owner as I_状态机;
        当前状态 = 0;
    }

    public override void _PhysicsProcess(double delta)
    {
        while (true)
        {
            int 下一个状态 = 状态机所有者.获取下一个状态(当前状态);
            if (当前状态 == 下一个状态)
                break;
            当前状态 = 下一个状态;//?????
        }

       状态机所有者.状态机_PhysicsProcess(当前状态, delta);
    }


}
