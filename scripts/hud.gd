extends CanvasLayer


@export var player: CharacterBody3D


func _ready():
	pass

func _physics_process(delta):
	var vel2d = Vector2(player.velocity.x, player.velocity.z)
	
	$velocityLabel.text = "%v" % player.velocity
	$VelocityLabelMag.text = "%f (xz %f)" % [player.velocity.length(), vel2d.length()]
	$VelocityLabelAngle.text = "%d°" % rad_to_deg(Vector3(vel2d.x, 0, vel2d.y).angle_to(-player.transform.basis.z))
	$groundedLabel.text = "Grounded" if player.is_on_ground else "In air"

	$FPSLabel.text = "FPS: %s" % int(1/delta)
