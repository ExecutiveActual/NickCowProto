using Godot;

public partial class CharacterJump3D : Node
{
    [Export] public float JumpVelocity { get; set; } = 8.0f;

    private CharacterController3D _controller;

    public override void _Ready()
    {
        _controller = GetParent<CharacterController3D>();
    }

    public override void _PhysicsProcess(double _)
    {
        // No jump while crouched
        if (Input.IsActionJustPressed("jump") && _controller.IsOnFloor() && !_controller.IsCrouched)
            _controller.AddVerticalImpulse(JumpVelocity);
    }
}