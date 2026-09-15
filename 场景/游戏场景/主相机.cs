using Godot;
using System;

public partial class 主相机 : Camera2D
{
	public override async void _Ready()
	{
		ResetSmoothing();
	}
}
