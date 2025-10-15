extends Node

# Save System for Godot
# Unity의 SaveSystem.cs를 GDScript로 변환

var save_file_path: String = "user://save_data.json"
var game_data: Dictionary = {}

func _ready():
	print("Save System initialized")
	load_game_data()

func save_game_data():
	"""게임 데이터 저장"""
	print("Saving game data...")
	
	# 게임 데이터 준비
	var save_data = {
		"version": "1.0.0",
		"timestamp": Time.get_unix_time_from_system(),
		"player_data": get_player_data(),
		"game_settings": get_game_settings(),
		"inventory": get_inventory_data(),
		"progress": get_progress_data()
	}
	
	# JSON으로 변환
	var json = JSON.stringify(save_data, "\t")
	
	# 파일 저장
	var file = FileAccess.open(save_file_path, FileAccess.WRITE)
	if file:
		file.store_string(json)
		file.close()
		print("Game data saved successfully")
		return true
	else:
		print("Failed to save game data")
		return false

func load_game_data():
	"""게임 데이터 로드"""
	print("Loading game data...")
	
	var file = FileAccess.open(save_file_path, FileAccess.READ)
	if file:
		var json_string = file.get_as_text()
		file.close()
		
		var json = JSON.new()
		var error = json.parse(json_string)
		
		if error == OK:
			game_data = json.data
			apply_loaded_data()
			print("Game data loaded successfully")
			return true
		else:
			print("Failed to parse save data")
			return false
	else:
		print("No save file found, creating new game")
		create_new_game_data()
		return false

func create_new_game_data():
	"""새 게임 데이터 생성"""
	print("Creating new game data...")
	
	game_data = {
		"version": "1.0.0",
		"timestamp": Time.get_unix_time_from_system(),
		"player_data": {
			"name": "Player",
			"level": 1,
			"experience": 0,
			"health": 100,
			"mana": 100,
			"position": {"x": 0, "y": 0, "z": 0},
			"rotation": {"x": 0, "y": 0, "z": 0}
		},
		"game_settings": {
			"master_volume": 1.0,
			"music_volume": 0.8,
			"sfx_volume": 1.0,
			"voice_volume": 1.0,
			"ar_enabled": true,
			"vr_enabled": true,
			"multiplayer_enabled": true
		},
		"inventory": {
			"items": [],
			"equipment": {
				"weapon": null,
				"armor": null,
				"accessory": null
			}
		},
		"progress": {
			"completed_quests": [],
			"unlocked_areas": ["tutorial"],
			"achievements": []
		}
	}
	
	save_game_data()

func apply_loaded_data():
	"""로드된 데이터 적용"""
	print("Applying loaded data...")
	
	# 플레이어 데이터 적용
	if "player_data" in game_data:
		apply_player_data(game_data.player_data)
	
	# 게임 설정 적용
	if "game_settings" in game_data:
		apply_game_settings(game_data.game_settings)
	
	# 인벤토리 데이터 적용
	if "inventory" in game_data:
		apply_inventory_data(game_data.inventory)
	
	# 진행도 데이터 적용
	if "progress" in game_data:
		apply_progress_data(game_data.progress)

func get_player_data() -> Dictionary:
	"""플레이어 데이터 가져오기"""
	# 실제 구현에서는 플레이어 노드에서 데이터 가져오기
	return game_data.get("player_data", {})

func get_game_settings() -> Dictionary:
	"""게임 설정 가져오기"""
	# 실제 구현에서는 설정 노드에서 데이터 가져오기
	return game_data.get("game_settings", {})

func get_inventory_data() -> Dictionary:
	"""인벤토리 데이터 가져오기"""
	# 실제 구현에서는 인벤토리 노드에서 데이터 가져오기
	return game_data.get("inventory", {})

func get_progress_data() -> Dictionary:
	"""진행도 데이터 가져오기"""
	# 실제 구현에서는 진행도 노드에서 데이터 가져오기
	return game_data.get("progress", {})

func apply_player_data(player_data: Dictionary):
	"""플레이어 데이터 적용"""
	print("Applying player data: ", player_data)
	# 실제 구현에서는 플레이어 노드에 데이터 적용

func apply_game_settings(settings: Dictionary):
	"""게임 설정 적용"""
	print("Applying game settings: ", settings)
	# 실제 구현에서는 설정 노드에 데이터 적용

func apply_inventory_data(inventory_data: Dictionary):
	"""인벤토리 데이터 적용"""
	print("Applying inventory data: ", inventory_data)
	# 실제 구현에서는 인벤토리 노드에 데이터 적용

func apply_progress_data(progress_data: Dictionary):
	"""진행도 데이터 적용"""
	print("Applying progress data: ", progress_data)
	# 실제 구현에서는 진행도 노드에 데이터 적용

func delete_save_data():
	"""저장 데이터 삭제"""
	print("Deleting save data...")
	
	if FileAccess.file_exists(save_file_path):
		DirAccess.remove_absolute(save_file_path)
		print("Save data deleted")
		create_new_game_data()
	else:
		print("No save data to delete")

func get_save_info() -> Dictionary:
	"""저장 정보 가져오기"""
	if FileAccess.file_exists(save_file_path):
		var file = FileAccess.open(save_file_path, FileAccess.READ)
		if file:
			var json_string = file.get_as_text()
			file.close()
			
			var json = JSON.new()
			var error = json.parse(json_string)
			
			if error == OK:
				var data = json.data
				return {
					"exists": true,
					"version": data.get("version", "unknown"),
					"timestamp": data.get("timestamp", 0),
					"player_name": data.get("player_data", {}).get("name", "unknown"),
					"player_level": data.get("player_data", {}).get("level", 1)
				}
	
	return {"exists": false}

func _input(event):
	"""입력 처리"""
	if event.is_action_pressed("ui_accept"):
		# F5 키로 저장
		save_game_data()
	
	if event.is_action_pressed("ui_cancel"):
		# F9 키로 로드
		load_game_data()
