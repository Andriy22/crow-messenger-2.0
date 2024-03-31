import 'dart:convert';

import 'package:client/auth.dart';
import 'package:client/data.dart';
import 'package:client/messagesView.dart';
import 'package:flutter/material.dart';

import 'authorizedView.dart';
import 'package:http/http.dart' as http;

import 'consts.dart';

class ChatView extends AuthorizedView {
  ChatView(Account account) : super(account);

  @override
  State<ChatView> createState() => _ChatViewState();
}

class _ChatViewState extends State<ChatView> {
  late TextEditingController _controller;
  final ScrollController _scrollController = ScrollController();

  @override
  void initState() {
    super.initState();
    _controller = TextEditingController();

    widget.account.onConnected = () => widget.account.GetChats();
    widget.account.onGetMessages = (list) { setState(() { }); };
    widget.account.onGetChats = (chats) { setState(() { }); };
    widget.account.onNewMessage = (message) { setState(() { }); };
  }

  @override
  void dispose() {
    super.dispose();
    _controller.dispose();
  }

  void _incrementCounter() {
    var users = List<User>.empty();

    List<Widget> GetChats() {
      var userWidgets = List<Widget>.empty(growable: true);
      for(int i = 0; i < users.length; i++) {
        if(users[i].id == widget.account.user.id) {
          continue;
        }

        userWidgets.add(Padding(
            padding: const EdgeInsets.all(10),
            child: InkWell(
                splashColor: Colors.blue.withAlpha(30),
                onTap: () {
                  var chat = widget.account.chats.where((x) => x.users.any((j) => j.id == users[i].id)).firstOrNull;
                  if(chat != null) {
                    Navigator.pop(context);
                    OpenChat(chat);
                    return;
                  }

                  ShowNewMessageDialog(users[i]);
                },
                child: Row(children: [
                  Text(users[i].nickName as String)
        ],))));
      }

      return userWidgets;
    }

    var textFieldController = TextEditingController();
    void Function(void Function()) setDialogStateFunction = (void Function()) => {};
    textFieldController.addListener(() {
      var auth = http.get(
        Uri.parse('${URL}/api/users/get-users-by-nickname?nickname=${textFieldController.text}'),
        headers: <String, String>{
          'Authorization': "bearer ${widget.account.user.accessToken}",
          'Content-Type': 'application/json; charset=UTF-8',
        },

      );

      auth.then((value) {
        print(users);
        setDialogStateFunction(() {
          users = (jsonDecode(value.body) as List).map((x) => User.fromJson(x)).toList();
        });
      });
    });

    showDialog<String>(
        context: context,
        builder: (context) {
          return StatefulBuilder(builder: (context, setDialogState) {
            setDialogStateFunction = setDialogState;
            return Dialog(
              child: Padding(
                padding: const EdgeInsets.all(10),
                child: Column(
                  children: [
                    Padding(padding: EdgeInsets.fromLTRB(5, 10, 5, 20),
                      child: TextField(
                        decoration: InputDecoration(prefixIcon: Icon(Icons.search), labelText: "Username", border: OutlineInputBorder(borderRadius: BorderRadius.circular(20))),
                        controller: textFieldController),),

                    SingleChildScrollView(child: Column( children: GetChats(),),)
                  ],
                ),
              ),
            );
          });
        }
    );

  }

  void OpenChat(Chat chat) {
    Navigator.of(context).push(MaterialPageRoute(builder: (context) => MessagesView(widget.account, chat)));
  }

