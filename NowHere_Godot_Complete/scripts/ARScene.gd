extends Node3D

# AR Scene Script for Godot
# Unity의 ARScene.cs를 GDScript로 변환

var game_manager: Node
var ar_manager: Node
var ar_camera: Camera3D
var ar_anchor: Node3D
var ui: Control

func _ready():
	print("AR Scene loaded")
	
	# GameManager 찾기
	game_manager = get_node("/root/GameManager")
	if not game_manager:
		print("GameManager not found, creating...")
		game_manager = preload("res://scripts/GameManager.gd").new()
		game_manager.name = "GameManager"
		get_tree().root.add_child(game_manager)
	
	# ARManager 찾기
	ar_manager = get_node("/root/GameManager/ARManager")
	if not ar_manager:
		print("ARManager not found, creating...")
		ar_manager = preload("res://scripts/ARManager.gd").new()
		ar_manager.name = "ARManager"
		game_manager.add_child(ar_manager)
	
	# 씬 컴포넌트 찾기
	ar_camera = $ARCamera
	ar_anchor = $ARAnchor
	ui = $UI
	
	# UI 버튼 연결
	connect_ui_buttons()
	
	# AR 시작
	if ar_manager:
		ar_manager.start_ar()
	
	# AR 상태 업데이트
	update_ar_status()

func connect_ui_buttons():
	"""UI 버튼 연결"""
	var place_button = $UI/PlaceObjectButton
	var back_button = $UI/BackButton
	
	if place_button:
		place_button.pressed.connect(_on_place_object_button_pressed)
	
	if back_button:
		back_button.pressed.connect(_on_back_button_pressed)

func _on_place_object_button_pressed():
	"""오브젝트 배치 버튼 클릭"""
	print("Place Object button pressed")
	
	# AR 오브젝트 배치
	if ar_manager:
		var position = Vector3(0, 0, -2)  # 카메라 앞 2미터
		ar_manager.place_ar_object("res://scenes/ARObject.tscn", position)
	
	# AR 상태 업데이트
	update_ar_status()

func _on_back_button_pressed():
	"""뒤로가기 버튼 클릭"""
	print("Back to Menu button pressed")
	
	# AR 중지
	if ar_manager:
		ar_manager.stop_ar()
	
	# 메인 메뉴로 돌아가기
	get_tree().change_scene_to_file("res://scenes/MainMenu.tscn")

func update_ar_status():
	"""AR 상태 업데이트"""
	var status_label = $UI/ARStatus
	if status_label:
		if ar_manager and ar_manager.is_ar_active():
			status_label.text = "AR Mode Active - Objects: " + str(ar_manager.get_ar_objects().size())
		else:
			status_label.text = "AR Mode Inactive"

func _input(event):
	"""입력 처리"""
	# 터치 입력으로 AR 오브젝트 배치
	if event is InputEventScreenTouch:
		if event.pressed:
			_on_place_object_button_pressed()
	
	# 마우스 클릭으로 AR 오브젝트 배치 (테스트용)
	if event is InputEventMouseButton:
		if event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
			_on_place_object_button_pressed()
	
	# ESC 키로 메인 메뉴로 돌아가기
	if event.is_action_pressed("ui_cancel"):
		_on_back_button_pressed()

func _process(delta):
	"""프레임마다 실행"""
	# AR 상태 업데이트
	update_ar_status()
	
	# AR 카메라 위치 업데이트
	if ar_manager and ar_camera:
		# 실제 구현에서는 ARCore/WebXR의 카메라 위치 사용
		# 대체 시스템에서는 시뮬레이션된 카메라 움직임
		pass
