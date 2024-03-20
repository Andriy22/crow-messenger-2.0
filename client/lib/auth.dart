import 'dart:convert';
import 'dart:math';

import 'package:signalr_netcore/signalr_client.dart';
import 'package:http/http.dart' as http;

import 'consts.dart';
import 'data.dart';

class Account {
  late User user;
  late MessageHelper messageHelper;
  late HubConnection connection;

  List<Chat> chats = [];

  Account.Login(String login, String password,
      void Function(List<MessageResponse>) onGetMessages,
      void Function(MessageResponse) onNewMessage) {
    var auth = http.post(
      Uri.parse('${URL}/api/auth/authorize'),
      headers: <String, String>{
        'Content-Type': 'application/json; charset=UTF-8',
      },
      body: jsonEncode(<String, String> {
        "nickName": login,
        "password": password
      }),
    );

    auth.then((value) {
      print(value.body);
      user = User.fromJson(jsonDecode(value.body));
      messageHelper = MessageHelper(user);
      var options = HttpConnectionOptions(skipNegotiation: true,
          transport: HttpTransportType.WebSockets,
          accessTokenFactory: () {
            return Future(() => "bearer ${user.accessToken!}");
          });

      connection = HubConnectionBuilder()
          .withUrl("${URL}/api/live/chat", options: options)
          .build();

      connection.start();

      connection.on('Connected', (x) {
        connection.send("get-my-chats");
      });

      connection.on("ReceiveMyChats", (list) {
        var chatList = (list![0] as dynamic);
        print(chatList);
        print(chatList.length);
        chats.clear();
        try {
          for(int i = 0; i < (chatList.length as int); i++) {
            chats.add(Chat.fromDynamic(chatList[i]));
          }
        } catch(ex) {
          print(ex);
        }
      });

      connection.on("ReceivedAllMessagesWithUser", (list) {
        try {
          var chatList = (list![0] as dynamic);
          List<MessageResponse> messages = [];
          for(int i = 0; i < (chatList.length as int); i++) {
            messages.add(MessageResponse.fromDynamic(chatList[i]));
          }
          print(messages);
          onGetMessages(messages);
        } catch(ex) {
          print(ex);
        }
      });

      connection.on("ReceivedNewMessage", (list) {
        try {
          print("RECEIVED");
          print(list);
          onNewMessage(MessageResponse.fromDynamic(list![0]));
        } catch(ex) {
          print(ex);
        }
      });
    });
  }

  void GetMessages(User user) {
    connection.send("get-all-messages-with-user", args: [user.id]);
  }
}