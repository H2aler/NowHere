extends Node

# Network Manager for Godot - Complete Version
# Unity의 NetworkManager.cs를 GDScript로 완전 변환

var is_connected: bool = false
var server_url: String = "ws://localhost:8080"
var websocket: WebSocketPeer
var players: Dictionary = {}
var player_id: String = ""

# 네트워크 설정
var network_settings: Dictionary = {
	"server_url": "ws://localhost:8080",
	"reconnect_attempts": 5,
	"reconnect_delay": 2.0,
	"heartbeat_interval": 30.0,
	"max_ping": 1000.0
}

func _ready():
	print("Network Manager initialized (Godot)")
	initialize_network()

func initialize_network():
	"""네트워크 초기화"""
	print("Initializing network...")
	
	# WebSocket 초기화
	websocket = WebSocketPeer.new()
	
	# 연결 설정
	server_url = network_settings.server_url
	
	print("Network initialization complete")

func connect_to_server():
	"""서버 연결"""
	if is_connected:
		print("Already connected to server")
		return
	
	print("Connecting to server: ", server_url)
	
	# WebSocket 연결
	var error = websocket.connect_to_url(server_url)
	if error != OK:
		print("Failed to connect to server: ", error)
		return
	
	# 연결 상태 확인
	check_connection_status()

func disconnect_from_server():
	"""서버 연결 해제"""
	if not is_connected:
		print("Not connected to server")
		return
	
	print("Disconnecting from server...")
	
	# WebSocket 연결 해제
	websocket.close()
	is_connected = false
	
	# 플레이어 목록 초기화
	players.clear()
	
	print("Disconnected from server")

func check_connection_status():
	"""연결 상태 확인"""
	var state = websocket.get_ready_state()
	
	match state:
		WebSocketPeer.STATE_CONNECTING:
			print("Connecting to server...")
		WebSocketPeer.STATE_OPEN:
			print("Connected to server successfully")
			is_connected = true
			on_connected()
		WebSocketPeer.STATE_CLOSING:
			print("Closing connection...")
		WebSocketPeer.STATE_CLOSED:
			print("Connection closed")
			is_connected = false
			on_disconnected()

func on_connected():
	"""연결 성공 시 호출"""
	print("Successfully connected to server")
	
	# 플레이어 등록
	register_player()
	
	# 하트비트 시작
	start_heartbeat()

func on_disconnected():
	"""연결 해제 시 호출"""
	print("Disconnected from server")
	
	# 하트비트 중지
	stop_heartbeat()
	
	# 자동 재연결 시도
	attempt_reconnect()

func register_player():
	"""플레이어 등록"""
	if not is_connected:
		return
	
	# 플레이어 정보 생성
	var player_info = {
		"id": generate_player_id(),
		"name": "Player",
		"position": Vector3.ZERO,
		"rotation": Vector3.ZERO,
		"health": 100,
		"level": 1
	}
	
	player_id = player_info.id
	
	# 서버에 플레이어 등록 요청
	var message = {
		"type": "register_player",
		"data": player_info
	}
	
	send_message(message)
	print("Player registered: ", player_id)

func generate_player_id() -> String:
	"""플레이어 ID 생성"""
	var timestamp = Time.get_unix_time_from_system()
	var random = randi() % 10000
	return "player_" + str(timestamp) + "_" + str(random)

func send_message(message: Dictionary):
	"""메시지 전송"""
	if not is_connected:
		print("Cannot send message: not connected")
		return
	
	var json_string = JSON.stringify(message)
	var error = websocket.send_text(json_string)
	
	if error != OK:
		print("Failed to send message: ", error)
	else:
		print("Message sent: ", message.type)

func receive_message():
	"""메시지 수신"""
	if not is_connected:
		return
	
	# WebSocket에서 메시지 수신
	websocket.poll()
	
	var state = websocket.get_ready_state()
	if state == WebSocketPeer.STATE_OPEN:
		while websocket.get_available_packet_count() > 0:
			var packet = websocket.get_packet()
			var message_text = packet.get_string_from_utf8()
			
			# JSON 파싱
			var json = JSON.new()
			var parse_result = json.parse(message_text)
			
			if parse_result == OK:
				var message = json.data
				process_message(message)
			else:
				print("Failed to parse message: ", message_text)

func process_message(message: Dictionary):
	"""메시지 처리"""
	if not message.has("type"):
		print("Invalid message format")
		return
	
	var message_type = message.type
	var message_data = message.get("data", {})
	
	match message_type:
		"player_joined":
			on_player_joined(message_data)
		"player_left":
			on_player_left(message_data)
		"player_update":
			on_player_update(message_data)
		"game_state":
			on_game_state_update(message_data)
		"chat_message":
			on_chat_message(message_data)
		"heartbeat_response":
			on_heartbeat_response(message_data)
		_:
			print("Unknown message type: ", message_type)

