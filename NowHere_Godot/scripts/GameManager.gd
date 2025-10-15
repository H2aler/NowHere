extends Node

# NowHere Game Manager
# Unity의 GameManager.cs를 GDScript로 변환

enum GameState {
	MAIN_MENU,
	PLAYING,
	PAUSED,
	GAME_OVER
}

var current_state: GameState = GameState.MAIN_MENU
var enable_ar_mode: bool = true
var enable_multiplayer: bool = true
var enable_vr_mode: bool = true

# 게임 컴포넌트들
var ar_manager: Node
var network_manager: Node
var ui_manager: Node
var audio_manager: Node
var save_system: Node

func _ready():
	print("=== NowHere Game Manager Started ===")
	initialize_game_components()
	setup_game_systems()

func initialize_game_components():
	# AR Manager 초기화
	if enable_ar_mode:
		ar_manager = get_node_or_null("ARManager")
		if ar_manager:
			print("AR Manager initialized")
	
	# Network Manager 초기화
	if enable_multiplayer:
		network_manager = get_node_or_null("NetworkManager")
		if network_manager:
			print("Network Manager initialized")
	
	# UI Manager 초기화
	ui_manager = get_node_or_null("UIManager")
	if ui_manager:
		print("UI Manager initialized")
	
	# Audio Manager 초기화
	audio_manager = get_node_or_null("AudioManager")
	if audio_manager:
		print("Audio Manager initialized")
	
	# Save System 초기화
	save_system = get_node_or_null("SaveSystem")
	if save_system:
		print("Save System initialized")

func setup_game_systems():
	# 게임 시스템 설정
	print("Setting up game systems...")
	
	# AR 시스템 설정
	if enable_ar_mode and ar_manager:
		print("AR system enabled")
	
	# VR 시스템 설정
	if enable_vr_mode:
		print("VR system enabled")
	
	# 멀티플레이어 시스템 설정
	if enable_multiplayer and network_manager:
		print("Multiplayer system enabled")

func start_game():
	"""게임 시작"""
	if current_state == GameState.MAIN_MENU:
		set_game_state(GameState.PLAYING)
		print("Game started!")
		
		# AR 모드 활성화
		if enable_ar_mode and ar_manager:
			print("AR mode activated")
		
		# 네트워킹 시작
		if enable_multiplayer and network_manager:
			print("Networking started")
		
		# UI 업데이트
		print("UI updated")

func set_game_state(new_state: GameState):
	"""게임 상태 변경"""
	var old_state = current_state
	current_state = new_state
	print("Game state changed: ", GameState.keys()[old_state], " -> ", GameState.keys()[new_state])

func pause_game():
	"""게임 일시정지"""
	if current_state == GameState.PLAYING:
		set_game_state(GameState.PAUSED)
		get_tree().paused = true
		print("Game paused")

func resume_game():
	"""게임 재개"""
	if current_state == GameState.PAUSED:
		set_game_state(GameState.PLAYING)
		get_tree().paused = false
		print("Game resumed")

func end_game():
	"""게임 종료"""
	set_game_state(GameState.GAME_OVER)
	print("Game ended")

func _input(event):
	"""입력 처리"""
	if event.is_action_pressed("ui_accept"):
		if current_state == GameState.MAIN_MENU:
			start_game()
		elif current_state == GameState.PAUSED:
			resume_game()
	
	if event.is_action_pressed("ui_cancel"):
		if current_state == GameState.PLAYING:
			pause_game()
		elif current_state == GameState.PAUSED:
			resume_game()
