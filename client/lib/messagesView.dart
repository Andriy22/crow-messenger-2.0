import 'dart:io';

import 'package:client/auth.dart';
import 'package:client/authorizedView.dart';
import 'package:client/components.dart';
import 'package:client/data.dart';
import 'package:client/helpers.dart';
import 'package:file_picker/file_picker.dart';
import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';

import 'consts.dart';

class MessagesView extends AuthorizedView {
  MessagesView(Account account, this.currentChat) : super(account);

  Chat currentChat;

  @override
  State<MessagesView> createState() => _MessagesViewState();
}

class _MessagesViewState extends State<MessagesView> {
  late TextEditingController _controller;
  final ScrollController _scrollController = ScrollController();
  late List<File> _atachments = List<File>.empty(growable: true);

  List<MessageResponse> messages = [];

  @override
  void initState() {
    super.initState();
    _controller = TextEditingController();

    widget.account.onGetMessages = (list) {
      setState(() {
      messages = list;
      });
      scrollDown(1000);
    };

    widget.account.onGetChats = (chats) {};

    widget.account.onNewMessage = (message) {
      if(message.chatId == widget.currentChat.id) {
        messages.add(message);
      }

      setState(() {
        scrollDown(500);
      });
    };

    widget.account.GetMessages(widget.currentChat);
  }

  @override
  void dispose() {
    super.dispose();
    _controller.dispose();
  }

