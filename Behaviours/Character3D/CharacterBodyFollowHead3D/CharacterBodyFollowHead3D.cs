using Godot;

public partial class CharacterBodyFollowHead3D : Node
{
    [ExportCategory("Body Follow Settings")]
    [Export] public float BaseStiffness { get; set; } = 35.0f;      // How strongly body pulls toward head
    [Export] public float Damping { get; set; } = 22.0f;            // Lower = more bounce/overshoot
    [Export] public float MaxAngularVelocity { get; set; } = 12.0f; // rad/s (prevents crazy spins)

    [Export] public Curve FollowResponseCurve { get; set; }         // Distance scaling curve

    [ExportCategory("Step Angle (Deadzone)")]
    [Export] public float StepAngleDegrees { get; set; } = 8.0f;    // Body ignores turns < this angle

    [Export] public NodePath HeadPath { get; set; } = new NodePath("../Coll_Head");

    private CharacterController3D _controller;
    private Node3D _head;
    private float _angularVelocity = 0.0f;

    public override void _Ready()
    {
        _controller = GetParent<CharacterController3D>();
        _head = GetNode<Node3D>(HeadPath);

        if (_head == null)
            GD.PrintErr("CharacterBodyFollowHead3D: Head node not found!");
    }

    public override void _Process(double delta)
    {
        if (_controller == null || _head == null) return;

        // Current relative yaw error (head yaw relative to body)
        float error = WrapAngle(_head.Rotation.Y);
        float absError = Mathf.Abs(error);
        float stepRad = Mathf.DegToRad(StepAngleDegrees);

        // ── DEADZONE: Ignore small turns, quickly stop any momentum ──
        if (absError <= stepRad)
        {
            // Strong damping to snap to zero velocity
            _angularVelocity = Mathf.MoveToward(_angularVelocity, 0f, Damping * 3f * (float)delta);
            return; // No body rotation or head counter-rotation
        }

        // ── ACTIVE FOLLOW: Scale response by distance via curve ──
        float normError = Mathf.Clamp(absError / Mathf.Pi, 0f, 1f);
        float multiplier = FollowResponseCurve?.Sample(normError) ?? 1.0f;
        float stiffness = BaseStiffness * multiplier;

        // Spring + damper physics
        float springForce = error * stiffness;
        float dampingForce = _angularVelocity * Damping;

        _angularVelocity += (springForce - dampingForce) * (float)delta;
        _angularVelocity = Mathf.Clamp(_angularVelocity, -MaxAngularVelocity, MaxAngularVelocity);

        float rotationThisFrame = _angularVelocity * (float)delta;

        // Apply to body
        _controller.RotateY(rotationThisFrame);

        // Counter-rotate head (keeps camera view perfectly smooth)
        _head.RotateY(-rotationThisFrame);
    }

    private static float WrapAngle(float angle)
    {
        angle = angle % Mathf.Tau;
        if (angle > Mathf.Pi) angle -= Mathf.Tau;
        else if (angle < -Mathf.Pi) angle += Mathf.Tau;
        return angle;
    }
}