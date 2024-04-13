import 'dart:convert';

import 'package:flutter_secure_storage/flutter_secure_storage.dart';
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

  static Future<Account> LoginWithToken(String token, void Function(Account account) onSucces) async {
    Account account = Account();

    var user = await account.GetMe(token: token);
    print(user);
    user.accessToken = token;
    account.user = user;

    account.messageHelper = MessageHelper(account.user);
    var options = HttpConnectionOptions(skipNegotiation: true,
        transport: HttpTransportType.WebSockets,
        accessTokenFactory: () {
          return Future(() => "bearer ${account.user.accessToken!}");
        });

    account.connection = HubConnectionBuilder()
        .withUrl("${URL}/api/live/chat", options: options)
        .build();

    account.connection.start();

    account.connection.on('Connected', (x) {
      account.onConnected!();
    });

    account.connection.on("ReceiveMyChats", (list) {
      var chatList = (list![0] as dynamic);
      account.chats.clear();
      try {
        for(int i = 0; i < (chatList.length as int); i++) {
          var chat = Chat.fromDynamic(chatList[i]);
          account.chats.add(chat);
        }
        account.onGetChats(account.chats);
      } catch(ex) {
        print(ex);
      }
    });

    account.connection.on("ReceivedChatMessages", (list) {
      try {
        var chatList = (list![0] as dynamic);
        List<MessageResponse> messages = [];
        for(int i = 0; i < (chatList.length as int); i++) {
          messages.add(MessageResponse.fromDynamic(chatList[i]));
        }
        account.onGetMessages(messages);
      } catch(ex) {
        print(ex);
      }
    });

    account.connection.on("ReceivedNewMessage", (list) {
      try {
        account.onNewMessage(MessageResponse.fromDynamic(list![0]));
      } catch(ex) {
        print(ex);
      }
    });

    onSucces(account);

    return account;
  }

  static Future<Account> Login(String login, String password, void Function(Account account) onSucces) async {
    Account account = Account();

    var auth = await http.post(
      Uri.parse('${URL}/api/auth/authorize'),
      headers: <String, String>{
        'Content-Type': 'application/json; charset=UTF-8',
      },
      body: jsonEncode(<String, String> {
        "nickName": login,
        "password": password
      }),
    );

    if(auth.statusCode != 200) {
      throw Exception(jsonDecode(auth.body)["message"]);
    }

    //var userData = await http.get()

    String authToken = jsonDecode(auth.body)["accessToken"];
    FlutterSecureStorage storage = FlutterSecureStorage();
    storage.write(key: AUTH_TOKEN_KEY, value: authToken);

    return LoginWithToken(authToken, onSucces);
  }

  static Future<Account?> Register(String login, String password, void Function(Account account) onSucces) async {
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
      return Account.Login(login, password, onSucces);
    }

    throw Exception(jsonDecode(auth.body)["message"]);
  }

  void GetChats() {
    connection.send("get-my-chats");
  }

  Future<User> GetMe({String? token = null}) async {
    token ?? user.accessToken;

    try {
      var response = await http.get(
          Uri.parse('${URL}/api/profile/me'),
          headers: <String, String>{
            'Authorization': "bearer ${token}",
          }
      );

      print(response.body);
      return User.fromJson(jsonDecode(response.body));

    } catch(ex) {
      print(ex);
    }
    throw Exception();
  }

  void UpdateAccountData() async {
    // http.Response auth = await http.post(
    //   Uri.parse('${URL}/api/accounts/create-account'),
    //   headers: <String, String>{
    //     'Content-Type': 'application/json; charset=UTF-8',
    //   },
    //   body: jsonEncode(<String, String> {
    //     "nickName": login,
    //     "password": password
    //   }),
    // );
  }

  void GetMessages(Chat chat) {
    connection.send("get-chat-messages", args: [chat.id]);
  }

  Future<String> UpdateBio(String bio) async {
    http.Response auth = await http.patch(
          Uri.parse('${URL}/api/profile/update-bio'),
          headers: <String, String>{
            'Content-Type': 'application/json; charset=UTF-8',
            'Authorization': "bearer ${user.accessToken}",
          },
          body: jsonEncode(<String, String> {
            "bio": bio
          }),
        );

    user.bio = bio;

    return auth.body;
  }

  Future<String> UpdateStatus(String status) async {
    http.Response auth = await http.patch(
      Uri.parse('${URL}/api/profile/update-status'),
      headers: <String, String>{
        'Content-Type': 'application/json; charset=UTF-8',
        'Authorization': "bearer ${user.accessToken}",
      },
      body: jsonEncode(<String, String> {
        "status": status
      }),
    );

    user.status = status;

    return auth.body;
  }

  void GetChatByUser(User user) {

  }
}