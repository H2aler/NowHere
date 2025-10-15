extends Control

# UI Manager for Godot
# Unity의 UIManager.cs를 GDScript로 변환

var main_menu: Control
var game_ui: Control
var pause_menu: Control
var settings_menu: Control

func _ready():
	print("UI Manager initialized")
	setup_ui()

func setup_ui():
	"""UI 설정"""
	print("Setting up UI...")
	
	# 메인 메뉴 설정
	main_menu = get_node_or_null("MainMenu")
	if main_menu:
		print("Main menu found")
	
	# 게임 UI 설정
	game_ui = get_node_or_null("GameUI")
	if game_ui:
		print("Game UI found")
	
	# 일시정지 메뉴 설정
	pause_menu = get_node_or_null("PauseMenu")
	if pause_menu:
		print("Pause menu found")
	
	# 설정 메뉴 설정
	settings_menu = get_node_or_null("SettingsMenu")
	if settings_menu:
		print("Settings menu found")

func show_main_menu():
	"""메인 메뉴 표시"""
	print("Showing main menu")
	
	if main_menu:
		main_menu.visible = true
	if game_ui:
		game_ui.visible = false
	if pause_menu:
		pause_menu.visible = false
	if settings_menu:
		settings_menu.visible = false

func show_game_ui():
	"""게임 UI 표시"""
	print("Showing game UI")
	
	if main_menu:
		main_menu.visible = false
	if game_ui:
		game_ui.visible = true
	if pause_menu:
		pause_menu.visible = false
	if settings_menu:
		settings_menu.visible = false

func show_pause_menu():
	"""일시정지 메뉴 표시"""
	print("Showing pause menu")
	
	if main_menu:
		main_menu.visible = false
	if game_ui:
		game_ui.visible = true
	if pause_menu:
		pause_menu.visible = true
	if settings_menu:
		settings_menu.visible = false

func show_settings_menu():
	"""설정 메뉴 표시"""
	print("Showing settings menu")
	
	if main_menu:
		main_menu.visible = false
	if game_ui:
		game_ui.visible = false
	if pause_menu:
		pause_menu.visible = false
	if settings_menu:
		settings_menu.visible = true

func update_health_bar(current_health: int, max_health: int):
	"""체력 바 업데이트"""
	var health_bar = get_node_or_null("GameUI/HealthBar")
	if health_bar:
		var health_percentage = float(current_health) / float(max_health)
		health_bar.value = health_percentage * 100
		print("Health updated: ", current_health, "/", max_health)

func update_mana_bar(current_mana: int, max_mana: int):
	"""마나 바 업데이트"""
	var mana_bar = get_node_or_null("GameUI/ManaBar")
	if mana_bar:
		var mana_percentage = float(current_mana) / float(max_mana)
		mana_bar.value = mana_percentage * 100
		print("Mana updated: ", current_mana, "/", max_mana)

func update_experience_bar(current_exp: int, max_exp: int):
	"""경험치 바 업데이트"""
	var exp_bar = get_node_or_null("GameUI/ExperienceBar")
	if exp_bar:
		var exp_percentage = float(current_exp) / float(max_exp)
		exp_bar.value = exp_percentage * 100
		print("Experience updated: ", current_exp, "/", max_exp)

func show_notification(message: String, duration: float = 3.0):
	"""알림 표시"""
	print("Notification: ", message)
	
	# 알림 UI 생성
	var notification = Label.new()
	notification.text = message
	notification.add_theme_color_override("font_color", Color.WHITE)
	notification.add_theme_font_size_override("font_size", 24)
	
	add_child(notification)
	
	# 알림 위치 설정
	notification.position = Vector2(50, 50)
	
	# 알림 애니메이션
	var tween = create_tween()
	tween.tween_property(notification, "modulate:a", 0.0, duration)
	tween.tween_callback(notification.queue_free)

func show_dialog(title: String, message: String):
	"""다이얼로그 표시"""
	print("Dialog: ", title, " - ", message)
	
	# 다이얼로그 UI 생성
	var dialog = AcceptDialog.new()
	dialog.title = title
	dialog.dialog_text = message
	dialog.popup_centered()
	
	add_child(dialog)

func _input(event):
	"""입력 처리"""
	if event.is_action_pressed("ui_cancel"):
		# ESC 키로 일시정지 메뉴 토글
		if pause_menu and pause_menu.visible:
			show_game_ui()
		elif game_ui and game_ui.visible:
			show_pause_menu()
