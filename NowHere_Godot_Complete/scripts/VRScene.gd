extends Node3D

# VR Scene Script for Godot
# Unity의 VRScene.cs를 GDScript로 변환

var game_manager: Node
var xr_manager: Node
var xr_origin: XROrigin3D
var xr_camera: XRCamera3D
var left_controller: XRController3D
var right_controller: XRController3D
var ui: Control

func _ready():
	print("VR Scene loaded")
	
	# GameManager 찾기
	game_manager = get_node("/root/GameManager")
	if not game_manager:
		print("GameManager not found, creating...")
		game_manager = preload("res://scripts/GameManager.gd").new()
		game_manager.name = "GameManager"
		get_tree().root.add_child(game_manager)
	
	# XRManager 찾기
	xr_manager = get_node("/root/GameManager/XRManager")
	if not xr_manager:
		print("XRManager not found, creating...")
		xr_manager = preload("res://scripts/XRManager.gd").new()
		xr_manager.name = "XRManager"
		game_manager.add_child(xr_manager)
	
	# 씬 컴포넌트 찾기
	xr_origin = $XROrigin
	xr_camera = $XROrigin/XRCamera
	left_controller = $XROrigin/LeftController
	right_controller = $XROrigin/RightController
	ui = $UI
	
	# UI 버튼 연결
	connect_ui_buttons()
	
	# VR 시작
	if xr_manager:
		xr_manager.start_vr()
	
	# VR 상태 업데이트
	update_vr_status()

func connect_ui_buttons():
	"""UI 버튼 연결"""
	var hand_tracking_button = $UI/HandTrackingButton
	var voice_command_button = $UI/VoiceCommandButton
	var back_button = $UI/BackButton
	
	if hand_tracking_button:
		hand_tracking_button.pressed.connect(_on_hand_tracking_button_pressed)
	
	if voice_command_button:
		voice_command_button.pressed.connect(_on_voice_command_button_pressed)
	
	if back_button:
		back_button.pressed.connect(_on_back_button_pressed)

func _on_hand_tracking_button_pressed():
	"""핸드 트래킹 버튼 클릭"""
	print("Hand Tracking button pressed")
	
	if xr_manager:
		if xr_manager.xr_settings.hand_tracking:
			xr_manager.stop_hand_tracking()
			xr_manager.xr_settings.hand_tracking = false
			print("Hand tracking disabled")
		else:
			xr_manager.start_hand_tracking()
			xr_manager.xr_settings.hand_tracking = true
			print("Hand tracking enabled")
	
	# VR 상태 업데이트
	update_vr_status()

func _on_voice_command_button_pressed():
	"""음성 명령 버튼 클릭"""
	print("Voice Command button pressed")
	
	# 음성 명령 테스트
	if xr_manager:
		xr_manager.process_voice_command("attack")
		xr_manager.process_voice_command("defend")
		xr_manager.process_voice_command("jump")
		xr_manager.process_voice_command("menu")

func _on_back_button_pressed():
	"""뒤로가기 버튼 클릭"""
	print("Back to Menu button pressed")
	
	# VR 중지
	if xr_manager:
		xr_manager.stop_xr()
	
	# 메인 메뉴로 돌아가기
	get_tree().change_scene_to_file("res://scenes/MainMenu.tscn")

func update_vr_status():
	"""VR 상태 업데이트"""
	var status_label = $UI/VRStatus
	if status_label:
		if xr_manager and xr_manager.is_xr_active():
			var mode = xr_manager.get_current_xr_mode()
			var mode_text = ""
			match mode:
				xr_manager.XRMode.VR:
					mode_text = "VR"
				xr_manager.XRMode.AR:
					mode_text = "AR"
				xr_manager.XRMode.MR:
					mode_text = "MR"
			
			status_label.text = mode_text + " Mode Active - Hand Tracking: " + str(xr_manager.xr_settings.hand_tracking)
		else:
			status_label.text = "VR Mode Inactive"

func _input(event):
	"""입력 처리"""
	# VR 컨트롤러 입력 처리
	if event is InputEventJoypadButton:
		if event.pressed:
			handle_controller_input(event.device, event.button_index, true)
		else:
			handle_controller_input(event.device, event.button_index, false)
	
	# 키보드 입력으로 음성 명령 테스트
	if event is InputEventKey and event.pressed:
		match event.keycode:
			KEY_1:
				xr_manager.process_voice_command("attack")
			KEY_2:
				xr_manager.process_voice_command("defend")
			KEY_3:
				xr_manager.process_voice_command("jump")
			KEY_4:
				xr_manager.process_voice_command("menu")
	
	# ESC 키로 메인 메뉴로 돌아가기
	if event.is_action_pressed("ui_cancel"):
		_on_back_button_pressed()

func handle_controller_input(device: int, button: int, pressed: bool):
	"""컨트롤러 입력 처리"""
	print("VR Controller input: device=", device, " button=", button, " pressed=", pressed)
	
	# 실제 구현에서는 XR 컨트롤러 입력 처리
	# 대체 시스템에서는 시뮬레이션된 컨트롤러 입력
	
	# 햅틱 피드백 테스트
	if pressed:
		xr_manager.trigger_haptic_feedback(device, 0.5)

func _process(delta):
	"""프레임마다 실행"""
	# VR 상태 업데이트
	update_vr_status()
	
	# VR 컨트롤러 상태 업데이트
	if xr_manager:
		# 왼쪽 컨트롤러 위치/회전 확인
		var left_pos = xr_manager.get_controller_position(1)
		var left_rot = xr_manager.get_controller_rotation(1)
		
		# 오른쪽 컨트롤러 위치/회전 확인
		var right_pos = xr_manager.get_controller_position(2)
		var right_rot = xr_manager.get_controller_rotation(2)
		
		# 컨트롤러 버튼 상태 확인
		var left_trigger = xr_manager.get_controller_button_pressed(1, "trigger")
		var right_trigger = xr_manager.get_controller_button_pressed(2, "trigger")
		
		# 실제 구현에서는 이 정보를 사용하여 VR 상호작용 구현