func on_player_joined(player_data: Dictionary):
	"""플레이어 참가 시 호출"""
	var player_id = player_data.get("id", "")
	if player_id != "":
		players[player_id] = player_data
		print("Player joined: ", player_id)

func on_player_left(player_data: Dictionary):
	"""플레이어 떠남 시 호출"""
	var player_id = player_data.get("id", "")
	if player_id in players:
		players.erase(player_id)
		print("Player left: ", player_id)

func on_player_update(player_data: Dictionary):
	"""플레이어 업데이트 시 호출"""
	var player_id = player_data.get("id", "")
	if player_id in players:
		players[player_id] = player_data
		print("Player updated: ", player_id)

func on_game_state_update(game_state: Dictionary):
	"""게임 상태 업데이트 시 호출"""
	print("Game state updated: ", game_state)
	
	# 실제 구현에서는 게임 상태에 따른 처리
	# 예: 적 스폰, 아이템 생성, 이벤트 발생 등

func on_chat_message(chat_data: Dictionary):
	"""채팅 메시지 수신 시 호출"""
	var player_id = chat_data.get("player_id", "")
	var message = chat_data.get("message", "")
	
	print("Chat from ", player_id, ": ", message)
	
	# 실제 구현에서는 채팅 UI에 메시지 표시

func on_heartbeat_response(response_data: Dictionary):
	"""하트비트 응답 수신 시 호출"""
	var timestamp = response_data.get("timestamp", 0)
	var current_time = Time.get_unix_time_from_system()
	var ping = current_time - timestamp
	
	print("Heartbeat response received, ping: ", ping, "ms")
	
	# 핑이 너무 높으면 경고
	if ping > network_settings.max_ping:
		print("Warning: High ping detected: ", ping, "ms")

func send_player_update(player_data: Dictionary):
	"""플레이어 업데이트 전송"""
	var message = {
		"type": "player_update",
		"data": player_data
	}
	
	send_message(message)

func send_chat_message(message_text: String):
	"""채팅 메시지 전송"""
	var message = {
		"type": "chat_message",
		"data": {
			"player_id": player_id,
			"message": message_text
		}
	}
	
	send_message(message)

func get_players() -> Dictionary:
	"""플레이어 목록 반환"""
	return players

func get_player_count() -> int:
	"""플레이어 수 반환"""
	return players.size()

func is_server_connected() -> bool:
	"""서버 연결 상태 반환"""
	return is_connected

# 하트비트 시스템
var heartbeat_timer: Timer

func start_heartbeat():
	"""하트비트 시작"""
	if heartbeat_timer:
		heartbeat_timer.queue_free()
	
	heartbeat_timer = Timer.new()
	heartbeat_timer.wait_time = network_settings.heartbeat_interval
	heartbeat_timer.timeout.connect(_on_heartbeat_timeout)
	add_child(heartbeat_timer)
	heartbeat_timer.start()
	
	print("Heartbeat started")

func stop_heartbeat():
	"""하트비트 중지"""
	if heartbeat_timer:
		heartbeat_timer.stop()
		heartbeat_timer.queue_free()
		heartbeat_timer = null
	
	print("Heartbeat stopped")

func _on_heartbeat_timeout():
	"""하트비트 타임아웃"""
	if not is_connected:
		return
	
	var message = {
		"type": "heartbeat",
		"data": {
			"timestamp": Time.get_unix_time_from_system()
		}
	}
	
	send_message(message)

# 자동 재연결 시스템
var reconnect_timer: Timer
var reconnect_attempts: int = 0

func attempt_reconnect():
	"""자동 재연결 시도"""
	if reconnect_attempts >= network_settings.reconnect_attempts:
		print("Max reconnect attempts reached")
		return
	
	reconnect_attempts += 1
	print("Attempting to reconnect (", reconnect_attempts, "/", network_settings.reconnect_attempts, ")")
	
	if reconnect_timer:
		reconnect_timer.queue_free()
	
	reconnect_timer = Timer.new()
	reconnect_timer.wait_time = network_settings.reconnect_delay
	reconnect_timer.one_shot = true
	reconnect_timer.timeout.connect(_on_reconnect_timeout)
	add_child(reconnect_timer)
	reconnect_timer.start()

func _on_reconnect_timeout():
	"""재연결 타임아웃"""
	connect_to_server()

func _process(delta):
	"""프레임마다 실행"""
	if is_connected:
		receive_message()
