extends Node

# Save System for Godot - Complete Version
# Unity의 SaveSystem.cs를 GDScript로 완전 변환

var save_directory: String = "user://saves/"
var save_file_extension: String = ".json"
var current_save_slot: int = 0
var max_save_slots: int = 10

# 저장 데이터 구조
var save_data: Dictionary = {
	"player_data": {},
	"game_settings": {},
	"inventory_data": {},
	"game_progress": {},
	"statistics": {},
	"timestamp": 0,
	"version": "1.0.0"
}

func _ready():
	print("Save System initialized (Godot)")
	initialize_save_system()

func initialize_save_system():
	"""저장 시스템 초기화"""
	print("Initializing save system...")
	
	# 저장 디렉토리 생성
	create_save_directory()
	
	# 기본 저장 데이터 초기화
	initialize_default_save_data()
	
	print("Save system initialization complete")

func create_save_directory():
	"""저장 디렉토리 생성"""
	if not DirAccess.dir_exists_absolute(save_directory):
		DirAccess.open("user://").make_dir_recursive("saves")
		print("Save directory created: ", save_directory)
	else:
		print("Save directory already exists: ", save_directory)

func initialize_default_save_data():
	"""기본 저장 데이터 초기화"""
	save_data = {
		"player_data": {
			"name": "Player",
			"level": 1,
			"experience": 0,
			"health": 100,
			"max_health": 100,
			"mana": 100,
			"max_mana": 100,
			"position": {"x": 0, "y": 0, "z": 0},
			"rotation": {"x": 0, "y": 0, "z": 0},
			"skills": [],
			"equipment": {
				"weapon": null,
				"armor": null,
				"accessory": null
			}
		},
		"game_settings": {
			"master_volume": 1.0,
			"music_volume": 0.8,
			"sfx_volume": 1.0,
			"voice_volume": 1.0,
			"ar_enabled": true,
			"vr_enabled": true,
			"multiplayer_enabled": true,
			"graphics_quality": "high",
			"language": "ko"
		},
		"inventory_data": {
			"items": [],
			"equipment": {
				"weapon": null,
				"armor": null,
				"accessory": null
			},
			"gold": 100
		},
		"game_progress": {
			"current_scene": "MainMenu",
			"completed_quests": [],
			"unlocked_areas": ["starting_area"],
			"story_progress": 0
		},
		"statistics": {
			"play_time": 0,
			"enemies_defeated": 0,
			"items_collected": 0,
			"deaths": 0,
			"levels_gained": 0
		},
		"timestamp": Time.get_unix_time_from_system(),
		"version": "1.0.0"
	}
	
	print("Default save data initialized")

func save_game_data(slot: int = 0):
	"""게임 데이터 저장"""
	if slot < 0 or slot >= max_save_slots:
		print("Invalid save slot: ", slot)
		return false
	
	current_save_slot = slot
	save_data.timestamp = Time.get_unix_time_from_system()
	
	var save_file_path = save_directory + "save_" + str(slot) + save_file_extension
	
	# JSON으로 변환
	var json_string = JSON.stringify(save_data, "\t")
	
	# 파일 저장
	var file = FileAccess.open(save_file_path, FileAccess.WRITE)
	if file:
		file.store_string(json_string)
		file.close()
		print("Game data saved to slot ", slot, ": ", save_file_path)
		return true
	else:
		print("Failed to save game data to slot ", slot)
		return false

func load_game_data(slot: int = 0):
	"""게임 데이터 로드"""
	if slot < 0 or slot >= max_save_slots:
		print("Invalid save slot: ", slot)
		return false
	
	current_save_slot = slot
	var save_file_path = save_directory + "save_" + str(slot) + save_file_extension
	
	# 파일 존재 확인
	if not FileAccess.file_exists(save_file_path):
		print("Save file not found: ", save_file_path)
		return false
	
	# 파일 로드
	var file = FileAccess.open(save_file_path, FileAccess.READ)
	if file:
		var json_string = file.get_as_text()
		file.close()
		
		# JSON 파싱
		var json = JSON.new()
		var parse_result = json.parse(json_string)
		
		if parse_result == OK:
			save_data = json.data
			print("Game data loaded from slot ", slot, ": ", save_file_path)
			return true
		else:
			print("Failed to parse save file: ", save_file_path)
			return false
	else:
		print("Failed to load save file: ", save_file_path)
		return false

func delete_save_data(slot: int):
	"""저장 데이터 삭제"""
	if slot < 0 or slot >= max_save_slots:
		print("Invalid save slot: ", slot)
		return false
	
	var save_file_path = save_directory + "save_" + str(slot) + save_file_extension
	
	if FileAccess.file_exists(save_file_path):
		DirAccess.remove_absolute(save_file_path)
		print("Save data deleted from slot ", slot)
		return true
	else:
		print("Save file not found: ", save_file_path)
		return false

