import 'dart:convert';
import 'dart:io';
import 'dart:ui';

import 'package:client/auth.dart';
import 'package:client/components.dart';
import 'package:client/data.dart';
import 'package:client/imageCropingView.dart';
import 'package:client/loginView.dart';
import 'package:client/messagesView.dart';
import 'package:file_picker/file_picker.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

import 'authorizedView.dart';
import 'package:http/http.dart' as http;
import 'package:http_parser/src/media_type.dart';

import 'consts.dart';
import 'helpers.dart';

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
                        decoration: InputDecoration(prefixIcon: Icon(Icons.search),
                          labelText: "Username",
                          border: OutlineInputBorder(borderRadius: BorderRadius.circular(20))),
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
      var previewText = "";
      var previewTime = "";

      if(widget.account.chats[i].lastMessage != null) {
        previewText = widget.account.chats[i].lastMessage!.sender!.id == widget.account.user.id
            ? "You: ${widget.account.chats[i].lastMessage!.message}"
            : "${widget.account.chats[i].lastMessage!.sender!.nickName}: ${widget.account.chats[i].lastMessage!.message}";

        previewTime = getShortDate(widget.account.chats[i].lastMessage!.createdAt);
      }

      var previewLable = Text(
        previewText,
        overflow: TextOverflow.ellipsis,
      );

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
                            : NetworkImage("${URL}/static/users/${widget.account.chats[i].profileImage!}"),
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
                        Text(widget.account.chats[i].title,
                            style:
                            Theme.of(context).textTheme.labelLarge),

                        previewLable
                      ],
                    ))),
            Expanded(
                flex: 1,
                child: Text(previewTime))
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
      automaticallyImplyLeading: true,
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

  Widget createUserAvatar() {
    return InkWell(
      onTap: () async {
        FilePickerResult? result = await FilePicker.platform.pickFiles();

        if (result == null) {
          return;
        }

        File file = File(result.files.first.path!);

        Navigator.of(context).push(MaterialPageRoute(builder: (context) {
          return ImageCropingView(Image.file(file), (croppedControler) async {
            print(file.path);
            var request = http.MultipartRequest("POST", Uri.parse("${URL}/api/profile/change-image"));
            request.headers.addAll(<String, String>{
              'Authorization': "bearer ${widget.account.user.accessToken}",
            });

            var bitmap = await croppedControler.croppedBitmap();
            var iamge = await croppedControler.croppedImage();
            var byteData = await bitmap.toByteData(format: ImageByteFormat.png);

            request.files.add(await http.MultipartFile.fromBytes("file",
                byteData!.buffer.asUint8List(),
                filename: 'uploaded_file.png',
                contentType: MediaType('image', 'png'),));

            var responseStream = await request.send();
            var response = await http.Response.fromStream(responseStream);
            setState(() {
              print(response.body);
              widget.account.user.profileImage = response.body;
            });
          });
        }));
      },
      child: CircleAvatar(
        backgroundImage: NetworkImage(
            widget.account.user.profileImage.isEmpty
                ? "https://img2.joyreactor.cc/pics/post/TheFikus-artist-Pixel-Art-8365648.png"
                : "${URL}/static/users/${widget.account.user.profileImage}"),
      ),
    );
  }

  Drawer createDrawer() {
    return Drawer(
      child: ListView(
        padding: EdgeInsets.zero,
        children: [
          UserAccountsDrawerHeader(
              accountName: Text(widget.account.user.nickName,
                  style: const TextStyle(
                    fontWeight: FontWeight.bold,
                  )),
              accountEmail: InkWell(
                onTap: () {
                  showDialog(context: context, builder: (context) {
                    var textController = TextEditingController();
                    textController.text = widget.account.user.status;
                    return Dialog(
                        child: Padding(padding: const EdgeInsets.all(10),
                            child: Column(
                              mainAxisSize: MainAxisSize.min,
                              children: [
                                Padding(padding: const EdgeInsets.fromLTRB(0, 0, 0, 0),
                                  child: TextField(
                                    decoration: InputDecoration(labelText: "Status",
                                        border: OutlineInputBorder(borderRadius: BorderRadius.circular(20)),
                                        suffixIcon: InkWell(child: Icon(Icons.save_outlined),
                                          onTap: () {
                                            setState(() {
                                              widget.account.UpdateStatus(textController.text);
                                            });
                                          },)),
                                    controller: textController,
                                    minLines: 1,
                                    maxLines: 1,
                                    maxLength: 50,
                                    maxLengthEnforcement: MaxLengthEnforcement.truncateAfterCompositionEnds,),),
                              ],
                            )
                        )
                    );
                  });
                },
                child: Row(children: [
                  Text(widget.account.user.status.isEmpty
                      ? "online"
                      : widget.account.user.status),

                  const Padding(padding: EdgeInsets.fromLTRB(5, 0, 0, 0), child: Icon(Icons.edit, color: Colors.white,),)
                ],),
              ),
              currentAccountPicture: createUserAvatar(),
              decoration: const BoxDecoration(
                image: DecorationImage(
                  image: NetworkImage(
                    "https://img2.joyreactor.cc/pics/post/PainterKira-Pixel-Gif-Pixel-Art-8320631.gif",
                  ),
                  fit: BoxFit.cover,
                ),
              )),

          ListTile(
            leading: const Icon(Icons.person_outline),
            title: const Text("Profile"),
            onTap: () {
              showDialog(context: context, builder: (context) {
                var textController = TextEditingController();
                textController.text = widget.account.user.bio;
                return StatefulBuilder(builder: (context, setDialogState) {
                  return Dialog(
                    child: Padding(
                      padding: const EdgeInsets.all(10),
                      child: Column(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Row(
                            children: [
                              createUserAvatar(),
                              Padding(padding: EdgeInsets.fromLTRB(10, 10, 10, 10),
                                child: Text(widget.account.user.nickName, textScaler: TextScaler.linear(1.5),),),

                            ],
                          ),

                          Padding(padding: EdgeInsets.fromLTRB(0, 10, 0, 0),
                            child: TextField(
                                decoration: InputDecoration(labelText: "Bio",
                                    border: OutlineInputBorder(borderRadius: BorderRadius.circular(20))),
                                controller: textController,
                                minLines: 3,
                                maxLines: 3,
                                maxLength: 200,
                                maxLengthEnforcement: MaxLengthEnforcement.truncateAfterCompositionEnds,),),

                          Padding(padding: EdgeInsets.fromLTRB(0, 10, 0, 0),
                            child: Row(
                              mainAxisAlignment: MainAxisAlignment.end,
                              children: [
                                CardButton("Save", Colors.blue, () {
                                  widget.account.UpdateBio(textController.text);
                                }, icon: Icon(Icons.save_outlined))
                              ],),)
                        ],
                      ),
                    ),
                  );
                });
              });
            },
          ),

          ListTile(
            leading: const Icon(Icons.settings_outlined),
            title: const Text("Settings"),
            onTap: () {

            },
          ),

          ListTile(
            leading: const Icon(Icons.logout_outlined),
            title: const Text("Log Out"),
            onTap: () {
              Navigator.pushAndRemoveUntil(context, MaterialPageRoute(builder: (context1) => LoginView()), (route) => false);
              var storage = FlutterSecureStorage();
              storage.delete(key: AUTH_TOKEN_KEY);
            },
          ),

          const Padding(padding: EdgeInsets.all(10), child: Text("CrowMessenger <3"),)
        ],
      ),
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
      ),
      drawer: createDrawer(),
    );
  }

  void  ShowNewMessageDialog(User user) {
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
                                      var chat = Chat(response.chatId, 0, user.nickName, user.profileImage, [widget.account.user, user] );
                                      widget.account.chats.add(chat);
                                      OpenChat(chat);
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
