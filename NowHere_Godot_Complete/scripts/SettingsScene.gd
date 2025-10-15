extends Control

# Settings Scene Script for Godot
# Unity의 SettingsScene.cs를 GDScript로 변환

var game_manager: Node
var settings_data: Dictionary = {}

func _ready():
	print("Settings Scene loaded")
	
	# GameManager 찾기
	game_manager = get_node("/root/GameManager")
	if not game_manager:
		print("GameManager not found, creating...")
		game_manager = preload("res://scripts/GameManager.gd").new()
		game_manager.name = "GameManager"
		get_tree().root.add_child(game_manager)
	
	# 설정 데이터 로드
	load_settings()
	
	# UI 컴포넌트 연결
	connect_ui_components()
	
	# 설정 값 적용
	apply_settings_to_ui()

func load_settings():
	"""설정 데이터 로드"""
	if game_manager and game_manager.game_settings:
		settings_data = game_manager.game_settings.duplicate()
	else:
		# 기본 설정
		settings_data = {
			"master_volume": 1.0,
			"music_volume": 0.8,
			"sfx_volume": 1.0,
			"voice_volume": 1.0,
			"ar_enabled": true,
			"vr_enabled": true,
			"multiplayer_enabled": true,
			"graphics_quality": "high",
			"language": "ko"
		}

func connect_ui_components():
	"""UI 컴포넌트 연결"""
	# 볼륨 슬라이더 연결
	var master_volume_slider = $MasterVolumeSlider
	var music_volume_slider = $MusicVolumeSlider
	var sfx_volume_slider = $SFXVolumeSlider
	var voice_volume_slider = $VoiceVolumeSlider
	
	if master_volume_slider:
		master_volume_slider.value_changed.connect(_on_master_volume_changed)
	
	if music_volume_slider:
		music_volume_slider.value_changed.connect(_on_music_volume_changed)
	
	if sfx_volume_slider:
		sfx_volume_slider.value_changed.connect(_on_sfx_volume_changed)
	
	if voice_volume_slider:
		voice_volume_slider.value_changed.connect(_on_voice_volume_changed)
	
	# 체크박스 연결
	var ar_mode_checkbox = $ARModeCheckBox
	var vr_mode_checkbox = $VRModeCheckBox
	var multiplayer_checkbox = $MultiplayerCheckBox
	
	if ar_mode_checkbox:
		ar_mode_checkbox.toggled.connect(_on_ar_mode_toggled)
	
	if vr_mode_checkbox:
		vr_mode_checkbox.toggled.connect(_on_vr_mode_toggled)
	
	if multiplayer_checkbox:
		multiplayer_checkbox.toggled.connect(_on_multiplayer_toggled)
	
	# 옵션 버튼 연결
	var graphics_quality_option = $GraphicsQualityOption
	var language_option = $LanguageOption
	
	if graphics_quality_option:
		graphics_quality_option.item_selected.connect(_on_graphics_quality_selected)
	
	if language_option:
		language_option.item_selected.connect(_on_language_selected)
	
	# 버튼 연결
	var save_button = $SaveButton
	var back_button = $BackButton
	
	if save_button:
		save_button.pressed.connect(_on_save_button_pressed)
	
	if back_button:
		back_button.pressed.connect(_on_back_button_pressed)

func apply_settings_to_ui():
	"""설정 값을 UI에 적용"""
	# 볼륨 슬라이더 값 설정
	$MasterVolumeSlider.value = settings_data.master_volume
	$MusicVolumeSlider.value = settings_data.music_volume
	$SFXVolumeSlider.value = settings_data.sfx_volume
	$VoiceVolumeSlider.value = settings_data.voice_volume
	
	# 체크박스 상태 설정
	$ARModeCheckBox.button_pressed = settings_data.ar_enabled
	$VRModeCheckBox.button_pressed = settings_data.vr_enabled
	$MultiplayerCheckBox.button_pressed = settings_data.multiplayer_enabled
	
	# 그래픽 품질 옵션 설정
	var graphics_quality_option = $GraphicsQualityOption
	graphics_quality_option.clear()
	graphics_quality_option.add_item("Low")
	graphics_quality_option.add_item("Medium")
	graphics_quality_option.add_item("High")
	graphics_quality_option.add_item("Ultra")
	
	match settings_data.graphics_quality:
		"low":
			graphics_quality_option.selected = 0
		"medium":
			graphics_quality_option.selected = 1
		"high":
			graphics_quality_option.selected = 2
		"ultra":
			graphics_quality_option.selected = 3
	
	# 언어 옵션 설정
	var language_option = $LanguageOption
	language_option.clear()
	language_option.add_item("한국어")
	language_option.add_item("English")
	language_option.add_item("日本語")
	language_option.add_item("中文")
	
	match settings_data.language:
		"ko":
			language_option.selected = 0
		"en":
			language_option.selected = 1
		"ja":
			language_option.selected = 2
		"zh":
			language_option.selected = 3

