# 프로젝트 작업 규칙

## DTO 규칙

- 서버로 데이터를 보내거나 서버에서 데이터를 받을 때 사용하는 DTO 클래스에는 반드시 `[Serializable]`을 붙인다.
- DTO 필드는 `public field`로 둔다.
- DTO 클래스 이름은 `JoinRoomRequestDto`처럼 반드시 `Dto`로 끝낸다.
- DTO에는 통신 데이터만 담는다. `RTCPeerConnection`, `RTCIceCandidate` 같은 WebRTC 런타임 객체를 DTO에 넣지 않는다.

## 통신 구조 규칙

- REST API DTO는 `CreateRoomRequestDto`, `RoomListResponseDto`처럼 request/response 역할이 드러나는 이름을 사용한다.
- Socket signaling DTO는 이벤트 메시지를 표현하며, 필요하면 `from`, `to`, `roomCode` 같은 라우팅 필드를 포함한다.
- DTO를 런타임 상태로 직접 사용하지 않는다. `RoomState`, `PeerState` 같은 내부 상태 객체로 변환해서 관리한다.
- mesh WebRTC에서는 상대 `peerId` 하나당 `RTCPeerConnection` 하나를 관리한다.
- `OnIceCandidate`에서 생성된 candidate는 Socket.IO로 상대에게 전달한다.
- Socket.IO로 받은 candidate는 `AddIceCandidate`로 적용한다.
