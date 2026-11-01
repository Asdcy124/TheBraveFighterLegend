using Godot;
using System;
using System.Threading.Tasks;

public partial class 野猪 : 敌人
{
    #region 子节点
    public RayCast2D 墙体检测;
    public RayCast2D 地面检测;
    public RayCast2D 玩家检测;

    public Timer 冷静计时器;
    #endregion

    #region 重写Godot方法
    public override void _Ready()
    {
        base._Ready();
        墙体检测 = GetNode<RayCast2D>("动画精灵/墙体检测");
        地面检测 = GetNode<RayCast2D>("动画精灵/地面检测");
        玩家检测 = GetNode<RayCast2D>("动画精灵/玩家检测");
        冷静计时器 = GetNode<Timer>("冷静计时器");
    }

    #endregion

    #region 移动
    public override E_方向 方向
    {
        get => base.方向;//C#的bug不用管, 'get' 访问器应使用“field”，因为其他访问器正在使用它。
        set
        {
            if (!IsNodeReady())
            {
                field = value;
                Set方向_延时(value);
                return;
            }

            base.方向 = value;
            //强制更新射线, 避免在同一帧无法及时更新
            墙体检测.ForceRaycastUpdate();
            地面检测.ForceRaycastUpdate();
            玩家检测.ForceRaycastUpdate();
        }
    }
    async void Set方向_延时(E_方向 value)
    {
        await ToSignal(this, SignalName.Ready);
        方向 = value;
    }

    #endregion

    #region 状态机
    public enum E_状态
    {
        空闲,
        行走,
        奔跑,
        受击
    }


    public override void 切换状态(int 当前状态, int 下一个状态)
    {
        //GD.Print($"[{Engine.GetPhysicsFrames()}] {(E_状态)当前状态} => {(E_状态)下一个状态}");

        switch ((E_状态)下一个状态)
        {
            case E_状态.空闲:
                动画播放器.Play("空闲");
                // if (墙体检测.IsColliding())//没有必要 面壁思过
                //     方向 = 方向 == E_方向.左 ? E_方向.右 : E_方向.左;
                //GD.Print($"是否有墙 {墙体检测.IsColliding()}"); //RayCast2D 需要需要在下一帧进行碰撞处理, 因此不会得到及时的反馈
                break;

            case E_状态.行走:
                动画播放器.Play("行走");
                if (墙体检测.IsColliding() || !地面检测.IsColliding())//别往悬崖走啊
                    方向 = 方向 == E_方向.左 ? E_方向.右 : E_方向.左;
                break;

            case E_状态.奔跑:
                动画播放器.Play("奔跑");
                break;
        }
    }

    public override int 获取下一个状态(int 当前状态)
    {
        if (玩家检测.IsColliding() && 玩家检测.GetCollider() is 玩家)//不能透过墙看人
            return (int)E_状态.奔跑;

        switch ((E_状态)当前状态)
        {
            case E_状态.空闲:
                if (状态机.当前状态持续时间 > 2)
                    return (int)E_状态.行走;
                break;

            case E_状态.行走:
                if (墙体检测.IsColliding() || !地面检测.IsColliding())
                    return (int)E_状态.空闲;
                break;

            case E_状态.奔跑:
                if (冷静计时器.IsStopped())
                    return (int)E_状态.行走;
                break;
        }

        return 当前状态;
    }

    public override void 状态机_PhysicsProcess(int 当前状态, double delta)
    {
        switch ((E_状态)当前状态)
        {
            case E_状态.空闲:
                移动(0, (float)delta);
                break;

            case E_状态.行走:
                移动(最大速度 / 3, (float)delta);
                break;

            case E_状态.奔跑:
                if (墙体检测.IsColliding() || !地面检测.IsColliding())
                    方向 = 方向 == E_方向.左 ? E_方向.右 : E_方向.左;
                移动(最大速度, (float)delta);
                if (玩家检测.IsColliding() && 玩家检测.GetCollider() is 玩家)
                    冷静计时器.Start();
                break;
        }
    }
    #endregion

}
