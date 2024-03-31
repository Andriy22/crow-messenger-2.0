import 'package:client/auth.dart';
import 'package:client/authorizedView.dart';
import 'package:client/data.dart';
import 'package:flutter/material.dart';

class MessagesView extends AuthorizedView {
  MessagesView(Account account, this.currentChat) : super(account);

  Chat currentChat;

  @override
  State<MessagesView> createState() => _MessagesViewState();
}

class _MessagesViewState extends State<MessagesView> {
  late TextEditingController _controller;
  final ScrollController _scrollController = ScrollController();

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

  List<Widget> drawMessages(Chat userId) {
    List<Widget> widgets = [];
    for (int i = 0; i < messages.length; i++) {
      if(messages[i].message == null) {
        continue;
      }
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
      title: Row(
        mainAxisAlignment: MainAxisAlignment.start,
        //crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          ClipRRect(
              borderRadius: BorderRadius.circular(25),
              child: Image(
                  image: widget.currentChat.profileImage!.isEmpty
                      ? NetworkImage("https://img2.joyreactor.cc/pics/post/TheFikus-artist-Pixel-Art-8365649.png")
                      : NetworkImage(widget.currentChat.profileImage!),
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
            onPressed: () {}, icon: const Icon(Icons.add_circle_outline)),
        Expanded(
            child: TextField(
              controller: _controller,
            )),
        IconButton(
            onPressed: () {
              setState(() {
                if(_controller.text.isEmpty)
                {
                  return;
                }
                widget.account.messageHelper.SendMessageByChatID(widget.currentChat.id, _controller.text);
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
    );
  }
}
