import 'package:client/chatsView.dart';
import 'package:client/loginView.dart';
import 'package:flutter/material.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

import 'auth.dart';
import 'consts.dart';

class LoadingView extends StatefulWidget {
  @override
  State<LoadingView> createState() => _LoadingViewState();
}

class _LoadingViewState extends State<LoadingView> {
  @override
  Widget build(BuildContext context) {
    FlutterSecureStorage storage = const FlutterSecureStorage();
    storage.containsKey(key: AUTH_TOKEN_KEY).then((value) async {
      if(value == true) {
        var account = await Account.LoginWithToken((await storage.read(key: AUTH_TOKEN_KEY))! , (account) { });
        Navigator.pushAndRemoveUntil(context, MaterialPageRoute(builder: (context1) => ChatView(account)), (route) => false);
      } else {
        Navigator.pushAndRemoveUntil(context, MaterialPageRoute(builder: (context1) => LoginView()), (route) => false);
      }
    });
    
    return Scaffold();
  }
}