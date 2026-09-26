using Godot;
using System;
using System.Threading.Tasks;

/// <summary>
/// 要求状态机的父结点Owner 继承 I_状态机(接口)
/// </summary>
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
            当前状态持续时间 = 0;
        }
    }
    public double 当前状态持续时间;

    public override async void _Ready()
    {
        await ToSignal(Owner, Node.SignalName.Ready);
        状态机所有者 = Owner as I_状态机;
        当前状态 = 0;
    }

    public override void _PhysicsProcess(double delta)
    {
        while (true)//厉害, 看这么远
        {
            int 下一个状态 = 状态机所有者.获取下一个状态(当前状态);
            if (当前状态 == 下一个状态)
                break;
            当前状态 = 下一个状态;//?????

            //成也如此, 败也如此. 
            // 虽然他变稳定了, 但会暂时循环在同一帧
            // 此时如果修改了Godot中的某些属性, 你不会得到及时的反馈, 他们通常也需要下一帧来进行处理

            //三步之内, 必有解药
            //Godot中也有强制更新的手段
        }

        状态机所有者.状态机_PhysicsProcess(当前状态, delta);
        当前状态持续时间 += delta;
    }


}
