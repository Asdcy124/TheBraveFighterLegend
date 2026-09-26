using Godot;
using System;
using System.Threading.Tasks;

[GlobalClass]
public partial class 敌人 : CharacterBody2D, I_状态机
{
    #region 子节点
    public AnimatedSprite2D 动画精灵;
    public AnimationPlayer 动画播放器;
    public 状态机 状态机;
    #endregion

    #region 重写Godot方法
    public override void _Ready()
    {
        动画精灵 = GetNode<AnimatedSprite2D>("动画精灵");
        动画播放器 = GetNode<AnimationPlayer>("动画播放器");
        状态机 = GetNode<状态机>("状态机");
    }
    #endregion

    #region 移动
    public enum E_方向
    {
        左 = -1,
        右 = 1
    }

    [Export]
    public virtual E_方向 方向
    {
        get;
        set
        {
            if (!IsNodeReady())
            {
                field = value;
                Set方向_延时(value);
                return;
            }
            
            field = value;
            动画精灵.Scale = new Vector2(-(int)value, 动画精灵.Scale.Y);
        }
    } = E_方向.左;
    private async void Set方向_延时(E_方向 value)
    {
        await ToSignal(this, SignalName.Ready);
        方向 = value;
    }

    [Export] public float 最大速度 = 180;
    [Export] public float 加速度 = 1000;

    public void 移动(float 速度, float delta)
    {
        Vector2 速度矢量 = Velocity;
        //重力
        速度矢量 += GetGravity() * delta;
        //移动
        速度矢量.X = Mathf.MoveToward(Velocity.X, 速度 * (int)方向, 加速度 * delta);

        Velocity = 速度矢量;
        MoveAndSlide();
    }
    #endregion

    #region 状态机
    public virtual int 获取下一个状态(int 当前状态) { throw new NotImplementedException(); }
    public virtual void 状态机_PhysicsProcess(int 当前状态, double delta) { throw new NotImplementedException(); }
    public virtual void 切换状态(int 当前状态, int 下一个状态) { throw new NotImplementedException(); }
    #endregion


}
