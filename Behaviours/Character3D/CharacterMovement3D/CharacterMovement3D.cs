using Godot;

public partial class CharacterMovement3D : Node3D
{
    [Export] public float Speed { get; set; } = 10.0f;
    [Export] public float SprintSpeed { get; set; } = 16.0f;
    [Export] public float CrouchSpeed { get; set; } = 5.0f;  // ← NEW

    private CharacterController3D _controller;

    public override void _Ready()
    {
        _controller = GetParent<CharacterController3D>();
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 input = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
        if (input == Vector2.Zero)
            return;

        // Sprint: forward + sprint + ground + NOT crouched
        bool isSprinting = Input.IsActionPressed("sprint") && input.Y < 0f && 
                           _controller.IsOnFloor() && !_controller.IsCrouched;

        float currentSpeed = isSprinting ? SprintSpeed : 
                            (_controller.IsCrouched ? CrouchSpeed : Speed);

        Vector3 localInput = new Vector3(input.X, 0, input.Y);
        Vector3 direction = (_controller.Transform.Basis * localInput).Normalized();

        _controller.SetDesiredHorizontalVelocity(direction * currentSpeed);
    }
}