func _on_master_volume_changed(value: float):
	"""마스터 볼륨 변경"""
	settings_data.master_volume = value
	print("Master volume changed to: ", value)
	
	# 오디오 매니저에 적용
	if game_manager and game_manager.audio_manager:
		game_manager.audio_manager.set_master_volume(value)

func _on_music_volume_changed(value: float):
	"""음악 볼륨 변경"""
	settings_data.music_volume = value
	print("Music volume changed to: ", value)
	
	# 오디오 매니저에 적용
	if game_manager and game_manager.audio_manager:
		game_manager.audio_manager.set_music_volume(value)

func _on_sfx_volume_changed(value: float):
	"""효과음 볼륨 변경"""
	settings_data.sfx_volume = value
	print("SFX volume changed to: ", value)
	
	# 오디오 매니저에 적용
	if game_manager and game_manager.audio_manager:
		game_manager.audio_manager.set_sfx_volume(value)

func _on_voice_volume_changed(value: float):
	"""음성 볼륨 변경"""
	settings_data.voice_volume = value
	print("Voice volume changed to: ", value)
	
	# 오디오 매니저에 적용
	if game_manager and game_manager.audio_manager:
		game_manager.audio_manager.set_voice_volume(value)

func _on_ar_mode_toggled(enabled: bool):
	"""AR 모드 토글"""
	settings_data.ar_enabled = enabled
	print("AR mode toggled: ", enabled)
	
	# AR 매니저에 적용
	if game_manager:
		game_manager.enable_ar_mode = enabled

func _on_vr_mode_toggled(enabled: bool):
	"""VR 모드 토글"""
	settings_data.vr_enabled = enabled
	print("VR mode toggled: ", enabled)
	
	# XR 매니저에 적용
	if game_manager:
		game_manager.enable_vr_mode = enabled

func _on_multiplayer_toggled(enabled: bool):
	"""멀티플레이어 토글"""
	settings_data.multiplayer_enabled = enabled
	print("Multiplayer toggled: ", enabled)
	
	# 네트워크 매니저에 적용
	if game_manager:
		game_manager.enable_multiplayer = enabled

func _on_graphics_quality_selected(index: int):
	"""그래픽 품질 선택"""
	var quality_options = ["low", "medium", "high", "ultra"]
	settings_data.graphics_quality = quality_options[index]
	print("Graphics quality changed to: ", settings_data.graphics_quality)
	
	# 그래픽 설정 적용
	apply_graphics_settings()

func _on_language_selected(index: int):
	"""언어 선택"""
	var language_options = ["ko", "en", "ja", "zh"]
	settings_data.language = language_options[index]
	print("Language changed to: ", settings_data.language)
	
	# 언어 설정 적용
	apply_language_settings()

func apply_graphics_settings():
	"""그래픽 설정 적용"""
	match settings_data.graphics_quality:
		"low":
			# 낮은 품질 설정
			RenderingServer.viewport_set_render_direct_to_screen(get_viewport().get_viewport_rid(), true)
		"medium":
			# 중간 품질 설정
			RenderingServer.viewport_set_render_direct_to_screen(get_viewport().get_viewport_rid(), true)
		"high":
			# 높은 품질 설정
			RenderingServer.viewport_set_render_direct_to_screen(get_viewport().get_viewport_rid(), true)
		"ultra":
			# 최고 품질 설정
			RenderingServer.viewport_set_render_direct_to_screen(get_viewport().get_viewport_rid(), true)

func apply_language_settings():
	"""언어 설정 적용"""
	# 실제 구현에서는 다국어 지원 시스템 사용
	# 현재는 콘솔 출력으로 대체
	print("Language settings applied: ", settings_data.language)

func _on_save_button_pressed():
	"""설정 저장 버튼 클릭"""
	print("Save Settings button pressed")
	
	# 설정 저장
	save_settings()
	
	# GameManager에 설정 적용
	if game_manager:
		game_manager.game_settings = settings_data.duplicate()
	
	# 저장 완료 메시지
	show_save_message()

func _on_back_button_pressed():
	"""뒤로가기 버튼 클릭"""
	print("Back to Menu button pressed")
	
	# 메인 메뉴로 돌아가기
	get_tree().change_scene_to_file("res://scenes/MainMenu.tscn")

func save_settings():
	"""설정 저장"""
	print("Saving settings...")
	
	# 실제 구현에서는 파일 시스템에 설정 저장
	# 현재는 메모리에만 저장
	print("Settings saved successfully")

func show_save_message():
	"""저장 완료 메시지 표시"""
	# 실제 구현에서는 UI 메시지 표시
	# 현재는 콘솔 출력으로 대체
	print("Settings saved successfully!")

func _input(event):
	"""입력 처리"""
	# ESC 키로 메인 메뉴로 돌아가기
	if event.is_action_pressed("ui_cancel"):
		_on_back_button_pressed()
	
	# Enter 키로 설정 저장
	if event.is_action_pressed("ui_accept"):
		_on_save_button_pressed()
