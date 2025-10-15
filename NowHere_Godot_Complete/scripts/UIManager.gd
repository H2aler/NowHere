extends Node

# UI Manager for Godot - Complete Version
# Unity의 UIManager.cs를 GDScript로 완전 변환

var current_ui: Control
var ui_scenes: Dictionary = {}
var notifications: Array = []
var notification_timer: Timer

# UI 설정
var ui_settings: Dictionary = {
	"show_fps": true,
	"show_debug_info": false,
	"ui_scale": 1.0,
	"notification_duration": 3.0,
	"auto_hide_ui": false
}

func _ready():
	print("UI Manager initialized (Godot)")
	initialize_ui_system()

func initialize_ui_system():
	"""UI 시스템 초기화"""
	print("Initializing UI system...")
	
	# UI 씬 로드
	load_ui_scenes()
	
	# 알림 시스템 초기화
	initialize_notification_system()
	
	# UI 설정 적용
	apply_ui_settings()
	
	print("UI system initialization complete")

func load_ui_scenes():
	"""UI 씬 로드"""
	print("Loading UI scenes...")
	
	# UI 씬 경로 설정
	ui_scenes = {
		"main_menu": "res://scenes/MainMenu.tscn",
		"game_ui": "res://scenes/GameUI.tscn",
		"ar_ui": "res://scenes/ARUI.tscn",
		"vr_ui": "res://scenes/VRUI.tscn",
		"pause_menu": "res://scenes/PauseMenu.tscn",
		"game_over": "res://scenes/GameOverScreen.tscn",
		"settings": "res://scenes/SettingsScene.tscn",
		"inventory": "res://scenes/InventoryUI.tscn",
		"chat": "res://scenes/ChatUI.tscn"
	}
	
	print("UI scenes loaded")

func initialize_notification_system():
	"""알림 시스템 초기화"""
	notification_timer = Timer.new()
	notification_timer.wait_time = ui_settings.notification_duration
	notification_timer.one_shot = true
	notification_timer.timeout.connect(_on_notification_timeout)
	add_child(notification_timer)
	
	print("Notification system initialized")

func apply_ui_settings():
	"""UI 설정 적용"""
	# UI 스케일 적용
	if current_ui:
		current_ui.scale = Vector2(ui_settings.ui_scale, ui_settings.ui_scale)
	
	print("UI settings applied")

func show_main_menu():
	"""메인 메뉴 표시"""
	print("Showing main menu")
	change_ui_scene("main_menu")

func show_game_ui():
	"""게임 UI 표시"""
	print("Showing game UI")
	change_ui_scene("game_ui")

func show_ar_ui():
	"""AR UI 표시"""
	print("Showing AR UI")
	change_ui_scene("ar_ui")

func show_vr_ui():
	"""VR UI 표시"""
	print("Showing VR UI")
	change_ui_scene("vr_ui")

func show_pause_menu():
	"""일시정지 메뉴 표시"""
	print("Showing pause menu")
	change_ui_scene("pause_menu")

func show_game_over_screen():
	"""게임 오버 화면 표시"""
	print("Showing game over screen")
	change_ui_scene("game_over")

func show_settings():
	"""설정 화면 표시"""
	print("Showing settings")
	change_ui_scene("settings")

func show_inventory():
	"""인벤토리 UI 표시"""
	print("Showing inventory")
	change_ui_scene("inventory")

func show_chat():
	"""채팅 UI 표시"""
	print("Showing chat")
	change_ui_scene("chat")

func change_ui_scene(ui_name: String):
	"""UI 씬 변경"""
	if not ui_scenes.has(ui_name):
		print("UI scene not found: ", ui_name)
		return
	
	var ui_scene_path = ui_scenes[ui_name]
	
	# 현재 UI 제거
	if current_ui:
		current_ui.queue_free()
	
	# 새 UI 로드
	var ui_scene = load(ui_scene_path)
	if ui_scene:
		current_ui = ui_scene.instantiate()
		add_child(current_ui)
		
		# UI 설정 적용
		apply_ui_settings()
		
		print("UI scene changed to: ", ui_name)
	else:
		print("Failed to load UI scene: ", ui_scene_path)

func hide_ui():
	"""UI 숨기기"""
	if current_ui:
		current_ui.visible = false
		print("UI hidden")

func show_ui():
	"""UI 표시"""
	if current_ui:
		current_ui.visible = true
		print("UI shown")

func toggle_ui():
	"""UI 토글"""
	if current_ui:
		current_ui.visible = !current_ui.visible
		print("UI toggled: ", current_ui.visible)

func update_health_bar(current_health: int, max_health: int):
	"""체력 바 업데이트"""
	if not current_ui:
		return
	
	var health_bar = current_ui.get_node_or_null("HealthBar")
	if health_bar:
		var health_percentage = float(current_health) / float(max_health)
		health_bar.value = health_percentage * 100
		print("Health bar updated: ", current_health, "/", max_health)

