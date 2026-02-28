using Godot;

public partial class CharacterHeadMovementFPS : Node
{
    [Export] public float MouseSensitivity { get; set; } = 0.002f;
    [Export] public float MaxPitchDegrees { get; set; } = 89f;
    [Export] public float MaxRelativeYawDegrees { get; set; } = 135f; // 135° neck limit. Set to 360+ for no limit

    [Export] public NodePath HeadPath { get; set; } = new NodePath("../Coll_Head");

    private Node3D _head;
    private float _pitch = 0f;

    public override void _Ready()
    {
        _head = GetNode<Node3D>(HeadPath);
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouse)
        {
            // ── Yaw → ONLY Head (direct mouse control)
            float yawDelta = -mouse.Relative.X * MouseSensitivity;
            float maxYaw = Mathf.DegToRad(MaxRelativeYawDegrees);
            float newYaw = Mathf.Clamp(_head.Rotation.Y + yawDelta, -maxYaw, maxYaw);

            // ── Pitch → Head only
            float pitchDelta = -mouse.Relative.Y * MouseSensitivity;
            _pitch += pitchDelta;
            _pitch = Mathf.Clamp(_pitch, Mathf.DegToRad(-MaxPitchDegrees), Mathf.DegToRad(MaxPitchDegrees));

            // Apply both
            _head.Rotation = new Vector3(_pitch, newYaw, _head.Rotation.Z);
        }
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured
                ? Input.MouseModeEnum.Visible
                : Input.MouseModeEnum.Captured;
        }
    }
}