extends Node

# Network Manager for Godot
# Unity의 NetworkManager.cs를 GDScript로 변환

var is_connected: bool = false
var server_url: String = "ws://localhost:8080"
var websocket: WebSocketPeer
var players: Dictionary = {}

func _ready():
	print("Network Manager initialized")
	setup_websocket()

func setup_websocket():
	"""WebSocket 설정"""
	websocket = WebSocketPeer.new()
	print("WebSocket setup complete")

func connect_to_server():
	"""서버 연결"""
	if is_connected:
		print("Already connected to server")
		return
	
	print("Connecting to server: ", server_url)
	
	var error = websocket.connect_to_url(server_url)
	if error != OK:
		print("Failed to connect to server: ", error)
		return
	
	print("Connecting to server...")

func disconnect_from_server():
	"""서버 연결 해제"""
	if not is_connected:
		return
	
	print("Disconnecting from server...")
	websocket.close()
	is_connected = false

func send_message(message: String):
	"""메시지 전송"""
	if not is_connected:
		print("Not connected to server")
		return
	
	websocket.send_text(message)
	print("Message sent: ", message)

func send_player_data(player_data: Dictionary):
	"""플레이어 데이터 전송"""
	var json = JSON.stringify(player_data)
	send_message(json)

func handle_server_message(message: String):
	"""서버 메시지 처리"""
	print("Received message: ", message)
	
	var json = JSON.new()
	var error = json.parse(message)
	
	if error != OK:
		print("Failed to parse message: ", message)
		return
	
	var data = json.data
	
	match data.type:
		"player_joined":
			handle_player_joined(data)
		"player_left":
			handle_player_left(data)
		"game_state":
			handle_game_state(data)
		"chat_message":
			handle_chat_message(data)

func handle_player_joined(data: Dictionary):
	"""플레이어 참가 처리"""
	var player_id = data.player_id
	var player_info = data.player_info
	
	players[player_id] = player_info
	print("Player joined: ", player_id, " - ", player_info)

func handle_player_left(data: Dictionary):
	"""플레이어 나가기 처리"""
	var player_id = data.player_id
	
	if player_id in players:
		players.erase(player_id)
		print("Player left: ", player_id)

func handle_game_state(data: Dictionary):
	"""게임 상태 처리"""
	print("Game state updated: ", data.state)

func handle_chat_message(data: Dictionary):
	"""채팅 메시지 처리"""
	var player_id = data.player_id
	var message = data.message
	
	print("Chat from ", player_id, ": ", message)

func _process(delta):
	"""프레임마다 실행"""
	if websocket.get_ready_state() == WebSocketPeer.STATE_OPEN:
		if not is_connected:
			is_connected = true
			print("Connected to server!")
		
		# 메시지 수신 처리
		while websocket.get_available_packet_count() > 0:
			var packet = websocket.get_packet()
			var message = packet.get_string_from_utf8()
			handle_server_message(message)
	
	elif websocket.get_ready_state() == WebSocketPeer.STATE_CLOSED:
		if is_connected:
			is_connected = false
			print("Disconnected from server")

func _input(event):
	"""입력 처리"""
	if event.is_action_pressed("ui_accept"):
		if not is_connected:
			connect_to_server()
		else:
			disconnect_from_server()
