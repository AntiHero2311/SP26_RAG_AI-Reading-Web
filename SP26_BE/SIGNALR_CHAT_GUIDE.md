# Staff-Author Chat với SignalR - Hướng dẫn sử dụng

## 📁 Cấu trúc đã tạo

### Backend (ASP.NET Core)
- ✅ **Models**: `StaffAuthorContact.cs`, `StaffAuthorMessage.cs`
- ✅ **Repositories**: `StaffAuthorContactRepository.cs`, `StaffAuthorMessageRepository.cs`
- ✅ **Services**: `IStaffAuthorChatService.cs`, `StaffAuthorChatService.cs`
- ✅ **DTOs**: 5 DTOs cho request/response
- ✅ **Controller**: `StaffAuthorChatController.cs`
- ✅ **SignalR Hub**: `ChatHub.cs` - Real-time messaging
- ✅ **Database Script**: `Database_StaffAuthorChat.sql`

---

## 🗄️ Bước 1: Tạo Database Tables

Chạy script SQL trong file `Database_StaffAuthorChat.sql`:

```sql
-- Tạo 2 bảng: StaffAuthorContact và StaffAuthorMessage
-- Script tự động tạo indexes và foreign keys
```

---

## 🚀 Bước 2: Chạy Backend

Backend đã được cấu hình sẵn trong `Program.cs`:
- SignalR Hub endpoint: `/chatHub`
- CORS cho phép `localhost:3000` và `localhost:5173`
- JWT Authentication hỗ trợ cả HTTP và SignalR

Chạy ứng dụng:
```bash
dotnet run
```

---

## 📡 API Endpoints (REST)

### **Contacts**
- `GET /api/staffauthorchat/contacts` - Lấy danh sách contacts của user
- `GET /api/staffauthorchat/contacts/{contactId}` - Chi tiết contact + messages
- `POST /api/staffauthorchat/contacts` - Tạo contact mới
- `PUT /api/staffauthorchat/contacts/{contactId}/status` - Cập nhật status
- `DELETE /api/staffauthorchat/contacts/{contactId}` - Xóa contact (Staff only)

### **Messages**
- `GET /api/staffauthorchat/contacts/{contactId}/messages` - Lấy tất cả messages
- `POST /api/staffauthorchat/messages` - Gửi message (hoặc dùng SignalR)
- `POST /api/staffauthorchat/contacts/{contactId}/mark-read` - Đánh dấu đã đọc
- `GET /api/staffauthorchat/unread-count` - Số lượng tin chưa đọc

---

## 🔌 SignalR Client Integration

### **JavaScript/TypeScript (React, Vue, Angular)**

#### 1. Cài đặt package
```bash
npm install @microsoft/signalr
```

#### 2. Kết nối với ChatHub
```typescript
import * as signalR from "@microsoft/signalr";

// Lấy JWT token (từ localStorage hoặc context)
const token = localStorage.getItem("accessToken");

// Tạo connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:7xxx/chatHub", {
        accessTokenFactory: () => token,
        transport: signalR.HttpTransportType.WebSockets
    })
    .withAutomaticReconnect()
    .build();

// Bắt đầu kết nối
connection.start()
    .then(() => console.log("✅ Connected to ChatHub"))
    .catch(err => console.error("❌ Connection error:", err));
```

#### 3. Lắng nghe events từ server
```typescript
// Khi kết nối thành công
connection.on("Connected", (data) => {
    console.log("Connected:", data);
    // data: { userId, role, connectionId }
});

// Nhận tin nhắn mới
connection.on("ReceiveMessage", (message) => {
    console.log("New message:", message);
    // message: { messageId, contactId, senderType, senderId, 
    //           messageText, sendAt, isRead, senderName }
    
    // Cập nhật UI với tin nhắn mới
    addMessageToUI(message);
});

// Tin nhắn đã được đọc
connection.on("MessagesMarkedAsRead", (data) => {
    console.log("Messages read:", data.contactId);
    // Cập nhật UI: đánh dấu tin nhắn đã đọc
});

// Người dùng đang gõ
connection.on("UserTyping", (data) => {
    console.log(`${data.senderType} is typing...`);
    // Hiển thị "đang gõ..." trong UI
});

// Lỗi
connection.on("Error", (errorMessage) => {
    console.error("Error:", errorMessage);
});
```

#### 4. Gửi tin nhắn (SignalR)
```typescript
async function sendMessage(contactId: number, messageText: string) {
    try {
        await connection.invoke("SendMessage", contactId, messageText);
        console.log("✅ Message sent");
    } catch (err) {
        console.error("❌ Send error:", err);
    }
}

// Sử dụng
sendMessage(1, "Hello, I need help with my story!");
```