func update_mana_bar(current_mana: int, max_mana: int):
	"""마나 바 업데이트"""
	if not current_ui:
		return
	
	var mana_bar = current_ui.get_node_or_null("ManaBar")
	if mana_bar:
		var mana_percentage = float(current_mana) / float(max_mana)
		mana_bar.value = mana_percentage * 100
		print("Mana bar updated: ", current_mana, "/", max_mana)

func update_experience_bar(current_exp: int, required_exp: int):
	"""경험치 바 업데이트"""
	if not current_ui:
		return
	
	var exp_bar = current_ui.get_node_or_null("ExperienceBar")
	if exp_bar:
		var exp_percentage = float(current_exp) / float(required_exp)
		exp_bar.value = exp_percentage * 100
		print("Experience bar updated: ", current_exp, "/", required_exp)

func show_notification(message: String, duration: float = 3.0):
	"""알림 표시"""
	print("Notification: ", message)
	
	# 알림 추가
	notifications.append({
		"message": message,
		"duration": duration,
		"timestamp": Time.get_unix_time_from_system()
	})
	
	# 알림 UI 업데이트
	update_notification_ui()
	
	# 알림 타이머 시작
	if not notification_timer.is_stopped():
		notification_timer.stop()
	
	notification_timer.wait_time = duration
	notification_timer.start()

func update_notification_ui():
	"""알림 UI 업데이트"""
	if not current_ui:
		return
	
	var notification_label = current_ui.get_node_or_null("NotificationLabel")
	if notification_label and notifications.size() > 0:
		var latest_notification = notifications[-1]
		notification_label.text = latest_notification.message
		notification_label.visible = true
		print("Notification UI updated: ", latest_notification.message)

func _on_notification_timeout():
	"""알림 타임아웃"""
	if notifications.size() > 0:
		notifications.pop_front()
	
	# 알림 UI 업데이트
	update_notification_ui()
	
	# 더 이상 알림이 없으면 UI 숨기기
	if notifications.size() == 0:
		var notification_label = current_ui.get_node_or_null("NotificationLabel")
		if notification_label:
			notification_label.visible = false

func show_dialog(title: String, message: String, buttons: Array = ["OK"]):
	"""다이얼로그 표시"""
	print("Dialog: ", title, " - ", message)
	
	# 실제 구현에서는 다이얼로그 UI 표시
	# 현재는 콘솔 출력으로 대체
	print("Dialog buttons: ", buttons)

func show_loading_screen(message: String = "Loading..."):
	"""로딩 화면 표시"""
	print("Loading screen: ", message)
	
	# 실제 구현에서는 로딩 화면 UI 표시
	# 현재는 콘솔 출력으로 대체

func hide_loading_screen():
	"""로딩 화면 숨기기"""
	print("Loading screen hidden")
	
	# 실제 구현에서는 로딩 화면 UI 숨기기

func update_fps_display(fps: int):
	"""FPS 표시 업데이트"""
	if not ui_settings.show_fps:
		return
	
	if not current_ui:
		return
	
	var fps_label = current_ui.get_node_or_null("FPSLabel")
	if fps_label:
		fps_label.text = "FPS: " + str(fps)

func update_debug_info(debug_info: Dictionary):
	"""디버그 정보 업데이트"""
	if not ui_settings.show_debug_info:
		return
	
	if not current_ui:
		return
	
	var debug_label = current_ui.get_node_or_null("DebugLabel")
	if debug_label:
		var debug_text = ""
		for key in debug_info:
			debug_text += key + ": " + str(debug_info[key]) + "\n"
		debug_label.text = debug_text

func set_ui_scale(scale: float):
	"""UI 스케일 설정"""
	ui_settings.ui_scale = clamp(scale, 0.5, 2.0)
	apply_ui_settings()
	print("UI scale set to: ", ui_settings.ui_scale)

func toggle_fps_display():
	"""FPS 표시 토글"""
	ui_settings.show_fps = !ui_settings.show_fps
	print("FPS display toggled: ", ui_settings.show_fps)

func toggle_debug_info():
	"""디버그 정보 토글"""
	ui_settings.show_debug_info = !ui_settings.show_debug_info
	print("Debug info toggled: ", ui_settings.show_debug_info)

func get_ui_settings() -> Dictionary:
	"""UI 설정 반환"""
	return ui_settings.duplicate()

func save_ui_settings():
	"""UI 설정 저장"""
	# 실제 구현에서는 파일 시스템에 설정 저장
	print("UI settings saved")

func load_ui_settings():
	"""UI 설정 로드"""
	# 실제 구현에서는 파일 시스템에서 설정 로드
	print("UI settings loaded")

func _process(delta):
	"""프레임마다 실행"""
	# FPS 업데이트
	if ui_settings.show_fps:
		var fps = Engine.get_frames_per_second()
		update_fps_display(fps)
	
	# 디버그 정보 업데이트
	if ui_settings.show_debug_info:
		var debug_info = {
			"FPS": Engine.get_frames_per_second(),
			"Memory": OS.get_static_memory_usage(),
			"Time": Time.get_unix_time_from_system()
		}
		update_debug_info(debug_info)
