using Godot;
using System;

[GlobalClass]
public partial class 受击框 : Area2D
{
    [Signal] public delegate void 受击EventHandler(Node2D 攻击者);
}
