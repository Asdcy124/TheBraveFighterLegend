using Godot;
using System;

public partial class 玩家 : CharacterBody2D, I_状态机
{
    #region 子节点
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
    #endregion

    #region 重写Godot方法
    public override void _Ready()
    {
        动画精灵 = GetNode<AnimatedSprite2D>("动画精灵");
        动画播放器 = GetNode<AnimationPlayer>("动画播放器");
        踏空跳跃计时器 = GetNode<Timer>("踏空跳跃计时器");
        提前跳跃计时器 = GetNode<Timer>("提前跳跃计时器");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("跳跃"))
            提前跳跃计时器.Start();
        if (@event.IsActionReleased("跳跃") && Velocity.Y < -跳跃速度 / 2)
            Velocity = new Vector2(Velocity.X, -跳跃速度 / 2);//将跳跃力缩减一半
    }
    #endregion

    #region 移动相关属性
    [Export] public float 奔跑速度 = 160;
    public float 地面奔跑加速度 = 800;
    public float 空中奔跑加速度 = 8000;
    [Export] public float 跳跃速度 = 320;

    public void 移动(double delta, bool is零重力 = false)
    {
        #region 移动
        Vector2 速度矢量 = Velocity;

        //重力
        if (!is零重力)
            速度矢量 += GetGravity() * (float)delta;
        //移动
        float 左右移动 = Input.GetAxis("移动_左", "移动_右");
        float 奔跑加速度 = IsOnFloor() ? 地面奔跑加速度 : 空中奔跑加速度;
        速度矢量.X = (float)Mathf.MoveToward(速度矢量.X, 左右移动 * 奔跑速度, 奔跑加速度 * delta);

        Velocity = 速度矢量;
        MoveAndSlide();
        #endregion

        #region 动画
        //翻转
        if (左右移动 != 0)
            动画精灵.FlipH = 左右移动 < 0;
        #endregion
    }

    public void 站立(double delta)
    {
        Vector2 速度矢量 = Velocity;

        //重力
        速度矢量 += GetGravity() * (float)delta;
        //移动
        速度矢量.X = (float)Mathf.MoveToward(速度矢量.X, 0, 空中奔跑加速度 * delta);

        Velocity = 速度矢量;
        MoveAndSlide();
    }

    #endregion

    #region 状态机
    public enum E_状态
    {
        空闲,
        奔跑,
        跳跃上升,
        跳跃下落,
        跳跃着陆
    }

    public E_状态[] 站在地面上的状态s = { E_状态.空闲, E_状态.奔跑, E_状态.跳跃着陆 };
    private bool Is切换状态后第一帧 = false;

    public void 切换状态(int 当前状态, int 下一个状态)
    {
        //即将从 非地面状态 切换到 地面状态
        if (!站在地面上的状态s.Contains((E_状态)当前状态) && 站在地面上的状态s.Contains((E_状态)下一个状态))
            踏空跳跃计时器.Stop();

        switch ((E_状态)下一个状态)
        {
            case E_状态.空闲:
                动画播放器.Play("空闲");
                break;

            case E_状态.奔跑:
                动画播放器.Play("奔跑");
                break;

            case E_状态.跳跃上升:
                动画播放器.Play("跳跃上升");
                float Y = Input.IsActionPressed("跳跃") ? -跳跃速度 : -跳跃速度 / 2;//如果提前跳的按键时间太短, 就缩减跳跃力
                Velocity = new Vector2(Velocity.X, Y);
                踏空跳跃计时器.Stop();
                提前跳跃计时器.Stop();
                break;

            case E_状态.跳跃下落:
                动画播放器.Play("跳跃下落");
                if (站在地面上的状态s.Contains((E_状态)当前状态))
                    踏空跳跃计时器.Start();
                break;

            case E_状态.跳跃着陆:
                动画播放器.Play("跳跃着陆");
                break;
        }

        动画精灵.Play();
        Is切换状态后第一帧 = true;
    }

    public int 获取下一个状态(int 当前状态)
    {
        bool is能跳跃 = IsOnFloor() || 踏空跳跃计时器.TimeLeft > 0;
        bool is需要跳跃 = is能跳跃 && 提前跳跃计时器.TimeLeft > 0;
        if (is需要跳跃)
            return (int)E_状态.跳跃上升;

        float 左右移动 = Input.GetAxis("移动_左", "移动_右");
        bool is奔跑 = 左右移动 != 0 || Velocity.X != 0;

        switch ((E_状态)当前状态)
        {
            case E_状态.空闲:
                if (!IsOnFloor())
                    return (int)(Velocity.Y <= 0 ? E_状态.跳跃上升 : E_状态.跳跃下落);
                if (is奔跑)
                    return (int)E_状态.奔跑;
                break;

            case E_状态.奔跑:
                if (!IsOnFloor())
                    return (int)(Velocity.Y <= 0 ? E_状态.跳跃上升 : E_状态.跳跃下落);
                if (!is奔跑)
                    return (int)E_状态.空闲;
                break;

            case E_状态.跳跃上升:
                if (Velocity.Y > 0)
                    return (int)E_状态.跳跃下落;
                break;

            case E_状态.跳跃下落:
                if (IsOnFloor())
                    return (int)(Velocity.X == 0 ? E_状态.跳跃着陆 : E_状态.奔跑);
                break;

            case E_状态.跳跃着陆:
                if (!动画精灵.IsPlaying())
                    return (int)E_状态.空闲;
                break;
        }

        return 当前状态;
    }

    public void 状态机_PhysicsProcess(int 当前状态, double delta)
    {
        switch ((E_状态)当前状态)
        {
            case E_状态.空闲:
                移动(delta);
                break;

            case E_状态.奔跑:
                移动(delta);
                break;

            case E_状态.跳跃上升:
                移动(delta, Is切换状态后第一帧);
                break;

            case E_状态.跳跃下落:
                移动(delta);
                break;

            case E_状态.跳跃着陆:
                站立(delta);
                break;
        }

        Is切换状态后第一帧 = false;
    }

    #endregion

}
