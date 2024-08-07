extends Node


func line(start: Vector3, end: Vector3, color = Color.WHITE) -> MeshInstance3D:
	var mesh_instance = MeshInstance3D.new()
	var imediate_mesh = ImmediateMesh.new()
	var material = ORMMaterial3D.new()
	
	material.albedo_color = color
	material.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	mesh_instance.mesh = imediate_mesh
	mesh_instance.cast_shadow = false
	imediate_mesh.surface_begin(Mesh.PRIMITIVE_LINES, material)
	imediate_mesh.surface_add_vertex(start)
	imediate_mesh.surface_add_vertex(end)
	imediate_mesh.surface_end()
	
	get_tree().root.add_child(mesh_instance)
	return mesh_instance

