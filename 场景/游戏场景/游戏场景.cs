using Godot;
using System;

public partial class 游戏场景 : Node2D
{
    public void On_测试传送_BodyEntered(Node2D body)
    {
        if (body is 玩家 player)
        {
            player.GlobalPosition = new Vector2(1950, -450);
        }
    }
}
