extends Node

# Audio Manager for Godot
# Unity의 AudioManager.cs를 GDScript로 변환

var music_player: AudioStreamPlayer
var sfx_player: AudioStreamPlayer
var voice_player: AudioStreamPlayer

var master_volume: float = 1.0
var music_volume: float = 0.8
var sfx_volume: float = 1.0
var voice_volume: float = 1.0

var background_music: AudioStream
var sound_effects: Dictionary = {}
var voice_clips: Dictionary = {}

func _ready():
	print("Audio Manager initialized")
	setup_audio_players()

func setup_audio_players():
	"""오디오 플레이어 설정"""
	print("Setting up audio players...")
	
	# 배경음악 플레이어
	music_player = AudioStreamPlayer.new()
	music_player.name = "MusicPlayer"
	add_child(music_player)
	
	# 효과음 플레이어
	sfx_player = AudioStreamPlayer.new()
	sfx_player.name = "SFXPlayer"
	add_child(sfx_player)
	
	# 음성 플레이어
	voice_player = AudioStreamPlayer.new()
	voice_player.name = "VoicePlayer"
	add_child(voice_player)
	
	print("Audio players setup complete")

func load_audio_resources():
	"""오디오 리소스 로드"""
	print("Loading audio resources...")
	
	# 배경음악 로드
	background_music = load("res://audio/background_music.ogg")
	if background_music:
		print("Background music loaded")
	
	# 효과음 로드
	sound_effects["sword_swing"] = load("res://audio/sword_swing.ogg")
	sound_effects["magic_cast"] = load("res://audio/magic_cast.ogg")
	sound_effects["item_pickup"] = load("res://audio/item_pickup.ogg")
	sound_effects["level_up"] = load("res://audio/level_up.ogg")
	
	# 음성 클립 로드
	voice_clips["attack"] = load("res://audio/voice_attack.ogg")
	voice_clips["defend"] = load("res://audio/voice_defend.ogg")
	voice_clips["heal"] = load("res://audio/voice_heal.ogg")
	
	print("Audio resources loaded")

func play_background_music():
	"""배경음악 재생"""
	if background_music and music_player:
		music_player.stream = background_music
		music_player.volume_db = linear_to_db(music_volume * master_volume)
		music_player.play()
		print("Background music started")

func stop_background_music():
	"""배경음악 중지"""
	if music_player:
		music_player.stop()
		print("Background music stopped")

func play_sound_effect(sound_name: String):
	"""효과음 재생"""
	if sound_name in sound_effects and sfx_player:
		sfx_player.stream = sound_effects[sound_name]
		sfx_player.volume_db = linear_to_db(sfx_volume * master_volume)
		sfx_player.play()
		print("Sound effect played: ", sound_name)

func play_voice_clip(voice_name: String):
	"""음성 클립 재생"""
	if voice_name in voice_clips and voice_player:
		voice_player.stream = voice_clips[voice_name]
		voice_player.volume_db = linear_to_db(voice_volume * master_volume)
		voice_player.play()
		print("Voice clip played: ", voice_name)

func set_master_volume(volume: float):
	"""마스터 볼륨 설정"""
	master_volume = clamp(volume, 0.0, 1.0)
	update_all_volumes()
	print("Master volume set to: ", master_volume)

func set_music_volume(volume: float):
	"""음악 볼륨 설정"""
	music_volume = clamp(volume, 0.0, 1.0)
	if music_player:
		music_player.volume_db = linear_to_db(music_volume * master_volume)
	print("Music volume set to: ", music_volume)

func set_sfx_volume(volume: float):
	"""효과음 볼륨 설정"""
	sfx_volume = clamp(volume, 0.0, 1.0)
	if sfx_player:
		sfx_player.volume_db = linear_to_db(sfx_volume * master_volume)
	print("SFX volume set to: ", sfx_volume)

func set_voice_volume(volume: float):
	"""음성 볼륨 설정"""
	voice_volume = clamp(volume, 0.0, 1.0)
	if voice_player:
		voice_player.volume_db = linear_to_db(voice_volume * master_volume)
	print("Voice volume set to: ", voice_volume)

func update_all_volumes():
	"""모든 볼륨 업데이트"""
	if music_player:
		music_player.volume_db = linear_to_db(music_volume * master_volume)
	if sfx_player:
		sfx_player.volume_db = linear_to_db(sfx_volume * master_volume)
	if voice_player:
		voice_player.volume_db = linear_to_db(voice_volume * master_volume)

func fade_out_music(duration: float = 2.0):
	"""음악 페이드 아웃"""
	if music_player:
		var tween = create_tween()
		tween.tween_property(music_player, "volume_db", -80.0, duration)
		tween.tween_callback(stop_background_music)

func fade_in_music(duration: float = 2.0):
	"""음악 페이드 인"""
	if music_player:
		music_player.volume_db = -80.0
		play_background_music()
		var tween = create_tween()
		tween.tween_property(music_player, "volume_db", linear_to_db(music_volume * master_volume), duration)

func _input(event):
	"""입력 처리"""
	if event.is_action_pressed("ui_accept"):
		# 테스트용 효과음 재생
		play_sound_effect("sword_swing")
	
	if event.is_action_pressed("ui_cancel"):
		# 테스트용 음성 재생
		play_voice_clip("attack")
