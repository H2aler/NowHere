extends Control

# Main Menu Script for Godot
# Unity의 MainMenu.cs를 GDScript로 변환

var game_manager: Node

func _ready():
	print("Main Menu loaded")
	
	# GameManager 찾기
	game_manager = get_node("/root/GameManager")
	if not game_manager:
		print("GameManager not found, creating...")
		game_manager = preload("res://scripts/GameManager.gd").new()
		game_manager.name = "GameManager"
		get_tree().root.add_child(game_manager)
	
	# 버튼 연결
	connect_buttons()

func connect_buttons():
	"""버튼 연결"""
	var start_button = $StartButton
	var ar_button = $ARButton
	var vr_button = $VRButton
	var settings_button = $SettingsButton
	var exit_button = $ExitButton
	
	if start_button:
		start_button.pressed.connect(_on_start_button_pressed)
	
	if ar_button:
		ar_button.pressed.connect(_on_ar_button_pressed)
	
	if vr_button:
		vr_button.pressed.connect(_on_vr_button_pressed)
	
	if settings_button:
		settings_button.pressed.connect(_on_settings_button_pressed)
	
	if exit_button:
		exit_button.pressed.connect(_on_exit_button_pressed)

func _on_start_button_pressed():
	"""게임 시작 버튼 클릭"""
	print("Start Game button pressed")
	
	if game_manager:
		game_manager.start_game()
	
	# 게임 씬으로 전환
	get_tree().change_scene_to_file("res://scenes/GameScene.tscn")

func _on_ar_button_pressed():
	"""AR 모드 버튼 클릭"""
	print("AR Mode button pressed")
	
	if game_manager:
		game_manager.start_ar_mode()
	
	# AR 씬으로 전환
	get_tree().change_scene_to_file("res://scenes/ARScene.tscn")

func _on_vr_button_pressed():
	"""VR 모드 버튼 클릭"""
	print("VR Mode button pressed")
	
	if game_manager:
		game_manager.start_vr_mode()
	
	# VR 씬으로 전환
	get_tree().change_scene_to_file("res://scenes/VRScene.tscn")

func _on_settings_button_pressed():
	"""설정 버튼 클릭"""
	print("Settings button pressed")
	
	# 설정 씬으로 전환
	get_tree().change_scene_to_file("res://scenes/SettingsScene.tscn")

func _on_exit_button_pressed():
	"""종료 버튼 클릭"""
	print("Exit button pressed")
	
	# 게임 종료
	get_tree().quit()

func _input(event):
	"""입력 처리"""
	if event.is_action_pressed("ui_accept"):
		_on_start_button_pressed()
	
	if event.is_action_pressed("ui_cancel"):
		_on_exit_button_pressed()
