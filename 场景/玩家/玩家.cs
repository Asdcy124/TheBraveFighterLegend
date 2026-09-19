using Godot;
using System;

public partial class 玩家 : CharacterBody2D
{
    public AnimatedSprite2D 动画精灵;
    public AnimationPlayer 动画播放器;
    /// <summary>
    /// 在不小心掉下平台的0.1秒内, 依旧可以进行跳跃<br/>
    /// 如果 TimeLeft 大于0, 说明还在0.1秒内
    /// </summary>
    public Timer 踏空跳跃计时器;
    /// <summary>
    /// 在即将落地的前0.1秒内按下跳跃时, 在落地后会立马进行跳跃<br/>
    /// 如果 TimeLeft 大于0, 说明还在0.1秒内
    /// </summary>
    public Timer 提前跳跃计时器;
    public override void _Ready()
    {
        动画精灵 = GetNode<AnimatedSprite2D>("动画精灵");
        动画播放器 = GetNode<AnimationPlayer>("动画播放器");
        踏空跳跃计时器 = GetNode<Timer>("踏空跳跃计时器");
        提前跳跃计时器 = GetNode<Timer>("提前跳跃计时器");
    }

    [Export] public float 奔跑速度 = 160;
    [Export] public float 地面奔跑加速度 = 800;
    [Export] public float 空中奔跑加速度 = 8000;
    [Export] public float 跳跃速度 = 320;

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("跳跃"))
            提前跳跃计时器.Start();
        if (@event.IsActionReleased("跳跃") && Velocity.Y < -跳跃速度 / 2)
            Velocity = new Vector2(Velocity.X, -跳跃速度 / 2);//将跳跃力缩减一半
    }

    public override void _PhysicsProcess(double delta)
    {
        #region 移动
        Vector2 速度矢量 = Velocity;

        //重力
        速度矢量 += GetGravity() * (float)delta;

        //移动
        float 左右移动 = Input.GetAxis("移动_左", "移动_右");
        左右移动 = Mathf.Round(左右移动 * 2f) / 2f;//[-1, -0.5, 0, 0.5, 1]
        float 奔跑加速度 = IsOnFloor() ? 地面奔跑加速度 : 空中奔跑加速度;
        速度矢量.X = (float)Mathf.MoveToward(速度矢量.X, 左右移动 * 奔跑速度, 奔跑加速度 * delta);

        //跳跃
        //1.站在地面上可以跳跃  2.离开地面0.1秒内也可以跳跃 3.在即将落地的前0.1秒内可以跳跃
        bool is能跳跃 = IsOnFloor() || 踏空跳跃计时器.TimeLeft > 0;
        bool is需要跳跃 = is能跳跃 && 提前跳跃计时器.TimeLeft > 0;
        if (is需要跳跃)
        {
            速度矢量.Y = Input.IsActionPressed("跳跃") ? -跳跃速度 : -跳跃速度 / 2;//如果提前跳的按键时间太短, 就缩减跳跃力
            踏空跳跃计时器.Stop();
            提前跳跃计时器.Stop();
        }
        Velocity = 速度矢量;

        bool is移动前在地面 = IsOnFloor();
        MoveAndSlide();

        //踏空跳跃
        if (IsOnFloor() != is移动前在地面)
            if (is移动前在地面 && !is需要跳跃)//不是因为跳跃而离开地面 (感觉不如判断Y轴)
                踏空跳跃计时器.Start();
            else 踏空跳跃计时器.Stop();//从空中回到地面, 就关闭踏空跳跃
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
        else if (Velocity.X != 0 || 左右移动 != 0)
            动画播放器.Play("奔跑");
        //待机
        else
            动画播放器.Play("待机");
        #endregion

    }


}
