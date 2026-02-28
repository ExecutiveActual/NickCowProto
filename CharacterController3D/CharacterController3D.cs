using Godot;
using System.Linq; // For Exclude array

public partial class CharacterController3D : CharacterBody3D
{
    [Export] public float Gravity { get; set; } = 20.0f;

    [Export] public float GroundAcceleration { get; set; } = 25.0f;
    [Export] public float AirAcceleration { get; set; } = 10.0f;
    [Export] public float GroundFriction { get; set; } = 30.0f;
    [Export] public float AirFriction { get; set; } = 5.0f;

    // ── Crouch ──
    [Export] public float StandingHalfHeight { get; set; } = 0.9f;
    [Export] public float CrouchHalfHeight { get; set; } = 0.5f;
    [Export] public float HeightChangeSpeed { get; set; } = 15.0f;
    [Export] public float EyeOffsetFromTop { get; set; } = 0.15f;

    private CollisionShape3D _bodyColl;
    private CapsuleShape3D _capsuleShape;
    private Node3D _headNode;
    private float _currentHalfHeight;
    private bool _targetCrouched;
    private bool _isCrouched;

    public bool IsCrouched => _isCrouched;
    public void SetCrouchTarget(bool crouched) => _targetCrouched = crouched;

    // Modules will add their desired horizontal velocity here each frame
    private Vector3 _desiredHorizontalVelocity = Vector3.Zero;
    private float _desiredVerticalVelocity = 0.0f;

    public void SetDesiredHorizontalVelocity(Vector3 vel) => _desiredHorizontalVelocity = vel;
    public void AddVerticalImpulse(float impulse) => _desiredVerticalVelocity += impulse;

    public override void _Ready()
    {
        _bodyColl = GetNode<CollisionShape3D>("Coll_Body");
        _capsuleShape = _bodyColl.Shape as CapsuleShape3D;
        if (_capsuleShape == null)
        {
            GD.PrintErr("CharacterController3D: Coll_Body must have CapsuleShape3D!");
            return;
        }

        _headNode = GetNode<Node3D>("Coll_Head");
        if (_headNode == null)
        {
            GD.PrintErr("CharacterController3D: Coll_Head not found!");
            return;
        }

        // Init to standing
        _currentHalfHeight = StandingHalfHeight;
        _isCrouched = false;
        UpdateShape();
    }

    public override void _PhysicsProcess(double delta)
    {
        float d = (float)delta;

        // ── Crouch FIRST (affects IsOnFloor/IsOnCeiling) ──
        HandleCrouch(d);

        Vector3 velocity = Velocity;

        // Gravity
        if (!IsOnFloor())
            velocity.Y -= Gravity * d;

        // Vertical impulses
        if (_desiredVerticalVelocity != 0.0f)
            velocity.Y += _desiredVerticalVelocity;

        // Smooth horizontal
        Vector3 hVel = new Vector3(velocity.X, 0, velocity.Z);
        Vector3 hTarget = _desiredHorizontalVelocity;

        if (hTarget.LengthSquared() > 0.001f)
        {
            float accel = IsOnFloor() ? GroundAcceleration : AirAcceleration;
            hVel = hVel.MoveToward(hTarget, accel * d);
        }
        else
        {
            float friction = IsOnFloor() ? GroundFriction : AirFriction;
            hVel = hVel.MoveToward(Vector3.Zero, friction * d);
        }

        velocity.X = hVel.X;
        velocity.Z = hVel.Z;

        Velocity = velocity;
        MoveAndSlide();

        // Reset
        _desiredHorizontalVelocity = Vector3.Zero;
        _desiredVerticalVelocity = 0.0f;
    }

    private void HandleCrouch(float delta)
{
    // Check if we can stand up
    bool canStand = true;
    if (!_targetCrouched && _isCrouched)
    {
        Vector3 rayStart = GlobalPosition + Vector3.Up * _currentHalfHeight;
        float deltaH = StandingHalfHeight - _currentHalfHeight;
        Vector3 rayEnd = rayStart + Vector3.Up * deltaH;

        var spaceState = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(rayStart, rayEnd);

        query.Exclude = new Godot.Collections.Array<Rid> { GetRid() };

        var result = spaceState.IntersectRay(query);

        if (result.Count > 0)
            canStand = false;
    }

    if (!canStand)
        _targetCrouched = true;

    // Lerp height
    float targetHalfHeight = _targetCrouched ? CrouchHalfHeight : StandingHalfHeight;
    _currentHalfHeight = Mathf.MoveToward(_currentHalfHeight, targetHalfHeight, HeightChangeSpeed * delta);
    _isCrouched = _currentHalfHeight < (StandingHalfHeight + CrouchHalfHeight) * 0.5f;

    UpdateShape();
}

    private void UpdateShape()
    {
        _capsuleShape.Height = _currentHalfHeight * 2f;
        _bodyColl.Position = new Vector3(0, _currentHalfHeight, 0);

        // Position head/camera at eye level (slightly below top)
        float eyeY = _currentHalfHeight * 2f - EyeOffsetFromTop;
        _headNode.Position = new Vector3(0, eyeY, 0);
    }
}