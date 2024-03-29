import 'package:client/components.dart';
import 'package:flutter/material.dart';

import 'auth.dart';
import 'chatsView.dart';

class LoginView extends StatefulWidget {
  @override
  State<LoginView> createState() => _LoginViewState();
}

class _LoginViewState extends State<LoginView> {
  TextEditingController _loginTextController = TextEditingController();
  TextEditingController _passwordTextController = TextEditingController();

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      resizeToAvoidBottomInset: false,
      appBar: AppBar(),
      body: Column(children: [
        Padding(padding: EdgeInsets.fromLTRB(10, 10, 10, 10),
            child: TextField(
                decoration: InputDecoration(prefixIcon: Icon(Icons.person_outline), labelText: "Login", border: OutlineInputBorder()),
                controller: _loginTextController)),
        Padding(padding: EdgeInsets.fromLTRB(10, 10, 10, 10),
          child: TextField(
            obscureText: true,
            decoration: InputDecoration(prefixIcon: Icon(Icons.lock_outlined), labelText: "Passward", border: OutlineInputBorder()),
            controller: _passwordTextController)),
        Padding(padding: EdgeInsets.fromLTRB(10, 10, 10, 10),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              CardButton("SIGN IN", Colors.blue, () {
                Account.Login(_loginTextController.text, _passwordTextController.text, (ex) {
                  String description = "Error code: ${ex}";
                  if(ex == 400) {
                    description = "The is no user with this login.";
                  }

                  if(ex == 500) {
                    description = "The password is wrong.";
                  }

                  showDialog<String>(
                      context: context,
                      builder: (context) => AlertDialog(
                        title: const Text("Failed to login!"),
                        content: Text(description),
                        actions: [ TextButton(onPressed: () => Navigator.pop(context, 'Cancel'), child: const Text("Cancel")) ],
                      )
                  );

                }, (account) {
                  Navigator.of(context).push(MaterialPageRoute(builder: (context) => ChatView(account)));
                });
              }),
              CardButton("SIGN UP", Colors.white70, () {
                Account.Register(_loginTextController.text, _passwordTextController.text, (ex) {
                  String description = "Error code: ${ex}";
                  if(ex == 400) {
                    description = "Password is too weak.";
                  }

                  if(ex == 500) {
                    description = "User already exists.";
                  }

                  showDialog<String>(
                      context: context,
                      builder: (context) => AlertDialog(
                        title: const Text("Failed to create account!"),
                        content: Text(description),
                        actions: [ TextButton(onPressed: () => Navigator.pop(context, 'Cancel'), child: const Text("Cancel")) ],
                      )
                  );

                }, (account) {
                  Navigator.of(context).push(MaterialPageRoute(builder: (context) => ChatView(account)));
                });
              })
            ],
          ))
      ],),
    );
  }

}