func get_save_slot_info(slot: int) -> Dictionary:
	"""저장 슬롯 정보 반환"""
	if slot < 0 or slot >= max_save_slots:
		return {}
	
	var save_file_path = save_directory + "save_" + str(slot) + save_file_extension
	
	if not FileAccess.file_exists(save_file_path):
		return {"exists": false}
	
	# 파일 로드
	var file = FileAccess.open(save_file_path, FileAccess.READ)
	if file:
		var json_string = file.get_as_text()
		file.close()
		
		# JSON 파싱
		var json = JSON.new()
		var parse_result = json.parse(json_string)
		
		if parse_result == OK:
			var data = json.data
			return {
				"exists": true,
				"player_name": data.get("player_data", {}).get("name", "Unknown"),
				"level": data.get("player_data", {}).get("level", 1),
				"timestamp": data.get("timestamp", 0),
				"version": data.get("version", "Unknown"),
				"play_time": data.get("statistics", {}).get("play_time", 0)
			}
	
	return {"exists": false}

func get_all_save_slots() -> Array:
	"""모든 저장 슬롯 정보 반환"""
	var save_slots = []
	
	for i in range(max_save_slots):
		var slot_info = get_save_slot_info(i)
		slot_info["slot"] = i
		save_slots.append(slot_info)
	
	return save_slots

func update_player_data(player_data: Dictionary):
	"""플레이어 데이터 업데이트"""
	save_data.player_data = player_data.duplicate()
	print("Player data updated")

func update_game_settings(game_settings: Dictionary):
	"""게임 설정 업데이트"""
	save_data.game_settings = game_settings.duplicate()
	print("Game settings updated")

func update_inventory_data(inventory_data: Dictionary):
	"""인벤토리 데이터 업데이트"""
	save_data.inventory_data = inventory_data.duplicate()
	print("Inventory data updated")

func update_game_progress(game_progress: Dictionary):
	"""게임 진행도 업데이트"""
	save_data.game_progress = game_progress.duplicate()
	print("Game progress updated")

func update_statistics(statistics: Dictionary):
	"""통계 데이터 업데이트"""
	save_data.statistics = statistics.duplicate()
	print("Statistics updated")

func get_player_data() -> Dictionary:
	"""플레이어 데이터 반환"""
	return save_data.player_data.duplicate()

func get_game_settings() -> Dictionary:
	"""게임 설정 반환"""
	return save_data.game_settings.duplicate()

func get_inventory_data() -> Dictionary:
	"""인벤토리 데이터 반환"""
	return save_data.inventory_data.duplicate()

func get_game_progress() -> Dictionary:
	"""게임 진행도 반환"""
	return save_data.game_progress.duplicate()

func get_statistics() -> Dictionary:
	"""통계 데이터 반환"""
	return save_data.statistics.duplicate()

func export_save_data(slot: int, export_path: String):
	"""저장 데이터 내보내기"""
	if slot < 0 or slot >= max_save_slots:
		print("Invalid save slot: ", slot)
		return false
	
	var save_file_path = save_directory + "save_" + str(slot) + save_file_extension
	
	if not FileAccess.file_exists(save_file_path):
		print("Save file not found: ", save_file_path)
		return false
	
	# 파일 복사
	var source_file = FileAccess.open(save_file_path, FileAccess.READ)
	var target_file = FileAccess.open(export_path, FileAccess.WRITE)
	
	if source_file and target_file:
		target_file.store_string(source_file.get_as_text())
		source_file.close()
		target_file.close()
		print("Save data exported to: ", export_path)
		return true
	else:
		print("Failed to export save data")
		return false

func import_save_data(import_path: String, slot: int):
	"""저장 데이터 가져오기"""
	if slot < 0 or slot >= max_save_slots:
		print("Invalid save slot: ", slot)
		return false
	
	if not FileAccess.file_exists(import_path):
		print("Import file not found: ", import_path)
		return false
	
	# 파일 복사
	var source_file = FileAccess.open(import_path, FileAccess.READ)
	var target_file = FileAccess.open(save_directory + "save_" + str(slot) + save_file_extension, FileAccess.WRITE)
	
	if source_file and target_file:
		target_file.store_string(source_file.get_as_text())
		source_file.close()
		target_file.close()
		print("Save data imported to slot ", slot, " from: ", import_path)
		return true
	else:
		print("Failed to import save data")
		return false

func auto_save():
	"""자동 저장"""
	print("Auto saving...")
	return save_game_data(current_save_slot)

func quick_save():
	"""빠른 저장"""
	print("Quick saving...")
	return save_game_data(current_save_slot)

func quick_load():
	"""빠른 로드"""
	print("Quick loading...")
	return load_game_data(current_save_slot)

func get_save_data() -> Dictionary:
	"""저장 데이터 반환"""
	return save_data.duplicate()

func set_save_data(data: Dictionary):
	"""저장 데이터 설정"""
	save_data = data.duplicate()
	print("Save data set")

func _process(delta):
	"""프레임마다 실행"""
	# 자동 저장 체크
	# 실제 구현에서는 일정 시간마다 자동 저장
	pass