  Widget drawMessage(MessageResponse message) {
    Widget getText() {
      return Column(
        crossAxisAlignment: CrossAxisAlignment.end,
        children: [
          Padding(
              padding: const EdgeInsets.fromLTRB(11, 5, 11, 14),
              child: Text(message.message!)),
        ],
      );
    }

    var widgets = List<Widget>.empty(growable: true);
    if(message.attachments.isEmpty == false) {
      var stack = List<Widget>.empty(growable: true);
      var maxWidth = BoxConstraints(maxWidth: MediaQuery.of(context).size.width - 100).maxWidth/2-9;
      stack.add(ClipRRect(
        borderRadius: BorderRadius.vertical(top: Radius.circular(12), bottom: Radius.circular(message.message == null ? 12 : 0)),
        child: GridImages(message.attachments, context),
        ));

      widgets.add(Stack(children: stack,));
    }

    if(message.message != null && message.message!.isEmpty == false) {
      widgets.add(getText());
    } else {
    }

    return Align(
        alignment: message.sender!.id == widget.account.user.id ? Alignment.centerRight : Alignment.centerLeft,
        child: Container(
          constraints: BoxConstraints(maxWidth: MediaQuery.of(context).size.width - 100),
          child: Padding(
            padding: const EdgeInsets.all(5),
            child: Card(
                color: message.sender!.id == widget.account.user.id
                    ? Theme.of(context).colorScheme.inversePrimary
                    : Theme.of(context).colorScheme.primary,
                child: Stack(children: [
                  Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: widgets,
                  ),
                  Positioned(
                    right: 0.0,
                    bottom: 0.0,
                    child: Align(
                      alignment: Alignment.bottomRight,
                      child: Padding(padding: const EdgeInsets.fromLTRB(0, 0, 6, 0),
                        child: Text(
                            getShortDate(message.createdAt),
                            style: Theme.of(context).textTheme.bodySmall)),
                    ),
                  )
                ])),
          ),
        ));
  }

  List<Widget> drawMessages(Chat userId) {
    List<Widget> widgets = [];
    for (int i = 0; i < messages.length; i++) {
      // if(messages[i].message == null) {
      //   continue;
      // }
      widgets.add(drawMessage(messages[i]));
    }
    return widgets;
  }

  AppBar drawAppBar() {
    return AppBar(
      backgroundColor: Theme.of(context).colorScheme.inversePrimary,
      leading: BackButton(
        onPressed: () {
          setState(() {
            Navigator.of(context).pop();
            widget.account.GetChats();
          });
        },
      ),
      title: InkWell(
        onTap: () {
          showDialog(context: context, builder: (context) {
            return StatefulBuilder(builder: (context, setDialogState) {
              return Dialog(
                child: Padding(
                  padding: const EdgeInsets.all(10),
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        children: [
                          CircleAvatar(
                            backgroundImage: NetworkImage(widget.currentChat.profileImage!.isEmpty
                                ? "https://img2.joyreactor.cc/pics/post/TheFikus-artist-Pixel-Art-8365648.png"
                                : "${URL}/static/users/${widget.currentChat.profileImage}")),

                          Padding(padding: EdgeInsets.fromLTRB(10, 10, 10, 10),
                            child: Text(widget.currentChat.title, textScaler: TextScaler.linear(1.5),),),

                        ],
                      ),

                      Padding(padding: EdgeInsets.fromLTRB(10, 10, 10, 0),
                        child: Text(
                            "Bio: ${widget.currentChat.users.where((element) => element.id != widget.account.user.id).first.bio}",
                            maxLines: 3,
                            textAlign: TextAlign.start,)
                      ),
                    ],
                  ),
                ),
              );
            });
          });
        },
        child: Row(
          mainAxisAlignment: MainAxisAlignment.start,
          //crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            ClipRRect(
                borderRadius: BorderRadius.circular(25),
                child: Image(
                    image: widget.currentChat.profileImage!.isEmpty
                        ? NetworkImage("https://img2.joyreactor.cc/pics/post/TheFikus-artist-Pixel-Art-8365649.png")
                        : NetworkImage("${URL}/static/users/${widget.currentChat.profileImage!}"),
                    width: 50,
                    height: 50,
                    fit: BoxFit.cover)),
            Expanded(
                flex: 10,
                child: Column(
                  children: [
                    Padding(padding: const EdgeInsets.fromLTRB(10, 0, 10, 0), child: Text(widget.currentChat.title)),
                    Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        const Icon(Icons.phone_android, size: 14),
                        Text(" online",
                            style: Theme.of(context).textTheme.labelLarge)
                      ],
                    )
                  ],
                )),
          ],
        ),
      ),
      actions: [
        IconButton(
            onPressed: () {}, icon: const Icon(Icons.phone_enabled)),
        IconButton(onPressed: () {}, icon: const Icon(Icons.more_vert))
      ],
    );
  }

  Widget drawBottomBar() {
    _controller = TextEditingController();
    return Row(
      children: [
        IconButton(
            onPressed: () async {
              FilePickerResult? result = await FilePicker.platform.pickFiles();

              if (result == null) {
                return;
              }

              File file = File(result.files.first.path!);

              setState(() {
                _atachments.add(file);
              });
            }, icon: const Icon(Icons.add_circle_outline)),
        Expanded(
            child: TextField(
              controller: _controller,
            )),
        IconButton(
            onPressed: () {
              setState(() {
                if(_controller.text.isEmpty && _atachments.isEmpty) {
                  return;
                }
                widget.account.messageHelper
                    .SendMessageByChatID(widget.currentChat.id, _controller.text, attachments: _atachments);
                _atachments.clear();
              });
            },
            icon: const Icon(Icons.send))
      ],
    );
  }

  void scrollDown(int milliseconds) {
    _scrollController.animateTo(
      _scrollController.position.maxScrollExtent,
      duration: Duration(milliseconds: milliseconds),
      curve: Curves.fastOutSlowIn,
    );
  }

  List<Widget> getAttachmentsWidgets() {
    var list = List<Widget>.empty(growable: true);
    for(int i = 0; i < _atachments.length; i++) {
      list.add(Padding(padding: const EdgeInsets.all(5),
        child: Stack(
          children: [
            ClipRRect(
              borderRadius: BorderRadius.circular(8),
              child: Image(image: FileImage(_atachments[i]),
                width: 64, height: 64,
                fit: BoxFit.cover,),
            ),

            Positioned(
              right: 0.0,
              child: GestureDetector(
                onTap: (){
                  setState(() {
                    _atachments.removeAt(i);
                  });
                },
                child: const Align(
                  alignment: Alignment.topRight,
                  child: Padding(padding: EdgeInsets.all(3),
                    child: CircleAvatar(
                      radius: 14.0,
                      backgroundColor: Colors.white,
                      child: Icon(Icons.close, color: Colors.black),
                    ),),
                ),
              ),
            ),
          ],
        ),));
    }
    return list;
  }

  @override
  Widget build(BuildContext context) {
    var scrollView = SingleChildScrollView(
        controller: _scrollController,
        child: Column(
          mainAxisAlignment: MainAxisAlignment.start,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: drawMessages(widget.currentChat),
        ));

    return Scaffold(
      resizeToAvoidBottomInset: false,
      appBar: drawAppBar(),
      body: scrollView,
      floatingActionButton: null, // This trailing comma makes auto-formatting nicer for build methods.
      bottomNavigationBar: Padding(
          padding: MediaQuery.of(context).viewInsets,
          child: drawBottomBar()),
      bottomSheet: SingleChildScrollView(
        child: Row(
          children: getAttachmentsWidgets(),
        ),
      ),
    );
  }
}
