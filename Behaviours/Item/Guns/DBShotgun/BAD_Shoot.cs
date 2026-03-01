using Godot;

public partial class BAD_Shoot : Node
{
	[Export] private Node3D Muzzle;                    // Assign your muzzle Marker3D/Node3D in inspector

	[Export] private float MaxRange { get; set; } = 1000.0f;
	[Export(PropertyHint.Layers3DPhysics)]
	private uint CollisionMask { get; set; } = 0b00000001; // e.g. layer 1

	[Export] private float Damage { get; set; } = 25.0f;
	[Export] private PackedScene BulletImpactEffect;   // Optional particles/decal

	// Cache character reference once (faster & cleaner)
	private CharacterBody3D _character;

	public override void _Ready()
	{
		Node current = this;

		while (current != null)
		{
			if (current is CharacterBody3D body)
			{
				_character = body;
				break;
			}
			current = current.GetParent();
		}

		if (_character == null)
		{
			GD.PrintErr("BAD_Shoot: Could not find any CharacterBody3D in parent chain!");
		}

		if (Muzzle == null)
		{
			GD.PrintErr("BAD_Shoot: Muzzle not assigned!");
		}
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("shoot"))
		{

			GD.Print("Shooting...");
			Shoot();
		}
	}

	private void Shoot()
	{
		if (Muzzle == null || _character == null) return;

		var spaceState = _character.GetWorld3D().DirectSpaceState;
		if (spaceState == null) return;

		Vector3 origin = Muzzle.GlobalPosition;
		Vector3 forward = -Muzzle.GlobalTransform.Basis.Z.Normalized(); // -Z = forward in Godot
		Vector3 end = origin + forward * MaxRange;

		var query = PhysicsRayQueryParameters3D.Create(origin, end);
		query.CollisionMask = CollisionMask;
		query.CollideWithAreas = false;
		query.CollideWithBodies = true;

		// Exclude player's collision object (CharacterBody3D has RID)
		query.Exclude = new Godot.Collections.Array<Rid> { _character.GetRid() };

		var result = spaceState.IntersectRay(query);

		if (result.Count > 0)
		{
			Vector3 hitPos = (Vector3)result["position"];
			Node collider = (Node)result["collider"];

			GD.Print($"Hit: {collider.Name} at {hitPos}");

			// Apply damage if target has the method
			if (collider.HasMethod("TakeDamage"))
			{
				collider.Call("TakeDamage", Damage);
			}

			// // Spawn impact effect
			// if (BulletImpactEffect != null)
			// {
			//     var impact = BulletImpactEffect.Instantiate<Node3D>();
			//     GetTree().CurrentScene.AddChild(impact);
			//     impact.GlobalPosition = hitPos;

			//     // Face surface normal (nice for decals/particles)
			//     Vector3 normal = (Vector3)result["normal"];
			//     impact.LookAt(hitPos + normal, Vector3.Up);
			// }
		}
		else
		{
			GD.Print("Shot missed (no hit within range)");
		}
	}
}