using Godot;
using System;
using System.Threading.Tasks;

public partial class 敌人 : CharacterBody2D
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

    #region 方向
    public enum E_方向
    {
        左 = -1,
        右 = 1
    }

    [Export]
    public E_方向 方向
    {
        get;
        set
        {
            if (!IsNodeReady())
            {
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

    #endregion


}
