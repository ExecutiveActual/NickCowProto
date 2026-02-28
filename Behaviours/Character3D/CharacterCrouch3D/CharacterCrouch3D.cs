using Godot;

public partial class CharacterCrouch3D : Node
{
    private CharacterController3D _controller;

    public override void _Ready()
    {
        _controller = GetParent<CharacterController3D>();
    }

    public override void _PhysicsProcess(double _)
    {
        _controller.SetCrouchTarget(Input.IsActionPressed("crouch"));
    }
}