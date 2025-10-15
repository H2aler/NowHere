extends Node3D

# Game Scene Script for Godot
# Unity의 GameScene.cs를 GDScript로 변환

var game_manager: Node
var player: CharacterBody3D
var camera: Camera3D
var ui: Control

func _ready():
	print("Game Scene loaded")
	
	# GameManager 찾기
	game_manager = get_node("/root/GameManager")
	if not game_manager:
		print("GameManager not found, creating...")
		game_manager = preload("res://scripts/GameManager.gd").new()
		game_manager.name = "GameManager"
		get_tree().root.add_child(game_manager)
	
	# 씬 컴포넌트 찾기
	player = $Player
	camera = $Player/Camera3D
	ui = $UI
	
	# UI 버튼 연결
	connect_ui_buttons()
	
	# 게임 시작
	if game_manager:
		game_manager.start_game()

func connect_ui_buttons():
	"""UI 버튼 연결"""
	var pause_button = $UI/PauseButton
	if pause_button:
		pause_button.pressed.connect(_on_pause_button_pressed)

func _on_pause_button_pressed():
	"""일시정지 버튼 클릭"""
	print("Pause button pressed")
	
	if game_manager:
		game_manager.pause_game()

func _input(event):
	"""입력 처리"""
	if event.is_action_pressed("ui_cancel"):
		_on_pause_button_pressed()
	
	if event.is_action_pressed("attack"):
		perform_attack()
	
	if event.is_action_pressed("defend"):
		perform_defend()
	
	if event.is_action_pressed("jump"):
		perform_jump()

func perform_attack():
	"""공격 수행"""
	print("Player attacks!")
	
	if game_manager:
		game_manager.perform_attack()

func perform_defend():
	"""방어 수행"""
	print("Player defends!")
	
	if game_manager:
		game_manager.perform_defend()

func perform_jump():
	"""점프 수행"""
	print("Player jumps!")
	
	if game_manager:
		game_manager.perform_jump()

func _process(delta):
	"""프레임마다 실행"""
	# 게임 로직 업데이트
	# TODO: 게임 로직 구현
	pass
