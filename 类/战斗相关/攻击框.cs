using Godot;
using System;

[GlobalClass]
public partial class 攻击框 : Area2D
{
    public override void _Ready()
    {
        AreaEntered += On_攻击框_Entered;
    }



    [Signal] public delegate void 攻击EventHandler(Node2D 受击者);

    public void On_攻击框_Entered(Area2D area)
    {
        if (area is 受击框 受击者)
        {
            GD.Print($"{Owner.Name} 攻击=> {受击者.Owner.Name}");
            this.EmitSignal(SignalName.攻击, 受击者);
            受击者.EmitSignal(受击框.SignalName.受击, this);
        }
    }
}
