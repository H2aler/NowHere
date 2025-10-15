extends Node

# Audio Manager for Godot - Complete Version
# Unity의 AudioManager.cs를 GDScript로 완전 변환

var master_volume: float = 1.0
var music_volume: float = 0.8
var sfx_volume: float = 1.0
var voice_volume: float = 1.0

# 오디오 컴포넌트들
var music_player: AudioStreamPlayer
var sfx_player: AudioStreamPlayer
var voice_player: AudioStreamPlayer
var ambient_player: AudioStreamPlayer

# 오디오 리소스들
var audio_resources: Dictionary = {
	"music": {},
	"sfx": {},
	"voice": {},
	"ambient": {}
}

# 오디오 설정
var audio_settings: Dictionary = {
	"master_volume": 1.0,
	"music_volume": 0.8,
	"sfx_volume": 1.0,
	"voice_volume": 1.0,
	"mute_music": false,
	"mute_sfx": false,
	"mute_voice": false,
	"mute_ambient": false
}

func _ready():
	print("Audio Manager initialized (Godot)")
	initialize_audio_system()

func initialize_audio_system():
	"""오디오 시스템 초기화"""
	print("Initializing audio system...")
	
	# 오디오 플레이어 생성
	create_audio_players()
	
	# 오디오 리소스 로드
	load_audio_resources()
	
	# 오디오 설정 적용
	apply_audio_settings()
	
	print("Audio system initialization complete")

func create_audio_players():
	"""오디오 플레이어 생성"""
	# 음악 플레이어
	music_player = AudioStreamPlayer.new()
	music_player.name = "MusicPlayer"
	music_player.volume_db = linear_to_db(music_volume)
	add_child(music_player)
	
	# 효과음 플레이어
	sfx_player = AudioStreamPlayer.new()
	sfx_player.name = "SFXPlayer"
	sfx_player.volume_db = linear_to_db(sfx_volume)
	add_child(sfx_player)
	
	# 음성 플레이어
	voice_player = AudioStreamPlayer.new()
	voice_player.name = "VoicePlayer"
	voice_player.volume_db = linear_to_db(voice_volume)
	add_child(voice_player)
	
	# 환경음 플레이어
	ambient_player = AudioStreamPlayer.new()
	ambient_player.name = "AmbientPlayer"
	ambient_player.volume_db = linear_to_db(0.5)
	add_child(ambient_player)
	
	print("Audio players created")

func load_audio_resources():
	"""오디오 리소스 로드"""
	print("Loading audio resources...")
	
	# 실제 구현에서는 파일 시스템에서 오디오 파일 로드
	# 현재는 기본 오디오 리소스 생성
	
	# 음악 리소스
	audio_resources.music["main_theme"] = create_dummy_audio_stream()
	audio_resources.music["battle_theme"] = create_dummy_audio_stream()
	audio_resources.music["menu_theme"] = create_dummy_audio_stream()
	
	# 효과음 리소스
	audio_resources.sfx["sword_swing"] = create_dummy_audio_stream()
	audio_resources.sfx["shield_block"] = create_dummy_audio_stream()
	audio_resources.sfx["jump"] = create_dummy_audio_stream()
	audio_resources.sfx["level_up"] = create_dummy_audio_stream()
	audio_resources.sfx["game_start"] = create_dummy_audio_stream()
	
	# 음성 리소스
	audio_resources.voice["attack"] = create_dummy_audio_stream()
	audio_resources.voice["defend"] = create_dummy_audio_stream()
	audio_resources.voice["victory"] = create_dummy_audio_stream()
	audio_resources.voice["defeat"] = create_dummy_audio_stream()
	
	# 환경음 리소스
	audio_resources.ambient["forest"] = create_dummy_audio_stream()
	audio_resources.ambient["cave"] = create_dummy_audio_stream()
	audio_resources.ambient["city"] = create_dummy_audio_stream()
	
	print("Audio resources loaded")

func create_dummy_audio_stream() -> AudioStream:
	"""더미 오디오 스트림 생성 (테스트용)"""
	# 실제 구현에서는 실제 오디오 파일 로드
	# 현재는 더미 스트림 생성
	var stream = AudioStreamGenerator.new()
	stream.mix_rate = 44100
	stream.buffer_length = 0.1
	return stream

func apply_audio_settings():
	"""오디오 설정 적용"""
	master_volume = audio_settings.master_volume
	music_volume = audio_settings.music_volume
	sfx_volume = audio_settings.sfx_volume
	voice_volume = audio_settings.voice_volume
	
	# 볼륨 적용
	set_master_volume(master_volume)
	set_music_volume(music_volume)
	set_sfx_volume(sfx_volume)
	set_voice_volume(voice_volume)
	
	print("Audio settings applied")

func set_master_volume(volume: float):
	"""마스터 볼륨 설정"""
	master_volume = clamp(volume, 0.0, 1.0)
	audio_settings.master_volume = master_volume
	
	# 모든 오디오 플레이어의 볼륨 업데이트
	update_all_volumes()
	
	print("Master volume set to: ", master_volume)

func set_music_volume(volume: float):
	"""음악 볼륨 설정"""
	music_volume = clamp(volume, 0.0, 1.0)
	audio_settings.music_volume = music_volume
	
	if music_player:
		music_player.volume_db = linear_to_db(music_volume * master_volume)
	
	print("Music volume set to: ", music_volume)

