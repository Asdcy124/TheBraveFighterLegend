using Godot;
using System;

public partial class 玩家 : CharacterBody2D
{
    public AnimatedSprite2D 动画精灵;
    public AnimationPlayer 动画播放器;
    public override void _Ready()
    {
        动画精灵 = GetNode<AnimatedSprite2D>("动画精灵");
        动画播放器 = GetNode<AnimationPlayer>("动画播放器");
    }

    [Export] public float 奔跑速度 = 200;
    [Export] public float 跳跃速度 = 300;

    public override void _PhysicsProcess(double delta)
    {
        #region 移动
        Vector2 速度矢量 = Velocity;

        //重力
        速度矢量 += GetGravity() * (float)delta;
        //移动
        int 左右移动 = 0;
        if (Input.IsActionPressed("移动_左")) 左右移动--;
        if (Input.IsActionPressed("移动_右")) 左右移动++;

        速度矢量.X = 左右移动 * 奔跑速度;
        //跳跃
        if (IsOnFloor() && Input.IsActionJustPressed("跳跃"))
            速度矢量.Y = -跳跃速度;

        Velocity = 速度矢量;
        MoveAndSlide();
        #endregion

        #region 动画
        //翻转
        if (左右移动 != 0)
            动画精灵.FlipH = 左右移动 < 0;

        //跳跃
        if (!IsOnFloor())
        {
            动画播放器.Play("跳跃");
        }
        //奔跑
        else if (Velocity.X != 0)
            动画播放器.Play("奔跑");
        //待机
        else
            动画播放器.Play("待机");
        #endregion

    }


}
