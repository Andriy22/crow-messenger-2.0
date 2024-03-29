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

  late void Function(List<MessageResponse>) onGetMessages;
  late void Function()? onConnected;
  late void Function(List<Chat>) onGetChats;
  late void Function(MessageResponse) onNewMessage;

  Account.Login(String login, String password, void Function(int statusCode) onFailed, void Function(Account account) onSucces) {
    Future<http.Response>? auth = http.post(
      Uri.parse('${URL}/api/auth/authorize'),
      headers: <String, String>{
        'Content-Type': 'application/json; charset=UTF-8',
      },
      body: jsonEncode(<String, String> {
        "nickName": login,
        "password": password
      }),
    );

    auth?.then((value) {
      if(value.statusCode != 200) {
        onFailed(value.statusCode);
        return;
      }

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
        onConnected!();
      });

      connection.on("ReceiveMyChats", (list) {
        var chatList = (list![0] as dynamic);
        chats.clear();
        try {
          for(int i = 0; i < (chatList.length as int); i++) {
            chats.add(Chat.fromDynamic(chatList[i]));
          }
          onGetChats(chats);
        } catch(ex) {
          print(ex);
        }
      });

      connection.on("ReceivedChatMessages", (list) {
        try {
          var chatList = (list![0] as dynamic);
          List<MessageResponse> messages = [];
          for(int i = 0; i < (chatList.length as int); i++) {
            messages.add(MessageResponse.fromDynamic(chatList[i]));
          }
          onGetMessages(messages);
        } catch(ex) {
          print(ex);
        }
      });

      connection.on("ReceivedNewMessage", (list) {
        try {
          onNewMessage(MessageResponse.fromDynamic(list![0]));
        } catch(ex) {
          print(ex);
        }
      });

      onSucces(this);
    });
  }

  static Future<Account?> Register(String login, String password, void Function(int statusCode) onFailed, void Function(Account account) onSucces) async {
    http.Response auth = await http.post(
      Uri.parse('${URL}/api/accounts/create-account'),
      headers: <String, String>{
        'Content-Type': 'application/json; charset=UTF-8',
      },
      body: jsonEncode(<String, String> {
        "nickName": login,
        "password": password
      }),
    );

    print(auth.body);

    if(auth.statusCode == 200) {
      return Account.Login(login, password, onFailed, onSucces);
    }

    onFailed(auth.statusCode);
    return null;
  }

  void GetChats() {
    connection.send("get-my-chats");
  }

  void GetMessages(Chat chat) {
    connection.send("get-chat-messages", args: [chat.id]);
  }
}