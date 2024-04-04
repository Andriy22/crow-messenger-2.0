import 'dart:convert';

import 'package:http/http.dart' as http;

import 'consts.dart';

class User {
  String id;
  String nickName;
  String status;
  String bio;
  String profileImage;
  String? accessToken;

  User(this.id, this.nickName, this.profileImage, {this.status = "", this.bio = "", this.accessToken});

  User.fromJson(Map<String, dynamic> map) : id = map["id"],
        nickName = map["nickName"],
        bio = map["bio"] ?? "",
        status = map["status"] ?? "",
        profileImage = map["profileImg"] ?? "",
        accessToken = map["accessToken"] ?? "";
}

class Chat {
  int id;
  int chatType;
  String title;
  String? profileImage;
  List<User> users;

  Chat(this.id, this.chatType, this.title, this.profileImage, this.users);

  Chat.fromDynamic(dynamic map) : id = map["id"],
        title = map["title"],
        chatType = map["chatType"],
        profileImage = map["profileImg"],
        users =  (map["users"] as List).map((x) => User.fromJson(x)).toList();
}

class MessageRequest {
  int? id;
  String? message;
  String? receiverId;
  String? senderId;
  int messageType;
  int? replyMessageId;

  MessageRequest.Reply(this.id, this.message, this.messageType, this.receiverId, this.senderId, this.replyMessageId);
  MessageRequest(this.id, this.message, this.messageType, this.receiverId, this.senderId);


  // @override
  // String toString() {
  //   return "{${id}, ${message}";
  // }
}

class MessageResponse  {
  int id;
  String? message;
  int messageType;
  User? sender;
  DateTime createdAt;
  int chatId;
  int? replyMessageId;
  DateTime? seenAt;

  MessageResponse(this.id, this.message, this.messageType, this.chatId, this.createdAt, this.sender, this.replyMessageId, this.seenAt);

  MessageResponse.fromDynamic(dynamic map) : id = map["id"],
    message = map["message"],
    messageType = map["messageType"],
    sender = User.fromJson(map["sender"]),
    createdAt = DateTime.parse(map["createdAt"]),
    chatId = map["chatId"],
    replyMessageId = map["replyMessageId"],
    seenAt = map["seenAt"];
}

class MessageHelper {
  User user;

  MessageHelper(this.user);

  SendMessageByChatID(int chatId, String message, {void Function(MessageResponse)? onResponse = null}) {
    var auth = http.post(
      Uri.parse('${URL}/api/chat/send-message-to-chat'),
      headers: <String, String>{
        'Authorization': "bearer ${user!.accessToken}",
      },
      body: <String, dynamic>{
        "message": message,
        "messageType": 0.toString(),
        "chatId": chatId.toString(),
      },
    ).then((value) {
      print("SUKA " + MessageResponse.fromDynamic(jsonDecode(value.body)).id.toString());
      onResponse!(MessageResponse.fromDynamic(jsonDecode(value.body)));
    }).catchError((x) => print(x));
  }

  SendMessageByUserID(String receiverId, String message, {void Function(MessageResponse)? onResponse = null}) {
    var auth = http.post(
      Uri.parse('${URL}/api/chat/send-private-message'),
      headers: <String, String>{
        'Authorization': "bearer ${user!.accessToken}",
      },
      body: <String, dynamic>{
        "message": message,
        "messageType": 0.toString(),
        "receiverId": receiverId,
      },
    ).then((value) {
      print("SUKA " + MessageResponse.fromDynamic(jsonDecode(value.body)).toString());
      onResponse!(MessageResponse.fromDynamic(jsonDecode(value.body)));
    }).catchError((x) => print(x));
  }
}