func set_sfx_volume(volume: float):
	"""효과음 볼륨 설정"""
	sfx_volume = clamp(volume, 0.0, 1.0)
	audio_settings.sfx_volume = sfx_volume
	
	if sfx_player:
		sfx_player.volume_db = linear_to_db(sfx_volume * master_volume)
	
	print("SFX volume set to: ", sfx_volume)

func set_voice_volume(volume: float):
	"""음성 볼륨 설정"""
	voice_volume = clamp(volume, 0.0, 1.0)
	audio_settings.voice_volume = voice_volume
	
	if voice_player:
		voice_player.volume_db = linear_to_db(voice_volume * master_volume)
	
	print("Voice volume set to: ", voice_volume)

func update_all_volumes():
	"""모든 볼륨 업데이트"""
	set_music_volume(music_volume)
	set_sfx_volume(sfx_volume)
	set_voice_volume(voice_volume)
	
	if ambient_player:
		ambient_player.volume_db = linear_to_db(0.5 * master_volume)

func play_background_music(music_name: String = "main_theme"):
	"""배경음악 재생"""
	if not audio_resources.music.has(music_name):
		print("Music not found: ", music_name)
		return
	
	if audio_settings.mute_music:
		print("Music is muted")
		return
	
	if music_player:
		music_player.stream = audio_resources.music[music_name]
		music_player.play()
		print("Playing background music: ", music_name)

func stop_background_music():
	"""배경음악 중지"""
	if music_player:
		music_player.stop()
		print("Background music stopped")

func pause_background_music():
	"""배경음악 일시정지"""
	if music_player:
		music_player.stream_paused = true
		print("Background music paused")

func resume_background_music():
	"""배경음악 재개"""
	if music_player:
		music_player.stream_paused = false
		print("Background music resumed")

func play_sound_effect(sfx_name: String):
	"""효과음 재생"""
	if not audio_resources.sfx.has(sfx_name):
		print("SFX not found: ", sfx_name)
		return
	
	if audio_settings.mute_sfx:
		print("SFX is muted")
		return
	
	if sfx_player:
		sfx_player.stream = audio_resources.sfx[sfx_name]
		sfx_player.play()
		print("Playing sound effect: ", sfx_name)

func play_voice_clip(voice_name: String):
	"""음성 클립 재생"""
	if not audio_resources.voice.has(voice_name):
		print("Voice clip not found: ", voice_name)
		return
	
	if audio_settings.mute_voice:
		print("Voice is muted")
		return
	
	if voice_player:
		voice_player.stream = audio_resources.voice[voice_name]
		voice_player.play()
		print("Playing voice clip: ", voice_name)

func play_ambient_sound(ambient_name: String):
	"""환경음 재생"""
	if not audio_resources.ambient.has(ambient_name):
		print("Ambient sound not found: ", ambient_name)
		return
	
	if audio_settings.mute_ambient:
		print("Ambient sound is muted")
		return
	
	if ambient_player:
		ambient_player.stream = audio_resources.ambient[ambient_name]
		ambient_player.play()
		print("Playing ambient sound: ", ambient_name)

func stop_ambient_sound():
	"""환경음 중지"""
	if ambient_player:
		ambient_player.stop()
		print("Ambient sound stopped")

func mute_music(mute: bool):
	"""음악 음소거"""
	audio_settings.mute_music = mute
	
	if mute:
		pause_background_music()
	else:
		resume_background_music()
	
	print("Music muted: ", mute)

func mute_sfx(mute: bool):
	"""효과음 음소거"""
	audio_settings.mute_sfx = mute
	print("SFX muted: ", mute)

func mute_voice(mute: bool):
	"""음성 음소거"""
	audio_settings.mute_voice = mute
	print("Voice muted: ", mute)

func mute_ambient(mute: bool):
	"""환경음 음소거"""
	audio_settings.mute_ambient = mute
	
	if mute:
		stop_ambient_sound()
	
	print("Ambient sound muted: ", mute)

func mute_all(mute: bool):
	"""모든 오디오 음소거"""
	mute_music(mute)
	mute_sfx(mute)
	mute_voice(mute)
	mute_ambient(mute)
	print("All audio muted: ", mute)

func fade_in_music(music_name: String, duration: float = 2.0):
	"""음악 페이드 인"""
	play_background_music(music_name)
	
	if music_player:
		# 페이드 인 효과
		var tween = create_tween()
		tween.tween_method(set_music_volume, 0.0, music_volume, duration)
		print("Fading in music: ", music_name)

func fade_out_music(duration: float = 2.0):
	"""음악 페이드 아웃"""
	if music_player:
		# 페이드 아웃 효과
		var tween = create_tween()
		tween.tween_method(set_music_volume, music_volume, 0.0, duration)
		tween.tween_callback(stop_background_music)
		print("Fading out music")

func get_audio_settings() -> Dictionary:
	"""오디오 설정 반환"""
	return audio_settings.duplicate()

func save_audio_settings():
	"""오디오 설정 저장"""
	# 실제 구현에서는 파일 시스템에 설정 저장
	print("Audio settings saved")

func load_audio_settings():
	"""오디오 설정 로드"""
	# 실제 구현에서는 파일 시스템에서 설정 로드
	print("Audio settings loaded")

func _process(delta):
	"""프레임마다 실행"""
	# 오디오 상태 업데이트
	# 실제 구현에서는 오디오 상태 모니터링
	pass