#### 5. Các hành động khác
```typescript
// Đánh dấu đã đọc
async function markAsRead(contactId: number) {
    await connection.invoke("MarkAsRead", contactId);
}

// Thông báo đang gõ
async function notifyTyping(contactId: number) {
    await connection.invoke("UserTyping", contactId);
}

// Join vào room của contact (để nhận real-time updates)
async function joinContact(contactId: number) {
    await connection.invoke("JoinContact", contactId);
}

// Leave room
async function leaveContact(contactId: number) {
    await connection.invoke("LeaveContact", contactId);
}
```

#### 6. Ngắt kết nối
```typescript
// Khi unmount component hoặc đóng app
connection.stop()
    .then(() => console.log("Disconnected"))
    .catch(err => console.error(err));
```

---

## 📱 Ví dụ React Component

```tsx
import { useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';

interface Message {
    messageId: number;
    messageText: string;
    senderType: string;
    sendAt: string;
    senderName: string;
}

export default function ChatComponent({ contactId }: { contactId: number }) {
    const [messages, setMessages] = useState<Message[]>([]);
    const [inputText, setInputText] = useState('');
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null);

    useEffect(() => {
        // Tạo SignalR connection
        const token = localStorage.getItem('accessToken');
        const newConnection = new signalR.HubConnectionBuilder()
            .withUrl('https://localhost:7000/chatHub', {
                accessTokenFactory: () => token || ''
            })
            .withAutomaticReconnect()
            .build();

        // Lắng nghe tin nhắn mới
        newConnection.on('ReceiveMessage', (message: Message) => {
            if (message.contactId === contactId) {
                setMessages(prev => [...prev, message]);
            }
        });

        // Kết nối
        newConnection.start()
            .then(() => {
                console.log('✅ Connected');
                newConnection.invoke('JoinContact', contactId);
            })
            .catch(err => console.error('❌ Connection error:', err));

        setConnection(newConnection);

        // Cleanup
        return () => {
            newConnection.invoke('LeaveContact', contactId);
            newConnection.stop();
        };
    }, [contactId]);

    const sendMessage = async () => {
        if (connection && inputText.trim()) {
            try {
                await connection.invoke('SendMessage', contactId, inputText);
                setInputText('');
            } catch (err) {
                console.error('Send error:', err);
            }
        }
    };

    return (
        <div className="chat-container">
            <div className="messages">
                {messages.map(msg => (
                    <div key={msg.messageId} className={`message ${msg.senderType}`}>
                        <strong>{msg.senderName}</strong>: {msg.messageText}
                        <small>{new Date(msg.sendAt).toLocaleTimeString()}</small>
                    </div>
                ))}
            </div>
            <div className="input-area">
                <input
                    value={inputText}
                    onChange={(e) => setInputText(e.target.value)}
                    onKeyPress={(e) => e.key === 'Enter' && sendMessage()}
                    placeholder="Type a message..."
                />
                <button onClick={sendMessage}>Send</button>
            </div>
        </div>
    );
}
```

---

## 🔐 Authentication

SignalR sử dụng JWT token từ:
- **Query string**: `?access_token=YOUR_JWT_TOKEN`
- **HTTP Header**: `Authorization: Bearer YOUR_JWT_TOKEN`

Backend đã cấu hình tự động xử lý cả 2 cách trong `Program.cs`.

---

## 📊 Luồng hoạt động

### **Staff gửi tin cho Author:**
1. Staff kết nối SignalR với JWT token
2. Staff gọi `SendMessage(contactId, "Hello Author")`
3. ChatHub xử lý và lưu vào database
4. ChatHub gửi tin nhắn đến Author qua `User_{authorId}` group
5. Author nhận event `ReceiveMessage` và hiển thị

### **Author đánh dấu đã đọc:**
1. Author gọi `MarkAsRead(contactId)`
2. ChatHub cập nhật database (IsRead = true)
3. Gửi thông báo đến Staff qua `MessagesMarkedAsRead`

---

## 🎯 Features

✅ Real-time messaging với SignalR  
✅ JWT Authentication  
✅ Read/Unread tracking  
✅ Typing indicators  
✅ User groups (Staff/Author)  
✅ Message persistence trong database  
✅ REST API fallback  
✅ Automatic reconnection  

---

## 🐛 Troubleshooting

**Lỗi 401 Unauthorized:**
- Kiểm tra JWT token có hợp lệ không
- Đảm bảo token được gửi trong `accessTokenFactory`

**Không kết nối được:**
- Kiểm tra CORS trong `Program.cs`
- Thêm frontend URL vào `WithOrigins(...)`

**Không nhận được message:**
- Kiểm tra `JoinContact(contactId)` đã được gọi chưa
- Xem Console log để debug events

---

## 📞 Support

Nếu có vấn đề, kiểm tra:
1. Database tables đã được tạo chưa
2. JWT token có role "Staff" hoặc "User/Author"
3. CORS đã cấu hình đúng frontend URL
4. SignalR connection state (Connected/Disconnected)
