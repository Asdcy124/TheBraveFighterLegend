using Godot;
using System;

public partial class 玩家 : CharacterBody2D, I_状态机
{
    #region 子节点
    public AnimatedSprite2D 动画精灵;
    public RayCast2D 滑墙手;
    public RayCast2D 滑墙脚;

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

    public 状态机 状态机;
    #endregion

    #region 重写Godot方法
    public override void _Ready()
    {
        动画精灵 = GetNode<AnimatedSprite2D>("动画精灵");
        滑墙手 = GetNode<RayCast2D>("动画精灵/滑墙手");
        滑墙脚 = GetNode<RayCast2D>("动画精灵/滑墙脚");

        动画播放器 = GetNode<AnimationPlayer>("动画播放器");
        踏空跳跃计时器 = GetNode<Timer>("踏空跳跃计时器");
        提前跳跃计时器 = GetNode<Timer>("提前跳跃计时器");

        状态机 = GetNode<状态机>("状态机");
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
    public float 空中奔跑加速度 = 1600;
    [Export] public float 跳跃速度 = 320;
    [Export] public Vector2 蹬墙跳速度 = new Vector2(380, 250);

    public void 移动(double delta, Vector2 重力)
    {
        #region 移动
        Vector2 速度矢量 = Velocity;

        //重力
        速度矢量 += 重力 * (float)delta;
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
            动画精灵.Scale = new Vector2(左右移动 < 0 ? -1 : 1, 动画精灵.Scale.Y);
        #endregion
    }

    public void 站立(double delta, Vector2 重力)
    {
        Vector2 速度矢量 = Velocity;

        //重力
        速度矢量 += 重力 * (float)delta;
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
        跳跃着陆,
        滑墙,
        蹬墙跳
    }

    public E_状态[] 站在地面上的状态s = { E_状态.空闲, E_状态.奔跑, E_状态.跳跃着陆 };
    private bool Is切换状态后第一帧 = false;

    public bool Is可以进入滑墙状态() => IsOnWall() && 滑墙手.IsColliding() && 滑墙脚.IsColliding();

    public void 切换状态(int 当前状态, int 下一个状态)
    {
        //即将从 非地面状态 切换到 地面状态
        if (!站在地面上的状态s.Contains((E_状态)当前状态) && 站在地面上的状态s.Contains((E_状态)下一个状态))
            踏空跳跃计时器.Stop();

        switch ((E_状态)当前状态)
        {
            case E_状态.跳跃上升:
            case E_状态.跳跃下落:
                动画精灵.Position = new Vector2(0, -28);
                break;

            case E_状态.滑墙:
                动画精灵.FlipH = false;
                动画精灵.Position = new Vector2(0, -28);
                滑墙手.Position = new Vector2(0, 2);
                滑墙手.TargetPosition = new Vector2(8, 0);
                滑墙脚.Position = new Vector2(0, 22);
                滑墙脚.TargetPosition = new Vector2(8, 0);
                break;

            case E_状态.蹬墙跳:
                Engine.TimeScale = 1;
                break;
        }

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
                float y = Input.IsActionPressed("跳跃") ? -跳跃速度 : -跳跃速度 / 2;//如果提前跳的按键时间太短, 就缩减跳跃力
                Velocity = new Vector2(Velocity.X, y);
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

            case E_状态.滑墙:
                动画播放器.Play("滑墙");
                动画精灵.Position = new Vector2(6 * GetWallNormal().X, -24);
                break;

            case E_状态.蹬墙跳:
                //类似跳跃
                动画播放器.Play("跳跃上升");
                float x = 蹬墙跳速度.X * GetWallNormal().X;
                y = -蹬墙跳速度.Y;//这里就对玩家友好些
                Velocity = new Vector2(x, y);
                提前跳跃计时器.Stop();
                Engine.TimeScale = 0.3;
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
                if (Is可以进入滑墙状态())
                    return (int)E_状态.滑墙;
                break;

            case E_状态.跳跃着陆:
                if (is奔跑)
                    return (int)E_状态.奔跑;
                if (!动画精灵.IsPlaying())
                    return (int)E_状态.空闲;
                break;

            case E_状态.滑墙:
                if (提前跳跃计时器.TimeLeft > 0 && !Is切换状态后第一帧)
                    return (int)E_状态.蹬墙跳;
                else if (IsOnFloor())
                    return (int)E_状态.空闲;
                else if (!IsOnWall())
                    return (int)E_状态.跳跃下落;
                break;

            case E_状态.蹬墙跳:
                if (!Is切换状态后第一帧 && Is可以进入滑墙状态())
                    return (int)E_状态.滑墙;
                else if (Velocity.Y >= 0)
                    return (int)E_状态.跳跃下落;
                break;
        }

        return 当前状态;
    }

    public void 状态机_PhysicsProcess(int 当前状态, double delta)
    {
        switch ((E_状态)当前状态)
        {
            case E_状态.空闲:
                移动(delta, GetGravity());
                break;

            case E_状态.奔跑:
                移动(delta, GetGravity());
                break;

            case E_状态.跳跃上升:
                移动(delta, Is切换状态后第一帧 ? Vector2.Zero : GetGravity());
                break;

            case E_状态.跳跃下落:
                移动(delta, GetGravity());
                break;

            case E_状态.跳跃着陆:
                站立(delta, GetGravity());
                break;

            case E_状态.滑墙:
                移动(delta, GetGravity() / 3);
                动画精灵.Scale = new Vector2(GetWallNormal().X, 动画精灵.Scale.Y);
                break;

            case E_状态.蹬墙跳:
                if (状态机.当前状态持续时间 < 0.1)
                {
                    站立(delta, Is切换状态后第一帧 ? Vector2.Zero : GetGravity());
                    动画精灵.Scale = new Vector2(GetWallNormal().X, 动画精灵.Scale.Y);
                }
                else
                    移动(delta, GetGravity());
                break;
        }

        Is切换状态后第一帧 = false;
    }

    #endregion

}
