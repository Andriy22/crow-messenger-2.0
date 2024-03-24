import 'package:client/auth.dart';
import 'package:client/data.dart';
import 'package:flutter/material.dart';

void main() {
  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  // This widget is the root of your application.
  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Flutter Demo',
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(seedColor: Colors.deepPurple),
        useMaterial3: true,
      ),
      home: const MyHomePage(title: 'Flutter Demo Home Page )))'),
    );
  }
}

class MyHomePage extends StatefulWidget {
  const MyHomePage({super.key, required this.title});

  final String title;

  @override
  State<MyHomePage> createState() => _MyHomePageState();
}

class _MyHomePageState extends State<MyHomePage> {
  late TextEditingController _controller;
  late Account _account;
  final ScrollController _scrollController = ScrollController();

  Chat? currentChat = null;
  List<MessageResponse> messages = [];

  @override
  void initState() {
    super.initState();
    _controller = TextEditingController();
    _account = Account.Login("admin", "adminhuesos", (list) {
      setState(() {
        messages = list;
      });
      scrollDown(1000);
    }, (chats) {
      setState(() { });
    },
    (message) {
      if(currentChat == null) {
        setState(() { });
        return;
      }

      if(message.chatId == currentChat!.id) {
        messages.add(message);
      }
      setState(() {
        scrollDown(500);
      });

    });
  }

  @override
  void dispose() {
    super.dispose();
    _controller.dispose();
  }

  void _incrementCounter() {
    setState(() {
      //_account.messageHelper.SendMessageByUserID("c79d8c6f-7b38-4b1d-953c-05e585f697bd", "hello");
    });
  }

  void _incrementMessages(Chat chat) {
    setState(() {
      currentChat = chat;
      _account.GetMessages(chat);
    });
  }

  List<Widget> drawChats() {
    List<Widget> widgets = [];
    for (int i = 0; i < _account.chats.length; i++) {
      widgets.add(InkWell(
          splashColor: Colors.blue.withAlpha(30),
          onTap: () {
            _incrementMessages(_account.chats[i]);
          },
          child: Row(children: [
            Padding(
                padding: const EdgeInsets.fromLTRB(7.5, 5, 5, 5),
                child: ClipRRect(
                    borderRadius: BorderRadius.circular(25),
                    child: Image(
                        image: _account.chats[i].profileImage!.isEmpty
                            ? NetworkImage("https://img2.joyreactor.cc/pics/post/TheFikus-artist-Pixel-Art-8365649.png")
                            : NetworkImage(_account.chats[i].profileImage!),
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
                        Text(_account.chats[i].users[0].nickName,
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
        alignment: message.sender!.id == _account.user.id ? Alignment.centerRight : Alignment.centerLeft,
        child: Container(
          constraints:
              BoxConstraints(maxWidth: MediaQuery.of(context).size.width - 100),
          child: Padding(
            padding: const EdgeInsets.all(5),
            child: Card(
                color: message.sender!.id == _account.user.id
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
      leading: currentChat == null
          ? null
          : BackButton(
              onPressed: () {
                setState(() {
                  currentChat = null;
                });
              },
            ),
      title: currentChat == null
          ? const Text("Messenger")
          : Row(
              mainAxisAlignment: MainAxisAlignment.start,
              //crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                ClipRRect(
                    borderRadius: BorderRadius.circular(25),
                    child: Image(
                        image: currentChat!.profileImage!.isEmpty
                            ? NetworkImage("https://img2.joyreactor.cc/pics/post/TheFikus-artist-Pixel-Art-8365649.png")
                            : NetworkImage(currentChat!.profileImage!),
                        width: 50,
                        height: 50,
                        fit: BoxFit.cover)),
                Expanded(
                    flex: 10,
                    child: Column(
                      children: [
                        Padding(padding: const EdgeInsets.fromLTRB(10, 0, 10, 0), child: Text(currentChat!.users[0].nickName)),
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
      actions: currentChat == null
          ? []
          : [
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
                _account.messageHelper.SendMessageByChatID(currentChat!.id, _controller.text);
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
        children: currentChat == null ? drawChats() : drawMessages(currentChat!),
      ));

    return Scaffold(
      resizeToAvoidBottomInset: false,
      appBar: drawAppBar(),
      body: scrollView,
      floatingActionButton: currentChat == null
          ? FloatingActionButton(
              onPressed: _incrementCounter,
              tooltip: 'Increment',
              child: const Icon(Icons.add),
            )
          : null, // This trailing comma makes auto-formatting nicer for build methods.
      bottomNavigationBar: currentChat == null
          ? null
          : Padding(
              padding: MediaQuery.of(context).viewInsets,
              child: drawBottomBar()),
    );
  }
}