  List<Widget> drawChats() {
    List<Widget> widgets = [];
    for (int i = 0; i < widget.account.chats.length; i++) {
      widgets.add(InkWell(
          splashColor: Colors.blue.withAlpha(30),
          onTap: () {
            OpenChat(widget.account.chats[i]);
          },
          child: Row(children: [
            Padding(
                padding: const EdgeInsets.fromLTRB(7.5, 5, 5, 5),
                child: ClipRRect(
                    borderRadius: BorderRadius.circular(25),
                    child: Image(
                        image: widget.account.chats[i].profileImage!.isEmpty
                            ? NetworkImage("https://img2.joyreactor.cc/pics/post/TheFikus-artist-Pixel-Art-8365649.png")
                            : NetworkImage(widget.account.chats[i].profileImage!),
                        width: 50,
                        height: 50,
                        fit: BoxFit.cover))),
            Expanded(
                flex: 7,
                child: Padding(
                    padding: const EdgeInsets.fromLTRB(10, 0, 5, 0),
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.start,
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(widget.account.chats[i].users[0].nickName,
                            style:
                            Theme.of(context).textTheme.labelLarge),
                        Text(
                          // messages[i][messages[i].length - 1].my
                          //     ? "You: ${messages[i][messages[i].length - 1].message}"
                          //     : messages[i][messages[i].length - 1]
                          //     .message,
                          "hi",
                          overflow: TextOverflow.ellipsis,
                        )
                      ],
                    ))),
            Expanded(
                flex: 1,
                child: Text(
                  //"${messages[i][messages[i].length - 1].dateTime.hour}:${messages[i][messages[i].length - 1].dateTime.minute}"))
                    "00:00"))
          ])));
    }
    return widgets;
  }

  Widget drawMessage(MessageResponse message) {

    Widget getTime() {
      return //message.imageLink == null ?
        Text(
            "${message.createdAt.hour}:${message.createdAt.minute}",
            style: Theme.of(context).textTheme.bodySmall);
      // : Row(mainAxisAlignment: MainAxisAlignment.end, children: [Text(
      // "${message.createdAt.hour}:${message.createdAt.minute}",
      // style: Theme.of(context).textTheme.bodySmall)],);
    }

    Widget getText() {
      return Column(
        crossAxisAlignment: /*message.imageLink == null ?*/ CrossAxisAlignment.end,// : CrossAxisAlignment.start,
        children: [
          Padding(
              padding: const EdgeInsets.fromLTRB(11, 5, 11, 0),
              child: Text(message.message!)),
          Padding(
              padding: const EdgeInsets.fromLTRB(7.5, 0, 7.5, 0),
              child: getTime())
        ],
      );
    }


    return Align(
        alignment: message.sender!.id == widget.account.user.id ? Alignment.centerRight : Alignment.centerLeft,
        child: Container(
          constraints:
          BoxConstraints(maxWidth: MediaQuery.of(context).size.width - 100),
          child: Padding(
            padding: const EdgeInsets.all(5),
            child: Card(
                color: message.sender!.id == widget.account.user.id
                    ? Theme.of(context).colorScheme.inversePrimary
                    : Theme.of(context).colorScheme.primary,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.end,
                  children:// message.imageLink != null
                  // ? [
                  //     ClipRRect(
                  //       borderRadius: const BorderRadius.vertical(top: Radius.circular(12), bottom: Radius.circular(0)),
                  //       child:
                  //           Image(image: NetworkImage(message.imageLink!)),
                  //     ),
                  //     getText(),
                  //   ] :
                  [
                    getText(),
                  ],
                )),
          ),
        ));
  }

  AppBar drawAppBar() {
    return AppBar(
      automaticallyImplyLeading: false,
      leading: null,
      backgroundColor: Theme.of(context).colorScheme.inversePrimary,
      title: const Text("Messenger"),
    );
  }

  void scrollDown(int milliseconds) {
    _scrollController.animateTo(
      _scrollController.position.maxScrollExtent,
      duration: Duration(milliseconds: milliseconds),
      curve: Curves.fastOutSlowIn,
    );
  }

  @override
  Widget build(BuildContext context) {
    var scrollView = SingleChildScrollView(
        controller: _scrollController,
        child: Column(
          mainAxisAlignment: MainAxisAlignment.start,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: drawChats(),
        ));

    return Scaffold(
      resizeToAvoidBottomInset: false,
      appBar: drawAppBar(),
      body: scrollView,
      floatingActionButton: FloatingActionButton(
        onPressed: _incrementCounter,
        tooltip: 'Increment',
        child: const Icon(Icons.add),
      )
    );
  }

  void ShowNewMessageDialog(User user) {
    var textFieldController = TextEditingController();
    textFieldController.text = "Greatings!";

    showDialog<String>(
        context: context,
        builder: (context) {
          return StatefulBuilder(builder: (context, setDialogState) {
            return Dialog(
              child: Padding(
                padding: const EdgeInsets.all(10),
                child: Column(
                  children: [
                    Padding(padding: EdgeInsets.fromLTRB(5, 10, 5, 20),
                      child: Text(user.nickName)),

                    TextField(
                        decoration: InputDecoration(
                            labelText: "Message",
                            suffixIcon: InkWell(
                              onTap: () {
                                widget.account.messageHelper.SendMessageByUserID(
                                    user.id,
                                    textFieldController.text,
                                    onResponse: (response) {
                                      OpenChat(Chat(response.chatId, 0, user.nickName, user.profileImage, [widget.account.user, user] ));
                                    });
                                Navigator.pop(context);

                                //OpenChat()
                              },
                              child: Icon(Icons.send),
                            ),
                            border: OutlineInputBorder(borderRadius: BorderRadius.circular(20))),
                        controller: textFieldController)
                  ],
                ),
              ),
            );
          });
        }
    );
  }
}
